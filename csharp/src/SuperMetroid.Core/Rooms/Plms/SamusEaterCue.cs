namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>What the plants' programs do besides drawing and counting (#1296).</summary>
internal enum SamusEaterCue
{
    /// <summary>Installs the pre-instruction that holds Samus in the plant's mouth.</summary>
    HoldSamus,
    /// <summary>Queues the chew sound.</summary>
    QueueChewSound,
    /// <summary>Deals one chew's periodic damage.</summary>
    DamageSamus,
    /// <summary>Gives Samus her release immunity.</summary>
    ReleaseSamus,
    /// <summary>Clears the hold pre-instruction.</summary>
    ClearPreInstruction,
}
