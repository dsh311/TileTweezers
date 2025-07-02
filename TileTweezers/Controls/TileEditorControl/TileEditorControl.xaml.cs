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

using _TileTweezers.Controls.TileEditorControl.TileEditorInterfaces;
using _TileTweezers.Controls.TileEditorControl.TileEditorState;
using _TileTweezers.Controls.TileEditorControl.TileEditorTools;
using _TileTweezers.Controls.TileEditorControl.TileEditorUtils;
using _TileTweezers.Controls.TileEditorControl.TileEditorWindows.ResizeImageDialog;
using _TileTweezers.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace _TileTweezers
{
    /// <summary>
    /// Interaction logic for Toolbar.xaml
    /// </summary>
    public partial class TileEditorControl : UserControl, ITileEditControl
    {
        public event EventHandler OpenFileClicked;
        public event EventHandler SaveFileClicked;

        public event Action<bool> GridToggleClicked;
        public event Action<bool> CheckerboardToggleClicked;
        public event Action<double> ZoomSliderChanged;

        public ITileEditControl? SourceTileControl { get; set; }
        public ITileEditControl? DestinationTileControl { get; set; }

        public string TilesetPath { get; set; }
        public Rectangle? SelectionRect { get; set; }

        public WriteableBitmap? SelectionAsBitmap { get; set; }

        public int SelectedTilesetRowStart { get; set; }
        public int SelectedTilesetColumnStart { get; set; }
        public int SelectedTilesetRowEnd { get; set; }
        public int SelectedTilesetColumnEnd { get; set; }

        public int SelectedTilesetRowMouseDownOffset { get; set; }
        public int SelectedTilesetColumnMouseDownOffset { get; set; }

        //Tools
        public IPaintTool? TheTool { get; set; }
        public IPaintTool? pencilTool { get; set; }
        public IPaintTool? eraserTool { get; set; }
        public IPaintTool? slurperTool { get; set; }
        public IPaintTool? bucketTool { get; set; }
        public IPaintTool? selectTool { get; set; }
        public IPaintTool? stampTool { get; set; }

        public Point? MouseDownLastLoc { get; set; }
        public Point MouseOverLocation { get; set; }
        public int MouseOverGridRow { get; set; }
        public int MouseOverGridCol { get; set; }


        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public int GridDimention { get; set; }

        public Image? InputImage { get; set; }
        public Image? InputImagePreview { get; set; }
        public Image? OutputImage { get; set; }

        public bool IsDragging { get; set; }
        public bool IsDraggingSelection { get; set; }

        private SolidColorBrush selectedBrush;
        public UndoRedoManager undoRedoManager;
        private DateTime _lastUndoTime = DateTime.MinValue;
        private TimeSpan _undoCooldown = TimeSpan.FromMilliseconds(100);

        private bool _tileSetImageInitialized = false;
        private bool _tileSetImagePreviewInitialized = false;
        private bool _overlayTilesetGridInitialized = false;
        private bool _checkerboardBackgroundInitialized = false;

        public enum ToolMode
        {
            Stamp,
            Select,
            SelectFree,
            Pencil,
            Bucket,
            Eraser,
            Slurper
        }

        ToolMode selectedTool;

        public void deselectToolButtons()
        {
            //Clear all button selections
            stampBtn.BorderBrush = Brushes.Transparent;
            stampBtn.BorderThickness = new Thickness(0);

            selectBtn.BorderBrush = Brushes.Transparent;
            selectBtn.BorderThickness = new Thickness(0);

            selectFreeBtn.BorderBrush = Brushes.Transparent;
            selectFreeBtn.BorderThickness = new Thickness(0);

            selectOvalFreeBtn.BorderBrush = Brushes.Transparent;
            selectOvalFreeBtn.BorderThickness = new Thickness(0);

            pencilBtn.BorderBrush = Brushes.Transparent;
            pencilBtn.BorderThickness = new Thickness(0);

            bucketBtn.BorderBrush = Brushes.Transparent;
            bucketBtn.BorderThickness = new Thickness(0);

            eraserBtn.BorderBrush = Brushes.Transparent;
            eraserBtn.BorderThickness = new Thickness(0);

            slurperBtn.BorderBrush = Brushes.Transparent;
            slurperBtn.BorderThickness = new Thickness(0);
        }

        public TileEditorControl()
        {
            InitializeComponent();

            MainGrid.Focus();

            IsDragging = false;
            IsDraggingSelection = false;

            SelectedTilesetRowStart = -1;
            SelectedTilesetColumnStart = -1;
            SelectedTilesetRowEnd = -1;
            SelectedTilesetColumnEnd = -1;
            // Keep track of click inside of a selection
            SelectedTilesetRowMouseDownOffset = 0;
            SelectedTilesetColumnMouseDownOffset = 0;

            TilesetPath = "";
            SelectionRect = null;
            SelectionAsBitmap = null;
            GridDimention = 16;

            MouseDownLastLoc = null;

            stampTool = new StampTool();
            pencilTool = new PencilTool();
            eraserTool = new EraserTool();
            slurperTool = new SlurperTool();
            bucketTool = new BucketTool();
            selectTool = new SelectTool();
            if (selectTool is SelectTool concreteSelectTool)
            {
                concreteSelectTool.useThisGridDimension = GridDimention;
            }

            selectedBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));

            SelectGrid_Click(selectBtn, new RoutedEventArgs(Button.ClickEvent));

            TileSetImage.Loaded += (s, e) =>
            {
                if (!_tileSetImageInitialized)
                {
                    //Ensure we are always working with a WritableBitmap
                    //TileSetImage is not a WriteableBitmap, but instead a BitmapFrameDecode
                    //which is what WPF creates when you load an image from a file
                    GraphicsUtils.transparentImage(TileSetImage);

                    //TileSetImage.Source.Width and .Height give you device-independent units (DIPs), not actual pixels.
                    var bmpSource = TileSetImage.Source as BitmapSource;
                    if (bmpSource != null)
                    {
                        ImageWidth = bmpSource.PixelWidth;
                        ImageHeight = bmpSource.PixelHeight;

                        imgDimensions.Text = ImageWidth + " x " + ImageHeight;
                    }

                    // Save the current state of the TileSetImage
                    if (undoRedoManager == null)
                    {
                        undoRedoManager = new UndoRedoManager(); // Create the undo redo manager
                        
                        //Ensure the current state is saved since the UndoRedoManager requires knowing the current state
                        WriteableBitmap currentImage = new WriteableBitmap((BitmapSource)TileSetImage.Source);
                        EditorState currentState = new EditorState(currentImage);
                        undoRedoManager.SaveState(currentState);
                    }
                }
                _tileSetImageInitialized = true;
            };

            TileSetImagePreview.Loaded += (s, e) =>
            {
                if (!_tileSetImagePreviewInitialized)
                {
                    //Ensure we are always working with a WritableBitmap
                    //TileSetImagePreview is not a WriteableBitmap, but instead a BitmapFrameDecode
                    //which is what WPF creates when you load an image from a file
                    GraphicsUtils.transparentImage(TileSetImagePreview);
                }
                _tileSetImagePreviewInitialized = true;
            };

            overlayTilesetGrid.Loaded += (s, e) =>
            {
                if (!_overlayTilesetGridInitialized)
                {
                    GraphicsUtils.DrawGridOnCanvas(overlayTilesetGrid, 512, 512, GridDimention, Brushes.Gray, 0.5);
                }
                _overlayTilesetGridInitialized = true;
            };

            CheckerboardBackground.Loaded += (s, e) =>
            {
                if (!_checkerboardBackgroundInitialized)
                {
                    GraphicsUtils.DrawCheckerboard(CheckerboardBackground, GridDimention);
                }
                _checkerboardBackgroundInitialized = true;
            };

        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            string theMsg = "TileTweezers" + Environment.NewLine + Environment.NewLine;
            theMsg += "By David S. Shelley - (2025)" + Environment.NewLine + Environment.NewLine;

            MessageBox.Show(theMsg);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ToolbarControl_SaveFileClicked(this, EventArgs.Empty);
        }

        private void LoadTileSet_Click(object sender, RoutedEventArgs e)
        {
            openFileDialogChooseFileset();
        }

        private void CheckBox_Checked_GridOverlay(object sender, RoutedEventArgs e)
        {
            if (overlayTilesetGrid != null)
            {
                overlayTilesetGrid.Visibility = Visibility.Visible;
            }
        }

        private void CheckBox_UnChecked_GridOverlay(object sender, RoutedEventArgs e)
        {
            if (overlayTilesetGrid != null)
            {
                overlayTilesetGrid.Visibility = Visibility.Hidden;
            }
        }

        private void CheckBox_Checked_CheckerboardUnderlay(object sender, RoutedEventArgs e)
        {
            if (CheckerboardBackground != null)
            {
                CheckerboardBackground.Visibility = Visibility.Visible;
            }
        }

        private void CheckBox_UnChecked_CheckerboardUnderlay(object sender, RoutedEventArgs e)
        {
            if (CheckerboardBackground != null)
            {
                CheckerboardBackground.Visibility = Visibility.Hidden;
            }
        }

        private void ToolbarControl_SaveFileClicked(object sender, EventArgs e)
        {
            if (TileSetImage == null)
            {
                MessageBox.Show("Error, main image is empty");
                return;
            }

            // Create and configure the SaveFileDialog
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PNG Files (*.png)|*.png|All Files (*.*)|*.*",
                DefaultExt = "png",
                FileName = "Untitled.png"
            };
            bool? result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                string filePath = saveFileDialog.FileName;
                // Now that we have a file path and are ready, flatten the preview layer
                // Clear any selection since we are flattening
                savePreviewLayerToImageWhenSelectTool();
                overlayTilesetSelection.Children.Clear();

                // Remove the selection
                if (selectTool is SelectTool concreteSelectTool)
                {
                    removeSelectionFromSelectTool(concreteSelectTool);
                }

                GraphicsUtils.SaveImageToFile(TileSetImage, filePath);
            }
        }

        private void MyGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                double delta = e.Delta > 0 ? 0.1 : -0.1;
                double currentValue = ZoomSlider.Value;
                double newValue = Math.Clamp(currentValue + delta, ZoomSlider.Minimum, ZoomSlider.Maximum);

                ZoomSlider.Value = newValue; // This triggers ZoomSlider_ValueChanged
                e.Handled = true;
            }
        }

        private void removeSelectionFromSelectTool(SelectTool selectedTool)
        {
            // Remove the selection
            if (selectedTool is SelectTool concreteSelectTool)
            {
                concreteSelectTool.SelectionRect = null;
                concreteSelectTool.LastValidSelectionRect = null;
                concreteSelectTool.SelectionAsBitmap = null;
                concreteSelectTool.LastValidSelectionAsBitmap = null;
                concreteSelectTool.MouseIsDown = false;
                concreteSelectTool.IsDraggingSelection = false;
                concreteSelectTool.useThisGridDimension = GridDimention;
            }
        }

        public async void openFileDialogChooseFileset()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "PNG and BMP Files (*.png;*.bmp)|*.png;*.bmp|PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.exbmpe";
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                string filePath = openFileDialog.FileName;
                // Save state
                TilesetPath = filePath;


                if (File.Exists(TilesetPath))
                {
                    // Remove the selection
                    if (selectTool is SelectTool concreteSelectTool)
                    {
                        removeSelectionFromSelectTool(concreteSelectTool);
                    }
                    overlayTilesetSelection.Children.Clear();

                    tilesetFileNameTxtBox.Text = System.IO.Path.GetFileName(TilesetPath);

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(TilesetPath);
                    //bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.CacheOption = BitmapCacheOption.Default; // Never cache
                    bitmap.EndInit();

                    var fixedBitmap = GraphicsUtils.NormalizeImageDpi(bitmap);
                    var fixedBitDepth = GraphicsUtils.EnsureBgra32Writable(fixedBitmap);

                    // Show loaded image
                    TileSetImage.Source = fixedBitDepth;


                    // Save current loaded image so we can undo it
                    WriteableBitmap currentImage = new WriteableBitmap((BitmapSource)TileSetImage.Source);
                    EditorState currentState = new EditorState(currentImage);
                    undoRedoManager.SaveState(currentState);


                    //Zoom should be 1
                    ZoomSlider.Value = 1;


                    int pixelWidth = bitmap.PixelWidth;
                    int pixelHeight = bitmap.PixelHeight;

                    ImageWidth = pixelWidth;
                    ImageHeight = pixelHeight;

                    imgDimensions.Text = pixelWidth + " x " + pixelHeight;

                    GraphicsUtils.DrawGridOnCanvas(overlayTilesetGrid,
                        ImageWidth,
                        ImageHeight,
                        GridDimention,
                        Brushes.Gray,
                        0.5);
                }
                else
                {
                    MessageBox.Show("Image file not found.");
                }

                //Redraw or checkerboard might have too many squres form when the control is initialized and draws it
                CheckerboardBackground.Height = TileSetImage.Source.Height;
                CheckerboardBackground.Width = TileSetImage.Source.Width;
                GraphicsUtils.DrawCheckerboard(CheckerboardBackground, GridDimention);

                var layoutTransform = TileSetImage.LayoutTransform as ScaleTransform;
                if (layoutTransform != null)
                {
                    double scaleX = layoutTransform.ScaleX;
                    double scaleY = layoutTransform.ScaleY;
                    System.Windows.MessageBox.Show("Image scale is: " + scaleX + "," + scaleY);
                }
            }
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            double newZoom = e.NewValue;

            if (UnifiedScaleTransform != null)
            {
                // Display the current percentage
                // Convert to percentage string
                string zoomText = $"{Math.Round(newZoom * 100)}%";
                // Clear selection to avoid it auto-selecting the nearest ComboBoxItem
                ZoomComboBox.SelectedItem = null;
                ZoomComboBox.Text = zoomText;

                double currentScale = UnifiedScaleTransform.ScaleX;

                UnifiedScaleTransform.ScaleX = newZoom;
                UnifiedScaleTransform.ScaleY = newZoom;

                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    // Mouse position relative to ScrollViewer
                    Point mousePosInScrollViewer = Mouse.GetPosition(scrollViewerForImage);
                    double logicalX = (scrollViewerForImage.HorizontalOffset + mousePosInScrollViewer.X) / currentScale;
                    double logicalY = (scrollViewerForImage.VerticalOffset + mousePosInScrollViewer.Y) / currentScale;

                    void OnLayoutUpdated(object? sender, EventArgs e)
                    {
                        // Unhook after first call
                        scrollViewerForImage.LayoutUpdated -= OnLayoutUpdated;

                        double newOffsetX = logicalX * newZoom - mousePosInScrollViewer.X;
                        double newOffsetY = logicalY * newZoom - mousePosInScrollViewer.Y;

                        scrollViewerForImage.ScrollToHorizontalOffset(newOffsetX);
                        scrollViewerForImage.ScrollToVerticalOffset(newOffsetY);
                    }

                    scrollViewerForImage.LayoutUpdated += OnLayoutUpdated;
                    e.Handled = true;
                }
            }

        }
        private void Toolboar_Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void Toolboar_Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            ZoomSlider.Value *= 0.75;
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            ZoomSlider.Value *= 1.25;
        }

        private void Image_MouseMove_Tileset(object sender, MouseEventArgs e)
        {

            Point position = e.GetPosition((IInputElement)sender);
            ToolResult aToolResult;

            if (TheTool is StampTool concreteStampTool)
            {
                aToolResult = concreteStampTool?.OnMouseMoveStamp(SourceTileControl, TileSetImage, TileSetImagePreview, overlayTilesetSelection, position, GridDimention, selectedBrush);
            }
            else
            {
                aToolResult = TheTool?.OnMouseMove(TileSetImage, TileSetImagePreview, overlayTilesetSelection, position, GridDimention, selectedBrush);
            }

            // Save Undo state
            if (aToolResult?.Success == true && aToolResult.ShouldSaveForUndo)
            {
                // Ensure UI has rendered before capturing image state
                TileSetImage.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);

                if (TileSetImage.Source is BitmapSource source)
                {
                    var currentImage = new WriteableBitmap(source);
                    var currentState = new EditorState(currentImage);
                    undoRedoManager.SaveState(currentState);
                }
            }

            if (aToolResult != null && aToolResult.Success)
            {
                if (aToolResult.PickedColor != null)
                {
                    selectedBrush = new SolidColorBrush(aToolResult.PickedColor.Value);
                    colorPickerBoxBtn.Background = selectedBrush;
                }
            }

            if (aToolResult?.SelectionRect != null)
            {
                selectionRectTxtBox.Text =
                    aToolResult?.SelectionRect.Value.Width.ToString() + "px ," +
                    aToolResult?.SelectionRect.Value.Height.ToString() + "px";
            }
            else
            {
                selectionRectTxtBox.Text = "";
            }

            // Coordinates to display
            Point dipPosition = e.GetPosition((IInputElement)sender);
            var dpi = VisualTreeHelper.GetDpi((Visual)sender);
            Point pixelPosition = new Point(dipPosition.X * dpi.DpiScaleX, dipPosition.Y * dpi.DpiScaleY);
            mouseLoc.Text = pixelPosition.X.ToString("0") + ", " + pixelPosition.Y.ToString("0");

            (int theRow, int theColumn) = GraphicsUtils.GetGridXYFromPosition(sender, position, GridDimention);
            gridLoc.Text = "R: " + theRow.ToString() + " C: " + theColumn.ToString();
        }

        private void Image_MouseDown_Tileset(object sender, MouseButtonEventArgs e)
        {
            IsDragging = true;

            Point position = e.GetPosition((IInputElement)sender);

            ToolResult aToolResult;


            if (TheTool is StampTool concreteStampTool)
            {
                aToolResult = concreteStampTool?.OnMouseDownStamp(SourceTileControl, TileSetImage, TileSetImagePreview, overlayTilesetSelection, position, GridDimention, selectedBrush);

            }
            else
            {
                aToolResult = TheTool?.OnMouseDown(TileSetImage, TileSetImagePreview, overlayTilesetSelection, position, GridDimention, selectedBrush);
            }

            // Save Undo state
            if (aToolResult?.Success == true && aToolResult.ShouldSaveForUndo)
            {
                // Ensure UI has rendered before capturing image state
                TileSetImage.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);

                if (TileSetImage.Source is BitmapSource source)
                {
                    var currentImage = new WriteableBitmap(source);
                    var currentState = new EditorState(currentImage);
                    undoRedoManager.SaveState(currentState);
                }
            }

            if (aToolResult != null && aToolResult.PickedColor != null)
            {
                selectedBrush = new SolidColorBrush(aToolResult.PickedColor.Value);
                colorPickerBoxBtn.Background = selectedBrush;
            }

            if (aToolResult?.SelectionRect != null)
            {
                selectionRectTxtBox.Text =
                    aToolResult?.SelectionRect.Value.Width.ToString() + "px ," +
                    aToolResult?.SelectionRect.Value.Height.ToString() + "px";
            }
            else
            {
                selectionRectTxtBox.Text = "";
            }

            mouseLoc.Text = position.X.ToString("0") + ", " + position.Y.ToString("0");
            (int theRow, int theColumn) = GraphicsUtils.GetGridXYFromPosition(sender, position, GridDimention);
            gridLoc.Text = "R: " + theRow.ToString() + " C: " + theColumn.ToString();
        }

        private void Image_MouseUp_Tileset(object sender, MouseButtonEventArgs e)
        {
            IsDragging = false;
            MouseDownLastLoc = null;
            Point position = e.GetPosition((IInputElement)sender);

            ToolResult aToolResult = TheTool?.OnMouseUp(TileSetImage, TileSetImagePreview, overlayTilesetSelection, position, GridDimention, selectedBrush);

            // Save Undo state
            if (aToolResult?.Success == true && aToolResult.ShouldSaveForUndo)
            {
                // Ensure UI has rendered before capturing image state
                TileSetImage.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);

                if (TileSetImage.Source is BitmapSource source)
                {
                    var currentImage = new WriteableBitmap(source);
                    var currentState = new EditorState(currentImage);
                    undoRedoManager.SaveState(currentState);
                }
            }

            if (aToolResult?.SelectionRect != null)
            {
                selectionRectTxtBox.Text =
                    aToolResult?.SelectionRect.Value.Width.ToString() + "px ," +
                    aToolResult?.SelectionRect.Value.Height.ToString() + "px";
            }
            else
            {
                selectionRectTxtBox.Text = "";
            }


            mouseLoc.Text = position.X.ToString("0") + ", " + position.Y.ToString("0");
            (int theRow, int theColumn) = GraphicsUtils.GetGridXYFromPosition(sender, position, GridDimention);
            gridLoc.Text = "R: " + theRow.ToString() + " C: " + theColumn.ToString();
        }

        private void TileSet_ScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void ScrollViewerForImage_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

        }

        private void Image_MouseEnter_Tileset(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Cross;
        }

        private void Image_MouseLeave_Tileset(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = null;
            //Clear the square that appears under the cursor when the cursor leaves the image
            overlayTilesetCursor.Children.Clear();

            Point position = e.GetPosition((IInputElement)sender);
            ToolResult aToolResult;

            if (TheTool is StampTool concreteStampTool)
            {
                aToolResult = concreteStampTool?.OnMouseLeaveStamp(SourceTileControl, TileSetImage, TileSetImagePreview, overlayTilesetSelection, position, GridDimention, selectedBrush);
            }
            else
            {
                aToolResult = TheTool?.OnMouseLeave(TileSetImage, TileSetImagePreview, overlayTilesetSelection, position, GridDimention, selectedBrush);
            }

        }

        private void ZoomComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ZoomComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string? content = selectedItem.Content as string;

                if (content != null && content.EndsWith("%"))
                {
                    string numberPart = content.TrimEnd('%');

                    if (double.TryParse(numberPart, out double percent))
                    {
                        double newValue = percent / 100.0;
                        ZoomSlider.Value = newValue;
                    }
                }
            }
        }

        private void ZoomComboBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void ColorPicker_Click(object sender, RoutedEventArgs e)
        {
            //var colorDialog = new System.Windows.Forms.ColorDialog();
            var colorDialog = new System.Windows.Forms.ColorDialog
            {
                AllowFullOpen = true,   // Allows user to click "Define Custom Colors"
                FullOpen = true,        // Opens the dialog with the custom colors section already expanded
                AnyColor = true         // Show all available colors (not just the system palette)
            };
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Convert from System.Drawing.Color to System.Windows.Media.Color
                var drawingColor = colorDialog.Color;
                var mediaColor = Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
                selectedBrush = new SolidColorBrush(mediaColor);
                colorPickerBoxBtn.Background = selectedBrush;
            }
        }

        public void SetGridDimension(int gridDim)
        {
            // Clear any selection since the grid is about to change
            savePreviewLayerToImageWhenSelectTool();
            if (overlayTilesetSelection != null)
            {
                overlayTilesetSelection.Children.Clear();
            }

            // Save the new grid dimension to the select tool
            GridDimention = gridDim;

            if (selectTool is SelectTool concreteSelectTool)
            {
                removeSelectionFromSelectTool(concreteSelectTool);
                concreteSelectTool.useThisGridDimension = GridDimention;
            }

            // Redraw the new grid and background
            GraphicsUtils.DrawGridOnCanvas(overlayTilesetGrid,
                ImageWidth,
                ImageHeight,
                GridDimention,
                Brushes.Gray,
                0.5);

            GraphicsUtils.DrawCheckerboard(CheckerboardBackground, GridDimention);

        }

        private void GridResComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string content = selectedItem.Content.ToString();
                if (!string.IsNullOrEmpty(content))
                {
                    // Split at 'x' and try to parse the first part
                    string[] parts = content.Split('x');
                    if (parts.Length > 0 && int.TryParse(parts[0], out int parsedValue))
                    {

                        SetGridDimension(parsedValue);

                        //If there is a destination control, set the grid for that control to match
                        if (DestinationTileControl != null)
                        {
                            DestinationTileControl.SetGridDimension(parsedValue);
                        }
                    }
                }
            }
        }

        private void GridResComboBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void MainGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                // Force choosing the select tool
                if (TheTool is SelectTool concreteSelectTool)
                {
                    concreteSelectTool.EscapeSelection(TileSetImage, TileSetImagePreview, overlayTilesetSelection);
                }
            }

            // Ctrl + C for Copy selection
            if (e.Key == Key.C && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (TheTool is SelectTool concreteSelectTool)
                {
                }
            }
            // Ctrl + V for Past selection
            if (e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (TheTool is SelectTool concreteSelectTool)
                {
                }
            }

            // Ctrl + A for Select All
            if (e.Key == Key.A && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                savePreviewLayerToImageWhenSelectTool();
                deselectToolButtons();
                overlayTilesetSelection.Children.Clear();
                selectedTool = ToolMode.Select;
                TheTool = selectTool;
                selectBtn.BorderBrush = new SolidColorBrush(Color.FromRgb(76, 194, 255));
                selectBtn.BorderThickness = new Thickness(1);
                if (TheTool is SelectTool concreteSelectTool)
                {
                    concreteSelectTool.SelectAll(TileSetImage, TileSetImagePreview, overlayTilesetSelection);
                }
            }

            // Ctrl + Z for Undo
            if (e.Key == Key.Z && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                var now = DateTime.Now;
                if (now - _lastUndoTime >= _undoCooldown)
                {
                    _lastUndoTime = now;

                    undoRedoManager.Undo();
                    if (undoRedoManager.currentState != null && undoRedoManager.currentState.Image != null)
                    {
                        WriteableBitmap restoreThisImage = undoRedoManager.currentState.Image;
                        if (restoreThisImage != null)
                        {
                            // Remove the selection
                            if (selectTool is SelectTool concreteSelectTool)
                            {
                                overlayTilesetSelection.Children.Clear();
                                removeSelectionFromSelectTool(concreteSelectTool);
                            }
                            TileSetImage.Source = new WriteableBitmap(restoreThisImage);
                            int pixelWidth = restoreThisImage.PixelWidth;
                            int pixelHeight = restoreThisImage.PixelHeight;
                            imgDimensions.Text = pixelWidth + " x " + pixelHeight;
                        }
                    }
                }

                e.Handled = true; //Prevents further propagation of this key event
                return;
            }

            // Ctrl + Y for Re-Undo
            if (e.Key == Key.Y && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                undoRedoManager.Redo();
                if (undoRedoManager.currentState?.Image != null)
                {
                    WriteableBitmap restoreThisImage = undoRedoManager.currentState.Image;
                    if (restoreThisImage != null)
                    {
                        // Remove the selection
                        if (selectTool is SelectTool concreteSelectTool)
                        {
                            overlayTilesetSelection.Children.Clear();
                            removeSelectionFromSelectTool(concreteSelectTool);
                        }
                        TileSetImage.Source = new WriteableBitmap(restoreThisImage);
                        int pixelWidth = restoreThisImage.PixelWidth;
                        int pixelHeight = restoreThisImage.PixelHeight;
                        imgDimensions.Text = pixelWidth + " x " + pixelHeight;
                    }
                }

                e.Handled = true; // Optional: Prevents further propagation of this key event
                return;
            }

            if (e.Key == Key.Delete)
            {

                if (TheTool is SelectTool concreteSelectTool)
                {
                    concreteSelectTool.DeleteSelection(TileSetImage, TileSetImagePreview, overlayTilesetSelection);
                }
            }
        }


        private void savePreviewLayerToImageWhenSelectTool()
        {
            if (TheTool is SelectTool concreteSelectTool)
            {
                concreteSelectTool.saveValidSelectionBitmapToImageLayer(TileSetImage, TileSetImagePreview);
            }
        }
        private void Pencil_Click(object sender, RoutedEventArgs e)
        {
            savePreviewLayerToImageWhenSelectTool();
            // Clear selection canvas
            SelectionRect = null;
            overlayTilesetSelection.Children.Clear();
            deselectToolButtons();
            selectedTool = ToolMode.Pencil;
            pencilBtn.BorderBrush = new SolidColorBrush(Color.FromRgb(76, 194, 255));
            pencilBtn.BorderThickness = new Thickness(1);
            TheTool = pencilTool;
        }

        private void SelectGrid_Click(object sender, RoutedEventArgs e)
        {
            savePreviewLayerToImageWhenSelectTool();
            deselectToolButtons();
            overlayTilesetSelection.Children.Clear();
            selectedTool = ToolMode.Select;
            TheTool = selectTool;

            // Remove the selection
            if (selectTool is SelectTool concreteSelectTool)
            {
                removeSelectionFromSelectTool(concreteSelectTool);
                concreteSelectTool.shouldUseGrid = true;
                concreteSelectTool.shouldUseEllipse = false;
            }
            selectBtn.BorderBrush = new SolidColorBrush(Color.FromRgb(76, 194, 255));
            selectBtn.BorderThickness = new Thickness(1);
        }

        private void SelectFree_Click(object sender, RoutedEventArgs e)
        {
            savePreviewLayerToImageWhenSelectTool();
            deselectToolButtons();
            overlayTilesetSelection.Children.Clear();
            selectedTool = ToolMode.SelectFree;
            TheTool = selectTool;
            if (TheTool is SelectTool concreteSelectTool)
            {
                removeSelectionFromSelectTool(concreteSelectTool);
                concreteSelectTool.shouldUseGrid = false;
                concreteSelectTool.shouldUseEllipse = false;
            }
            selectFreeBtn.BorderBrush = new SolidColorBrush(Color.FromRgb(76, 194, 255));
            selectFreeBtn.BorderThickness = new Thickness(1);
        }

        private void SelectFreeOval_Click(object sender, RoutedEventArgs e)
        {
            savePreviewLayerToImageWhenSelectTool();
            deselectToolButtons();
            overlayTilesetSelection.Children.Clear();
            selectedTool = ToolMode.SelectFree;
            TheTool = selectTool;
            if (TheTool is SelectTool concreteSelectTool)
            {
                removeSelectionFromSelectTool(concreteSelectTool);
                concreteSelectTool.shouldUseEllipse = true;
                concreteSelectTool.shouldUseGrid = false;
            }
            selectOvalFreeBtn.BorderBrush = new SolidColorBrush(Color.FromRgb(76, 194, 255));
            selectOvalFreeBtn.BorderThickness = new Thickness(1);
        }

        public void Stamp_Click(object sender, RoutedEventArgs e)
        {
            savePreviewLayerToImageWhenSelectTool();
            overlayTilesetSelection.Children.Clear();
            deselectToolButtons();
            selectedTool = ToolMode.Stamp;
            stampBtn.BorderBrush = new SolidColorBrush(Color.FromRgb(76, 194, 255));
            stampBtn.BorderThickness = new Thickness(1);
            TheTool = stampTool;
        }

        private void ColorCircle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Content is Ellipse ellipse && ellipse.Fill is SolidColorBrush brush)
            {
                selectedBrush = brush;
                colorPickerBoxBtn.Background = selectedBrush;
            }
        }

        private void Bucket_Click(object sender, RoutedEventArgs e)
        {
            savePreviewLayerToImageWhenSelectTool();
            overlayTilesetSelection.Children.Clear();
            deselectToolButtons();
            bucketBtn.BorderBrush = new SolidColorBrush(Color.FromRgb(76, 194, 255));
            bucketBtn.BorderThickness = new Thickness(1);
            selectedTool = ToolMode.Bucket;
            TheTool = bucketTool;
        }

        private void ColorPickerBox_Click(object sender, RoutedEventArgs e)
        {
            deselectToolButtons();
            ColorPicker_Click(sender, e);
        }

        private void Eraser_Click(object sender, RoutedEventArgs e)
        {
            savePreviewLayerToImageWhenSelectTool();
            // Clear selection canvas
            SelectionRect = null;
            overlayTilesetSelection.Children.Clear();
            deselectToolButtons();
            eraserBtn.BorderBrush = new SolidColorBrush(Color.FromRgb(76, 194, 255));
            eraserBtn.BorderThickness = new Thickness(1);
            selectedTool = ToolMode.Eraser;
            TheTool = eraserTool;
        }

        private void Slurper_Click(object sender, RoutedEventArgs e)
        {
            savePreviewLayerToImageWhenSelectTool();
            // Clear selection canvas
            SelectionRect = null;
            overlayTilesetSelection.Children.Clear();
            deselectToolButtons();
            slurperBtn.BorderBrush = new SolidColorBrush(Color.FromRgb(76, 194, 255));
            slurperBtn.BorderThickness = new Thickness(1);
            selectedTool = ToolMode.Slurper;
            TheTool = slurperTool;
        }

        private void ResizeImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ResizeImageDialog(ImageWidth, ImageHeight);
            dialog.Owner = Application.Current.MainWindow;
            bool? result = dialog.ShowDialog(); // Blocks UI until closed

            if (result == true)
            {
                int newWidth = dialog.ImageWidth;
                int newHeight = dialog.ImageHeight;

                if ((newWidth != ImageWidth) || (newHeight != ImageHeight))
                {
                    var src = TileSetImage.Source as BitmapSource;
                    if (src == null)
                    {
                        MessageBox.Show("Image source is not set or not a BitmapSource.");
                    }
                    else
                    {
                        // Save the preview before resizing
                        savePreviewLayerToImageWhenSelectTool();
                        overlayTilesetSelection.Children.Clear();

                        // Remove the selection
                        if (selectTool is SelectTool concreteSelectTool)
                        {
                            removeSelectionFromSelectTool(concreteSelectTool);
                        }

                        // Resize the image and save
                        WriteableBitmap? shrunkImage = GraphicsUtils.resizeImageSource(TileSetImage, newWidth, newHeight);
                        if (shrunkImage != null)
                        {
                            TileSetImage.Source = shrunkImage;


                            ImageWidth = newWidth;
                            ImageHeight = newHeight;


                            GraphicsUtils.DrawGridOnCanvas(overlayTilesetGrid,
                                ImageWidth,
                                ImageHeight,
                                GridDimention,
                                System.Windows.Media.Brushes.Gray,
                                0.5);

                            // Redraw Checkerboard
                            CheckerboardBackground.Height = TileSetImage.Source.Height;
                            CheckerboardBackground.Width = TileSetImage.Source.Width;
                            GraphicsUtils.DrawCheckerboard(CheckerboardBackground, GridDimention);

                            int pixelWidth = shrunkImage.PixelWidth;
                            int pixelHeight = shrunkImage.PixelHeight;
                            imgDimensions.Text = pixelWidth + " x " + pixelHeight;
                        }

                    }
                }

            }
        }
    }
}
