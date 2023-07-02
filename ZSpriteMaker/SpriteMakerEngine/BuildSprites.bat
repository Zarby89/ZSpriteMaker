copy alttp.sfc alttp_sprites.sfc


@echo off
break>BatGeneratedSprites.asm
for /f %%f in ('dir /b Sprites\') do echo incsrc "Sprites/%%f" >> BatGeneratedSprites.asm


Asar.exe SpriteEngine.asm alttp_sprites.sfc
pause