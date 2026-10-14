using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction;

/// <summary>Independent cartridge walker for extended enemy spritemap collision
/// used as a reference for compiled overlap geometry.</summary>
internal static class ExtendedCollisionReferenceVerification
{
    /// <summary>Walks native component and rectangle records for $A0:9A5A/$9B7F,
    /// retaining shot versus touch edge rules and the selected reaction pointer.</summary>
    /// <remarks>The argument array mirrors the reflected production helper, including
    /// its output slot; this oracle remains independent of compiled geometry.</remarks>
    [CpuBusBoundary("exact bus test: retail ROM reference model of extended spritemap collision")]
    internal static bool ReferenceExtendedCollision(CartridgeImportAddressSpace source, object?[] arguments)
    {
        var enemy = (RoomEnemySlot)(arguments[0] ?? throw new InvalidOperationException("Verification requires arguments[0]."));
        var target = (Position16<RoomPixels>)(arguments[1] ?? throw new InvalidOperationException("Verification requires arguments[1]."));
        var radius = (Extent16<RoomPixels>)(arguments[2] ?? throw new InvalidOperationException("Verification requires arguments[2]."));
        ushort x = target.X, y = target.Y;
        ushort radiusX = radius.X, radiusY = radius.Y;
        bool shot = (bool)(arguments[3] ?? throw new InvalidOperationException("Verification requires arguments[3]."));
        arguments[4] = (ushort)0;
        int bank = (int)new SnesAddress((enemy ?? throw new InvalidOperationException("Verification requires enemy.")).Definition.Bank, 0);
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
