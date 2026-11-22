using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>The connection kind a grapple connection makes and the pose it puts Samus in.</summary>
/// <param name="Kind">What the beam has hold of.</param>
/// <param name="Pose">The pose the connection puts Samus in.</param>
internal readonly partial record struct GrappleConnection(GrappleConnectionKind Kind, SamusPoseId Pose);
