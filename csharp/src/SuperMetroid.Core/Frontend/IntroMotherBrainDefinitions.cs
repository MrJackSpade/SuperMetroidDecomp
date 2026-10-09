namespace SuperMetroid.Core.Frontend;

/// <summary>Fixed actor and explosion-layout definitions for the intro Mother Brain scene.</summary>
internal static class IntroMotherBrainDefinitions
{

    /// <summary><c>$8B:CE55</c>, intro Mother Brain initialization, pre-instruction, and initial list.</summary>
    public static IntroMotherBrainActorDefinition MotherBrain =>
        new(0xcb05);

    /// <summary><c>$8B:CF15</c>, large explosion initialization, pre-instruction, and initial list.</summary>
    public static IntroMotherBrainActorDefinition BigExplosionActor =>
        new(0xcdab);

    /// <summary><c>$8B:CF1B</c>, small explosion initialization, pre-instruction, and initial list.</summary>
    public static IntroMotherBrainActorDefinition SmallExplosionActor =>
        new(0xcdcb);

    /// <summary><c>$8B:B773</c> fixes Mother Brain's cinematic origin at (56,111).</summary>
    public static (ushort X, ushort Y) MotherBrainOrigin => (0x0038, 0x006f);

    /// <summary>Number of records in the <c>$8B:B9B6-$B9D3</c> large-explosion tables.</summary>
    public const int BigExplosionCount = 5;

    /// <summary>Number of records in the <c>$8B:B9FD-$BA0E</c> small-explosion tables.</summary>
    public const int SmallExplosionCount = 3;

    /// <summary>Returns one complete <c>$8B:B98D</c> large-explosion placement record.</summary>
    public static IntroMotherBrainExplosionPlacement BigExplosion(int index) => index switch
    {
        0 => new(0, 0, 1),
        1 => new(16, -16, 16),
        2 => new(-16, 8, 32),
        3 => new(-8, -16, 48),
        4 => new(8, 8, 64),
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>Returns one complete <c>$8B:B9D4</c> small-explosion placement record.</summary>
    public static IntroMotherBrainExplosionPlacement SmallExplosion(int index) => index switch
    {
        0 => new(16, 0, 1),
        1 => new(-16, 4, 8),
        2 => new(-16, -8, 16),
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
}

/// <summary>One native six-byte bank-$8B cinematic-object definition.</summary>
/// <param name="InstructionList">Bank-$8B instruction-list address from which the cinematic object begins execution.</param>
internal readonly record struct IntroMotherBrainActorDefinition(
    ushort InstructionList);

/// <summary>One signed origin-offset and start-delay record for an intro explosion actor.</summary>
/// <param name="XOffset">Signed horizontal displacement from Mother Brain's cinematic origin.</param>
/// <param name="YOffset">Signed vertical displacement from Mother Brain's cinematic origin.</param>
/// <param name="StartTimer">Number of actor updates to wait before consuming its first instruction-list entry.</param>
internal readonly record struct IntroMotherBrainExplosionPlacement(
    short XOffset,
    short YOffset,
    ushort StartTimer);
