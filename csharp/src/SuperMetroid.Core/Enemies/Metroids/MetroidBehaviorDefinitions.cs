namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>Compiled fixed behavior definitions for ordinary Metroids.</summary>
internal static class MetroidBehaviorDefinitions
{
    /// <summary>Library-two cry $50, selected by native random slots0/3 at $A3:EAD6/$EADC.</summary>
    private static readonly SoundEffectId FirstCry = Library2Sound.MetroidDrain;
    /// <summary>Library-two cry $58, selected by native random slots1/4/6 at $A3:EAD8/$EADE/$EAE2.</summary>
    private static readonly SoundEffectId SecondCry = Library2Sound.ShotMochtroid;
    /// <summary>Library-two cry $5A, selected by native random slots2/5/7 at $A3:EADA/$EAE0/$EAE4.</summary>
    private static readonly SoundEffectId ThirdCry = Library2Sound.ShotMetroid;

    /// <summary><c>BombedOffVelocities</c> at $A3:EA3F: four successive two-pixel cardinal displacements.</summary>
    internal static Offset16<RoomPixels> EscapeDisplacement(int countdown)
    {
        int phase = countdown & 3;
        int signedRadius = 2 - 2 * (phase & 2);
        return (phase & 1) == 0 ? new((short)signedRadius, 0) : new(0, (short)-signedRadius);
    }

    /// <summary><c>Instruction_Metroid_PlayRandomMetroidSFX.SFX</c> at $A3:EAD6 preserves the native2/3/3 choice distribution and order.</summary>
    internal static SoundEffectId RandomCrySoundEffect(int random) => (random & 7) switch
    {
        0 or 3 => FirstCry,
        1 or 4 or 6 => SecondCry,
        _ => ThirdCry,
    };

    /// <summary>A sixteenth of a health point, in the units of the drain accumulator.</summary>
    private const ushort SixteenthHealth = 0x1000;

    /// <summary>Gravity Suit Samus loses one health each time this fraction underflows the drain accumulator: three sixteenths.</summary>
    internal const ushort GravityDrainFraction = 3 * SixteenthHealth;

    /// <summary>Varia Suit Samus's drain fraction: six sixteenths.</summary>
    internal const ushort VariaDrainFraction = 6 * SixteenthHealth;

    /// <summary>Suitless Samus's drain fraction: twelve sixteenths.</summary>
    internal const ushort PowerSuitDrainFraction = 12 * SixteenthHealth;
}
