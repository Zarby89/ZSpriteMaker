SprRoom      = $0C9A ;X W Contains the area or room id the sprite has been loaded in
SprDrop      = $0CBA ;X W 00: Drop nothing, 01: drop normal key, 03: Drop green rupee, OtherValues: Drop big key

SprAction    = $0D80 ;X W This is used to determine what action(subroutine) we are currerntly running
SprFrame     = $0DC0 ;X W Determine the Frame used for the sprite
SprDamage    = $0CD2 ;X W Bump damage the sprite can inflict to the player
SprDmgTaken  = $0CE2 ;X W any value written here is the number of HP the sprite will lose on next frame

SprVisible   = $0D90 ;X W If non zero sprite will not be drawn

SprMiscA     = $0DA0 ;X W This can be used to do anything in sprite
SprMiscB     = $0DB0 ;X W This can be used to do anything in sprite
SprMiscC     = $0DE0 ;X W This can be used to do anything in sprite
SprMiscD     = $0E90 ;X W This can be used to do anything in sprite
SprMiscE     = $0EB0 ;X W This can be used to do anything in sprite
SprMiscF     = $0EC0 ;X W This can be used to do anything in sprite
SprMiscG     = $0ED0 ;X W This can be used to do anything in sprite
SprMiscH     = $0E80 ;X W This can be used to do anything in sprite

SprStunTimer = $0B58 ;X W The sprite will be stunned for that amount of timer value is decreased by 1 every frame

SprTimerA    = $0DF0 ;X W This is a timer, value is decreased by 1 every frame
SprTimerB    = $0E00 ;X W This is a timer, value is decreased by 1 every frame
SprTimerC    = $0E10 ;X W This is a timer, value is decreased by 1 every frame
SprTimerD    = $0EE0 ;X W This is a timer, value is decreased by 1 every frame
SprTimerE    = $0F10 ;X W This is a timer, value is decreased by 1 every frame
SprTimerF    = $0F80 ;X W This is a timer, value is decreased by 2 every frame is also used by the gravity routine

SprPause     = $0F00 ;X W Will put the sprite in pause mode used by the (IsActive) function
SprLayer     = $0F20 ;X W Return the floor the sprite is on either 0 (top layer), 1 (bottom layer)
SprType      = $0E20 ;X W This contains the ID of the sprite 00 = raven, 01 = vulture, etc...
SprSubtype   = $0E30 ;X W This contains the Subtype ID of the sprite
SprState     = $0DD0 ;X W This tells if the sprite is alive, dead, frozen, etc...

SprOAMHarm   = $0E40 ;X W HPTOOOOO [H Harmless][P Prevent Death][T Lite Tile Hit][O Oam slots used by the sprite]
SprHealth    = $0E50 ;X W Determine the number of health the sprite currently have
SprGfxProps  = $0E60 ;X W DISWPPPG [D Custom Death Anim.][I Invlunerable][S Small Shadow][W Draw Shadow][P Palette][G Graphic Page]
SprCollision = $0E70 ;X W ----UDLR [U Up][D Down][L Left][R Right] When sprite collide, this is set to the direction in which the collision occurred. 
SprRecoil    = $0EA0 ;X W Recoil Timer - Indicate that the sprite is recoiling (has been hit), value is decreased by 1 every frame
SprDeath     = $0EF0 ;X W Death Timer - Indicate that the sprite is about to die (has been hit), value is decreased by 1 every frame

SprProps     = $0F50 ;X W ---- PPPG [N Null - Unused][P Palette][G Graphic Page]
SprHitbox    = $0F60 ;X W ISPH HHHH [I ignore collisions][S Statis (not alive eg beamos)][P Persist code still run outside of camera][H Hitbox] 
SprHeight    = $0F70 ;X W Distance from the shadow ( Z position )
SprHeightS   = $0F90 ;X W Distance from the shadow subpixel ( Z position )

SprYRecoil   = $0F30 ;X W Recoiling speed Y where is it recoiling too
SprXRecoil   = $0F40 ;X W Recoiling speed X where is it recoiling too

OAMPtr       = $90 ; B (Advanced) Pointer where the draw will be written in the OAM Table
OAMPtrH      = $92 ; B (Advanced) Pointer High where the draw will be written in the OAM Table

SprY         = $0D00 ;X W Position Y of the sprite (Up to Down)
SprX         = $0D10 ;X W Position X of the sprite (Left to Right) 
SprYH        = $0D20 ;X W High (often determine the room) Position Y of the sprite (Up to Down)
SprXH        = $0D30 ;X W High (often determine the room) Position X of the sprite (Left to Right) 

