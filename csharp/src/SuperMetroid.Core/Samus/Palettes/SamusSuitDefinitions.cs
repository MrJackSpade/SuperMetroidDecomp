using System.Collections.Immutable;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>How Samus's per-suit colour lists are laid out.</summary>
public static class SamusSuitDefinitions
{
    /// <summary>
    /// The suits in the order their colour lists follow one another: each per-suit cycle, charge and death list holds
    /// Power's entries, then Varia's, then Gravity's, so a cycle step that runs past one suit's list reads the next
    /// suit's.
    /// </summary>
    public static ImmutableArray<SamusSuit> ListOrder { get; } = [SamusSuit.Power, SamusSuit.Varia, SamusSuit.Gravity];
}
