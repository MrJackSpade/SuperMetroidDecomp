using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>One editable Kraid ceiling or spike draw in native block order.</summary>
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
        Append(KraidRoomPlmDrawDefinitions.CrumbleFirst, crumbleFirst);
        Append(KraidRoomPlmDrawDefinitions.CrumbleSecond, crumbleSecond);
        Append(KraidRoomPlmDrawDefinitions.CrumbleThird, crumbleThird);
        Append(KraidRoomPlmDrawDefinitions.CeilingBackground1, ceilingBackground1);
        Append(KraidRoomPlmDrawDefinitions.CeilingBackground2, ceilingBackground2);
        Append(KraidRoomPlmDrawDefinitions.CeilingBackground3, ceilingBackground3);
        Append(KraidRoomPlmDrawDefinitions.SpikeFirst, spikeFirst);
        Append(KraidRoomPlmDrawDefinitions.SpikeSecond, spikeSecond);
        Append(KraidRoomPlmDrawDefinitions.ClearCeiling, clearCeiling);
        Append(KraidRoomPlmDrawDefinitions.ClearSpikes, clearSpikes);

        void Append(ushort pointer, ushort[] words)
        {
            content.Append("frame", pointer);
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
        switch (pointer)
        {
            case KraidRoomPlmDrawDefinitions.CrumbleFirst: return ref crumbleFirst;
            case KraidRoomPlmDrawDefinitions.CrumbleSecond: return ref crumbleSecond;
            case KraidRoomPlmDrawDefinitions.CrumbleThird: return ref crumbleThird;
            case KraidRoomPlmDrawDefinitions.CeilingBackground1: return ref ceilingBackground1;
            case KraidRoomPlmDrawDefinitions.CeilingBackground2: return ref ceilingBackground2;
            case KraidRoomPlmDrawDefinitions.CeilingBackground3: return ref ceilingBackground3;
            case KraidRoomPlmDrawDefinitions.SpikeFirst: return ref spikeFirst;
            case KraidRoomPlmDrawDefinitions.SpikeSecond: return ref spikeSecond;
            case KraidRoomPlmDrawDefinitions.ClearCeiling: return ref clearCeiling;
            case KraidRoomPlmDrawDefinitions.ClearSpikes: return ref clearSpikes;
            default: throw new InvalidDataException($"Kraid room visuals lack draw ${pointer:X4}.");
        }
    }

    public RoomPlmKraidVisualCatalog(IEnumerable<RoomPlmKraidVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        int count = 0;
        foreach (RoomPlmKraidVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !KraidRoomPlmDrawDefinitions.TryGetByVisualId(
                    entry.Id, out var draw) ||
                entry.Blocks.Length != draw.Runs.Span.ToArray().Sum(run =>
                    run.LevelWords.Length) ||
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
        if (count != KraidRoomPlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Kraid room visuals do not cover all ten compiled draws.");
    }

    public static RoomPlmKraidVisualCatalog Stock() => new(
        KraidRoomPlmDrawDefinitions.All.Select(draw =>
            new RoomPlmKraidVisualEntry(
                KraidRoomPlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span.ToArray().SelectMany(run =>
                    run.LevelWords.Span.ToArray().Select(word =>
                        new RoomLevelWord(word).VisualWord)).ToArray())));

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        ushort[] words = Frame(drawPointer);
        if (!KraidRoomPlmDrawDefinitions.TryGet(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Kraid room visuals lack draw ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)draw.Runs.Length ||
            (uint)blockIndex >= (uint)draw.Runs.Span[runIndex].LevelWords.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += draw.Runs.Span[run].LevelWords.Length;
        return words[flatIndex];
    }
}
