using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Visual-only Kraid ceiling and floor-spike selections. Block appearances,
/// animation timing and movement callbacks remain compiled and immutable.
/// </summary>
public sealed class InstalledRoomPlmKraidVisuals
{
    /// <summary>
    /// The ten draw roles in content-identity order: the three ceiling-crumble stages, the three ceiling background draws
    /// of Kraid's room sequence, the two floor-spike appearances, and the two room-clearing layouts that replace the
    /// ceiling blocks and the floor spikes after Kraid is defeated.
    /// </summary>
    internal static readonly KraidRoomDraw[] ContentOrder =
    [
        KraidRoomDraw.CrumbleFirst, KraidRoomDraw.CrumbleSecond, KraidRoomDraw.CrumbleThird,
        KraidRoomDraw.CeilingBackground1, KraidRoomDraw.CeilingBackground2, KraidRoomDraw.CeilingBackground3,
        KraidRoomDraw.SpikeFirst, KraidRoomDraw.SpikeSecond, KraidRoomDraw.ClearCeiling, KraidRoomDraw.ClearSpikes,
    ];

    /// <summary>Each draw role's installed block appearances.</summary>
    internal readonly IReadOnlyDictionary<KraidRoomDraw, RoomBlockAppearance[]> frames;

    /// <summary>The installed block appearances of one of the ten named Kraid draw roles.</summary>
    private RoomBlockAppearance[] Frame(KraidRoomDraw draw) =>
        frames.TryGetValue(draw, out RoomBlockAppearance[]? frame)
            ? frame : throw new InvalidOperationException($"Undefined {nameof(KraidRoomDraw)} {draw}.");

    /// <summary>Holds the import-validated block appearances of all ten Kraid ceiling and spike layouts.</summary>
    /// <param name="frames">Each draw role's owned block appearances.</param>
    internal InstalledRoomPlmKraidVisuals(IReadOnlyDictionary<KraidRoomDraw, RoomBlockAppearance[]> frames) =>
        this.frames = Ensure.NotNull(frames);

    /// <summary>Resolves the installed appearance of one Kraid-layout block while its terrain remains compiled.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Run ordinal; must be zero because each layout has one horizontal run.</param>
    /// <param name="blockIndex">Zero-based block ordinal along that run, not a room coordinate.</param>
    /// <returns>The copied frame's appearance.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported Kraid layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run is nonzero or the block ordinal is outside the compiled draw shape.</exception>
    internal RoomBlockAppearance GetAppearance(PlmDraw draw, int runIndex, int blockIndex)
    {
        if (!KraidRoomPlmDraws.TryGet(draw, out var definition))
            throw new InvalidDataException(
                $"Kraid room visuals lack draw {draw}.");
        RoomBlockAppearance[] appearances = Frame(definition.Frame);
        if (runIndex != 0 ||
            (uint)blockIndex >= (uint)definition.BlockCount)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return appearances[blockIndex];
    }
}
