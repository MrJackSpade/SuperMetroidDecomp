namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Supplies a converted movie's evidenced door-loader progress for the update being
/// replayed: how many enemy slots native's <c>$82:E4A9</c> had initialized by the end of the
/// update. Like the music-upload NMIs, this is CPU-timing evidence from the native capture.
/// </summary>
internal sealed class EvidencedRoomLoadProgress : IRoomLoadProgress
{
    /// <summary>Native initialized-slot count for the pending replay update, or null without timing evidence.</summary>
    private int? completedSlots;

    /// <summary>Declares the loader progress native reached by the end of the next update.</summary>
    public void Expect(int? completedEnemySlots)
    {
        if (completedEnemySlots is < 0)
            throw new ArgumentOutOfRangeException(nameof(completedEnemySlots));
        completedSlots = completedEnemySlots;
    }

    /// <summary>Reports a slot initialized only when native evidence places it below the completed-slot count.</summary>
    public bool HasInitializedEnemySlot(int slot) => completedSlots is int completed && slot < completed;
}
