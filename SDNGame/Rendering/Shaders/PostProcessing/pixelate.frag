#version 330 core

in vec2 uv;

out vec4 FragColor;

uniform sampler2D screenTexture;
uniform int pixelSize;
                
void main() {
    vec2 scaledUV = floor(uv * pixelSize) / pixelSize;
    FragColor = texture(screenTexture, scaledUV);
}