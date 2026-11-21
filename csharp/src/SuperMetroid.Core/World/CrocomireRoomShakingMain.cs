using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.World;

/// <summary>The Crocomire room's main (#2287): the rumble while his death uncovers the hidden wall.</summary>
/// <param name="crocomire">The room's Crocomire, whom the room's enemies set up as the room loaded (#2486).</param>
internal sealed class CrocomireRoomShakingMain(CrocomireEnemyState crocomire) : IRoomMain
{
    /// <summary>The room's Crocomire.</summary>
    private readonly CrocomireEnemyState _crocomire = Ensure.NotNull(crocomire);

    /// <inheritdoc/>
    public RoomMainOutcome Run(RoomMainUpdate update)
    {
        Rumble(_crocomire, Ensure.NotNull(update).BackgroundScroll);
        return RoomMainOutcome.None;
    }

    /// <summary>Crocomire's room shaking while his comeback hides the wall.</summary>
    /// <param name="crocomire">The room's Crocomire.</param>
    /// <param name="layers">Where the layers stand, whose foreground view the rumble shakes.</param>
    internal static void Rumble(CrocomireEnemyState crocomire, BackgroundScrollState layers)
    {
        Ensure.NotNull(crocomire);
        Ensure.NotNull(layers);
        if (crocomire.Body.Presence.HasAny(EnemyPresence.Hidden) ||
            crocomire.DeathStage.Entry is not CrocomireDeathStage.RumbleHiddenWall)
            return;

        // The rumble moves the foreground's view by a plain offset from the camera's row: in Crocomire's one-screen-high
        // room the camera's row is the room's top, so the view rises above it, a negative row (#2384).
        layers.ShiftForegroundViewFromCamera(crocomire.Death.RumbleYOffset);
    }
}
