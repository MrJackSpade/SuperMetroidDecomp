namespace SuperMetroid.Core.Enemies.Metaree;

/// <summary>Metaree's native property write whose general meaning is unproven.</summary>
internal static class MetareeDefinitions
{
    /// <summary>
    /// The two low property bits Metaree ORs on as it descends. Their engine-wide meaning is not yet proven, so they keep
    /// this Metaree-specific name rather than a flags member.
    /// </summary>
    internal const byte DescentPropertyBits = 3;
}
