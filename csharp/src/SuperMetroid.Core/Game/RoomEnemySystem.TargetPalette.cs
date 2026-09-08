namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// Publishes enemy-authored target colors to the current room-fade owner. Only written
    /// entries are transferred: untouched HUD, suit, and room rows retain their own targets.
    /// Current CGRAM must not be changed by instructions which target the fade buffer.
    /// </summary>
    internal void ConsumeTargetPaletteWrites(Action<int, ushort> write)
    {
        ArgumentNullException.ThrowIfNull(write);
        _sporeSpawn?.ConsumeTargetColors(write);
    }
}
