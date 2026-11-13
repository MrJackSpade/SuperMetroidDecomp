namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How a behaviour written for one collision kind reads under every other kind the block later takes (#1551, #1722). A
/// block keeps one behaviour through a change of its collision kind, and each kind reads that same behaviour its own way:
/// each reacting kind as its own reaction (a shot block's break, a bomb block's, a crumble or Speed Booster block, a
/// conveyor, a spike's damage, a grapple anchor), a slope as a shape with its two reflections, an extension as a signed
/// offset and a door as an entry of the room's door list. The behaviours a block can keep are therefore each one complete
/// set of readings, and a behaviour written as any one reading is the first kept behaviour holding that reading.
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
/// word is rewritten. Its reaction dispatchers test the byte's sign to choose the area's table and index with the low seven
/// bits; the slope handlers read the low five bits as the shape and bits six and seven as the reflections; the extension
/// handlers read the byte sign-extended; and the door handler indexes the door list with the low seven bits. The kept
/// behaviours are the import codec's decoding of every byte under every kind, in the byte's order, so the first holding a
/// reading is the byte the game's own writes of that reading store; the carry-over check compares every kind's reading of
/// every behaviour written as each other kind's reading with that decoding.
/// </para>
/// </remarks>
/// <typeparam name="TBehavior">A kept behaviour: every reading of one parameter the block keeps.</typeparam>
/// <typeparam name="TReading">One kind's reading of it.</typeparam>
[CompatibilityService("a block behaviour written for one collision kind is read by every kind the block later takes as the single parameter the block keeps")]
public sealed class BlockParameterCarryOver<TBehavior, TReading>
    where TBehavior : class
    where TReading : notnull
{
    /// <summary>The first kept behaviour holding each reading.</summary>
    private readonly Dictionary<TReading, TBehavior> carrying = [];

    /// <summary>Indexes the kept behaviours by the reading <paramref name="read"/> takes of each, the first winning.</summary>
    /// <param name="kept">Every behaviour a block can keep, in the order the game numbers them.</param>
    /// <param name="read">The reading the converter writes behaviours as.</param>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public BlockParameterCarryOver(IEnumerable<TBehavior> kept, Func<TBehavior, TReading> read)
    {
        Ensure.NotNull(kept);
        Ensure.NotNull(read);
        foreach (TBehavior behavior in kept)
            carrying.TryAdd(read(Ensure.NotNull(behavior)), behavior);
    }

    /// <summary>The behaviour a write of <paramref name="reading"/> keeps: the first kept behaviour holding it.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No kept behaviour holds <paramref name="reading"/>.</exception>
    public TBehavior Carrying(TReading reading) =>
        carrying.TryGetValue(reading, out TBehavior? behavior)
            ? behavior
            : throw new ArgumentOutOfRangeException(nameof(reading), reading, "No kept block behaviour holds this reading.");
}
