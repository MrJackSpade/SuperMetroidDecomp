using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The cartridge's layout of Samus's animation programs (#1473, #1535, #2048): where each program starts, in the order the
/// programs lie, read once from the table of each pose record's program for the over-read decoders.
/// </summary>
internal static class SamusAnimationProgramLayout
{
    /// <summary>A program as the cartridge lays it out.</summary>
    /// <param name="Program">The program: the animation it holds.</param>
    /// <param name="Start">Where it starts in the programs' bank.</param>
    internal readonly record struct LaidOutProgram(SamusPoseAnimation Program, int Start);

    /// <summary>A pose record's number and the animation the record plays.</summary>
    /// <param name="Number">The record's number in the cartridge's pose tables.</param>
    /// <param name="Animation">The animation the record plays.</param>
    internal readonly record struct RecordAnimation(int Number, SamusPoseAnimation Animation);

    /// <summary>
    /// The animation each record the import lays out plays, by record number: the animation of the pose the record names,
    /// or, for a record no pose names, the animation a read past an earlier animation still reaches (#2048).
    /// </summary>
    internal static IEnumerable<RecordAnimation> RecordAnimations()
    {
        for (int number = 0; number < SamusPoseNumbering.RecordCount; number++)
        {
            if (SamusPoseNumbering.NamesPose(number))
                yield return new(number, SamusAnimationDelays.AnimationForPose(SamusPoseNumbering.FromNumber(number)));
            else if (SamusPoseNumbering.AnimationWithoutPose(number) is { } animation)
                yield return new(number, SamusAnimationDelays.AnimationNamed(animation));
        }
    }

    /// <summary>Every program, in the order the cartridge lays them out.</summary>
    /// <exception cref="InvalidDataException">
    /// Two records of one program start apart, two programs start together, or an animation of Samus's is not laid out.
    /// </exception>
    internal static LaidOutProgram[] Read(IImportCartridgeSource cartridge)
    {
        var starts = new Dictionary<SamusPoseAnimation, int>();
        foreach ((int number, SamusPoseAnimation program) in RecordAnimations())
        {
            int start = cartridge.ReadCartridgeWord(SamusOverreadSourceDefinitions.SamusAnimationProgramTable + number * sizeof(ushort));
            if (starts.TryGetValue(program, out int known))
            {
                if (known != start)
                    throw new InvalidDataException($"Record {number}'s {program.Name} animation starts at {start}, but another record of the same animation starts at {known}.");
                continue;
            }
            starts.Add(program, start);
        }
        if (starts.Values.Distinct().Count() != starts.Count)
            throw new InvalidDataException("Two Samus animation programs start at the same place.");
        if (starts.Count != SamusAnimationDelayDefinitions.All.Count)
            throw new InvalidDataException($"The cartridge lays out {starts.Count} of Samus's {SamusAnimationDelayDefinitions.All.Count} animations.");
        return [.. starts.OrderBy(pair => pair.Value).Select(pair => new LaidOutProgram(pair.Key, pair.Value))];
    }
}
