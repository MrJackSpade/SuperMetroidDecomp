using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's three fixed hitbox lists, separate from its editable BG2 frames: the inert point carried by hidden
/// parts and tentacles, the five-rectangle full body, and the vulnerable eye alone. Every frame root has zero
/// component offsets; the three tentacle roots each have two components.
/// </summary>
internal static class PhantoonCollisionDefinitions
{

    /// <summary>Half width of the full body's central collision rectangle.</summary>
    private const int BodyHalfWidth = 33;
    /// <summary>Half width of the vulnerable eye's central collision rectangle.</summary>
    private const int EyeHalfWidth = 9;
    /// <summary>Half width of the centered tentacle collision rectangle.</summary>
    private const int CenterTentacleHalfWidth = 12;
    /// <summary>Near horizontal edge of each mirrored side tentacle rectangle.</summary>
    private const int SideTentacleInnerExtent = 16;
    /// <summary>Far horizontal edge of each mirrored side tentacle rectangle.</summary>
    private const int SideTentacleOuterExtent = 23;

    // Centered intervals have left = -right - 1; the side tentacles reflect exactly around that same pixel-centered
    // axis, while each rectangle's vertical bounds are independent. Declared before the lists below, which read them
    // during static initialization.

    /// <summary>The full body's central rectangle.</summary>
    private static readonly PhantoonCollisionHitbox Body = Centered(BodyHalfWidth, top: -40, bottom: 56);
    /// <summary>The vulnerable eye's rectangle.</summary>
    private static readonly PhantoonCollisionHitbox Eye = Centered(EyeHalfWidth, top: 22, bottom: 39);
    /// <summary>The left tentacle's rectangle.</summary>
    private static readonly PhantoonCollisionHitbox LeftTentacle =
        Contact(-SideTentacleOuterExtent, top: 52, -SideTentacleInnerExtent, bottom: 71);
    /// <summary>The right tentacle's rectangle, the left one reflected about the centered axis.</summary>
    private static readonly PhantoonCollisionHitbox RightTentacle =
        Contact(SideTentacleInnerExtent - 1, top: 53, SideTentacleOuterExtent - 1, bottom: 70);
    /// <summary>The center tentacle's rectangle.</summary>
    private static readonly PhantoonCollisionHitbox CenterTentacle = Centered(CenterTentacleHalfWidth, top: 53, bottom: 69);

    /// <summary>The single no-reaction point carried by hidden parts and by both tentacle components.</summary>
    private static readonly PhantoonCollisionHitbox[] PointList =
        [new(0, 0, 0, 0, PhantoonHitboxTouch.Inert, PhantoonHitboxShot.Inert)];
    /// <summary>The visible body: body, eye, left, right and center tentacle rectangles, tested in that order.</summary>
    private static readonly PhantoonCollisionHitbox[] FullBodyList =
        [Body, Eye, LeftTentacle, RightTentacle, CenterTentacle];
    /// <summary>The vulnerable eye alone, repeating the full body's eye rectangle.</summary>
    private static readonly PhantoonCollisionHitbox[] EyeOnlyList = [Eye];

    /// <summary>The one inert point component most frames carry.</summary>
    private static readonly ImmutableArray<PhantoonCollisionComponent> PointComponents = [new(Offset16<RoomPixels>.Zero, PointList)];
    /// <summary>The full body's one component.</summary>
    private static readonly ImmutableArray<PhantoonCollisionComponent> FullBodyComponents = [new(Offset16<RoomPixels>.Zero, FullBodyList)];
    /// <summary>The eye-only body's one component.</summary>
    private static readonly ImmutableArray<PhantoonCollisionComponent> EyeOnlyComponents = [new(Offset16<RoomPixels>.Zero, EyeOnlyList)];
    /// <summary>A tentacle frame's two inert point components.</summary>
    private static readonly ImmutableArray<PhantoonCollisionComponent> TentacleComponents =
        [new(Offset16<RoomPixels>.Zero, PointList), new(Offset16<RoomPixels>.Zero, PointList)];

    /// <summary>A body rectangle that Samus touches and shoots.</summary>
    private static PhantoonCollisionHitbox Contact(int left, int top, int right, int bottom) =>
        new(left, top, right, bottom, PhantoonHitboxTouch.Contact, PhantoonHitboxShot.Vulnerable);

    /// <summary>A body rectangle centered on the component's pixel-centered axis.</summary>
    private static PhantoonCollisionHitbox Centered(int halfWidth, int top, int bottom) =>
        Contact(-halfWidth, top, halfWidth - 1, bottom);
    /// <summary>
    /// Each Phantoon BG2 frame's components: the full body and the eye alone each have one component, the three
    /// tentacle frames two inert points, and every other frame one inert point.
    /// </summary>
    private static readonly Dictionary<EnemyFrameId, ImmutableArray<PhantoonCollisionComponent>> FrameComponents = new()
    {
        [EnemyFrameId.PhantoonInvulnerableBody0] = PointComponents,
        [EnemyFrameId.PhantoonFullHitboxBody0] = FullBodyComponents,
        [EnemyFrameId.PhantoonEyeHitboxBody0] = EyeOnlyComponents,
        [EnemyFrameId.PhantoonClosedEye0] = PointComponents,
        [EnemyFrameId.PhantoonOpeningEye0] = PointComponents,
        [EnemyFrameId.PhantoonOpeningEye1] = PointComponents,
        [EnemyFrameId.PhantoonOpeningEye2] = PointComponents,
        [EnemyFrameId.PhantoonCenteredEye0] = PointComponents,
        [EnemyFrameId.PhantoonGaze0] = PointComponents,
        [EnemyFrameId.PhantoonGaze4] = PointComponents,
        [EnemyFrameId.PhantoonGaze6] = PointComponents,
        [EnemyFrameId.PhantoonGaze2] = PointComponents,
        [EnemyFrameId.PhantoonGaze5] = PointComponents,
        [EnemyFrameId.PhantoonGaze3] = PointComponents,
        [EnemyFrameId.PhantoonGaze7] = PointComponents,
        [EnemyFrameId.PhantoonGaze1] = PointComponents,
        [EnemyFrameId.PhantoonTentacle0] = TentacleComponents,
        [EnemyFrameId.PhantoonTentacle1] = TentacleComponents,
        [EnemyFrameId.PhantoonTentacle2] = TentacleComponents,
        [EnemyFrameId.PhantoonMouth0] = PointComponents,
        [EnemyFrameId.PhantoonMouth1] = PointComponents,
        [EnemyFrameId.PhantoonMouth2] = PointComponents,
    };

    /// <summary>The collision components of Phantoon frame <paramref name="frame"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not one of Phantoon's BG2 frames.</exception>
    internal static ImmutableArray<PhantoonCollisionComponent> ComponentsAt(EnemyFrameId? frame) =>
        frame is { } shown && FrameComponents.TryGetValue(shown, out ImmutableArray<PhantoonCollisionComponent> components)
            ? components
            : throw new InvalidDataException($"Phantoon frame {frame} has no compiled hitbox identity.");

}
