namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>Compiled fixed behavior definitions for ordinary Metroids.</summary>
internal static class MetroidBehaviorDefinitions
{
    /// <summary>Library-two cry $50, selected by native random slots0/3 at $A3:EAD6/$EADC.</summary>
    private const ushort FirstCry = 0x0050;
    /// <summary>Library-two cry $58, selected by native random slots1/4/6 at $A3:EAD8/$EADE/$EAE2.</summary>
    private const ushort SecondCry = 0x0058;
    /// <summary>Library-two cry $5A, selected by native random slots2/5/7 at $A3:EADA/$EAE0/$EAE4.</summary>
    private const ushort ThirdCry = 0x005a;

    /// <summary><c>BombedOffVelocities</c> at $A3:EA3F: four successive two-pixel cardinal displacements.</summary>
    internal static Offset16<RoomPixels> EscapeDisplacement(ushort countdown)
    {
        int phase = countdown & 3;
        int signedRadius = 2 - 2 * (phase & 2);
        return (phase & 1) == 0 ? new((short)signedRadius, 0) : new(0, (short)-signedRadius);
    }

    /// <summary><c>Instruction_Metroid_PlayRandomMetroidSFX.SFX</c> at $A3:EAD6 preserves the native2/3/3 choice distribution and order.</summary>
    internal static ushort RandomCrySoundEffect(ushort random) => (random & 7) switch
    {
        0 or 3 => FirstCry,
        1 or 4 or 6 => SecondCry,
        _ => ThirdCry,
    };

    /// <summary>Gravity Suit Samus loses one health each time this fraction underflows the drain accumulator.</summary>
    internal const ushort GravityDrainFraction = 0x3000;

    /// <summary>Varia Suit Samus's drain fraction.</summary>
    internal const ushort VariaDrainFraction = 0x6000;

    /// <summary>Suitless Samus's drain fraction.</summary>
    internal const ushort PowerSuitDrainFraction = 0xc000;
}
