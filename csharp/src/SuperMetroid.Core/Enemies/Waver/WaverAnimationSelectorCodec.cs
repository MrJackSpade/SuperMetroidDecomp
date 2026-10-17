namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>
/// The codec between <see cref="WaverAnimationSelector"/> and the Waver slot words that hold it (the current and
/// requested animation selections). Those slot words stay exact because shared enemy code reads slot memory by offset;
/// this type is the one place the selector becomes a slot word.
/// </summary>
[SnesEncodingOwner]
internal static class WaverAnimationSelectorCodec
{
    /// <summary>The animation selector as the slot word that stores it.</summary>
    internal static ushort ToNativeWord(this WaverAnimationSelector selector) => (ushort)selector;
}
