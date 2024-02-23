;#ENABLED=True
;#PATCH_NAME=Rainstate Skip
;#PATCH_AUTHOR=Zarby89
;#PATCH_VERSION=1.0
;#PATCH_DESCRIPTION
;Skips over gamestates 0 and 1 (rain) straight to 2 (Zelda rescued)
;Setting BedIntro to 00 will keep the opening bed sequence
;#ENDPATCH_DESCRIPTION


;#DEFINE_START
;#name=Remove bed intro
;#type=bool
!BedIntro = $01
;#DEFINE_END
pushpc
if !BedIntro == 00

	
	org $828356
	JSL NewIntroRain

pullpc
	NewIntroRain:
	LDA.l $7EF3C5 : BNE +
	LDA.b #$02 : STA.l $7EF3C5 ; Set Game mode on 2

	PHX

	JSL $80FC62 ; Sprite_LoadGfxProperties.justLightWorld

	PLX

	LDA.b #!BedIntro ; will take care of the bed intro wether we are in game rainstate or not
+	RTL

else
	org $0CD8F6
	JSL NewIntroRain

pullpc
	NewIntroRain:
	PHA
	LDA.w #$1002
	STA.l $7003C5, X
	PLA
	STA.l $7003D9, X
	RTL
endif
