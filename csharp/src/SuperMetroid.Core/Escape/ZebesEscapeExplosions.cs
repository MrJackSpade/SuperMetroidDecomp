namespace SuperMetroid.Core.Escape;

/// <summary>Chooses a Zebes escape explosion from the selector's weighted choices (<see cref="ZebesEscapeExplosionDefinitions"/>).</summary>
internal static class ZebesEscapeExplosions
{
    /// <summary>The explosion the native low-three-bit choice <paramref name="index"/> selects.</summary>
    internal static ZebesEscapeExplosionDefinition ForIndex(int index) =>
        (uint)index < ZebesEscapeExplosionDefinitions.Choices.Length
            ? ZebesEscapeExplosionDefinitions.Choices[index]
            : throw new InvalidDataException($"Zebes escape explosion index {index} is outside eight definitions.");
}
