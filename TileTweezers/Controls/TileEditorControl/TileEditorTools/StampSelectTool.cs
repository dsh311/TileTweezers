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
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls; //Image
using System.Windows.Input; // Mouse
using System.Windows.Media; // Point, SolidColorBrush
using System.Windows.Media.Imaging;

namespace _TileTweezers.Controls.TileEditorControl.TileEditorTools
{
    internal class StampSelectTool : IPaintTool
    {
        public Point? MouseDownPointFirst { get; set; }
        public Point? MouseMovePointLast { get; set; }
        public bool MouseIsDown { get; set; }

        public WriteableBitmap? SelectionAsBitmap { get; set; } = null;
        public WriteableBitmap? LastValidSelectionAsBitmap { get; set; } = null;

        public bool shouldUseEllipse { get; set; } = false;
        public bool shouldUseGrid { get; set; } = true;
        public int useThisGridDimension { get; set; }

        public bool IsDraggingSelection = false;

        public Point? SelectionMouseDownOffset { get; set; }

        public Int32Rect? SelectionRect { get; set; }
        public Int32Rect? LastValidSelectionRect { get; set; }

        public void EscapeSelection(Image targetImage, Image previewImage, Canvas overlaySelectionCanvas)
        {
            //saveValidSelectionBitmapToImageLayer(targetImage, previewImage);

            SelectionRect = null;
            LastValidSelectionRect = null;
            SelectionAsBitmap = null;
            LastValidSelectionAsBitmap = null;
            IsDraggingSelection = false;
            shouldUseGrid = true;

            //Clear the selection
            overlaySelectionCanvas.Children.Clear();
        }

        public void SelectAll(Image targetImage, Image previewImage, Canvas overlaySelectionCanvas, EditorCell[,] tileMapArray)
        {
            EscapeSelection(targetImage, previewImage, overlaySelectionCanvas);
            // Force select all to use Grid Select
            shouldUseEllipse = false;

            var targetAsBitmap = targetImage.Source as BitmapSource;
            if (targetAsBitmap != null)
            {
                Point topLeftPoint = new Point(0, 0);

                int pixelWidth = targetAsBitmap.PixelWidth;
                int pixelHeight = targetAsBitmap.PixelHeight;
                int lastXOnFullGrid = useThisGridDimension * ((int)Math.Floor(((decimal)pixelWidth / useThisGridDimension)));
                int lastYOnFullGrid = useThisGridDimension * ((int)Math.Floor(((decimal)pixelHeight / useThisGridDimension)));
                Point bottomRightPoint = new Point(lastXOnFullGrid, lastYOnFullGrid);

                MouseDownPointFirst = topLeftPoint;
                MouseMovePointLast = bottomRightPoint;

                // Draw the rectangle so the OnMouseUp will create the SelectionRect.
                SolidColorBrush fillColor = new SolidColorBrush(Color.FromArgb(80, 0, 255, 255));
                GraphicsUtils.DrawSelectionRectangle(
                    overlaySelectionCanvas,
                    (int)topLeftPoint.Y,
                    (int)topLeftPoint.X,
                    (int)bottomRightPoint.Y,
                    (int)bottomRightPoint.X,
                    1,
                    fillColor,
                    false);

                MouseIsDown = true;
                ToolResult theResult = OnMouseUp(targetImage, previewImage, overlaySelectionCanvas, tileMapArray, bottomRightPoint, useThisGridDimension, fillColor);
            }
        }

