namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ceres Ridley's getaway: the zoom and drift at each step of the authored curves.</summary>
public static class CeresRidleyGetawayCurve
{
    /// <summary>Preserves the serialized native byte index, including its terminal entry.</summary>
    public static CeresRidleyGetawayFrame FromIndex(int frame) =>
        (uint)frame < (uint)CeresRidleyGetawayDefinitions.Frames.Length
            ? CeresRidleyGetawayDefinitions.Frames[frame]
            : throw new InvalidDataException($"Ceres Ridley getaway frame {frame} is outside its authored entries.");
}
