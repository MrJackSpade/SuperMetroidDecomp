using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>One editable Kraid ceiling or spike draw in native block order.</summary>
/// <param name="Id">Compiled identity of a crumble stage, ceiling background, spike column, or defeated-room clearing layout.</param>
/// <param name="Blocks">Metatile/flip words in the single horizontal run's native order; the catalog copies every frame array.</param>
public sealed record RoomPlmKraidVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Visual-only Kraid ceiling and floor-spike selections. Native block words,
/// animation timing and movement callbacks remain compiled and immutable.
/// </summary>
public sealed class RoomPlmKraidVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmKraidVisualCatalog), content =>
    {
        Append(KraidRoomDraw.CrumbleFirst, crumbleFirst);
        Append(KraidRoomDraw.CrumbleSecond, crumbleSecond);
        Append(KraidRoomDraw.CrumbleThird, crumbleThird);
        Append(KraidRoomDraw.CeilingBackground1, ceilingBackground1);
        Append(KraidRoomDraw.CeilingBackground2, ceilingBackground2);
        Append(KraidRoomDraw.CeilingBackground3, ceilingBackground3);
        Append(KraidRoomDraw.SpikeFirst, spikeFirst);
        Append(KraidRoomDraw.SpikeSecond, spikeSecond);
        Append(KraidRoomDraw.ClearCeiling, clearCeiling);
        Append(KraidRoomDraw.ClearSpikes, clearSpikes);

        void Append(KraidRoomDraw draw, ushort[] words)
        {
            content.Append("frame", (ushort)draw);
            content.Append("runs", 1);
            content.AppendWords("words", words);
        }
    });

    private ushort[] crumbleFirst = [], crumbleSecond = [], crumbleThird = [];
    private ushort[] ceilingBackground1 = [], ceilingBackground2 = [], ceilingBackground3 = [];
    private ushort[] spikeFirst = [], spikeSecond = [], clearCeiling = [], clearSpikes = [];

    /// <summary>Selects one of the ten named $84:9367..93BF Kraid draw roles.</summary>
    /// <remarks>Installed block values stay editable. The fixed pointer-to-role mapping
    /// is direct dispatch, independent of entry order; it needs no dictionary.</remarks>
    private ref ushort[] Frame(ushort pointer)
    {
        KraidRoomDraw draw = ClosedNativeWords.Decode<KraidRoomDraw>(pointer, "Kraid room visual draw");
        switch (draw)
        {
            case KraidRoomDraw.CrumbleFirst: return ref crumbleFirst;
            case KraidRoomDraw.CrumbleSecond: return ref crumbleSecond;
            case KraidRoomDraw.CrumbleThird: return ref crumbleThird;
            case KraidRoomDraw.CeilingBackground1: return ref ceilingBackground1;
            case KraidRoomDraw.CeilingBackground2: return ref ceilingBackground2;
            case KraidRoomDraw.CeilingBackground3: return ref ceilingBackground3;
            case KraidRoomDraw.SpikeFirst: return ref spikeFirst;
            case KraidRoomDraw.SpikeSecond: return ref spikeSecond;
            case KraidRoomDraw.ClearCeiling: return ref clearCeiling;
            case KraidRoomDraw.ClearSpikes: return ref clearSpikes;
            default: throw new InvalidOperationException($"Undefined {nameof(KraidRoomDraw)} {pointer:X4}.");
        }
    }

    /// <summary>Validates and copies all ten Kraid ceiling and spike layouts without changing collision words, crumble timing, or movement callbacks.</summary>
    /// <param name="entries">Exactly one visual-only entry per compiled role, with one block for individual stages or fifteen/twenty-two for the clearing layouts.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats a role, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    public RoomPlmKraidVisualCatalog(IEnumerable<RoomPlmKraidVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        int count = 0;
        foreach (RoomPlmKraidVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !KraidRoomPlmDrawDefinitions.TryGetByVisualId(
                    entry.Id, out var draw) ||
                entry.Blocks.Length != draw.BlockCount ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Kraid room visuals changed a frame identity, draw shape, or visual word.");
            ref ushort[] frame = ref Frame(draw.Pointer);
            if (frame.Length != 0)
                throw new InvalidDataException(
                    $"Kraid room visuals repeat frame {entry.Id}.");
            frame = entry.Blocks.ToArray();
            count++;
        }
        if (count != KraidRoomPlmDrawDefinitions.DrawCount)
            throw new InvalidDataException(
                "Kraid room visuals do not cover all ten compiled draws.");
    }

    /// <summary>Resolves the installed appearance of one Kraid-layout block while its physical level word remains compiled.</summary>
    /// <param name="drawPointer">Bank-$84 identity of one of the ten compiled Kraid ceiling or spike draws.</param>
    /// <param name="runIndex">Native run ordinal; must be zero because each layout has one horizontal run.</param>
    /// <param name="blockIndex">Zero-based word ordinal along that run, not a room coordinate.</param>
    /// <returns>The copied frame's metatile/flip word.</returns>
    /// <exception cref="InvalidDataException">The pointer does not identify a supported Kraid layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run is nonzero or the block ordinal is outside the compiled draw shape.</exception>
    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        ushort[] words = Frame(drawPointer);
        if (!KraidRoomPlmDrawDefinitions.TryGet(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Kraid room visuals lack draw ${drawPointer:X4}.");
        if (runIndex != 0 ||
            (uint)blockIndex >= (uint)draw.BlockCount)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return words[blockIndex];
    }
}