        public ToolResult OnMouseDown(Image targetImage, Image previewImage, Canvas overlaySelectionCanvas, EditorCell[,] tileMapArray, Point position, int gridDimension, SolidColorBrush brushColor)
        {
            MouseIsDown = true;
            Point originalMouseLoc = new Point(position.X, position.Y);

            // If using grid, this will make the first point down snap to a grid
            if (shouldUseGrid)
            {
                position.X = useThisGridDimension * ((int)Math.Floor((position.X / useThisGridDimension)));
                position.Y = useThisGridDimension * ((int)Math.Floor((position.Y / useThisGridDimension)));
            }

            ToolResult returnResult = new ToolResult();
            returnResult.Success = true;


            bool mousedownWithinSelectionRect = false;

            // Did user click down on selection? If so, save the actual offset in pixels
            if (SelectionRect != null)
            {
                // Remember where the user clicked down when within the selection
                bool withinRectWidth = (originalMouseLoc.X >= SelectionRect.Value.X) && (originalMouseLoc.X <= SelectionRect.Value.X + SelectionRect.Value.Width);
                bool withinRectHeight = (originalMouseLoc.Y >= SelectionRect.Value.Y) && (originalMouseLoc.Y <= SelectionRect.Value.Y + SelectionRect.Value.Height);
                mousedownWithinSelectionRect = (withinRectWidth & withinRectHeight);

                // If mousedown within selection, save the non-grid locked mousedown offset in pixels
                if (mousedownWithinSelectionRect)
                {
                    // The MouseDownPointFirst in the MouseUp is modified to always be the top left of the rect 
                    int selectYoffset = (int)originalMouseLoc.Y - (int)SelectionRect.Value.Y;
                    int selectXoffset = (int)originalMouseLoc.X - (int)SelectionRect.Value.X;
                    SelectionMouseDownOffset = new Point(selectXoffset, selectYoffset);
                }
                else
                {
                    SelectionMouseDownOffset = null;
                }
            }
            else
            {
                // If there is no selection then there is no mouse down within the selection
                mousedownWithinSelectionRect = false;
            }

            // If they click down outside of a selection or selection doesn't exist
            // Mouse Down is fired once, so this is the first time they clicked to drag or clicked away from selection
            if (!mousedownWithinSelectionRect)
            {
                returnResult.ShouldSaveForUndo = false;

                returnResult.SelectionRect = SelectionRect;
                SelectionRect = null;
                SelectionAsBitmap = null;
                IsDraggingSelection = false;

                MouseDownPointFirst = position;
                MouseMovePointLast = position;
            }

            return returnResult;
        }

