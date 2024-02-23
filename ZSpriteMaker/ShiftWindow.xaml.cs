using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace ZSpriteMaker
{
    /// <summary>
    /// Logique d'interaction pour ShiftWindow.xaml
    /// </summary>
    public partial class ShiftWindow : Window
    {
        internal int xShift = 0;
        internal int yShift = 0;
        public ShiftWindow()
        {
            InitializeComponent();
        }

        private void TextBox_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(xTextbox.Text, out xShift);
            int.TryParse(yTextbox.Text, out yShift);
            this.Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            xShift = 0;
            yShift = 0;
            this.Close();
        }
    }
}
