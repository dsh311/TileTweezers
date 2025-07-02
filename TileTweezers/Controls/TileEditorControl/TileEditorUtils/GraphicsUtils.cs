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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace _TileTweezers.Controls.TileEditorControl.TileEditorUtils
{
    public static class GraphicsUtils
    {
        public static void DrawSelectionRectangle(
                                                        Canvas drawOnCanvas,
                                                        int rowStart,
                                                        int colStart,
                                                        int rowEnd,
                                                        int colEnd,
                                                        int gridDim,
                                                        SolidColorBrush fillColor,
                                                        bool shouldUseEllipse = false)
        {
            int rowFinalStart = rowStart;
            int rowFinalEnd = rowEnd;
            int colFinalStart = colStart;
            int colFinalEnd = colEnd;

            // Normalize the rectangle coordinates
            int x = Math.Min(colFinalStart, colFinalEnd) * gridDim;
            int y = Math.Min(rowFinalStart, rowFinalEnd) * gridDim;

            // This might not be needed since the selection rect already has a fixed width and height
            int width = (Math.Abs(colFinalEnd - colFinalStart) + 1) * gridDim;
            int height = (Math.Abs(rowFinalEnd - rowFinalStart) + 1) * gridDim;


            // Remove all selection rectangles
            drawOnCanvas.Children.Clear();

            //In WPF, positioning of visual elements like a Rectangle on a Canvas is
            //handled by the parent container, not by the shape itself.

            if (shouldUseEllipse)
            {
                // Create an ellipse
                Ellipse selectionOval = new Ellipse
                {
                    Stroke = Brushes.Blue,
                    StrokeThickness = 1,
                    Fill = fillColor,
                    Width = width,
                    Height = height
                };

                Canvas.SetLeft(selectionOval, x);
                Canvas.SetTop(selectionOval, y);

                drawOnCanvas.Children.Add(selectionOval);
            }

            if (!shouldUseEllipse)
            {
                Rectangle selectionRect = new Rectangle
                {
                    Stroke = Brushes.Blue,
                    StrokeThickness = 1,
                    Fill = fillColor, // semi-transparent blue
                    Width = width,
                    Height = height
                };
                Canvas.SetLeft(selectionRect, x);
                Canvas.SetTop(selectionRect, y);

                drawOnCanvas.Children.Add(selectionRect);
            }
        }

        public static void DrawGridOnCanvas(Canvas drawOnThisCanvas, double imageWidth, double imageHeight, int tileSize, SolidColorBrush theColor, double thickness)
        {
            if (drawOnThisCanvas == null)
            {
                return;
            }
            drawOnThisCanvas.Children.Clear(); // Optional: clear existing lines

            int numVerticalLines = (int)Math.Floor(imageWidth / tileSize);
            int numHorizontalLines = (int)Math.Floor(imageHeight / tileSize);

            // Vertical lines
            for (int i = 0; i <= numVerticalLines; i++)
            {
                double x = i * tileSize;
                Line verticalLine = new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = imageHeight,
                    Stroke = theColor,
                    StrokeThickness = thickness
                };
                drawOnThisCanvas.Children.Add(verticalLine);
            }

            // Horizontal lines
            for (int i = 0; i <= numHorizontalLines; i++)
            {
                double y = i * tileSize;
                Line horizontalLine = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = imageWidth,
                    Y2 = y,
                    Stroke = theColor,
                    StrokeThickness = thickness
                };
                drawOnThisCanvas.Children.Add(horizontalLine);
            }

        }

        public static void DrawCheckerboard(Canvas canvas, double tileSize = 16)
        {
            if (canvas == null)
            {
                return;
            }
            canvas.Children.Clear();

            int rows = (int)Math.Floor(canvas.Height / tileSize);
            int cols = (int)Math.Floor(canvas.Width / tileSize);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if ((row + col) % 2 == 0)
                    {
                        Rectangle rect = new Rectangle
                        {
                            Width = tileSize,
                            Height = tileSize,
                            Fill = new SolidColorBrush(Color.FromArgb(100, 200, 200, 200))
                        };
                        Canvas.SetLeft(rect, col * tileSize);
                        Canvas.SetTop(rect, row * tileSize);
                        canvas.Children.Add(rect);
                    }
                }
            }
        }

        public static void transparentImage(Image thisImage)
        {
            // Clear the tileMapImgPreview to transparent
            int width = (int)(thisImage.Width > 0 ? thisImage.Width : thisImage.ActualWidth);
            int height = (int)(thisImage.Height > 0 ? thisImage.Height : thisImage.ActualHeight);

            if (width > 0 && height > 0)
            {
                // Create a transparent WriteableBitmap
                var wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
                int stride = wb.BackBufferStride;

                int pixelCount = height * stride;
                // This creates byte array filled with zeros, so alpha
                // Transparent black (B=0, G=0, R=0, A=0) in BGRA32 format.
                byte[] pixels = new byte[pixelCount];

                // All bytes already zero by default => transparent BGRA
                wb.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);

                thisImage.Source = wb;
            }
        }

        public static WriteableBitmap? resizeImageSource(Image imageControl, int newWidth, int newHeight)
        {
            if (imageControl?.Source is BitmapSource originalBitmap)
            {
                // Create the WriteableBitmap with the full desired size
                WriteableBitmap writable = new WriteableBitmap(
                    newWidth,
                    newHeight,
                    originalBitmap.DpiX,
                    originalBitmap.DpiY,
                    originalBitmap.Format,
                    originalBitmap.Palette);

                // Determine how much of the original to copy
                int copyWidth = Math.Min(newWidth, originalBitmap.PixelWidth);
                int copyHeight = Math.Min(newHeight, originalBitmap.PixelHeight);
                var srcRect = new Int32Rect(0, 0, copyWidth, copyHeight);

                int bytesPerPixel = (originalBitmap.Format.BitsPerPixel + 7) / 8;
                int stride = copyWidth * bytesPerPixel;
                byte[] pixels = new byte[stride * copyHeight];

                // Copy from original to buffer
                originalBitmap.CopyPixels(srcRect, pixels, stride, 0);

                // Write to the top-left corner of the new bitmap
                writable.WritePixels(new Int32Rect(0, 0, copyWidth, copyHeight), pixels, stride, 0);

                return writable;
            }
            return null;
        }

        public static WriteableBitmap createColoredBitmap(int width, int height, SolidColorBrush fillColor)
        {
            if (width < 0 || height < 0)
            {
                //return null;
                throw new ArgumentException("Width and height must be positive.");
            }

            var wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            int stride = wb.BackBufferStride;
            int totalBytes = height * stride;
            byte[] pixels = new byte[totalBytes];

            // Extract BGRA components from the SolidColorBrush
            Color color = fillColor.Color;
            byte[] colorBytes = new byte[] { color.B, color.G, color.R, color.A };

            // Fill one row with the color
            byte[] oneRow = new byte[stride];
            for (int x = 0; x < width; x++)
            {
                Buffer.BlockCopy(colorBytes, 0, oneRow, x * 4, 4);
            }

            // Copy the row into each row of the full pixel array
            for (int y = 0; y < height; y++)
            {
                Buffer.BlockCopy(oneRow, 0, pixels, y * stride, stride);
            }

            wb.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
            return wb;
        }

        public static void CopyImageRegion(
                                            WriteableBitmap source,
                                            int sourceRow,
                                            int sourceColumn,
                                            WriteableBitmap destination,
                                            int destRow,
                                            int destColumn,
                                            int selectedRegionWidth,
                                            int selectedRegionHeight,
                                            int gridDim,
                                            bool shouldBlend = false,
                                            bool useEllipse = true)
        {
            int bytesPerPixel = source.Format.BitsPerPixel / 8;
            //if (bytesPerPixel != 4)
            if (source.Format != PixelFormats.Bgra32 && source.Format != PixelFormats.Pbgra32)
            {
                throw new NotSupportedException("Only 32bpp images are supported.");
            }

            int sourceStartX = sourceColumn * gridDim;
            int sourceStartY = sourceRow * gridDim;
            int destStartX = destColumn * gridDim;
            int destStartY = destRow * gridDim;

            int stride = selectedRegionWidth * bytesPerPixel;

            byte[] sourcePixels = new byte[selectedRegionHeight * stride];
            byte[] destPixels = new byte[selectedRegionHeight * stride];

            int numColumnsInDestImage = (int)Math.Floor(destination.Width / gridDim);
            int numRowsInDestImage = (int)Math.Floor(destination.Height / gridDim);
            int numColumnsInSelection = selectedRegionWidth / gridDim;
            int numRowsInSelection = selectedRegionHeight / gridDim;

            if (destColumn + numColumnsInSelection <= numColumnsInDestImage &&
                destRow + numRowsInSelection <= numRowsInDestImage)
            {
                source.CopyPixels(
                    new Int32Rect(sourceStartX, sourceStartY, selectedRegionWidth, selectedRegionHeight),
                    sourcePixels,
                    stride,
                    0);

                if (shouldBlend || useEllipse)
                {
                    destination.CopyPixels(
                        new Int32Rect(destStartX, destStartY, selectedRegionWidth, selectedRegionHeight),
                        destPixels,
                        stride,
                        0);
                }

                // Ellipse bounds
                float rx = selectedRegionWidth / 2f;
                float ry = selectedRegionHeight / 2f;
                float cx = rx;
                float cy = ry;

                for (int y = 0; y < selectedRegionHeight; y++)
                {
                    for (int x = 0; x < selectedRegionWidth; x++)
                    {
                        // If useEllipse is true, skip pixels outside the ellipse
                        if (useEllipse)
                        {
                            float dx = x - cx;
                            float dy = y - cy;
                            if ((dx * dx) / (rx * rx) + (dy * dy) / (ry * ry) > 1)
                                continue;
                        }

                        int pixelIndex = (y * selectedRegionWidth + x) * bytesPerPixel;

                        byte srcB = sourcePixels[pixelIndex + 0];
                        byte srcG = sourcePixels[pixelIndex + 1];
                        byte srcR = sourcePixels[pixelIndex + 2];
                        byte srcA = sourcePixels[pixelIndex + 3];

                        if (shouldBlend)
                        {
                            byte dstB = destPixels[pixelIndex + 0];
                            byte dstG = destPixels[pixelIndex + 1];
                            byte dstR = destPixels[pixelIndex + 2];
                            byte dstA = destPixels[pixelIndex + 3];

                            float srcAlpha = srcA / 255f;
                            float invSrcAlpha = 1f - srcAlpha;

                            destPixels[pixelIndex + 0] = (byte)(srcB * srcAlpha + dstB * invSrcAlpha);
                            destPixels[pixelIndex + 1] = (byte)(srcG * srcAlpha + dstG * invSrcAlpha);
                            destPixels[pixelIndex + 2] = (byte)(srcR * srcAlpha + dstR * invSrcAlpha);
                            destPixels[pixelIndex + 3] = (byte)Math.Min(255, srcA + dstA * invSrcAlpha);
                        }
                        else
                        {
                            destPixels[pixelIndex + 0] = srcB;
                            destPixels[pixelIndex + 1] = srcG;
                            destPixels[pixelIndex + 2] = srcR;
                            destPixels[pixelIndex + 3] = srcA;
                        }
                    }
                }

                destination.WritePixels(
                    new Int32Rect(destStartX, destStartY, selectedRegionWidth, selectedRegionHeight),
                    destPixels,
                    stride,
                    0);
            }
        }

        public static void DrawLineOnImage(Image targetImage, Point pointA, Point pointB, Color color)
        {
            WriteableBitmap bmp;
            bool shouldSaveBackSinceCopied = false;
            if (targetImage.Source is WriteableBitmap writable)
            {
                bmp = writable;
            }
            else if (targetImage.Source is FormatConvertedBitmap || targetImage.Source is CachedBitmap || targetImage.Source is BitmapSource)
            {
                var source = targetImage.Source as BitmapSource;
                if (source == null)
                    throw new InvalidOperationException("The Image.Source must be a BitmapSource-compatible type.");

                bmp = new WriteableBitmap(source);
                shouldSaveBackSinceCopied = true;
            }
            else
            {
                throw new InvalidOperationException("Unsupported Image.Source type: " + targetImage.Source.GetType().Name);
            }

            int x0 = (int)pointA.X;
            int y0 = (int)pointA.Y;
            int x1 = (int)pointB.X;
            int y1 = (int)pointB.Y;

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            byte[] pixelColor = { color.B, color.G, color.R, color.A }; // BGRA

            bmp.Lock(); // Lock the bitmap for writing

            while (true)
            {
                if (x0 >= 0 && x0 < bmp.PixelWidth && y0 >= 0 && y0 < bmp.PixelHeight)
                {
                    Int32Rect pixelRect = new Int32Rect(x0, y0, 1, 1);
                    bmp.WritePixels(pixelRect, pixelColor, 4, 0);
                }

                if (x0 == x1 && y0 == y1) break;

                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }

            bmp.Unlock(); // Unlock after writing
            
            if (shouldSaveBackSinceCopied)
            {
                targetImage.Source = bmp;
            }
            
        }

        public static void DrawPixelOnImage(Image targetImage, int theRow, int theColumn, Color fillColor)
        {
            int x = (int)theColumn;
            int y = (int)theRow;

            WriteableBitmap bmp = targetImage.Source as WriteableBitmap;
            if (bmp == null || x < 0 || y < 0 || x >= bmp.PixelWidth || y >= bmp.PixelHeight)
            {
                return;
            }

            // BGRA format: Blue, Green, Red, Alpha
            byte[] colorData = { fillColor.B, fillColor.G, fillColor.R, fillColor.A }; // Red pixel
            Int32Rect rect = new Int32Rect(x, y, 1, 1);

            bmp.WritePixels(rect, colorData, bmp.BackBufferStride, 0);
        }

        public static void DrawSquareOnImage(Image targetImage, Point clickPointLoc, int widthOfSquare, Color fillColor)
        {
            int width = (int)targetImage.Width;
            int height = (int)targetImage.Height;

            WriteableBitmap bmp = targetImage.Source as WriteableBitmap;

            // If the source is not already a WriteableBitmap, convert it
            if (bmp == null)
            {
                if (targetImage.Source is BitmapSource sourceBitmap)
                {
                    bmp = new WriteableBitmap(sourceBitmap);
                }
                else
                {
                    // If source is null or incompatible, make a new empty WriteableBitmap
                    bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
                }
                targetImage.Source = bmp;
            }

            int bytesPerPixel = (bmp.Format.BitsPerPixel + 7) / 8;
            int stride = bmp.PixelWidth * bytesPerPixel;

            int half = widthOfSquare / 2;

            int left = Math.Max((int)clickPointLoc.X - half, 0);
            int top = Math.Max((int)clickPointLoc.Y - half, 0);
            int right = Math.Min(left + widthOfSquare, bmp.PixelWidth);
            int bottom = Math.Min(top + widthOfSquare, bmp.PixelHeight);

            int rectWidth = right - left;
            int rectHeight = bottom - top;

            if (rectWidth <= 0 || rectHeight <= 0)
                return;

            // Create a buffer just for the square region
            byte[] fillPixels = new byte[rectHeight * rectWidth * bytesPerPixel];

            for (int y = 0; y < rectHeight; y++)
            {
                for (int x = 0; x < rectWidth; x++)
                {
                    int index = (y * rectWidth + x) * bytesPerPixel;
                    fillPixels[index + 0] = fillColor.B;
                    fillPixels[index + 1] = fillColor.G;
                    fillPixels[index + 2] = fillColor.R;
                    fillPixels[index + 3] = fillColor.A;
                }
            }

            bmp.WritePixels(
                new Int32Rect(left, top, rectWidth, rectHeight),
                fillPixels,
                rectWidth * bytesPerPixel, // stride for the small square buffer
                0
            );
        }

        public static void FillGridSquareOnImage(Image targetImage, int theRow, int theColumn, int gridDim, Color fillColor)
        {
            int width = (int)targetImage.Width;
            int height = (int)targetImage.Height;

            // Create or get the WriteableBitmap
            WriteableBitmap bmp = targetImage.Source as WriteableBitmap;
            if (bmp == null || bmp.PixelWidth != width || bmp.PixelHeight != height)
            {
                bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
                targetImage.Source = bmp;
            }

            int bytesPerPixel = (bmp.Format.BitsPerPixel + 7) / 8;
            int stride = bmp.PixelWidth * bytesPerPixel;
            byte[] pixels = new byte[bmp.PixelHeight * stride];

            // Copy the current bitmap into the pixel buffer
            bmp.CopyPixels(pixels, stride, 0);

            // Define color in BGRA format
            byte[] colorData = new byte[]
            {
                fillColor.B, fillColor.G, fillColor.R, fillColor.A
            };

            for (int y = theRow * gridDim; y < (theRow + 1) * gridDim; y++)
            {
                if (y < 0 || y >= bmp.PixelHeight) continue;

                for (int x = theColumn * gridDim; x < (theColumn + 1) * gridDim; x++)
                {
                    if (x < 0 || x >= bmp.PixelWidth) continue;

                    int pixelIndex = y * stride + x * bytesPerPixel;
                    Array.Copy(colorData, 0, pixels, pixelIndex, 4);
                }
            }

            // Write the modified pixels back to the bitmap
            bmp.WritePixels(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight), pixels, stride, 0);
        }

        public static SolidColorBrush? GetColorAtPoint(Image image, Point clickPoint)
        {
            BitmapSource bitmap = image.Source as BitmapSource;
            if (bitmap == null)
            {
                return null;
            }

            int x = (int)clickPoint.X;
            int y = (int)clickPoint.Y;

            // Safety check
            if (x < 0 || y < 0 || x >= bitmap.PixelWidth || y >= bitmap.PixelHeight)
            {
                return null;
            }

            // BGRA format (4 bytes per pixel)
            int bytesPerPixel = 4;
            byte[] pixel = new byte[bytesPerPixel];

            Int32Rect rect = new Int32Rect(x, y, 1, 1);
            bitmap.CopyPixels(rect, pixel, bytesPerPixel, 0);

            Color color = Color.FromArgb(pixel[3], pixel[2], pixel[1], pixel[0]);
            return new SolidColorBrush(color);
        }

        public static (int, int) GetGridXYFromPosition(object sender, Point position, int gridDim)
        {
            int theColumn = -1;
            int theRow = -1;
            if (sender is Image clickedImage)
            {
                theColumn = (int)(position.X / gridDim);
                theRow = (int)(position.Y / gridDim);
            }
            return (theRow, theColumn);
        }
        public static void SaveImageToFile(Image imageControl, string filePath)
        {
            if (imageControl.Source is BitmapSource bitmapSource)
            {
                BitmapEncoder encoder = new PngBitmapEncoder(); // or JpegBitmapEncoder, etc.
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                try
                {
                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        encoder.Save(stream);
                    }
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Error saving image: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("The image source is not a BitmapSource.");
            }
        }

        // Both WritePixels or CopyPixels need a Bgra32
        public static WriteableBitmap? EnsureBgra32Writable(BitmapSource sourceBitmap)
        {
            try
            {
                BitmapSource compatibleSource = sourceBitmap;

                // Convert only if needed
                if (sourceBitmap.Format != PixelFormats.Bgra32)
                {
                    compatibleSource = new FormatConvertedBitmap(
                        sourceBitmap,
                        PixelFormats.Bgra32,
                        null,
                        0.0);
                }

                // Return a WriteableBitmap based on the (possibly converted) source
                return new WriteableBitmap(compatibleSource);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static BitmapSource NormalizeImageDpi(BitmapImage bitmap)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            var stride = width * (bitmap.Format.BitsPerPixel + 7) / 8;
            var pixels = new byte[height * stride];
            bitmap.CopyPixels(pixels, stride, 0);
            var normalizedSource = BitmapSource.Create(
                width, height,
                96, 96, // Normalize to 96 DPI
                bitmap.Format,
                bitmap.Palette,
                pixels, stride);

            return new WriteableBitmap(normalizedSource);
        }

        //Useful for debugging
        public static void SaveWriteableBitmapToPng(WriteableBitmap bitmap, string filePath)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                encoder.Save(stream);
            }
        }

        public static void FloodFillAtPoint(Image targetImage, int x, int y, Color targetColor, Color replacementColor)
        {
            // Ensure the Image has a valid source
            if (targetImage.Source is not BitmapSource source)
            {
                throw new InvalidOperationException("Image must have a BitmapSource.");
            }

            // Ensure we have a WriteableBitmap to modify
            WriteableBitmap bmp = targetImage.Source switch
            {
                WriteableBitmap writable => writable,
                CachedBitmap or BitmapSource => new WriteableBitmap((BitmapSource)targetImage.Source),
                _ => throw new InvalidOperationException("Unsupported image source type.")
            };

            // Update the Image's source in case we converted from CachedBitmap
            if (targetImage.Source is not WriteableBitmap)
            {
                targetImage.Source = bmp;
            }

            int width = bmp.PixelWidth;
            int height = bmp.PixelHeight;
            int stride = width * 4;
            int[] pixels = new int[width * height];
            bmp.CopyPixels(pixels, stride, 0);

            int targetArgb = ColorToInt(targetColor);
            int replacementArgb = ColorToInt(replacementColor);

            if (targetArgb == replacementArgb) return;

            Queue<Point> queue = new();
            queue.Enqueue(new Point(x, y));

            while (queue.Count > 0)
            {
                Point p = queue.Dequeue();
                int px = (int)p.X;
                int py = (int)p.Y;
                if (px < 0 || px >= width || py < 0 || py >= height)
                    continue;

                int index = py * width + px;
                if (pixels[index] != targetArgb)
                    continue;

                pixels[index] = replacementArgb;

                queue.Enqueue(new Point(px + 1, py));
                queue.Enqueue(new Point(px - 1, py));
                queue.Enqueue(new Point(px, py + 1));
                queue.Enqueue(new Point(px, py - 1));
            }

            // Write modified pixel data back to the bitmap
            bmp.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        }

        // Helper to convert Color to BGRA int
        private static int ColorToInt(Color color)
        {
            return (color.B) | (color.G << 8) | (color.R << 16) | (color.A << 24);
        }
    }
}
