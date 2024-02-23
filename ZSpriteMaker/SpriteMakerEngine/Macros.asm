; Write Sprite Properties in the rom MACRO
macro Set_Sprite_Properties(SprPrep, SprMain)

pushpc ; Save writing Position for the sprite
org $0DB080+!SPRID ; Oam Harmless ($0E40)
db ((!Harmless<<7)|(!HVelocity<<6)|!NbrTiles)

org $0DB173+!SPRID ; Sprite HP ($0E50)
db !Health

org $0DB266+!SPRID ; Sprite Damage ($0CD2)
db !Damage

org $0DB359+!SPRID ; Sprite Data ($0E60 / $0F50)
db ((!DeathAnimation<<7)|(!ImperviousAll<<6)|(!SmallShadow<<5)|(!Shadow<<4)|(!Palette<<1))

org $0DB44C+!SPRID ; Sprite Hitbox ($0F60)
db ((!CollisionLayer<<7)|(!Statis<<6)|(!Persist<<5)|(!Hitbox))

org $0DB53F+!SPRID ; Sprite Fall ($0B6B)
db ((!DeflectArrow<<3)|(!Boss<<1)|!CanFall)

org $0DB632+!SPRID ; Sprite Prize ($0BE0)
db ((!Interaction<<7)|(!WaterSprite<<6)|(!Blockable<<5)|(!Sound<<4)|!Prize)

org $0DB725+!SPRID ; Sprite ($0CAA)
db ((!Statue<<5)|(!DeflectProjectiles<<4)|(!ImpervSwordHammer<<2)|(!ImperviousArrow<<1))

org $069283+(!SPRID*2) ; Vanilla Sprite Main Pointer
dw NewMainSprFunction

org $06865B+(!SPRID*2) ; Vanilla Sprite Prep Pointer
dw NewSprPrepFunction

org NewSprRoutinesLong+(!SPRID*3) ; New Long Sprite Pointer
dl <SprMain>

org NewSprPrepRoutinesLong+(!SPRID*3) ; New Long Sprite Pointer
dl <SprPrep>
pullpc ; Get back the writing position for the sprite

endmacro

; Go to the action specified ID can be Hex or Decimal
macro GotoAction(action)
    LDA.b #<action> : STA.w SprAction, X
endmacro


; Increase the sprite frame every (frames_wait) frames
; reset to (frame_start) when reaching (frame_end)
; This is using SprTimerB
macro PlayAnimation(frame_start, frame_end, frame_wait)
    LDA.w SprTimerB, X : BNE +
        LDA.w SprFrame, X : INC : STA.w SprFrame, X : CMP.b #<frame_end>+1 : BCC .noframereset
        LDA.b #<frame_start> : STA.w SprFrame, X
        .noframereset
        LDA.b #<frame_wait> : STA.w SprTimerB, X
    +
endmacro


; Show message if the player is facing toward sprite and pressing A
; Return Carry Set if message is displayed
; can use BCC .label <> .label to see if message have been displayed
macro ShowSolicitedMessage(message_id)
    LDY.b #(<message_id>)>>8
    LDA.b #<message_id>
    JSL Sprite_ShowSolicitedMessageIfPlayerFacing
endmacro

; Show message no matter what (should not be used without code condition)
macro ShowUnconditionalMessage(message_id)
    LDY.b #(<message_id>)>>8
    LDA.b #<message_id>
    JSL Sprite_ShowMessageUnconditional
endmacro

; Make the sprite move towards the player at a speed of (speed)
macro MoveTowardPlayer(speed)
    LDA.b #<speed>
    JSL Sprite_ApplySpeedTowardsPlayer
    JSL Sprite_MoveLong
endmacro

; Prevent the player from passing through sprite hitbox
macro PlayerCantPassThrough()
    JSL Sprite_PlayerCantPassThrough
endmacro

; Do damage to player on contact if sprite is on same layer as player
macro DoDamageToPlayerSameLayerOnContact()
    JSL Sprite_CheckDamageToPlayerSameLayer
endmacro

; Set harmless flag, 0 = harmful, 1 = harmless
macro SetHarmless(value)
    LDA.w SprNbrOAM, X
    AND #$7F
    if <value> != 0
        ORA.b #(<value>)<<7 
    endif
    STA.w SprNbrOAM, X
endmacro

; Set Room Flag (Chest 6) 
; Do not use if you have more than 5 chests or a small key under a pot
; in that room unless you want it to be already opened/taken
macro SetRoomFlag(value)
    if <value> != 0
    LDA $0403 : ORA #$20 : STA $0403
    else
    LDA $0403 : AND #$DF : STA $0403
    endif
