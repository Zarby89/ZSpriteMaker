PHB : PHK : PLB

JSR Sprite_Template_Draw ; Call the draw code
JSL Sprite_CheckActive   ; Check if game is not paused
BCC .SpriteIsNotActive   ; Skip Main code is sprite is innactive

JSR Sprite_Template_Main ; Call the main sprite code

.SpriteIsNotActive
PLB ; Get back the databank we stored previously
RTL ; Go back to original code
