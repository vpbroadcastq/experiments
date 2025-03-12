section .text
global movsx, movzx


movsx:
	movsx rax, byte [rdi]
	ret



movzx:
	movzx rax, byte [rdi]
	ret




