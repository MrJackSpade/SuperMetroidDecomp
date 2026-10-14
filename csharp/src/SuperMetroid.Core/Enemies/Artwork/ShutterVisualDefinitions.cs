
namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Five distinct bank-$A2 shutter OAM compositions. The forty-pixel frame is
/// shared by the growing and plain vertical shutter programs; motion, collision,
/// shot reactions and program timing remain compiled gameplay behavior.
/// </summary>
internal static class ShutterVisualDefinitions
{
    /// <summary>Native shutter sprite-map bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>
    /// Supplies the five shutter OAM compositions: three growing heights, the shared
    /// forty-pixel vertical frame, and the horizontal frame.
    /// </summary>
    internal static EnemySpritemapDefinition[] Frames() =>
    [
        new(Bank, GrowingFrame(0), "shutter_growing_10px"),
        new(Bank, GrowingFrame(1), "shutter_growing_20px"),
        new(Bank, GrowingFrame(2), "shutter_growing_30px"),
        new(Bank, GrowingFrame(3), "shutter_vertical_40px"),
        new(Bank, HorizontalFrame, "shutter_horizontal"),
    ];

    /// <summary><c>Spritemap_Shutters_Horizontal</c> at $A2:EDB1.</summary>
    private const ushort HorizontalFrame = 0xedb1;

    /// <summary>
    /// Finds the bank-$A2 spritemap pointer for a growing shutter stage. Each preceding
    /// stage contributes a used map of 2+5n bytes and an unused intermediate map of
    /// 2+5(n+1) bytes, with n starting at one; the first used map begins at $A2:ED44.
    /// </summary>
    private static ushort GrowingFrame(int stage) => (ushort)(0xed44 + 19 * stage + 5 * stage * (stage - 1));

}
