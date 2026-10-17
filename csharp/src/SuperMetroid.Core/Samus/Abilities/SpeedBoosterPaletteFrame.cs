using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Samus's pose, equipment and submersion as the speed-booster palette reads them, and whether another special palette owns her colours this frame (#633).</summary>
public readonly record struct SpeedBoosterPaletteFrame
{
    /// <summary>Movement type.</summary>
    public required SamusMovementType MovementType { get; init; }

    /// <summary>
    /// Samus's current animation frame index, copied from her animation state; the palette compares it with zero,
    /// three and twenty-seven.
    /// </summary>
    public required int AnimationFrame { get; init; }

    /// <summary>Equipped items.</summary>
    public required SuperMetroid.Core.Samus.SamusEquipmentFlags EquippedItems { get; init; }

    /// <summary>Suppress active speed booster palette.</summary>
    public bool SuppressActiveSpeedBoosterPalette { get; init; }

    /// <summary>Bottom boundary submerged.</summary>
    public bool BottomBoundarySubmerged { get; init; }
}
