using SuperMetroid.Core;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where each Samus projectile program's steps stand in the cartridge's bank-$93 lists (#1301, #1411, #1474): extraction's
/// private knowledge of the instruction pointer each animation position stands for, which the glitch paths' list
/// arithmetic and fresh trails read. The import pipeline decodes those reads into the Samus overread services; the game
/// never sees a list position.
/// </summary>
internal sealed class SamusProjectileListLayout
{
    /// <summary>Bytes in one timed record: its timer, spritemap, radii and animation frame.</summary>
    internal const int TimedRecordByteCount = 8;

    /// <summary>Each step's instruction pointer, by program; checks read it through their access extension.</summary>
    internal readonly Dictionary<SamusProjectileProgram, ushort[]> stepPositions;
    /// <summary>The cursor each instruction pointer stands for: a program's entry, else the one step at that pointer.</summary>
    private readonly Dictionary<ushort, AnimationProgramCursor<SamusProjectileProgram>> _cursors;
    /// <summary>Each timed record's instruction pointer, by <see cref="SamusProjectileFrame.Record"/>.</summary>
    private readonly ushort[] _recordPositions;
    /// <summary>Each program's list entry and the word just before it.</summary>
    private readonly IReadOnlyDictionary<SamusProjectileProgram, ListPosition> _positions;

    /// <summary>Where one program's list stands: its entry and the word just before it.</summary>
    /// <param name="Entry">The list's first instruction pointer.</param>
    /// <param name="PrecedingWord">The word just before the list, which a trail spawned at the list's start reads as its animation frame.</param>
    internal readonly record struct ListPosition(ushort Entry, ushort PrecedingWord);

    /// <summary>Lays every program's steps out from its list position.</summary>
    /// <param name="positions">Every program's list entry and preceding word.</param>
    /// <exception cref="InvalidDataException">A program has no position, or a timed record stands at no pointer or at two.</exception>
    internal SamusProjectileListLayout(IReadOnlyDictionary<SamusProjectileProgram, ListPosition> positions)
    {
        Ensure.NotNull(positions);
        foreach (SamusProjectileProgram program in Enum.GetValues<SamusProjectileProgram>())
        {
            if (!positions.ContainsKey(program))
                throw new InvalidDataException($"The projectile list layout has no position for {program}.");
        }
        _positions = positions;
        stepPositions = Enum.GetValues<SamusProjectileProgram>().ToDictionary(program => program, Layout);
        _cursors = BuildCursors();
        _recordPositions = BuildRecordPositions();
    }

    /// <summary>The word just before <paramref name="program"/>'s list.</summary>
    internal ushort Prefix(SamusProjectileProgram program) => _positions[program].PrecedingWord;

    /// <summary>The instruction pointer <paramref name="cursor"/> stands for.</summary>
    internal ushort Position(AnimationProgramCursor<SamusProjectileProgram> cursor) => stepPositions[cursor.Program][cursor.Position];

    /// <summary>The instruction pointer timed record <paramref name="record"/> stands at.</summary>
    internal ushort RecordPosition(int record) => _recordPositions[record];

    /// <summary>The cursor a nonzero instruction pointer <paramref name="place"/> stands for, when it is a list position.</summary>
    internal bool TryCursorAt(int place, out AnimationProgramCursor<SamusProjectileProgram> cursor) =>
        _cursors.TryGetValue(checked((ushort)place), out cursor);

    /// <summary>The programs in list order.</summary>
    private IEnumerable<SamusProjectileProgram> ListOrder => _positions.OrderBy(position => position.Value.Entry).Select(position => position.Key);

    /// <summary>Each step's place: eight bytes per timed record, two per delete, four per goto; a fall-through rests on the next list.</summary>
    private ushort[] Layout(SamusProjectileProgram program)
    {
        var steps = SamusProjectileAnimations.Programs.Steps(program);
        var places = new ushort[steps.Count];
        int place = _positions[program].Entry;
        for (int position = 0; position < steps.Count; position++)
        {
            var step = steps[position];
            places[position] = step is AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.FallIntoStep fallInto
                ? _positions[fallInto.Target].Entry
                : (ushort)place;
            place += step is AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.ShowStep ? TimedRecordByteCount : sizeof(ushort);
        }
        return places;
    }

    /// <summary>Indexes every list position; an entry stands for its own program's start.</summary>
    private Dictionary<ushort, AnimationProgramCursor<SamusProjectileProgram>> BuildCursors()
    {
        var cursors = ListOrder.ToDictionary(program => _positions[program].Entry, AnimationProgramCursor<SamusProjectileProgram>.Start);
        foreach (SamusProjectileProgram program in ListOrder)
        {
            ushort[] places = stepPositions[program];
            for (int position = 0; position < places.Length; position++)
                cursors.TryAdd(places[position], new(program, position));
        }
        return cursors;
    }

    /// <summary>Finds the one pointer each timed record is shown from.</summary>
    private ushort[] BuildRecordPositions()
    {
        var places = new int?[SamusProjectileRecordDefinitions.Records.Length];
        foreach (SamusProjectileProgram program in ListOrder)
        {
            var steps = SamusProjectileAnimations.Programs.Steps(program);
            for (int position = 0; position < steps.Count; position++)
            {
                if (steps[position] is not AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.ShowStep show)
                    continue;
                int place = stepPositions[program][position];
                int record = show.Frame.Record;
                if (places[record] is { } known && known != place)
                    throw new InvalidDataException($"Timed projectile record {record} stands at $93:{known:X4} and $93:{place:X4}.");
                places[record] = place;
            }
        }
        return [.. places.Select((place, record) => checked((ushort)(place ?? throw new InvalidDataException($"Timed projectile record {record} stands in no list."))))];
    }
}
