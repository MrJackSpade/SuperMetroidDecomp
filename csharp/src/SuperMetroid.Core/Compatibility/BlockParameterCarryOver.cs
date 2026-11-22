namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How a behaviour written for one collision kind reads under every other kind the block later takes (#1551, #1722). A
/// block keeps one behaviour through a change of its collision kind, and each kind reads that same behaviour its own way:
/// each reacting kind as its own reaction (a shot block's break, a bomb block's, a crumble or Speed Booster block, a
/// conveyor, a spike's damage, a grapple anchor), a slope as a shape with its two reflections, an extension as a signed
/// offset and a door as an entry of the room's door list. The behaviours a block can keep are therefore each one complete
/// set of readings, and a behaviour written as any one reading is the behaviour the game's own write of that reading keeps,
/// which the kept behaviours' catalog names for each reading.
/// </summary>
/// <remarks>
/// <para>
/// Consulted whenever a behaviour is written as one kind's reading: when a room object writes a block's behaviour (a door
/// cap's trigger, a scroll's extension, a statue's slopes) and when the installation's level files, which hold each block's
/// behaviour as its collision kind's reading, are loaded; a later change of the block's collision kind, by the object's own
/// draws or by another object, then finds the reading the game finds. One converter per reading, created once with the kept
/// behaviours; lifetime: the process's, holding no state that changes.
/// </para>
/// <para>
/// Evidence: the original game stores one behaviour byte beside each block's level word and keeps it when a block's level
/// word is rewritten. Its reactions test the byte's sign to choose the area's table and index with the low seven
/// bits; the slope reactions read the low five bits as the shape and bits six and seven as the reflections; the extension
/// reactions read the byte sign-extended; and the door reaction indexes the door list with the low seven bits. The game
/// writes each reading as the first byte holding it; the kept behaviours' generator decides from the import codec which
/// behaviour each write keeps and writes it out by the reading, so no order is read at run time; the carry-over check
/// compares every kind's reading of every behaviour written as each other kind's reading with the codec's decoding of the
/// game's byte.
/// </para>
/// </remarks>
/// <typeparam name="TBehavior">A kept behaviour: every reading of one parameter the block keeps.</typeparam>
/// <typeparam name="TReading">One kind's reading of it.</typeparam>
[CompatibilityService("a block behaviour written for one collision kind is read by every kind the block later takes as the single parameter the block keeps")]
public sealed partial class BlockParameterCarryOver<TBehavior, TReading>
    where TBehavior : class
    where TReading : notnull
{
    /// <summary>The kept behaviour a write of each reading keeps.</summary>
    private readonly IReadOnlyDictionary<TReading, TBehavior> carrying;

    /// <summary>Holds the behaviour a write of each reading keeps.</summary>
    /// <param name="carrying">Each reading the converter writes behaviours as, with the kept behaviour its write keeps.</param>
    /// <exception cref="ArgumentNullException">The table or one of its behaviours is null.</exception>
    public BlockParameterCarryOver(IReadOnlyDictionary<TReading, TBehavior> carrying)
    {
        this.carrying = Ensure.NotNull(carrying);
        foreach (TBehavior behavior in carrying.Values)
            _ = Ensure.NotNull(behavior);
    }

    /// <summary>The behaviour a write of <paramref name="reading"/> keeps.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No kept behaviour holds <paramref name="reading"/>.</exception>
    public TBehavior Carrying(TReading reading) =>
        carrying.TryGetValue(reading, out TBehavior? behavior)
            ? behavior
            : throw new ArgumentOutOfRangeException(nameof(reading), reading, "No kept block behaviour holds this reading.");
}
