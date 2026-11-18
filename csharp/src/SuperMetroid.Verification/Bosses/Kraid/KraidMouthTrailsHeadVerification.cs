using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.TestSupport.MovieReplay;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>
/// #1887: Kraid's mouth tests take the shapes of the head step just passed (<see cref="KraidMouthTrailsHead"/>), checked at
/// the places the service exists for: a pose, an animation's start and just after a roar.
/// </summary>
internal static class KraidMouthTrailsHeadVerification
{
    /// <summary>
    /// Just after a pose the mouth tests take that pose's shapes; just after the roar's cry the body test takes the wider
    /// pose's inner shape and the mouth test finds no pose, so a mouth hit there fails loudly; at the start of the eye glow
    /// the body test takes the closed mouth's missing inner shape and the mouth test again finds no pose; at the roar's start
    /// neither finds a pose.
    /// </summary>
    internal static void VerifyKraidMouthTrailsHead()
    {
        // Just after the roar's first opening pose: that pose's shapes.
        var afterOpening = KraidHeadNativeCursors.PlaybackAtStep(KraidHeadSegment.Roar, 2);
        AssertEqual(KraidMouthHitbox.OpeningVulnerable, KraidHeadAnimations.OuterMouthAt(afterOpening),
            "after a pose the body test takes its outer shape");
        AssertEqual((KraidMouthHitbox?)KraidMouthHitbox.OpeningInvulnerable, KraidHeadAnimations.InnerMouthAt(afterOpening),
            "after a pose the mouth test takes its inner shape");

        // Just after the roar's cry, which follows the wider pose.
        var afterRoar = KraidHeadNativeCursors.PlaybackAtStep(KraidHeadSegment.Roar, 4);
        AssertEqual(KraidMouthHitbox.WiderInvulnerable, KraidHeadAnimations.OuterMouthAt(afterRoar),
            "just after the roar the body test takes the wider pose's inner shape");
        AssertThrows<InvalidDataException>(() => KraidHeadAnimations.InnerMouthAt(afterRoar),
            "just after the roar the mouth test finds no pose");

        // At the eye glow's start, where the tests find the closed mouth before it.
        var eyeGlowStart = KraidHeadNativeCursors.PlaybackAtStep(KraidHeadSegment.EyeGlow, 0);
        KraidHeadCursorReads atStart = KraidHeadAnimations.ReadsAt(eyeGlowStart);
        AssertEqual(KraidMouthShapeReadKind.NoShape, atStart.Outer.Kind, "at an animation's start the body test takes the closed mouth's missing inner shape");
        AssertEqual(KraidMouthShapeReadKind.NoPose, atStart.Inner.Kind, "at an animation's start the mouth test finds no pose");
        AssertThrows<InvalidDataException>(() => KraidHeadAnimations.OuterMouthAt(eyeGlowStart),
            "a body hit at an animation's start has no shape to test");

        // At the roar's start: nothing is laid out before it.
        KraidHeadCursorReads atRoarStart = KraidHeadAnimations.ReadsAt(KraidHeadNativeCursors.PlaybackAtStep(KraidHeadSegment.Roar, 0));
        AssertEqual(KraidMouthShapeReadKind.NoPose, atRoarStart.Outer.Kind, "the roar's start finds no pose for the body test");
        AssertEqual(KraidMouthShapeReadKind.NoPose, atRoarStart.Inner.Kind, "the roar's start finds no pose for the mouth test");
        Console.WriteLine("Kraid's mouth trails his head: a pose's shapes after it, the wider pose's inner shape after the roar, the closed mouth at an eye glow's start.");
    }
}
