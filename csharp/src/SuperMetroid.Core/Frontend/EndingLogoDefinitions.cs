namespace SuperMetroid.Core.Frontend;

/// <summary>Cartridge definitions for the final assembling Super Metroid icon.</summary>
internal static class EndingLogoDefinitions
{
    /// <summary>Bank containing the native six-byte cinematic-object definitions.</summary>
    public const int NativeDefinitionBank = 0x8b0000;

    /// <summary>$8B:EF81/EF87/EF8D/EF93, in native allocation order.</summary>
    public static ReadOnlySpan<ushort> Actors => [0xef81, 0xef87, 0xef8d, 0xef93];

    /// <summary>Returns one complete native logo actor definition in allocation order.</summary>
    public static EndingLogoActorDefinition Actor(int index) => index switch
    {
        0 => new(0xef81, 0xf18f, 0xf1e7, 0xee5d),
        1 => new(0xef87, 0xf1a8, 0xf227, 0xee65),
        2 => new(0xef8d, 0xf1c1, EndingRewardActorDefinitions.SharedNoOp, 0xee6d),
        3 => new(0xef93, 0xf1d4, EndingRewardActorDefinitions.SharedNoOp, 0xee87),
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
    /// <summary>F18F–F1D4 initialize these world-space centers before the $100 scroll subtraction.</summary>
    public static (ushort X, ushort Y) Origin(int actor) => actor switch
    {
        0 => (530, 231), 1 => (246, 519), 2 => (385, 366), 3 => (391, 384),
        _ => throw new ArgumentOutOfRangeException(nameof(actor)),
    };
    /// <summary>E504 sets both layer-one camera coordinates to $100.</summary>
    public const ushort Camera = 256;
    /// <summary>F18F/F1A8 initialize the S-half motion scratch word to eight.</summary>
    public const int InitialSpeed = 8;
    /// <summary>F1E7/F227 accelerate the approaching S halves by two pixels per call.</summary>
    public const int Acceleration = 2;
    /// <summary>F1E7 clamps the upper S half at world (394,367).</summary>
    public static (ushort X, ushort Y) TopLanding => (394, 367);
    /// <summary>F227 clamps the lower S half at world (382,383).</summary>
    public static (ushort X, ushort Y) BottomLanding => (382, 383);
    /// <summary>$8B:F25E, circle actor instruction starting the logo palette crossfade.</summary>
    public const ushort GreyOutInstruction = 0xf25e;
    /// <summary>$8B:E5E7, sixteen pairs of reverse-copy bank-$8C palette pointers.</summary>
    public const int PaletteTable = 0x8be5e7;
    /// <summary>E58A completes after sixteen palette pointer pairs.</summary>
    public const int PaletteSteps = 16;
    /// <summary>E504 initializes OBJ palette seven from $8C:EFE9.</summary>
    public const int InitialPalette = 0x8cefe9;
    /// <summary>F1E7 spawns the logo's palette-FX object $8D:E200 at landing.</summary>
    public const ushort LandingPaletteFx = 0xe200;
    /// <summary>F25E selects the logo BG2 tilemap at VRAM word $5400.</summary>
    public const ushort Tilemap = 0x5400;
    /// <summary>F25E selects logo BG2 characters at VRAM word $6000.</summary>
    public const ushort Characters = 0x6000;
}

/// <summary>One native six-byte final-logo cinematic-object definition.</summary>
internal readonly record struct EndingLogoActorDefinition(
    ushort Pointer,
    ushort Initialization,
    ushort PreInstruction,
    ushort InstructionList);
