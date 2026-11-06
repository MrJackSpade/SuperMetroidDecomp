using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Fx.PaletteFx;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Definitions for the final assembling Super Metroid icon.</summary>
internal static class EndingLogoDefinitions
{

    /// <summary>Upper S, moving into place, showing LogoUpperS.</summary>
    internal static CinematicActorDefinition UpperS => new(CinematicSpriteFrameBehavior.SuperMetroidIconTopHalf, CinematicSpriteProgram.LogoUpperS);
    /// <summary>Lower S, moving into place, showing LogoLowerS.</summary>
    internal static CinematicActorDefinition LowerS => new(CinematicSpriteFrameBehavior.SuperMetroidIconBottomHalf, CinematicSpriteProgram.LogoLowerS);
    /// <summary>Upper circle, stationary, running LogoUpperCircle.</summary>
    internal static CinematicActorDefinition UpperCircle => new(CinematicSpriteFrameBehavior.NoOp, CinematicSpriteProgram.LogoUpperCircle);
    /// <summary>Lower circle, stationary, running LogoLowerCircle.</summary>
    internal static CinematicActorDefinition LowerCircle => new(CinematicSpriteFrameBehavior.NoOp, CinematicSpriteProgram.LogoLowerCircle);

    /// <summary>The logo actors in allocation order.</summary>
    internal static readonly EndingLogoActorKind[] ActorKinds =
        [EndingLogoActorKind.UpperS, EndingLogoActorKind.LowerS, EndingLogoActorKind.UpperCircle, EndingLogoActorKind.LowerCircle];
    /// <summary>E504 sets both camera coordinates to $100; the actors draw at their world centers minus it.</summary>
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
    /// <summary>The upper S spawns the logo's glare palette-FX object at landing.</summary>
    public const PaletteFxObject LandingPaletteFx = PaletteFxObject.PostCreditsSuperMetroidIcon;
}
