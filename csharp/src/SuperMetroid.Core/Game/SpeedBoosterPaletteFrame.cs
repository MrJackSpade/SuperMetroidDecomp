using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Game;

/// <summary>Samus's pose, equipment and submersion as the speed-booster palette reads them, and whether another special palette owns her colours this frame (#633).</summary>
public readonly record struct SpeedBoosterPaletteFrame
{
    /// <summary>Movement type.</summary>
    public required SamusMovementType MovementType { get; init; }

    /// <summary>Animation frame.</summary>
    public required ushort AnimationFrame { get; init; }

    /// <summary>Equipped items.</summary>
    public required ushort EquippedItems { get; init; }

    /// <summary>Suppress active speed booster palette.</summary>
    public bool SuppressActiveSpeedBoosterPalette { get; init; }

    /// <summary>Bottom boundary submerged.</summary>
    public bool BottomBoundarySubmerged { get; init; }
}
