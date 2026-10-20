namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's walk-credit word, <c>$7E:780E</c>: one word that the encounter's walking and shot reactions and the
/// rainbow, Baby and phase-three sequence all read and write. The encounter owns it, and a sequence attached to the
/// encounter shares it.
/// </summary>
public sealed class MotherBrainWalkCounter
{
    /// <summary>The credit the word holds.</summary>
    public MotherBrainWalkCredit Credit { get; internal set; }
}
