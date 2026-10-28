using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Where each Samus projectile program's list stands in the cartridge (#1301, #1411): its entry and the word just before
/// it, read from the player's cartridge at install with the other Samus overread data. The instruction pointer a cursor
/// stands for is what the SpaceTime and chainsaw glitch paths and the Power Bomb fuse's list arithmetic see, and a trail
/// spawned at a list's start reads the preceding word as its animation frame.
/// </summary>
public sealed class InstalledProjectileListPositions
{
    /// <summary>Bytes in one timed record: its timer, spritemap, radii and animation frame.</summary>
    internal const int TimedRecordByteCount = 8;

    /// <summary>Each step's instruction pointer, by program.</summary>
    internal readonly Dictionary<SamusProjectileProgram, ushort[]> stepPositions;
    /// <summary>The cursor each instruction pointer stands for: a program's entry, else the one step at that pointer.</summary>
    private readonly Dictionary<ushort, AnimationProgramCursor<SamusProjectileProgram>> _cursors;
    /// <summary>Each timed record's instruction pointer, by <see cref="SamusProjectileFrame.Record"/>.</summary>
    private readonly ushort[] _recordPositions;

    /// <summary>Lays every program's steps out from its installed position.</summary>
    /// <param name="positions">Every program's list entry and preceding word.</param>
    /// <exception cref="InvalidDataException">A program has no position, or a timed record stands at no pointer or at two.</exception>
    public InstalledProjectileListPositions(IReadOnlyDictionary<SamusProjectileProgram, SamusProjectileListPosition> positions)
    {
        Ensure.NotNull(positions);
        foreach (SamusProjectileProgram program in Enum.GetValues<SamusProjectileProgram>())
            if (!positions.TryGetValue(program, out SamusProjectileListPosition position))
                throw new InvalidDataException($"The projectile list layout has no position for {program}.");
            else if ((uint)position.Entry > ushort.MaxValue || (uint)position.PrecedingWord > ushort.MaxValue)
                throw new InvalidDataException($"{program}'s list position {position} lies outside a sixteen-bit bank.");
        Positions = positions;
        stepPositions = Enum.GetValues<SamusProjectileProgram>().ToDictionary(program => program, Layout);
        _cursors = BuildCursors();
        _recordPositions = BuildRecordPositions();
    }

    /// <summary>Every program's list entry and preceding word.</summary>
    public IReadOnlyDictionary<SamusProjectileProgram, SamusProjectileListPosition> Positions { get; }

    /// <summary>The word just before <paramref name="program"/>'s list.</summary>
    internal ushort Prefix(SamusProjectileProgram program) => (ushort)Positions[program].PrecedingWord;

    /// <summary>The instruction pointer <paramref name="cursor"/> stands for.</summary>
    internal ushort Position(AnimationProgramCursor<SamusProjectileProgram> cursor) => stepPositions[cursor.Program][cursor.Position];

    /// <summary>The instruction pointer of a projectile's cursor; a cleared projectile's is zero.</summary>
    internal ushort Position(AnimationProgramCursor<SamusProjectileProgram>? cursor) => cursor is { } live ? Position(live) : (ushort)0;

    /// <summary>The instruction pointer timed record <paramref name="record"/> stands at.</summary>
    internal ushort RecordPosition(int record) => _recordPositions[record];

    /// <summary>The cursor instruction pointer <paramref name="place"/> stands for; zero is a cleared projectile.</summary>
    internal AnimationProgramCursor<SamusProjectileProgram>? CursorAt(int place) =>
        place == 0 ? null : _cursors.TryGetValue(checked((ushort)place), out var cursor) ? cursor
            : throw new InvalidDataException($"$93:{place:X4} is not a Samus projectile list position.");

    /// <summary>
    /// Moves <paramref name="cursor"/> <paramref name="bytes"/> further through the lists, as native list arithmetic does;
    /// the result must land on a list position.
    /// </summary>
    internal AnimationProgramCursor<SamusProjectileProgram> Offset(AnimationProgramCursor<SamusProjectileProgram> cursor, int bytes) =>
        CursorAt(unchecked((ushort)(Position(cursor) + bytes))) ?? throw new InvalidDataException("A list offset cleared the projectile.");

    /// <summary>The programs in list order.</summary>
    private IEnumerable<SamusProjectileProgram> ListOrder => Positions.OrderBy(position => position.Value.Entry).Select(position => position.Key);

    /// <summary>Each step's place: eight bytes per timed record, two per delete, four per goto; a fall-through rests on the next list.</summary>
    private ushort[] Layout(SamusProjectileProgram program)
    {
        var steps = SamusProjectileAnimations.Programs.Steps(program);
        var places = new ushort[steps.Count];
        int place = Positions[program].Entry;
        for (int position = 0; position < steps.Count; position++)
        {
            var step = steps[position];
            places[position] = step is AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.FallIntoStep fallInto
                ? (ushort)Positions[fallInto.Target].Entry
                : (ushort)place;
            place += step is AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.ShowStep ? TimedRecordByteCount : sizeof(ushort);
        }
        return places;
    }

    /// <summary>Indexes every list position; an entry stands for its own program's start.</summary>
    private Dictionary<ushort, AnimationProgramCursor<SamusProjectileProgram>> BuildCursors()
    {
        var cursors = ListOrder.ToDictionary(program => (ushort)Positions[program].Entry, AnimationProgramCursor<SamusProjectileProgram>.Start);
        foreach (SamusProjectileProgram program in ListOrder)
        {
            ushort[] places = stepPositions[program];
            for (int position = 0; position < places.Length; position++)
                if (!cursors.ContainsKey(places[position]))
                    cursors.Add(places[position], new(program, position));
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
