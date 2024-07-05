LDA.w SprVisible, X : BEQ .continueDraw
RTS ; exit instead
.continueDraw
JSL Sprite_PrepOamCoord
JSL Sprite_OAM_AllocateDeferToPlayer

LDA.w SprFrame, X : TAY ;Animation Frame
LDA .start_index, Y : STA $06


PHX
LDX .nbr_of_tiles, Y ;amount of tiles -1
LDY.b #$00
.nextTile

PHX ; Save current Tile Index?
    
TXA : CLC : ADC $06 ; Add Animation Index Offset

PHA ; Keep the value with animation index offset?

ASL A : TAX 

REP #$20

LDA $00 : CLC : ADC .x_offsets, X : STA ($90), Y
AND.w #$0100 : STA $0E 
INY
LDA $02 : CLC : ADC .y_offsets, X : STA ($90), Y
CLC : ADC #$0010 : CMP.w #$0100
SEP #$20
BCC .on_screen_y

LDA.b #$F0 : STA ($90), Y ;Put the sprite out of the way
STA $0E
.on_screen_y

PLX ; Pullback Animation Index Offset (without the *2 not 16bit anymore)
INY
LDA .chr, X : STA ($90), Y
INY
LDA .properties, X : STA ($90), Y

PHY 
    
TYA : LSR #2 : TAY
    
LDA .sizes, X : ORA $0F : STA ($92), Y ; store size in oam buffer
    
PLY : INY
    
PLX : DEX : BPL .nextTile

PLX

RTS

