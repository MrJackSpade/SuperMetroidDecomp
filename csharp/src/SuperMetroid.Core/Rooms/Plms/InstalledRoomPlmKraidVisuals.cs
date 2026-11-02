using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Visual-only Kraid ceiling and floor-spike selections. Block appearances,
/// animation timing and movement callbacks remain compiled and immutable.
/// </summary>
public sealed class InstalledRoomPlmKraidVisuals
{
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
