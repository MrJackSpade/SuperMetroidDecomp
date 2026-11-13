using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Beam charge state: the charge count and the Hyper Beam's flare, the charge sampled before the charge input, the
/// charged-shot glow, the charge palette step and the three flare components' pictures and holds.
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

    /// <summary>
    /// The picture each flare component shows, in <see cref="ChargeFlareComponent"/> order: a place of its program, or,
    /// after the Hyper Beam's backward count has run a component past its first picture, the 0 through 65535 place the
    /// count wrapped to.
    /// </summary>
    internal readonly int[] FlarePhases = new int[3];

    /// <summary>The wait each flare component holds its picture for, in <see cref="ChargeFlareComponent"/> order.</summary>
    internal readonly WrappingCountdown[] FlareHolds = new WrappingCountdown[3];

    /// <summary>The picture flare <paramref name="component"/> shows.</summary>
    internal int FlarePhase(ChargeFlareComponent component) => FlarePhases[Place(component)];

    /// <summary>Shows <paramref name="step"/>'s picture on flare <paramref name="component"/> and holds it for the step's wait.</summary>
    internal void ShowFlare(ChargeFlareComponent component, ChargeFlareStep step)
    {
        FlarePhases[Place(component)] = Ensure.BetweenInclusive(0, WrappingCounter.LargestCount, step.Phase);
        FlareHolds[Place(component)] = step.Hold;
    }

    /// <summary>Counts one update off flare <paramref name="component"/>'s wait, keeping what remains.</summary>
    internal CountdownStep CountFlareHold(ChargeFlareComponent component)
    {
        CountdownStep step = FlareHolds[Place(component)].Decrement();
        FlareHolds[Place(component)] = step.Remaining;
        return step;
    }

    /// <summary>The ordinary charge flare's first call: every component starts its program at its first picture.</summary>
    internal void StartChargeFlare()
    {
        foreach (ChargeFlareComponent component in Enum.GetValues<ChargeFlareComponent>())
            ShowFlare(component, ChargeFlareAnimations.First(component));
    }

    /// <summary>The Hyper Beam's shot: every component shows its last picture, to count its pictures backward from there.</summary>
    internal void StartHyperBeamFlare()
    {
        FlareCounter = 0;
        HyperBeamFlare = true;
        foreach (ChargeFlareComponent component in Enum.GetValues<ChargeFlareComponent>())
        {
            ShowFlare(component, new(ChargeFlareAnimations.LastPicture(component),
                WrappingCountdown.Start(ChargeFlareAnimationDefinitions.HyperBeamPictureUpdates)));
        }
    }

    /// <summary>Clears every flare component to its first picture with no wait.</summary>
    internal void ClearFlare()
    {
        Array.Clear(FlarePhases);
        Array.Clear(FlareHolds);
    }

    /// <summary>The place of <paramref name="component"/>'s picture and wait in the flare's arrays.</summary>
    private static int Place(ChargeFlareComponent component) => component switch
    {
        ChargeFlareComponent.MainFlare => 0,
        ChargeFlareComponent.SlowSparks => 1,
        ChargeFlareComponent.FastSparks => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(component), component, $"Undefined {nameof(ChargeFlareComponent)}."),
    };

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
