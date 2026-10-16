namespace SuperMetroid.Core.Samus;

/// <summary>Native transitions and the stored-word boundary of <see cref="HudItem"/>.</summary>
public static class HudItems
{
    /// <summary>The next entry of the <c>$90:C51D</c> selection ring, wrapping after X-ray.</summary>
    public static HudItem Next(HudItem item) => item switch
    {
        HudItem.XRay => HudItem.Beam,
        >= HudItem.Beam and < HudItem.XRay => item + 1,
        _ => throw new InvalidDataException($"HUD item {(ushort)item} is outside the six-entry dispatcher."),
    };

    /// <summary>A stored word as a HUD item; any other word is corrupt.</summary>
    public static HudItem Decode(ushort word) => Enum.IsDefined((HudItem)word)
        ? (HudItem)word
        : throw new InvalidDataException($"HUD item word ${word:X4} is outside the six-entry dispatcher.");

    /// <summary>The stored word of a HUD item.</summary>
    public static ushort ToNativeWord(this HudItem item) => (ushort)item;
}
