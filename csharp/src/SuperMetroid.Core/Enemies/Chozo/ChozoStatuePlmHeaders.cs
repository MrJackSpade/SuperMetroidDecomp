namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Finds the definition of a PLM a Chozo statue publishes.</summary>
internal static class ChozoStatuePlmHeaders
{
    /// <summary>Resolves a supported header without interpreting adjacent bank-$84 data.</summary>
    /// <exception cref="InvalidDataException"><paramref name="header"/> is not one of the five Chozo PLMs.</exception>
    internal static ChozoStatuePlmDefinition Resolve(PlmHeaderId header) =>
        ChozoStatuePlmDefinitions.ByHeader.TryGetValue(header, out ChozoStatuePlmDefinition definition)
            ? definition
            : throw new InvalidDataException($"Unsupported Chozo PLM {header}.");
}
