using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Frontend;

/// <summary>Cartridge definitions for the final assembling Super Metroid icon.</summary>
internal static class EndingLogoDefinitions
{

    /// <summary>$8B:EF81: upper S, initialized by F18F, moved by F1E7, displaying list EE5D.</summary>
    private static EndingLogoActorDefinition UpperS => new(0xf1e7, 0xee5d);
    /// <summary>$8B:EF87: lower S, initialized by F1A8, moved by F227, displaying list EE65.</summary>
    private static EndingLogoActorDefinition LowerS => new(0xf227, 0xee65);
    /// <summary>$8B:EF8D: upper circle, initialized by F1C1, stationary with right-wrap list EE6D.</summary>
    private static EndingLogoActorDefinition UpperCircle => new(EndingRewardActorDefinitions.SharedNoOp, 0xee6d);
    /// <summary>$8B:EF93: lower circle, initialized by F1D4, stationary with left-wrap list EE87.</summary>
    private static EndingLogoActorDefinition LowerCircle => new(EndingRewardActorDefinitions.SharedNoOp, 0xee87);

    /// <summary>Named native actor cases; allocation index is not a sampled numeric curve.</summary>
    public static EndingLogoActorDefinition Actor(int index) => (EndingLogoActorKind)index switch
    {
        EndingLogoActorKind.UpperS => UpperS,
        EndingLogoActorKind.LowerS => LowerS,
        EndingLogoActorKind.UpperCircle => UpperCircle,
        EndingLogoActorKind.LowerCircle => LowerCircle,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
    /// <summary>F18F–F1D4 initialize these world-space centers before the $100 scroll subtraction.</summary>
    public static Position16<ScreenPixels> Origin(int actor) => (EndingLogoActorKind)actor switch
    {
        EndingLogoActorKind.UpperS => new(530, 231),
        EndingLogoActorKind.LowerS => new(246, 519),
        EndingLogoActorKind.UpperCircle => new(385, 366),
        EndingLogoActorKind.LowerCircle => new(391, 384),
        _ => throw new ArgumentOutOfRangeException(nameof(actor)),
    };
    /// <summary>E504 sets both layer-one camera coordinates to $100; the actors draw at their world centers minus it.</summary>
    public static Offset16<ScreenPixels> Camera => new(256, 256);
    /// <summary>F18F/F1A8 initialize the S-half motion scratch word to eight.</summary>
    public const int InitialSpeed = 8;
    /// <summary>F1E7/F227 accelerate the approaching S halves by two pixels per call.</summary>
    public const int Acceleration = 2;
    /// <summary>F1E7 clamps the upper S half at world (394,367).</summary>
    public static Position16<ScreenPixels> TopLanding => new(394, 367);
    /// <summary>F227 clamps the lower S half at world (382,383).</summary>
    public static Position16<ScreenPixels> BottomLanding => new(382, 383);
    /// <summary>E58A completes after sixteen palette pointer pairs.</summary>
    public const int PaletteSteps = 16;
    /// <summary>E504 initializes OBJ palette seven from $8C:EFE9. The sixteen base
    /// colors are the logo drawing's authored palette, retained under #1165's nonsense
    /// exception after original tile/OAM inspection; the temporal fade is calculated
    /// by EndingLogoPaletteFade. This is not an exemption for animation colors.</summary>
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
    ushort PreInstruction,
    ushort InstructionList);

/// <summary>Four mutually exclusive native logo roles, in E554..E569 allocation order.</summary>
internal enum EndingLogoActorKind
{
    /// <summary>Upper S half approaches the final logo from its initial off-center position.</summary>
    UpperS,
    /// <summary>Lower S half moves toward the opposite landing point.</summary>
    LowerS,
    /// <summary>Stationary upper circle uses the right-wrap display list.</summary>
    UpperCircle,
    /// <summary>Stationary lower circle uses the left-wrap display list.</summary>
    LowerCircle,
}
