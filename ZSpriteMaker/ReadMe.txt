Alttp Sprite Maker

To add new sprites, create your sprite with Alttp Sprite Maker
Use the button File/Export ASM save it to a file example MyFirstSprite.asm
save it into the folder SpriteMakerEngine\Sprites\MyFirstSprite.asm


if you want to use the provided BuildSprites.bat
you must put your rom file named alttp.sfc in the root directory
example below


How your project folder should looks like

MyRomhackFolder\
    alttp.sfc
    BuildSprites.bat
    ReadMe.txt
    	SpriteMakerEngine\
	asar.exe
	sprite_functions_hooks.asm
	sprite_new_functions.asm
	sprite_new_table.asm
	SpriteEngine.asm
	Template.asm
            Sprites\
                MyFirstSprite.asm


How to use it places your custom sprites in the Sprites folder inside the SpriteMakerEngine folder
place a file named alttp.sfc (your romhack) in the folder
double click on the file BuildSprites.bat a console windows will appear and tell you if any errors happened
this will create a copy of your rom for safety named alttp_sprites.sfc that will have your sprites patched in!