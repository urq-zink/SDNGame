using System.Drawing;
using System.Runtime.InteropServices;

namespace SDNGame.Platform.Windows
{
    public class NativeLayeredWindow : IDisposable
    {
        #region Win32 API Constants and Structures

        // Window Styles
        private const int WS_POPUP = unchecked((int)0x80000000);
        private const int WS_EX_LAYERED = 0x80000;

        // UpdateLayeredWindow flags
        private const int ULW_ALPHA = 0x2;

        // Window messages
        private const uint WM_DESTROY = 0x0002;

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SIZE
        {
            public int cx;
            public int cy;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WNDCLASSEX
        {
            public int cbSize;
            public uint style;
            public nint lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public nint hInstance;
            public nint hIcon;
            public nint hCursor;
            public nint hbrBackground;
            public string? lpszMenuName;
            public string lpszClassName;
            public nint hIconSm;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public nint hwnd;
            public uint message;
            public nuint wParam;
            public nint lParam;
            public uint time;
            public POINT pt;
        }

        #endregion

        #region Win32 API Function Declarations

        public delegate nint WndProcDelegate(nint hWnd, uint msg, nint wParam, nint lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern nint CreateWindowEx(
            int dwExStyle,
            string lpClassName,
            string lpWindowName,
            int dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            nint hWndParent,
            nint hMenu,
            nint hInstance,
            nint lpParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UpdateLayeredWindow(
            nint hwnd,
            nint hdcDst,
            ref POINT pptDst,
            ref SIZE psize,
            nint hdcSrc,
            ref POINT pptSrc,
            int crKey,
            ref BLENDFUNCTION pblend,
            int dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern nint GetDC(nint hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int ReleaseDC(nint hWnd, nint hDC);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern nint CreateCompatibleDC(nint hDC);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteDC(nint hdc);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern nint SelectObject(nint hdc, nint hgdiobj);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(nint hObject);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(nint hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool GetMessage(out MSG lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage([In] ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern nint DispatchMessage([In] ref MSG lpmsg);

        [DllImport("user32.dll")]
        private static extern nint DefWindowProc(nint hWnd, uint msg, nint wParam, nint lParam);

        [DllImport("user32.dll")]
        private static extern void PostQuitMessage(int nExitCode);

        #endregion

        #region Fields

        private nint _hWnd = nint.Zero;
        private nint _hBitmap = nint.Zero;
        private nint _memDC = nint.Zero;
        private nint _screenDC = nint.Zero;
        private nint _oldBitmap = nint.Zero;

        // Fields to store window position and size.
        private SIZE _sizeStruct;
        private POINT _pointSource;
        private POINT _topPos;

        // Keep a reference to the delegate so it doesn't get garbage-collected.
        private WndProcDelegate _wndProcDelegate;
        private bool _disposed = false;

        #endregion

        /// <summary>
        /// Creates a new layered window that displays the specified PNG image.
        /// </summary>
        /// <param name="imagePath">Full path to the PNG image.</param>
        /// <param name="x">X position of the window on the screen.</param>
        /// <param name="y">Y position of the window on the screen.</param>
        public NativeLayeredWindow(string imagePath, int x = 760, int y = 340)
        {
            // Load the image from file.
            Bitmap bitmap;
            try
            {
                bitmap = new Bitmap(imagePath);
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading image: " + ex.Message, ex);
            }

            int width = bitmap.Width;
            int height = bitmap.Height;

            // Register a window class.
            string className = "MyLayeredWindowClass";
            WNDCLASSEX wndClass = new WNDCLASSEX();
            wndClass.cbSize = Marshal.SizeOf(typeof(WNDCLASSEX));
            wndClass.style = 0;
            _wndProcDelegate = new WndProcDelegate(WndProc);
            wndClass.lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate);
            wndClass.cbClsExtra = 0;
            wndClass.cbWndExtra = 0;
            wndClass.hInstance = nint.Zero;
            wndClass.hIcon = nint.Zero;
            wndClass.hCursor = nint.Zero;
            wndClass.hbrBackground = nint.Zero;
            wndClass.lpszMenuName = null;
            wndClass.lpszClassName = className;
            wndClass.hIconSm = nint.Zero;

            ushort regResult = RegisterClassEx(ref wndClass);
            if (regResult == 0)
                throw new Exception("Failed to register window class.");

            // Create the window.
            _hWnd = CreateWindowEx(
                WS_EX_LAYERED,
                className,
                "Layered Window",
                WS_POPUP,
                x,
                y,
                width,
                height,
                nint.Zero,
                nint.Zero,
                nint.Zero,
                nint.Zero);

            if (_hWnd == nint.Zero)
                throw new Exception("Failed to create window.");

            // Prepare to update the layered window.
            _screenDC = GetDC(nint.Zero);
            _memDC = CreateCompatibleDC(_screenDC);

            // Create an HBITMAP from the Bitmap with transparency.
            _hBitmap = bitmap.GetHbitmap(Color.FromArgb(0)); // Ensure transparency
            _oldBitmap = SelectObject(_memDC, _hBitmap);

            // Set up the size and position structures.
            _sizeStruct = new SIZE { cx = width, cy = height };
            _pointSource = new POINT { x = 0, y = 0 };
            _topPos = new POINT { x = x, y = y };

            // Set up blend function for per-pixel alpha with full opacity.
            BLENDFUNCTION blend = new BLENDFUNCTION
            {
                BlendOp = 0,    // AC_SRC_OVER
                BlendFlags = 0,
                SourceConstantAlpha = 255, // fully opaque
                AlphaFormat = 1 // AC_SRC_ALPHA
            };

            // Update the layered window with the bitmap.
            bool updateResult = UpdateLayeredWindow(_hWnd, _screenDC, ref _topPos, ref _sizeStruct,
                _memDC, ref _pointSource, 0, ref blend, ULW_ALPHA);

            if (!updateResult)
                throw new Exception("UpdateLayeredWindow failed.");

            // Show the window.
            ShowWindow(_hWnd, 1);

            // Dispose of the managed Bitmap; its HBITMAP is now owned by the GDI.
            bitmap.Dispose();
        }

        /// <summary>
        /// Runs a minimal message loop for the window.
        /// This call is blocking until the window is closed.
        /// </summary>
        public void Run()
        {
            MSG msg;
            while (GetMessage(out msg, nint.Zero, 0, 0))
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }

        /// <summary>
        /// The window procedure.
        /// </summary>
        private static nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam)
        {
            if (msg == WM_DESTROY)
            {
                PostQuitMessage(0);
                return nint.Zero;
            }
            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        /// <summary>
        /// Updates the window's alpha transparency.
        /// </summary>
        /// <param name="alpha">Alpha value (0-255).</param>
        private void UpdateAlpha(byte alpha)
        {
            BLENDFUNCTION blend = new BLENDFUNCTION
            {
                BlendOp = 0,    // AC_SRC_OVER
                BlendFlags = 0,
                SourceConstantAlpha = alpha,
                AlphaFormat = 1 // AC_SRC_ALPHA
            };

            UpdateLayeredWindow(_hWnd, _screenDC, ref _topPos, ref _sizeStruct,
                _memDC, ref _pointSource, 0, ref blend, ULW_ALPHA);
        }

        /// <summary>
        /// Fades the window in, holds for a specified duration, then fades it out.
        /// </summary>
        /// <param name="fadeInDurationMs">Fade-in duration in milliseconds.</param>
        /// <param name="holdDurationMs">Hold duration in milliseconds.</param>
        /// <param name="fadeOutDurationMs">Fade-out duration in milliseconds.</param>
        public void FadeInHoldFadeOut(int fadeInDurationMs, int holdDurationMs, int fadeOutDurationMs)
        {
            int steps = 50;
            // Fade In
            for (int i = 0; i <= steps; i++)
            {
                byte alpha = (byte)(i * 255 / steps);
                UpdateAlpha(alpha);
                Thread.Sleep(fadeInDurationMs / steps);
            }
            // Hold
            Thread.Sleep(holdDurationMs);
            // Fade Out
            for (int i = steps; i >= 0; i--)
            {
                byte alpha = (byte)(i * 255 / steps);
                UpdateAlpha(alpha);
                Thread.Sleep(fadeOutDurationMs / steps);
            }
        }

        #region IDisposable Support

        public void Dispose()
        {
            if (!_disposed)
            {
                // Clean up GDI objects.
                if (_memDC != nint.Zero)
                {
                    if (_oldBitmap != nint.Zero)
                        SelectObject(_memDC, _oldBitmap);
                    DeleteDC(_memDC);
                }
                if (_hBitmap != nint.Zero)
                    DeleteObject(_hBitmap);
                if (_screenDC != nint.Zero)
                    ReleaseDC(nint.Zero, _screenDC);

                _disposed = true;
            }
        }

        #endregion

        #region New Method: HideWindow
        /// <summary>
        /// Hides the layered window.
        /// </summary>
        public void HideWindow()
        {
            if (_hWnd != nint.Zero)
            {
                ShowWindow(_hWnd, 0); // SW_HIDE = 0
            }
        }
        #endregion
    }
}
