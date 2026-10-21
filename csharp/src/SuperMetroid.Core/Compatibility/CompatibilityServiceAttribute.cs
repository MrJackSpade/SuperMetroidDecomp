namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Marks a compatibility service: the only form in which behaviour of the original hardware survives in Core. A service is
/// named for what it does for the game, owns its own typed state, is injected at the session's composition root, and its
/// summary states the gameplay behaviour it reproduces, the point in the update at which it is consulted, and the evidence
/// (a recording or the original routine) that established it (#1401, <c>docs/compatibility-layer.md</c>).
/// </summary>
/// <param name="behaviour">The gameplay behaviour the service reproduces, in the game's terms.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false)]
public sealed class CompatibilityServiceAttribute(string behaviour) : Attribute
{
    /// <summary>The gameplay behaviour the service reproduces.</summary>
    public string Behaviour { get; } = behaviour;
}
