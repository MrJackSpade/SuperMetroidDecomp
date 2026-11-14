
namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>The connection kind a grapple connection makes and the pose it puts Samus in.</summary>
/// <param name="Kind">The connection kind.</param>
/// <param name="Pose">The pose.</param>
internal readonly record struct GrappleConnection(GrappleConnectionKind Kind, GrappleConnectionPose Pose);
