; This file MUST be included in your main asm file this is the core of the sprit engine

; I would recommend putting sprites in their own bank
; so before incsrc that file put a org $3A8000 (for example)

pushpc
incsrc SymbolsDefines.asm
incsrc Macros.asm
incsrc sprite_functions_hooks.asm
pullpc

incsrc sprite_new_functions.asm