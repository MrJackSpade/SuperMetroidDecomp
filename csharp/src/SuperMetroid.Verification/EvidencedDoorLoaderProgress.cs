using SuperMetroid.Core.Runtime;

/// <summary>
/// Supplies a converted movie's evidenced door-loader progress for the update being
/// replayed: how many enemy slots native's <c>$82:E4A9</c> had initialized by the end of the
/// update. Like the music-upload NMIs, this is CPU-timing evidence from the native capture.
/// </summary>
internal sealed class EvidencedDoorLoaderProgress : IDoorLoaderProgressSource
{
    /// <summary>Enemy-slot completion count recorded for the replay update currently being applied, or null when unknown.</summary>
    private int? completedSlots;

    /// <summary>Declares the loader progress native reached by the end of the next update.</summary>
    public void Expect(int? completedEnemySlots)
    {
        if (completedEnemySlots is < 0)
            throw new ArgumentOutOfRangeException(nameof(completedEnemySlots));
        completedSlots = completedEnemySlots;
    }

    /// <summary>Checks whether the recorded native loader progress has reached the requested enemy slot.</summary>
    /// <param name="slot">Zero-based enemy slot ordinal to compare with the recorded completed-slot count.</param>
    /// <returns><see langword="true"/> when a count is available and the slot is below it; otherwise, <see langword="false"/>.</returns>
    public bool HasInitializedEnemySlot(int slot) => completedSlots is int completed && slot < completed;
}
