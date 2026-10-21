using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Visual-only Kraid ceiling and floor-spike selections. Native block words,
/// animation timing and movement callbacks remain compiled and immutable.
/// </summary>
public sealed class RoomPlmKraidVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmKraidVisualCatalog), content =>
    {
        foreach (KraidRoomDraw draw in ContentOrder.OrderBy(draw => KraidRoomPlmDrawDefinitions.VisualId(PlmDraw.Of(draw)), StringComparer.Ordinal))
            Append(draw, frames[draw]);

        void Append(KraidRoomDraw draw, ushort[] words)
        {
            content.AppendName("frame", KraidRoomPlmDrawDefinitions.VisualId(PlmDraw.Of(draw)));
            content.Append("runs", 1);
            content.AppendWords("words", words);
        }
    });

    /// <summary>
    /// The ten draw roles in content-identity order: the three ceiling-crumble stages, the three ceiling background draws
    /// of Kraid's room sequence, the two floor-spike appearances, and the two room-clearing layouts that replace the
    /// ceiling blocks and the floor spikes after Kraid is defeated.
    /// </summary>
    private static readonly KraidRoomDraw[] ContentOrder =
    [
        KraidRoomDraw.CrumbleFirst, KraidRoomDraw.CrumbleSecond, KraidRoomDraw.CrumbleThird,
        KraidRoomDraw.CeilingBackground1, KraidRoomDraw.CeilingBackground2, KraidRoomDraw.CeilingBackground3,
        KraidRoomDraw.SpikeFirst, KraidRoomDraw.SpikeSecond, KraidRoomDraw.ClearCeiling, KraidRoomDraw.ClearSpikes,
    ];

    /// <summary>Each draw role's installed block words; empty until its entry is copied.</summary>
    private readonly Dictionary<KraidRoomDraw, ushort[]> frames = ContentOrder.ToDictionary(draw => draw, _ => Array.Empty<ushort>());

    /// <summary>Selects one of the ten named Kraid draw roles; installed block values stay editable.</summary>
    private ref ushort[] Frame(KraidRoomDraw draw)
    {
        ref ushort[] frame = ref CollectionsMarshal.GetValueRefOrNullRef(frames, draw);
        if (Unsafe.IsNullRef(ref frame))
            throw new InvalidOperationException($"Undefined {nameof(KraidRoomDraw)} {draw}.");
        return ref frame;
    }

    /// <summary>Validates and copies all ten Kraid ceiling and spike layouts without changing collision words, crumble timing, or movement callbacks.</summary>
    /// <param name="entries">Exactly one visual-only entry per compiled role, with one block for individual stages or fifteen/twenty-two for the clearing layouts.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats a role, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    public RoomPlmKraidVisualCatalog(IEnumerable<RoomPlmVisualEntry> entries)
    {
        Ensure.NotNull(entries);
        int count = 0;
        foreach (RoomPlmVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !KraidRoomPlmDrawDefinitions.TryGetByVisualId(
                    entry.Id, out var draw) ||
                entry.Blocks.Length != draw.BlockCount ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Kraid room visuals changed a frame identity, draw shape, or visual word.");
            ref ushort[] frame = ref Frame(draw.Frame);
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
    /// <param name="draw">The draw whose visual word is resolved.</param>
    /// <param name="runIndex">Native run ordinal; must be zero because each layout has one horizontal run.</param>
    /// <param name="blockIndex">Zero-based word ordinal along that run, not a room coordinate.</param>
    /// <returns>The copied frame's metatile/flip word.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported Kraid layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run is nonzero or the block ordinal is outside the compiled draw shape.</exception>
    internal ushort GetWord(PlmDraw draw, int runIndex, int blockIndex)
    {
        if (!KraidRoomPlmDrawDefinitions.TryGet(draw, out var definition))
            throw new InvalidDataException(
                $"Kraid room visuals lack draw {draw}.");
        ushort[] words = Frame(definition.Frame);
        if (runIndex != 0 ||
            (uint)blockIndex >= (uint)definition.BlockCount)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return words[blockIndex];
    }
}
