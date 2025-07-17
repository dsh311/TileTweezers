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
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using _TileTweezers.Controls.TileEditorControl.TileEditorState;
using System.Xml.Linq;
using System.Diagnostics;
using System.Windows;

namespace _TileTweezers.Controls.TileEditorControl.TileEditorUtils
{
    internal class FileUtils
    {
        public static void GenerateTilesetTres(EditorCell[,] tileSetArray, string tilesetPngPath, string tresOutputPath, int tileWidth, int tileHeight)
        {
            string tres = @$"[gd_resource type=""TileSet"" load_steps=2 format=3]" + Environment.NewLine + Environment.NewLine;

            tres += @$"[ext_resource path=""{tilesetPngPath}"" type=""Texture2D"" id=""1_tileset_texture""]" + Environment.NewLine + Environment.NewLine;
            tres += "[resource]" + Environment.NewLine;
            tres += @"sources/0 = SubResource(1)" + Environment.NewLine;

            for (int curRow = 0; curRow < tileSetArray.GetLength(0); curRow++)
            {
                for (int curCol = 0; curCol < tileSetArray.GetLength(1); curCol++)
                {
                    int zeroStartTileId = tileSetArray[curRow, curCol].TileId - 1;
                    tres += $@"tiles/{zeroStartTileId}/atlas_source_id = 1" + Environment.NewLine;
                    tres += $@"tiles/{zeroStartTileId}/atlas_coords = Vector2i({curCol}, {curRow})" + Environment.NewLine;
                }
            }


            tres += Environment.NewLine + @$"[sub_resource type=""TileSetAtlasSource"" id=1]" + Environment.NewLine;
            tres += $@"texture = ExtResource(1)" + Environment.NewLine;
            tres += $@"texture_region_size = Vector2i({tileWidth}, {tileHeight})" + Environment.NewLine;

            File.WriteAllText(tresOutputPath, tres);
        }

