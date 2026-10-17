namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The single conversion between <see cref="MotherBrainHitboxParts"/> and the shared hitbox-enable word a native
/// snapshot stores, whose three low bits are body, brain and neck. The snapshot import decodes through
/// <see cref="FromNativeWord"/>, rejecting a word with any other bit set, and word-level checks encode through
/// <see cref="ToNativeWord"/>; nothing else turns the parts into a word.
/// </summary>
[SnesEncodingOwner]
public static class MotherBrainHitboxPartsCodec
{
    /// <summary>Every part the word can enable.</summary>
    private const MotherBrainHitboxParts AllParts =
        MotherBrainHitboxParts.Body | MotherBrainHitboxParts.Brain | MotherBrainHitboxParts.Neck;

    /// <summary>The hitbox-enable word holding exactly <paramref name="parts"/>.</summary>
    public static ushort ToNativeWord(this MotherBrainHitboxParts parts) =>
        (parts & ~AllParts) == 0
            ? (ushort)parts
            : throw new ArgumentOutOfRangeException(nameof(parts), parts, "Mother Brain has only body, brain and neck hitboxes.");

    /// <summary>Decodes a hitbox-enable word, rejecting any bit outside body, brain and neck.</summary>
    public static MotherBrainHitboxParts FromNativeWord(ushort word)
    {
        var parts = (MotherBrainHitboxParts)word;
        return (parts & ~AllParts) == 0
            ? parts
            : throw new InvalidDataException($"${word:X4} is not a Mother Brain hitbox-enable word.");
    }
}
