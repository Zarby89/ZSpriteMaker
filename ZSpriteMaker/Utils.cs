using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ZSpriteMaker
{
    internal static class Utils
    {

        public static byte[] Decompress(byte[] romData, int pos, bool reversed = false)
        {
            byte[] buffer = new byte[0x1000];
            for (int i = 0; i < 0x1000; i++)
            {
                buffer[i] = 0;
            }
            int bufferPos = 0;
            while (true)
            {

                byte databyte = romData[pos];
                if (databyte == 0xFF) // End of decompression
                {
                    break;
                }

                byte cmd;
                int length;
                if ((databyte & 0xE0) == 0xE0) // Expanded Command
                {
                    cmd = (byte)((databyte >> 2) & 0x07);
                    length = (((databyte & 0x03) << 8) | romData[pos + 1]);
                    pos += 2; // Advance 2 bytes in ROM
                }
                else // Normal Command
                {
                    cmd = (byte)((databyte >> 5) & 0x07);
                    length = (byte)(databyte & 0x1F);
                    pos += 1; // Advance 1 byte in ROM
                }
                length += 1; // Every commands are at least 1 size even if 00
                switch (cmd)
                {
                    case 00: // Direct Copy
                        for (int i = 0; i < length; i++)
                        {
                            buffer[bufferPos++] = romData[pos++];
                        }
                        break;
                    case 01: // Byte Fill
                        for (int i = 0; i < length; i++)
                        {
                            buffer[bufferPos++] = romData[pos];
                        }
                        pos += 1; // Advance 1 byte in the ROM
                        break;
                    case 02: // Word Fill
                        for (int i = 0; i < length; i += 2)
                        {
                            buffer[bufferPos++] = romData[pos];
                            buffer[bufferPos++] = romData[pos + 1];
                        }
                        pos += 2; // Advance 2 byte in the ROM
                        break;
                    case 03: // Increasing Fill
                        byte incByte = romData[pos];
                        for (int i = 0; i < length; i++)
                        {
                            buffer[bufferPos++] = incByte++;
                        }
                        pos += 1; // Advance 1 byte in the ROM
                        break;
                    case 04: // Repeat (Reversed byte order for maps)
                        byte b1 = romData[pos + 1];
                        byte b2 = romData[pos];
                        int Addr = ((b1<<8) | b2);
                        for (int i = 0; i < length; i++)
                        {
                            buffer[bufferPos++] = buffer[Addr++];
                        }
                        pos += 2;  // Advance 2 bytes in the ROM
                        break;
                }
            }
            return buffer;
        }


        public static int SNEStoPC(this int addr) => (addr & 0x7FFF) | ((addr & 0x7F0000) >> 1);
        public static int PCtoSNES(this int addr) => (addr & 0x7FFF) | 0x8000 | ((addr & 0x7F8000) << 1);


        static readonly byte[] bitmask = { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };
         public static byte[] SNES3bppTo8bppSheet(byte[] buffer) // 128x32
         {
             byte[] data = new byte[0x1000];
             int xx = 0; //positions where we are at on the sheet
             int yy = 0;
             int pos = 0;
             int ypos = 0;
             for (int i = 0; i < 64; i++) //for each tiles //16 per lines
             {
                 for (int y = 0; y < 8; y++)//for each lines
                 {
                     for (int x = 0; x < 8; x++)
                     {

                         byte b1 = (byte)(buffer[(y * 2) + (24 * pos)] & (bitmask[x]));
                         byte b2 = (byte)(buffer[(y * 2) + (24 * pos) + 1] & (bitmask[x]));
                         byte b3 = (byte)(buffer[(16 + y) + (24 * pos)] & (bitmask[x]));
                         byte b = 0;
                         if (b1 != 0) { b |= 1; };
                         if (b2 != 0) { b |= 2; };
                         if (b3 != 0) { b |= 4; };
                         data[x + (xx) + (y * 128) + (yy * 1024)] = b;
                     }

                 }
                 pos++;
                 ypos++;
                 xx += 8;
                 if (ypos >= 16)
                 {
                     yy++;
                     xx = 0;
                     ypos = 0;

                 }
             }

             return data;
         }
        public static bool BitsAllSet(this byte b, byte test) => (b & test) == test;
        public static byte[] SNES3bppTo8bppSheet2(byte[] data)
        {
            int index = 0;
            byte[] sheetdata = new byte[0x1000];
            for (int j = 0; j < 4; j++) // Per Tile Line Y
            {
                for (int i = 0; i < 16; i++) // Per Tile Line X
                {
                    for (int y = 0; y < 8; y++) // Per Pixel Line
                    {
                        var lineBits0 = data[y * 2 + i * 24 + j * 384];
                        var lineBits1 = data[y * 2 + i * 24 + j * 384 + 1];
                        var lineBits2 = data[y + i * 24 + j * 384 + 16];

                        for (byte mask = 0x80; mask > 0; mask >>= 1) // Per Pixel X
                        {
                            byte pixdata = 0;

                            if (lineBits0.BitsAllSet(mask)) { pixdata |= 1; }
                            if (lineBits1.BitsAllSet(mask)) { pixdata |= 2; }
                            if (lineBits2.BitsAllSet(mask)) { pixdata |= 4; }

                            sheetdata[index++] = pixdata;
                        }
                    }
                }
            }
            return sheetdata;
        }


        public static Color ReadPaletteSingle(byte[] romData, int romPosition)
        {
            // Lets write new palette code since i can't find the old one :scream:
            Color color;
            short colorS = (short)((romData[romPosition + 1] << 8) + romData[romPosition]);
            color = Color.FromRgb((byte)((colorS & 0x1F) * 8), (byte)(((colorS >> 5) & 0x1F) * 8), (byte)(((colorS >> 10) & 0x1F) * 8));

            return color;
        }
    }
}
