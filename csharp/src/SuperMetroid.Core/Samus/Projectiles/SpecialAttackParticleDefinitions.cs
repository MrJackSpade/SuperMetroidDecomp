namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The four particles of a special beam attack, each naming the shot place it takes (#1683, #2565).</summary>
internal static class SpecialAttackParticleDefinitions
{
    /// <summary>
    /// Each particle's part and shot place: the Ice and Plasma orbits start a quarter turn apart; the Wave particles
    /// start away from Samus on both axes, one in each corner; the Spazer beam pair turns clockwise and
    /// counterclockwise with its trailing pair after it; the first particle voices the Spazer convergence and the last
    /// the Wave pulse.
    /// </summary>
    internal static IReadOnlyList<SpecialAttackParticle> InPlaceOrder { get; } =
    [
        new()
        {
            ShotPlace = 0,
            OrbitStart = TrigonometryAngle.Zero,
            WaveStart = new Offset16<RoomPixels>(128, 128),
            SpazerTurn = RotationDirection.Clockwise,
            VoicesSpazerConvergence = true,
        },
        new()
        {
            ShotPlace = 1,
            OrbitStart = TrigonometryAngle.QuarterTurn,
            WaveStart = new Offset16<RoomPixels>(128, -128),
            SpazerTurn = RotationDirection.Counterclockwise,
        },
        new()
        {
            ShotPlace = 2,
            OrbitStart = TrigonometryAngle.HalfTurn,
            WaveStart = new Offset16<RoomPixels>(-128, -128),
            SpazerTurn = RotationDirection.Clockwise,
            SpazerTrail = true,
            FallSideStep = HorizontalDirection.Left,
        },
        new()
        {
            ShotPlace = 3,
            OrbitStart = TrigonometryAngle.HalfTurn + TrigonometryAngle.QuarterTurn,
            WaveStart = new Offset16<RoomPixels>(-128, 128),
            SpazerTurn = RotationDirection.Counterclockwise,
            SpazerTrail = true,
            FallSideStep = HorizontalDirection.Left,
            VoicesWavePulse = true,
        },
    ];
}
