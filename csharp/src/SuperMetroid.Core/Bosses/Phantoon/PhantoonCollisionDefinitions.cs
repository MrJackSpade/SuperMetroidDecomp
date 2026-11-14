using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's three fixed hitbox lists, separate from its editable background frames: the inert point carried by hidden
/// parts and tentacles, the five-rectangle full body, and the vulnerable eye alone. Every frame root has zero
/// component offsets; the three tentacle roots each have two components.
/// </summary>
internal static class PhantoonCollisionDefinitions
{

    /// <summary>Half width of the full body's central collision rectangle.</summary>
    internal const int BodyHalfWidth = 33;
    /// <summary>Half width of the vulnerable eye's central collision rectangle.</summary>
    internal const int EyeHalfWidth = 9;
    /// <summary>Half width of the centered tentacle collision rectangle.</summary>
    internal const int CenterTentacleHalfWidth = 12;
    /// <summary>Near horizontal edge of each mirrored side tentacle rectangle.</summary>
    internal const int SideTentacleInnerExtent = 16;
    /// <summary>Far horizontal edge of each mirrored side tentacle rectangle.</summary>
    internal const int SideTentacleOuterExtent = 23;

    /// <summary>The full body's central rectangle.</summary>
    internal static readonly EnemyCollisionHitbox Body = Centered(BodyHalfWidth, top: -40, bottom: 56);
    /// <summary>The vulnerable eye's rectangle.</summary>
    internal static readonly EnemyCollisionHitbox Eye = Centered(EyeHalfWidth, top: 22, bottom: 39);
    /// <summary>The left tentacle's rectangle.</summary>
    internal static readonly EnemyCollisionHitbox LeftTentacle =
        Contact(-SideTentacleOuterExtent, top: 52, -SideTentacleInnerExtent, bottom: 71);
    /// <summary>The right tentacle's rectangle, the left one reflected about the centered axis.</summary>
    internal static readonly EnemyCollisionHitbox RightTentacle =
        Contact(SideTentacleInnerExtent - 1, top: 53, SideTentacleOuterExtent - 1, bottom: 70);
    /// <summary>The center tentacle's rectangle.</summary>
    internal static readonly EnemyCollisionHitbox CenterTentacle = Centered(CenterTentacleHalfWidth, top: 53, bottom: 69);

    /// <summary>The single no-reaction point carried by hidden parts and by both tentacle components.</summary>
    internal static readonly EnemyCollisionHitbox[] PointList =
        [new(0, 0, 0, 0, EnemyReaction.Ignore, EnemyReaction.Ignore)];
    /// <summary>The visible body: body, eye, left, right and center tentacle rectangles, tested in that order.</summary>
    internal static readonly EnemyCollisionHitbox[] FullBodyList =
        [Body, Eye, LeftTentacle, RightTentacle, CenterTentacle];
    /// <summary>The vulnerable eye alone, repeating the full body's eye rectangle.</summary>
    internal static readonly EnemyCollisionHitbox[] EyeOnlyList = [Eye];

    /// <summary>The one inert point component most frames carry.</summary>
    internal static readonly ImmutableArray<EnemyCollisionComponent> PointComponents = [new(Offset16<RoomPixels>.Zero, PointList)];
    /// <summary>The full body's one component.</summary>
    internal static readonly ImmutableArray<EnemyCollisionComponent> FullBodyComponents = [new(Offset16<RoomPixels>.Zero, FullBodyList)];
    /// <summary>The eye-only body's one component.</summary>
    internal static readonly ImmutableArray<EnemyCollisionComponent> EyeOnlyComponents = [new(Offset16<RoomPixels>.Zero, EyeOnlyList)];
    /// <summary>A tentacle frame's two inert point components.</summary>
    internal static readonly ImmutableArray<EnemyCollisionComponent> TentacleComponents =
        [new(Offset16<RoomPixels>.Zero, PointList), new(Offset16<RoomPixels>.Zero, PointList)];

    /// <summary>A body rectangle that Samus touches and shoots.</summary>
    private static EnemyCollisionHitbox Contact(int left, int top, int right, int bottom) =>
        new(left, top, right, bottom, PhantoonReactions.Touch, PhantoonReactions.Shot);

    /// <summary>
    /// A body rectangle centered on the component's pixel-centered axis: its left edge is one pixel beyond the negated
    /// right edge. The side tentacles reflect exactly around the same axis; each rectangle's vertical bounds are
    /// independent.
    /// </summary>
    private static EnemyCollisionHitbox Centered(int halfWidth, int top, int bottom) =>
        Contact(-halfWidth, top, halfWidth - 1, bottom);
    /// <summary>
    /// Each Phantoon background frame's components: the full body and the eye alone each have one component, the three
    /// tentacle frames two inert points, and every other frame one inert point.
    /// </summary>
    internal static readonly Dictionary<EnemyFrameId, ImmutableArray<EnemyCollisionComponent>> FrameComponents = new()
    {
        [EnemyFrameId.PhantoonInvulnerableBody] = PointComponents,
        [EnemyFrameId.PhantoonFullHitboxBody] = FullBodyComponents,
        [EnemyFrameId.PhantoonEyeHitboxBody] = EyeOnlyComponents,
        [EnemyFrameId.PhantoonEyeClosed] = PointComponents,
        [EnemyFrameId.PhantoonEyeOpenFirst] = PointComponents,
        [EnemyFrameId.PhantoonEyeOpenSecond] = PointComponents,
        [EnemyFrameId.PhantoonEyeOpenThird] = PointComponents,
        [EnemyFrameId.PhantoonEyeballCentered] = PointComponents,
        [EnemyFrameId.PhantoonEyeLookingUp] = PointComponents,
        [EnemyFrameId.PhantoonEyeLookingDown] = PointComponents,
        [EnemyFrameId.PhantoonEyeLookingLeft] = PointComponents,
        [EnemyFrameId.PhantoonEyeLookingRight] = PointComponents,
        [EnemyFrameId.PhantoonEyeLookingDownLeft] = PointComponents,
        [EnemyFrameId.PhantoonEyeLookingDownRight] = PointComponents,
        [EnemyFrameId.PhantoonEyeLookingUpLeft] = PointComponents,
        [EnemyFrameId.PhantoonEyeLookingUpRight] = PointComponents,
        [EnemyFrameId.PhantoonTentaclesFirst] = TentacleComponents,
        [EnemyFrameId.PhantoonTentaclesSecond] = TentacleComponents,
        [EnemyFrameId.PhantoonTentaclesThird] = TentacleComponents,
        [EnemyFrameId.PhantoonMouthInitial] = PointComponents,
        [EnemyFrameId.PhantoonMouthSpawnFlameSecond] = PointComponents,
        [EnemyFrameId.PhantoonMouthSpawnFlameFirst] = PointComponents,
    };
}
