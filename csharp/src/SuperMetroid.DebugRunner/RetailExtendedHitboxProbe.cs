using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>
/// ROM-structure helper for focused extended-spritemap collision assertions. It does not
/// approximate an actor with a host rectangle: it walks the same component and hitbox lists
/// consumed by bank $A0 and returns a point strictly inside the requested authored record.
/// </summary>
internal static class RetailExtendedHitboxProbe
{
    public static IReadOnlyList<RetailExtendedHitboxShotPoint> ReadShotPoints(
        ISnesAddressSpace bus,
        RoomEnemySlot enemy)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var points = new List<RetailExtendedHitboxShotPoint>();

        // Native extended maps are bank-local negative pointers. `$804F` is a legitimate
        // empty map and naturally returns an empty result through its zero component count.
        if (!enemy.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap) ||
            (enemy.SpritemapPointer & 0x8000) == 0)
            return points;

        int bank = enemy.Definition.Bank << 16;
        int extendedMap = bank | enemy.SpritemapPointer;
        int componentCount = bus.ReadByte(extendedMap);
        for (int componentIndex = 0; componentIndex < componentCount; componentIndex++)
        {
            int component = AddWithinBank(extendedMap, 2 + componentIndex * 8);
            ushort componentX = unchecked((ushort)(
                enemy.XPosition + ReadWord(bus, component)));
            ushort componentY = unchecked((ushort)(
                enemy.YPosition + ReadWord(bus, AddWithinBank(component, 2))));
            int hitboxList = bank | ReadWord(bus, AddWithinBank(component, 6));
            int hitboxCount = ReadWord(bus, hitboxList);

            for (int hitboxIndex = 0; hitboxIndex < hitboxCount; hitboxIndex++)
            {
                int hitbox = AddWithinBank(hitboxList, 2 + hitboxIndex * 12);
                ushort shotCallback = ReadWord(bus, AddWithinBank(hitbox, 10));

                // Bounds are signed component-relative words. Choosing their arithmetic
                // midpoint keeps the one-pixel audit projectile away from the native
                // inclusive/exclusive edge asymmetry.
                int left = unchecked((short)ReadWord(bus, hitbox));
                int top = unchecked((short)ReadWord(bus, AddWithinBank(hitbox, 2)));
                int right = unchecked((short)ReadWord(bus, AddWithinBank(hitbox, 4)));
                int bottom = unchecked((short)ReadWord(bus, AddWithinBank(hitbox, 6)));
                points.Add(new RetailExtendedHitboxShotPoint(
                    unchecked((ushort)(componentX + (left + right) / 2)),
                    unchecked((ushort)(componentY + (top + bottom) / 2)),
                    shotCallback));
            }
        }

        return points;
    }

    public static bool TryFindShotPoint(
        ISnesAddressSpace bus,
        RoomEnemySlot enemy,
        ushort requestedCallback,
        out ushort worldX,
        out ushort worldY)
    {
        worldX = 0;
        worldY = 0;
        foreach (RetailExtendedHitboxShotPoint point in ReadShotPoints(bus, enemy))
        {
            if (point.Callback != requestedCallback)
                continue;
            worldX = point.X;
            worldY = point.Y;
            return true;
        }

        return false;
    }

    private static int AddWithinBank(int address, int offset) =>
        (address & 0xff0000) | unchecked((ushort)(address + offset));

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(
            bus.ReadByte(address) |
            (bus.ReadByte(AddWithinBank(address, 1)) << 8)));
}

internal readonly record struct RetailExtendedHitboxShotPoint(
    ushort X,
    ushort Y,
    ushort Callback);
