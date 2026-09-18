namespace SuperMetroid.Core.Game;

/// <summary>One random Zebes-escape explosion sprite and its optional sound.</summary>
internal readonly record struct ZebesEscapeExplosionDefinition(
    RoomSpriteObjectKind SpriteKind,
    byte SoundEffect);

/// <summary>Compiled fixed definitions for the eight bank-$8F escape explosions.</summary>
internal static class ZebesEscapeExplosionDefinitions
{
    /// <summary>
    /// Sprite-object IDs at <c>$8F:C1D6-$C1DD</c> paired with library-two sound IDs at
    /// <c>$8F:C1DE-$C1E5</c>.
    /// </summary>
    private static readonly ZebesEscapeExplosionDefinition[] Definitions =
    [
        new((RoomSpriteObjectKind)0x03, 0x24),
        new((RoomSpriteObjectKind)0x03, 0x00),
        new((RoomSpriteObjectKind)0x09, 0x00),
        new((RoomSpriteObjectKind)0x0c, 0x25),
        new((RoomSpriteObjectKind)0x0c, 0x00),
        new((RoomSpriteObjectKind)0x12, 0x00),
        new((RoomSpriteObjectKind)0x12, 0x00),
        new((RoomSpriteObjectKind)0x15, 0x00),
    ];

    /// <summary>Returns one explosion definition selected by the native low three bits.</summary>
    internal static ZebesEscapeExplosionDefinition ForIndex(int index)
    {
        if ((uint)index >= Definitions.Length)
        {
            throw new InvalidDataException(
                $"Zebes escape explosion index {index} is outside eight definitions.");
        }

        return Definitions[index];
    }
}