SprYSpeed    = $0D40 ;X W Y Speed of the sprite can go negative to go up, positive to go down is used by the Sprite_Move function
SprXSpeed    = $0D50 ;X W X Speed of the sprite can go negative to go left, positive to go right is used by the Sprite_Move function

SprYRound    = $0D60 ;X W Y Position rounded to 8 pixel
SprXRound    = $0D70 ;X W X Position rounded to 8 pixel

SprCachedX   = $0FD8 ; W Doesn't need to be indexed with X it contains the 16bit position X of the sprite
SprCachedY   = $0FDA ; W Doesn't need to be indexed with X it contains the 16bit position Y of the sprite

FrameCounter = $1A ; B value that is increasing every frame and loop forever
Indoor       = $1B ; B 0: outside, 1: indoor
UpdPalFlag   = $15 ; B Update all palettes from values in $7EC500-$7EC700 if non-zero

LinkY        = $20 ; B Position Y of link
LinkYH       = $21 ; B High position Y of link
LinkX        = $22 ; B Position X of link
LinkXH       = $23 ; B High position X of link

LinkPushDir  = $26 ; B ----UDLR [U Up][D Down][L Left][R Right] Direction link is pushing against 
LinkFaceDir  = $2F ; B Direction link is facing 00:Up, 02:Down, 04:Left, 06:Right
LinkLastDir  = $66 ; B Last direction link moved towards 00:Up, 01:Down, 02:Left, 03:Right
LinkMoveDir  = $67 ; B ----UDLR [U Up][D Down][L Left][R Right] direction link is "walking towards"
LinkMoveInfo = $6A ; B 0: Not moving, 1: Moving but NOT diagonally, 2: Moving diagonally

LinkRecoilY  = $27 ; B Recoiling speed Y of link
LinkRecoilX  = $28 ; B Recoiling speed X of link

ButtonAFlag  = $3B ; B bit7: Button A is down (A-------)

LinkVisible  = $4B ; B if set to 0x0C link will be invisible
LinkBunnyGfx = $56 ; B if set to 1 link will be bunny, otherwise link

LinkSpeed    = $57 ; B 0x00: normal speed, 0x01-0x0F: slow, > 0x10:fast
LinkSpeedTbl = $5E ; B 0x00: normal speed, 0x02: walking on stair speed, 0x10: dashing speed
LinkFalling  = $5B ; B if is set to 0x02 or 0x03 link is falling

LinkState    = $5D ; B See documentation for that address (0x00 = normal ground state, 0x01 falling, 0x02 recoil, 0x03 spin attack) and many more
LinkDoorway  = $6C ; B 0: Link is not in a doorway, 1: is in a vertical doorway, 2: is in horizontal doorway

Mosaic       = $95 ; B set the mosaic setting ($2106) XXXXDCBA [ABCD BG1/BG2/BG3/BG4][X size of the mosaic pixels 0-16]

RawJoypad1L  = $F0 ; B BYSTUDLR [B BButton][Y YButton][SSelect Button][TStart Button][UDLR dpad buttons Up, Down, Left, Right]
RawJoypad1H  = $F2 ; B AXLRIIII [A AButton][X Xbutton][L LButton][R RButton][I = controller ID]

PressPad1L   = $F4 ; B BYSTUDLR [B BButton][Y YButton][SSelect Button][TStart Button][UDLR dpad buttons Up, Down, Left, Right]
PressPad1H   = $F6 ; B AXLRIIII [A AButton][X Xbutton][L LButton][R RButton][I = controller ID]

MusicControl = $012C ; W set the music
SFX1Control  = $012E ; W set sfx1
SFX2Control  = $012F ; W set sfx2
SFX3Control  = $012D ; W set sfx3

LinkGrabGfx  = $02DA ; W 0: Nothing, 1: a hand in the air, 2: 2 hands in the air (like getting triforce)
LinkPoofGfx  = $02E1 ; W if not 0 add a poof gfx on link
LinkBunTimer = $02E2 ; W Bunny timer for link how many time you will stay in bunny before transforming back
LinkMenuMove = $02E4 ; W if not 0 prevent link from moving and opening the menu
LinkDamage   = $037B ; W if not 0 prevent link from getting any damages from sprites

LinkColChest = $02E5 ; W ----CCCC [C Touching chest id]
LinkSomaria  = $02F5 ; W 0: Not on somaria platform, 2: On somaria platform
LinkItemUse  = $0301 ; W BP-AETHR [B Boomerang][P Powder][A Bow&Arrows][E UnusedItem][T UnusedItem][H Hammer][R Rods]
LinkItemY    = $0303 ; W Currently equipped item on the Y button
LinkCarrying = $0308 ; W 0: Nothing, 1:Picking up something, 2: Throwing something

