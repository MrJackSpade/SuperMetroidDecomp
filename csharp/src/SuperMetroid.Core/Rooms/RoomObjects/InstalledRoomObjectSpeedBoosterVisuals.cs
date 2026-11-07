using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Editable bomb-reveal appearance. The type-B collision and PLM timing
/// remain in compiled definitions.
/// </summary>
public sealed class InstalledRoomObjectSpeedBoosterVisuals
{
    /// <summary>
    /// Captured bomb-reveal metatile and flip bits; collision type and PLM timing
    /// continue to come from the compiled Speed Booster definition.
    /// </summary>
    private readonly RoomBlockAppearance appearance;

    /// <summary>Holds the import-validated bomb-reveal appearance.</summary>
    /// <param name="appearance">The block the reveal's sole cell shows.</param>
    internal InstalledRoomObjectSpeedBoosterVisuals(RoomBlockAppearance appearance) => this.appearance = appearance;

    /// <summary>Resolves the installed appearance for the sole Speed Booster bomb-reveal cell.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Zero, the reveal's only draw run.</param>
    /// <param name="blockIndex">Zero, the run's only 16-by-16 block.</param>
    /// <returns>The block and its mirroring; the compiled terrain supplies collision separately.</returns>
    /// <exception cref="InvalidDataException">Any selector does not name the sole compiled reveal cell.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int runIndex, int blockIndex)
    {
        if (draw != SpeedBoosterBlockObjectDrawDefinitions.BombRevealDraw ||
            runIndex != 0 || blockIndex != 0)
            throw new InvalidDataException(
                $"Speed Booster visuals lack draw {draw} run {runIndex} block {blockIndex}.");
        return appearance;
    }
}
