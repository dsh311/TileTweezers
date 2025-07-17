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

        public EditorCell[,] TileMapArray { get; set; }
        public string TilesetPath { get; set; }

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

        public void RefreshLayers();
        public void ClearFilePath();

    }


}