endmacro

; Will prevent the player from moving or opening his menu
macro PreventPlayerMovement()
LDA #$01 : STA $02E4
endmacro

; Will allow the player to move or open his menu
macro AllowPlayerMovement()
STZ.w $02E4
endmacro

; Enter 16bit mode
macro Set16bitmode()
REP #$30
endmacro

; Enter 8bit mode
macro Set8bitmode()
SEP #$30
endmacro

; This is a 16 bit will load A with current rupee count
; to use with instructions CMP and BCC/BCS
macro GetPlayerRupees()
LDA $7EF360
endmacro

; Set the velocity Y of the sprite at (speed) value
; this require the use of the function JSL Sprite_MoveLong
macro SetSpriteSpeedY(speed)
LDA.b #<speed> : STA.w SprYSpeed, x
endmacro

; Set the velocity X of the sprite at (speed) value
; this require the use of the function JSL Sprite_MoveLong
macro SetSpriteSpeedX(speed)
LDA.b #<speed> : STA.w SprXSpeed, x
endmacro

; Will play a sound SFX 1 See Zelda_3_RAM.log for more informations
macro PlaySFX1(sfxid)
LDA.b #<sfxid> : STA $012E
endmacro

; Will play a sound SFX 2 See Zelda_3_RAM.log for more informations
macro PlaySFX2(sfxid)
LDA.b #<sfxid> : STA $012F
endmacro

; Will play a music See Zelda_3_RAM.log for more informations
macro PlayMusic(musicid)
LDA.b #<musicid> : STA $012C
endmacro

; Will set the timer A to wait (length) amount of frames
macro SetTimerA(length)
LDA.b #<length> : STA.w SprTimerA, X
endmacro

; Will set the timer B to wait (length) amount of frames
macro SetTimerB(length)
LDA.b #<length> : STA.w SprTimerB, X
endmacro

; Will set the timer C to wait (length) amount of frames
macro SetTimerC(length)
LDA.b #<length> : STA.w SprTimerC, X
endmacro

; Will set the timer D to wait (length) amount of frames
macro SetTimerD(length)
LDA.b #<length> : STA.w SprTimerD, X
endmacro

; Will set the timer E to wait (length) amount of frames
macro SetTimerE(length)
LDA.b #<length> : STA.w SprTimerE, X
endmacro

; Will set the timer F to wait (length) amount of frames
macro SetTimerF(length)
LDA.b #<length> : STA.w SprTimerF, X
endmacro

SprRoom      = $0C9A ; Contains the area or room id the sprite has been loaded in
SprDrop      = $0CBA ; 00: Drop nothing, 01: drop normal key, 03: Drop green rupee, OtherValues: Drop big key

SprAction    = $0D80 ; This is used to determine what action(subroutine) we are currerntly running
SprFrame     = $0DC0 ; Determine the Frame used for the sprite
SprDamage    = $0CD2 ; Bump damage the sprite can inflict to the player
SprDmgTaken  = $0CE2 ; any value written here is the number of HP the sprite will lose on next frame

SprMiscA     = $0DA0 ; This can be used to do anything in sprite
SprMiscB     = $0DB0 ; This can be used to do anything in sprite
SprMiscC     = $0DE0 ; This can be used to do anything in sprite
SprMiscD     = $0E90 ; This can be used to do anything in sprite
SprMiscE     = $0EB0 ; This can be used to do anything in sprite
SprMiscF     = $0EC0 ; This can be used to do anything in sprite
SprMiscG     = $0ED0 ; This can be used to do anything in sprite
SprMiscH     = $0E80 ; This can be used to do anything in sprite
SprMiscI     = $0D90 ; This can be used to do anything in sprite

SprStunTimer = $0B58 ; The sprite will be stunned for that amount of timer value is decreased by 1 every frame

SprTimerA    = $0DF0 ; This is a timer, value is decreased by 1 every frame
SprTimerB    = $0E00 ; This is a timer, value is decreased by 1 every frame
SprTimerC    = $0E10 ; This is a timer, value is decreased by 1 every frame
SprTimerD    = $0EE0 ; This is a timer, value is decreased by 1 every frame
SprTimerE    = $0F10 ; This is a timer, value is decreased by 1 every frame
SprTimerF    = $0F80 ; This is a timer, value is decreased by 2 every frame is also used by the gravity routine

