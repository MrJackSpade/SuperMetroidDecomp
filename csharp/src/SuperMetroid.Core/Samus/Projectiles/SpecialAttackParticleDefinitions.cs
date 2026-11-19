namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The four particles of a special beam attack, in the order of the shot places they take (#1683).</summary>
internal static class SpecialAttackParticleDefinitions
{
    /// <summary>
    /// Each particle's part, in place order: the Ice and Plasma orbits start a quarter turn apart; the Wave particles
    /// start 128 pixels from Samus on both axes, one in each corner; the Spazer beam pair turns clockwise and
    /// counterclockwise with its trailing pair after it; the first particle voices the Spazer convergence and the last
    /// the Wave pulse.
    /// </summary>
    internal static IReadOnlyList<SpecialAttackParticle> InPlaceOrder { get; } =
    [
        new()
        {
            OrbitStart = TrigonometryAngle.Zero,
            WaveStart = new Offset16<RoomPixels>(128, 128),
            SpazerTurn = RotationDirection.Clockwise,
            VoicesSpazerConvergence = true,
        },
        new()
        {
            OrbitStart = TrigonometryAngle.QuarterTurn,
            WaveStart = new Offset16<RoomPixels>(128, -128),
            SpazerTurn = RotationDirection.Counterclockwise,
        },
        new()
        {
            OrbitStart = TrigonometryAngle.HalfTurn,
            WaveStart = new Offset16<RoomPixels>(-128, -128),
            SpazerTurn = RotationDirection.Clockwise,
            SpazerTrail = true,
        },
        new()
        {
            OrbitStart = TrigonometryAngle.HalfTurn + TrigonometryAngle.QuarterTurn,
            WaveStart = new Offset16<RoomPixels>(-128, 128),
            SpazerTurn = RotationDirection.Counterclockwise,
            SpazerTrail = true,
            VoicesWavePulse = true,
        },
    ];
}
