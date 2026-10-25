namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The codec between <see cref="LevelBlockFlipFlags"/> and the reflection bits of a native <c>level_data</c> word. It
/// is the one place the flags become those bits; the flags' values are the bits themselves, so the conversion is
/// lossless.
/// </summary>
[EncodingOwner("level data block flip bits")]
internal static class LevelBlockFlipFlagsCodec
{
    /// <summary>The level-word bits that store <paramref name="flags"/>.</summary>
    internal static ushort ToLevelWordBits(this LevelBlockFlipFlags flags) => (ushort)flags;
}
