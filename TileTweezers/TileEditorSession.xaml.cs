using _TileTweezers.Controls.TileEditorControl.TileEditorUtils;
using System;
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
                    tileEditorControl_Source.OutputImage = tileEditorControl_Source.TileSetImage;

                    tileEditorControl_Destination.SourceTileControl = tileEditorControl_Source;

                    tileEditorControl_Destination.InputImage = tileEditorControl_Source.TileSetImage;
                    tileEditorControl_Destination.InputImagePreview = tileEditorControl_Source.TileSetImagePreview;

                    // Hide the stamp tool on the left side
                    tileEditorControl_Source.stampBtn.Visibility = Visibility.Collapsed;

                    tileEditorControl_Source.loadTilemap_Btn.Visibility = Visibility.Collapsed;
                    tileEditorControl_Source.saveTilemapImgBtn.Visibility = Visibility.Collapsed;

                    tileEditorControl_Source.tilesetFileNameTxtBox.Visibility = Visibility.Collapsed;

                    //Set this, or zooming in will be smoothed out and you wont see pixels as easily
                    RenderOptions.SetBitmapScalingMode(tileEditorControl_Source.TileSetImage, BitmapScalingMode.NearestNeighbor);
                    RenderOptions.SetBitmapScalingMode(tileEditorControl_Source.TileSetImagePreview, BitmapScalingMode.NearestNeighbor);


                    // Choose the Stamp Selection tool
                    tileEditorControl_Source.StampGridSelect_Click(tileEditorControl_Destination.stampSelectBtn, new RoutedEventArgs(Button.ClickEvent));


                }
                _tileEditorControl_SourceInitialized = true;
            };

            tileEditorControl_Destination.Loaded += (s, e) =>
            {
                if (!_tileEditorControl_DestinationInitialized)
                {
                    tileEditorControl_Source.DestinationTileControl = tileEditorControl_Destination;

                    // Hide the selection stamp tool on the right side
                    tileEditorControl_Destination.stampSelectBtn.Visibility = Visibility.Collapsed;

                    // Hide the color tools
                    tileEditorControl_Destination.ColorToolsGrid.Visibility = Visibility.Hidden;
                    tileEditorControl_Destination.bottomToolsBar.Visibility = Visibility.Hidden;

                    // Change icon of open image file button
                    tileEditorControl_Destination.tilesetOrmapImg.Source = new BitmapImage(
                        new Uri("pack://application:,,,/Controls/TileEditorControl/TileEditorImages/tilemap.png"));
                    
                    tileEditorControl_Destination.loadTileset_Btn.Visibility = Visibility.Collapsed;
                    tileEditorControl_Destination.saveTilesetImgBtn.Visibility = Visibility.Collapsed;

                    //tileEditorControl_Destination.loadImg_Btn.Visibility = Visibility.Hidden;
                    //tileEditorControl_Destination.loadImg_Btn.Image = ;
                    //tileEditorControl_Destination.saveTilesetImgBtn.Visibility = Visibility.Hidden;

                    // Hide grid slice
                    tileEditorControl_Destination.GridSlicePanel.Visibility = Visibility.Hidden;
                    tileEditorControl_Destination.GridSlicePanel.IsEnabled = false;

                    tileEditorControl_Destination.tilesetFileNameTxtBox.Visibility = Visibility.Collapsed;

                    //Set this, or zooming in will be smoothed out and you wont see pixels as easily
                    RenderOptions.SetBitmapScalingMode(tileEditorControl_Destination.TileSetImage, BitmapScalingMode.NearestNeighbor);
                    RenderOptions.SetBitmapScalingMode(tileEditorControl_Destination.TileSetImagePreview, BitmapScalingMode.NearestNeighbor);

                    // Choose the Stamp tool
                    tileEditorControl_Destination.Stamp_Click(tileEditorControl_Destination.stampBtn, new RoutedEventArgs(Button.ClickEvent));
                }
                _tileEditorControl_DestinationInitialized = true;
            };

        }

        private void About_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OpenTilemap_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
