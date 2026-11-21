using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Samus's pose, equipment and submersion as the Speed Booster glow reads them, and whether another special colouring owns her colours this frame (#633).</summary>
public readonly record struct SpeedBoosterGlowFrame
{
    /// <summary>How Samus is moving, which decides whether her glow shows.</summary>
    public required SamusMovementType MovementType { get; init; }

    /// <summary>Where her spin's animation stands for the glow (#2560), which <see cref="SpinPlaceOf"/> reads.</summary>
    public required SpinGlowPlace SpinPlace { get; init; }

    /// <summary>The items Samus has equipped, whose Speed Booster the glow belongs to.</summary>
    public required SamusEquipmentFlags EquippedItems { get; init; }

    /// <summary>Whether another special colouring owns Samus's colours, which keeps the active Speed Booster glow off.</summary>
    public required bool SuppressActiveSpeedBoosterGlow { get; init; }

    /// <summary>Whether Samus's feet are under the liquid's surface.</summary>
    public required bool BottomBoundarySubmerged { get; init; }

    /// <summary>
    /// Where <paramref name="samus"/>'s spin animation stands for the glow while she moves as <paramref name="movementType"/>:
    /// a spin jump restarts the glow on the Screw Attack spin's first frame and shows her normal colours from its first
    /// wall-jump-ready frame on; a wall jump restarts it until its spin starts; any other movement spins no glow.
    /// </summary>
    internal static SpinGlowPlace SpinPlaceOf(SamusState samus, SamusMovementType movementType)
    {
        Ensure.NotNull(samus);
        if (movementType == SamusMovementType.SpinJumping)
        {
            return samus.AnimationIsOn(SamusAnimationFrames.ScrewAttackSpinStart) ? SpinGlowPlace.Restarting
                : samus.AnimationHasReached(SamusAnimationFrames.ScrewAttackWallJumpReady) ? SpinGlowPlace.ShowsNormalColours
                : SpinGlowPlace.Glowing;
        }
        if (movementType == SamusMovementType.WallJumping)
        {
            return samus.AnimationHasReached(SamusAnimationFrames.WallJumpOrdinarySpinStart)
                ? SpinGlowPlace.Glowing
                : SpinGlowPlace.Restarting;
        }
        return SpinGlowPlace.NotSpinning;
    }
}
