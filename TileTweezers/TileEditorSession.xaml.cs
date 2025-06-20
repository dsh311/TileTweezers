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

using _TileTweezers.Controls.TileEditorControl.TileEditorUtils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace _TileTweezers
{
    /// <summary>
    /// Interaction logic for TileEditorSession.xaml
    /// </summary>
    public partial class TileEditorSession : UserControl
    {
        private bool _tileEditorControl_SourceInitialized = false;
        private bool _tileEditorControl_DestinationInitialized = false;
        public TileEditorSession()
        {
            InitializeComponent();


            tileEditorControl_Source.Loaded += (s, e) =>
            {
                if (!_tileEditorControl_SourceInitialized)
                {
                    tileEditorControl_Destination.SourceTileControl = tileEditorControl_Source;

                    tileEditorControl_Destination.InputImage = tileEditorControl_Source.TileSetImage;
                    tileEditorControl_Destination.InputImagePreview = tileEditorControl_Source.TileSetImagePreview;

                    tileEditorControl_Source.stampBtn.Visibility = Visibility.Collapsed;

                    //Set this, or zooming in will be smoothed out and you wont see pixels as easily
                    RenderOptions.SetBitmapScalingMode(tileEditorControl_Source.TileSetImage, BitmapScalingMode.NearestNeighbor);
                    RenderOptions.SetBitmapScalingMode(tileEditorControl_Source.TileSetImagePreview, BitmapScalingMode.NearestNeighbor);
                }
                _tileEditorControl_SourceInitialized = true;
            };

            tileEditorControl_Destination.Loaded += (s, e) =>
            {
                if (!_tileEditorControl_DestinationInitialized)
                {
                    tileEditorControl_Source.DestinationTileControl = tileEditorControl_Destination;

                    // Hide the color tools
                    tileEditorControl_Destination.ColorToolsGrid.Visibility = Visibility.Hidden;
                    tileEditorControl_Destination.bottomToolsBar.Visibility = Visibility.Hidden;

                    // Hide grid slice
                    tileEditorControl_Destination.GridSlicePanel.Visibility = Visibility.Hidden;
                    tileEditorControl_Destination.GridSlicePanel.IsEnabled = false;

                    //Set this, or zooming in will be smoothed out and you wont see pixels as easily
                    RenderOptions.SetBitmapScalingMode(tileEditorControl_Destination.TileSetImage, BitmapScalingMode.NearestNeighbor);
                    RenderOptions.SetBitmapScalingMode(tileEditorControl_Destination.TileSetImagePreview, BitmapScalingMode.NearestNeighbor);

                    // Choose the Stamp tool
                    tileEditorControl_Destination.Stamp_Click(tileEditorControl_Destination.stampBtn, new RoutedEventArgs(Button.ClickEvent));
                }
                _tileEditorControl_DestinationInitialized = true;
            };

        }
    }
}