        public static void ConvertBasicTmxString_ToGodot4(EditorCell[,] tileSetArray, string tmxString, string outputTscnPath, string tilesetResourcePath)
        {
            try
            {
                // 1. Load the TMX XML document from the string
                var doc = XDocument.Parse(tmxString); // <--- CHANGE IS HERE
                var mapElement = doc.Element("map");

                // Basic error checking for map element
                if (mapElement == null)
                {
                    return;
                }

                // 2. Parse map dimensions and tile size
                int mapWidth = int.Parse(mapElement.Attribute("width")?.Value ?? throw new InvalidOperationException("Map width not found."));
                int mapHeight = int.Parse(mapElement.Attribute("height")?.Value ?? throw new InvalidOperationException("Map height not found."));
                int tileWidth = int.Parse(mapElement.Attribute("tilewidth")?.Value ?? throw new InvalidOperationException("Tile width not found."));
                int tileHeight = int.Parse(mapElement.Attribute("tileheight")?.Value ?? throw new InvalidOperationException("Tile height not found."));

                // 3. Parse tileset information (assuming a single tileset)
                var tilesetElement = mapElement.Element("tileset");
                if (tilesetElement == null)
                {
                    return;
                }

                int firstGid = int.Parse(tilesetElement.Attribute("firstgid")?.Value ?? throw new InvalidOperationException("Tileset firstgid not found."));
                int tilesetColumns = int.Parse(tilesetElement.Attribute("columns")?.Value ?? throw new InvalidOperationException("Tileset columns not found."));
                // We'll also extract the tileset image source for the reminder message
                string tilesetImageSource = tilesetElement.Element("image")?.Attribute("source")?.Value ?? "unknown_image.png";


                // 4. Parse layer data (assuming a single layer and CSV encoding)
                var layerElement = mapElement.Element("layer");
                if (layerElement == null)
                {
                    return;
                }

                var dataElement = layerElement.Element("data");
                if (dataElement == null)
                {
                    return;
                }

                // Split CSV data and parse into integers
                string[] csvValues = dataElement.Value
                    .Trim()
                    .Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                int[] tmxGids = csvValues.Select(s => int.Parse(s)).ToArray();

                // Validate data size
                if (tmxGids.Length != mapWidth * mapHeight)
                {
                    return;
                }

                // 5. Write the Godot 4 TileMap scene file (.tscn)
                using (var writer = new StreamWriter(outputTscnPath))
                {
                    // Scene header
                    writer.WriteLine("[gd_scene load_steps=2 format=3 uid=\"uid://auto_generated_map_scene\"]"); // Unique UID is good practice
                    writer.WriteLine();

                    // External resource definition for the TileSet
                    writer.WriteLine("[ext_resource type=\"TileSet\" path=\"{0}\" id=\"1_map_tileset\"]", tilesetResourcePath);
                    writer.WriteLine();

                    // TileMap node definition
                    writer.WriteLine("[node name=\"TileMap\" type=\"TileMap\"]");
                    writer.WriteLine("tile_set = ExtResource(\"1_map_tileset\")"); // Reference by string ID
                    writer.WriteLine("cell_size = Vector2i({0}, {1})", tileWidth, tileHeight);
                    writer.WriteLine("format = 2"); // Godot 4 TileMap format version

                    string tileData = @"tile_data = {" + Environment.NewLine;

                    for (int curRow = 0; curRow < tileSetArray.GetLength(0); curRow++)
                    {
                        for (int curCol = 0; curCol < tileSetArray.GetLength(1); curCol++)
                        {
                            // Only add tileData when not empty
                            if (tileSetArray[curRow, curCol].IsEmpty == false)
                            {
                                int zeroStartTileId = tileSetArray[curRow, curCol].TileId - 1;
                                int tilesetRow = tileSetArray[curRow, curCol].TilesetRow;
                                int tilesetCol = tileSetArray[curRow, curCol].TilesetColumn;
                                tileData += $"\"{curCol}/{curRow}\": {{\"id\": {zeroStartTileId}, \"autotile_coord\": Vector2i({tilesetCol}, {tilesetRow}), \"source_id\": 1}},";
                                tileData += Environment.NewLine;
                            }
                        }
                    }
                    tileData += "}";
                    writer.WriteLine(tileData);

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error converting TMX string to Godot 4: {ex.Message}");
            }
        }

        public static string SaveToTmxString(
            EditorCell[,] tileSetArray,
            EditorCell[,] tileMapArray,
            int tileWidth,
            int tileHeight,
            int tileSetWidth,
            int tileSetHeight,
            string tilesetPath)
        {
            int numRowsInTileSet = tileSetArray.GetLength(0);
            int numColsInTileSet = tileSetArray.GetLength(1);

            int numRowsInTileMap = tileMapArray.GetLength(0);
            int numColsInTileMap = tileMapArray.GetLength(1);

            var map = new XElement("map",
                new XAttribute("version", "1.4"),
                new XAttribute("tiledversion", "1.10.1"),
                new XAttribute("orientation", "orthogonal"),
                new XAttribute("renderorder", "right-down"),
                new XAttribute("width", numColsInTileMap),
                new XAttribute("height", numRowsInTileMap),
                new XAttribute("tilewidth", tileWidth),
                new XAttribute("tileheight", tileHeight)
            );

            // Tileset (placeholder)
            int tilecount = tileSetArray.Length;
            var tileset = new XElement("tileset",
                new XAttribute("firstgid", "1"),
                new XAttribute("name", "tileset"),
                new XAttribute("tilewidth", tileWidth),
                new XAttribute("tileheight", tileHeight),
                new XAttribute("tilecount", tileSetArray.Length),
                new XAttribute("columns", numColsInTileSet),
                new XElement("image",
                    new XAttribute("source", tilesetPath),
                    new XAttribute("width", tileSetWidth),
                    new XAttribute("height", tileSetHeight))
            );
            map.Add(tileset);

            // Build CSV data
            var csv = new StringBuilder();

            for (int rowIndex = 0; rowIndex < numRowsInTileMap; rowIndex++)
            {
                for (int colIndex = 0; colIndex < numColsInTileMap; colIndex++)
                {
                    EditorCell curCell = tileMapArray[rowIndex, colIndex];
                    int tilesetRow = curCell.TilesetRow;
                    int tilesetCol = curCell.TilesetColumn;

                    // If the tilemap cell is not empty then use the index to the tileSetArray to get the TileId of that element
                    int tileIdFromTileSet = curCell.IsEmpty ? 0 : tileSetArray[tilesetRow, tilesetCol].TileId;

                    // TileID is 0 when empty or a reference to tileset
                    csv.Append(tileIdFromTileSet);
                    if (colIndex < numColsInTileMap - 1)
                    {
                        csv.Append(",");
                    }
                }

                // Add a new line, except on last line
                if (rowIndex < numRowsInTileMap - 1)
                {
                    csv.Append(",");
                    csv.AppendLine();
                }
            }

            // Data element
            var data = new XElement("data",
                new XAttribute("encoding", "csv"),
                csv.ToString()
            );

            // Layer element
            var layer = new XElement("layer",
                new XAttribute("name", "Tile Layer 1"),
                new XAttribute("width", numColsInTileMap),
                new XAttribute("height", numRowsInTileMap),
                data
            );

            map.Add(layer);

            // Generate final XML document
            var doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), map);
            return doc.ToString();
        }

