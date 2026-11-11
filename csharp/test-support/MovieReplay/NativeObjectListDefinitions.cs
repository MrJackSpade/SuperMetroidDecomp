using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// A structural reader of native bank-$84 PLM instruction lists for the #1296 ROM oracles: it walks a list word by
/// word beside the typed program that replaced it. Timed draws, sleeps, deletions, gotos, the loop timer and
/// fall-through into the next authored list are compared here; each family names what its other instructions mean.
/// Only verification reads cartridge lists; the port runs typed programs.
/// </summary>
internal static class NativeObjectListDefinitions
{
    /// <summary>The PLM program bank, as a long-address base.</summary>
    internal const int ProgramBank = 0x840000;

    /// <summary>
    /// Decodes a family instruction at <paramref name="cursor"/>: the typed cue it became and its native length in
    /// bytes, or false when the family has no such instruction.
    /// </summary>
    internal delegate bool CueReader<TCue>(IImportCartridgeSource cartridge, ushort cursor, [MaybeNullWhen(false)] out TCue cue, out int length);

    /// <summary>
    /// Decodes an instruction at <paramref name="cursor"/> that draws and ends the pass, such as the restore of a
    /// respawning block: the typed frame it became, its duration and its native length, or false for any other word.
    /// </summary>
    internal delegate bool ShowReader<TFrame>(IImportCartridgeSource cartridge, ushort cursor, out TFrame frame, out int duration, out int length);

    /// <summary>The cue reader of families whose lists hold only draws, loops and deletions.</summary>
    internal static CueReader<TCue> NoCues<TCue>() where TCue : struct =>
        (_, _, out cue, out length) =>
        {
            cue = default;
            length = 0;
            return false;
        };

    /// <summary>The native instruction word at <paramref name="cursor"/>, as the routine catalog <typeparamref name="TRoutine"/>.</summary>
    internal static TRoutine InstructionAt<TRoutine>(IImportCartridgeSource cartridge, int cursor) where TRoutine : struct, Enum =>
        (TRoutine)Enum.ToObject(typeof(TRoutine), cartridge.ReadCartridgeWord(ProgramBank | cursor));

    /// <summary>The byte operand at <paramref name="address"/>: the low byte of the word read there.</summary>
    internal static byte ReadOperandByte(IImportCartridgeSource cartridge, int address) =>
        unchecked((byte)cartridge.ReadCartridgeWord(ProgramBank | address));
}
