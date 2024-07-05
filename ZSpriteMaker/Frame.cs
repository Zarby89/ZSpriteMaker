using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZSpriteMaker
{
    internal class Frame
    {
        public List<List<OamTile>> undoTiles = new List<List<OamTile>>();
        public List<OamTile> Tiles { get; set; } = new List<OamTile>();
        public int undoPos = 0;
        public Frame()
        {

        }

        public void Draw(PointeredImage dest, PointeredImage source)
        {

            foreach (OamTile tile in Tiles)
            {
                int size = tile.size ? 16 : 8;
                byte[] xy = tile.GetXYTile();
                dest.DrawBitmapTile(tile.x, tile.y, size, size, xy[0], xy[1], tile.palette, source, (byte)(tile.mirrorX ? 1 : 0), (byte)(tile.mirrorY ? 1 : 0));
            }
        }

    }
}
