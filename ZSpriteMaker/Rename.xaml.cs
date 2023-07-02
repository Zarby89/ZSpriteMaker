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
    /// Logique d'interaction pour Rename.xaml
    /// </summary>
    public partial class Rename : Window
    {
        public Rename()
        {
            InitializeComponent();
            DataContext = this;
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

            DialogResult = false;
            this.Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {

            DialogResult = true;
            this.Close();
        }
        private void SimpleCommand_OnExecuted(object sender, object e)
        {
            OkButton_Click(null,null);
        }

    }

    class SimpleCommand : ICommand
    {
        public event EventHandler<object> Executed;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (Executed != null)
                Executed(this, parameter);
        }

        public event EventHandler CanExecuteChanged;
    }

}
