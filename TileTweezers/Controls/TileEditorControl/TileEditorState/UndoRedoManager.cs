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


using _TileTweezers.Controls.TileEditorControl.TileEditorUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO; // Path

namespace _TileTweezers.Controls.TileEditorControl.TileEditorState
{
    public class UndoRedoManager
    {
        public Stack<EditorState> undoStack = new();
        public Stack<EditorState> redoStack = new();

        public EditorState? currentState;

        public void SaveState(EditorState statePassedIn)
        {
            if (statePassedIn != null)
            {
                undoStack.Push(statePassedIn);

                currentState = statePassedIn;
                redoStack.Clear();
            }
        }

        public void Undo()
        {
            if (undoStack.Count != 0 && undoStack.Count != 1)
            {
                // The currentState holds what the user currently sees.
                // Save the current state to REDO stack so we can get it back.
                redoStack.Push(currentState);

                // Pop what the user sees since and discard it
                undoStack.Pop();

                // Set the currenState to the top of the stack
                currentState = undoStack.Peek();
            }
        }

        public void Redo()
        {
            if (redoStack.Count > 0)
            {
                currentState = redoStack.Pop();
                // Make sure the undoStack knows the current state
                undoStack.Push(currentState);
            }
        }

    }
}
