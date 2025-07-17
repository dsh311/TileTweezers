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
using _TileTweezers.Controls.TileEditorControl.TileEditorUtils;
using System.Windows;
using System.Windows.Controls; //Image
using System.Windows.Media; // Point, SolidColorBrush
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System; // Debug

namespace _TileTweezers.Controls.TileEditorControl.TileEditorTools
{
    internal class PencilTool : IPaintTool
    {
        public Point? MouseDownPointFirst { get; set; }
        public Point? MouseMovePointLast { get; set; }
        public bool MouseIsDown { get; set; }
        public WriteableBitmap? LocalSavedWritableBitmap { get; set; }


        public ToolResult OnMouseDown(Image targetImage, Image previewImage, Canvas overlaySelectionCanvas, EditorCell[,] tileMapArray, Point position, int gridDimension, SolidColorBrush brushColor)
        {
            MouseDownPointFirst = position;
            MouseIsDown = true;

            GraphicsUtils.DrawPixelOnImage(targetImage, (int)position.Y, (int)position.X, brushColor.Color);

            return ToolResult.None;
        }

        public ToolResult OnMouseUp(Image targetImage, Image previewImage, Canvas overlaySelectionCanvas, EditorCell[,] tileMapArray, Point position, int gridDimension, SolidColorBrush brushColor)
        {
            MouseIsDown = false;

            Point lastPoint = (MouseMovePointLast != null) ? MouseMovePointLast.Value : position;
            GraphicsUtils.DrawLineOnImage(targetImage, lastPoint, position, brushColor.Color);
            MouseMovePointLast = null;
            MouseDownPointFirst = null;

            ToolResult returnResult = new ToolResult();
            returnResult.Success = true;
            returnResult.ShouldSaveForUndo = true;

            return returnResult;
        }

        public ToolResult OnMouseMove(Image targetImage, Image previewImage, Canvas overlaySelectionCanvas, EditorCell[,] tileMapArray, Point position, int gridDimension, SolidColorBrush brushColor)
        {
            Point lastPoint = (MouseMovePointLast != null) ? MouseMovePointLast.Value : position;

            if (MouseIsDown)
            {
                GraphicsUtils.DrawLineOnImage(targetImage, lastPoint, position, brushColor.Color);
            }
            MouseMovePointLast = position;

            return ToolResult.None;
        }

        public ToolResult OnMouseLeave(Image targetImage, Image previewImage, Canvas overlaySelectionCanvas, EditorCell[,] tileMapArray, Point position, int gridDimension, SolidColorBrush brushColor)
        {
            return ToolResult.None;
        }
    }
}
