namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Lower Norfair Ridley's selected colours: the initial OBJ colours and the arena-reveal rows.</summary>
/// <param name="Initial">Compiled initial OBJ colors, with shared paint relationships retained by the selector.</param>
/// <param name="Reveal">Fifteen ordered arena background rows, excluding the runtime terminator.</param>
internal sealed record NorfairRidleyColorSelection(NorfairRidleyInitialPaint Initial, NorfairRidleyRevealPaint Reveal);
