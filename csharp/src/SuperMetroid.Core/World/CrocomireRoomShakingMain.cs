using SuperMetroid.Core.Bosses.Crocomire;

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
        Ensure.NotNull(update).Mains.RunCrocomireComebackRoomMain(_crocomire);
        return RoomMainOutcome.None;
    }
}
