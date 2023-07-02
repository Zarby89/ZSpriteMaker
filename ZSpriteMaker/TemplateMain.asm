
;==================================================================================================
; Sprite Main code - THIS CODE WILL BE GENERATED SO CHANGING IT WON'T DO ANYTHING
;--------------------------------------------------------------------------------------------------
; This is the main local code of your sprite
; This contains all the Subroutines of your sprites you can add more below
;==================================================================================================
Sprite_Template_Main:
{
    LDA.w SprAction, X ; Load the SprAction
    JSL UseImplicitRegIndexedLocalJumpTable ; Goto the SprAction we are currently in

    dw Template_Action00
    dw Template_Action01
}