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
    internal void ClearCount()
    {
        charge.Restart(BeamChargeDefinitions.SpecialAttackCounter);
        HyperBeamFlare = false;
    }

    /// <summary>
    /// The updates Samus has held the charge: <see cref="BeamChargeDefinitions.FullyChargedCounter"/> arms a charged shot and
    /// <see cref="BeamChargeDefinitions.SpecialAttackCounter"/> is the special-attack clamp, past which the
    /// charge advances no further. Zero while the Hyper Beam's flare shows (<see cref="HyperBeamFlare"/>).
    /// </summary>
    public int UpdatesHeld => charge.Age;

    /// <summary>
    /// The charge, lasting <see cref="BeamChargeDefinitions.SpecialAttackCounter"/> and counted once each update Samus
    /// holds it until it is over: its age is <see cref="UpdatesHeld"/>, whose named stages arm the charged shot, start
    /// the charging sound and show the flare's components.
    /// </summary>
    private readonly UpdateCountdown charge = UpdateCountdown.Lasting(BeamChargeDefinitions.SpecialAttackCounter);

    /// <summary>Counts one update of the charge held.</summary>
    /// <exception cref="InvalidOperationException">The charge has already reached the special-attack clamp.</exception>
    internal void CountChargeUpdate() => charge.Count();

    /// <summary>
    /// Whether the Hyper Beam's flare shows (#1682): a Hyper Beam shot starts it in place of the charge, and it ends when
    /// the flare's fast sparks show their last frame or anything clears the charge. While it shows, the charge reads as
    /// past every threshold (<see cref="Level"/>).
    /// </summary>
    private bool HyperBeamFlare { get; set; }

    /// <summary>The charge as its tests read it now.</summary>
    public BeamChargeLevel Level => HyperBeamFlare ? BeamChargeLevel.HyperBeam : BeamChargeLevel.Charged(UpdatesHeld);

    /// <summary>Whether any charge is held: a counted charge or the Hyper Beam's flare.</summary>
    internal bool IsCharging => Level.IsCharging;

    /// <summary>Whether the charge has reached <paramref name="updates"/>; the Hyper Beam's flare has reached every count.</summary>
    /// <param name="updates">The charge threshold, in updates held.</param>
    internal bool HasCharged(int updates) => Level.HasCharged(updates);

    /// <summary>The charge sampled before the current charge-input update.</summary>
    public BeamChargeLevel PreviousCharge { get; internal set; }

    /// <summary>The glow Samus's suit shows after a charged shot, or null while it shows none.</summary>
    public ChargedShotGlow? ChargedShotGlow { get; internal set; }

    /// <summary>
    /// Where the charge pulse stands (#2351): just past the shade it showed last, or its start. Every pulse runs its six
    /// shades alike (<see cref="SamusGlowAnimationDefinitions.ChargePulses"/>), so a change of suit or of charge kind goes
    /// on from the same place in the other pulse.
    /// </summary>
    private AnimationPlace ChargePulse { get; set; } = AnimationPlace.StartOfAnimation;

    /// <summary>
    /// Plays the charge pulse on by one update for <paramref name="kind"/> in <paramref name="suit"/> and shows the shade it
    /// shows on <paramref name="colours"/>' suit; after its last shade it starts again.
    /// </summary>
    internal void PlayChargePulse(SamusChargeKind kind, SamusSuit suit, SamusColours colours)
    {
        AnimationPlace place = ChargePulse;
        SamusGlowPlay.ShowNextShade(SamusGlowAnimationDefinitions.ChargePulses[kind][suit], ref place, colours);
        ChargePulse = place;
    }

    /// <summary>Restarts the charge pulse, so its next update shows its first shade.</summary>
    internal void RestartChargePulse() => ChargePulse = AnimationPlace.StartOfAnimation;

    /// <summary>Each flare component's ordinary animation: the picture it shows and the wait it holds it for.</summary>
    private readonly IdentityTable<ChargeFlareComponent, IChargeFlarePlay> _flare = new(ChargeFlarePlays.Of);

    /// <summary>
    /// The Hyper Beam's backward count of each component's pictures (#2553), or none while the ordinary flare plays: a
    /// Hyper Beam shot starts it from the last pictures, the first backward update with the Hyper Beam takes the ordinary
    /// flare over where it stands, and a restart or a clear of the flare ends it.
    /// </summary>
    private IdentityTable<ChargeFlareComponent, IHyperBeamFlareCount>? _hyperBeamCount;

    /// <summary>
    /// The composition flare <paramref name="component"/> shows for Samus's facing: the Hyper Beam's count's while it counts,
    /// none once it has stepped back past the component's first picture, and the ordinary flare's otherwise.
    /// </summary>
    internal ProjectileSpriteId? FlareComposition(ChargeFlareComponent component, bool facingLeft) =>
        _hyperBeamCount is { } count ? count[component].Composition(facingLeft) : _flare[component].Composition(facingLeft);

    /// <summary>Plays flare <paramref name="component"/>'s ordinary animation one update on.</summary>
    internal void PlayFlareForward(ChargeFlareComponent component) => _flare[component].PlayForward();

    /// <summary>
    /// Counts the Hyper Beam's flare of <paramref name="component"/> one update back, the count taking the ordinary flare
    /// over where it stands on its first update.
    /// </summary>
    /// <returns>Whether the update reached the component's first picture.</returns>
    internal bool PlayHyperBeamFlareBackward(ChargeFlareComponent component)
    {
        _hyperBeamCount ??= new(each => _flare[each].CountBackwardFromHere());
        return _hyperBeamCount[component].PlayBackward();
    }

    /// <summary>The ordinary charge flare's first update: every component starts its animation at its first picture.</summary>
    internal void StartChargeFlare()
    {
        _hyperBeamCount = null;
        foreach (IChargeFlarePlay flare in _flare.Values)
            flare.Start();
    }

    /// <summary>The Hyper Beam's shot: every component shows its last picture, to count its pictures backward from there.</summary>
    internal void StartHyperBeamFlare()
    {
        charge.Restart(BeamChargeDefinitions.SpecialAttackCounter);
        HyperBeamFlare = true;
        _hyperBeamCount = new(component => _flare[component].CountBackwardFromLast());
    }

    /// <summary>Clears every flare component to its first picture with no wait, ending any Hyper Beam count.</summary>
    internal void ClearFlare()
    {
        _hyperBeamCount = null;
        foreach (IChargeFlarePlay flare in _flare.Values)
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
