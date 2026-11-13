using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The cartridge's layout of Samus's animation programs (#1473, #1535): where each program starts, in the order the
/// programs lie, read once from the table of each pose's program for the over-read decoders.
/// </summary>
internal static class SamusAnimationProgramLayout
{
    /// <summary>A program as the cartridge lays it out.</summary>
    /// <param name="Program">The program.</param>
    /// <param name="NamedBy">Its lowest-numbered pose, which names it.</param>
    /// <param name="Start">Where it starts in the programs' bank.</param>
    internal readonly record struct LaidOutProgram(SamusPoseAnimation Program, SamusPoseId NamedBy, int Start);

    /// <summary>Every pose with a record, in the game's numbering.</summary>
    internal static SamusPoseId[] PosesWithRecords() =>
        [.. Enum.GetValues<SamusPoseId>().Where(SamusPoseNumbers.HasRecord).OrderBy(SamusPoseNumbers.Number)];

    /// <summary>Every program, in the order the cartridge lays them out.</summary>
    /// <exception cref="InvalidDataException">Two poses of one program start apart, or two programs start together.</exception>
    internal static LaidOutProgram[] Read(IImportCartridgeSource cartridge)
    {
        var starts = new Dictionary<SamusPoseAnimation, int>();
        var namedBy = new Dictionary<SamusPoseAnimation, SamusPoseId>();
        foreach (SamusPoseId pose in PosesWithRecords())
        {
            SamusPoseAnimation program = SamusAnimationDelays.AnimationForPose(pose);
            int start = cartridge.ReadCartridgeWord(
                SamusOverreadSourceDefinitions.SamusAnimationProgramTable + SamusPoseNumbers.Number(pose) * sizeof(ushort));
            if (starts.TryGetValue(program, out int known))
            {
                if (known != start)
                    throw new InvalidDataException($"Pose {pose}'s animation program starts at {start}, but another pose of the same program starts at {known}.");
                continue;
            }
            starts.Add(program, start);
            namedBy.Add(program, pose);
        }
        if (starts.Values.Distinct().Count() != starts.Count)
            throw new InvalidDataException("Two Samus animation programs start at the same place.");
        return [.. starts.OrderBy(pair => pair.Value).Select(pair => new LaidOutProgram(pair.Key, namedBy[pair.Key], pair.Value))];
    }
}
