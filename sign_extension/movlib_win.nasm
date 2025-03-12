section .text
global movsx, movzx


movsx:
	movsx rax, byte [rcx]
	ret



movzx:
	movzx rax, byte [rcx]
	ret




