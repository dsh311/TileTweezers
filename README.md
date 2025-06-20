# TileTweezers
A simple yet powerful tilemap editor for game development, built with C# and WPF.
![TileTweezers](TileTweezers.png)  
## How to use
TileTweezers is divided into a left and right side.  
The left side is where the tileset should be loaded.  
The right side is where the tilemap is created by stamping down what is Grid selected on the left side.  

Follow these steps to get started:  

1) ![Open Folder Icon](/TileTweezers/Controls/TileEditorControl/TileEditorImages/openfolder.png) On the left side, choose the "Open File" button and select a tileset image.
2) ![Grid Select Icon](/TileTweezers/Controls/TileEditorControl/TileEditorImages/selectGrid.png)With the "Grid Select" tool active, select the region on the tileset that you wish to stamp onto the tilemap.
3) Move your cursor to the right side (the tilemap area). The selected region from the left side will appear under the cursor when the ![Grid Stamp Icon](/TileTweezers/Controls/TileEditorControl/TileEditorImages/stampgrid.png)"Stamp Grid" tool  is chosen.
4) Click to stamp the image that appears under the cursor onto the tilemap.
5) Click the ![Save Icon](/TileTweezers/Controls/TileEditorControl/TileEditorImages/save.png)"Save" button  on the right side to save your created tilemap.

## About 
Tilemap editor, currently under active development, is written in C# and WPF.
Currently only supports saving tilemaps to .png format. Future updates will include support for more useful formats.

Features Pending Completion:
* Undo/Redo functionality is partially implemented.
* Missing some keyboard shortcuts for faster workflow.
* Currently saves only to .png; will be expanded to include other useful formats.
* Planned feature: Advanced tile rule-based painting for automated map generation.

