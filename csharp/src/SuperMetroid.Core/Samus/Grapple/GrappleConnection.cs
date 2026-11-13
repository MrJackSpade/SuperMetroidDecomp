
namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>The function and handler a grapple connection runs.</summary>
/// <param name="Kind">The function.</param>
/// <param name="Handler">The handler.</param>
internal readonly record struct GrappleConnection(GrappleConnectionKind Kind, GrappleConnectionHandler Handler);
