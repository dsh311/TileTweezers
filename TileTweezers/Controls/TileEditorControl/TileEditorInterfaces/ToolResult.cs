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

using System.Windows;
using System.Windows.Media; // Point, SolidColorBrush
using System.Windows.Media.Imaging; // WritableBitmap

namespace _TileTweezers.Controls.TileEditorControl.TileEditorInterfaces
{
    public class ToolResult
    {
        public bool Success { get; set; } = true;
        public Color? PickedColor { get; set; }
        public bool RedrawRequired { get; set; }

        public bool ShouldSaveForUndo { get; set; }
        public WriteableBitmap? SavedWritableBitmap { get; set; }

        public int FirstMouseDownLoc { get; set; }
        public int LastMouseMoveLoc { get; set; }

        public Int32Rect? SelectionRect { get; set; }


        public static readonly ToolResult None = new ToolResult
        {
            Success = false,
            RedrawRequired = false,
            ShouldSaveForUndo = false,
            SavedWritableBitmap = null,
            SelectionRect = null
        };

    }
}