LinkAnim     = $037A ; W 0: Normal, 1: Shovel, 2: Praying, 4:Hookshot, 8:Somaria, 10: Bug net, 20: Read book, 40: Tree pull

MovingFloorV = $0310 ; W Moving floor Vertical speed 16 bit
MovingFloorH = $0312 ; W Moving floor Horizontal speed 16 bit
LinkWallCheat = $037F ; W If non zero can walk through walls

AnciOAMPrior = $0280 ; W Ancilla oam priority if non zero use highest priority for draw
AnciColTimer = $028A ; W Ancilla collision timer to prevent doing collision code too often set to 06 after a collision
AnciZSpeed   = $0294 ; W Ancilla Z Speed
AnciHeight   = $029E ; W Ancilla Height how far it is from its shadow
AnciHeightH  = $02A8 ; W Ancilla Height hight byte how far it is from its shadow

AnciMiscA    = $0BF0 ; W This can be used to do anything in ancilla
AnciMiscB    = $0C54 ; W This can be used to do anything in ancilla
AnciMiscC    = $0C5E ; W This can be used to do anything in ancilla (often used to track item received)
AnciMiscD    = $0C72 ; W This can be used to do anything in ancilla (often used to track direction)

AnciTimerA   = $0C68 ; W This is a timer, value is decreased by 1 every frame

AnciY        = $0BFA ; W Position Y of the ancilla (Up to Down)
AnciX        = $0C04 ; W Position X of the ancilla (Left to Right) 
AnciYH       = $0C0E ; W High (often determine the room) Position Y of the ancilla (Up to Down)
AnciXH       = $0C18 ; W High (often determine the room) Position X of the ancilla (Left to Right) 
AnciXSpeed   = $0C22 ; W Y Speed of the ancilla can go negative to go up
AnciYSpeed   = $0C2C ; W X Speed of the ancilla can go negative to go left
AnciLayer    = $0C7C ; W return the floor where the ancilla is
AnciOamBuf   = $0C86 ; W Oam buffer?
AnciOAMNbr   = $0C90 ; W Number of OAM slots used

AnciYsub     = $0C36 ; W sub pixel for Y position for ancilla
AnciXsub     = $0C40 ; W sub pixel for X position for ancilla

AnciType     = $0C4A ; W Define what ancilla it is (00:nothing, 01: somaria blast, 02: Fire rod shot, etc...) read documentation

RoomIndex    = $A0 ; B Return the current room ID
AreaIndex    = $8A ; B Return the current overworld area ID

;==================================================
; SRAM Stuff
;==================================================
SRAMUWDataL  = $7EF000 ; L Should be used while being indexed with X (X = RoomIndex ($A0)) CCCCQQQQ [C Chest data][Quadrant Data 4, 3, 2, 1 order]
SRAMUWDataH  = $7EF001 ; L Should be used while being indexed with X (X = RoomIndex ($A0)) DDDDBKKR [D Doors opened][B Boss killed][K Key or Floor item or 6th chest][5th chest or rupee floor]

SRAMOWData   = $7EF280 ; L Should be used while being indexed with X (X = AreaIndex ($8A)) -HO---S-[H Mainly Heartpieces][O Overlay][S Secondary overlays (bomb holes, rockstairs)]

