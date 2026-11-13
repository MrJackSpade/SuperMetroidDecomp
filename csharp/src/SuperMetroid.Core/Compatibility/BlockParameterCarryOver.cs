namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How the behaviour a room object writes for one collision kind reads under every other kind the block later takes
/// (#1551). A block keeps one behaviour through a change of its collision kind, and each kind reads that same behaviour
/// its own way: a reacting kind as an entry of its common or area reactions, a slope as a shape with its two
/// reflections, an extension as a signed offset, and a door as an entry of the room's door list. A behaviour written as
/// any one of these readings therefore carries the other three, related as stated here.
/// </summary>
/// <remarks>
/// <para>
/// Consulted whenever a room object writes a block's behaviour (a door cap's trigger, a scroll's extension, a statue's
/// slopes), so that a later change of the block's collision kind, by the object's own draws or by another object, finds
/// the reading the game finds. Behaviours the installation holds are decoded once, with every reading, by the import's
/// codec of the game's block behaviour.
/// </para>
/// <para>
/// Evidence: the original game stores one behaviour byte beside each block's level word and keeps it when a block's
/// level word is rewritten. Its reaction dispatchers test the byte's sign to choose the area's table and index with the
/// low seven bits; the slope handlers read the low five bits as the shape and bits six and seven as the reflections; the
/// extension handlers read the byte sign-extended; and the door handler indexes the door list with the low seven bits.
/// Reading the area's reactions therefore coincides with a negative offset and with a vertical reflection, and entries
/// from 64 coincide with a horizontal reflection.
/// </para>
/// </remarks>
[CompatibilityService("a block behaviour written for one collision kind is read by every kind the block later takes as the single parameter the block keeps")]
public static class BlockParameterCarryOver
{
    /// <summary>The entries each reaction list holds; an extension's offset spans as many backward as forward.</summary>
    public const int EntriesPerReactionList = 128;

    /// <summary>The slope shapes; a reaction entry reads as a slope shape in a cycle of this length.</summary>
    public const int SlopeShapes = 32;

    /// <summary>The first reaction entry that reads as a slope reflected left to right.</summary>
    public const int FirstHorizontallyReflectedEntry = 64;

    /// <summary>The extension offset a reaction reads as: an area reaction reads as a backward offset.</summary>
    /// <param name="areaSpecific">Whether the reaction is one of the area's.</param>
    /// <param name="entry">The reaction's entry, 0 through 127.</param>
    public static int ExtensionOffsetOf(bool areaSpecific, int entry) =>
        areaSpecific ? entry - EntriesPerReactionList : entry;

    /// <summary>Whether an extension offset reads as one of the area's reactions: a backward offset does.</summary>
    /// <param name="offset">The offset, -128 through 127.</param>
    public static bool IsAreaSpecificReaction(int offset) => offset < 0;

    /// <summary>The reaction entry an extension offset reads as.</summary>
    /// <param name="offset">The offset, -128 through 127.</param>
    public static int ReactionEntryOf(int offset) => offset < 0 ? offset + EntriesPerReactionList : offset;

    /// <summary>The slope shape a reaction entry reads as.</summary>
    /// <param name="entry">The reaction's entry, 0 through 127.</param>
    public static int SlopeShapeOf(int entry) => entry % SlopeShapes;

    /// <summary>Whether a reaction entry reads as a slope reflected left to right.</summary>
    /// <param name="entry">The reaction's entry, 0 through 127.</param>
    public static bool ReflectsSlopeHorizontally(int entry) => entry >= FirstHorizontallyReflectedEntry;

    /// <summary>The reaction entry a slope reads as: its shape, past the reflected entries when reflected left to right.</summary>
    /// <param name="shape">The slope's shape, 0 through 31.</param>
    /// <param name="reflectedHorizontally">Whether the slope is reflected left to right.</param>
    public static int ReactionEntryOf(int shape, bool reflectedHorizontally) =>
        shape + (reflectedHorizontally ? FirstHorizontallyReflectedEntry : 0);
}
