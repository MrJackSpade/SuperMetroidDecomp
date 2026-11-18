using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// One of the swing poses' pictures of Samus hanging from the rope. The swing poses hold one picture drawn for each
/// swing direction, in direction order; the installation chooses which picture each direction shows.
/// </summary>
/// <param name="DrawnFor">The swing direction the picture shows Samus hanging in.</param>
public readonly record struct GrappleSwingPicture(GrappleSwingDirection DrawnFor)
{
    /// <summary>The frame of the swing pose's animation that holds this picture: one frame for each direction, in direction order.</summary>
    internal SamusAnimationFrame AnimationFrame => new(SamusAnimationSegmentName.Main, DrawnFor.Number);
}
