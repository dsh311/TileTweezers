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
using _TileTweezers.Interfaces;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls; //Image
using System.Windows.Input; // Mouse
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace _TileTweezers.Controls.TileEditorControl.TileEditorTools
{
    internal class StampTool : IPaintTool
    {
        public Point? MouseDownPointFirst { get; set; }
        public Point? MouseMovePointLast { get; set; }
        public bool MouseIsDown { get; set; }
        public bool shouldUseGrid { get; set; } = true;

        public WriteableBitmap? LastStampRemovedUnderSelectionAsBitmap { get; set; } = null;
        public Point LastStampRemovedUnderSelectionPoint { get; set; }


        private WriteableBitmap? getImageWithSelectedDeletion(ITileEditControl SourceTileControl, ImageSource deleteFromThisImage, Point topLeftCorner)
        {
            if (SourceTileControl.TheTool is StampSelectTool stampSelectTool && stampSelectTool.SelectionAsBitmap != null)
            {
                int srcWidth = (int)stampSelectTool.SelectionAsBitmap.Width;
                int srcHeight = (int)stampSelectTool.SelectionAsBitmap.Height;

                SolidColorBrush brushColor = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                WriteableBitmap sourceImage = GraphicsUtils.createColoredBitmap(srcWidth, srcHeight, brushColor);
                WriteableBitmap destinationImage = new WriteableBitmap((BitmapSource)deleteFromThisImage);

                GraphicsUtils.CopyImageRegion(
                    sourceImage,
                    0,
                    0,
                    destinationImage,
                    (int)topLeftCorner.Y,
                    (int)topLeftCorner.X,
                    srcWidth,
                    srcHeight,
                    1,
                    shouldBlend: false,
                    useEllipse: false
                );

                return destinationImage;
            }

            return null;
        }

        public void RestoreLastStampRemovedUnderSelection(ITileEditControl SourceTileControl, Image targetImage)
        {
            // Restore the last removed image under the selection when both we are dragging and not dragging
            if (LastStampRemovedUnderSelectionAsBitmap != null)
            {
                if (SourceTileControl.TheTool is StampSelectTool stampSelectTool && stampSelectTool.SelectionAsBitmap != null)
                {
                    int srcWidth = (int)stampSelectTool.SelectionAsBitmap.Width;
                    int srcHeight = (int)stampSelectTool.SelectionAsBitmap.Height;

                    WriteableBitmap sourceImageNeededRestore = new WriteableBitmap((BitmapSource)targetImage.Source);
                    GraphicsUtils.CopyImageRegion(
                        LastStampRemovedUnderSelectionAsBitmap,
                        0,
                        0,
                        sourceImageNeededRestore,
                        (int)LastStampRemovedUnderSelectionPoint.Y,
                        (int)LastStampRemovedUnderSelectionPoint.X,
                        srcWidth,
                        srcHeight,
                        1,
                        shouldBlend: false,
                        useEllipse: false
                    );

                    targetImage.Source = sourceImageNeededRestore;
                }

                // Now that we restored what was under the stamp tool, we can remove the saved image
                LastStampRemovedUnderSelectionAsBitmap = null;
            }
        }

        public ToolResult OnMouseDownStamp(ITileEditControl SourceTileControl, Image targetImage, Image previewImage, Canvas overlaySelectionCanvas, EditorCell[,] tileMapArray, Point position, int gridDimension, SolidColorBrush brushColor)
        {
            //Only continue if the grid select tool is chosen in the source
            bool shouldContinueWithStampSelect = (SourceTileControl?.TheTool is StampSelectTool concreteStampSelectTool && concreteStampSelectTool.shouldUseGrid);
            if (!shouldContinueWithStampSelect)
            {
                return ToolResult.None;
            }

            MouseDownPointFirst = position;

            if (shouldUseGrid)
            {
                position.X = (gridDimension * ((int)Math.Floor((position.X / gridDimension)) + 1));
                position.Y = (gridDimension * ((int)Math.Floor((position.Y / gridDimension)) + 1));
            }

            MouseIsDown = true;

            ToolResult returnResult = new ToolResult();
            returnResult.Success = false;

            if (SourceTileControl.TheTool is StampSelectTool stampSelectTool &&
                stampSelectTool.SelectionAsBitmap != null &&
                SourceTileControl.OutputImage != null &&
                stampSelectTool.MouseDownPointFirst != null &&
                stampSelectTool.MouseMovePointLast != null)
            {
                int srcWidth = (int)stampSelectTool.SelectionAsBitmap.Width;
                int srcHeight = (int)stampSelectTool.SelectionAsBitmap.Height;

                // The current mouse position is the top left so we figure out where the mouse would be to center the image
                double selectionBitmap_HalfWidth = ((int)stampSelectTool.SelectionAsBitmap.Width / 2);
                double selectionBitmap_HalfHeight = ((int)stampSelectTool.SelectionAsBitmap.Height / 2);

                double finalTopLeftX = position.X;
                double finalTopLeftY = position.Y;

                if (shouldUseGrid)
                {
                    finalTopLeftX = (gridDimension * ((int)Math.Floor(((position.X - selectionBitmap_HalfWidth) / gridDimension)) ));
                    finalTopLeftY = (gridDimension * ((int)Math.Floor(((position.Y - selectionBitmap_HalfHeight) / gridDimension)) ));
                }

                // The left side must have the SelectTool selected
                bool xAreWithinBoard = (int)finalTopLeftX >= 0 && ((int)finalTopLeftX + srcWidth <= targetImage.Source.Width);
                bool yAreWithinBoard = (int)finalTopLeftY >= 0 && ((int)finalTopLeftY + srcHeight <= targetImage.Source.Height);
                bool selectRectWithinImage = (xAreWithinBoard && yAreWithinBoard);

                if (selectRectWithinImage)
                {

                    WriteableBitmap destinationImage = new WriteableBitmap((BitmapSource)targetImage.Source);
                    GraphicsUtils.CopyImageRegion(
                        stampSelectTool.SelectionAsBitmap,
                        0,
                        0,
                        destinationImage,
                        (int)finalTopLeftY,
                        (int)finalTopLeftX,
                        srcWidth,
                        srcHeight,
                        1,
                        shouldBlend: false,
                        useEllipse: false
                    );

                    targetImage.Source = destinationImage;

                    // Clear the preview image so the image we just dropped doesn't have a preview above it
                    // When the preview above stays around, it looks off
                    GraphicsUtils.transparentImage(previewImage);

                    // Save the changes to the array
                    Point topLeftLocOfTilemap = new Point(finalTopLeftX, finalTopLeftY);
                    (int tileMapRowStart, int tileMapColStart) = GraphicsUtils.GetGridXYFromPosition(targetImage, topLeftLocOfTilemap, gridDimension);

                    int numColsInSelection = srcWidth / gridDimension;
                    int numRowsInSelection = srcHeight / gridDimension;

                    Point topLeftSelectionPoint = new Point(stampSelectTool.MouseDownPointFirst.Value.X, stampSelectTool.MouseDownPointFirst.Value.Y);
                    Point botRightSelectionPoint = new Point(stampSelectTool.MouseMovePointLast.Value.X, stampSelectTool.MouseMovePointLast.Value.Y);
                    (int topLeftGridRowOfTilesetSelection, int topLeftGridColumnOfTilesetSelection) = GraphicsUtils.GetGridXYFromPosition(SourceTileControl.OutputImage, topLeftSelectionPoint, gridDimension);
                    (int botRightGridRowOfTilesetSelection, int botRightGridColumnOfTilesetSelection) = GraphicsUtils.GetGridXYFromPosition(SourceTileControl.OutputImage, botRightSelectionPoint, gridDimension);


                    // Move from top row to bottom row
                    for (int curRowCounter = 0; curRowCounter < numRowsInSelection; curRowCounter++)
                    {
                        int actualCurRow = curRowCounter + tileMapRowStart;
                        int actualTilesetCurRow = curRowCounter + topLeftGridRowOfTilesetSelection;

                        // Move left to right through the row columns
                        for (int curColCounter = 0; curColCounter < numColsInSelection; curColCounter++)
                        {
                            int actualCurCol = curColCounter + tileMapColStart;
                            int actualTilesetCurCol = curColCounter + topLeftGridColumnOfTilesetSelection;

                            // Reference the corresponding location on the orginal tileset image
                            EditorCell saveThisCell = new EditorCell(actualTilesetCurRow, actualTilesetCurCol);
                            saveThisCell.IsEmpty = false;
                            // Save the TileId that was given when array initialized
                            saveThisCell.TileId = tileMapArray[actualCurRow, actualCurCol].TileId;
                            tileMapArray[actualCurRow, actualCurCol] = saveThisCell;
                        }
                    }

                }

            }

            return ToolResult.None;
        }

        public ToolResult OnMouseUp(Image targetImage, Image previewImage, Canvas overlaySelectionCanvas, EditorCell[,] tileMapArray, Point position, int gridDimension, SolidColorBrush brushColor)
        {
            MouseIsDown = false;
            LastStampRemovedUnderSelectionAsBitmap = null;

            return ToolResult.None;
        }

        public ToolResult OnMouseMoveStamp(ITileEditControl SourceTileControl, Image targetImage, Image previewImage, Canvas overlaySelectionCanvas, EditorCell[,] tileMapArray, Point position, int gridDimension, SolidColorBrush brushColor)
        {
            //Only continue if the grid select tool is chosen in the source
            bool shouldContinueWithStamp = (SourceTileControl?.TheTool is StampSelectTool concreteStampSelectTool && concreteStampSelectTool.shouldUseGrid);
            if (!shouldContinueWithStamp)
            {
                return ToolResult.None;
            }

            if (shouldUseGrid)
            {
                position.X = (gridDimension * ((int)Math.Floor((position.X / gridDimension)) + 1));
                position.Y = (gridDimension * ((int)Math.Floor((position.Y / gridDimension)) + 1));
            }

            MouseMovePointLast = position;

            // Clear the preview image so we can draw on it
            GraphicsUtils.transparentImage(previewImage);
            // Save the cleared image
            WriteableBitmap destinationImage = null;

            if (MouseIsDown)
            {
                destinationImage = new WriteableBitmap((BitmapSource)targetImage.Source);
            }
            else
            {
                destinationImage = new WriteableBitmap((BitmapSource)previewImage.Source);
            }

            if (SourceTileControl.TheTool is StampSelectTool stampSelectTool && stampSelectTool.SelectionAsBitmap != null)
            {
                int srcWidth = (int)stampSelectTool.SelectionAsBitmap.Width;
                int srcHeight = (int)stampSelectTool.SelectionAsBitmap.Height;

                // The current mouse position is the top left so we figure out where the mouse would be to center the image
                double selectionBitmap_HalfWidth = ((int)stampSelectTool.SelectionAsBitmap.Width / 2);
                double selectionBitmap_HalfHeight = ((int)stampSelectTool.SelectionAsBitmap.Height / 2);

                double finalTopLeftX = position.X;
                double finalTopLeftY = position.Y;

                if (shouldUseGrid)
                {
                    finalTopLeftX = (gridDimension * ((int)Math.Floor(((position.X - selectionBitmap_HalfWidth) / gridDimension)) ));
                    finalTopLeftY = (gridDimension * ((int)Math.Floor(((position.Y - selectionBitmap_HalfHeight) / gridDimension)) ));
                }


                bool xAreWithinBoard = (int)finalTopLeftX >= 0 && ((int)finalTopLeftX + srcWidth <= targetImage.Source.Width);
                bool yAreWithinBoard = (int)finalTopLeftY >= 0 && ((int)finalTopLeftY + srcHeight <= targetImage.Source.Height);
                bool selectRectWithinImage = (xAreWithinBoard && yAreWithinBoard);

                RestoreLastStampRemovedUnderSelection(SourceTileControl, targetImage);

                // What is selected can be show under the stamp tool on the right side
                if (selectRectWithinImage)
                {
                    Mouse.OverrideCursor = Cursors.SizeAll;

                    if (!MouseIsDown)
                    {
                        // Save what is under the stamp tool when not dragging meaning simply mouse moving
                        Point topLeftCorner = new Point(finalTopLeftX, finalTopLeftY);
                        LastStampRemovedUnderSelectionPoint = new Point(finalTopLeftX, finalTopLeftY);

                        // Save the image region under the selection when mouse is up
                        SolidColorBrush tempBrushColor = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                        LastStampRemovedUnderSelectionAsBitmap = GraphicsUtils.createColoredBitmap(srcWidth, srcHeight, tempBrushColor);

                        GraphicsUtils.CopyImageRegion(
                            (WriteableBitmap)targetImage.Source,
                            (int)finalTopLeftY,
                            (int)finalTopLeftX,
                            LastStampRemovedUnderSelectionAsBitmap,
                            0,
                            0,
                            srcWidth,
                            srcHeight,
                            1,
                            shouldBlend: false,
                            useEllipse: false
                        );

                        // Save source without what is under the stamp
                        targetImage.Source = getImageWithSelectedDeletion(SourceTileControl, (BitmapSource)targetImage.Source, topLeftCorner);
                    }

                    GraphicsUtils.CopyImageRegion(
                        stampSelectTool.SelectionAsBitmap,
                        0,
                        0,
                        destinationImage,
                        (int)finalTopLeftY,
                        (int)finalTopLeftX,
                        srcWidth,
                        srcHeight,
                        1,
                        shouldBlend: false,
                        useEllipse: false
                    );

                    if (MouseIsDown)
                    {
                        targetImage.Source = destinationImage;

                        // Save the changes to the array
                        Point topLeftLocOfTilemap = new Point(finalTopLeftX, finalTopLeftY);
                        (int tileMapRowStart, int tileMapColStart) = GraphicsUtils.GetGridXYFromPosition(targetImage, topLeftLocOfTilemap, gridDimension);

                        int numColsInSelection = srcWidth / gridDimension;
                        int numRowsInSelection = srcHeight / gridDimension;

                        Point topLeftSelectionPoint = new Point(stampSelectTool.MouseDownPointFirst.Value.X, stampSelectTool.MouseDownPointFirst.Value.Y);
                        Point botRightSelectionPoint = new Point(stampSelectTool.MouseMovePointLast.Value.X, stampSelectTool.MouseMovePointLast.Value.Y);
                        (int topLeftGridRowOfTilesetSelection, int topLeftGridColumnOfTilesetSelection) = GraphicsUtils.GetGridXYFromPosition(SourceTileControl.OutputImage, topLeftSelectionPoint, gridDimension);
                        (int botRightGridRowOfTilesetSelection, int botRightGridColumnOfTilesetSelection) = GraphicsUtils.GetGridXYFromPosition(SourceTileControl.OutputImage, botRightSelectionPoint, gridDimension);


                        // Move from top row to bottom row
                        for (int curRowCounter = 0; curRowCounter < numRowsInSelection; curRowCounter++)
                        {
                            int actualCurRow = curRowCounter + tileMapRowStart;
                            int actualTilesetCurRow = curRowCounter + topLeftGridRowOfTilesetSelection;

                            // Move left to right through the row columns
                            for (int curColCounter = 0; curColCounter < numColsInSelection; curColCounter++)
                            {
                                int actualCurCol = curColCounter + tileMapColStart;
                                int actualTilesetCurCol = curColCounter + topLeftGridColumnOfTilesetSelection;
                                EditorCell saveThisCell = new EditorCell(actualTilesetCurRow, actualTilesetCurCol);
                                saveThisCell.TileId = tileMapArray[actualCurRow, actualCurCol].TileId;
                                saveThisCell.IsEmpty = false;
                                tileMapArray[actualCurRow, actualCurCol] = saveThisCell;

                            }
                        }
                    }
                    else
                    {
                        previewImage.Source = destinationImage;
                    }
                }
                else
                {
                    Mouse.OverrideCursor = Cursors.No;
                    GraphicsUtils.transparentImage(previewImage);
                }
            }

            return ToolResult.None;
        }

        public ToolResult OnMouseLeaveStamp(ITileEditControl SourceTileControl, Image targetImage, Image previewImage, Canvas overlaySelectionCanvas, EditorCell[,] tileMapArray, Point position, int gridDimension, SolidColorBrush brushColor)
        {
            //Only continue if the grid select tool is chosen in the source
            bool shouldContinueWithStampSelect = (SourceTileControl?.TheTool is StampSelectTool concreteStampSelectTool && concreteStampSelectTool.shouldUseGrid);
            if (!shouldContinueWithStampSelect)
            {
                return ToolResult.None;
            }

            RestoreLastStampRemovedUnderSelection(SourceTileControl, targetImage);

            GraphicsUtils.transparentImage(previewImage);
            MouseIsDown = false;
            return ToolResult.None;
        }

        public ToolResult OnMouseDown(Image targetImage, Image previewImage, Canvas overlaySelectionCanvas, EditorCell[,] tileMapArray, Point position, int gridDimension, SolidColorBrush brushColor)
        {
            return ToolResult.None;
        }

        public ToolResult OnMouseMove(Image targetImage, Image previewImage, Canvas overlaySelectionCanvas, EditorCell[,] tileMapArray, Point position, int gridDimension, SolidColorBrush brushColor)
        {
            return ToolResult.None;
        }

        public ToolResult OnMouseLeave(Image targetImage, Image previewImage, Canvas overlaySelectionCanvas, EditorCell[,] tileMapArray, Point position, int gridDimension, SolidColorBrush brushColor)
        {
            return ToolResult.None;
        }

    }
}
