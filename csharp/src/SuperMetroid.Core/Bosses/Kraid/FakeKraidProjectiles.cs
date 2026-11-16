namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Launches Fake Kraid's spit and body-spike projectiles.</summary>
internal static class FakeKraidProjectiles
{
    /// <summary>
    /// The launch velocity of <paramref name="spit"/>, in pixels and 256ths of a pixel: its speed toward
    /// <paramref name="direction"/> and the shots' common rise.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="spit"/> is neither of the two shots.</exception>
    internal static EightEightVector<RoomPixels> SpitLaunch(HorizontalDirection direction, FakeKraidSpit spit)
    {
        SignedEightEight horizontal = SignedEightEight.FromPixels(
            FakeKraidProjectileDefinitions.SpitSpeedPixels.TryGetValue(spit, out int speed)
                ? speed
                : throw new InvalidDataException($"Fake Kraid spit {spit} is neither of its two shots."));
        return new(direction == HorizontalDirection.Right ? horizontal : -horizontal, FakeKraidProjectileDefinitions.SpitRise);
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
