using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One of the four Rinkas of the Mother Brain flashback: the sprite it shows and what its launch chose.</summary>
internal sealed partial class IntroRinka
{
    /// <summary>Creates the Rinka of <paramref name="launch"/> where it appears, running the Rinka animation.</summary>
    /// <param name="launch">The Rinka's launch: where it appears, its drift and how it flies.</param>
    public IntroRinka(IntroRinkaLaunch launch)
    {
        Flight = launch.Flight;
        Drift = launch.Drift;
        Sprite = new(launch.Position, IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors), IntroRinkaAnimations.Rinka,
            IntroRinkaDefinitions.RinkaActor.Animation);
    }

    /// <summary>The sprite showing the Rinka: its position and animation.</summary>
    internal CinematicActor<IntroRinkaAnimation, CinematicSpritePose, IntroRinkaCue> Sprite { get; }

    /// <summary>How the Rinka flies once it starts moving: only the first one launched strikes Samus, the others miss.</summary>
    internal IntroRinkaFlight Flight { get; }

    /// <summary>Whether the Rinka has started moving: it holds still until its animation starts it.</summary>
    internal bool IsMoving { get; private set; }

    /// <summary>Starts the Rinka moving on its flight.</summary>
    internal void StartMoving() => IsMoving = true;

    /// <summary>How far the Rinka moves sideways each update.</summary>
    internal SignedSixteenSixteen Drift { get; }
}
