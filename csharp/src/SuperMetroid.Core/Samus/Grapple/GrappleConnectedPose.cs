using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>The pose a grapple connection gives Samus and whether she swings.</summary>
/// <param name="Pose">The pose.</param>
/// <param name="Swinging">The swinging.</param>
[IndependentFlag("whether the connection swings, a fact of the connection handler")]
internal readonly record struct GrappleConnectedPose(SamusPoseId Pose, bool Swinging);
