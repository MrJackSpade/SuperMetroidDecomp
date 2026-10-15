using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Validated decoding of bank-$8B cinematic instruction words into their closed per-owner
/// instruction sets. A list word with <see cref="CinematicCodePointers.InstructionCommandBit"/>
/// set names an executable entry point; each interpreter owner accepts only its own set.
/// </summary>
internal static class CinematicInstructionWords
{
    /// <summary>True when <paramref name="word"/> is a member of <typeparamref name="TInstruction"/>.</summary>
    public static bool TryDecode<TInstruction>(ushort word, out TInstruction instruction)
        where TInstruction : struct, Enum
    {
        instruction = Unsafe.BitCast<ushort, TInstruction>(word);
        return Enum.IsDefined(instruction);
    }

    /// <summary>Decodes the pre-instruction <paramref name="word"/> an actor running <paramref name="program"/> holds, or throws.</summary>
    public static TPreInstruction DecodePreInstruction<TPreInstruction>(ushort word, CinematicSpriteProgram program)
        where TPreInstruction : struct, Enum =>
        TryDecode(word, out TPreInstruction instruction)
            ? instruction
            : throw new InvalidDataException(
                $"Cinematic pre-instruction $8B:{word:X4} of an actor running {program} is not a {typeof(TPreInstruction).Name}.");

    /// <summary>Decodes <paramref name="word"/>, read at <paramref name="address"/>, or throws.</summary>
    public static TInstruction Decode<TInstruction>(ushort word, ushort address)
        where TInstruction : struct, Enum =>
        TryDecode(word, out TInstruction instruction)
            ? instruction
            : throw new InvalidDataException(
                $"Cinematic word $8B:{word:X4} at $8B:{address:X4} is not a {typeof(TInstruction).Name}.");
}
