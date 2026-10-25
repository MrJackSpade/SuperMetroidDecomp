namespace SuperMetroid.Core.Samus;

/// <summary>Native transitions and the stored-word boundary of <see cref="HudItem"/>.</summary>
public static class HudItems
{
    /// <summary>The next entry of the <c>$90:C51D</c> selection ring, wrapping after X-ray.</summary>
    public static HudItem Next(HudItem item) => EnumOrder.NextWrapping(item);

    /// <summary>A stored word as a HUD item; any other word is corrupt.</summary>
    public static HudItem Decode(ushort word) => word switch
    {
        0 => HudItem.Beam,
        1 => HudItem.Missile,
        2 => HudItem.SuperMissile,
        3 => HudItem.PowerBomb,
        4 => HudItem.Grapple,
        5 => HudItem.XRay,
        _ => throw new InvalidDataException($"HUD item word ${word:X4} is outside the six-entry dispatcher."),
    };

    /// <summary>The stored word of a HUD item: its position in the selection ring.</summary>
    public static ushort ToNativeWord(this HudItem item) => item switch
    {
        HudItem.Beam => 0,
        HudItem.Missile => 1,
        HudItem.SuperMissile => 2,
        HudItem.PowerBomb => 3,
        HudItem.Grapple => 4,
        HudItem.XRay => 5,
        _ => throw new ArgumentOutOfRangeException(nameof(item), item, "Undefined HUD item."),
    };
}
