namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>Finds a Puyo's hop by its native byte index.</summary>
public static class PuyoHops
{
    /// <summary>Preserves native byte-index state while returning named fields.</summary>
    /// <exception cref="InvalidDataException"><paramref name="index"/> is not one of the seven records.</exception>
    public static PuyoHopDefinition FromByteIndex(int index) =>
        PuyoHopDefinitions.Hops.TryGetValue(index, out PuyoHopDefinition hop)
            ? hop
            : throw new InvalidDataException($"Puyo hop byte index ${index:X4} is outside seven authored records.");
}
