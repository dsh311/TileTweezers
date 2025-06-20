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

namespace _TileTweezers.Controls.TileEditorControl.TileEditorWindows.ResizeImageDialog
{
    /// <summary>
    /// Interaction logic for ResizeImageDialog.xaml
    /// </summary>
    public partial class ResizeImageDialog : Window
    {
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public ResizeImageDialog(int imageWidth, int imageHeight)
        {
            ImageWidth = imageWidth;
            ImageHeight = imageHeight;

            InitializeComponent();

            WidthTxtBox.Text = imageWidth.ToString();
            HeightTxtBox.Text = imageHeight.ToString();

            WidthTxtBox.Focus();
        }

        public ResizeImageDialog()
        {
            InitializeComponent();
            WidthTxtBox.Focus();
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text);
        }

        private static bool IsTextNumeric(string text)
        {
            return text.All(char.IsDigit);
        }

        private void CancelImageResize_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void MyTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            tb?.SelectAll();
        }

        private void OkImageResize_Click(object sender, RoutedEventArgs e)
        {
            int txtBoxImageWidth;
            if (int.TryParse(WidthTxtBox.Text, out txtBoxImageWidth))
            {
                ImageWidth = txtBoxImageWidth;
            }
            else
            {
                this.DialogResult = false;
            }

            int txtBoxImageHeight;
            if (int.TryParse(HeightTxtBox.Text, out txtBoxImageHeight))
            {
                ImageHeight = txtBoxImageHeight;
            }
            else
            {
                this.DialogResult = false;
            }

            this.DialogResult = true; // Also closes the dialog
        }
    }
}
