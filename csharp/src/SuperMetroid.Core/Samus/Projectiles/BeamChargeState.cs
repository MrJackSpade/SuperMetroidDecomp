using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Beam charge state: the flare counter, the previous charge, the charged-shot glow timer, the charge palette index and the three flare components' pictures and waits.</summary>
internal sealed class BeamChargeState
{
    /// <summary>Clears the charge count alone, as a Varia or Gravity Suit's pickup does (#1577).</summary>
    internal void ClearCount() => FlareCounter = 0;

    /// <summary>
    /// The flare counter a Hyper Beam shot stores: past every charge threshold, so the Hyper Beam always reads as fully
    /// charged and never advances it; the largest value the counter holds.
    /// </summary>
    internal const int HyperBeamFlareCounter = 1 << 15;

    /// <summary>
    /// 60 frames arms a charged shot and 120 is the SBA clamp; the charge advances no further, and a Hyper Beam shot
    /// stores <see cref="HyperBeamFlareCounter"/>, the setter's upper bound.
    /// </summary>
    [IntegerQuantity("updates the charge flare has shown")]
    public int FlareCounter
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, HyperBeamFlareCounter, value);
    }

    /// <summary>The <see cref="FlareCounter"/> sampled before the current charge-input update.</summary>
    public int PreviousBeamChargeCounter
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, HyperBeamFlareCounter, value);
    }

    /// <summary>
    /// The charged-shot glow timer. Ordinary charged shots use values four through zero for three
    /// white frames and a fourth-call suit restore. Hyper Beam uses 32788 through 32768
    /// for ten authored palettes held two calls each before the same restore path.
    /// </summary>
    public WrappingCountdown ChargedShotGlowTimer { get; internal set; }

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
        FlareCounter = 0;
        ClearFlare();
    }
}
