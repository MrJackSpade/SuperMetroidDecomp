using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>
/// Confirms the #1684 typed X-ray aim against the unwrapped whole-step arithmetic it replaces: for every centre step,
/// several fractions and every half-width the scope reaches, aiming up or down lands on the centre the edge clamps of
/// the earlier arithmetic chose.
/// </summary>
internal static class XrayAimClampVerification
{
    /// <summary>The largest half-width, in steps, the scope widens to.</summary>
    private const int WidestHalfWidth = 10;

    /// <summary>Compares every aim update with the unwrapped edge arithmetic.</summary>
    internal static void VerifyXrayAimClamp()
    {
        int cases = 0;
        for (int steps = 0; steps < RotationAngle.StepsPerTurn; steps++)
        foreach (int fraction in new[] { 0, 1, 128, 255 })
        for (int width = 0; width <= WidestHalfWidth; width++)
        {
            RotationAngle center = RotationAngle.FromSteps(steps, fraction);
            AssertEqual(UnwrappedAimUp(center, width), SamusXrayState.AimedUp(center, width), $"aim up from {steps}.{fraction} with half-width {width}");
            AssertEqual(UnwrappedAimDown(center, width), SamusXrayState.AimedDown(center, width), $"aim down from {steps}.{fraction} with half-width {width}");
            cases += 2;
        }

        Console.WriteLine($"  X-ray aim clamp: {cases} aim updates equal the unwrapped edge arithmetic.");
    }

    /// <summary>The earlier aim-up arithmetic: edges compared as whole steps that may lie below zero or past a full turn.</summary>
    private static RotationAngle UnwrappedAimUp(RotationAngle angle, int width)
    {
        int Center() => angle.ToTrigonometryAngle().RawValue;
        if (angle < RotationAngle.HalfTurn)
        {
            int candidate = Center() - width;
            if (candidate == 0)
                return angle;
            if (candidate < 0)
                return RotationAngle.FromSteps(width);
            angle = angle.TurnedBySteps(-1);
            return Center() - width < 0 ? RotationAngle.FromSteps(width) : angle;
        }

        int leftEdge = Center() + width;
        if (leftEdge == RotationAngle.StepsPerTurn)
            return angle;
        if (leftEdge > RotationAngle.StepsPerTurn)
            return RotationAngle.WrappedSteps(RotationAngle.StepsPerTurn - width);
        angle = angle.TurnedBySteps(1);
        return Center() + width > RotationAngle.StepsPerTurn ? RotationAngle.WrappedSteps(RotationAngle.StepsPerTurn - width) : angle;
    }

    /// <summary>The earlier aim-down arithmetic, clamping an edge at straight down.</summary>
    private static RotationAngle UnwrappedAimDown(RotationAngle angle, int width)
    {
        const int halfTurnSteps = RotationAngle.StepsPerTurn / 2;
        int Center() => angle.ToTrigonometryAngle().RawValue;
        if (angle < RotationAngle.HalfTurn)
        {
            int lowerEdge = Center() + width;
            if (lowerEdge == halfTurnSteps)
                return angle;
            if (lowerEdge > halfTurnSteps)
                return RotationAngle.WrappedSteps(halfTurnSteps - width);
            angle = angle.TurnedBySteps(1);
            return Center() + width > halfTurnSteps ? RotationAngle.WrappedSteps(halfTurnSteps - width) : angle;
        }

        int upperEdge = Center() - width;
        if (upperEdge == halfTurnSteps)
            return angle;
        if (upperEdge < halfTurnSteps)
            return RotationAngle.WrappedSteps(halfTurnSteps + width);
        angle = angle.TurnedBySteps(-1);
        return Center() - width < halfTurnSteps ? RotationAngle.WrappedSteps(halfTurnSteps + width) : angle;
    }
}
