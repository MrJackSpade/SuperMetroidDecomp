using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction;

internal static class ExtendedCollisionReferenceVerification
{
    // Test-only cartridge walker for $A0:9A5A/$9B7F. Kept independent of the
    // compiled geometry and production overlap helper after removal of the
    // runtime ROM fallback in a15b3aea0. Arguments mirror the reflected subject,
    // including its output callback, so existing boundary assertions stay intact.
    [CpuBusBoundary("exact bus test: retail ROM reference model of extended spritemap collision")]
    internal static bool ReferenceExtendedCollision(CartridgeImportAddressSpace source, object?[] arguments)
    {
        var enemy = (RoomEnemySlot)arguments[0]!;
        var target = (Position16<RoomPixels>)arguments[1]!;
        var radius = (Extent16<RoomPixels>)arguments[2]!;
        ushort x = target.X, y = target.Y;
        ushort radiusX = radius.X, radiusY = radius.Y;
        bool shot = (bool)arguments[3]!;
        arguments[4] = (ushort)0;
        int bank = (int)new SnesAddress(enemy.Definition.Bank, 0);
        byte Byte(int offset) => source.ReadCartridgeByte(bank | (ushort)offset);
        ushort Word(int offset) => (ushort)(SnesLittleEndian.Word(Byte(offset), Byte(offset + 1)));
        static bool Negative(int value) => unchecked((short)value) < 0;
        ushort targetLeft = unchecked((ushort)(x - radiusX));
        ushort targetRight = unchecked((ushort)(x + radiusX));
        ushort targetTop = unchecked((ushort)(y - radiusY));
        ushort targetBottom = unchecked((ushort)(y + radiusY));

        // The header's high byte is drawing metadata, not part of the count.
        int count = Byte(enemy.SpritemapPointer);
        for (int componentIndex = 0; componentIndex < count; componentIndex++)
        {
            int component = enemy.SpritemapPointer + 2 + componentIndex * 8;
            ushort componentX = unchecked((ushort)(enemy.Position.X.Whole + Word(component)));
            ushort componentY = unchecked((ushort)(enemy.Position.Y.Whole + Word(component + 2)));
            ushort list = Word(component + 6);
            int rectangles = Word(list);
            for (int rectangleIndex = 0; rectangleIndex < rectangles; rectangleIndex++)
            {
                int rectangle = list + 2 + rectangleIndex * 12;
                ushort left = unchecked((ushort)(componentX + Word(rectangle)));
                ushort top = unchecked((ushort)(componentY + Word(rectangle + 2)));
                ushort right = unchecked((ushort)(componentX + Word(rectangle + 4)));
                ushort bottom = unchecked((ushort)(componentY + Word(rectangle + 6)));
                bool overlaps = shot
                    ? !Negative(targetRight - left) && Negative(targetLeft - right) &&
                      !Negative(targetBottom - top) && Negative(targetTop - bottom)
                    : Negative(left - targetRight) && !Negative(right - targetLeft) &&
                      Negative(top - targetBottom) && !Negative(bottom - targetTop);
                if (!overlaps) continue;
                arguments[4] = Word(rectangle + (shot ? 10 : 8));
                return true;
            }
        }
        return false;
    }
}
