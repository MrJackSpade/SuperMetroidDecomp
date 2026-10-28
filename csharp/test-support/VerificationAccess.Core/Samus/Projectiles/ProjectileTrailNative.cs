using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification's native view of the trail programs: where bank $90 stores their steps and selections.</summary>
internal static class ProjectileTrailNative
{
    /// <summary>$90:B5BB, the left selection table; the right table follows it at $90:B609.</summary>
    private const int LeftSelections = 0x90b5bb;

    /// <summary>The words the selector reads: the left table, the right table and the spawner code past it.</summary>
    private const int SelectorWords = 103;

    /// <summary>The selections each side's table holds.</summary>
    private const int TableSelections = 39;

    /// <summary>The bank-$90 address of the movement instruction a move step names.</summary>
    internal static ushort InstructionAddress(ProjectileTrailInstruction movement) => movement switch
    {
        ProjectileTrailInstruction.MoveLeftDown => 0xb525,
        ProjectileTrailInstruction.MoveRightDown => 0xb587,
        ProjectileTrailInstruction.MoveLeftUp => 0xb5b3,
        _ => throw new ArgumentOutOfRangeException(nameof(movement), movement, "Undefined trail movement."),
    };

    /// <summary>The selection tables' native words.</summary>
    extension(ProjectileTrailDefinitions)
    {
        /// <summary>$90:B4C9, the empty program.</summary>
        internal static ushort Empty => ProjectileTrailSources.Start(ProjectileTrailProgram.Empty);
        /// <summary>$90:B4CB, the left ice program.</summary>
        internal static ushort LeftIce => ProjectileTrailSources.Start(ProjectileTrailProgram.LeftIce);
        /// <summary>$90:B52D, the right ice program.</summary>
        internal static ushort RightIce => ProjectileTrailSources.Start(ProjectileTrailProgram.RightIce);
        /// <summary>$90:B58F, the wave program.</summary>
        internal static ushort Wave => ProjectileTrailSources.Start(ProjectileTrailProgram.Wave);
        /// <summary>$90:B5A1, the missile program.</summary>
        internal static ushort Missile => ProjectileTrailSources.Start(ProjectileTrailProgram.Missile);

        /// <summary>
        /// The start of the program the selector word at <paramref name="address"/> selects, or null for a word past the
        /// right table, which names no program.
        /// </summary>
        internal static ushort? ReadSelector(int address)
        {
            int offset = address - LeftSelections;
            if ((uint)offset >= SelectorWords * sizeof(ushort) || (offset & 1) != 0)
                throw new InvalidDataException($"Projectile trail selector ${address:X6} is outside the bounded selector window.");
            int index = offset / sizeof(ushort);
            ProjectileTrailProgram? program = index < TableSelections
                ? ProjectileTrailSelection.Select(index, HorizontalDirection.Left)
                : ProjectileTrailSelection.Select(index - TableSelections, HorizontalDirection.Right);
            return program is { } selected ? ProjectileTrailSources.Start(selected) : null;
        }
    }

    /// <summary>The programs' native mechanics words.</summary>
    extension(ProjectileTrailProgramDefinitions)
    {
        /// <summary>The native word of the step stored at bus address <paramref name="address"/>: a pose's duration, a move's instruction, zero for the end.</summary>
        internal static bool TryRead(int address, out ushort word)
        {
            word = 0;
            if ((address & ~0xffff) != SamusSourceDefinitions.MovementBank.Value)
                return false;
            foreach ((ProjectileTrailProgram program, ProjectileTrailStep[] steps) in ProjectileTrailProgramDefinitions.Programs)
            for (int step = 0; step < steps.Length; step++)
            {
                if (ProjectileTrailSources.Address(new ProjectileTrailCursor(program, step)) != (ushort)address)
                    continue;
                word = steps[step] switch
                {
                    ProjectileTrailStep.End => 0,
                    ProjectileTrailStep.Move move => InstructionAddress(move.Movement),
                    ProjectileTrailStep.Pose pose => checked((ushort)pose.Duration),
                    _ => throw new InvalidOperationException($"Undefined trail step {steps[step]}."),
                };
                return true;
            }
            return false;
        }

        /// <summary>The native word of the step stored at <paramref name="address"/>; any other address is rejected.</summary>
        internal static ushort Read(int address) => ProjectileTrailProgramDefinitions.TryRead(address, out ushort word)
            ? word
            : throw new InvalidDataException($"Projectile trail program word ${address:X6} is outside compiled mechanics data.");
    }

    /// <summary>The bank-$90 address of <paramref name="frame"/>'s pose.</summary>
    internal static ushort Address(ProjectileTrailFrame frame) => ProjectileTrailSources.FrameAddress(frame);

    /// <summary>The frame whose pose is stored at bank-$90 address <paramref name="address"/>.</summary>
    internal static ProjectileTrailFrame FrameAt(ushort address) =>
        ProjectileTrailVisualDefinitions.Frames.FirstOrDefault(frame => Address(frame) == address) is var found && Address(found) == address
            ? found
            : throw new InvalidDataException($"No trail pose is stored at $90:{address:X4}.");

    /// <summary>The cartridge's retained tile word for a side's shown pose: the pose's stock appearance, or zero once cleared.</summary>
    internal static ushort NativeWord(ProjectileTrailFrame? shownPose) =>
        shownPose is { } pose
            ? SnesObjAttributeWord.From(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Projectiles.Trails.Resolve(pose)).Raw
            : (ushort)0;

    /// <summary>
    /// The shown pose a retained tile word stands for: none for the cleared word, otherwise the first pose whose stock
    /// appearance it is. Poses that share a word look alike.
    /// </summary>
    /// <exception cref="InvalidDataException">No pose has the word as its appearance.</exception>
    internal static ProjectileTrailFrame? ShownPose(ushort nativeWord)
    {
        if (nativeWord == 0)
            return null;
        foreach (ProjectileTrailFrame pose in ProjectileTrailVisualDefinitions.Frames)
        {
            if (NativeWord(pose) == nativeWord)
                return pose;
        }
        throw new InvalidDataException($"No trail pose appears as tile word ${nativeWord:X4}.");
    }

    /// <summary>The installed name of <paramref name="frame"/>.</summary>
    internal static string Name(ProjectileTrailFrame frame) =>
        ProjectileTrailVisualDefinitions.FrameNames[ProjectileTrailVisualDefinitions.Frames.ToList().IndexOf(frame)];
}