SRAMBow       = $7EF340 ; L 0 = Nothing, 1 = Bow, 2 = Bow & Arrow, 3 = Silver Arrow Bow, 4 = Bow & Silver Arrows
SRAMBoomerang = $7EF341 ; L 0 = Nothing, 1 = Blue boomerang, 2 = Red Boomerang
SRAMHookshot  = $7EF342 ; L 0 = Nothing, 1 = Hookshot
SRAMBombs     = $7EF343 ; L 0 = Nothing, Value = bomb count
SRAMMushPowdr = $7EF344 ; L 0 = Nothing, 1 = Mushroom, 2 = Magic powder
SRAMFirerod   = $7EF345 ; L 0 = Nothing, 1 = Fire rod
SRAMIcerod    = $7EF346 ; L 0 = Nothing, 1 = Ice rod
SRAMBombos    = $7EF347 ; L 0 = Nothing, 1 = Bombos
SRAMEther     = $7EF348 ; L 0 = Nothing, 1 = Ether
SRAMQuake     = $7EF349 ; L 0 = Nothing, 1 = Quake
SRAMLamp      = $7EF34A ; L 0 = Nothing, 1 = Lamp
SRAMHammer    = $7EF34B ; L 0 = Nothing, 1 = Hammer
SRAMShovFlute = $7EF34C ; L 0 = Nothing, 1 = Shovel, 2 = Flute, 3 = Active Flute 
SRAMBugnet    = $7EF34D ; L 0 = Nothing, 1 = Bugnet
SRAMBook      = $7EF34E ; L 0 = Nothing, 1 = Book
SRAMBottles   = $7EF34F ; L 0 = Nothing, Value = number of bottles
SRAMSomaria   = $7EF350 ; L 0 = Nothing, 1 = Somaria
SRAMByrna     = $7EF351 ; L 0 = Nothing, 1 = Byrna
SRAMCape      = $7EF352 ; L 0 = Nothing, 1 = Cape
SRAMMirror    = $7EF353 ; L 0 = Nothing, 1 = Scroll, 2 = Mirror
SRAMGloves    = $7EF354 ; L 0 = Nothing, 1 = Power gloves, 2 = Titans Mitt
SRAMBoots     = $7EF355 ; L 0 = Nothing, 1 = Pegasus boots
SRAMFlippers  = $7EF356 ; L 0 = Nothing, 1 = Flippers
SRAMPearl     = $7EF357 ; L 0 = Nothing, 1 = Moon pearl
SRAMSwords    = $7EF359 ; L 0 = Nothing, 1 = Fighter Sword, 2 = Master Sword, 3 = Tempered Sword, 4 = Golden Sword
SRAMShields   = $7EF35A ; L 0 = Nothing, 1 = Blue Shield, 2 = Hero Shield, 3 = Mirror Shield
SRAMArmors    = $7EF35B ; L 0 = Green Mail, 1 = Blue Mail, 2 = Red Mail
SRAMBotCont1  = $7EF35C ; L 0 = No bottle, 1 = Mush, 2 = Empty, 3 = Red, 4 = Green, 5 = Blue, 6 = Fairy, 7 = Bee, 8 = Gold bee
SRAMBotCont2  = $7EF35D ; L 0 = No bottle, 1 = Mush, 2 = Empty, 3 = Red, 4 = Green, 5 = Blue, 6 = Fairy, 7 = Bee, 8 = Gold bee
SRAMBotCont3  = $7EF35E ; L 0 = No bottle, 1 = Mush, 2 = Empty, 3 = Red, 4 = Green, 5 = Blue, 6 = Fairy, 7 = Bee, 8 = Gold bee
SRAMBotCont4  = $7EF35F ; L 0 = No bottle, 1 = Mush, 2 = Empty, 3 = Red, 4 = Green, 5 = Blue, 6 = Fairy, 7 = Bee, 8 = Gold bee
SRAMRupees    = $7EF360 ; L Number of rupees 
SRAMRupeesCtr = $7EF362 ; L Rupees counter should not be changed use SRAMRupees


HeartPieceNbr = $7EF36B ; L Number of heart pieces up to 4 that you have collected
MaxHealth     = $7EF36C ; L Number of hearts maximum you have (04 = 1 heart)
ActualHealth  = $7EF36D ; L Number of hearts you currently have (04 = 1 heart)
ActualMagic   = $7EF36E ; L Number of magic you currently have (128 = max (0x80))
CurrDungKeys  = $7EF36F ; L Number of key you currently have for the current dungeon
BombUpgrade   = $7EF370 ; L Number of bomb upgrade you have bought
ArrowUpgrade  = $7EF371 ; L Number of arrow upgrade you have bought
HealthFill    = $7EF372 ; L Number of health to refill (04 = 1 heart)
MagicFill     = $7EF373 ; L Number of magic to refill (128 = max)
Pendants      = $7EF374 ; L Bitwise uuuu ugbr ; g = green, b = blue, r = red, u = unused
BombFill      = $7EF375 ; L Number of bomb to refill
ArrowFill     = $7EF376 ; L Number of arrows to refill
ActualArrows  = $7EF377 ; L Number of actual arrows
Abilities     = $7EF379 ; L Bitwise urtupdsu ; u = unused, r = read, t = talk, p = pull, d = dash, s = swim
Crystals      = $7EF37A ; L Bitwise uwgstidm ; u = unused, w = skull woods, g = thieve town, s = swamp, t = turtle rock, i = ice, d = palace of darkness, m = mire
MagicCost     = $7EF37B ; L 0 = normal magic usage, 1 = half magic usage, 2 = quarter magic usage
GameState     = $7EF3C5 ; L 0 = rain state no sword, 1 = you got sword, 2 = zelda rescued, 3 = agahnim defeated
Follower      = $7EF3CC ; L 0 = none, 1 = zelda, 4 = old man, 5 = invis. zelda, 6 = blind maiden, 7 = smithfrog, 8 = smith, 9 = sign thief, A = kiki, C = purple chest, D = Big bomb
