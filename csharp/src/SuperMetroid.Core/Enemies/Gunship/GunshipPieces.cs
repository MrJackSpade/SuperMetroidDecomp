namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship's three pieces, which its top hull found when it was placed (#1542): the room lists the top hull, then
/// the bottom hull, then the entrance pad.
/// </summary>
/// <param name="Top">The top hull, which runs the ship.</param>
/// <param name="Bottom">The bottom hull, listed right after the top.</param>
/// <param name="EntrancePad">The entrance pad, listed right after the bottom.</param>
internal sealed record GunshipPieces(RoomEnemySlot Top, RoomEnemySlot Bottom, RoomEnemySlot EntrancePad)
{
    /// <summary>The three pieces in the room's order, which the ship moves together.</summary>
    internal IEnumerable<RoomEnemySlot> All
    {
        get
        {
            yield return Top;
            yield return Bottom;
            yield return EntrancePad;
        }
    }
}
