using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Enemies.PipeBugs.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.PipeBugs;

/// <summary>
/// Translates the Pipe Bugs' typed program cursors to and from the native instruction pointers a recording holds, so
/// an imported native state resumes the same animation and replay comparisons keep checking it. Each typed step sits at
/// the native instruction the ROM oracle matched it with.
/// </summary>
internal sealed class PipeBugNativeInstructionPointers
{
    /// <summary>Each program's step addresses.</summary>
    private readonly Dictionary<PipeBugProgram, ushort[]> steps;

    /// <summary>Walks every typed Pipe Bug program beside its native list, failing on any mismatch.</summary>
    internal PipeBugNativeInstructionPointers(IImportCartridgeSource cartridge) =>
        steps = NativeEnemyProgramOracle.StepAddresses(cartridge, PipeBugNativeListAddresses.Bank, PipeBugNativeListAddresses.Entries,
            PipeBugAnimations.Programs, PipeBugFrameDefinitions.Spritemap, _ => null,
            (word, _, _, at) => throw new InvalidDataException($"{at}: the Pipe Bug lists use no owner instruction ${word:X4}."));

    /// <summary>The native instruction pointer <paramref name="cursor"/> stands for.</summary>
    internal ushort Pointer(AnimationProgramCursor<PipeBugProgram> cursor) => steps[cursor.Program][cursor.Position];

    /// <summary>The cursor native pointer <paramref name="address"/> stands for: the program entry when it is one.</summary>
    internal AnimationProgramCursor<PipeBugProgram> CursorAt(ushort address)
    {
        var entry = PipeBugNativeListAddresses.Entries.Where(item => item.Entry == address).ToList();
        if (entry.Count == 1)
            return AnimationProgramCursor<PipeBugProgram>.Start(entry[0].Program);
        var matches = steps.SelectMany(program => program.Value.Select((step, position) => (program.Key, step, position)))
            .Where(item => item.step == address)
            .Select(item => new AnimationProgramCursor<PipeBugProgram>(item.Key, item.position))
            .ToList();
        return matches.Count == 1
            ? matches[0]
            : throw new InvalidDataException($"Native Pipe Bug instruction pointer ${PipeBugNativeListAddresses.Bank:X2}:{address:X4} matches {matches.Count} typed steps.");
    }
}
