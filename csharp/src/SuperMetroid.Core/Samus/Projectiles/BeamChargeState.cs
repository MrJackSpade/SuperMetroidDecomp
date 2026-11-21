using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Colours;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Beam charge state: the charge count and the Hyper Beam's flare, the charge sampled before the charge input, the
/// charged-shot glow, the charge pulse and each flare component's animation.
/// </summary>
internal sealed class BeamChargeState
{
    /// <summary>
    /// Clears the charge, its count and the Hyper Beam's flare alike, as a Varia or Gravity Suit's pickup does (#1577)
    /// and every shot, special attack and cancellation that ends a charge does.
    /// </summary>
    internal void ClearCount() => Level = BeamChargeLevel.None;

    /// <summary>
    /// The updates Samus has held the charge: <see cref="BeamChargeDefinitions.FullyChargedCounter"/> arms a charged shot and
    /// <see cref="BeamChargeDefinitions.SpecialAttackCounter"/> is the special-attack clamp, past which the
    /// charge advances no further. Zero while the Hyper Beam's flare shows.
    /// </summary>
    public int UpdatesHeld => Level.UpdatesHeld;

    /// <summary>Counts one update of the charge held.</summary>
    /// <exception cref="InvalidOperationException">
    /// The charge has already reached the special-attack clamp, or the Hyper Beam's flare shows, which counts nothing.
    /// </exception>
    internal void CountChargeUpdate() => Level = Level is BeamChargeLevel.CountedCharge { Updates: var updates } &&
        updates < BeamChargeDefinitions.SpecialAttackCounter
        ? BeamChargeLevel.Charged(updates + 1)
        : throw new InvalidOperationException($"A {Level} charge counts no further update.");

    /// <summary>
    /// The charge as its tests read it now (#2560): the updates Samus has held it, counted once each update she holds it
    /// up to <see cref="BeamChargeDefinitions.SpecialAttackCounter"/>, whose named stages arm the charged shot, start the
    /// charging sound and show the flare's components; or the Hyper Beam's flare (#1682), which a Hyper Beam shot starts in
    /// place of the charge and which ends when the flare's fast sparks show their last frame or anything clears the charge.
    /// </summary>
    public BeamChargeLevel Level { get; internal set; } = BeamChargeLevel.None;

    /// <summary>Whether any charge is held: a counted charge or the Hyper Beam's flare.</summary>
    internal bool IsCharging => Level.IsCharging;

    /// <summary>Whether the charge has reached <paramref name="updates"/>; the Hyper Beam's flare has reached every count.</summary>
    /// <param name="updates">The charge threshold, in updates held.</param>
    internal bool HasCharged(int updates) => Level.HasCharged(updates);

    /// <summary>The charge sampled before the current charge-input update.</summary>
    public BeamChargeLevel PreviousCharge { get; internal set; } = BeamChargeLevel.None;

    /// <summary>The glow Samus's suit shows after a charged shot, or null while it shows none.</summary>
    public ChargedShotGlow? ChargedShotGlow { get; internal set; }

    /// <summary>
    /// Where the charge pulse stands (#2351): just past the shade it showed last, or its start. Every pulse runs its six
    /// shades alike (<see cref="SamusGlowAnimationDefinitions.ChargePulses"/>), so a change of suit or of charge kind goes
    /// on from the same place in the other pulse.
    /// </summary>
    internal AnimationPlace ChargePulse { get; set; } = AnimationPlace.StartOfAnimation;

    /// <summary>
    /// Plays the charge pulse on by one update for <paramref name="kind"/> in <paramref name="suit"/> and returns the
    /// shade it shows; after its last shade it starts again.
    /// </summary>
    internal PictureShade PlayChargePulse(SamusChargeKind kind, SamusSuit suit)
    {
        AnimationPlace place = ChargePulse;
        PictureShade shown = SamusGlowPlay.NextShade(SamusGlowSegment.In(SamusGlowAnimationDefinitions.ChargePulses[kind], suit), ref place).Shown;
        ChargePulse = place;
        return shown;
    }

    /// <summary>Restarts the charge pulse, so its next update shows its first shade.</summary>
    internal void RestartChargePulse() => ChargePulse = AnimationPlace.StartOfAnimation;

    /// <summary>Each flare component's animation: the picture it shows and the wait it holds it for.</summary>
    private readonly Dictionary<ChargeFlareComponent, ChargeFlareComponentState> _flare =
        Enum.GetValues<ChargeFlareComponent>().ToDictionary(component => component, component => new ChargeFlareComponentState(component));

    /// <summary>Flare <paramref name="component"/>'s animation.</summary>
    internal ChargeFlareComponentState Flare(ChargeFlareComponent component) =>
        _flare.TryGetValue(component, out ChargeFlareComponentState? flare)
            ? flare
            : throw new ArgumentOutOfRangeException(nameof(component), component, $"Undefined {nameof(ChargeFlareComponent)}.");

    /// <summary>The ordinary charge flare's first update: every component starts its animation at its first picture.</summary>
    internal void StartChargeFlare()
    {
        foreach (ChargeFlareComponent component in Enum.GetValues<ChargeFlareComponent>())
            Flare(component).Start();
    }

    /// <summary>The Hyper Beam's shot: every component shows its last picture, to count its pictures backward from there.</summary>
    internal void StartHyperBeamFlare()
    {
        Level = BeamChargeLevel.HyperBeam;
        foreach (ChargeFlareComponent component in Enum.GetValues<ChargeFlareComponent>())
            Flare(component).StartBackward();
    }

    /// <summary>Clears every flare component to its first picture with no wait.</summary>
    internal void ClearFlare()
    {
        foreach (ChargeFlareComponentState flare in _flare.Values)
            flare.Clear();
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
