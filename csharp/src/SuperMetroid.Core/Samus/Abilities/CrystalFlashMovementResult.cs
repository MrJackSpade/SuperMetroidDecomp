namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>What one update of the Crystal Flash movement did.</summary>
/// <param name="BubbleStarted">Whether the update finished the raise and started the bubble's explosion and its glow (#2562).</param>
public readonly partial record struct CrystalFlashMovementResult(
    bool BubbleStarted);
