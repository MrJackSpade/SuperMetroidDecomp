namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The single conversion between <see cref="MotherBrainHitboxParts"/> and the shared hitbox-enable word a native
/// snapshot stores, whose three low bits are body, brain and neck. The parts' values are arbitrary: each part's word
/// bit is named here, and every conversion walks this table bit by bit. The snapshot import decodes through
/// <see cref="FromNativeWord"/>, rejecting a word with any other bit set, and word-level checks encode through
/// <see cref="ToNativeWord"/>; nothing else turns the parts into a word.
/// </summary>
[EncodingOwner("snapshot hitbox-enable word")]
public static class MotherBrainHitboxPartsCodec
{
    /// <summary>Each part's bit in the hitbox-enable word.</summary>
    private static readonly Dictionary<MotherBrainHitboxParts, int> PartBits = new()
    {
        [MotherBrainHitboxParts.Body] = 1 << 0,
        [MotherBrainHitboxParts.Brain] = 1 << 1,
        [MotherBrainHitboxParts.Neck] = 1 << 2,
    };

    /// <summary>The hitbox-enable word holding exactly <paramref name="parts"/>.</summary>
    public static ushort ToNativeWord(this MotherBrainHitboxParts parts)
    {
        int word = 0;
        MotherBrainHitboxParts encoded = MotherBrainHitboxParts.None;
        foreach ((MotherBrainHitboxParts part, int bit) in PartBits)
        {
            if ((parts & part) == MotherBrainHitboxParts.None)
                continue;
            word |= bit;
            encoded |= part;
        }
        return encoded == parts
            ? checked((ushort)word)
            : throw new ArgumentOutOfRangeException(nameof(parts), parts, "Mother Brain has only body, brain and neck hitboxes.");
    }

    /// <summary>Decodes a hitbox-enable word, rejecting any bit outside body, brain and neck.</summary>
    public static MotherBrainHitboxParts FromNativeWord(ushort word)
    {
        MotherBrainHitboxParts parts = MotherBrainHitboxParts.None;
        foreach ((MotherBrainHitboxParts part, int bit) in PartBits)
            if ((word & bit) != 0)
                parts |= part;
        return parts.ToNativeWord() == word
            ? parts
            : throw new InvalidDataException($"${word:X4} is not a Mother Brain hitbox-enable word.");
    }
}
