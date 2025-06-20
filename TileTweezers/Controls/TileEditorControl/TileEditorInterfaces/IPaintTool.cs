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

using System.Windows; // Point
using System.Windows.Controls; //Image
using System.Windows.Media; // SolidColorBrush


namespace _TileTweezers.Controls.TileEditorControl.TileEditorInterfaces
{
    public interface IPaintTool
    {
        public bool MouseIsDown { get; set; }
        public Point? MouseDownPointFirst { get; set; }
        public Point? MouseMovePointLast { get; set; }

        ToolResult OnMouseDown(Image targetImage, Image previewImage, Canvas overlaySelectionCanvas, Point position, int GridDimension, SolidColorBrush brushColor);

        ToolResult OnMouseUp(Image targetImage, Image previewImage, Canvas overlaySelectionCanvas, Point position, int GridDimension, SolidColorBrush brushColor);

        ToolResult OnMouseMove(Image targetImage, Image previewImage, Canvas overlaySelectionCanvas, Point position, int GridDimension, SolidColorBrush brushColor);

        ToolResult OnMouseLeave(Image targetImage, Image previewImage, Canvas overlaySelectionCanvas, Point position, int GridDimension, SolidColorBrush brushColor);
    }
}
