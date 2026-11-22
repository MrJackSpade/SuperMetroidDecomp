using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World;

/// <summary>The Ceres pre-elevator hall's main (#2287): its falling debris once Ridley has escaped.</summary>
/// <param name="debris">The countdown to the next falling tile.</param>
internal sealed partial class CeresFallingDebrisMain(CeresFallingDebrisRoomMainState debris) : IRoomMain
{
    /// <inheritdoc/>
    /// <remarks>
    /// A tile falls each time the hall's countdown runs out, light or dark and in a column the update's random number
    /// chooses; the hall samples the number already drawn rather than drawing a new one, so later random consumers see
    /// the same numbers.
    /// </remarks>
    public RoomMainOutcome Run(RoomMainUpdate update)
    {
        RoomScope room = Ensure.NotNull(update).Room;
        if (room.Enemies.Ceres.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
            return RoomMainOutcome.None;

        // The countdown starts from what the previous room's main left, normally the shaft's turn step.
        if (!debris.Step())
            return RoomMainOutcome.None;

        int random = update.RandomNumber;
        CeresFallingDebrisSegment look = CeresFallingDebrisDefinitions.PieceIsDark.IsTakenBy(random)
            ? CeresFallingDebrisSegment.Dark
            : CeresFallingDebrisSegment.Light;
        room.Enemies.CeresFallingTiles.Spawn(
            CeresFallingDebrisDefinitions.ColumnX.Choose(random),
            new CeresFallingDebrisAnimation(look),
            CeresFallingDebrisVisualDefinitions.Graphics);
        return RoomMainOutcome.None;
    }
}
