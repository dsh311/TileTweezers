using _TileTweezers.Controls.TileEditorControl.TileEditorInterfaces;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
namespace _TileTweezers.Interfaces
{
    public interface ITileEditControl
    {
        Image? InputImage { get; set; }
        Image? InputImagePreview { get; set; }
        Image? OutputImage { get; set; }
        Rectangle? SelectionRect { get; set; }
        WriteableBitmap? SelectionAsBitmap { get; set; }


        ITileEditControl? SourceTileControl { get; set; }
        ITileEditControl? DestinationTileControl { get; set; }

        string TilesetPath { get; set; }

        Point MouseOverLocation { get; set; }

        int MouseOverGridRow { get; set; }
        int MouseOverGridCol { get; set; }

        int ImageWidth { get; set; }
        int ImageHeight { get; set; }

        int GridDimention { get; set; }

        int SelectedTilesetRowStart { get; set; }
        int SelectedTilesetColumnStart { get; set; }
        int SelectedTilesetRowEnd { get; set; }
        int SelectedTilesetColumnEnd { get; set; }

        int SelectedTilesetRowMouseDownOffset { get; set; }
        int SelectedTilesetColumnMouseDownOffset { get; set; }

        bool IsDragging { get; set; }
        bool IsDraggingSelection { get; set; }

        public IPaintTool? TheTool { get; set; }

        public void SetGridDimension(int gridDimension);

    }


}
