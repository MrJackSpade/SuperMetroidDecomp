namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>What one of Kraid's mouth-collision reads finds where his head animation stands.</summary>
internal enum KraidMouthShapeReadKind
{
    /// <summary>A mouth shape to test shots against.</summary>
    Shape,
    /// <summary>The closed mouth's missing inner shape: the inner test is skipped, and the outer test cannot load it.</summary>
    NoShape,
    /// <summary>
    /// The read finds no pose (<see cref="Compatibility.KraidMouthTrailsHead"/>): Kraid's head never stands there while a shot
    /// is tested, and collision fails loudly if it does.
    /// </summary>
    NoPose,
}
