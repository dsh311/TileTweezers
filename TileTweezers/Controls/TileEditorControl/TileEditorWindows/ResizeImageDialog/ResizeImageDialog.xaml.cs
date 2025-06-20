/*
 * Copyright (C) 2025 David S. Shelley <davidsmithshelley@gmail.com>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License 
 * along with this program. If not, see <https://www.gnu.org/licenses/>.
 */

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
