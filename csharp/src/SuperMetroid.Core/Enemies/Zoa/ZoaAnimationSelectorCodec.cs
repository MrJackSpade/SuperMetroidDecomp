namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>
/// The codec between <see cref="ZoaAnimationSelector"/> and the Zoa slot words that hold it (the requested and
/// previously installed animation selections). Those slot words stay exact because shared enemy code reads slot memory
/// by offset; this type is the one place the selector becomes a slot word.
/// </summary>
[SnesEncodingOwner]
internal static class ZoaAnimationSelectorCodec
{
    /// <summary>The animation selector as the slot word that stores it.</summary>
    internal static ushort ToNativeWord(this ZoaAnimationSelector selector) => (ushort)selector;
}
