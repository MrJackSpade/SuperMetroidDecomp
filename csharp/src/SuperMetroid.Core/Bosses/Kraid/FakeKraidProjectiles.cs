namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Launches Fake Kraid's spit and body-spike projectiles.</summary>
internal static class FakeKraidProjectiles
{
    /// <summary>Computes the signed8.8 near/far launches from $A6:9A48-9A57, mirrored by facing.</summary>
    internal static EightEightVector<RoomPixels> SpitLaunch(bool movingRight, int projectile)
    {
        if ((uint)projectile >= FakeKraidProjectileDefinitions.SpitCount)
        {
            throw new InvalidDataException(
                $"Fake Kraid spit index {projectile} exceeds its two-entry direction set.");
        }

        int horizontal = (projectile + 1) * FakeKraidProjectileDefinitions.SpitSpeedStep;
        return new((short)(movingRight ? horizontal : -horizontal), FakeKraidProjectileDefinitions.SpitRise);
    }

    /// <summary>Selects a fixed body launch position from $86:9E7D..9E82.
    /// Native $A6:9AC2..9B10 selects one of three independent spike timers at byte
    /// offsets0/2/4; the managed boundary divides that selector by two.
    /// $86:9E4E adds this signed integer offset with16-bit wrap, independently of
    /// animation and facing.</summary>
    internal static int SpikeYOffset(int row) => (uint)row < (uint)FakeKraidProjectileDefinitions.Rows.Length
        ? FakeKraidProjectileDefinitions.SpikeYOffsets.TryGetValue(FakeKraidProjectileDefinitions.Rows[row], out int offset)
            ? offset
            : throw new InvalidDataException($"Fake Kraid spike row {FakeKraidProjectileDefinitions.Rows[row]} is not a body launch port.")
        : throw new InvalidDataException($"Fake Kraid spike row {row} is not a body launch port.");
}
