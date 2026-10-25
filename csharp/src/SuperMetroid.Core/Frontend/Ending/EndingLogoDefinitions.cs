using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Fx.PaletteFx;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Cartridge definitions for the final assembling Super Metroid icon.</summary>
internal static class EndingLogoDefinitions
{

    /// <summary>$8B:EF81: upper S, initialized by F18F, moved by F1E7, showing LogoUpperS.</summary>
    private static CinematicActorDefinition UpperS => new(CinematicSpritePreInstruction.SuperMetroidIconTopHalf, CinematicSpriteProgram.LogoUpperS);
    /// <summary>$8B:EF87: lower S, initialized by F1A8, moved by F227, showing LogoLowerS.</summary>
    private static CinematicActorDefinition LowerS => new(CinematicSpritePreInstruction.SuperMetroidIconBottomHalf, CinematicSpriteProgram.LogoLowerS);
    /// <summary>$8B:EF8D: upper circle, initialized by F1C1, stationary running LogoUpperCircle.</summary>
    private static CinematicActorDefinition UpperCircle => new(CinematicSpritePreInstruction.NoOp, CinematicSpriteProgram.LogoUpperCircle);
    /// <summary>$8B:EF93: lower circle, initialized by F1D4, stationary running LogoLowerCircle.</summary>
    private static CinematicActorDefinition LowerCircle => new(CinematicSpritePreInstruction.NoOp, CinematicSpriteProgram.LogoLowerCircle);

    /// <summary>The logo actors in allocation order.</summary>
    private static readonly EndingLogoActorKind[] ActorKinds =
        [EndingLogoActorKind.UpperS, EndingLogoActorKind.LowerS, EndingLogoActorKind.UpperCircle, EndingLogoActorKind.LowerCircle];

    /// <summary>The logo actor allocated at <paramref name="index"/>.</summary>
    private static EndingLogoActorKind KindAt(int index, string parameter) =>
        (uint)index < (uint)ActorKinds.Length ? ActorKinds[index] : throw new ArgumentOutOfRangeException(parameter);

    /// <summary>Named native actor cases; allocation index is not a sampled numeric curve.</summary>
    public static CinematicActorDefinition Actor(int index) => KindAt(index, nameof(index)) switch
    {
        EndingLogoActorKind.UpperS => UpperS,
        EndingLogoActorKind.LowerS => LowerS,
        EndingLogoActorKind.UpperCircle => UpperCircle,
        EndingLogoActorKind.LowerCircle => LowerCircle,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
    /// <summary>F18F–F1D4 initialize these world-space centers before the $100 scroll subtraction.</summary>
    public static Position16<ScreenPixels> Origin(int actor) => KindAt(actor, nameof(actor)) switch
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
    /// <summary>F1E7 spawns the logo's palette-FX object $8D:E200 at landing.</summary>
    public const PaletteFxObject LandingPaletteFx = PaletteFxObject.PostCreditsSuperMetroidIcon;
}
