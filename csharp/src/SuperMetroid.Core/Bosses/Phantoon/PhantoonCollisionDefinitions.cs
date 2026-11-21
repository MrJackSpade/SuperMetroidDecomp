using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's fixed hitboxes, separate from his editable body pictures (#1937): the five-rectangle full body, the
/// vulnerable eye alone, and the invulnerable body's centre point (<see cref="PhantoonSolidifyingCentre"/>). Every
/// component lies at his origin. The eye's, tentacles' and mouth's looks have no hitboxes: those parts always ignore
/// Samus.
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

    /// <summary>The visible body: body, eye, left, right and center tentacle rectangles, tested in that order.</summary>
    internal static readonly EnemyCollisionHitbox[] FullBodyList =
        [Body, Eye, LeftTentacle, RightTentacle, CenterTentacle];
    /// <summary>The vulnerable eye alone, repeating the full body's eye rectangle.</summary>
    internal static readonly EnemyCollisionHitbox[] EyeOnlyList = [Eye];

    /// <summary>The invulnerable body's one component: the centre point that absorbs a shot or a bomb as it turns solid.</summary>
    internal static readonly ImmutableArray<EnemyCollisionComponent> SolidifyingCentreComponents =
        [new(Offset16<RoomPixels>.Zero, [new(PhantoonSolidifyingCentre.Centre.X, PhantoonSolidifyingCentre.Centre.Y,
            PhantoonSolidifyingCentre.Centre.X, PhantoonSolidifyingCentre.Centre.Y, EnemyReaction.Ignore, EnemyReaction.Ignore)])];
    /// <summary>The components of a part's look that carries no hitbox.</summary>
    internal static readonly ImmutableArray<EnemyCollisionComponent> NoComponents = [];
    /// <summary>The full body's one component.</summary>
    internal static readonly ImmutableArray<EnemyCollisionComponent> FullBodyComponents = [new(Offset16<RoomPixels>.Zero, FullBodyList)];
    /// <summary>The eye-only body's one component.</summary>
    internal static readonly ImmutableArray<EnemyCollisionComponent> EyeOnlyComponents = [new(Offset16<RoomPixels>.Zero, EyeOnlyList)];

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
    /// Each of Phantoon's body pictures' components: the full body, the eye alone and the invulnerable body each have
    /// one component; the eye's, tentacles' and mouth's looks are <see cref="PartsWithoutHitboxes"/>.
    /// </summary>
    internal static readonly Dictionary<EnemyFrameId, ImmutableArray<EnemyCollisionComponent>> FrameComponents = new()
    {
        [EnemyFrameId.PhantoonInvulnerableBody] = SolidifyingCentreComponents,
        [EnemyFrameId.PhantoonFullHitboxBody] = FullBodyComponents,
        [EnemyFrameId.PhantoonEyeHitboxBody] = EyeOnlyComponents,
    };

    /// <summary>
    /// The looks of his eye, tentacles and mouth, which carry no hitbox: his body's pictures alone hold his components, so
    /// every one of these has none.
    /// </summary>
    internal static readonly IReadOnlySet<EnemyFrameId> PartsWithoutHitboxes = new HashSet<EnemyFrameId>
    {
        EnemyFrameId.PhantoonEyeClosed, EnemyFrameId.PhantoonEyeHalfOpen, EnemyFrameId.PhantoonEyeballCentered,
        EnemyFrameId.PhantoonEyeLookingUp, EnemyFrameId.PhantoonEyeLookingDown, EnemyFrameId.PhantoonEyeLookingLeft,
        EnemyFrameId.PhantoonEyeLookingRight, EnemyFrameId.PhantoonEyeLookingDownLeft, EnemyFrameId.PhantoonEyeLookingDownRight,
        EnemyFrameId.PhantoonEyeLookingUpLeft, EnemyFrameId.PhantoonEyeLookingUpRight,
        EnemyFrameId.PhantoonTentaclesNarrow, EnemyFrameId.PhantoonTentaclesSpreading, EnemyFrameId.PhantoonTentaclesSpread,
        EnemyFrameId.PhantoonMouthOpen, EnemyFrameId.PhantoonMouthHalfOpen, EnemyFrameId.PhantoonMouthNarrow,
    };
}
