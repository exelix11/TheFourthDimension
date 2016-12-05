# Setup:
You need the unpacked romfs of the game, on the first start select it and the editor will get all the needed files and convert the models from szs to obj to display them.<br />
to repeat this procedure delete the models folder.<br />

# 3D view basic controls:
(These controls are for the default settings,you can select in the settings the 3d mode you prefer)<br />
Use the right mouse button to rotate the camera, while pressing the middle button (the whell) will move the camera.<br />
The muse whell can also zoom in and out, but don't use the zoom to move though the level, if you zoom in too much the camera will start glitching.<br />
Left mouse button selects an object, press space to move the camera to it.<br />
Press control (ctrl) and drag an object with the left mouse button to move it, if you keep alt pressed the object will be snapped every 100 units (the size of a block).<br />
Press d to duplicate an Object.<br />
Press Del to delete an Object.<br />
Press + to add an Object.<br />
Press N while having a rail selected to add a point to it.<br />

# Rails:
Rails are paths that objects follow, they must be created in the AllRailsInfo section and then attacched to an object by copying and pasting it.<br />
Warning: if you attached the rail to the object, if you edit the rail the object's rail won't change, you must copy and paste it again<br />
Not every object supports rails, check on the objects database or look for examples in official levels.

# Copy and paste
The editor includes a custom clipboard that can contain up to 10 items, the clipboard can store (and paste):<br />
The position, the scale, the rotation or the args of an object.<br />
It can also store a rail, a full object or a list of objects that will be pasted all at once.<br />
When pasting an object or a list of objects over an existing object, those will be added as children of that object.<br />
Pasting a rail will attach that rail to the object.<br />
To paste an object in the level not as a child, left click on the object list to deselect every selected objects then click on paste.

# Children objects (C0Lists):
a C0List is a container that holds objects, usually it's used to store children objects.<br />
to edit a C0List click the ... button in the properties, to quickly edit the selected object's Children objects press C on the keyboard.<br />
To go back to the previus C0List or the main objects list press B or click the back button.

# Full Hotkeys list:
 Ctrl + Z : Undo<br />
 Space : Move the camera on the selected object<br />
 Ctrl + D : Duplicate selected object<br />
 + : Add a new object<br />
 Del : Delete selected object<br />
 Ctrl + R : Round the selected object position to a multiple of 100 (like Ctrl + alt + drag, but without dragging)<br />
 Ctrl + F : Open the search menu<br />
 C : If the selected object has a GenerateChildren property edits it<br />
 B : If you are editing a C0List goes back<br />
In the 3D view:<br />
 Shift + Click an object : Multi selection<br />
 Ctrl + drag : Move object<br />
 Ctrl + Alt + drag : Move object snapping every 100 units<br />
 Ctrl + Shift + drag : Move object snapping every 50 units<br />
 (With a rail selected) N : Add a new point at the end of the rail<br />
 in the 3D view you don't need to press Ctrl for the other hotkeys<br />

# Tools menu:
CreatorClassNameTable editor: To make a custom object appear in-game you must add it in the CreatorClassNameTable (CCNT) file from this editor.<br />
Change stages BGM: edits the list with the BGM of every stage<br />
Ogg to Bcstm converter: This tool can convert ogg files to multi channel bcstms, https://www.youtube.com/watch?v=L01TS3J7pO4<br />
Model import: This tool creates a szs package to put in the ObjectData folder, you can use a bcmdl made with the official tools or an obj (Note that the converter obj to bcmdl doesn't always work well with 3D land)<br />
Generate PreLoadFileList (beta): this will generate a PreLoadFileList file with the objects in the stage, it's not complete yet since it only includes objects files<br />

# Other functions:
You can use this also to edit byml files just by dragging them over the main exe, it's compatible with both 3ds and wii u formats, but some files from wii u games may not work (SM3DW)<br />
CollisionsMng.exe : this tool can create the collision data (.kcl and .pa) from an obj or convert the collision data to an obj<br />

# Sharing mods:
Mods that add custom objects should not include the CCNT file since part of it is copyrighted, instead you should use the "Generate CCNT patch files" function from Tools -> CreatorClassNameTable editor, this will create a patch file and the needed files to automatically apply it to an unmodified version of the file.<br />
You can also use other ways of distributing patch files, just avoid including the complete CCNT file or files that are fully or patially made by the developers of the game.