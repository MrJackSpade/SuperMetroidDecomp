
namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>The connection kind and handler a grapple connection runs.</summary>
/// <param name="Kind">The connection kind.</param>
/// <param name="Handler">The handler.</param>
internal readonly record struct GrappleConnection(GrappleConnectionKind Kind, GrappleConnectionHandler Handler);
