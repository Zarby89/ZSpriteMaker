using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ZSpriteMaker
{
    internal class Editor
    {
        public byte[] ROM { get; set; }
        public byte[] allSpritesData = new byte[0x70000];
        public byte SelectedFrame { get; set; } = 0;
        public ushort SelectedTile { get; set; } = 0;
        public byte SelectedPalette { get; set; } = 0;
        public int UndoBufferSize { get; set; } = 0;

        public Frame[] Frames { get; set; } = new Frame[48];

        public List<OamTile> selectedTiles = new();
        readonly Color[] spritescolors = new Color[256]; // 128 to 256 is used by the editor

        public Editor(byte[] ROM)
        {
            this.ROM = ROM;
            //115 to 126 not compressed 
            //127 to 217
            int sheetID = 0;
            for (int j = 115; j <= 126; j++)
            {
                int snesAddr = ((ROM[0x4F80 + j] << 16) | ((ROM[0x505F + j]) << 8) | ROM[0x513E + j]);
                int pcAddr = Utils.SNEStoPC(snesAddr);
                byte[] decompressedData = ROM[pcAddr..(pcAddr + 0x800)];

                byte[] sheet = Utils.SNES3bppTo8bppSheet(decompressedData);
                for (int i = 0; i < 0x1000; i++)
                {
                    allSpritesData[i + (sheetID * 0x1000)] = sheet[i];
                }
                sheetID++;

            }
            for (int j = 127; j <= 217; j++)
            {
                int snesAddr = ((ROM[0x4F80 + j] << 16) | ((ROM[0x505F + j]) << 8) | ROM[0x513E + j]);
                int pcAddr = Utils.SNEStoPC(snesAddr);
                int cs = 0;
                byte[] decompressedData = ZCompressLibrary.Decompress.ALTTPDecompressGraphics(ROM, pcAddr, 0x1000, ref cs); //Utils.Decompress(ROM, pcAddr);
                byte[] sheet = Utils.SNES3bppTo8bppSheet(decompressedData);

                for (int i = 0; i < 0x1000; i++)
                {
                    allSpritesData[i + (sheetID * 0x1000)] = sheet[i];
                }
                sheetID++;
            }

            for (int i = 0; i < 48; i++)
            {
                Frames[i] = new();
                AddUndo(i);
            }



            byte b = 0x80;
            b >>= 1;
            Console.WriteLine(b);
        }

        public void Draw(PointeredImage dest, PointeredImage source)
        {
            Frames[SelectedFrame].Draw(dest, source);
            foreach (OamTile tile in selectedTiles)
            {
                int size = (tile.size ? 16 : 8);
                dest.WriteBitmapRect(tile.x, tile.y, size, size, 243);
            }


        }
        public void DrawPrevious(PointeredImage dest, PointeredImage source)
        {
            if (SelectedFrame != 0)
            {
                Frames[SelectedFrame - 1].Draw(dest, source);
            }
        }


        public void UpdateSheets(PointeredImage sheetsImage, PointeredImage sheetsImageoverlay, byte[] sheetsids)
        {
            for (int j = 0; j < 8; j++)
            {
                for (int i = 0; i < 0x1000; i++)
                {
                    sheetsImage[i + (j * 0x1000)] = allSpritesData[i + (sheetsids[j] * 0x1000)];
                }
            }


            int tileY = SelectedTile / 16;
            int tileX = SelectedTile - (tileY * 16);

            sheetsImageoverlay.ClearBitmap(0);
            sheetsImageoverlay.WriteBitmapRect((tileX * 8), (tileY * 8), 16, 16, 243);
            sheetsImageoverlay.UpdateBitmap();
            sheetsImage.UpdateBitmap();
        }

        public void UpdatePalette(PointeredImage sheetsImage)
        {
            Color[] sheetPalette = new Color[256];
            spritescolors.CopyTo(sheetPalette, 0);
            for (int i = 0; i < 16; i++)
            {
                sheetPalette[i] = spritescolors[(SelectedPalette * 16) + i];
            }
            sheetsImage.UpdatePalette(sheetPalette);
            sheetsImage.UpdateBitmap();
        }

        const int globalSpritePalettesLW = 0xDD218;
        const int armorPalettes = 0xDD308; // Green, Blue, Red, Bunny, Electrocuted (15 colors each)
        const int spritePalettesAux1 = 0xDD39E; // 7 colors each
        const int spritePalettesAux2 = 0xDD446; // 7 colors each
        const int spritePalettesAux3 = 0xDD4E0; // 7 colors each
        const int swordPalettes = 0xDD630; // 3 colors each - 4 entries
        const int shieldPalettes = 0xDD648; // 4 colors each - 3 entries


        public Color[] GetPalettes(byte spr0 = 0, byte globalID = 0, byte spr1 = 3, byte spr2 = 1, byte pot = 10, byte sword = 0, byte shield = 0, byte link = 0)
        {



            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 15; i++)
                {
                    spritescolors[(i + 1) + (j * 16) + 16] = Utils.ReadPaletteSingle(ROM, globalSpritePalettesLW + ((i + (j * 15)) * 2) + (globalID * 120));
                }
            }

            for (int i = 0; i < 7; i++)
            {
                spritescolors[(i + 1)] = Utils.ReadPaletteSingle(ROM, spritePalettesAux1 + (spr0 * 14) + (i * 2));

                spritescolors[(i + 1) + 96] = Utils.ReadPaletteSingle(ROM, (spritePalettesAux3 + (spr2 * 14)) + (i * 2));
                spritescolors[(i + 1) + 80] = Utils.ReadPaletteSingle(ROM, (spritePalettesAux3 + (spr1 * 14)) + (i * 2));
                spritescolors[(i + 1) + 104] = Utils.ReadPaletteSingle(ROM, (spritePalettesAux2 + (pot * 14)) + (i * 2));
            }

            for (int i = 0; i < 3; i++)
            {
                spritescolors[(i + 1) + 88] = Utils.ReadPaletteSingle(ROM, (swordPalettes + (sword * 6)) + (i * 2));
            }
            for (int i = 0; i < 4; i++)
            {
                spritescolors[(i + 1) + 91] = Utils.ReadPaletteSingle(ROM, (shieldPalettes + (shield * 8)) + (i * 2));
            }


            for (int i = 0; i < 15; i++)
            {
                spritescolors[(i + 1) + 112] = Utils.ReadPaletteSingle(ROM, armorPalettes + (link * 30) + (i * 2));
            }


            GetEditorPalettes();


            return spritescolors;
        }

        public void GetEditorPalettes()
        {
            //======================================================================
            //Editor Related Palettes to do background draw, selection, etc
            //======================================================================
            spritescolors[0] = Color.FromArgb(0, 0, 0, 0);
            spritescolors[240] = Color.FromRgb(102, 102, 102);
            spritescolors[241] = Color.FromRgb(153, 153, 153);

            spritescolors[242] = Color.FromRgb(
                Properties.Settings.Default.gridR,
                Properties.Settings.Default.gridG,
                Properties.Settings.Default.gridB);

            spritescolors[243] = Color.FromRgb(
                Properties.Settings.Default.selR,
                Properties.Settings.Default.selG,
                Properties.Settings.Default.selB);


            spritescolors[244] = Color.FromRgb(150, 253, 100);

            spritescolors[245] = Color.FromRgb(
    Properties.Settings.Default.hitboxR,
    Properties.Settings.Default.hitboxG,
    Properties.Settings.Default.hitboxB);
        }



        public byte mouseDownX = 0;
        public byte mouseDownY = 0;
        public bool mouseDown = false;
        int movedOffsetX = 0;
        int movedOffsetY = 0;
        public bool multiSelection = false;
        byte mouse_x_old = 0;
        byte mouse_y_old = 0;
        byte rectXStart = 0;
        byte rectYStart = 0;
        byte rectXEnd = 0;
        byte rectYEnd = 0;
        public void MouseDown(MouseEventArgs e, int DPI, PointeredImage main, PointeredImage layer, PointeredImage sheetImage)
        {

            byte mouse_x = (byte)(e.GetPosition((IInputElement)e.Source).X / DPI);
            byte mouse_y = (byte)(e.GetPosition((IInputElement)e.Source).Y / DPI);

            mouseDownX = mouse_x;
            mouseDownY = mouse_y;
            rectXEnd = mouse_x;
            rectYEnd = mouse_y;
            rectXStart = mouse_x;
            rectYStart = mouse_y;
            mouseDown = true;

            foreach (OamTile tile in selectedTiles)
            {
                tile.tempx = tile.x;
                tile.tempy = tile.y;
                if (mouseDownX >= tile.x && mouseDownX <= tile.x + (tile.size ? 16 : 8)
                && mouseDownY >= tile.y && mouseDownY <= tile.y + (tile.size ? 16 : 8))
                {
                    multiSelection = true;
                }
            }

            if (multiSelection == false)
            {
                selectedTiles.Clear();
                foreach (OamTile tile in Frames[SelectedFrame].Tiles)
                {
                    if (mouseDownX >= tile.x && mouseDownX <= tile.x + (tile.size ? 16 : 8)
                    && mouseDownY >= tile.y && mouseDownY <= tile.y + (tile.size ? 16 : 8))
                    {
                        tile.tempx = tile.x;
                        tile.tempy = tile.y;
                        selectedTiles.Add(tile);
                        break;
                    }
                }
            }

            if (e.RightButton == MouseButtonState.Pressed)
            {
                mouseDown = false;
                multiSelection = false;
            }

            main.ClearBitmap(0);
            Draw(main, sheetImage);
            main.UpdateBitmap();
        }

        public void MouseMove(MouseEventArgs e, int DPI, PointeredImage main, PointeredImage layer, PointeredImage sheetImage)
        {
            byte mouse_x = (byte)(e.GetPosition((IInputElement)e.Source).X / DPI);
            byte mouse_y = (byte)(e.GetPosition((IInputElement)e.Source).Y / DPI);

            if (mouseDown)
            {

                if (selectedTiles.Count == 0) // to draw rectangle, no need to refresh the same everytime mouse move otherwise
                {
                    main.ClearBitmap(0);
                    Draw(main, sheetImage);
                    rectXEnd = mouse_x;
                    rectYEnd = mouse_y;
                    rectXStart = mouseDownX;
                    rectYStart = mouseDownY;

                    if (mouse_x < mouseDownX)
                    {
                        rectXStart = mouse_x;
                        rectXEnd = mouseDownX;
                    }

                    if (mouse_y < mouseDownY)
                    {
                        rectYStart = mouse_y;
                        rectYEnd = mouseDownY;
                    }


                    main.WriteBitmapRect(rectXStart, rectYStart, rectXEnd - rectXStart, rectYEnd - rectYStart, 244);
                    main.UpdateBitmap();
                }
                else if (mouse_x != mouse_x_old || mouse_y != mouse_y_old)
                {
                    movedOffsetX = mouseDownX - mouse_x;
                    movedOffsetY = mouseDownY - mouse_y;

                    foreach (OamTile tile in selectedTiles)
                    {
                        tile.x = SnapByte((byte)(tile.tempx - movedOffsetX));
                        tile.y = SnapByte((byte)(tile.tempy - movedOffsetY));
                    }

                    main.ClearBitmap(0);
                    Draw(main, sheetImage);
                    main.UpdateBitmap();

                    if (Properties.Settings.Default.showOnion)
                    {
                        layer.ClearBitmap(0);
                        DrawPrevious(layer, sheetImage);
                        layer.UpdateBitmap();
                    }

                    mouse_x_old = mouse_x;
                    mouse_y_old = mouse_y;
                }

            }

        }

        public void MouseUp(MouseEventArgs e, int DPI, PointeredImage main, PointeredImage layer, PointeredImage sheetImage)
        {
            if (multiSelection == false)
            {
                if (selectedTiles.Count == 0)
                {
                    foreach (OamTile tile in Frames[SelectedFrame].Tiles)
                    {
                        if (tile.x + (tile.size ? 16 : 8) >= rectXStart && tile.x <= rectXEnd &&
                            tile.y + (tile.size ? 16 : 8) >= rectYStart && tile.y <= rectYEnd)
                        {
                            selectedTiles.Add(tile);
                        }
                    }
                }
            }

            foreach (OamTile tile in selectedTiles)
            {
                tile.tempx = tile.x;
                tile.tempy = tile.y;
            }




            multiSelection = false;
            mouseDown = false;

            main.ClearBitmap(0);
            Draw(main, sheetImage);
            main.UpdateBitmap();

        }

        public void AddUndo(int frame)
        {

            if (Frames[frame].undoPos != Frames[frame].undoTiles.Count - 1 && Frames[frame].undoPos != 20)
            {
                Frames[frame].undoTiles.RemoveRange(Frames[frame].undoPos, Frames[frame].undoTiles.Count - Frames[frame].undoPos);
            }

            Console.WriteLine("Added at : " + Frames[frame].undoPos);
            List<OamTile> newList = new List<OamTile>();
            foreach (OamTile tile in Frames[frame].Tiles)
            {
                newList.Add(new OamTile(tile.x, tile.y, tile.mirrorX, tile.mirrorY, tile.id, tile.palette, tile.size, tile.priority));
            }

            Frames[frame].undoTiles.Insert(Frames[frame].undoPos, newList);
            Frames[frame].undoPos++;
            if (Frames[frame].undoTiles.Count > UndoBufferSize)
            {
                Frames[frame].undoPos = UndoBufferSize;
                Frames[frame].undoTiles.RemoveAt(0);
            }
            Console.WriteLine("Now at position : " + Frames[frame].undoPos);
        }

        public void Undo(int DPI, PointeredImage main, PointeredImage layer, PointeredImage sheetImage)
        {
            if (Frames[SelectedFrame].undoPos > 0)
            {
                selectedTiles.Clear();
                Frames[SelectedFrame].undoPos--;
                Frames[SelectedFrame].Tiles = Frames[SelectedFrame].undoTiles[Frames[SelectedFrame].undoPos];
                main.ClearBitmap(0);
                Draw(main, sheetImage);
                main.UpdateBitmap();

            }

        }

        public void Redo(int DPI, PointeredImage main, PointeredImage layer, PointeredImage sheetImage)
        {

            if (Frames[SelectedFrame].undoPos < Frames[SelectedFrame].undoTiles.Count - 1)
            {
                selectedTiles.Clear();
                Frames[SelectedFrame].undoPos++;
                Frames[SelectedFrame].Tiles = Frames[SelectedFrame].undoTiles[Frames[SelectedFrame].undoPos];
                main.ClearBitmap(0);
                Draw(main, sheetImage);
                main.UpdateBitmap();


            }
        }

        public static byte SnapByte(byte b)
        {
            return (byte)((b / Properties.Settings.Default.snapSize) * Properties.Settings.Default.snapSize);
        }

        public static IEnumerable<T> FindVisualChilds<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield return (T)Enumerable.Empty<T>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject ithChild = VisualTreeHelper.GetChild(depObj, i);
                if (ithChild == null) continue;
                if (ithChild is T t) yield return t;
                foreach (T childOfChild in FindVisualChilds<T>(ithChild)) yield return childOfChild;
            }
        }

    }
}
