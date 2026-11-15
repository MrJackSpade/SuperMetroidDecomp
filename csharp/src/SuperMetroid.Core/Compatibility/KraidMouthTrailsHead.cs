namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which mouth shapes Kraid's shots are tested against while his head animates (#1887). His mouth collision does not test
/// the pose his head shows next but the step his head animation has just passed: after a pose, that pose's outer and
/// inner shapes. A roar or a dying cry between two poses has no shapes of its own, so just after one the outer test takes
/// the inner shape of the pose before it and the inner test finds no pose at all. At the start of a head animation the
/// tests look at the end of the animation laid out before it: the outer test takes that animation's last pose's inner
/// shape and the inner test again finds no pose; before his first animation there is nothing for either test to find.
/// </summary>
/// <remarks>
/// Consulted by Kraid's mouth pass and body pass on each update that tests a shot, at the place his head animation then
/// stands. A test that finds no pose, or an outer test that finds the closed mouth's missing inner shape, has no shape to
/// test; Kraid's head never stands there while a shot is tested, and his collision fails loudly if it does. Evidence: the
/// original mouth tests read the shape words just behind the head animation's place, a cue's or the previous animation's
/// last record included; the head-animation check compares every place's reads with the cartridge's.
/// </remarks>
[CompatibilityService("Kraid's mouth tests take the shapes of the head step just passed: a cry between poses lends its preceding pose's inner shape to the outer test and leaves the inner test no pose, as an animation's start does")]
internal static class KraidMouthTrailsHead
{
    /// <summary>The shapes Kraid's two mouth tests take at <paramref name="place"/> of a head animation.</summary>
    /// <typeparam name="TPose">A head pose.</typeparam>
    /// <param name="steps">The animation's steps in order: each a pose, or null for a cry.</param>
    /// <param name="place">How many of <paramref name="steps"/> the animation has passed, zero through their count.</param>
    /// <param name="poseBefore">The last pose of the animation laid out before this one, or null when none is.</param>
    /// <returns>Where the outer test, tested by body hits, and the inner test, tested by mouth hits, take their shapes.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="place"/> lies outside the animation.</exception>
    /// <exception cref="InvalidDataException">A cry follows no pose.</exception>
    internal static MouthShapes<TPose> ShapesAt<TPose>(IReadOnlyList<TPose?> steps, int place, TPose? poseBefore)
        where TPose : struct
    {
        Ensure.NotNull(steps);
        Ensure.BetweenInclusive(0, steps.Count, place);
        if (place == 0)
            return new(poseBefore is { } before ? MouthShape<TPose>.InnerOf(before) : MouthShape<TPose>.NoPose, MouthShape<TPose>.NoPose);
        if (steps[place - 1] is { } pose)
            return new(MouthShape<TPose>.OuterOf(pose), MouthShape<TPose>.InnerOf(pose));
        TPose cried = place >= 2 && steps[place - 2] is { } preceding
            ? preceding
            : throw new InvalidDataException($"Kraid's head cries at step {place - 1} with no pose before it.");
        return new(MouthShape<TPose>.InnerOf(cried), MouthShape<TPose>.NoPose);
    }

    /// <summary>Where Kraid's two mouth tests take their shapes at one place of his head animation.</summary>
    /// <typeparam name="TPose">A head pose.</typeparam>
    /// <param name="Outer">The outer test's shape, tested by body hits.</param>
    /// <param name="Inner">The inner test's shape, tested by mouth hits.</param>
    internal readonly record struct MouthShapes<TPose>(MouthShape<TPose> Outer, MouthShape<TPose> Inner)
        where TPose : struct;

    /// <summary>Which of a pose's two shapes a mouth test takes.</summary>
    internal enum PoseShape
    {
        /// <summary>The pose's outer shape, through which a shot hurts Kraid.</summary>
        Outer,
        /// <summary>The pose's inner shape, which deflects shots.</summary>
        Inner,
    }

    /// <summary>One of Kraid's mouth tests' shapes: a pose's outer or inner shape, or no pose at all.</summary>
    /// <typeparam name="TPose">A head pose.</typeparam>
    /// <param name="Pose">The pose whose shape the test takes, or null when it finds no pose.</param>
    /// <param name="Shape">Which of the pose's shapes the test takes.</param>
    internal readonly record struct MouthShape<TPose>(TPose? Pose, PoseShape Shape)
        where TPose : struct
    {
        /// <summary>A test that finds no pose.</summary>
        internal static MouthShape<TPose> NoPose => new(null, PoseShape.Outer);

        /// <summary>A test taking <paramref name="pose"/>'s outer shape.</summary>
        internal static MouthShape<TPose> OuterOf(TPose pose) => new(pose, PoseShape.Outer);

        /// <summary>A test taking <paramref name="pose"/>'s inner shape.</summary>
        internal static MouthShape<TPose> InnerOf(TPose pose) => new(pose, PoseShape.Inner);
    }
}
