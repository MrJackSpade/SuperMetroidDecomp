namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>What one 32x32 page of Kraid's BG2 body shows (#1352).</summary>
internal enum KraidBodyPage
{
    /// <summary>The blank tile the room's library background filled, before Kraid uploads or after he is defeated.</summary>
    Blank,

    /// <summary>Kraid's body as <c>$A7:AAC6</c> composed it, behind sprites.</summary>
    Body,

    /// <summary>Kraid's body with every word's priority set by <c>$A7:AD3A</c> for the second phase.</summary>
    PriorityBody,
}
