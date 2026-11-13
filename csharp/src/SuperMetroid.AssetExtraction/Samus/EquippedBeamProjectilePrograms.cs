using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The programs Samus's projectiles can run when she fires or places them with one beam combination equipped (#1513,
/// #1586): each projectile shows the sheet of the beams it was fired with, so the combination's projectile sheet holds
/// only what its projectiles draw:
/// <list type="bullet">
/// <item>the lists of the combination's uncharged and charged shots in every direction, its special attack's first list,
/// and, for the combination the Hyper Beam equips, the charged Plasma lists the Hyper Beam fires;</item>
/// <item>the missiles' and Super Missiles' lists in every direction, the bombs', Power Bombs' and the Super Missile
/// link's first lists, the Spazer special attack's trail and Shinespark echo lists in every direction, and the lists of
/// the uncharged Spazer and Plasma-Wave shots those echoes become when an enemy reflects them (#1559);</item>
/// <item>the explosions projectiles turn into and the bombs' fast lists;</item>
/// <item>every list those continue into.</item>
/// </list>
/// The glitched Chainsaw and SpaceTime combinations fire what the game selects past the beam selectors (a charged shot of
/// another combination, or a missile or bomb: <see cref="UnauthoredBeamShots{TBeams, TProjectile}"/>), and have no special
/// attack: the combo tables hold only the retail combinations.
/// Their shots' callbacks move no animation. SpaceTime's copies the beam palette on over saved progress. Chainsaw's runs the Power Bomb
/// fuse over the shot from a stopped fuse, which the fuse leaves stopped, so the shot never reaches the fast stage whose
/// move would land its animation in another list (<see cref="SuperMetroid.Core.Compatibility.ChainsawFuseAnimationShift{TCursor}"/>).
/// </summary>
internal static class EquippedBeamProjectilePrograms
{
    /// <summary>The combination the Hyper Beam equips, whose pictures its shots show: Wave with Plasma.</summary>
    private const SamusBeamCombination HyperBeamEquipped = SamusBeamCombination.PlasmaWave;

    /// <summary>The combination the Hyper Beam's shots take their lists from: charged Plasma.</summary>
    private const SamusBeamCombination HyperBeamFired = SamusBeamCombination.Plasma;

    /// <summary>The families a fired missile is: missile and Super Missile.</summary>
    private static readonly SamusProjectileFamily[] MissileFamilies = [SamusProjectileFamily.Missile, SamusProjectileFamily.SuperMissile];

    /// <summary>The families a placed bomb is: Power Bomb and bomb.</summary>
    private static readonly SamusProjectileFamily[] BombFamilies = [SamusProjectileFamily.PowerBomb, SamusProjectileFamily.Bomb];

    /// <summary>The lists projectiles turn into: the beam, missile, Super Missile and bomb explosions.</summary>
    private static readonly SamusProjectileProgram[] Explosions =
    [
        SamusProjectileProgram.BeamExplosion, SamusProjectileProgram.MissileExplosion,
        SamusProjectileProgram.SuperMissileExplosion, SamusProjectileProgram.BombExplosion,
    ];

    /// <summary>Every program a projectile fired or placed with <paramref name="beams"/> equipped can run.</summary>
    internal static IReadOnlySet<SamusProjectileProgram> Of(SamusBeamCombination beams)
    {
        var starts = new List<SamusProjectileProgram>();
        foreach (bool charged in (bool[])[false, true])
            starts.AddRange(BeamShotPrograms(beams, charged));
        if (beams == HyperBeamEquipped)
            starts.AddRange(BeamShotPrograms(HyperBeamFired, charged: true));
        // Only a retail combination has a special attack.
        if (beams.IsRetail && SamusProjectileSelection.SpecialAttack(beams) is { } specialAttack)
            AddSole(starts, specialAttack);
        foreach (SamusProjectileFamily family in MissileFamilies)
            AddEveryDirection(starts, SamusProjectileSelection.NonBeam(family));
        foreach (SamusProjectileFamily family in BombFamilies)
            AddSole(starts, SamusProjectileSelection.NonBeam(family));
        foreach (SamusBeamEcho echo in Enum.GetValues<SamusBeamEcho>())
        {
            AddEveryDirection(starts, SamusProjectileSelection.Echo(echo));
            // An enemy reflecting an echo restarts it as the beam shot its type names, whatever was equipped: a Metal
            // Pirate turns a Shinespark crash's echo into an uncharged Plasma-Wave shot released under the Power Beam (#1559).
            SamusProjectileType reflected = SamusProjectileType.EchoOf(echo);
            starts.AddRange(BeamShotPrograms(reflected.BeamCombination, reflected.IsChargedBeam));
        }
        AddSole(starts, SamusProjectileKind.SuperMissileLink);
        starts.AddRange(Explosions);
        starts.AddRange(SamusPowerBombFuseData.FastCounterparts.Values.Select(place => place.Segment));
        return Continued(starts);
    }

    /// <summary>Every record a projectile fired or placed with <paramref name="beams"/> equipped can show.</summary>
    internal static IEnumerable<SamusProjectileFrame> Records(SamusBeamCombination beams) =>
        Of(beams).SelectMany(program => SamusProjectileAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<SamusProjectileFrame, NoAnimationCue>.Frame>()
            .Select(show => show.Shown)
            .Distinct();

    /// <summary>
    /// Every animation a shot of <paramref name="beams"/>, charged when <paramref name="charged"/>, starts in any direction:
    /// its kind's programs, except the Murder Beam's, which reads past the Power Bomb kind it takes its damage from
    /// (<see cref="MurderBeamDirectionPrograms{TDirection, TProgram}"/>).
    /// </summary>
    internal static IEnumerable<SamusProjectileProgram> BeamShotPrograms(SamusBeamCombination beams, bool charged) =>
        SamusProjectileSelection.IsMurderBeam(beams, charged)
            ? SamusProjectileSelectionDefinitions.MurderBeamDirectionAnimations.Values.OfType<SamusProjectileProgram>()
            : SamusProjectileKinds.Programs(SamusProjectileSelection.BeamShot(beams, charged));

    /// <summary>Adds every program a shot of <paramref name="kind"/> can start.</summary>
    private static void AddEveryDirection(List<SamusProjectileProgram> starts, SamusProjectileKind kind) =>
        starts.AddRange(SamusProjectileKinds.Programs(kind));

    /// <summary>Adds the one program a shot, bomb or particle of <paramref name="kind"/> starts whatever its direction.</summary>
    private static void AddSole(List<SamusProjectileProgram> starts, SamusProjectileKind kind) =>
        starts.Add(SamusProjectileKinds.SoleProgram(kind));

    /// <summary><paramref name="starts"/> and every segment their animations play.</summary>
    private static HashSet<SamusProjectileProgram> Continued(IEnumerable<SamusProjectileProgram> starts) =>
        [.. starts.SelectMany(start => SamusProjectileAnimations.StartingWith(start).Phases).Select(phase => phase.Segment)];
}
