using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One of the four Rinkas of the Mother Brain flashback: the sprite it shows and what its launch chose.</summary>
internal sealed class IntroRinka
{
    /// <summary>Creates the Rinka of <paramref name="launch"/> at its authored place, running the Rinka program.</summary>
    /// <param name="launch">The launch, 0 through 3, in spawn order.</param>
    public IntroRinka(int launch)
    {
        IntroRinkaPhysicalDefinition definition = IntroRinkaLaunches.Rinka(launch);
        HitsSamus = launch == 0;
        XWholeVelocity = definition.XWholeVelocity;
        Sprite = new IntroDiscoverySprite(
            definition.Position,
            colouring: IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors),
            program: IntroRinkaDefinitions.RinkaActor.Animation);
        Sprite.InstallFrameBehavior(IntroRinkaDefinitions.RinkaActor.FrameBehavior);
    }

    /// <summary>The sprite showing the Rinka: its position and program.</summary>
    internal IntroDiscoverySprite Sprite { get; }

    /// <summary>Whether the Rinka takes the route that strikes Samus; only the first launch does, the others miss.</summary>
    internal bool HitsSamus { get; }

    /// <summary>The whole pixels the Rinka moves sideways each update, before its half pixel.</summary>
    internal int XWholeVelocity { get; }
}
