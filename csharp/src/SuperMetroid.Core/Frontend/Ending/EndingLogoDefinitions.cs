using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Fx.ColourEffects;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Definitions for the final assembling Super Metroid icon.</summary>
internal static class EndingLogoDefinitions
{
    /// <summary>The colouring every logo actor draws in.</summary>
    internal const EndingSpriteColouring Colouring = EndingSpriteColouring.Logo;

    /// <summary>The upper S half, which starts above the screen's top right and moves down and left into place.</summary>
    internal static EndingLogoActorDefinition UpperS { get; } = new(EndingLogoAnimation.UpperS, new(274, -25));
    /// <summary>The lower S half, which starts past the screen's bottom left and moves up and right into place.</summary>
    internal static EndingLogoActorDefinition LowerS { get; } = new(EndingLogoAnimation.LowerS, new(-10, 263));
    /// <summary>The upper circle, which holds still while it draws in.</summary>
    internal static EndingLogoActorDefinition UpperCircle { get; } = new(EndingLogoAnimation.UpperCircle, new(129, 110));
    /// <summary>The lower circle, which holds still while it draws in.</summary>
    internal static EndingLogoActorDefinition LowerCircle { get; } = new(EndingLogoAnimation.LowerCircle, new(135, 128));

    /// <summary>The logo's actors in the order they spawn, step and draw.</summary>
    internal static IReadOnlyList<EndingLogoActorDefinition> Actors { get; } = [UpperS, LowerS, UpperCircle, LowerCircle];

    /// <summary>The motion step the S halves start with.</summary>
    public const int InitialSpeed = 8;
    /// <summary>How much the approaching S halves accelerate each update, in pixels.</summary>
    public const int Acceleration = 2;
    /// <summary>Where the upper S half stops on the screen.</summary>
    public static Offset16<ScreenPixels> TopLanding => new(138, 111);
    /// <summary>Where the lower S half stops on the screen.</summary>
    public static Offset16<ScreenPixels> BottomLanding => new(126, 127);
    /// <summary>The colour pairs after which the logo's colour cycle completes.</summary>
    public const int CrossfadeSteps = 16;
    /// <summary>The upper S spawns the logo's glare colour effect at landing.</summary>
    public const ColourEffectId LandingColourEffect = ColourEffectId.PostCreditsSuperMetroidIcon;
}
