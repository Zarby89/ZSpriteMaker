using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ZSpriteMaker
{
    internal unsafe class PointeredImage
    {
        private readonly IntPtr ptr;
        private byte* Pointer => (byte*)ptr.ToPointer();
        public WriteableBitmap bitmap { get; set; }
        private readonly int width = 16;
        private readonly int height = 16;
        private readonly int bufferLength = 256;
        private int dpi;

        private Color[] colors = new Color[256];
        BitmapPalette palettes = BitmapPalettes.Gray256;

        public byte this[int x, int y]
        {
            get => Pointer[x + y * width];
            set
            {
                if (x < 0 || x >= width || y < 0 || y >= height) { return; }
                Pointer[x + (y * width)] = value;
            }
        }


        public byte this[int p]
        {
            get => Pointer[p];
            set
            {
                if (p < 0 || p >= bufferLength) { return; }
                Pointer[p] = value;
            }
        }

        public int DPI
        {
            get => dpi;
            set
            {
                if (dpi == value) { return; }
                dpi = value;
                bitmap = new WriteableBitmap(width, height, dpi, dpi, PixelFormats.Indexed8, palettes);
            }
        }

        public void UpdatePalette(Color[] colors)
        {
            this.colors = colors;
            palettes = new BitmapPalette(colors);
            bitmap = new WriteableBitmap(width, height, dpi, dpi, PixelFormats.Indexed8, palettes);

        }

        public void UpdateBitmap()
        {
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), ptr, bufferLength, width);
        }

        public void FillBitmapRect(int x, int y, int w, int h, byte[] data)
        {
            int ox = x;
            int oy = y;
            for (int i = 0; i < data.Length; i++)
            {
                this[ox, oy] = data[i];
                ox++;
                if ((ox - x) >= w)
                {
                    ox = x;
                    oy++;
                }
            }
            UpdateBitmap();

        }

        public void WriteBitmapRect(int x, int y, int w, int h, byte data)
        {
            for (int i = 0; i <= w; i++)
            {
                this[x + i, y] = data;
                this[x + i, y + h] = data;
            }
            for (int i = 0; i <= h; i++)
            {
                this[x, y + i] = data;
                this[x + w, y + i] = data;
            }
            UpdateBitmap();
        }

        public void WriteLineGrid(int xy, bool horizontal = true, byte value = 242)
        {
            if (horizontal)
            {
                for (int i = 0; i < 256; i++)
                {
                    this[i, xy] = value;
                }
            }
            else
            {
                for (int i = 0; i < 224; i++)
                {
                    this[xy, i] = value;
                }
            }



            //UpdateBitmap();
        }

        public void DrawBitmapTile(int x, int y, int w, int h, int xSource, int ySource, int palSource, PointeredImage source, byte mx, byte my)
        {
            for (int xl = 0; xl < w; xl++)
            {
                for (int yl = 0; yl < h; yl++)
                {
                    if (source[xSource + xl, ySource + yl] != 0)
                    {
                        int xx = xl * (1 - mx) + ((w - 1) - xl) * (mx);
                        int yy = yl * (1 - my) + ((h - 1) - yl) * (my);

                        this[xx + x, yy + y] = (byte)(source[xSource + xl, ySource + yl] + (palSource * 16));
                    }
                }
            }
            //UpdateBitmap();
        }

        public void FillBitmapRect(int x, int y, int w, int h, byte data)
        {
            int ox = x;
            int oy = y;
            for (int i = 0; i < (w * h); i++)
            {
                this[ox, oy] = data;
                ox++;
                if ((ox - x) >= w)
                {
                    ox = x;
                    oy++;
                }
            }
            UpdateBitmap();

        }

        public PointeredImage(int width, int height, int dpi, byte clear = 0)
        {
            this.width = width;
            this.height = height;
            this.bufferLength = width * height;
            this.dpi = dpi;
            ptr = Marshal.AllocHGlobal(width * height);
            colors = palettes.Colors.ToArray();
            bitmap = new WriteableBitmap(width, height, dpi, dpi, PixelFormats.Indexed8, palettes);
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), ptr, width * height, width);
            ClearBitmap(clear);
        }

        public void ClearBitmap(byte clear)
        {
            for (int i = 0; i < bufferLength; i++)
            {
                Pointer[i] = clear;
            }
        }



    }
}
