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

        SignedEightEight horizontal = SignedEightEight.FromPixels((projectile + 1) * FakeKraidProjectileDefinitions.SpitSpeedStepPixels);
        return new(movingRight ? horizontal : -horizontal, FakeKraidProjectileDefinitions.SpitRise);
    }

    /// <summary>
    /// The vertical launch offset of body port <paramref name="row"/>. The spike initializer adds this signed offset to
    /// the body's Y, wrapping at 65536, independently of animation and facing.
    /// </summary>
    internal static int SpikeYOffset(FakeKraidSpikeRow row) =>
        FakeKraidProjectileDefinitions.SpikeYOffsets.TryGetValue(row, out int offset)
            ? offset
            : throw new InvalidDataException($"Fake Kraid spike row {row} is not a body launch port.");
}
