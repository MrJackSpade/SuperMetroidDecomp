using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Fx.ColourEffects;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Definitions for the final assembling Super Metroid icon.</summary>
internal static class EndingLogoDefinitions
{
    /// <summary>The colouring every logo actor draws in.</summary>
    internal const EndingSpriteColouring Colouring = EndingSpriteColouring.Logo;

    /// <summary>The upper S half, which moves down and left into place.</summary>
    internal static EndingLogoActorDefinition UpperS { get; } = new(CinematicSpriteSegment.LogoUpperS, new(530, 231));
    /// <summary>The lower S half, which moves up and right into place.</summary>
    internal static EndingLogoActorDefinition LowerS { get; } = new(CinematicSpriteSegment.LogoLowerS, new(246, 519));
    /// <summary>The upper circle, which holds still while it draws in.</summary>
    internal static EndingLogoActorDefinition UpperCircle { get; } = new(CinematicSpriteSegment.LogoUpperCircle, new(385, 366));
    /// <summary>The lower circle, which holds still while it draws in.</summary>
    internal static EndingLogoActorDefinition LowerCircle { get; } = new(CinematicSpriteSegment.LogoLowerCircle, new(391, 384));

    /// <summary>The logo's actors in the order they spawn, step and draw.</summary>
    internal static IReadOnlyList<EndingLogoActorDefinition> Actors { get; } = [UpperS, LowerS, UpperCircle, LowerCircle];

    /// <summary>The logo scene sets both camera coordinates to 256; the actors draw at their world centers minus it.</summary>
    public static Offset16<ScreenPixels> Camera => new(256, 256);
    /// <summary>The S halves start with a motion step of eight.</summary>
    public const int InitialSpeed = 8;
    /// <summary>The approaching S halves accelerate by two pixels an update.</summary>
    public const int Acceleration = 2;
    /// <summary>The upper S half stops at world (394,367).</summary>
    public static Offset16<ScreenPixels> TopLanding => new(394, 367);
    /// <summary>The lower S half stops at world (382,383).</summary>
    public static Offset16<ScreenPixels> BottomLanding => new(382, 383);
    /// <summary>The logo's colour cycle completes after sixteen colour pairs.</summary>
    public const int CrossfadeSteps = 16;
    /// <summary>The upper S spawns the logo's glare colour effect at landing.</summary>
    public const ColourEffectId LandingColourEffect = ColourEffectId.PostCreditsSuperMetroidIcon;
}
