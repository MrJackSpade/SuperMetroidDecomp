namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>What one of Kraid's mouth-collision reads finds where the head program's cursor stands.</summary>
internal enum KraidMouthShapeReadKind
{
    /// <summary>A mouth shape to test shots against.</summary>
    Shape = 1,
    /// <summary>The closed mouth's missing inner shape: the inner test is skipped, and the outer test cannot load it.</summary>
    NoShape,
    /// <summary>The read lands outside every head pose; collision fails loudly, as reading past the head data does.</summary>
    OutsideHeadFrames,
}
