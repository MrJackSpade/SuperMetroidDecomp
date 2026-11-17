using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification's native view of the trail programs: where bank $90 stores their steps and selections.</summary>
internal static class ProjectileTrailNative
{
    /// <summary>The first selection past the beam shots' thirty-two: the missiles, then the echoes by their beams.</summary>
    private const int FirstSpecialSelection = 32;

    /// <summary>The selections of the beam shots' uncharged half; the charged half follows.</summary>
    private const int ChargedSelectionOffset = 16;

    /// <summary>The bank-$90 address of the movement instruction a move step names.</summary>
    internal static ushort InstructionAddress(ProjectileTrailCue movement) => movement switch
    {
        ProjectileTrailCue.MoveLeftDown => 0xb525,
        ProjectileTrailCue.MoveRightDown => 0xb587,
        _ => throw new ArgumentOutOfRangeException(nameof(movement), movement, "Undefined trail movement."),
    };

    /// <summary>The selection tables' native words.</summary>
    extension(ProjectileTrailDefinitions)
    {
        /// <summary>$90:B4C9, the empty program.</summary>
        internal static ushort Empty => ProjectileTrailSources.Start(ProjectileTrailSegment.Empty);
        /// <summary>$90:B4CB, the left ice program.</summary>
        internal static ushort LeftIce => ProjectileTrailSources.Start(ProjectileTrailSegment.LeftIce);
        /// <summary>$90:B52D, the right ice program.</summary>
        internal static ushort RightIce => ProjectileTrailSources.Start(ProjectileTrailSegment.RightIce);
        /// <summary>$90:B58F, the wave program.</summary>
        internal static ushort Wave => ProjectileTrailSources.Start(ProjectileTrailSegment.Wave);
        /// <summary>$90:B5A1, the missile program.</summary>
        internal static ushort Missile => ProjectileTrailSources.Start(ProjectileTrailSegment.Missile);
    }

    /// <summary>The programs' native mechanics words.</summary>
    extension(ProjectileTrailSparkleAnimations)
    {
        /// <summary>
        /// The native word stored at bus address <paramref name="address"/> for a beat or an end: a frame's duration, a
        /// move cue's instruction, zero for the end after a program's last beat.
        /// </summary>
        internal static bool TryRead(int address, out ushort word)
        {
            word = 0;
            if ((address & ~0xffff) != SamusSourceDefinitions.MovementBank.Value)
                return false;
            foreach (ProjectileTrailSegment program in Enum.GetValues<ProjectileTrailSegment>())
            {
                IReadOnlyList<AnimationBeat<ProjectileTrailFrame, ProjectileTrailCue>> beats = ProjectileTrailSparkleAnimations.Segments.Beats(program);
                for (int beat = 0; beat <= beats.Count; beat++)
                {
                    if (ProjectileTrailSources.Address(program, beat) != (ushort)address)
                        continue;
                    word = beat == beats.Count ? (ushort)0 : beats[beat] switch
                    {
                        AnimationBeat<ProjectileTrailFrame, ProjectileTrailCue>.Event move => InstructionAddress(move.Raised),
                        AnimationBeat<ProjectileTrailFrame, ProjectileTrailCue>.Frame frame => checked((ushort)frame.Duration),
                        _ => throw new InvalidOperationException($"Undefined trail beat {beats[beat]}."),
                    };
                    return true;
                }
            }
            return false;
        }

        /// <summary>The native word stored at <paramref name="address"/>; any other address is rejected.</summary>
        internal static ushort Read(int address) => ProjectileTrailSparkleAnimations.TryRead(address, out ushort word)
            ? word
            : throw new InvalidDataException($"Projectile trail program word ${address:X6} is outside compiled mechanics data.");
    }

    /// <summary>
    /// The place in each side's selection table at which the trail spawner reads <paramref name="trailing"/>'s programs:
    /// a beam shot's by its beams, in the charged half when charged; the missile's and Super Missile's after the beam
    /// shots; an echo's after those, by the beams it carries.
    /// </summary>
    internal static int Selection(SamusProjectileType trailing) => trailing.Family switch
    {
        SamusProjectileFamily.Missile => FirstSpecialSelection,
        SamusProjectileFamily.SuperMissile => FirstSpecialSelection + 1,
        SamusProjectileFamily.Beam => trailing.BeamCombination.TableIndex +
            (trailing.Echo is not null ? FirstSpecialSelection : trailing.IsChargedBeam ? ChargedSelectionOffset : 0),
        SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb or SamusProjectileFamily.BeamExplosion or
            SamusProjectileFamily.MissileExplosion => throw new ArgumentOutOfRangeException(nameof(trailing), trailing, "The projectile leaves no trail."),
        _ => throw new ArgumentOutOfRangeException(nameof(trailing), trailing, "Undefined projectile family."),
    };

    /// <summary>The bank-$90 address of the first record that shows <paramref name="frame"/>.</summary>
    internal static ushort Address(ProjectileTrailFrame frame) => ProjectileTrailSourcesConstants.FrameAddress(frame);

    /// <summary>The bank-$90 address of every timed record the trail programs store, in address order.</summary>
    internal static IEnumerable<ushort> RecordAddresses => ProjectileTrailSources.Records.Select(record => record.Key);

    /// <summary>The frame the record stored at bank-$90 address <paramref name="address"/> shows.</summary>
    internal static ProjectileTrailFrame FrameAt(ushort address) =>
        ProjectileTrailSources.Records.FirstOrDefault(record => record.Key == address) is { Key: not 0 } found
            ? found.Value
            : throw new InvalidDataException($"No trail record is stored at $90:{address:X4}.");

    /// <summary>The cartridge's retained tile word for a side's shown pose: the pose's stock appearance, or zero once cleared.</summary>
    internal static ushort NativeWord(ProjectileTrailFrame? shownPose) =>
        shownPose is { } pose
            ? SnesObjAttributeWord.From(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.ImportProjectiles.Compositions.Trails.Resolve(pose)).Raw
            : (ushort)0;

    /// <summary>
    /// The shown frame a retained tile word stands for: none for the cleared word, otherwise the frame whose stock
    /// appearance it is.
    /// </summary>
    /// <exception cref="InvalidDataException">No frame has the word as its appearance.</exception>
    internal static ProjectileTrailFrame? ShownPose(ushort nativeWord)
    {
        if (nativeWord == 0)
            return null;
        foreach (ProjectileTrailFrame pose in Enum.GetValues<ProjectileTrailFrame>())
        {
            if (NativeWord(pose) == nativeWord)
                return pose;
        }
        throw new InvalidDataException($"No trail pose appears as tile word ${nativeWord:X4}.");
    }

    /// <summary>The import document's name of the first record that shows <paramref name="frame"/>.</summary>
    internal static string Name(ProjectileTrailFrame frame) =>
        ProjectileTrailVisualDefinitions.RecordNames[ProjectileTrailSources.Records.Select(record => record.Value).ToList().IndexOf(frame)];
}
