using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Samus's pose, equipment and submersion as the Speed Booster glow reads them, and whether another special colouring owns her colours this frame (#633).</summary>
public readonly record struct SpeedBoosterGlowFrame
{
    /// <summary>Movement type.</summary>
    public required SamusMovementType MovementType { get; init; }

    /// <summary>Whether her animation is on the Screw Attack spin's first frame, which keeps her normal colours and restarts the glow's cycle.</summary>
    public required bool AnimationOnSpinStart { get; init; }

    /// <summary>Whether her animation has reached the Screw Attack spin's last segment, where she shows her normal colours.</summary>
    public required bool AnimationPastScrewAttackSpin { get; init; }

    /// <summary>Whether her animation is still before the wall-jump spin, which keeps her normal colours.</summary>
    public required bool AnimationBeforeWallJumpSpin { get; init; }

    /// <summary>Equipped items.</summary>
    public required SuperMetroid.Core.Progression.SamusEquipmentFlags EquippedItems { get; init; }

    /// <summary>Whether another special colouring owns Samus's colours, which keeps the active Speed Booster glow off.</summary>
    public bool SuppressActiveSpeedBoosterGlow { get; init; }

    /// <summary>Bottom boundary submerged.</summary>
    public bool BottomBoundarySubmerged { get; init; }
}
