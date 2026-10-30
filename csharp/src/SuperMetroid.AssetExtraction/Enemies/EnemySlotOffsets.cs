using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The codec between Core's enemy slot ordinals and the byte offsets the cartridge numbers its enemy records by (#1471):
/// each record takes 64 bytes of work memory, so slot <c>n</c> is offset <c>64 * n</c>, and a word that names no enemy
/// holds all ones. Core addresses enemies by slot object or ordinal; this type is the one place that knows the offset,
/// for the native-state imports and the checks that compare with native memory.
/// </summary>
[EncodingOwner("enemy slot byte offsets")]
public static class EnemySlotOffsets
{
    /// <summary>The bytes each enemy record takes in the cartridge's work memory.</summary>
    public const int RecordSize = 64;

    /// <summary>The word a slot reference holds when it names no enemy.</summary>
    public const int NoEnemy = ushort.MaxValue;

    /// <summary>The bytes every enemy record takes together.</summary>
    public static int TableSize => EnemySlotOrder.Capacity * RecordSize;

    /// <summary>The byte offset of slot <paramref name="slot"/>'s record.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="slot"/> is not a slot ordinal.</exception>
    public static int OffsetOf(int slot) => Ensure.BetweenInclusive(0, EnemySlotOrder.Capacity - 1, slot) * RecordSize;

    /// <summary>The slot whose record starts at byte offset <paramref name="offset"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is not the start of an enemy record.</exception>
    public static int SlotOf(int offset)
    {
        if (!IsSlotOffset(offset))
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "The offset is not the start of an enemy record.");
        return offset / RecordSize;
    }

    /// <summary>Whether <paramref name="offset"/> is the start of an enemy record.</summary>
    public static bool IsSlotOffset(int offset) => offset >= 0 && offset < TableSize && offset % RecordSize == 0;

    /// <summary>The slot a slot-reference word names, or null for <see cref="NoEnemy"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="word"/> is neither a record offset nor <see cref="NoEnemy"/>.</exception>
    public static int? SlotOrNoneOf(int word) => word == NoEnemy ? null : SlotOf(word);

    /// <summary>The slot-reference word that names <paramref name="slot"/>, or <see cref="NoEnemy"/> for null.</summary>
    public static int WordOf(int? slot) => slot is { } ordinal ? OffsetOf(ordinal) : NoEnemy;
}
