using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _TileTweezers.Controls.TileEditorControl.TileEditorState
{
    public class EditorCell
    {
        public int TileId { get; set; }

        // Reference to the tile from the tileset (e.g., row and column in tileset grid)
        public int TilesetRow { get; set; }
        public int TilesetColumn { get; set; }

        // Rotation in degrees (e.g., 0, 90, 180, 270)
        public int Rotation { get; set; } = 0;

        // Optional pixel offset when drawing the tile
        public int OffsetX { get; set; } = 0;
        public int OffsetY { get; set; } = 0;

        // Optional: Flip flags
        public bool FlipHorizontal { get; set; } = false;
        public bool FlipVertical { get; set; } = false;

        // Optional: Is this cell empty?
        public bool IsEmpty { get; set; } = true;

        // Optional constructor
        public EditorCell(int tilesetRow, int tilesetColumn)
        {
            IsEmpty = true;
            TilesetRow = tilesetRow;
            TilesetColumn = tilesetColumn;
        }
    }
}
