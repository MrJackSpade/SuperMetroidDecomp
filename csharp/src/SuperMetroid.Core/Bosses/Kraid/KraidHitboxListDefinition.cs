namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One compiled Kraid arm hitbox list and the rectangles it covers.</summary>
/// <param name="List">The hitbox list.</param>
/// <param name="First">Its first rectangle in the shared geometry.</param>
/// <param name="Count">How many rectangles it holds.</param>
internal readonly record struct KraidHitboxListDefinition(KraidArmHitboxList List, int First, int Count);
