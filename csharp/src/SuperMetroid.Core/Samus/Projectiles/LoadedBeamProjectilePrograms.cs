using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The programs Samus's projectiles can run while one beam combination is loaded (#1513), so that the combination's
/// projectile sheet holds only what its projectiles draw:
/// <list type="bullet">
/// <item>the lists of the combination's uncharged and charged shots in every direction, its special attack's first list,
/// and, for the combination the Hyper Beam loads, the charged Plasma lists the Hyper Beam fires;</item>
/// <item>the missiles' and Super Missiles' lists in every direction, the bombs', Power Bombs' and the Super Missile
/// link's first lists, and every echo and trail list;</item>
/// <item>the explosions projectiles turn into and the bombs' fast lists;</item>
/// <item>every list those continue into.</item>
/// </list>
/// The glitched Chainsaw and SpaceTime combinations select their shots the same way, reading past the beam rows into the
/// charged rows and the missiles' rows, and have no special attack: the combo tables hold only the retail combinations.
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

    /// <summary>The non-beam selector entries a missile reads, by the HUD item it is fired with: missile and Super Missile.</summary>
    private static readonly int[] MissileEntries = [1, 2];

    /// <summary>The non-beam selector entries a placed bomb reads: Power Bomb and bomb.</summary>
    private static readonly int[] BombEntries = [3, 5];

    /// <summary>The echo and trail selector entries.</summary>
    private const int EchoAndTrailEntries = 8;

    /// <summary>The Super Missile link's selector entry.</summary>
    private const int SuperMissileLinkEntry = 2;

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
        AddEveryDirection(starts, SamusProjectileSelectorTable.UnchargedBeam, beams.TableIndex);
        AddEveryDirection(starts, SamusProjectileSelectorTable.ChargedBeam, beams.TableIndex);
        if (beams == HyperBeamLoaded)
            AddEveryDirection(starts, SamusProjectileSelectorTable.ChargedBeam, HyperBeamFired.TableIndex);
        // Only a retail combination has a special attack.
        if (beams.IsRetail)
            AddFirst(starts, SamusProjectileSelectorTable.SpecialAttack, beams.TableIndex);
        foreach (int entry in MissileEntries)
            AddEveryDirection(starts, SamusProjectileSelectorTable.NonBeam, entry);
        foreach (int entry in BombEntries)
            AddFirst(starts, SamusProjectileSelectorTable.NonBeam, entry);
        for (int entry = 0; entry < EchoAndTrailEntries; entry++)
            AddEveryDirection(starts, SamusProjectileSelectorTable.EchoAndTrail, entry);
        AddFirst(starts, SamusProjectileSelectorTable.SuperMissileLink, SuperMissileLinkEntry);
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

    /// <summary>Adds every list the header at <paramref name="table"/>'s entry <paramref name="index"/> names, when it holds one.</summary>
    private static void AddEveryDirection(List<SamusProjectileProgram> starts, SamusProjectileSelectorTable table, int index)
    {
        if (SamusProjectileSelection.Entry(table, index) is { } header)
            starts.AddRange(SamusProjectileHeaders.Programs(header));
    }

    /// <summary>Adds the first list of the header at <paramref name="table"/>'s entry <paramref name="index"/>, when it holds one.</summary>
    private static void AddFirst(List<SamusProjectileProgram> starts, SamusProjectileSelectorTable table, int index)
    {
        if (SamusProjectileSelection.Entry(table, index) is { } header && SamusProjectileHeaders.Program(header, 0) is { } program)
            starts.Add(program);
    }

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
