using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ZSpriteMaker
{
    /// <summary>
    /// Logique d'interaction pour Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
        }
        bool loaded = false;
        private string tempRomPath;
        private string tempEmulatorPath;
        private string tempAsarPath;
        private byte[] tempGridColor = new byte[3];
        private byte[] tempSelColor = new byte[3];
        private byte[] tempHitColor = new byte[3];
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

            Properties.Settings.Default.gridR = tempGridColor[0];
            Properties.Settings.Default.gridG = tempGridColor[1];
            Properties.Settings.Default.gridB = tempGridColor[2];
            Properties.Settings.Default.selR = tempSelColor[0];
            Properties.Settings.Default.selG = tempSelColor[1];
            Properties.Settings.Default.selB = tempSelColor[2];
            Properties.Settings.Default.hitboxR = tempHitColor[0];
            Properties.Settings.Default.hitboxG = tempHitColor[1];
            Properties.Settings.Default.hitboxB = tempHitColor[2];
            Properties.Settings.Default.emulatorPath = tempEmulatorPath;
            Properties.Settings.Default.romPath = tempRomPath;
            Properties.Settings.Default.asarPath = tempAsarPath;

            DialogResult = false;
            this.Close();
        }

        private void emulatorPathButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.emulatorPath = GetPath(Properties.Settings.Default.emulatorPath, "Snes Emulator (*.exe)|*.exe");
        }

        private void romPathButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.romPath = GetPath(Properties.Settings.Default.romPath, "Snes ROM (*.sfc;*.smc)|*.sfc;*.smc|All files (*.*)|*.*");
        }

        private void asarPathButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.asarPath = GetPath(Properties.Settings.Default.asarPath, "Asar (*.exe)|*.exe");
        }

        private string GetPath(string property, string filter)
        {
            OpenFileDialog fileDialog = new();
            fileDialog.FileName = property;
            fileDialog.Filter = filter;
            fileDialog.ShowDialog();
            return fileDialog.FileName;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tempGridColor[0] = Properties.Settings.Default.gridR;
            tempGridColor[1] = Properties.Settings.Default.gridG;
            tempGridColor[2] = Properties.Settings.Default.gridB;

            tempSelColor[0] = Properties.Settings.Default.selR;
            tempSelColor[1] = Properties.Settings.Default.selG;
            tempSelColor[2] = Properties.Settings.Default.selB;

            tempSelColor[0] = Properties.Settings.Default.hitboxR;
            tempSelColor[1] = Properties.Settings.Default.hitboxG;
            tempSelColor[2] = Properties.Settings.Default.hitboxB;

            GridRLabel.Content = "R " + tempGridColor[0].ToString();
            GridGLabel.Content = "G " + tempGridColor[1].ToString();
            GridBLabel.Content = "B " + tempGridColor[2].ToString();

            SelRLabel.Content = "R " + tempSelColor[0].ToString();
            SelGLabel.Content = "G " + tempSelColor[1].ToString();
            SelBLabel.Content = "B " + tempSelColor[2].ToString();

            tempEmulatorPath = Properties.Settings.Default.emulatorPath;
            tempRomPath = Properties.Settings.Default.romPath;
            tempAsarPath = Properties.Settings.Default.asarPath;
            Color c = Color.FromRgb(tempGridColor[0], tempGridColor[1], tempGridColor[2]);
            gridColorRect.Fill = new SolidColorBrush(c);

            c = Color.FromRgb(tempSelColor[0], tempSelColor[1], tempSelColor[2]);
            selColorRect.Fill = new SolidColorBrush(c);


            c = Color.FromRgb(tempHitColor[0], tempHitColor[1], tempHitColor[2]);
            hitColorRect.Fill = new SolidColorBrush(c);
            loaded = true;
        }

        private void GridSliders_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (loaded)
            {
                byte R = Properties.Settings.Default.gridR;
                GridRLabel.Content = "R " + R.ToString();
                byte G = Properties.Settings.Default.gridG;
                GridGLabel.Content = "G " + G.ToString();
                byte B = Properties.Settings.Default.gridB;
                GridBLabel.Content = "B " + B.ToString();
                Color c = Color.FromRgb(R, G, B);
                gridColorRect.Fill = new SolidColorBrush(c);
            }
        }

        private void SelSliders_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (loaded)
            {
                byte R = Properties.Settings.Default.selR;
                SelRLabel.Content = "R " + R.ToString();
                byte G = Properties.Settings.Default.selG;
                SelGLabel.Content = "G " + G.ToString();
                byte B = Properties.Settings.Default.selB;
                SelBLabel.Content = "B " + B.ToString();
                Color c = Color.FromRgb(R, G, B);
                selColorRect.Fill = new SolidColorBrush(c);
            }
        }

        private void ChangeFont_Click(object sender, RoutedEventArgs e)
        {

            System.Windows.Forms.FontDialog fd = new System.Windows.Forms.FontDialog();
            fd.Font = new System.Drawing.Font(previewLabelFont.FontFamily.ToString(), (float)(previewLabelFont.FontSize * 72.0 / 96.0));
            var result = fd.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                previewLabelFont.FontFamily = new FontFamily(fd.Font.Name);
                previewLabelFont.FontSize = fd.Font.Size * 96.0 / 72.0;
            }
        }

        private void HitSliders_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (loaded)
            {
                byte R = Properties.Settings.Default.hitboxR;
                HitRLabel.Content = "R " + R.ToString();
                byte G = Properties.Settings.Default.hitboxG;
                HitGLabel.Content = "G " + G.ToString();
                byte B = Properties.Settings.Default.hitboxB;
                HitBLabel.Content = "B " + B.ToString();
                Color c = Color.FromRgb(R, G, B);
                hitColorRect.Fill = new SolidColorBrush(c);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.UndoSizeString = undoBufferBox.Text;
        }
    }
}
