using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Beam charge state: the charge count and the Hyper Beam's flare, the charge sampled before the charge input, the
/// charged-shot glow, the charge palette step and the three flare components' frames and timers.
/// </summary>
internal sealed class BeamChargeState
{
    /// <summary>
    /// Clears the charge, its count and the Hyper Beam's flare alike, as a Varia or Gravity Suit's pickup does (#1577)
    /// and every shot, special attack and cancellation that ends a charge does.
    /// </summary>
    internal void ClearCount()
    {
        FlareCounter = 0;
        HyperBeamFlare = false;
    }

    /// <summary>
    /// The updates Samus has held the charge: 60 arms a charged shot and 120 is the special-attack clamp, past which the
    /// charge advances no further. Zero while the Hyper Beam's flare shows (<see cref="HyperBeamFlare"/>).
    /// </summary>
    [IntegerQuantity("updates the charge flare has shown")]
    public int FlareCounter
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, BeamChargeDefinitions.SpecialAttackCounter, value);
    }

    /// <summary>
    /// Whether the Hyper Beam's flare shows (#1682): a Hyper Beam shot starts it in place of the charge, and it ends when
    /// the flare's fast sparks show their last frame or anything clears the charge. While it shows, the charge reads as
    /// past every threshold (<see cref="Level"/>).
    /// </summary>
    public bool HyperBeamFlare { get; private set; }

    /// <summary>The charge as its tests read it now.</summary>
    public BeamChargeLevel Level => HyperBeamFlare ? BeamChargeLevel.HyperBeam : BeamChargeLevel.Charged(FlareCounter);

    /// <summary>Whether any charge is held: a counted charge or the Hyper Beam's flare.</summary>
    internal bool IsCharging => Level.IsCharging;

    /// <summary>Whether the charge has reached <paramref name="updates"/>; the Hyper Beam's flare has reached every count.</summary>
    /// <param name="updates">The charge threshold, in updates held.</param>
    internal bool HasCharged(int updates) => Level.HasCharged(updates);

    /// <summary>The charge sampled before the current charge-input update.</summary>
    public BeamChargeLevel PreviousCharge { get; internal set; }

    /// <summary>The glow Samus's suit shows after a charged shot, or none.</summary>
    public ChargedShotGlow ChargedShotGlow { get; internal set; }

    /// <summary>
    /// The charge colour cycle's next step, 0 through 5: each charge-palette call shows it and advances it, and the
    /// sixth call wraps it to zero.
    /// </summary>
    public int ChargePaletteStep
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, SamusChargePaletteDefinitions.ChargeSteps - 1, value);
    }

    /// <summary>The most recent charged-shot palette sub-handler result, which survives room loads.</summary>
    internal SamusBeamChargePaletteStepResult LastBeamChargePaletteStep { get; set; }

    /// <summary>The nested visor palette result, which survives room loads.</summary>
    internal SamusVisorPaletteStepResult LastVisorPaletteStep { get; set; }

    /// <summary>Current animation frame for each of the three flare components.</summary>
    internal readonly int[] FlareFrames = new int[3];
    /// <summary>Countdown to each flare component's next authored frame.</summary>
    internal readonly WrappingCountdown[] FlareTimers = new WrappingCountdown[3];

    /// <summary>The current animation frame of flare <paramref name="component"/>.</summary>
    internal int FlareFrame(int component) => FlareFrames[component];

    /// <summary>Sets flare <paramref name="component"/>'s animation frame.</summary>
    internal void SetFlareFrame(int component, int frame) =>
        FlareFrames[component] = Ensure.BetweenInclusive(0, ChargeFlareAnimations.FramePlaces - 1, frame);

    /// <summary>Restarts flare <paramref name="component"/>'s countdown to its next frame.</summary>
    internal void StartFlareTimer(int component, int frames) => FlareTimers[component] = WrappingCountdown.Start(frames);

    /// <summary>Counts one call of flare <paramref name="component"/>'s countdown, keeping what remains.</summary>
    internal CountdownStep DecrementFlareTimer(int component)
    {
        CountdownStep step = FlareTimers[component].Decrement();
        FlareTimers[component] = step.Remaining;
        return step;
    }

    /// <summary>The ordinary charge flare's first call: every frame at zero, the three countdowns at 3, 5 and 4.</summary>
    internal void StartChargeFlare()
    {
        Array.Clear(FlareFrames);
        StartFlareTimer(0, 3);
        StartFlareTimer(1, 5);
        StartFlareTimer(2, 4);
    }

    /// <summary>Starts the Hyper Beam's flare in place of the charge: frames 29, 5 and 5, each countdown at 3.</summary>
    internal void StartHyperBeamFlare()
    {
        FlareCounter = 0;
        HyperBeamFlare = true;
        FlareFrames[0] = 29;
        FlareFrames[1] = 5;
        FlareFrames[2] = 5;
        StartFlareTimer(0, 3);
        StartFlareTimer(1, 3);
        StartFlareTimer(2, 3);
    }

    /// <summary>Clears every flare frame and countdown.</summary>
    internal void ClearFlare()
    {
        Array.Clear(FlareFrames);
        Array.Clear(FlareTimers);
    }

    /// <summary>
    /// Clears the charge and its whole flare, as the shinespark windup and the grapple wall jump's launch do: the count
    /// and every flare component (#1577).
    /// </summary>
    internal void ClearChargeAndFlare()
    {
        ClearCount();
        ClearFlare();
    }
}
