namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The ordinary and Mother Brain Rinkas' animation runs (#641, #2250), in authored order: each kind hides and fires in its
/// own run, and both then spin.
/// </summary>
internal enum RinkaSegment
{
    /// <summary>An ordinary Rinka hides, waits white hot and fires.</summary>
    Ordinary,
    /// <summary>A fired Rinka's spin through its colours, which it plays for good.</summary>
    Spin,
    /// <summary>Mother Brain's Rinka hides, still processed off screen, waits white hot and fires.</summary>
    Special,
}
