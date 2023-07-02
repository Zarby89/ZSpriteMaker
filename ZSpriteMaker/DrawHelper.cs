using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZSpriteMaker
{
    internal static class DrawHelper
    {
        public static void DrawCheckerboard(PointeredImage image)
        {
            for (int x = 0; x < 16 * 16; x += 8)
            {
                for (int y = x % 16; y < 14 * 16; y += 16)
                {
                    image.FillBitmapRect(x, y, 8, 8, 241);
                }
            }
        }

        public static void DrawGrid(PointeredImage image, int size)
        {
            for (int i = size; i < (16 * 16); i += size)
            {
                image.WriteLineGrid(i, true, 242);
                image.WriteLineGrid(i, false, 242);
            }
        }

        public static void DrawCenter(PointeredImage image)
        {
            image.WriteBitmapRect(120, 112, 16, 0, 243);
            image.WriteBitmapRect(128, 104, 0, 16, 243);
        }



    }
}
