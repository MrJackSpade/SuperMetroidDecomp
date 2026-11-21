namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Marks a compatibility service: the only form in which behaviour of the original hardware survives in Core. A service is
/// named for what it does for the game, owns its own typed state, is injected at the session's composition root, and its
/// summary states the gameplay behaviour it reproduces, the point in the update at which it is consulted, and the evidence
/// (a recording or the game's code) that established it (#1401).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false)]
public sealed partial class CompatibilityServiceAttribute : Attribute
{
    /// <summary>
    /// Marks a compatibility service. The behaviour argument exists only for the analyzers, which read it from the
    /// attribute's syntax; nothing reads it at run time, so the attribute stores nothing, and the constructor only rejects an
    /// empty description where reflection constructs the attribute.
    /// </summary>
    /// <param name="behaviour">The gameplay behaviour the service reproduces, in the game's terms.</param>
    public CompatibilityServiceAttribute(string behaviour) => ArgumentException.ThrowIfNullOrWhiteSpace(behaviour);
}
