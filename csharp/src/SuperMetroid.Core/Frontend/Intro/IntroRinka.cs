using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One of the four Rinkas of the Mother Brain flashback: the sprite it shows and what its launch chose.</summary>
internal sealed class IntroRinka
{
    /// <summary>Creates the Rinka of <paramref name="launch"/> where it appears, running the Rinka animation.</summary>
    /// <param name="launch">The Rinka's launch: where it appears, its drift and how it flies.</param>
    public IntroRinka(IntroRinkaLaunch launch)
    {
        Flight = launch.Flight;
        Drift = launch.Drift;
        Sprite = new IntroDiscoverySprite(
            launch.Position,
            colouring: IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors),
            segment: IntroRinkaDefinitions.RinkaActor.Animation);
        Sprite.InstallFrameBehavior(IntroRinkaDefinitions.RinkaActor.FrameBehavior);
    }

    /// <summary>The sprite showing the Rinka: its position and animation.</summary>
    internal IntroDiscoverySprite Sprite { get; }

    /// <summary>How the Rinka flies once it starts moving: only the first one launched strikes Samus, the others miss.</summary>
    internal CinematicSpriteFrameBehavior Flight { get; }

    /// <summary>How far the Rinka moves sideways each update.</summary>
    internal SignedSixteenSixteen Drift { get; }
}
