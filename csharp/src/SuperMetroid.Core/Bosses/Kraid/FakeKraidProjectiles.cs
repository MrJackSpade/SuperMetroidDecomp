namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Launches Fake Kraid's spit and body-spike projectiles.</summary>
internal static class FakeKraidProjectiles
{
    /// <summary>Computes the near and far spit launch velocities, in pixels and 256ths of a pixel, mirrored by facing.</summary>
    internal static EightEightVector<RoomPixels> SpitLaunch(bool movingRight, int projectile)
    {
        if ((uint)projectile >= FakeKraidProjectileDefinitions.SpitCount)
        {
            throw new InvalidDataException(
                $"Fake Kraid spit {projectile} is neither of its two directions.");
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
