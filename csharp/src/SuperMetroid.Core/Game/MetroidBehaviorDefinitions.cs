namespace SuperMetroid.Core.Game;

/// <summary>One world-position displacement during Metroid's Power Bomb escape shake.</summary>
internal readonly record struct MetroidEscapeDisplacement(short X, short Y);

/// <summary>Compiled fixed behavior definitions for ordinary Metroids.</summary>
internal static class MetroidBehaviorDefinitions
{
    /// <summary>
    /// Four X/Y displacement pairs from <c>BombedOffVelocities</c> at
    /// <c>$A3:EA3F-$A3:EA4E</c>. The escape countdown selects a frame with its low two bits.
    /// </summary>
    private static readonly MetroidEscapeDisplacement[] EscapeDisplacements =
    [
        new(2, 0),
        new(0, -2),
        new(-2, 0),
        new(0, 2),
    ];

    /// <summary>
    /// Library-two Metroid cry IDs from <c>Instruction_Metroid_PlayRandomMetroidSFX.SFX</c>
    /// at <c>$A3:EAD6-$A3:EAE5</c>.
    /// </summary>
    private static readonly ushort[] RandomCrySoundEffects =
    [
        0x0050,
        0x0058,
        0x005a,
        0x0050,
        0x0058,
        0x005a,
        0x0058,
        0x005a,
    ];

    /// <summary>Returns the native displacement selected by the countdown's low two bits.</summary>
    internal static MetroidEscapeDisplacement EscapeDisplacement(ushort countdown) =>
        EscapeDisplacements[countdown & 3];

    /// <summary>Returns the native cry selected by the random word's low three bits.</summary>
    internal static ushort RandomCrySoundEffect(ushort random) =>
        RandomCrySoundEffects[random & 7];
}
