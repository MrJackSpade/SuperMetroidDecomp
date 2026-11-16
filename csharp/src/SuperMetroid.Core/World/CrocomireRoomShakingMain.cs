namespace SuperMetroid.Core.World;

/// <summary>The Crocomire room's main (#2287): the rumble while his death uncovers the hidden wall.</summary>
internal sealed class CrocomireRoomShakingMain : IRoomMain
{
    /// <inheritdoc/>
    public void Run(RoomMainUpdate update) => update.Mains.RunCrocomireComebackRoomMain(update.Room);
}
