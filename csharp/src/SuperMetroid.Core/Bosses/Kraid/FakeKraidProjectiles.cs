namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Launches Fake Kraid's spit and body-spike projectiles.</summary>
internal static class FakeKraidProjectiles
{
    /// <summary>
    /// The launch velocity of <paramref name="spit"/>: its speed toward <paramref name="direction"/> and the shots'
    /// common rise.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="spit"/> is neither of the two shots.</exception>
    internal static PixelVelocity<RoomPixels> SpitLaunch(HorizontalDirection direction, FakeKraidSpit spit)
    {
        SignedEightEight horizontal = SignedEightEight.FromPixels(
            FakeKraidProjectileDefinitions.SpitSpeedPixels[spit]);
        return new(horizontal.Toward(direction), FakeKraidProjectileDefinitions.SpitRise);
    }

    /// <summary>
    /// The vertical launch offset of body port <paramref name="row"/>. A spike launches from the body's height moved by
    /// this offset, whatever the body's animation and facing.
    /// </summary>
    internal static int SpikeYOffset(FakeKraidSpikeRow row) =>
        FakeKraidProjectileDefinitions.SpikeYOffsets[row];
}
