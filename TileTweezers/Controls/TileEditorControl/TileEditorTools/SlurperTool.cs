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
using _TileTweezers.Controls.TileEditorControl.TileEditorUtils;
using System.Windows;
using System.Windows.Controls; //Image
using System.Windows.Media;


namespace _TileTweezers.Controls.TileEditorControl.TileEditorTools
{
    internal class SlurperTool : IPaintTool
    {
        public Point? MouseDownPointFirst { get; set; }
        public Point? MouseMovePointLast { get; set; }
        public bool MouseIsDown { get; set; }


        public ToolResult OnMouseDown(Image targetImage, Image previewImage, Canvas overlaySelectionCanvas, Point position, int gridDimension, SolidColorBrush brushColor)
        {
            MouseDownPointFirst = position;
            MouseIsDown = true;

            ToolResult returnResult = new ToolResult();
            returnResult.Success = false;

            SolidColorBrush? slurpedColor = GraphicsUtils.GetColorAtPoint(targetImage, position);
            // Only slurp if the color is visible
            if (slurpedColor != null && slurpedColor.Color.A > 0)
            {
                returnResult.PickedColor = slurpedColor.Color;
                returnResult.Success = true;
            }

            return returnResult;
        }

        public ToolResult OnMouseUp(Image targetImage, Image previewImage, Canvas overlaySelectionCanvas, Point position, int gridDimension, SolidColorBrush brushColor)
        {
            MouseIsDown = false;
            return ToolResult.None;
        }

        public ToolResult OnMouseMove(Image targetImage, Image previewImage, Canvas overlaySelectionCanvas, Point position, int gridDimension, SolidColorBrush brushColor)
        {
            MouseMovePointLast = position;

            ToolResult returnResult = new ToolResult();
            returnResult.Success = false;

            SolidColorBrush? slurpedColor = GraphicsUtils.GetColorAtPoint(targetImage, position);
            // Only slurp if the color is visible
            if (slurpedColor != null && slurpedColor.Color.A > 0)
            {
                returnResult.PickedColor = slurpedColor.Color;
                returnResult.Success = true;
            }

            return returnResult;
        }

        public ToolResult OnMouseLeave(Image targetImage, Image previewImage, Canvas overlaySelectionCanvas, Point position, int gridDimension, SolidColorBrush brushColor)
        {
            return ToolResult.None;
        }

    }
}
