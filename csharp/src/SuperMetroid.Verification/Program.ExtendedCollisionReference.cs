using SuperMetroid.Core.Game;
using SuperMetroid.AssetExtraction;

internal static partial class Program
{
    // Test-only cartridge walker for $A0:9A5A/$9B7F. Kept independent of the
    // compiled geometry and production overlap helper after removal of the
    // runtime ROM fallback in a15b3aea0. Arguments mirror the reflected subject,
    // including its output callback, so existing boundary assertions stay intact.
    /// <summary>Walks native extended-spritemap and hitbox records to independently evaluate overlap and select the first matching collision callback.</summary>
    /// <param name="source">Cartridge import address space containing the native component and hitbox records.</param>
    /// <param name="arguments">Reflected collision inputs in slots 0–5; slot 6 receives the matched rectangle's shot or contact callback value, or zero when there is no hit.</param>
    /// <returns><see langword="true"/> when a native hitbox overlaps the target; otherwise, <see langword="false"/>.</returns>
    private static bool ReferenceExtendedCollision(CartridgeImportAddressSpace source, object?[] arguments)
    {
        var enemy = (RoomEnemySlot)arguments[0]!;
        ushort x = (ushort)arguments[1]!, y = (ushort)arguments[2]!;
        ushort radiusX = (ushort)arguments[3]!, radiusY = (ushort)arguments[4]!;
        bool shot = (bool)arguments[5]!;
        arguments[6] = (ushort)0;
        int bank = enemy.Definition.Bank << 16;
        byte Byte(int offset) => source.ReadCartridgeByte(bank | (ushort)offset);
        ushort Word(int offset) => (ushort)(Byte(offset) | Byte(offset + 1) << 8);
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
            ushort componentX = unchecked((ushort)(enemy.XPosition + Word(component)));
            ushort componentY = unchecked((ushort)(enemy.YPosition + Word(component + 2)));
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
                arguments[6] = Word(rectangle + (shot ? 10 : 8));
                return true;
            }
        }
        return false;
    }
}
