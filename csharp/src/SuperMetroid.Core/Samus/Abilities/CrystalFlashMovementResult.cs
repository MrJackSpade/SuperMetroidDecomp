namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>What one update of the Crystal Flash movement did.</summary>
/// <param name="PhaseAtStart">Phase at call entry, before any transition made by that update.</param>
/// <param name="BubbleStarted">Whether the update finished the raise and started the bubble's explosion and its glow (#2562).</param>
public readonly record struct CrystalFlashMovementResult(
    CrystalFlashPhase PhaseAtStart,
    bool BubbleStarted);
