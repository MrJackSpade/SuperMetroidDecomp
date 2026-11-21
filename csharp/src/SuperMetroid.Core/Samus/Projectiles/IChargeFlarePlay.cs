using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A charge-flare component's ordinary animation, whatever its pictures (#2553, <see cref="ChargeFlarePlay{TSegment, TPicture}"/>).</summary>
internal interface IChargeFlarePlay
{
    /// <summary>The composition the component's picture shows in for Samus's facing.</summary>
    ProjectileSpriteId Composition(HorizontalDirection facing);

    /// <summary>The ordinary flare's first update: the component shows its first picture for that picture's own updates.</summary>
    void Start();

    /// <summary>Plays the ordinary flare one update on: once the picture's updates run out, the next picture shows.</summary>
    void PlayForward();

    /// <summary>Returns the component to its first picture, its wait spent.</summary>
    void Clear();

    /// <summary>
    /// The Hyper Beam's shot: a backward count of the component's pictures, showing its last picture for
    /// <see cref="ChargeFlareAnimationDefinitions.HyperBeamPictureUpdates"/>.
    /// </summary>
    IHyperBeamFlareCount CountBackwardFromLast();

    /// <summary>
    /// The Baby's gift catching the flare: a backward count taking the component over where it stands, its picture keeping
    /// one update less of the wait it had left, and at least one.
    /// </summary>
    IHyperBeamFlareCount CountBackwardFromHere();
}
