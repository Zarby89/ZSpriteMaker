using System;

namespace ZSpriteMaker
{
    [Serializable]
    internal class OamTile
    {
        private byte _x = 0;
        private byte _y = 0;
        public byte y
        {
            get => _y;
            set
            {
                if (value >= 220) { _y = 0; }
                else { _y = value; }
            }
        }

        public byte x
        {
            get => _x;
            set => _x = (byte)(value >= 252 ? 0 : value);
        }

        public byte z = 0;

        public byte tempx = 0;
        public byte tempy = 0;
        public bool mirrorX = false;
        public bool mirrorY = false;
        public ushort id = 0;
        public byte palette = 0;
        public byte priority = 3;
        public bool size = false;

        public OamTile(byte x, byte y, bool mirrorX, bool mirrorY, ushort id, byte palette, bool size, byte priority = 3)
        {
            this.x = x;
            this.y = y;
            this.mirrorX = mirrorX;
            this.mirrorY = mirrorY;
            this.id = id;
            this.palette = palette;
            this.priority = priority;
            this.size = size;
            this.tempx = x;
            this.tempy = y;
        }

        public byte[] GetXYTile()
        {
            byte[] xy = new byte[2];

            xy[1] = (byte)((id / 16) * 8); // Y Position
            xy[0] = (byte)((id % 16) * 8); // X Position

            return xy;
        }

        public override string ToString()
        {
            return (id.ToString() + "," + x.ToString() + "," + y.ToString() + "," + mirrorX.ToString() + "," + mirrorY.ToString() + "," + palette.ToString() + "," + size.ToString() + "," + z.ToString() + "," + priority.ToString());
        }

        public int ToInt()
        {
            //missing size and z
            return ((id << 16) | ((mirrorY ? 0 : 1) << 31) | ((mirrorX ? 0 : 1) << 30) | (priority << 28) | (palette << 25) | (x << 8) | y);
        }

    }
}