        public static (bool Success, string Message) SaveToZip(BitmapSource bitmapSource, string jsonString, string zipPath)
        {
            try
            {
                // Use a MemoryStream to build the zip archive in memory first
                using (var zipMemoryStream = new MemoryStream())
                {
                    using (var archive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Create, true))
                    {
                        // 1. Add JSON
                        var jsonEntry = archive.CreateEntry("tilemap.json");
                        using (var entryStream = jsonEntry.Open())
                        using (var writer = new StreamWriter(entryStream, Encoding.UTF8))
                        {
                            writer.Write(jsonString);
                        }

                        // 2. Add PNG image from BitmapSource
                        var imageEntry = archive.CreateEntry("tileset.png");
                        using (var entryStream = imageEntry.Open())
                        {
                            var encoder = new PngBitmapEncoder();
                            var safeBitmap = new WriteableBitmap(bitmapSource);
                            encoder.Frames.Add(BitmapFrame.Create(safeBitmap));

                            // The issue: ZipArchiveEntry.Open() returns a non-seekable stream.
                            // PngBitmapEncoder.Save() often requires a seekable stream.
                            // Solution: Save to an intermediate MemoryStream first, then copy to the entryStream.
                            using (var tempImageStream = new MemoryStream())
                            {
                                encoder.Save(tempImageStream); // Save the image to a seekable MemoryStream
                                tempImageStream.Seek(0, SeekOrigin.Begin); // Rewind the MemoryStream
                                tempImageStream.CopyTo(entryStream); // Copy its contents to the zip entry stream
                            }
                        }
                    } // The ZipArchive is disposed here, which writes the central directory to zipMemoryStream

                    // Write the complete zip file from memory to disk
                    using (var fileStream = File.Create(zipPath))
                    {
                        zipMemoryStream.Seek(0, SeekOrigin.Begin); // Rewind the zipMemoryStream
                        zipMemoryStream.CopyTo(fileStream); // Copy the entire zip content to the file
                    }
                }

                return (true, "Success");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to save .ttmap file: {ex.Message}");
            }
        }

        public static (bool Success, string Message, BitmapImage TileSetImage, string JsonString) LoadFromZip(string filePath)
        {
            try
            {
                BitmapImage tileSetImage = null;
                string jsonString = null;

                using (var zip = ZipFile.OpenRead(filePath))
                {
                    var jsonEntry = zip.GetEntry("tilemap.json");
                    var pngEntry = zip.GetEntry("tileset.png");

                    if (pngEntry == null)
                    {
                        return (false, "Missing tileset.png in .ttmap file.", null, null);
                    }

                    if (jsonEntry == null)
                    {
                        return (false, "Missing tilemap.json in .ttmap file.", null, null);
                    }

                    // Load PNG into BitmapImage
                    using (var stream = pngEntry.Open())
                    {
                        var memStream = new MemoryStream();
                        stream.CopyTo(memStream);
                        memStream.Position = 0;

                        tileSetImage = new BitmapImage();
                        tileSetImage.BeginInit();
                        tileSetImage.CacheOption = BitmapCacheOption.OnLoad;
                        tileSetImage.StreamSource = memStream;
                        tileSetImage.EndInit();
                        tileSetImage.Freeze();
                    }

                    // Load JSON string
                    using (var reader = new StreamReader(jsonEntry.Open()))
                    {
                        jsonString = reader.ReadToEnd();
                    }
                }

                return (true, "Success", tileSetImage, jsonString);
            }
            catch (Exception ex)
            {
                return (false, $"Error loading .ttmap file: {ex.Message}", null, null);
            }
        }

        public static string CreateUniqueTilesetPng(BitmapSource imageToSave, string tmxPath)
        {
            // Get the directory of the TMX file
            string tmxDirectory = Path.GetDirectoryName(tmxPath)!;

            // Ensure the output directory exists
            Directory.CreateDirectory(tmxDirectory);

            // Generate a unique filename
            string guid = Guid.NewGuid().ToString();
            string filename = $"tileset-{guid}.png";
            string filePath = Path.Combine(tmxDirectory, filename);

            // Create encoder and save PNG
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(imageToSave));

            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(stream);
            }

            return filename; // Return the name only
        }

        public static (bool Success, string Message) WriteStringToFile(string filePath, string content)
        {
            try
            {
                File.WriteAllText(filePath, content);
                return (true, $"Success writing to file at: " + filePath);
            }
            catch (Exception ex)
            {
                return (false, $"Error writing to file: {ex.Message}");
            }
        }

    }

}