SprPause     = $0F00 ; Will put the sprite in pause mode used by the (IsActive) function
SprLayer    = $0F20 ; Return the floor the sprite is on either 0 (top layer), 1 (bottom layer)
SprType      = $0E20 ; This contains the ID of the sprite 00 = raven, 01 = vulture, etc...
SprSubtype   = $0E30 ; This contains the Subtype ID of the sprite
SprState     = $0DD0 ; This tells if the sprite is alive, dead, frozen, etc...

SprOAMHarm   = $0E40 ; HPTOOOOO [H Harmless][P Prevent Death][T Lite Tile Hit][O Oam slots used by the sprite]
SprHealth    = $0E50 ; Determine the number of health the sprite currently have
SprGfxProps  = $0E60 ; DISWPPPG [D Custom Death Anim.][I Invlunerable][S Small Shadow][W Draw Shadow][P Palette][G Graphic Page]
SprCollision = $0E70 ; ----UDLR [U Up][D Down][L Left][R Right] When sprite collide, this is set to the direction in which the collision occurred. 
SprRecoil    = $0EA0 ; Recoil Timer - Indicate that the sprite is recoiling (has been hit), value is decreased by 1 every frame
SprDeath     = $0EF0 ; Death Timer - Indicate that the sprite is about to die (has been hit), value is decreased by 1 every frame

SprProps     = $0F50 ; ---- PPPG [N Null - Unused][P Palette][G Graphic Page]
SprHitbox    = $0F60 ; ISPH HHHH [I ignore collisions][S Statis (not alive eg beamos)][P Persist code still run outside of camera][H Hitbox] 
SprHeight    = $0F70 ; Distance from the shadow ( Z position )
SprHeightS   = $0F90 ; Distance from the shadow subpixel ( Z position )

SprYRecoil   = $0F30 ; Recoiling speed Y where is it recoiling too
SprXRecoil   = $0F40 ; Recoiling speed X where is it recoiling too

OAMPtr       = $90 ; (Advanced) Pointer where the draw will be written in the OAM Table
OAMPtrH      = $92 ; (Advanced) Pointer High where the draw will be written in the OAM Table

SprY         = $0D00 ; Position Y of the sprite (Up to Down)
SprX         = $0D10 ; Position X of the sprite (Left to Right) 
SprYH        = $0D20 ; High (often determine the room) Position Y of the sprite (Up to Down)
SprXH        = $0D30 ; High (often determine the room) Position X of the sprite (Left to Right) 

SprYSpeed    = $0D40 ; Y Speed of the sprite can go negative to go up, positive to go down is used by the Sprite_Move function
SprXSpeed    = $0D50 ; X Speed of the sprite can go negative to go left, positive to go right is used by the Sprite_Move function

SprYRound    = $0D60 ; Y Position rounded to 8 pixel
SprXRound    = $0D70 ; X Position rounded to 8 pixel

SprCachedX   = $0FD8 ; Doesn't need to be indexed with X it contains the 16bit position X of the sprite
SprCachedY   = $0FDA ; Doesn't need to be indexed with X it contains the 16bit position Y of the sprite

FrameCounter = $1A ; value that is increasing every frame and loop forever
Indoor       = $1B ; 0: outside, 1: indoor
UpdPalFlag   = $15 ; Update all palettes from values in $7EC500-$7EC700 if non-zero

LinkY        = $20 ; Position Y of link
LinkYH       = $21 ; High position Y of link
LinkX        = $22 ; Position X of link
LinkXH       = $23 ; High position X of link

LinkPushDir  = $26 ; ----UDLR [U Up][D Down][L Left][R Right] Direction link is pushing against 
LinkFacingDir= $2F ; Direction link is facing 00:Up, 02:Down, 04:Left, 06:Right
LinkLastDir  = $66 ; Last direction link moved towards 00:Up, 01:Down, 02:Left, 03:Right
LinkMoveDir  = $67 ; ----UDLR [U Up][D Down][L Left][R Right] direction link is "walking towards"
LinkMoveInfo = $6A ; 0: Not moving, 1: Moving but NOT diagonally, 2: Moving diagonally

LinkRecoilY  = $27 ; Recoiling speed Y of link
LinkRecoilX  = $28 ; Recoiling speed X of link

ButtonAFlag  = $3B ; bit7: Button A is down (A-------)

LinkVisible  = $4B ; if set to 0x0C link will be invisible
LinkBunnyGfx = $56 ; if set to 1 link will be bunny, otherwise link

LinkSpeed    = $57 ; 0x00: normal speed, 0x01-0x0F: slow, > 0x10:fast
LinkSpeedTbl = $5E ; 0x00: normal speed, 0x02: walking on stair speed, 0x10: dashing speed
LinkFalling  = $5B ; if is set to 0x02 or 0x03 link is falling

