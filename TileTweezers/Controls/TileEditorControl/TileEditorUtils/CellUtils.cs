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

using _TileTweezers.Controls.TileEditorControl.TileEditorState;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Documents;

namespace _TileTweezers.Controls.TileEditorControl.TileEditorUtils
{
    public static class CellUtils
    {
        public class EditorCellArrayWrapper
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public int TileWidth { get; set; }
            public int TileHeight { get; set; }
            public int TileId {  get; set; }
            public EditorCell[][] Cells { get; set; } = default!;
        }

        public static string SaveToJson(EditorCell[,] array, int tilewidth, int tileheight)
        {
            // The map width in tiles and height in tiles
            int numRows = array.GetLength(0);
            int numCols = array.GetLength(1);

            var wrapper = new EditorCellArrayWrapper
            {
                Width = numCols,
                Height = numRows,
                TileWidth = tilewidth,
                TileHeight = tileheight,
                Cells = new EditorCell[numRows][]
            };

            for (int y = 0; y < numRows; y++)
            {
                // Create a row of cells
                wrapper.Cells[y] = new EditorCell[numCols];
                for (int x = 0; x < numCols; x++)
                {
                    wrapper.Cells[y][x] = array[y, x];
                }
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true,
            };

            string json = JsonSerializer.Serialize(wrapper, options);
            return json;
        }

        public static EditorCell[,] LoadFromJsonFile(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return LoadFromJsonString(json);
        }
        public static EditorCell[,] LoadFromJsonString(string json)
        {
            var wrapper = JsonSerializer.Deserialize<EditorCellArrayWrapper>(json)!;

            var result = new EditorCell[wrapper.Height, wrapper.Width];

            for (int y = 0; y < wrapper.Height; y++)
            {
                for (int x = 0; x < wrapper.Width; x++)
                {
                    result[y, x] = wrapper.Cells[y][x];
                }
            }

            return result;
        }

        public static EditorCell[,] CreateEmptyTileMapArray(int numGridRows, int numGridCols)
        {
            var array = new EditorCell[numGridRows, numGridCols];

            for (int y = 0; y < numGridRows; y++)
            {
                for (int x = 0; x < numGridCols; x++)
                {

                    // TMX tile IDs start at 1 and go row-major: left-to-right, top-to-bottom
                    EditorCell newGridCell = new EditorCell(-1, -1);
                    int tileId = ((y * numGridCols) + x) + 1;
                    newGridCell.TileId = tileId;

                    array[y, x] = newGridCell;
                }
            }

            return array;
        }

    }
}
