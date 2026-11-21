namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The steps of Dachora's speed and stored-shine colour cycles.</summary>
public static class DachoraColorDefinitions
{
    /// <summary>The ordered images in each speed or stored-shine colour cycle.</summary>
    public const int AnimatedFrameCount = 4;

    /// <summary>The frames each colour phase shows: its normal colours one, the speed and stored-shine glows their animated frames.</summary>
    internal static readonly IReadOnlyDictionary<DachoraColourPhase, int> PhaseFrameCounts = new Dictionary<DachoraColourPhase, int>
    {
        [DachoraColourPhase.Default] = 1,
        [DachoraColourPhase.Speed] = AnimatedFrameCount,
        [DachoraColourPhase.Shine] = AnimatedFrameCount,
    };
}
