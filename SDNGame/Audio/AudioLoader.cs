using System;
using System.IO;
using NAudio.Wave;
using Silk.NET.OpenAL;

namespace SDNGame.Audio
{
    public class AudioLoader
    {
        public short NumChannels { get; private set; }
        public int SampleRate { get; private set; }
        public short BitsPerSample { get; private set; }
        public byte[] AudioData { get; private set; }

        public void LoadAudio(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            switch (extension)
            {
                case ".wav":
                    LoadWav(filePath);
                    break;
                case ".mp3":
                case ".ogg":
                    LoadWithNAudio(filePath, extension);
                    break;
                default:
                    throw new NotSupportedException($"Audio format '{extension}' is not supported.");
            }
        }

        public void LoadWav(string filePath)
        {
            using var reader = new WaveFileReader(filePath);
            NumChannels = (short)reader.WaveFormat.Channels;
            SampleRate = reader.WaveFormat.SampleRate;
            BitsPerSample = (short)reader.WaveFormat.BitsPerSample;
            AudioData = new byte[reader.Length];
            reader.Read(AudioData, 0, (int)reader.Length);
        }

        public void LoadWithNAudio(string filePath, string extension)
        {
            using WaveStream audioFile = extension == ".mp3" ? new Mp3FileReader(filePath) : new NAudio.Vorbis.VorbisWaveReader(filePath);
            using var pcmStream = WaveFormatConversionStream.CreatePcmStream(audioFile);
            NumChannels = (short)pcmStream.WaveFormat.Channels;
            SampleRate = pcmStream.WaveFormat.SampleRate;
            BitsPerSample = (short)pcmStream.WaveFormat.BitsPerSample;

            using var memoryStream = new MemoryStream();
            byte[] buffer = new byte[4096];
            int bytesRead;
            while ((bytesRead = pcmStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                memoryStream.Write(buffer, 0, bytesRead);
            }
            AudioData = memoryStream.ToArray();
        }

        public BufferFormat GetBufferFormat()
        {
            if (NumChannels == 1)
            {
                return BitsPerSample == 8 ? BufferFormat.Mono8 :
                       BitsPerSample == 16 ? BufferFormat.Mono16 :
                       throw new NotSupportedException($"Mono audio with {BitsPerSample} bits per sample is not supported.");
            }
            else if (NumChannels == 2)
            {
                return BitsPerSample == 8 ? BufferFormat.Stereo8 :
                       BitsPerSample == 16 ? BufferFormat.Stereo16 :
                       throw new NotSupportedException($"Stereo audio with {BitsPerSample} bits per sample is not supported.");
            }
            else
            {
                throw new NotSupportedException($"Audio with {NumChannels} channels is not supported.");
            }
        }
    }
}