        // SelectionRect is updated here when user is dragging
        public ToolResult OnMouseMove(Image targetImage, Image previewImage, Canvas overlaySelectionCanvas, EditorCell[,] tileMapArray, Point position, int gridDimension, SolidColorBrush brushColor)
        {
            if (shouldUseGrid)
            {
                position.X = (useThisGridDimension * ((int)Math.Floor((position.X / useThisGridDimension)) + 1));
                position.Y = (useThisGridDimension * ((int)Math.Floor((position.Y / useThisGridDimension)) + 1));
            }

            MouseMovePointLast = position;

            ToolResult returnResult = new ToolResult();
            returnResult.Success = false;

            // User can drag while there is no SelectionRect yet
            if (MouseIsDown)
            {
                //Clear the cursor title
                overlaySelectionCanvas.Children.Clear();

                // If was mouse down in selection in the OnMouseDown
                if (SelectionMouseDownOffset != null)
                {

                    // Next time, this will be true, not the first time
                    // We do this since the first time, we want to cut the selected rect from the TileSetImage
                    // The second time, we want to cut the iamge from the preview image
                    IsDraggingSelection = true;

                    // If we have coordinates for a rectangle and we have already saved a bitmap at those coords
                    // SelectionAsBitmap is saved when the user mouseups after using the Select tool
                    if (SelectionAsBitmap != null)
                    {
                        int srcWidth = (int)SelectionAsBitmap.Width;
                        int srcHeight = (int)SelectionAsBitmap.Height;

                        int topLeftYOfSelection = (int)position.Y - (int)SelectionMouseDownOffset.Value.Y;
                        int topLeftXOfSelection = (int)position.X - (int)SelectionMouseDownOffset.Value.X;

                        //Snap to the grid
                        if (shouldUseGrid)
                        {
                            topLeftXOfSelection = (useThisGridDimension * ((int)Math.Floor(((decimal)topLeftXOfSelection / useThisGridDimension))));
                            topLeftYOfSelection = (useThisGridDimension * ((int)Math.Floor(((decimal)topLeftYOfSelection / useThisGridDimension))));
                        }

                        MouseDownPointFirst = new Point(topLeftXOfSelection, topLeftYOfSelection);

                        int lastPointY = topLeftYOfSelection + srcHeight;
                        int lastPointX = topLeftXOfSelection + srcWidth;
                        MouseMovePointLast = new Point(lastPointX, lastPointY);

                        SolidColorBrush fillColor = new SolidColorBrush(Color.FromArgb(80, 0, 255, 255));
                        GraphicsUtils.DrawSelectionRectangle(
                            overlaySelectionCanvas,
                            (int)MouseDownPointFirst.Value.Y,
                            (int)MouseDownPointFirst.Value.X,
                            (int)MouseMovePointLast.Value.Y,
                            (int)MouseMovePointLast.Value.X,
                            1,
                            fillColor,
                            shouldUseEllipse);


                        // Save the new rect now that we drew the new selection above
                        int rectWidth = (Math.Abs((int)MouseMovePointLast.Value.X - (int)MouseDownPointFirst.Value.X));
                        int rectHeight = (Math.Abs((int)MouseMovePointLast.Value.Y - (int)MouseDownPointFirst.Value.Y));
                        // The 2nd place the SelectionRect values are changed. 1st place is mouseup
                        SelectionRect = new Int32Rect(x: (int)MouseDownPointFirst.Value.X, y: (int)MouseDownPointFirst.Value.Y, width: rectWidth, height: rectHeight);
                    }

                }

                // If they haven't clicked down in a selection then they must be dragging to create a selection
                if (SelectionMouseDownOffset == null)
                {
                    //Update the SelectionRect
                    SolidColorBrush fillColor = new SolidColorBrush(Color.FromArgb(80, 0, 255, 255));

                    GraphicsUtils.DrawSelectionRectangle(
                        overlaySelectionCanvas,
                        (int)MouseDownPointFirst.Value.Y,
                        (int)MouseDownPointFirst.Value.X,
                        (int)MouseMovePointLast.Value.Y,
                        (int)MouseMovePointLast.Value.X,
                        1,
                        fillColor,
                        shouldUseEllipse);

                    //Since the selection rect
                    int rectWidth = (Math.Abs((int)MouseMovePointLast.Value.X - (int)MouseDownPointFirst.Value.X));
                    int rectHeight = (Math.Abs((int)MouseMovePointLast.Value.Y - (int)MouseDownPointFirst.Value.Y));

                    returnResult.SelectionRect = new Int32Rect(
                                                        x: (int)MouseDownPointFirst.Value.X,
                                                        y: (int)MouseDownPointFirst.Value.Y,
                                                        width: rectWidth,
                                                        height: rectHeight
                    );

                } // End not within the existing rect

            } // End if mouse down while dragging


            // Detect when mouse is within an already created selection
            if (!MouseIsDown)
            {
                // If there is a selection then check if mouse over the selection
                if (SelectionRect != null)
                {
                    bool withinRectWidth = (position.X >= SelectionRect.Value.X) && (position.X <= SelectionRect.Value.X + SelectionRect.Value.Width);
                    bool withinRectHeight = (position.Y >= SelectionRect.Value.Y) && (position.Y <= SelectionRect.Value.Y + SelectionRect.Value.Height);

                    // Set cursor if we are within the selection rect
                    if (withinRectWidth && withinRectHeight)
                    {
                        // Moving is the only way to get into the selection rect
                        Mouse.OverrideCursor = Cursors.SizeAll;
                    }
                    else
                    {
                        // Moving is the only way to get out of the selection rect
                        Mouse.OverrideCursor = Cursors.Cross;
                    }
                }
            }


            return returnResult;
        }

