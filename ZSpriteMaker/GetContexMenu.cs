using System.Linq;
using System.Windows.Controls;

namespace ZSpriteMaker
{
    public partial class MainWindow
    {

        private MenuItem[] CM_items;
        private MenuItem[] GetContextMenu(MenuType menuType)
        {
            switch (menuType)
            {
                case MenuType.OneObjectSelected:
                    CM_items = new MenuItem[5];

                    CM_items[0] = new MenuItem { Header = "Delete Selected Tile" };
                    CM_items[0].Click += CM_DeleteOAM;

                    CM_items[1] = new MenuItem { Header = "Copy Selected Tile" };
                    CM_items[1].Click += CM_CopyOAM;

                    CM_items[2] = new MenuItem { Header = "Cut Selected Tile" };
                    CM_items[2].Click += CM_CutOAM;

                    CM_items[3] = new MenuItem { Header = "Send to Front" };
                    CM_items[3].Click += CM_SendToFront;

                    CM_items[4] = new MenuItem { Header = "Send to Back" };
                    CM_items[4].Click += CM_SendToBack;

                    break;
                case MenuType.MultipleObjectSelected:
                    CM_items = new MenuItem[5];

                    CM_items[0] = new MenuItem { Header = "Delete Selected Tiles" };
                    CM_items[0].Click += CM_DeleteOAM;

                    CM_items[1] = new MenuItem { Header = "Copy Selected Tiles" };
                    CM_items[1].Click += CM_CopyOAM;

                    CM_items[2] = new MenuItem { Header = "Cut Selected Tiles" };
                    CM_items[2].Click += CM_CutOAM;

                    CM_items[3] = new MenuItem { Header = "Send All to Front" };
                    CM_items[3].Click += CM_SendToFront;

                    CM_items[4] = new MenuItem { Header = "Send All to Back" };
                    CM_items[4].Click += CM_SendToBack;

                    break;
                case MenuType.NoneSelected:
                    CM_items = new MenuItem[2];

                    CM_items[0] = new MenuItem { Header = "Add Tile" };
                    CM_items[0].Click += CM_AddOAM;

                    CM_items[1] = new MenuItem { Header = "Paste Tiles" };
                    CM_items[1].Click += CM_PasteOAM;

                    break;
            }
            return CM_items;
        }

        private void SetMenuItems(ContextMenu CM, MenuItem[] items)
        {
            foreach (MenuItem i in items)
            {
                CM.Items.Add(i);
            }
        }

        private void CM_DeleteOAM(object sender, System.Windows.RoutedEventArgs e)
        {
            Delete_Command(null, null);
        }

        private void CM_CopyOAM(object sender, System.Windows.RoutedEventArgs e)
        {
            Copy_Command(null, null);
        }

        private void CM_CutOAM(object sender, System.Windows.RoutedEventArgs e)
        {
            Cut_Command(null, null);
        }
        private void CM_AddOAM(object sender, System.Windows.RoutedEventArgs e)
        {
            editor.Frames[editor.SelectedFrame].Tiles.Add(new OamTile(CMX, CMY, false, false, editor.SelectedTile, editor.SelectedPalette, true));
            RefreshScreen();
        }

        private void CM_PasteOAM(object sender, System.Windows.RoutedEventArgs e)
        {
            Paste_Command(null, null);
        }


        private void CM_SendToFront(object sender, System.Windows.RoutedEventArgs e)
        {
            //Check all tiles in the editor set Z higher than any other tiles
            byte highestZ = 0;
            foreach (OamTile t in editor.Frames[editor.SelectedFrame].Tiles)
            {
                if (t.z >= highestZ)
                {
                    if (t.z < 63)
                    {
                        highestZ = (byte)(t.z + 1);
                    }
                    else
                    {
                        highestZ = 63;
                    }
                }
            }
            foreach (OamTile t in editor.selectedTiles)
            {
                t.z = highestZ;
            }

            editor.Frames[editor.SelectedFrame].Tiles = editor.Frames[editor.SelectedFrame].Tiles.OrderBy(t => t.z).ToList();
            RefreshScreen();
        }

        private void CM_SendToBack(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (OamTile t in editor.selectedTiles)
            {
                t.z = 0;
            }

            editor.Frames[editor.SelectedFrame].Tiles = editor.Frames[editor.SelectedFrame].Tiles.OrderBy(t => t.z).ToList();
            RefreshScreen();
        }

    }

    enum MenuType
    {
        OneObjectSelected, MultipleObjectSelected, NoneSelected
    }
}
