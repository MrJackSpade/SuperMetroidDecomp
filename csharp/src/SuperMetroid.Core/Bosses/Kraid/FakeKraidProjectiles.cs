namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Launches Fake Kraid's spit and body-spike projectiles.</summary>
internal static class FakeKraidProjectiles
{
    /// <summary>Computes the signed8.8 near/far launches, mirrored by facing.</summary>
    internal static EightEightVector<RoomPixels> SpitLaunch(bool movingRight, int projectile)
    {
        if ((uint)projectile >= FakeKraidProjectileDefinitions.SpitCount)
        {
            throw new InvalidDataException(
                $"Fake Kraid spit index {projectile} exceeds its two-entry direction set.");
        }

        int horizontal = (projectile + 1) * FakeKraidProjectileDefinitions.SpitSpeedStep;
        return new(CoordinateAxis.AsSigned(movingRight ? horizontal : -horizontal), FakeKraidProjectileDefinitions.SpitRise);
    }

    /// <summary>Selects a fixed body launch position.
    /// The spit step selects one of three independent spike timers by
    /// row; the managed boundary takes the row directly.
    /// The spike initializer adds this signed integer offset with16-bit wrap, independently of
    /// animation and facing.</summary>
    internal static int SpikeYOffset(int row) => (uint)row < (uint)FakeKraidProjectileDefinitions.Rows.Length
        ? FakeKraidProjectileDefinitions.SpikeYOffsets.TryGetValue(FakeKraidProjectileDefinitions.Rows[row], out int offset)
            ? offset
            : throw new InvalidDataException($"Fake Kraid spike row {FakeKraidProjectileDefinitions.Rows[row]} is not a body launch port.")
        : throw new InvalidDataException($"Fake Kraid spike row {row} is not a body launch port.");
}
