namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The phase one of Kraid's lints runs.</summary>
public enum KraidLintPhase
{
    /// <summary>The inactive lint before projectile production begins.</summary>
    Inactive,
    /// <summary>
    /// Horizontally aligns the lint to Kraid's body while its phase timer runs, then produces it
    /// (<see cref="Produce"/>).
    /// </summary>
    AlignToKraidBeforeProducing,
    /// <summary>Produces the lint and aligns its starting position to Kraid.</summary>
    Produce,
    /// <summary>Advances the lint's charge preparation before firing.</summary>
    Charge,
    /// <summary>Moves the fired lint away from Kraid.</summary>
    Fire,
}
