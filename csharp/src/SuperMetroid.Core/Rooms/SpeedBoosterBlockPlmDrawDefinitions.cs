namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The one-block native bank-$84:A4F3 reveal used by bomb-special Speed Booster
/// terrain. Its type-$B level word remains physical even when appearance changes.
/// </summary>
internal static class SpeedBoosterBlockPlmDrawDefinitions
{
    internal static readonly RoomPlmShotBlockDrawDefinitions.DrawList BombReveal =
        new(SpeedBoosterBlockPlmProgramDefinitions.BombRevealDraw,
            new RoomPlmShotBlockDrawDefinitions.Run[]
            {
                new(1, new ushort[] { 0xb0b6 }, 0, 0),
            });

    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        if (pointer == BombReveal.Pointer)
        {
            list = BombReveal;
            return true;
        }
        list = default;
        return false;
    }
}
