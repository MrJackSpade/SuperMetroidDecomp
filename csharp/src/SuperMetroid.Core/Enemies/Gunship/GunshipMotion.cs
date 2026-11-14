namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>The Landing Site gunship's landing brake and idle bob schedules.</summary>
internal static class GunshipMotion
{
    /// <summary>The landing brake's signed whole-pixel Y delta at <paramref name="frameIndex"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frameIndex"/> is past the 17-frame schedule.</exception>
    internal static int LandingBrakeYDelta(int frameIndex)
    {
        if (frameIndex >= GunshipMotionDefinitions.LandingBrakeYDeltas.Length)
            throw new InvalidDataException(
                $"Gunship landing-brake frame {frameIndex} exceeds its 17-frame schedule.");
        return GunshipMotionDefinitions.LandingBrakeYDeltas[frameIndex];
    }

    /// <summary>The idle bob's timer and delta at <paramref name="phase"/>; validated before selecting.</summary>
    /// <exception cref="InvalidDataException"><paramref name="phase"/> is past the four-phase cycle.</exception>
    internal static GunshipIdleBobDefinition IdleBob(int phase)
    {
        if (phase >= GunshipMotionDefinitions.IdleBobs.Length)
            throw new InvalidDataException(
                $"Gunship idle-bob phase {phase} exceeds its four-phase cycle.");
        return GunshipMotionDefinitions.IdleBobs[phase];
    }
}