        // SelectionRect is initially created here when user mouse ups
        // A SelectionRect could already exist meaning we could be dragging the selection
        // Create the SelectionAsBitmap
        public ToolResult OnMouseUp(Image targetImage, Image previewImage, Canvas overlaySelectionCanvas, EditorCell[,] tileMapArray, Point position, int gridDimension, SolidColorBrush brushColor)
        {
            if (shouldUseGrid)
            {
                position.X = (useThisGridDimension * ((int)Math.Ceiling((position.X / useThisGridDimension))));
                position.Y = (useThisGridDimension * ((int)Math.Ceiling((position.Y / useThisGridDimension))));
            }

            MouseMovePointLast = position;

            SelectionMouseDownOffset = null;

            ToolResult returnResult = new ToolResult();
            returnResult.Success = false;

            int futureRectWidth = MouseDownPointFirst != null ? (Math.Abs((int)MouseMovePointLast.Value.X - (int)MouseDownPointFirst.Value.X)) : 0;
            int fugureRectHeight = MouseDownPointFirst != null ? (Math.Abs((int)MouseMovePointLast.Value.Y - (int)MouseDownPointFirst.Value.Y)) : 0;

            if (!IsDraggingSelection)
            {
                // Ensure the user didn't mouse down in the right pane then mouseup in this page, which would mean there is no MouseDownPointFirst
                bool selectionAreaTooSmall = futureRectWidth <= 0 || fugureRectHeight <= 0;
                if (selectionAreaTooSmall || overlaySelectionCanvas.Children.Count == 0)
                {
                    overlaySelectionCanvas.Children.Clear();
                    SelectionRect = null;
                    IsDraggingSelection = false;
                    LastValidSelectionAsBitmap = null;
                    LastValidSelectionRect = null;
                    MouseIsDown = false;
                    SelectionMouseDownOffset = null;
                    returnResult.Success = false;
                    // Exit early if user clicked without dragging and selection area is too small
                    return returnResult;
                }
            }

            // Flip saved mouse points if user dragged from bottom right to top left
            // Only sanitize the values if not already dragging selection since it would already be sanitized
            if (MouseIsDown && !IsDraggingSelection)
            {
                // Make sure the selection start is smaller than the end
                if ((MouseMovePointLast.Value.X <= MouseDownPointFirst.Value.X))
                {
                    if ((MouseDownPointFirst.Value.Y >= MouseMovePointLast.Value.Y))
                    {
                        Point? tempStart = MouseDownPointFirst;
                        MouseDownPointFirst = MouseMovePointLast;
                        MouseMovePointLast = tempStart;
                    }
                    else
                    {
                        Point? tempStart = new Point(MouseMovePointLast.Value.X, MouseDownPointFirst.Value.Y);
                        Point? tempEnd = new Point(MouseDownPointFirst.Value.X, MouseMovePointLast.Value.Y);
                        MouseDownPointFirst = tempStart;
                        MouseMovePointLast = tempEnd;
                    }
                }

                if ((MouseDownPointFirst.Value.Y > MouseMovePointLast.Value.Y))
                {
                    Point? tempStart = new Point(MouseDownPointFirst.Value.X, MouseMovePointLast.Value.Y);
                    Point? tempEnd = new Point(MouseMovePointLast.Value.X, MouseDownPointFirst.Value.Y);
                    MouseDownPointFirst = tempStart;
                    MouseMovePointLast = tempEnd;

                }

                // Save the new rect
                // This is the 1st place the SelectionRect values are updated. The only other place is in the MouseMove, when user drags selection.
                SelectionRect = new Int32Rect(x: (int)MouseDownPointFirst.Value.X, y: (int)MouseDownPointFirst.Value.Y, width: futureRectWidth, height: fugureRectHeight);
            }

            MouseIsDown = false;


            // Draw the rectangle. This is needed if the user clicks a single square and doesn't move
            SolidColorBrush fillColor = new SolidColorBrush(Color.FromArgb(80, 0, 255, 255));
            GraphicsUtils.DrawSelectionRectangle(
                overlaySelectionCanvas,
                (int)SelectionRect.Value.Y,
                (int)SelectionRect.Value.X,
                (int)SelectionRect.Value.Y + (int)SelectionRect.Value.Height,
                (int)SelectionRect.Value.X + (int)SelectionRect.Value.Width,
                1,
                fillColor,
                shouldUseEllipse);

            WriteableBitmap sourceImage;

            sourceImage = new WriteableBitmap((BitmapSource)targetImage.Source);

            // Save the image region under the selection when mouse is up
            SolidColorBrush tempBrushColor = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            SelectionAsBitmap = GraphicsUtils.createColoredBitmap(
                (int)SelectionRect.Value.Width,
                (int)SelectionRect.Value.Height,
                tempBrushColor);
            if (SelectionAsBitmap != null)
            {
                bool xAreWithinBoard = (int)SelectionRect.Value.X >= 0 && ((int)SelectionRect.Value.X + (int)SelectionRect.Value.Width <= targetImage.Source.Width);
                bool yAreWithinBoard = (int)SelectionRect.Value.Y >= 0 && ((int)SelectionRect.Value.Y + (int)SelectionRect.Value.Height <= targetImage.Source.Height);
                bool selectRectWithinImage = (xAreWithinBoard && yAreWithinBoard);

                if (selectRectWithinImage)
                {
                    GraphicsUtils.CopyImageRegion(
                        sourceImage,
                        (int)SelectionRect.Value.Y,
                        (int)SelectionRect.Value.X,
                        SelectionAsBitmap,
                        0,
                        0,
                        (int)SelectionRect.Value.Width,
                        (int)SelectionRect.Value.Height,
                        1,
                        shouldBlend: false,
                        useEllipse: shouldUseEllipse
                    );

                    LastValidSelectionAsBitmap = GraphicsUtils.createColoredBitmap(
                        (int)SelectionRect.Value.Width,
                        (int)SelectionRect.Value.Height,
                        tempBrushColor);

                    if (LastValidSelectionAsBitmap != null)
                    {

                        GraphicsUtils.CopyImageRegion(
                            sourceImage,
                            (int)SelectionRect.Value.Y,
                            (int)SelectionRect.Value.X,
                            LastValidSelectionAsBitmap,
                            0,
                            0,
                            (int)SelectionRect.Value.Width,
                            (int)SelectionRect.Value.Height,
                            1,
                            shouldBlend: false,
                            useEllipse: shouldUseEllipse
                        );
                    }

                    // SelectionRect is an Int32Rect which is a Struct so it does a Copy By Value
                    LastValidSelectionRect = SelectionRect;
                }

                if (!selectRectWithinImage)
                {
                    //Restore the preview and the image
                    SelectionRect = LastValidSelectionRect;
                    SelectionAsBitmap = LastValidSelectionAsBitmap;

                    overlaySelectionCanvas.Children.Clear();
                    SolidColorBrush fillColorTwo = new SolidColorBrush(Color.FromArgb(80, 0, 255, 255));
                    GraphicsUtils.DrawSelectionRectangle(
                        overlaySelectionCanvas,
                        (int)SelectionRect.Value.Y,
                        (int)SelectionRect.Value.X,
                        (int)SelectionRect.Value.Y + (int)SelectionRect.Value.Height,
                        (int)SelectionRect.Value.X + (int)SelectionRect.Value.Width,
                        1,
                        fillColorTwo,
                        shouldUseEllipse);
                }
            }

            returnResult.SelectionRect = SelectionRect;
            return returnResult;
        }

        public ToolResult OnMouseLeave(Image targetImage, Image previewImage, Canvas overlaySelectionCanvas, EditorCell[,] tileMapArray, Point position, int gridDimension, SolidColorBrush brushColor)
        {
            return ToolResult.None;
        }

    }
}
