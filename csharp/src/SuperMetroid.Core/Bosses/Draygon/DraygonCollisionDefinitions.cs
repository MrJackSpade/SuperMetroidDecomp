using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Owned collision for Draygon's body-layer and ordinary sprite frames. The
/// pictures these frames show are independently replaceable;
/// editing their artwork cannot move a hitbox or change a reaction.
/// </summary>
internal static partial class DraygonCollisionDefinitions
{
    /// <summary>Left-facing body rectangles: one shootable eye and three touch-only regions.</summary>
    internal static readonly EnemyCollisionHitbox[] FacingLeftBodyHitboxes =
    [
        new(-17, -16, 18, 30, EnemyReaction.Ignore, DraygonReactions.Shot),
        new(-59, -66, 3, -30, DraygonReactions.Touch, EnemyReaction.DudShot),
        new(16, 1, 60, 53, DraygonReactions.Touch, EnemyReaction.DudShot),
        new(-28, -37, 61, -10, DraygonReactions.Touch, EnemyReaction.DudShot),
    ];

    /// <summary>Mirrored right-facing body rectangles with the same shootable-eye rule.</summary>
    internal static readonly EnemyCollisionHitbox[] FacingRightBodyHitboxes =
    [
        new(-18, -7, 18, 30, EnemyReaction.Ignore, DraygonReactions.Shot),
        new(-10, -63, 62, -26, DraygonReactions.Touch, EnemyReaction.DudShot),
        new(-55, 0, -21, 57, DraygonReactions.Touch, EnemyReaction.DudShot),
        new(-62, -33, 15, -5, DraygonReactions.Touch, EnemyReaction.DudShot),
    ];

    /// <summary>A frame that cannot be hit or touched has no collision components.</summary>
    internal static readonly ImmutableArray<EnemyCollisionComponent> NoComponents = [];
    /// <summary>The left-facing body's one zero-offset component.</summary>
    internal static readonly ImmutableArray<EnemyCollisionComponent> FacingLeftBodyComponents = [new(Offset16<RoomPixels>.Zero, FacingLeftBodyHitboxes)];
    /// <summary>The right-facing body's one zero-offset component.</summary>
    internal static readonly ImmutableArray<EnemyCollisionComponent> FacingRightBodyComponents = [new(Offset16<RoomPixels>.Zero, FacingRightBodyHitboxes)];

    /// <summary>
    /// Each Draygon frame's collision components (#1937). His body's four jaw frames and his limp arm's four claw frames
    /// have that facing's one zero-offset body component; his arms, eye and tail cannot be hit or touched and have none.
    /// </summary>
    internal static readonly Dictionary<EnemyFrameId, ImmutableArray<EnemyCollisionComponent>> FrameComponents = new()
    {
        [EnemyFrameId.DraygonArmsFacingLeftForward] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingLeftLowering] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingLeftDown] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingLeftDownStretched] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingLeftRaising] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingLeftReaching] = NoComponents,
        [EnemyFrameId.DraygonLimpArmFacingLeftClawRaised] = FacingLeftBodyComponents,
        [EnemyFrameId.DraygonLimpArmFacingLeftClawDropping] = FacingLeftBodyComponents,
        [EnemyFrameId.DraygonLimpArmFacingLeftClawDropped] = FacingLeftBodyComponents,
        [EnemyFrameId.DraygonLimpArmFacingLeftClawCurled] = FacingLeftBodyComponents,
        [EnemyFrameId.DraygonBodyFacingLeftJawShut] = FacingLeftBodyComponents,
        [EnemyFrameId.DraygonBodyFacingLeftJawParting] = FacingLeftBodyComponents,
        [EnemyFrameId.DraygonBodyFacingLeftJawOpen] = FacingLeftBodyComponents,
        [EnemyFrameId.DraygonBodyFacingLeftJawWide] = FacingLeftBodyComponents,
        [EnemyFrameId.DraygonEyeFacingLeftShut] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingLeftOpening] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingLeftHalfOpen] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingLeftWide] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingLeftOpen] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingLeftBloodshot] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingLeftNarrowing] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingLeftClosing] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingLeftSpreadWide] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingLeftClosing] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingLeftGathering] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingLeftTucked] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftTipCurledIn] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftTipSwingingOut] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftTipOut] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftTipFurtherOut] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftTipFarthestOut] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftTipReturning] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftTipBack] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftWhipCocking] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftWhipRaising] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftWhipHangingLong] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftWhipSwinging] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftWhipLashedOut] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftWhipCurling] = NoComponents,
        [EnemyFrameId.DraygonTailFacingLeftWhipRecoiling] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingRightForward] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingRightLowering] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingRightDown] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingRightDownStretched] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingRightRaising] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingRightReaching] = NoComponents,
        [EnemyFrameId.DraygonLimpArmFacingRightClawRaised] = FacingRightBodyComponents,
        [EnemyFrameId.DraygonLimpArmFacingRightClawDropping] = FacingRightBodyComponents,
        [EnemyFrameId.DraygonLimpArmFacingRightClawDropped] = FacingRightBodyComponents,
        [EnemyFrameId.DraygonLimpArmFacingRightClawCurled] = FacingRightBodyComponents,
        [EnemyFrameId.DraygonBodyFacingRightJawShut] = FacingRightBodyComponents,
        [EnemyFrameId.DraygonBodyFacingRightJawParting] = FacingRightBodyComponents,
        [EnemyFrameId.DraygonBodyFacingRightJawOpen] = FacingRightBodyComponents,
        [EnemyFrameId.DraygonBodyFacingRightJawWide] = FacingRightBodyComponents,
        [EnemyFrameId.DraygonEyeFacingRightShut] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingRightOpening] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingRightHalfOpen] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingRightWide] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingRightOpen] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingRightBloodshot] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingRightNarrowing] = NoComponents,
        [EnemyFrameId.DraygonEyeFacingRightClosing] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingRightSpreadWide] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingRightClosing] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingRightGathering] = NoComponents,
        [EnemyFrameId.DraygonArmsFacingRightTucked] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightTipCurledIn] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightTipSwingingOut] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightTipOut] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightTipFurtherOut] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightTipFarthestOut] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightTipReturning] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightTipBack] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightWhipCocking] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightWhipRaising] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightWhipHangingLong] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightWhipSwinging] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightWhipLashedOut] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightWhipCurling] = NoComponents,
        [EnemyFrameId.DraygonTailFacingRightWhipRecoiling] = NoComponents,
    };
}
