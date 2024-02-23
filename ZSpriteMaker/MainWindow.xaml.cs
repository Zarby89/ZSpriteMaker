using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit;
using ICSharpCode;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.Xml;
using System.Xml.Linq;

namespace ZSpriteMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            using (var stream = new FileStream("asmalttp.xshd", FileMode.Open, FileAccess.Read))
            {
                using (var reader = new XmlTextReader(stream))
                {
                    HighlightingManager.Instance.RegisterHighlighting("ASM", new string[0],
                        HighlightingLoader.Load(reader,
                           HighlightingManager.Instance));
                }
            }

            InitializeComponent();
            codeEditor.FontFamily = new FontFamily(Properties.Settings.Default.FontFamily);
            codeEditor.FontSize = Properties.Settings.Default.FontSize;
            codeEditor.ShowLineNumbers = true;

            CreateJSL();
            foreach (ListBoxItem li in functionListItems)
            {
                functionsListbox.Items.Add(li);
            }

            CreateMacros();

            foreach (ListBoxItem li in macrosListItems)
            {
                macrosListbox.Items.Add(li);
            }

            foreach (ListBoxItem li in ramListbox.Items)
            {
                ramListItems.Add(li);
            }
            //ReadSymbolsFile();
            errorLabel.Text = "";
        }

        List<ListBoxItem> functionListItems = new List<ListBoxItem>();
        List<ListBoxItem> macrosListItems = new List<ListBoxItem>();
        List<ListBoxItem> ramListItems = new List<ListBoxItem>();
        List<UserRoutine> userRoutines = new List<UserRoutine>();
        string longMain;
        string draw;
        string prep;

        private void ReadSymbolsFile()
        {

            string[] lines = File.ReadAllLines("symbols_wram.asm");
            int i = 0;
            while(i < lines.Length)
            {
                string name = "";
                StringBuilder desc = new StringBuilder();
                string addr = "";
                if (!lines[i].StartsWith(';'))
                {
                    if (!lines[i].Contains('='))
                    {
                        i++;
                        continue;
                    }
                    string[] nameaddr = lines[i].Split('=');
                    name = nameaddr[0].Trim();
                    addr = nameaddr[1].Trim();
                    //read all previous lines until we hit a comment
                    //save that line then continue until we hit a white space line
                    //then that's the description

                    int lastCommentLine = -1;
                    int firstCommentLine = -1;
                    for (int j = i-1; j > 0; j--)
                    {
                        if (lines[j].Length == 0)
                        {
                            firstCommentLine = j + 1;
                            break;
                        }

                        if (lines[j].StartsWith(';')) // first comment we encounter
                        {
                            if (lastCommentLine == -1) //is it really the first one?
                            {
                                lastCommentLine = j;
                            }
                        }
                        else 
                        {
                            if (lastCommentLine != -1) // did we already found a comment ?
                            {
                                firstCommentLine = j+1;
                                break;
                            }
                        }
                    }

                    if (lastCommentLine != -1)
                    {
                        if (firstCommentLine == -1)
                        {
                            desc.AppendLine(lines[lastCommentLine]);
                        }
                        else
                        {
                            for (int j = firstCommentLine; j < lastCommentLine; j++)
                            {
                                desc.AppendLine(lines[j]);
                            }
                        }
                    }
                    else
                    {
                        desc.Append("No Description!");
                    }

                    ListBoxItem li = new ListBoxItem();
                    li.Content = addr + " | " +  name;
                    li.ToolTip = desc;
                    zelda3symbols.Add(li);

                }

                i++;
            }



        }
        Stopwatch stopwatch = new Stopwatch();
        List<ListBoxItem> zelda3symbols = new List<ListBoxItem>();
        Editor editor;
        int cDPI = 2;
        int sheetDPI = 2;
        int timer = 0;
        int timerWait = 8;
        bool animationPlaying = false;
        bool projectLoaded = false;
        bool changedFromForm = false;
        bool logicEdtiorSelected = false;
        string sprName = "Template";

        string savedFile = "";


        readonly PointeredImage MainScreenBack_Image = new(256, 224, 2, 240);
        readonly PointeredImage MainScreen_Image = new(256, 224, 2, 0);
        readonly PointeredImage MainScreenLayer_Image = new(256, 224, 2, 0);
        readonly PointeredImage SheetScreen_Image = new(128, 256, 2);
        readonly PointeredImage SheetScreenOverlay_Image = new(128, 256, 2);
        readonly PointeredImage PaletteScreen_Image = new(16, 8, 8);

        readonly List<AnimationGroup> animations = new();

        readonly Stopwatch animationStopwatch = new();
        readonly byte[] sheetValues = new byte[8] { 0, 10, 6, 7, 0, 0, 0, 0 };
        readonly List<Action> actions = new();
        readonly List<LogicState> logicStates = new();
        readonly List<SpriteState> spriteStates = new();


        private void Exit_Command(object sender, ExecutedRoutedEventArgs e)
        {

        }
        private void New_Command(object sender, ExecutedRoutedEventArgs e)
        {
            if (!File.Exists(Properties.Settings.Default.romPath))
            {
                MessageBox.Show("The editor cannot find a suitable ROM to use, you can set one in the menu Preferences/Settings");
                return;
            }

            CompositionTarget.Rendering += CompositionTarget_Rendering;

            CreateProject();

            animationStopwatch.Start();
        }
        private void Open_Command(object sender, ExecutedRoutedEventArgs e)
        {



            OpenFileDialog ofd = new()
            {
                Filter = "ZSprite Maker Project File (*.zsm)|*.zsm",
                DefaultExt = ".zsm"
            };

            if (ofd.ShowDialog() == true)
            {
                CreateProject(true);
                userRoutines.Clear();
                savedFile = ofd.FileName;
                FileStream fs = new(ofd.FileName, FileMode.Open, FileAccess.Read);

                BinaryReader br = new BinaryReader(fs);

                int aCount = br.ReadInt32(); // int 

                for(int i = 0;i<aCount;i++)
                {
                    string aname = br.ReadString();
                    byte afs = br.ReadByte();
                    byte afe = br.ReadByte();
                    byte afspeed = br.ReadByte();

                    animations.Add(new AnimationGroup(afs,afe,afspeed,aname));
                }
                RefreshAnimations();
                

                int fCount = br.ReadInt32(); // int 
                for (int i = 0; i < fCount; i++)
                {
                    editor.Frames[i] = new Frame();
                    int tCount = br.ReadInt32(); // int //number of tiles

                    for (int j = 0; j < tCount; j++)
                    {
                        ushort tid = br.ReadUInt16();
                        byte tpal = br.ReadByte();
                        bool tmx = br.ReadBoolean();
                        bool tmy = br.ReadBoolean();
                        byte tprior = br.ReadByte();
                        bool tsize = br.ReadBoolean();
                        byte tx = br.ReadByte();
                        byte ty = br.ReadByte();
                        byte tz = br.ReadByte();
                        OamTile to = new OamTile(tx, ty, tmx, tmy, tid, tpal, tsize, tprior);
                        to.z = tz;
                        editor.Frames[i].Tiles.Add(to);

                    }
                }

                //all sprites properties
                property_blockable.IsChecked = br.ReadBoolean();
                property_canfall.IsChecked = br.ReadBoolean();
                property_collisionlayer.IsChecked = br.ReadBoolean();
                property_customdeath.IsChecked = br.ReadBoolean();
                property_damagesound.IsChecked = br.ReadBoolean();
                property_deflectarrows.IsChecked = br.ReadBoolean();
                property_deflectprojectiles.IsChecked = br.ReadBoolean();
                property_fast.IsChecked = br.ReadBoolean();
                property_harmless.IsChecked = br.ReadBoolean();
                property_impervious.IsChecked = br.ReadBoolean();
                property_imperviousarrow.IsChecked = br.ReadBoolean();
                property_imperviousmelee.IsChecked = br.ReadBoolean();
                property_interaction.IsChecked = br.ReadBoolean();
                property_isboss.IsChecked = br.ReadBoolean();
                property_persist.IsChecked = br.ReadBoolean();
                property_shadow.IsChecked = br.ReadBoolean();
                property_smallshadow.IsChecked = br.ReadBoolean();
                property_statis.IsChecked = br.ReadBoolean();
                property_statue.IsChecked = br.ReadBoolean();
                property_watersprite.IsChecked = br.ReadBoolean();

                property_prize.Text = br.ReadByte().ToString();
                property_palette.Text = br.ReadByte().ToString();
                property_oamnbr.Text = br.ReadByte().ToString();
                property_hitbox.Text = br.ReadByte().ToString();
                property_health.Text = br.ReadByte().ToString();
                property_damage.Text = br.ReadByte().ToString();

                if (br.BaseStream.Position != br.BaseStream.Length)
                {
                    property_sprname.Text = br.ReadString();

                    int actionL = br.ReadInt32();
                    for(int i = 0; i < actionL; i++)
                    {

                        string a = br.ReadString();
                        string b = br.ReadString();
                        userRoutines.Add(new UserRoutine(a, b));
                    }

                }

                if (br.BaseStream.Position != br.BaseStream.Length)
                {
                    property_sprid.Text = br.ReadString();
                    br.Close();
                }



                UpdateUserRoutines();
                userroutinesListbox.SelectedIndex = 0;
            }
        }
        private void Save_Command(object sender, ExecutedRoutedEventArgs e)
        {
            if (savedFile != "")
            {
                Save(savedFile);
                return;
            }
            SaveFileDialog sfd = new()
            {
                Filter = "ZSprite Maker Project File (*.zsm)|*.zsm",
                DefaultExt = ".zsm"
            };

            if (sfd.ShowDialog() == true)
            {
                Save(sfd.FileName);
            }
        }

        private void Save(string filename)
        {
            if (previousCode != -1)
            {
                userRoutines[previousCode].code = codeEditor.Text;
                //save that code
            }


            FileStream fs = new(filename, FileMode.OpenOrCreate, FileAccess.Write);

            BinaryWriter bw = new BinaryWriter(fs);

            bw.Write(animations.Count); // int 
            foreach (AnimationGroup anim in animations)
            {
                bw.Write(anim.FrameName); // string
                bw.Write(anim.FrameStart); // byte
                bw.Write(anim.FrameEnd);  // byte
                bw.Write(anim.FrameSpeed);  // byte
            }

            bw.Write(editor.Frames.Length); // int 
            for (int i = 0; i < editor.Frames.Length; i++)
            {
                bw.Write(editor.Frames[i].Tiles.Count); // int //number of tiles

                for (int j = 0; j < editor.Frames[i].Tiles.Count; j++)
                {
                    bw.Write(editor.Frames[i].Tiles[j].id); //ushort
                    bw.Write(editor.Frames[i].Tiles[j].palette); //byte
                    bw.Write(editor.Frames[i].Tiles[j].mirrorX); //bool
                    bw.Write(editor.Frames[i].Tiles[j].mirrorY); //bool
                    bw.Write(editor.Frames[i].Tiles[j].priority); //byte
                    bw.Write(editor.Frames[i].Tiles[j].size); //bool
                    bw.Write(editor.Frames[i].Tiles[j].x); //byte
                    bw.Write(editor.Frames[i].Tiles[j].y); //byte
                    bw.Write(editor.Frames[i].Tiles[j].z); //byte
                }
            }

            //all sprites properties
            bw.Write((bool)property_blockable.IsChecked);
            bw.Write((bool)property_canfall.IsChecked);
            bw.Write((bool)property_collisionlayer.IsChecked);
            bw.Write((bool)property_customdeath.IsChecked);
            bw.Write((bool)property_damagesound.IsChecked);
            bw.Write((bool)property_deflectarrows.IsChecked);
            bw.Write((bool)property_deflectprojectiles.IsChecked);
            bw.Write((bool)property_fast.IsChecked);
            bw.Write((bool)property_harmless.IsChecked);
            bw.Write((bool)property_impervious.IsChecked);
            bw.Write((bool)property_imperviousarrow.IsChecked);
            bw.Write((bool)property_imperviousmelee.IsChecked);
            bw.Write((bool)property_interaction.IsChecked);
            bw.Write((bool)property_isboss.IsChecked);
            bw.Write((bool)property_persist.IsChecked);
            bw.Write((bool)property_shadow.IsChecked);
            bw.Write((bool)property_smallshadow.IsChecked);
            bw.Write((bool)property_statis.IsChecked);
            bw.Write((bool)property_statue.IsChecked);
            bw.Write((bool)property_watersprite.IsChecked);


            bw.Write(byte.Parse(property_prize.Text, NumberStyles.HexNumber));
            bw.Write(byte.Parse(property_palette.Text, NumberStyles.HexNumber));
            bw.Write(byte.Parse(property_oamnbr.Text, NumberStyles.HexNumber));
            bw.Write(byte.Parse(property_hitbox.Text, NumberStyles.HexNumber));
            bw.Write(byte.Parse(property_health.Text, NumberStyles.HexNumber));
            bw.Write(byte.Parse(property_damage.Text, NumberStyles.HexNumber));

            bw.Write(sprName); // string

            bw.Write(userRoutines.Count); // int 
            foreach (UserRoutine userR in userRoutines)
            {
                bw.Write(userR.name); // string
                bw.Write(userR.code); // string
            }

            bw.Write(property_sprid.Text);
            bw.Close();
        }


        private void SaveAs_Command(object sender, ExecutedRoutedEventArgs e)
        {

            SaveFileDialog sfd = new()
            {
                Filter = "ZSprite Maker Project File (*.zsm)|*.zsm",
                DefaultExt = ".zsm"
            };

            if (sfd.ShowDialog() == true)
            {
                Save(sfd.FileName);
            }
        }



        private void Undo_Command(object sender, ExecutedRoutedEventArgs e)
        {

        }
        private void Redo_Command(object sender, ExecutedRoutedEventArgs e)
        {

        }
        private void Cut_Command(object sender, ExecutedRoutedEventArgs e)
        {
            if (logicEdtiorSelected)
            {

            }
            else
            {
                if (editor.selectedTiles.Count > 0)
                {
                    Clipboard.Clear();
                    Clipboard.SetData("OamTiles", editor.selectedTiles.ToArray());
                    foreach (OamTile t in editor.selectedTiles)
                    {
                        editor.Frames[editor.SelectedFrame].Tiles.Remove(t);
                    }
                    editor.selectedTiles.Clear();
                    RefreshScreen();
                }
            }
        }
        private void Copy_Command(object sender, ExecutedRoutedEventArgs e)
        {
            if (logicEdtiorSelected)
            {

            }
            else
            {
                if (editor.selectedTiles.Count > 0)
                {
                    Clipboard.Clear();
                    Clipboard.SetData("OamTiles", editor.selectedTiles.ToArray());
                }
            }
        }
        private void Paste_Command(object sender, ExecutedRoutedEventArgs e)
        {

            if (logicEdtiorSelected)
            {

            }
            else
            {
                if (Clipboard.ContainsData("OamTiles"))
                {
                    OamTile[] data = (OamTile[])Clipboard.GetData("OamTiles");
                    byte leftMost = 255;
                    byte topMost = 224;
                    foreach (OamTile tile in data)
                    {
                        //locate the top left most tile
                        if (tile.x < leftMost) { leftMost = tile.x; }
                        if (tile.y < topMost) { topMost = tile.y; }
                    }

                    editor.mouseDownX = leftMost;
                    editor.mouseDownY = topMost;
                    editor.multiSelection = true;
                    editor.mouseDown = true;

                    editor.selectedTiles.Clear();


                    editor.selectedTiles.AddRange(data);
                    editor.Frames[editor.SelectedFrame].Tiles.AddRange(data);

                }
            }


        }
        private void SelectAll_Command(object sender, ExecutedRoutedEventArgs e)
        {

        }
        private void Delete_Command(object sender, ExecutedRoutedEventArgs e)
        {
                foreach (OamTile tile in editor.selectedTiles)
                {
                    editor.Frames[editor.SelectedFrame].Tiles.Remove(tile);
                }
                editor.selectedTiles.Clear();
                RefreshScreen();
        }

        private void Shift_Command(object sender, ExecutedRoutedEventArgs e)
        {
            ShiftWindow window = new ShiftWindow();
            window.ShowDialog();

            foreach (Frame frame in editor.Frames)
            {
                foreach (OamTile tile in frame.Tiles)
                {
                    tile.x += (byte)window.xShift;
                    tile.y += (byte)window.yShift;
                }
            }
            RefreshScreen();
        }


        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (projectLoaded)
            {
                if (animationStopwatch.ElapsedMilliseconds > 16) // 60 fps rendering
                {
                    animationStopwatch.Reset();
                    if (animationPlaying)
                    {
                        timer++;
                        if (timer >= timerWait)
                        {
                            _ = byte.TryParse(animminTextbox.Text, out byte minFrame);
                            _ = byte.TryParse(animmaxTextbox.Text, out byte maxFrame);
                            FrameSlider.Value++;
                            if (minFrame >= maxFrame)
                            {
                                FrameSlider.Value = minFrame;
                            }
                            else
                            {
                                if (FrameSlider.Value > maxFrame)
                                {
                                    FrameSlider.Value = minFrame;
                                }
                            }
                            timer = 0;
                        }
                    }

                    animationStopwatch.Start();
                }
            }
        }


        private void CM_DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            Delete_Command(null, null);
        }
        private void CM_ShiftSelected_Click(object sender, RoutedEventArgs e)
        {
            Shift_Command(null, null);
        }
        byte CMX = 0;
        byte CMY = 0;
        private void MainScreen_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (projectLoaded)
            {
                
                logicEdtiorSelected = false;
                gridMainScreen.Focus();
                MainScreen.Focus();
                CMX = Editor.SnapByte((byte)(e.GetPosition((IInputElement)e.Source).X / cDPI));
                CMY = Editor.SnapByte((byte)(e.GetPosition((IInputElement)e.Source).Y / cDPI));

                editor.MouseDown(e, cDPI, MainScreen_Image, MainScreenLayer_Image, SheetScreen_Image);


                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    editor.Frames[editor.SelectedFrame].Tiles.Add(new OamTile(CMX, CMY, false, false, editor.SelectedTile, editor.SelectedPalette, true));
                    RefreshScreen();
                    return;
                }
                //Handle for the context menu here
                if (e.RightButton == MouseButtonState.Pressed)
                {

                    ContextMenu contextMenu = new();
                    if (editor.selectedTiles.Count == 0)
                    {
                        SetMenuItems(contextMenu, GetContextMenu(MenuType.NoneSelected));
                    }
                    else if (editor.selectedTiles.Count == 1)
                    {
                        SetMenuItems(contextMenu, GetContextMenu(MenuType.OneObjectSelected));
                    }
                    else if (editor.selectedTiles.Count > 1)
                    {
                        SetMenuItems(contextMenu, GetContextMenu(MenuType.MultipleObjectSelected));
                    }

                    contextMenu.IsOpen = true;
                }



                if (editor.selectedTiles.Count == 1)
                {
                    groupboxProperty.IsEnabled = true;
                    mirrorXCheckbox.IsEnabled = true;
                    mirrorYCheckbox.IsEnabled = true;
                    sizeCheckbox.IsEnabled = true;
                    paletteTextbox.IsEnabled = true;
                    oamXTextbox.IsEnabled = true;
                    oamYTextbox.IsEnabled = true;
                    oamZTextbox.IsEnabled = true;
                    UpdateSelectedTileInfos();
                }
                else if (editor.selectedTiles.Count == 0)
                {
                    groupboxProperty.IsEnabled = false;
                }
                else
                {
                    groupboxProperty.IsEnabled = true;
                    mirrorXCheckbox.IsEnabled = false;
                    mirrorYCheckbox.IsEnabled = false;
                    paletteTextbox.IsEnabled = true;
                    sizeCheckbox.IsEnabled = false;
                    oamXTextbox.IsEnabled = false;
                    oamYTextbox.IsEnabled = false;
                    oamZTextbox.IsEnabled = false;
                    UpdateSelectedTileInfos();

                }



            }
        }


        private void MainScreen_MouseMove(object sender, MouseEventArgs e)
        {
            if (projectLoaded)
            {
                editor.MouseMove(e, cDPI, MainScreen_Image, MainScreenLayer_Image, SheetScreen_Image);
                if (editor.selectedTiles.Count == 1)
                {
                    if (editor.mouseDown)
                    {
                        groupboxProperty.IsEnabled = true;
                        UpdateSelectedTileInfos();
                    }
                }
            }
        }

        private void addAction()
        {
            /*OamTile[] array = new OamTile[editor.Frames[editor.SelectedFrame].Tiles.Count];
            for (int i = 0; i < array.Length; i++)
            {
                OamTile t = editor.Frames[editor.SelectedFrame].Tiles[i];
                array[i] = new OamTile(t.x, t.y, t.mirrorX, t.mirrorY, t.id, t.palette, t.size, t.priority);
                array[i].z = t.z;
            }

            actions.Add(new("Start - Nothing", editor.SelectedFrame, array));
            RefreshActions();*/
            }

            private void MainScreen_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (projectLoaded)
            {
                editor.MouseUp(e, cDPI, MainScreen_Image, MainScreenLayer_Image, SheetScreen_Image);

                /*if (editor.selectedTiles.Count == 1)
                {
                    groupboxProperty.IsEnabled = true;
                    UpdateSelectedTileInfos();
                }*/

                if (editor.selectedTiles.Count == 1)
                {
                    groupboxProperty.IsEnabled = true;
                    mirrorXCheckbox.IsEnabled = true;
                    mirrorYCheckbox.IsEnabled = true;
                    sizeCheckbox.IsEnabled = true;
                    paletteTextbox.IsEnabled = true;
                    oamXTextbox.IsEnabled = true;
                    oamYTextbox.IsEnabled = true;
                    oamZTextbox.IsEnabled = true;
                    UpdateSelectedTileInfos();
                }
                else if (editor.selectedTiles.Count == 0)
                {
                    groupboxProperty.IsEnabled = false;
                }
                else
                {
                    groupboxProperty.IsEnabled = true;
                    mirrorXCheckbox.IsEnabled = false;
                    mirrorYCheckbox.IsEnabled = false;
                    paletteTextbox.IsEnabled = true;
                    sizeCheckbox.IsEnabled = false;
                    oamXTextbox.IsEnabled = false;
                    oamYTextbox.IsEnabled = false;
                    oamZTextbox.IsEnabled = false;
                    UpdateSelectedTileInfos();

                }
            }
        }

        private bool FindAnyTilesChanges(OamTile[] history, List<OamTile> current)
        {

            foreach (OamTile t in history)
            {
                //Console.WriteLine(t.ToString() + "     " + current.Find(x => x.Equals(t)).ToString());

                bool a = (current.Find(x => x.Equals(t)).ToString() == t.ToString());

                if (a != null)
                {
                    return true;
                }
            }
            return false;

        }



        private void DarkTheme_Checked(object sender, RoutedEventArgs e)
        {
            ThemesController.SetTheme(ThemesController.ThemeTypes.Dark);
        }

        private void DarkTheme_Unchecked(object sender, RoutedEventArgs e)
        {
            ThemesController.SetTheme(ThemesController.ThemeTypes.Light);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (projectLoaded)
            {
                changedFromForm = true;
                sliderFramebox.Text = FrameSlider.Value.ToString();
                changedFromForm = false;
                
                //SliderLabel.Content = "Frame : " + FrameSlider.Value.ToString();
                editor.SelectedFrame = (byte)FrameSlider.Value;
                
                editor.selectedTiles.Clear();

                RefreshScreen();

            }
        }

        private void PlayAnimation_Click(object sender, RoutedEventArgs e)
        {
            animationPlaying = !animationPlaying;
            if (animationPlaying)
            {
                BtnImagePlay.Source = new BitmapImage(new Uri("Images/Pause.png", UriKind.Relative));
            }
            else
            {
                BtnImagePlay.Source = new BitmapImage(new Uri("Images/Play.png", UriKind.Relative));
            }
        }

        private void SettingMenu_Click(object sender, RoutedEventArgs e)
        {
            Settings settings = new();
            settings.previewLabelFont.FontFamily = codeEditor.FontFamily;
            settings.previewLabelFont.FontSize = codeEditor.FontSize;
            if (settings.ShowDialog() == true)
            {

                codeEditor.FontFamily = settings.previewLabelFont.FontFamily;
                codeEditor.FontSize = settings.previewLabelFont.FontSize;
                Properties.Settings.Default.FontFamily = codeEditor.FontFamily.ToString();
                Properties.Settings.Default.FontSize = codeEditor.FontSize;
                Properties.Settings.Default.Save();
            }

            if (projectLoaded)
            {
                UpdateAllImages(editor.GetPalettes());
            }

        }

        private void SheetTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (projectLoaded)
            {
                TextBox t = (TextBox)e.Source;

                if (t.Tag != null)
                {
                    string boxidstr = t.Tag.ToString();
                    int boxid = int.Parse(boxidstr);
                    _ = byte.TryParse(t.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out byte b);
                    if (b >= 103)
                    {
                        b = 102;
                    }
                    sheetValues[boxid] = b;

                    editor.UpdateSheets(SheetScreen_Image, SheetScreenOverlay_Image, sheetValues);
                    sheetScreen.Source = SheetScreen_Image.bitmap;

                    RefreshScreen();

                }

            }
        }

        private void SheetTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TextBox t = (TextBox)e.Source;
            if (t.Tag != null)
            {
                string boxidstr = t.Tag.ToString();
                int boxid = int.Parse(boxidstr);

                if (e.Delta > 0)
                {
                    if (sheetValues[boxid] < 103)
                    {
                        sheetValues[boxid]++;
                    }
                    t.Text = sheetValues[boxid].ToString("X2");
                }
                else if (e.Delta < 0)
                {
                    if (sheetValues[boxid] > 0)
                    {
                        sheetValues[boxid]--;
                    }
                    t.Text = sheetValues[boxid].ToString("X2");
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            gridSizeBox.Text = Properties.Settings.Default.gridSize.ToString();
            snapSizeBox.Text = Properties.Settings.Default.snapSize.ToString();
        }

        public void CreateMacros()
        {
            macrosListItems.Clear();
            string macrofile = File.ReadAllText("SpriteMakerEngine/Macros.asm");
            int pos = 0;
            while (pos <= macrofile.Length)
            {
                int descStart = macrofile.IndexOf(';', pos);
                int descEnd = macrofile.IndexOf("macro ", pos);
                pos = descEnd;
                if (pos == -1)
                {
                    break;
                }
                int macroNameStart = descEnd +6;
                int macroNameEnd = macrofile.IndexOf(')', pos);
                if (descStart == -1 || descEnd == -1 || macroNameStart == -1 || macroNameEnd == -1)
                {
                    break;
                }

                int macroEnd = macrofile.IndexOf("endmacro", pos);
                string desc = macrofile.Substring(descStart, (descEnd - descStart)-2);
                string name = macrofile.Substring(macroNameStart, (macroNameEnd - macroNameStart)+1);
                pos = macroEnd;
                ListBoxItem li = new ListBoxItem();
                li.Content = name;
                li.ToolTip = desc;
                macrosListItems.Add(li);
            }
            //macrosListItems
        }

        public void CreateJSL()
        {
            functionListItems.Clear();
            string jslfile = File.ReadAllText("SpriteMakerEngine/sprite_functions_hooks.asm");
            string[] allLines = jslfile.Split('\n');
            bool comment = true;
            string tmpComment = "";
            string tmpName = "";
            int tmpAddr = 0;
            foreach (string line in allLines)
            {
                

                if (comment)
                {
                    if (line.Contains(';'))
                    {
                        tmpComment += line.TrimStart(';');
                        continue;
                    }

                    if (line.Contains("org"))
                    {
                        string addrStr = line.Split(" $")[1];
                        tmpAddr = int.Parse(addrStr, NumberStyles.HexNumber);
                        comment = false;
                        continue;
                    }
                }
                else
                {
                    if (line.Contains(":"))
                    {
                        tmpName = line.Substring(0, line.IndexOf(":"));
                        // Write in a list comment, addr, label name
                        ListBoxItem li = new ListBoxItem();
                        li.Content = tmpName;
                        li.ToolTip = tmpComment;
                        functionListItems.Add(li);
                        comment = true;
                        tmpComment = "";
                        continue;
                    }
                }


            }
        }

        public void CreateProject(bool load = false)
        {

            if (File.Exists(Properties.Settings.Default.romPath))
            {

                FileStream fs = new(Properties.Settings.Default.romPath, FileMode.Open, FileAccess.Read);
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, (int)fs.Length);
                fs.Close();



                projectLoaded = true;



                editor = new Editor(data);
                spriteStates.Clear();
                actions.Clear();
                animations.Clear();

                //coreroutinesListbox.Items.Add("Add Routine");
                userRoutines.Clear();
                userRoutines.Add(new UserRoutine("Long Main", File.ReadAllText("TemplateLongMain.asm")));
                userRoutines.Add(new UserRoutine("Sprite Prep", File.ReadAllText("TemplatePrep.asm")));
                userRoutines.Add(new UserRoutine("Sprite Draw", File.ReadAllText("TemplateDraw.asm")));

                userRoutines.Add(new UserRoutine("Action00", "\r\n\r\n\r\nRTS"));
                UpdateUserRoutines();
                if (!load)
                {
                    userroutinesListbox.SelectedIndex = 0;
                }
                
                //UpdateStateListbox();


                //InitWordsList();
                //ReadSymbolsFile();


                editor.UpdateSheets(SheetScreen_Image, SheetScreenOverlay_Image, sheetValues);

                Color[] colors = editor.GetPalettes();
                for (int i = 0; i < 128; i++)
                {
                    PaletteScreen_Image[i] = (byte)i;
                }


                UpdateAllImages(colors);
                RefreshScreen();

            }
        }

        public void AddUserRoutine()
        {
            int id = 0;
            string s = "Action" + id.ToString("D2");
            while (userRoutines.Where(x => (x.name == s)).ToArray().Length>0)
            {
                id++;
                s = "Action" + id.ToString("D2");
            }
            userRoutines.Add(new UserRoutine(s, "\r\n\r\n\r\nRTS"));
            UpdateUserRoutines();
        }

        public void RemoveUserRoutine()
        {
            if (userroutinesListbox.SelectedItem != null)
            {
                userRoutines.RemoveAt(userroutinesListbox.SelectedIndex+3);
                UpdateUserRoutines();
            }
        }

        public void RenameUserRoutine()
        {
            if (userroutinesListbox.SelectedItem != null)
            {
                Rename renamewindow = new Rename();
                renamewindow.actionnameTextbox.Text = userRoutines[userroutinesListbox.SelectedIndex+3].name;
                if (renamewindow.ShowDialog() == true)
                {
                    userRoutines[userroutinesListbox.SelectedIndex+3].name = renamewindow.actionnameTextbox.Text;
                }

                UpdateUserRoutines();
            }
        }

        public void UpdateUserRoutines()
        {
            userroutinesListbox.Items.Clear();
            coreroutinesListbox.Items.Clear();
            for (int i = 0; i < 3; i++)
            {
                coreroutinesListbox.Items.Add(userRoutines[i].name);
            }
            for (int i = 3;i<userRoutines.Count;i++)
            {
                userroutinesListbox.Items.Add((i-3).ToString("D2") + " " + userRoutines[i].name);
            }
        }

        public void UpdateAllImages(Color[]? colors = null)
        {


            if (colors != null)
            {
                MainScreen_Image.UpdatePalette(colors);
                SheetScreen_Image.UpdatePalette(colors);
                SheetScreenOverlay_Image.UpdatePalette(colors);
                PaletteScreen_Image.UpdatePalette(colors);
                MainScreenLayer_Image.UpdatePalette(colors);
                MainScreenBack_Image.UpdatePalette(colors);
            }



            MainScreen_Image.UpdateBitmap();
            SheetScreen_Image.UpdateBitmap();
            PaletteScreen_Image.UpdateBitmap();
            MainScreenLayer_Image.UpdateBitmap();
            SheetScreenOverlay_Image.UpdateBitmap();


            MainScreen.Source = MainScreen_Image.bitmap;
            sheetScreen.Source = SheetScreen_Image.bitmap;
            PaletteScreen.Source = PaletteScreen_Image.bitmap;
            MainScreenLayer.Source = MainScreenLayer_Image.bitmap;
            sheetScreenOverlay.Source = SheetScreenOverlay_Image.bitmap;
            UpdateBackground();

        }

        public void UpdateBackground()
        {
            if (projectLoaded)
            {
                MainScreenBack_Image.ClearBitmap(240);
                if (Properties.Settings.Default.showBack)
                {
                    DrawHelper.DrawCheckerboard(MainScreenBack_Image);
                }
                if (Properties.Settings.Default.showGrid)
                {
                    DrawHelper.DrawGrid(MainScreenBack_Image, Properties.Settings.Default.gridSize);
                }
                if (Properties.Settings.Default.showCenter)
                {
                    DrawHelper.DrawCenter(MainScreenBack_Image);
                }
                if (Properties.Settings.Default.showHitbox)
                {
                    DrawHitbox();
                }
                MainScreenBack_Image.UpdatePalette(editor.GetPalettes());
                MainScreenBack_Image.UpdateBitmap();
                MainScreenBack.Source = MainScreenBack_Image.bitmap;
            }
        }

        private void HeaderButtonToggled_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (projectLoaded)
            {
                UpdateAllImages(editor.GetPalettes());
                RefreshScreen();
            }
        }

        private void GridSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (projectLoaded)
            {
                _ = int.TryParse((e.Source as TextBox).Text, out int size);

                if (size == 0) { size = 1; }

                Properties.Settings.Default.gridSize = (byte)size;
                Properties.Settings.Default.Save();
                MainScreenBack_Image.ClearBitmap(240);
                UpdateBackground();


            }
        }

        private void SnapSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (projectLoaded)
            {
                _ = int.TryParse((e.Source as TextBox).Text, out int size);
                if (size == 0) { size = 1; }

                Properties.Settings.Default.snapSize = (byte)size;
                Properties.Settings.Default.Save();


            }
        }

        private void RefreshScreen()
        {
            MainScreenLayer_Image.ClearBitmap(0);
            MainScreen_Image.ClearBitmap(0);
            editor.Draw(MainScreen_Image, SheetScreen_Image);
            editor.DrawPrevious(MainScreenLayer_Image, SheetScreen_Image);
            MainScreenLayer_Image.UpdateBitmap();
            MainScreen_Image.UpdateBitmap();
            if (!Properties.Settings.Default.showOnion)
            {
                MainScreenLayer.Opacity = 0;
            }
            else
            {
                MainScreenLayer.Opacity = 0.40f;
            }

        }

        private void RefreshActions()
        {
            actionListbox.Items.Clear();
            foreach (Action a in actions)
            {
                actionListbox.Items.Add(a.actionName);
            }
        }

        private void RefreshAnimations()
        {
            animationgroupListbox.Items.Clear();
            foreach (AnimationGroup a in animations)
            {
                animationgroupListbox.Items.Add(a.ToString());
            }
        }

        private void MirrorXCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (!changedFromForm)
            {
                if (editor.selectedTiles.Count == 1)
                {
                    editor.selectedTiles[0].mirrorX = mirrorXCheckbox.IsChecked ?? false;
                    RefreshScreen();
                }
            }
        }

        private void MirrorYCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (!changedFromForm)
            {
                if (editor.selectedTiles.Count == 1)
                {
                    editor.selectedTiles[0].mirrorY = mirrorYCheckbox.IsChecked ?? false;
                    RefreshScreen();
                }
            }

        }

        private void SizeTileCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (!changedFromForm)
            {
                if (editor.selectedTiles.Count == 1)
                {
                    editor.selectedTiles[0].size = sizeCheckbox.IsChecked ?? false;
                    RefreshScreen();
                }
            }

        }

        private void UpdateSelectedTileInfos()
        {
            changedFromForm = true;
            tileIdLabel.Content = "Tile ID " + editor.selectedTiles[0].id.ToString("X2");
            mirrorXCheckbox.IsChecked = editor.selectedTiles[0].mirrorX;
            mirrorYCheckbox.IsChecked = editor.selectedTiles[0].mirrorY;
            sizeCheckbox.IsChecked = editor.selectedTiles[0].size;
            paletteTextbox.Text = editor.selectedTiles[0].palette.ToString();
            oamXTextbox.Text = editor.selectedTiles[0].x.ToString();
            oamYTextbox.Text = editor.selectedTiles[0].y.ToString();
            oamZTextbox.Text = editor.selectedTiles[0].z.ToString();
            changedFromForm = false;
        }

        private void OamPosTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (projectLoaded)
            {
                if (!changedFromForm)
                {
                    if (editor.selectedTiles.Count == 1)
                    {
                        _ = byte.TryParse(oamXTextbox.Text, out byte x);
                        _ = byte.TryParse(oamYTextbox.Text, out byte y);
                        _ = byte.TryParse(oamZTextbox.Text, out byte z);


                        editor.selectedTiles[0].x = x;
                        editor.selectedTiles[0].y = y;
                        editor.selectedTiles[0].z = z;
                        RefreshScreen();
                    }
                }
            }
        }



        private void OamPosTextbox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (projectLoaded)
            {
                if (editor.selectedTiles.Count == 1)
                {
                    _ = byte.TryParse(oamXTextbox.Text, out byte x);
                    _ = byte.TryParse(oamYTextbox.Text, out byte y);
                    _ = byte.TryParse(oamZTextbox.Text, out byte z);

                    if (e.Delta > 0) //+
                    {
                        if (e.Source == oamXTextbox)
                        {
                            if (x < 255)
                            {
                                x++;
                            }

                        }
                        else if (e.Source == oamYTextbox)
                        {
                            if (x < 224)
                            {
                                y++;
                            }
                        }
                        else
                        {
                            if (z < 64)
                            {
                                z++;
                            }
                        }
                    }
                    else if (e.Delta < 0) //-
                    {
                        if (e.Source == oamXTextbox)
                        {
                            if (x > 0)
                            {
                                x--;
                            }
                        }
                        else if (e.Source == oamYTextbox)
                        {
                            if (y > 0)
                            {
                                y--;
                            }
                        }
                        else
                        {
                            if (z > 0)
                            {
                                z--;
                            }
                        }
                    }
                    oamXTextbox.Text = x.ToString();
                    oamYTextbox.Text = y.ToString();
                    oamZTextbox.Text = z.ToString();
                }
            }
        }

        private void PaletteTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (projectLoaded)
            {
                if (!changedFromForm)
                {
                    if (editor.selectedTiles.Count >= 1)
                    {
                        _ = byte.TryParse(paletteTextbox.Text, out byte pal);
                        for (int i = 0; i < editor.selectedTiles.Count; i++)
                        {
                            editor.selectedTiles[i].palette = pal;
                        }

                        RefreshScreen();
                    }
                }
            }
        }

        private void PaletteTextbox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (projectLoaded)
            {
                if (editor.selectedTiles.Count >= 1)
                {
                    _ = byte.TryParse(paletteTextbox.Text, out byte pal);


                    if (e.Delta > 0) //+
                    {
                        if (pal < 7) { pal++; }
                    }
                    else if (e.Delta < 0) //-
                    {
                        if (pal > 0) { pal--; }
                    }

                    for (int i = 0; i < editor.selectedTiles.Count; i++)
                    {
                        editor.selectedTiles[i].palette = pal;
                    }

                    paletteTextbox.Text = pal.ToString();
                }

            }
        }

        private void AnimSpeedTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _ = byte.TryParse(animSpeedTextbox.Text, out byte speed);
            if (speed > 0)
            {
                timerWait = speed;
            }
            else
            {
                timerWait = 8;
            }
        }


        private void AddAnimButton_Click(object sender, RoutedEventArgs e)
        {
            _ = byte.TryParse(animminTextbox.Text, out byte start);
            _ = byte.TryParse(animmaxTextbox.Text, out byte end);
            _ = byte.TryParse(animSpeedTextbox.Text, out byte speed);
            string name = animnameTextBox.Text;
            if (name == "")
            {
                name = "Animation" + animations.Count.ToString();
            }

            animations.Add(new AnimationGroup(start, end, speed, name));

            RefreshAnimations();
        }

        private void UpdateAnimButton_Click(object sender, RoutedEventArgs e)
        {

            if (animationgroupListbox.SelectedIndex != -1)
            {
                _ = byte.TryParse(animminTextbox.Text, out byte start);
                _ = byte.TryParse(animmaxTextbox.Text, out byte end);
                _ = byte.TryParse(animSpeedTextbox.Text, out byte speed);
                string name = animnameTextBox.Text;
                if (name == "")
                {
                    name = "Animation" + animations.Count.ToString();
                }

                animations[animationgroupListbox.SelectedIndex] = new AnimationGroup(start, end, speed, name);

                RefreshAnimations();
            }
        }

        private void DelAnimButton_Click(object sender, RoutedEventArgs e)
        {
            if (animationgroupListbox.SelectedIndex != -1)
            {
                animations.RemoveAt(animationgroupListbox.SelectedIndex);
                RefreshAnimations();
            }
        }


        private void AnimationgroupListbox_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void AnimationgroupListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (animationgroupListbox.SelectedIndex >= 0)
            {
                if (!changedFromForm)
                {

                    animminTextbox.Text = animations[animationgroupListbox.SelectedIndex].FrameStart.ToString();
                    animmaxTextbox.Text = animations[animationgroupListbox.SelectedIndex].FrameEnd.ToString();
                    animSpeedTextbox.Text = animations[animationgroupListbox.SelectedIndex].FrameSpeed.ToString();
                    animnameTextBox.Text = animations[animationgroupListbox.SelectedIndex].FrameName.ToString();

                    changedFromForm = true;
                    int index = animationgroupListbox.SelectedIndex;
                    RefreshAnimations();
                    animationgroupListbox.SelectedIndex = index;
                    changedFromForm = false;
                }

            }
        }

        private void SheetScreen_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (projectLoaded)
            {
                int mouse_x = (int)(e.GetPosition((IInputElement)e.Source).X / sheetDPI);
                int mouse_y = (int)(e.GetPosition((IInputElement)e.Source).Y / sheetDPI);

                int tileX = (mouse_x / 8);
                int tileY = (mouse_y / 8);

                ushort t = (ushort)(tileX + (tileY * 16));
                editor.SelectedTile = t;

                editor.selectedTiles.Clear();

                editor.UpdateSheets(SheetScreen_Image, SheetScreenOverlay_Image, sheetValues);

                RefreshScreen();
            }
        }

        private void PaletteScreen_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int mouse_y = (int)(e.GetPosition((IInputElement)e.Source).Y / 16);
            editor.SelectedPalette = (byte)mouse_y;

            editor.UpdatePalette(SheetScreen_Image);
            sheetScreen.Source = SheetScreen_Image.bitmap;
        }

        private void Zoom100_Click(object sender, RoutedEventArgs e)
        {
            cDPI = 1;
            sheetDPI = 1;
            UpdateScreenSize();
        }

        private void Zoom200_Click(object sender, RoutedEventArgs e)
        {
            cDPI = 2;
            sheetDPI = 2;
            UpdateScreenSize();
        }

        private void Zoom300_Click(object sender, RoutedEventArgs e)
        {
            cDPI = 3;
            sheetDPI = 3;
            UpdateScreenSize();
        }
        private void Zoom400_Click(object sender, RoutedEventArgs e)
        {
            cDPI = 4;
            sheetDPI = 4;
            UpdateScreenSize();
        }

        private void LogicViewer_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DrawHitbox()
        {
            if (projectLoaded)
            {
                byte.TryParse(property_hitbox.Text,NumberStyles.HexNumber,CultureInfo.CurrentCulture, out byte hitbox);

                byte hitbox_x = editor.ROM[0x3772F + hitbox];
                byte hitbox_xhigh = editor.ROM[0x3774F + hitbox];

                byte hitbox_y = editor.ROM[0x3778F + hitbox];
                byte hitbox_yhigh = editor.ROM[0x377AF + hitbox];

                int hitbox_width = editor.ROM[0x3776F + hitbox];
                int hitbox_height = editor.ROM[0x377CF + hitbox];

                short hitX = 0;
                short hitY = 0;

                hitX = (short)(hitbox_x + (hitbox_xhigh << 8));
                hitY = (short)(hitbox_y + (hitbox_yhigh << 8));

                MainScreenBack_Image.WriteBitmapRect(128 + hitX, 112 + hitY, hitbox_width, hitbox_height, 245);
            }

        }



        private void UpdateScreenSize()
        {
            mainStackPanel.Width = (512);

            MainScreen_Image.DPI = cDPI;
            MainScreenBack_Image.DPI = cDPI;
            MainScreenLayer_Image.DPI = cDPI;
            SheetScreen_Image.DPI = sheetDPI;

            sheetPanel.Width = 128 * (sheetDPI);
            sheetScreen.Width = 128 * (sheetDPI);
            sheetScreen.Height = 256 * (sheetDPI);

            sheetScreenOverlay.Width = 128 * (sheetDPI);
            sheetScreenOverlay.Height = 256 * (sheetDPI);

            gridMainScreen.Width = (256 * cDPI);
            gridMainScreen.Height = (224 * cDPI);

            MainScreenLayer.Width = (256 * cDPI);
            MainScreenLayer.Height = (224 * cDPI);
            MainScreenBack.Width = (256 * cDPI);
            MainScreenBack.Height = (224 * cDPI);

            MainScreen.Width = (256 * cDPI);
            MainScreen.Height = (224 * cDPI);
            UpdateAllImages();



            if ((256 * cDPI) > 512)
            {
                mainStackPanel.Width = (256 * cDPI);
            }
        }



        private void ButtonPlaytest_Click(object sender, RoutedEventArgs e)
        {
            // use Emulator path if they are empty ask for the user to fill them
            if (!File.Exists(Properties.Settings.Default.emulatorPath))
            {
                MessageBox.Show("Emulator location is unexisting, you can set the location in Preferences/Settings menu");
                return;
            }

            string asm = GetASM();

            FileStream fs = new(Properties.Settings.Default.romPath, FileMode.Open, FileAccess.Read);
            byte[] romData = new byte[fs.Length];
            fs.Read(romData, 0, (int)fs.Length);
            fs.Close();

            int roomPtrLocation = 0x0F8000; // 3 bytes ptrs
            int roomSprPtrlocation = 0x4D62E; //2 bytes ptrs


            byte b = romData[roomPtrLocation + (260 * 3) + 2];
            byte h = romData[roomPtrLocation + (260 * 3) + 1];
            byte l = romData[roomPtrLocation + (260 * 3) + 0];
            int roompos = ((b << 16) | (h << 8) | l);

            int sprPos = ((09 << 16) | (romData[roomSprPtrlocation + (260 * 2) + 1] << 8) | romData[roomSprPtrlocation + (260 * 2)]);


            int headerptr = (romData[0xB5DD + 2] << 16) | (romData[0xB5DD + 1] << 8) | (romData[0xB5DD + 0]);
            int headerlinkhouse = (romData[0xB5DD + 2] << 16 | romData[headerptr.SNEStoPC() + (260 * 2) + 1] << 8 | romData[headerptr.SNEStoPC() + (260 * 2)]);


            StringBuilder sb = new StringBuilder();

            sb.AppendLine("org $07F13C");

            sb.AppendLine("Init_Player:");


            sb.AppendLine("org $02816C; Override JSL Init_Player in Bank02: 239");
            sb.AppendLine("JSL DebugCode");


            sb.AppendLine("org $3A8000");
            sb.AppendLine("DebugCode:");
            sb.AppendLine("LDA #$03 : STA $7EF3C5 ; Set gamestate on 03");
            sb.AppendLine("LDA #$FF : STA $7EF3C6 ; Set uncle saved");
            sb.AppendLine("LDA #$01 : STA $7EF359 ; Give the sword");
            sb.AppendLine("LDA #$00 : STA $7EF3CC ; Remove Zelda tagalong");
            sb.AppendLine("JSL Init_Player");
            sb.AppendLine("RTL");

            sb.AppendLine("org $" + roompos.ToString("X6"));
            sb.AppendLine("dw $00E3, $FFFF, $FFFF, $FFF0, $FFFF, $FFFF");

            sb.AppendLine("org $00DC57");
            sb.AppendLine("db $" + blockset1.Text + ", $" + blockset2.Text + ", $" + blockset3.Text + ", $" + blockset4.Text);

            sb.AppendLine("org $" + sprPos.ToString("X6"));
            sb.AppendLine("db $00, $1A, $1A, $"+property_sprid.Text+", $FF");

            sb.AppendLine("org $" + (headerlinkhouse+3).ToString("X6"));
            sb.AppendLine("db $00");

            sb.AppendLine("; Skip intro");
            sb.AppendLine("org $0CC36C");
            sb.AppendLine("JMP $C2F0");

            sb.AppendLine("incsrc Macros.asm");

            sb.AppendLine("incsrc sprite_functions_hooks.asm");

            sb.AppendLine("org $388000; Might need to change that position");
            sb.AppendLine("incsrc sprite_new_table.asm");

            sb.AppendLine("org $398000; Might need to change that position");
            sb.AppendLine("incsrc sprite_new_functions.asm");

            sb.AppendLine("");

            sb.Append(asm);

            File.WriteAllText("SpriteMakerEngine/playtest.asm", sb.ToString());

            AsarCLR.Asar.init();

            AsarCLR.Asar.patch("SpriteMakerEngine/playtest.asm", ref romData ,new string[1] { "SpriteMakerEngine/" });

            AsarCLR.Asarerror[] errors = AsarCLR.Asar.geterrors();

            if (errors.Length >= 1)
            {
                int line = errors[0].Line;
                int action = 0;
                for (int i = routinesStartLine.Length-1; i >= 0;i--)
                {
                    if (errors[0].Line >= routinesStartLine[i])
                    {
                        line = (errors[0].Line - routinesStartLine[i])-2;
                        action = i;
                        break;
                    }
                }
                errorLabel.Text = "Error " + errors[0].Rawerrdata + " \"" + errors[0].Block + "\"  at line " + line + " of action " + action.ToString("D2");
                return;

            }
            else
            {

                errorLabel.Text = "";

                FileStream fso = new FileStream("temp.sfc", FileMode.OpenOrCreate, FileAccess.Write);
                fso.Write(romData, 0, romData.Length);
                fso.Close();

                Process.Start(Properties.Settings.Default.emulatorPath, "temp.sfc");
            }
            




        }

        int[] routinesStartLine;

        private static int CountLines(string str)
        {
            if (str == null)
                throw new ArgumentNullException("str");
            if (str == string.Empty)
                return 0;
            int index = -1;
            int count = 0;
            while (-1 != (index = str.IndexOf(Environment.NewLine, index + 1)))
                count++;

            return count + 1;
        }

        public string GetASM()
        {

            if (previousCode != -1)
            {
                userRoutines[previousCode].code = codeEditor.Text;
                //save that code
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(";==============================================================================");
            sb.AppendLine("; Sprite Properties");
            sb.AppendLine(";==============================================================================");
            sb.AppendLine("!SPRID              = $" + property_sprid.Text + "; The sprite ID you are overwriting (HEX)");
            sb.AppendLine("!NbrTiles           = " + property_oamnbr.Text + " ; Number of tiles used in a frame");
            sb.AppendLine("!Harmless           = " + ((bool)property_harmless.IsChecked ? 1 : 0).ToString("D2") + "  ; 00 = Sprite is Harmful,  01 = Sprite is Harmless");
            sb.AppendLine("!HVelocity          = " + ((bool)property_fast.IsChecked ? 1 : 0).ToString("D2") + "  ; Is your sprite going super fast? put 01 if it is");
            sb.AppendLine("!Health             = " + property_health.Text + "  ; Number of Health the sprite have");
            sb.AppendLine("!Damage             = " + property_damage.Text + "  ; (08 is a whole heart), 04 is half heart");
            sb.AppendLine("!DeathAnimation     = " + ((bool)property_customdeath.IsChecked ? 1 : 0).ToString("D2") + "  ; 00 = normal death, 01 = no death animation");
            sb.AppendLine("!ImperviousAll      = " + ((bool)property_impervious.IsChecked ? 1 : 0).ToString("D2") + "  ; 00 = Can be attack, 01 = attack will clink on it");
            sb.AppendLine("!SmallShadow        = " + ((bool)property_smallshadow.IsChecked ? 1 : 0).ToString("D2") + "  ; 01 = small shadow, 00 = no shadow");
            sb.AppendLine("!Shadow             = " + ((bool)property_shadow.IsChecked ? 1 : 0).ToString("D2") + "  ; 00 = don't draw shadow, 01 = draw a shadow ");
            sb.AppendLine("!Palette            = " + property_palette.Text + "  ; Unused in this template (can be 0 to 7)");
            sb.AppendLine("!Hitbox             = " + property_hitbox.Text + "  ; 00 to 31, can be viewed in sprite draw tool");
            sb.AppendLine("!Persist            = " + ((bool)property_persist.IsChecked ? 1 : 0).ToString("D2") + "  ; 01 = your sprite continue to live offscreen");
            sb.AppendLine("!Statis             = " + ((bool)property_statis.IsChecked ? 1 : 0).ToString("D2") + "  ; 00 = is sprite is alive?, (kill all enemies room)");
            sb.AppendLine("!CollisionLayer     = " + ((bool)property_collisionlayer.IsChecked ? 1 : 0).ToString("D2") + "  ; 01 = will check both layer for collision");
            sb.AppendLine("!CanFall            = " + ((bool)property_canfall.IsChecked ? 1 : 0).ToString("D2") + "  ; 01 sprite can fall in hole, 01 = can't fall");
            sb.AppendLine("!DeflectArrow       = " + ((bool)property_deflectarrows.IsChecked ? 1 : 0).ToString("D2") + "  ; 01 = deflect arrows");
            sb.AppendLine("!WaterSprite        = " + ((bool)property_watersprite.IsChecked ? 1 : 0).ToString("D2") + "  ; 01 = can only walk shallow water");
            sb.AppendLine("!Blockable          = " + ((bool)property_blockable.IsChecked ? 1 : 0).ToString("D2") + "  ; 01 = can be blocked by link's shield?");
            sb.AppendLine("!Prize              = " + property_prize.Text + "  ; 00-15 = the prize pack the sprite will drop from");
            sb.AppendLine("!Sound              = " + ((bool)property_damagesound.IsChecked ? 1 : 0).ToString("D2") + "  ; 01 = Play different sound when taking damage");
            sb.AppendLine("!Interaction        = " + ((bool)property_interaction.IsChecked ? 1 : 0).ToString("D2") + "  ; ?? No documentation");
            sb.AppendLine("!Statue             = " + ((bool)property_statue.IsChecked ? 1 : 0).ToString("D2") + "  ; 01 = Sprite is statue");
            sb.AppendLine("!DeflectProjectiles = " + ((bool)property_deflectprojectiles.IsChecked ? 1 : 0).ToString("D2") + "  ; 01 = Sprite will deflect ALL projectiles");
            sb.AppendLine("!ImperviousArrow    = " + ((bool)property_imperviousarrow.IsChecked ? 1 : 0).ToString("D2") + "  ; 01 = Impervious to arrows");
            sb.AppendLine("!ImpervSwordHammer  = " + ((bool)property_imperviousmelee.IsChecked ? 1 : 0).ToString("D2") + "  ; 01 = Impervious to sword and hammer attacks");
            sb.AppendLine("!Boss               = " + ((bool)property_isboss.IsChecked ? 1 : 0).ToString("D2") + "  ; 00 = normal sprite, 01 = sprite is a boss");
            sb.AppendLine("%Set_Sprite_Properties(Sprite_" + sprName + "_Prep, Sprite_" + sprName + "_Long);");


            sb.AppendLine(";==================================================================================================");
            sb.AppendLine("; Sprite Long Hook for that sprite");
            sb.AppendLine("; --------------------------------------------------------------------------------------------------");
            sb.AppendLine("; This code can be left unchanged");
            sb.AppendLine("; handle the draw code and if the sprite is active and should move or not");
            sb.AppendLine(";==================================================================================================");
            sb.AppendLine("Sprite_" + sprName + "_Long:");

            sb.Append(userRoutines[0].code.Replace("Template", sprName));

            sb.AppendLine("");


            sb.AppendLine(";==================================================================================================");
            sb.AppendLine("; Sprite initialization");
            sb.AppendLine("; --------------------------------------------------------------------------------------------------");
            sb.AppendLine("; this code only get called once perfect to initialize sprites substate or timers");
            sb.AppendLine("; this code as soon as the room transitions/ overworld transition occurs");
            sb.AppendLine(";==================================================================================================");
            sb.AppendLine("Sprite_" + sprName + "_Prep:");
            sb.Append(userRoutines[1].code.Replace("Template", sprName));


            sb.AppendLine("");

            //generated code here don't show that code
            //Sprite_Template_Main:



            sb.AppendLine(";==================================================================================================");
            sb.AppendLine("; Sprite Main routines code");
            sb.AppendLine("; --------------------------------------------------------------------------------------------------");
            sb.AppendLine("; This is the main local code of your sprite");
            sb.AppendLine("; This contains all the Subroutines of your sprites you can add more below");
            sb.AppendLine(";==================================================================================================");

            sb.AppendLine("Sprite_" + sprName + "_Main:");
            sb.AppendLine("LDA.w SprAction, X; Load the SprAction");
            sb.AppendLine("JSL UseImplicitRegIndexedLocalJumpTable; Goto the SprAction we are currently in");

            //TODO: FOR EACH ACTION IN userroutinesList write newline dw <actionname>
            for(int i =3;i< userRoutines.Count;i++)
            {
                sb.AppendLine("dw " + userRoutines[i].name);
            }

            sb.AppendLine("");
            sb.AppendLine("");



            routinesStartLine = new int[userRoutines.Count - 3];
            for (int i = 3; i < userRoutines.Count; i++)
            {
                sb.AppendLine(userRoutines[i].name + ":");
                routinesStartLine[i - 3] = 26 + CountLines(sb.ToString());
                sb.Append(userRoutines[i].code);
                sb.AppendLine("");
                sb.AppendLine("");
            }

            sb.AppendLine("");

            sb.AppendLine(";==================================================================================================");
            sb.AppendLine("; Sprite Draw code");
            sb.AppendLine("; --------------------------------------------------------------------------------------------------");
            sb.AppendLine("; Draw the tiles on screen with the data provided by the sprite maker editor");
            sb.AppendLine(";==================================================================================================");
            sb.AppendLine("Sprite_" + sprName + "_Draw:");


            sb.Append(userRoutines[2].code.Replace("Template", sprName));

            sb.AppendLine("");

            int allTiles = 0;

            sb.AppendLine(";==================================================================================================");
            sb.AppendLine("; Sprite Draw Generated Data");
            sb.AppendLine("; --------------------------------------------------------------------------------------------------");
            sb.AppendLine("; This is where the generated Data for the sprite go");
            sb.AppendLine(";==================================================================================================");


            StringBuilder sbIndex = new StringBuilder();
            StringBuilder sbNbr = new StringBuilder();
            StringBuilder sbX = new StringBuilder();
            StringBuilder sbY = new StringBuilder();
            StringBuilder sbCHR = new StringBuilder();
            StringBuilder sbP = new StringBuilder();
            StringBuilder sbS = new StringBuilder();
            sbIndex.AppendLine(".start_index");
            sbNbr.AppendLine(".nbr_of_tiles");
            sbIndex.Append("db");
            sbNbr.Append("db");
            for (int i = 0; i < 48; i++)
            {
                if (editor.Frames[i].Tiles.Count != 0)
                {

                    sbIndex.Append(" $" + allTiles.ToString("X2") + ",");
                    sbNbr.Append(" " + (editor.Frames[i].Tiles.Count - 1).ToString() + ",");

                    allTiles += (editor.Frames[i].Tiles.Count);
                }
            }

            sbX.Append(".x_offsets");
            sbY.Append(".y_offsets");
            sbCHR.Append(".chr");
            sbP.Append(".properties");
            sbS.Append(".sizes");

            for (int i = 0; i < 48; i++)
            {

                if (editor.Frames[i].Tiles.Count != 0)
                {
                    sbX.Append("\r\ndw");
                    sbY.Append("\r\ndw");
                    sbCHR.Append("\r\ndb");
                    sbP.Append("\r\ndb");
                    sbS.Append("\r\ndb");

                    for (int j = 0; j < editor.Frames[i].Tiles.Count; j++)
                    {
                        byte id = (byte)(editor.Frames[i].Tiles[j].id);
                        byte info = 0x30;
                        if (editor.Frames[i].Tiles[j].mirrorY)
                        {
                            info += 0x80;
                        }
                        if (editor.Frames[i].Tiles[j].mirrorX)
                        {
                            info += 0x40;
                        }
                        byte p = (byte)((editor.Frames[i].Tiles[j].palette & 0x07) << 1);
                        info += p;
                        if (editor.Frames[i].Tiles[j].id >= 256)
                        {
                            info += 0x01;
                        }
                        byte size = 0;
                        if (editor.Frames[i].Tiles[j].size)
                        {
                            size = 2;
                        }

                        int x = (editor.Frames[i].Tiles[j].x - 128);
                        int y = (editor.Frames[i].Tiles[j].y - 112);

                        if (j == editor.Frames[i].Tiles.Count - 1)
                        {
                            sbX.Append(" " + x.ToString());
                            sbY.Append(" " + y.ToString());
                            sbCHR.Append(" $" + id.ToString("X2"));
                            sbP.Append(" $" + info.ToString("X2"));
                            sbS.Append(" $" + size.ToString("X2"));
                        }
                        else
                        {
                            sbX.Append(" " + x.ToString() + ",");
                            sbY.Append(" " + y.ToString() + ",");
                            sbCHR.Append(" $" + id.ToString("X2") + ",");
                            sbP.Append(" $" + info.ToString("X2") + ",");
                            sbS.Append(" $" + size.ToString("X2") + ",");
                        }



                    }


                }
            }




            sb.AppendLine(sbIndex.ToString().Trim(','));
            sb.AppendLine(sbNbr.ToString().Trim(','));
            sb.AppendLine(sbX.ToString());
            sb.AppendLine(sbY.ToString());
            sb.AppendLine(sbCHR.ToString());
            sb.AppendLine(sbP.ToString());
            sb.AppendLine(sbS.ToString());

            return sb.ToString();
        }

        
        private void ExportAsm_Click(object sender, RoutedEventArgs e)
        {
            
            SaveFileDialog sfd = new SaveFileDialog() {DefaultExt = ".asm" };
            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, GetASM());
            }
        }

        private void property_sprid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (byte.TryParse(property_sprid.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out byte boxtxt))
            {
                if (e.Delta > 0)
                {
                    if (boxtxt < 255)
                    {
                        boxtxt++;
                    }
                    property_sprid.Text = boxtxt.ToString("X2");
                }
                else if (e.Delta < 0)
                {
                    if (boxtxt > 0)
                    {
                        boxtxt--;
                    }
                    property_sprid.Text = boxtxt.ToString("X2");
                }
            }


        }

        private void property_sprid_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (projectLoaded)
            {
                if (byte.TryParse(property_sprid.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out byte b))
                {
                    sprnameText.Content = spr_names[b];
                }
                else
                {
                    sprnameText.Content = spr_names[0];
                }
            }
        }


        static public string[] spr_names = new string[] {
            "00 Raven",
            "01 Vulture",
            "02 Flying Stalfos Head",
            "03 No Pointer (Empty)",
            "04 Pull Switch (good)",
            "05 Pull Switch (unused)",
            "06 Pull Switch (bad)",
            "07 Pull Switch (unused)",
            "08 Octorock (one way)",
            "09 Moldorm (Boss)",
            "0A Octorock (four way)",
            "0B Chicken",
            "0C Octorock (?)",
            "0D Buzzblock",
            "0E Snapdragon",
            "0F Octoballoon",
            "10 Octoballon Hatchlings",
            "11 Hinox",
            "12 Moblin",
            "13 Mini Helmasaure",
            "14 Gargoyle's Domain Gate",
            "15 Antifairy",
            "16 Sahasrahla / Aginah",
            "17 Bush Hoarder",
            "18 Mini Moldorm",
            "19 Poe",
            "1A Dwarves",
            "1B Arrow in wall",
            "1C Statue",
            "1D Weathervane",
            "1E Crystal Switch",
            "1F Bug-Catching Kid",
            "20 Sluggula",
            "21 Push Switch",
            "22 Ropa",
            "23 Red Bari",
            "24 Blue Bari",
            "25 Talking Tree",
            "26 Hardhat Beetle",
            "27 Deadrock",
            "28 Storytellers",
            "29 Blind Hideout attendant",
            "2A Sweeping Lady",
            "2B Storytellers",
            "2C Lumberjacks",
            "2D Telepathic Stones",
            "2E Multipurpose Sprite",
            "2F Race Npc",
            "30 Person?",
            "31 Fortune Teller",
            "32 Angry Brothers",
            "33 Pull for items",
            "34 Scared Girl",
            "35 Innkeeper",
            "36 Witch",
            "37 Waterfall",
            "38 Arrow Target",
            "39 Average Middle",
            "3A Half Magic Bat",
            "3B Dash Item",
            "3C Village Kid",
            "3D Signs? Chicken lady also showed up / Scared ladies outside houses.",
            "3E Rock Hoarder",
            "3F Tutorial Soldier",
            "40 Lightning Lock",
            "41 Blue Sword Soldier / Used by guards to detect player",
            "42 Green Sword Soldier",
            "43 Red Spear Soldier",
            "44 Assault Sword Soldier",
            "45 Green Spear Soldier",
            "46 Blue Archer",
            "47 Green Archer",
            "48 Red Javelin Soldier",
            "49 Red Javelin Soldier 2",
            "4A Red Bomb Soldiers",
            "4B Green Soldier Recruits",
            "4C Geldman",
            "4D Rabbit",
            "4E Popo",
            "4F Popo 2",
            "50 Cannon Balls",
            "51 Armos",
            "52 Giant Zora",
            "53 Armos Knights (Boss",
            "54 Lanmolas (Boss",
            "55 Fireball Zora",
            "56 Walking Zora",
            "57 Desert Palace Barriers",
            "58 Crab",
            "59 Bird",
            "5A Squirrel",
            "5B Spark (Left to Right",
            "5C Spark (Right to Left",
            "5D Roller (vertical moving",
            "5E Roller (vertical moving",
            "5F Roller",
            "60 Roller (horizontal moving",
            "61 Beamos",
            "62 Master Sword",
            "63 Devalant (Non",
            "64 Devalant (Shooter",
            "65 Shooting Gallery Proprietor",
            "66 Moving Cannon Ball Shooters (Right",
            "67 Moving Cannon Ball Shooters (Left",
            "68 Moving Cannon Ball Shooters (Down",
            "69 Moving Cannon Ball Shooters (Up",
            "6A Ball N' Chain Trooper",
            "6B Cannon Soldier",
            "6C Mirror Portal",
            "6D Rat",
            "6E Rope",
            "6F Keese",
            "70 Helmasaur King Fireball",
            "71 Leever",
            "72 Activator for the ponds (where you throw in items",
            "73 Uncle / Priest",
            "74 Running Man",
            "75 Bottle Salesman",
            "76 Princess Zelda",
            "77 Antifairy (Alternate",
            "78 Village Elder",
            "79 Bee",
            "7A Agahnim",
            "7B Agahnim Energy Ball",
            "7C Hyu",
            "7D Big Spike Trap",
            "7E Guruguru Bar (Clockwise",
            "7F Guruguru Bar (Counter Clockwise",
            "80 Winder",
            "81 Water Tektite",
            "82 Antifairy Circle",
            "83 Green Eyegore",
            "84 Red Eyegore",
            "85 Yellow Stalfos",
            "86 Kodongos",
            "87 Flames",
            "88 Mothula (Boss",
            "89 Mothula's Beam",
            "8A Spike Trap",
            "8B Gibdo",
            "8C Arrghus (Boss",
            "8D Arrghus spawn",
            "8E Terrorpin",
            "8F Slime",
            "90 Wallmaster",
            "91 Stalfos Knight",
            "92 Helmasaur King",
            "93 Bumper",
            "94 Swimmers",
            "95 Eye Laser (Right",
            "96 Eye Laser (Left",
            "97 Eye Laser (Down",
            "98 Eye Laser (Up",
            "99 Pengator",
            "9A Kyameron",
            "9B Wizzrobe",
            "9C Tadpoles",
            "9D Tadpoles",
            "9E Ostrich (Haunted Grove",
            "9F Flute",
            "A0 Birds (Haunted Grove",
            "A1 Freezor",
            "A2 Kholdstare (Boss",
            "A3 Kholdstare's Shell",
            "A4 Falling Ice",
            "A5 Zazak Fireball",
            "A6 Red Zazak",
            "A7 Stalfos",
            "A8 Bomber Flying Creatures from Darkworld",
            "A9 Bomber Flying Creatures from Darkworld",
            "AA Pikit",
            "AB Maiden",
            "AC Apple",
            "AD Lost Old Man",
            "AE Down Pipe",
            "AF Up Pipe",
            "B0 Right Pip",
            "B1 Left Pipe",
            "B2 Good bee again?",
            "B3 Hylian Inscription",
            "B4 Thief?s chest (not the one that follows you",
            "B5 Bomb Salesman",
            "B6 Kiki",
            "B7 Maiden following you in Blind Dungeon",
            "B8 Monologue Testing Sprite",
            "B9 Feuding Friends on Death Mountain",
            "BA Whirlpool",
            "BB Salesman / chestgame guy / 300 rupee giver guy / Chest game thief",
            "BC Drunk in the inn",
            "BD Vitreous (Large Eyeball",
            "BE Vitreous (Small Eyeball",
            "BF Vitreous' Lightning",
            "C0 Monster in Lake of Ill Omen / Quake Medallion",
            "C1 Agahnim teleporting Zelda to dark world",
            "C2 Boulders",
            "C3 Gibo",
            "C4 Thief",
            "C5 Medusa",
            "C6 Four Way Fireball Spitters (spit when you use your sword",
            "C7 Hokku",
            "C8 Big Fairy who heals you",
            "C9 Tektite",
            "CA Chain Chomp",
            "CB Trinexx",
            "CC Another part of trinexx",
            "CD Yet another part of trinexx",
            "CE Blind The Thief (Boss)",
            "CF Swamola",
            "D0 Lynel",
            "D1 Bunny Beam",
            "D2 Flopping fish",
            "D3 Stal",
            "D4 Landmine",
            "D5 Digging Game Proprietor",
            "D6 Ganon",
            "D7 Copy of Ganon",
            "D8 Heart",
            "D9 Green Rupee",
            "DA Blue Rupee",
            "DB Red Rupee",
            "DC Bomb Refill (1)",
            "DD Bomb Refill (4)",
            "DE Bomb Refill (8)",
            "DF Small Magic Refill",
            "E0 Full Magic Refill",
            "E1 Arrow Refill (5)",
            "E2 Arrow Refill (10)",
            "E3 Fairy",
            "E4 Key",
            "E5 Big Key",
            "E6 Shield",
            "E7 Mushroom",
            "E8 Fake Master Sword",
            "E9 Magic Shop dude / His items",
            "EA Heart Container",
            "EB Heart Piece",
            "EC Bushes",
            "ED Cane Of Somaria Platform",
            "EE Mantle",
            "EF Cane of Somaria Platform (Unused)",
            "F0 Cane of Somaria Platform (Unused)",
            "F1 Cane of Somaria Platform (Unused)",
            "F2 Medallion Tablet",
            "F3",
            "F4 Falling Rocks",
            "F5",
            "F6",
            "F7",
            "F8",
            "F9",
            "FA",
            "FB",
            "FC",
            "FD",
            "FE",
            "FF",
        };

        private void DecreaseFrameButton_Click(object sender, RoutedEventArgs e)
        {
            if (FrameSlider.Value > 0)
            {
                FrameSlider.Value--;
            }
        }

        private void IncreaseFrameButton_Click(object sender, RoutedEventArgs e)
        {
            if (FrameSlider.Value < 47)
            {
                FrameSlider.Value++;
            }
        }

        private void sliderFramebox_Changed(object sender, TextChangedEventArgs e)
        {
            if (changedFromForm == false)
            {
                int.TryParse(sliderFramebox.Text, out int a);
                FrameSlider.Value = a;
            }
        }

        private void FrameSlider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                if (FrameSlider.Value > 0)
                {
                    FrameSlider.Value--;
                }
            }
            else if (e.Delta > 0)
            {
                if (FrameSlider.Value < 47)
                {
                    FrameSlider.Value++;
                }
            }
        }

        private void functions_MouseDown(object sender, MouseButtonEventArgs e)
        {

            e.Handled = true;
        }

        private void searchfunctionBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            functionsListbox.SelectedIndex = -1;
            functionsListbox.Items.Clear();
            
            List<ListBoxItem> list = functionListItems.Where(x => (x.Content.ToString().ToLower()).Contains(searchfunctionBox.Text.ToLower())).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                functionsListbox.Items.Add(list[i]);
            }
        }

        private void functionsListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (functionsListbox.SelectedIndex != -1)
            {
                string s = "JSL " + (functionsListbox.SelectedItem as ListBoxItem).Content;
                codeEditor.Document.Insert(codeEditor.CaretOffset, s);
                //codeEditor.AppendText(s);

                e.Handled = true;
            }
        }


        private void macrosListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (macrosListbox.SelectedIndex != -1)
            {
                string s = "%" + (macrosListbox.SelectedItem as ListBoxItem).Content;
                codeEditor.Document.Insert(codeEditor.CaretOffset, s);
                //codeEditor.AppendText(s);

                e.Handled = true;
            }
        }

        private void searchmacrosBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            macrosListbox.SelectedIndex = -1;
            macrosListbox.Items.Clear();
            List<ListBoxItem> list = macrosListItems.Where(x => (x.Content.ToString().ToLower()).Contains(searchmacrosBox.Text.ToLower())).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                macrosListbox.Items.Add(list[i]);
            }
        }

        private void searchramBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ramListbox.SelectedIndex = -1;
            ramListbox.Items.Clear();

            List<ListBoxItem> list = ramListItems.Where(x => (x.Content.ToString().ToLower()).Contains(searchramBox.Text.ToLower())).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                ramListbox.Items.Add(list[i]);
            }
        }

        /*private void searchramBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ramListbox.SelectedIndex = -1;
            ramListbox.Items.Clear();

            List<ListBoxItem> list = zelda3symbols.Where(x => (x.ToolTip.ToString().ToLower()).Contains(searchramBox.Text.ToLower())).ToList();
            for (int i = 0; i < 50; i++)
            {
                if (i >= list.Count)
                {
                    break;
                }
                ramListbox.Items.Add(list[i]);
            }
        }*/

        private void ramListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ramListbox.SelectedIndex != -1)
            {
                string s = (ramListbox.SelectedItem as ListBoxItem).Content.ToString();
                if (((ramListbox.SelectedItem as ListBoxItem).Tag as string) == "X")
                {
                    s += ", X";
                }
                codeEditor.Document.Insert(codeEditor.CaretOffset, s);
                //codeEditor.AppendText(s);

                e.Handled = true;
            }
        }

        private void addActionButton_Click(object sender, RoutedEventArgs e)
        {
            AddUserRoutine();
        }

        private void renameActionButton_Click(object sender, RoutedEventArgs e)
        {
            RenameUserRoutine();
        }

        private void deleteActionButton_Click(object sender, RoutedEventArgs e)
        {
            previousCode = -1;
            RemoveUserRoutine();
            
        }

        private void shiftActionButton_Click(object sender, RoutedEventArgs e)
        {


        }


        int previousCode = -1;
        private void userroutinesListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (previousCode!= -1)
            {
                userRoutines[previousCode].code = codeEditor.Text;
                //save that code
            }

            if (userroutinesListbox.SelectedIndex != -1)
            {
                codeEditor.Text = userRoutines[userroutinesListbox.SelectedIndex+3].code;
                previousCode = userroutinesListbox.SelectedIndex + 3;
                coreroutinesListbox.SelectedIndex = -1;
            }

            
            
        }

        private void coreroutinesListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (previousCode != -1)
            {
                userRoutines[previousCode].code = codeEditor.Text;
                //save that code
            }

            if (coreroutinesListbox.SelectedIndex != -1)
            {
                codeEditor.Text = userRoutines[coreroutinesListbox.SelectedIndex].code;
                previousCode = coreroutinesListbox.SelectedIndex;
                userroutinesListbox.SelectedIndex = -1;
            }


        }

        private void property_sprname_TextChanged(object sender, TextChangedEventArgs e)
        {
            sprName = property_sprname.Text;
        }

        private void property_hitbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!projectLoaded)
            {
                return;
            }

            UpdateBackground();
        }

        private void property_hitbox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            byte.TryParse(property_hitbox.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out byte hitbox);

            if (e.Delta < 0)
            {
                if (hitbox > 0)
                {
                    hitbox--;
                }
            }
            else if (e.Delta > 0)
            {
                if (hitbox < 0x1F)
                {
                    hitbox++;
                }
            }

            property_hitbox.Text = hitbox.ToString("X2");

        }

        private void MainScreen_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Down)
            {
                MoveSelectedTiles(0, 1);
                e.Handled = true;
            }
            else if (e.Key == Key.Left)
            {
                MoveSelectedTiles(-1, 0);
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                MoveSelectedTiles(1, 0);
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                MoveSelectedTiles(0, -1);
                e.Handled = true;
            }

        }


        private void MoveSelectedTiles(int x, int y)
        {
            foreach (OamTile tile in editor.selectedTiles)
            {
                tile.x += (byte)x;
                tile.y += (byte)y;
            }
            RefreshScreen();
        }
    }
}
