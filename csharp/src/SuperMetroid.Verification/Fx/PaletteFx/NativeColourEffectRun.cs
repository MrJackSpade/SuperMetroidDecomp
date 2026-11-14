using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.TestSupport.MovieReplay;

namespace SuperMetroid.Verification.Fx.PaletteFx;

/// <summary>
/// One colour effect's bank-$8D list run update by update as the cartridge's handler runs a spawned object (#1733): the
/// spawn leaves its countdown at one, so the update it spawns on shows its first record; each update counts down, and when
/// the countdown runs out the list's commands run up to the next record, whose duration restarts the countdown. A colour
/// index command moves on, a goto jumps, and a delete ends the run with its last record still shown.
/// </summary>
internal sealed class NativeColourEffectRun
{
    /// <summary>The cartridge the list is read from.</summary>
    private readonly IImportCartridgeSource cartridge;

    /// <summary>The list word the run reads next when its countdown runs out.</summary>
    private ushort next;

    /// <summary>The updates left before the run reads its list again.</summary>
    private int countdown = 1;

    /// <summary>Starts the run of the object whose definition lies at <paramref name="definition"/> in bank $8D.</summary>
    internal NativeColourEffectRun(IImportCartridgeSource cartridge, ushort definition)
    {
        this.cartridge = cartridge;
        next = Word(definition + sizeof(ushort));
    }

    /// <summary>The bank-$8D address of the record the run shows now, or null before its first update.</summary>
    internal ushort? Record { get; private set; }

    /// <summary>Whether the list has deleted its object; its last record stays shown.</summary>
    internal bool Deleted { get; private set; }

    /// <summary>Runs one update of the object.</summary>
    /// <exception cref="InvalidDataException">The list holds a command this run does not follow.</exception>
    internal void Step()
    {
        if (Deleted || --countdown > 0)
            return;
        for (int guard = 0; guard < 64; guard++)
        {
            ushort word = Word(next);
            if (word < PaletteFxListWords.CommandWord)
            {
                Record = next;
                countdown = word;
                next = PastRecord(next);
                return;
            }
            switch ((PaletteFxInstruction)word)
            {
                case PaletteFxInstruction.SetColorIndex:
                    next = unchecked((ushort)(next + 2 * sizeof(ushort)));
                    continue;
                case PaletteFxInstruction.Goto:
                    next = Word(next + sizeof(ushort));
                    continue;
                case PaletteFxInstruction.Delete:
                    Deleted = true;
                    return;
                case PaletteFxInstruction.Wait:
                case PaletteFxInstruction.ColorPlus2:
                case PaletteFxInstruction.ColorPlus3:
                case PaletteFxInstruction.ColorPlus4:
                case PaletteFxInstruction.ColorPlus8:
                case PaletteFxInstruction.ColorPlus9:
                case PaletteFxInstruction.ColorPlus15:
                case PaletteFxInstruction.SetFrameBehavior:
                case PaletteFxInstruction.ClearFrameBehavior:
                case PaletteFxInstruction.DecrementTimerAndGoto:
                case PaletteFxInstruction.SetTimer:
                case PaletteFxInstruction.QueueMusic:
                case PaletteFxInstruction.QueueSfx1:
                case PaletteFxInstruction.QueueSfx2:
                case PaletteFxInstruction.QueueSfx3:
                case PaletteFxInstruction.SetPaletteFxIndex:
                default:
                    throw new InvalidDataException($"The native list holds command ${word:X4} at $8D:{next:X4}, which this run does not follow.");
            }
        }
        throw new InvalidDataException($"The native list at $8D:{next:X4} reaches no record.");
    }

    /// <summary>The colours the record at <paramref name="record"/> writes, in order.</summary>
    internal RgbColor[] ColoursOf(ushort record)
    {
        var colours = new List<RgbColor>();
        for (ushort word = unchecked((ushort)(record + sizeof(ushort))); Word(word) != (ushort)PaletteFxInstruction.Wait;
             word = unchecked((ushort)(word + sizeof(ushort))))
            colours.Add(RgbColor.FromWord(Word(word)));
        return [.. colours];
    }

    /// <summary>The list word after the record at <paramref name="record"/>: past its duration, its colours and its closing wait.</summary>
    private ushort PastRecord(ushort record)
    {
        ushort word = unchecked((ushort)(record + sizeof(ushort)));
        while (Word(word) != (ushort)PaletteFxInstruction.Wait)
            word = unchecked((ushort)(word + sizeof(ushort)));
        return unchecked((ushort)(word + sizeof(ushort)));
    }

    /// <summary>The bank-$8D word at <paramref name="address"/>.</summary>
    private ushort Word(int address) => cartridge.ReadCartridgeWord(PaletteFxListWords.ListBank | address);

    /// <summary>The bank-$8D address of the record <paramref name="frame"/> shows, as the port numbers the records.</summary>
    internal static ushort RecordOf(ColourEffectFrame frame) => PaletteFxColorLayoutAccess.RecordAddress(PaletteFxRecord.Of(frame));
}