LinkState    = $5D ; See documentation for that address (0x00 = normal ground state, 0x01 falling, 0x02 recoil, 0x03 spin attack) and many more
LinkDoorway  = $6C ; 0: Link is not in a doorway, 1: is in a vertical doorway, 2: is in horizontal doorway

Mosaic       = $95 ; set the mosaic setting ($2106) XXXXDCBA [ABCD BG1/BG2/BG3/BG4][X size of the mosaic pixels 0-16]

RawJoypad1L  = $F0 ; BYSTUDLR [B BButton][Y YButton][SSelect Button][TStart Button][UDLR dpad buttons Up, Down, Left, Right]
RawJoypad1H  = $F2 ; AXLRIIII [A AButton][X Xbutton][L LButton][R RButton][I = controller ID]

PressJoypad1L= $F4 ; BYSTUDLR [B BButton][Y YButton][SSelect Button][TStart Button][UDLR dpad buttons Up, Down, Left, Right]
PressJoypad1H= $F6 ; AXLRIIII [A AButton][X Xbutton][L LButton][R RButton][I = controller ID]

MusicControl = $012C ; set the music
SFX1Control  = $012E
SFX2Control  = $012F
AMBSFXControl= $012D

LinkGrabGfx  = $02DA ; 0: Nothing, 1: a hand in the air, 2: 2 hands in the air (like getting triforce)
LinkPoofGfx  = $02E1 ; if not 0 add a poof gfx on link
LinkBunTimer = $02E2 ; Bunny timer for link how many time you will stay in bunny before transforming back
LinkMenuMove = $02E4 ; if not 0 prevent link from moving and opening the menu
LinkGetDamage= $037B ; if not 0 prevent link from getting any damages from sprites

LinkTochChest= $02E5 ; ----CCCC [C Touching chest id]
LinkSomariaPl= $02F5 ; 0: Not on somaria platform, 2: On somaria platform
LinkItemUse  = $0301 ; BP-AETHR [B Boomerang][P Powder][A Bow&Arrows][E UnusedItem][T UnusedItem][H Hammer][R Rods]
LinkItemEquip= $0303 ; Currently equipped item on the Y button
LinkCarrying = $0308 ; 0: Nothing, 1:Picking up something, 2: Throwing something

LinkAnimation= $037A ; 0: Normal, 1: Shovel, 2: Praying, 4:Hookshot, 8:Somaria, 10: Bug net, 20: Read book, 40: Tree pull

MovingFloorVS= $0310 ; Moving floor Vertical speed 16 bit
MovingFloorHS= $0312 ; Moving floor Horizontal speed 16 bit
LinkWallCheat= $037F ; If non zero can walk through walls



AnciOAMPrior = $0280 ; Ancilla oam priority if non zero use highest priority for draw
AnciCollTimer= $028A ; Ancilla collision timer to prevent doing collision code too often set to 06 after a collision
AnciZSpeed   = $0294 ; Ancilla Z Speed
AnciHeight   = $029E ; Ancilla Height how far it is from its shadow
AnciHeightH  = $02A8 ; Ancilla Height hight byte how far it is from its shadow

AnciMiscA    = $0BF0 ; This can be used to do anything in ancilla
AnciMiscB    = $0C54 ; This can be used to do anything in ancilla
AnciMiscC    = $0C5E ; This can be used to do anything in ancilla (often used to track item received)
AnciMiscD    = $0C72 ; This can be used to do anything in ancilla (often used to track direction)


AnciTimerA   = $0C68 ; This is a timer, value is decreased by 1 every frame


AnciY        = $0BFA ; Position Y of the ancilla (Up to Down)
AnciX        = $0C04 ; Position X of the ancilla (Left to Right) 
AnciYH       = $0C0E ; High (often determine the room) Position Y of the ancilla (Up to Down)
AnciXH       = $0C18 ; High (often determine the room) Position X of the ancilla (Left to Right) 
AnciXSpeed   = $0C22 ; Y Speed of the ancilla can go negative to go up
AnciYSpeed   = $0C2C ; X Speed of the ancilla can go negative to go left
AnciLayer    = $0C7C ; return the floor where the ancilla is
AnciOamBuffer= $0C86 ; Oam buffer?
AnciOAMNbr   = $0C90 ; Number of OAM slots used

AnciYsub     = $0C36 ; sub pixel for Y position for ancilla
AnciXsub     = $0C40 ; sub pixel for X position for ancilla

AnciType     = $0C4A ; Define what ancilla it is (00:nothing, 01: somaria blast, 02: Fire rod shot, etc...) read documentation

