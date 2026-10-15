namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One immutable word-sized palette-program mechanic.</summary>
/// <param name="Pointer">Bank-$8D byte address of the control word or duration, including unaligned addresses after byte-sized timer operands.</param>
/// <param name="Value">Compiled sixteen-bit instruction, operand, or duration; never an editable BGR555 color payload.</param>
public readonly record struct PaletteFxMechanicsWord(ushort Pointer, ushort Value);
