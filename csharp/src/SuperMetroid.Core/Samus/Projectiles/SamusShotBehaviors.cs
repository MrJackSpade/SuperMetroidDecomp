namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The one shared instance of each behaviour Samus's shots run (#1923).</summary>
internal static class SamusShotBehaviors
{
    /// <summary>A beam without the Wave Beam: it stops at the first block it meets.</summary>
    internal static SamusShotBehavior Beam { get; } = new BeamFlight();

    /// <summary>An uncharged Wave or Ice and Wave shot: the Wave Beam's flight, a trail every three updates after its first.</summary>
    internal static WaveBeamFlight WaveBeamThreeUpdateTrail { get; } = new(ProjectileTimingDefinitions.UnchargedWaveTrailInterval);

    /// <summary>Every other Wave shot: the Wave Beam's flight, a trail every four updates.</summary>
    internal static WaveBeamFlight WaveBeamFourUpdateTrail { get; } = new(ProjectileTimingDefinitions.TrailInterval);

    /// <summary>The Hyper Beam shot.</summary>
    internal static SamusShotBehavior HyperBeam { get; } = new HyperBeamFlight();

    /// <summary>A missile.</summary>
    internal static SamusShotBehavior Missile { get; } = new MissileFlight();

    /// <summary>A Super Missile.</summary>
    internal static SamusShotBehavior SuperMissile { get; } = new SuperMissileFlight();

    /// <summary>A Super Missile's invisible link.</summary>
    internal static SamusShotBehavior SuperMissileLink { get; } = new SuperMissileLinkHold();

    /// <summary>An Ice special attack particle orbiting inward.</summary>
    internal static SpecialAttackParticleBehavior IceSpecialAttackInward { get; } = new IceSpecialAttackOrbit();

    /// <summary>An Ice special attack particle that has turned outward.</summary>
    internal static SpecialAttackParticleBehavior IceSpecialAttackOutward { get; } = new IceSpecialAttackSpread();

    /// <summary>A Wave special attack particle.</summary>
    internal static SpecialAttackParticleBehavior WaveSpecialAttack { get; } = new WaveSpecialAttackHoming();

    /// <summary>A Spazer special attack particle, circling, flying up and falling.</summary>
    internal static SpecialAttackParticleBehavior SpazerSpecialAttack { get; } = new SpazerSpecialAttackFlight();

    /// <summary>A Plasma special attack particle.</summary>
    internal static SpecialAttackParticleBehavior PlasmaSpecialAttack { get; } = new PlasmaSpecialAttackRing();

    /// <summary>A shinespark crash echo.</summary>
    internal static SamusShotBehavior ShinesparkEcho { get; } = new ShinesparkEchoFlight();

    /// <summary>The Spacetime shot, an uncharged Spazer, Plasma and Ice beam.</summary>
    internal static SamusShotBehavior Spacetime { get; } = new SpacetimeShotOverwrite();

    /// <summary>The Chainsaw shot, an uncharged Spazer, Plasma and Wave beam.</summary>
    internal static SamusShotBehavior ChainsawPowerBomb { get; } = new ChainsawPowerBombShot();

    /// <summary>The uncharged Spazer and Plasma shot, which runs whatever an unconnected region holds.</summary>
    internal static SamusShotBehavior UnchargedSpazerPlasma { get; } =
        new ShotRefusedWhenFired("whatever an unconnected region holds");

    /// <summary>The uncharged all-beams shot, which runs whatever the game's live state holds.</summary>
    internal static SamusShotBehavior UnchargedAllBeams { get; } =
        new ShotRefusedWhenFired("whatever the game's live state holds");

    /// <summary>The charged Spazer and Plasma shot, which runs whatever the game's live state holds.</summary>
    internal static SamusShotBehavior ChargedSpazerPlasma { get; } =
        new ShotRefusedWhenFired("whatever the game's live state holds");

    /// <summary>
    /// The charged Spazer, Plasma and Wave and Spazer, Plasma and Ice shots, which run whatever Samus's changing state
    /// holds, so what the game does depends on that state and may crash it.
    /// </summary>
    internal static SamusShotBehavior ChargedChainsaw { get; } =
        new ShotRefusedWhenRun("whatever Samus's changing state holds");

    /// <summary>
    /// The Murder Beam, the charged all-beams shot, which starts in the middle of an unrelated part of the game, and the
    /// game then stops making forward progress. The safe left-facing shot has no animation, so it never runs it.
    /// </summary>
    internal static SamusShotBehavior MurderBeam { get; } =
        new ShotRefusedWhenRun("from the middle of an unrelated part of the game");
}
