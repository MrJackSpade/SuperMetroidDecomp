namespace SuperMetroid.Core.Game;

/// <summary>
/// Processor-status subset returned by the bank-$94 grapple block-reaction dispatcher.
/// Carry means collision; overflow distinguishes a supported grapple connection from the
/// ordinary solid result that cancels the extending beam.
/// </summary>
internal readonly record struct GrappleBlockReaction(bool Carry, bool Overflow);
