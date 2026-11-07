using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The programs Samus's projectiles can run while one beam combination is loaded (#1513), so that the combination's
/// projectile sheet holds only what its projectiles draw:
/// <list type="bullet">
/// <item>the lists of the combination's uncharged and charged shots in every direction, its special attack's first list,
/// and, for the combination the Hyper Beam loads, the charged Plasma lists the Hyper Beam fires;</item>
/// <item>the missiles' and Super Missiles' lists in every direction, the bombs', Power Bombs' and the Super Missile
/// link's first lists, and the Spazer special attack's trail and Shinespark echo lists in every direction;</item>
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
internal static class LoadedBeamProjectilePrograms
{
    /// <summary>The combination whose beam and palette the Hyper Beam loads: Wave with Plasma.</summary>
    private const SamusBeamCombination HyperBeamLoaded = SamusBeamCombination.PlasmaWave;

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

    /// <summary>Every program a projectile can run while <paramref name="beams"/> is the loaded combination.</summary>
    internal static IReadOnlySet<SamusProjectileProgram> Of(SamusBeamCombination beams)
    {
        var starts = new List<SamusProjectileProgram>();
        foreach (bool charged in (bool[])[false, true])
            starts.AddRange(BeamShotPrograms(beams, charged));
        if (beams == HyperBeamLoaded)
            starts.AddRange(BeamShotPrograms(HyperBeamFired, charged: true));
        // Only a retail combination has a special attack.
        if (beams.IsRetail && SamusProjectileSelection.SpecialAttack(beams) is { } specialAttack)
            AddFirst(starts, specialAttack);
        foreach (SamusProjectileFamily family in MissileFamilies)
            AddEveryDirection(starts, SamusProjectileSelection.NonBeam(family));
        foreach (SamusProjectileFamily family in BombFamilies)
            AddFirst(starts, SamusProjectileSelection.NonBeam(family));
        foreach (SamusBeamEcho echo in Enum.GetValues<SamusBeamEcho>())
            AddEveryDirection(starts, SamusProjectileSelection.Echo(echo));
        AddFirst(starts, SamusProjectileHeader.SuperMissileLink);
        starts.AddRange(Explosions);
        starts.AddRange(SamusPowerBombFuseData.FastCounterparts.Values.Select(cursor => cursor.Program));
        return Continued(starts);
    }

    /// <summary>Every record a projectile can show while <paramref name="beams"/> is the loaded combination.</summary>
    internal static IEnumerable<SamusProjectileFrame> Records(SamusBeamCombination beams) =>
        Of(beams).SelectMany(program => SamusProjectileAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.ShowStep>()
            .Select(show => show.Frame)
            .Distinct();

    /// <summary>
    /// Every animation a shot of <paramref name="beams"/>, charged when <paramref name="charged"/>, starts in any direction:
    /// its header's lists, except the Murder Beam's, which reads past the Power Bomb header it takes its damage from
    /// (<see cref="MurderBeamDirectionPrograms{TDirection, TProgram}"/>).
    /// </summary>
    internal static IEnumerable<SamusProjectileProgram> BeamShotPrograms(SamusBeamCombination beams, bool charged) =>
        SamusProjectileSelection.IsMurderBeam(beams, charged)
            ? SamusProjectileSelectionDefinitions.MurderBeamDirectionAnimations.Values.OfType<SamusProjectileProgram>()
            : SamusProjectileHeaders.DirectionPrograms(SamusProjectileSelection.BeamShot(beams, charged));

    /// <summary>Adds every list <paramref name="header"/> names.</summary>
    private static void AddEveryDirection(List<SamusProjectileProgram> starts, SamusProjectileHeader header) =>
        starts.AddRange(SamusProjectileHeaders.DirectionPrograms(header));

    /// <summary>Adds the program a shot from <paramref name="header"/> starts in its first direction.</summary>
    private static void AddFirst(List<SamusProjectileProgram> starts, SamusProjectileHeader header) =>
        starts.Add(SamusProjectileHeaders.Program(header, 0));

    /// <summary><paramref name="starts"/> and every list they continue into.</summary>
    private static HashSet<SamusProjectileProgram> Continued(IEnumerable<SamusProjectileProgram> starts)
    {
        var reached = new HashSet<SamusProjectileProgram>();
        var pending = new Queue<SamusProjectileProgram>(starts);
        while (pending.TryDequeue(out SamusProjectileProgram program))
        {
            if (!reached.Add(program))
                continue;
            foreach (AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue> step in
                SamusProjectileAnimations.Programs.Steps(program))
            {
                switch (step)
                {
                    case AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.GoToStep goTo:
                        pending.Enqueue(goTo.Target);
                        break;
                    case AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.FallIntoStep fallInto:
                        pending.Enqueue(fallInto.Target);
                        break;
                    case AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.RepeatWhileCountingStep repeat:
                        pending.Enqueue(repeat.Target);
                        break;
                    default:
                        // A shown frame, a cue or a stop continues into no other list.
                        break;
                }
            }
        }
        return reached;
    }
}
