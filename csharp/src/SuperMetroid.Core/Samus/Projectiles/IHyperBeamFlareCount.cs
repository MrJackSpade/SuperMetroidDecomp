using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The Hyper Beam's backward count of one charge-flare component's pictures, whatever its pictures (#2553).</summary>
internal interface IHyperBeamFlareCount
{
    /// <summary>The composition the counted picture shows in for Samus's facing, or none once the count has stepped back past the first.</summary>
    ProjectileSpriteId? Composition(bool facingLeft);

    /// <summary>
    /// Counts one update back: once the picture's updates run out, the picture before it shows for
    /// <see cref="ChargeFlareAnimationDefinitions.HyperBeamPictureUpdates"/>, the first picture for
    /// <see cref="ChargeFlareAnimationDefinitions.HyperBeamFirstPictureUpdates"/>, and past the first the component shows
    /// nothing.
    /// </summary>
    /// <returns>Whether the update reached the component's first picture.</returns>
    bool PlayBackward();
}
