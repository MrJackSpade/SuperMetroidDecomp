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
    public BeamChargeLevel Level { get; private set; } = BeamChargeLevel.None;

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
    private AnimationPlace ChargePulse { get; set; } = AnimationPlace.StartOfAnimation;

    /// <summary>
    /// Plays the charge pulse on by one update for <paramref name="kind"/> in <paramref name="suit"/> and shows the shade it
    /// shows on <paramref name="colours"/>' suit; after its last shade it starts again.
    /// </summary>
    internal void PlayChargePulse(SamusChargeKind kind, SamusSuit suit, SamusColours colours)
    {
        AnimationPlace place = ChargePulse;
        SamusGlowPlay.ShowNextShade(SamusGlowSegment.In(SamusGlowAnimationDefinitions.ChargePulses[kind], suit), ref place, colours);
        ChargePulse = place;
    }

    /// <summary>Restarts the charge pulse, so its next update shows its first shade.</summary>
    internal void RestartChargePulse() => ChargePulse = AnimationPlace.StartOfAnimation;

    /// <summary>Each flare component's ordinary animation: the picture it shows and the wait it holds it for.</summary>
    internal IdentityTable<ChargeFlareComponent, IChargeFlarePlay> Flares { get; } = new(ChargeFlarePlays.Of);

    /// <summary>
    /// The Hyper Beam's backward count of each component's pictures (#2553), or none while the ordinary flare plays: a
    /// Hyper Beam shot starts it from the last pictures, the first backward update with the Hyper Beam takes the ordinary
    /// flare over where it stands, and a restart or a clear of the flare ends it.
    /// </summary>
    internal IdentityTable<ChargeFlareComponent, IHyperBeamFlareCount>? HyperBeamCount { get; private set; }

    /// <summary>
    /// The composition flare <paramref name="component"/> shows for Samus's facing: the Hyper Beam's count's while it counts,
    /// none once it has stepped back past the component's first picture, and the ordinary flare's otherwise.
    /// </summary>
    internal ProjectileSpriteId? FlareComposition(ChargeFlareComponent component, HorizontalDirection facing) =>
        HyperBeamCount is { } count ? count[component].Composition(facing) : Flares[component].Composition(facing);

    /// <summary>Plays flare <paramref name="component"/>'s ordinary animation one update on.</summary>
    internal void PlayFlareForward(ChargeFlareComponent component) => Flares[component].PlayForward();

    /// <summary>
    /// Counts the Hyper Beam's flare of <paramref name="component"/> one update back, the count taking the ordinary flare
    /// over where it stands on its first update.
    /// </summary>
    /// <returns>Whether the update reached the component's first picture.</returns>
    internal bool PlayHyperBeamFlareBackward(ChargeFlareComponent component)
    {
        HyperBeamCount ??= new(each => Flares[each].CountBackwardFromHere());
        return HyperBeamCount[component].PlayBackward();
    }

    /// <summary>The ordinary charge flare's first update: every component starts its animation at its first picture.</summary>
    internal void StartChargeFlare()
    {
        HyperBeamCount = null;
        foreach (IChargeFlarePlay flare in Flares.Values)
            flare.Start();
    }

    /// <summary>The Hyper Beam's shot: every component shows its last picture, to count its pictures backward from there.</summary>
    internal void StartHyperBeamFlare()
    {
        Level = BeamChargeLevel.HyperBeam;
        HyperBeamCount = new(component => Flares[component].CountBackwardFromLast());
    }

    /// <summary>Clears every flare component to its first picture with no wait, ending any Hyper Beam count.</summary>
    internal void ClearFlare()
    {
        HyperBeamCount = null;
        foreach (IChargeFlarePlay flare in Flares.Values)
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
