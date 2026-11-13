using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Fx.PaletteFx;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Definitions for the final assembling Super Metroid icon.</summary>
internal static class EndingLogoDefinitions
{
    /// <summary>The colouring every logo actor draws in.</summary>
    internal const EndingSpriteColouring Colouring = EndingSpriteColouring.Logo;

    /// <summary>Upper S, moving into place, showing LogoUpperS.</summary>
    internal static CinematicActorDefinition UpperS => new(CinematicSpriteFrameBehavior.SuperMetroidIconTopHalf, CinematicSpriteSegment.LogoUpperS);
    /// <summary>Lower S, moving into place, showing LogoLowerS.</summary>
    internal static CinematicActorDefinition LowerS => new(CinematicSpriteFrameBehavior.SuperMetroidIconBottomHalf, CinematicSpriteSegment.LogoLowerS);
    /// <summary>Upper circle, stationary, running LogoUpperCircle.</summary>
    internal static CinematicActorDefinition UpperCircle => new(CinematicSpriteFrameBehavior.NoOp, CinematicSpriteSegment.LogoUpperCircle);
    /// <summary>Lower circle, stationary, running LogoLowerCircle.</summary>
    internal static CinematicActorDefinition LowerCircle => new(CinematicSpriteFrameBehavior.NoOp, CinematicSpriteSegment.LogoLowerCircle);

    /// <summary>The logo actors in allocation order.</summary>
    internal static readonly EndingLogoActorKind[] ActorKinds =
        [EndingLogoActorKind.UpperS, EndingLogoActorKind.LowerS, EndingLogoActorKind.UpperCircle, EndingLogoActorKind.LowerCircle];
    /// <summary>The logo scene sets both camera coordinates to 256; the actors draw at their world centers minus it.</summary>
    public static Offset16<ScreenPixels> Camera => new(256, 256);
    /// <summary>The S halves' initializers set their motion step to eight.</summary>
    public const int InitialSpeed = 8;
    /// <summary>The approaching S halves accelerate by two pixels per call.</summary>
    public const int Acceleration = 2;
    /// <summary>The upper S half stops at world (394,367).</summary>
    public static Position16<ScreenPixels> TopLanding => new(394, 367);
    /// <summary>The lower S half stops at world (382,383).</summary>
    public static Position16<ScreenPixels> BottomLanding => new(382, 383);
    /// <summary>The logo's colour cycle completes after sixteen colour pairs.</summary>
    public const int PaletteSteps = 16;
    /// <summary>The upper S spawns the logo's glare palette-FX object at landing.</summary>
    public const PaletteFxObject LandingPaletteFx = PaletteFxObject.PostCreditsSuperMetroidIcon;
}
