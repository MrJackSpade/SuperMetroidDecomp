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
    private const short BodyHalfWidth = 33;
    /// <summary>Half width of the vulnerable eye's central collision rectangle.</summary>
    private const short EyeHalfWidth = 9;
    /// <summary>Half width of the centered tentacle collision rectangle.</summary>
    private const short CenterTentacleHalfWidth = 12;
    /// <summary>Near horizontal edge of each mirrored side tentacle rectangle.</summary>
    private const short SideTentacleInnerExtent = 16;
    /// <summary>Far horizontal edge of each mirrored side tentacle rectangle.</summary>
    private const short SideTentacleOuterExtent = 23;
    /// <summary>Position of the vulnerable eye among the full body's rectangles.</summary>
    private const int EyeRectangle = 1;
    /// <summary>
    /// Native vertical extents of the body, eye, and three tentacle hitboxes. Declared before the lists below, which
    /// read it during static initialization.
    /// </summary>
    private static readonly PhantoonVerticalExtent[] RequiredVerticalBounds =
        [new PhantoonVerticalExtent(-40, 56), new PhantoonVerticalExtent(22, 39), new PhantoonVerticalExtent(52, 71), new PhantoonVerticalExtent(53, 70), new PhantoonVerticalExtent(53, 69)];

    /// <summary>The single no-reaction point carried by hidden parts and by both tentacle components.</summary>
    private static readonly PhantoonCollisionHitbox[] PointList =
        [new(0, 0, 0, 0, PhantoonHitboxTouch.Inert, PhantoonHitboxShot.Inert)];
    /// <summary>The visible body: body, eye, left, right and center tentacle rectangles, tested in that order.</summary>
    private static readonly PhantoonCollisionHitbox[] FullBodyList =
        [BodyHitbox(0), BodyHitbox(EyeRectangle), BodyHitbox(2), BodyHitbox(3), BodyHitbox(4)];
    /// <summary>The vulnerable eye alone, repeating the full body's eye rectangle.</summary>
    private static readonly PhantoonCollisionHitbox[] EyeOnlyList = [BodyHitbox(EyeRectangle)];

    /// <summary>The one inert point component most frames carry.</summary>
    private static readonly ImmutableArray<PhantoonCollisionComponent> PointComponents = [new(Offset16<RoomPixels>.Zero, PointList)];
    /// <summary>The full body's one component.</summary>
    private static readonly ImmutableArray<PhantoonCollisionComponent> FullBodyComponents = [new(Offset16<RoomPixels>.Zero, FullBodyList)];
    /// <summary>The eye-only body's one component.</summary>
    private static readonly ImmutableArray<PhantoonCollisionComponent> EyeOnlyComponents = [new(Offset16<RoomPixels>.Zero, EyeOnlyList)];
    /// <summary>A tentacle frame's two inert point components.</summary>
    private static readonly ImmutableArray<PhantoonCollisionComponent> TentacleComponents =
        [new(Offset16<RoomPixels>.Zero, PointList), new(Offset16<RoomPixels>.Zero, PointList)];

    /// <summary>
    /// One of the body's rectangles. Centered intervals have left=-right-1; side tentacle intervals reflect exactly
    /// around that same pixel-centered axis, while their chosen vertical bounds remain independent.
    /// </summary>
    private static PhantoonCollisionHitbox BodyHitbox(int index)
    {
        var vertical = RequiredVerticalBounds[index];
        PhantoonHorizontalExtent horizontal = index switch
        {
            0 => Centered(BodyHalfWidth),
            EyeRectangle => Centered(EyeHalfWidth),
            2 => new PhantoonHorizontalExtent(-SideTentacleOuterExtent, -SideTentacleInnerExtent),
            3 => new PhantoonHorizontalExtent(SideTentacleInnerExtent - 1, SideTentacleOuterExtent - 1),
            4 => Centered(CenterTentacleHalfWidth),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        return new(horizontal.Left, vertical.Top, horizontal.Right, vertical.Bottom, PhantoonHitboxTouch.Contact, PhantoonHitboxShot.Vulnerable);

        static PhantoonHorizontalExtent Centered(int halfWidth) => new PhantoonHorizontalExtent(-halfWidth, halfWidth - 1);
    }
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
