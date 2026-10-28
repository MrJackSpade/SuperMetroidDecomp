using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Cartridge-owned collision for Draygon's BG2 and ordinary OAM frames. The
/// visual streams referenced by these frames are independently replaceable;
/// editing their artwork cannot move a hitbox or change a callback.
/// </summary>
internal static class DraygonCollisionDefinitions
{
    /// <summary>Left-facing body rectangles: one shootable eye and three touch-only regions.</summary>
    internal static readonly DraygonCollisionHitbox[] FirstBodyHitboxes =
    [
        new(-17, -16, 18, 30, DraygonHitboxTouch.Harmless, DraygonHitboxShot.Eye),
        new(-59, -66, 3, -30, DraygonHitboxTouch.Body, DraygonHitboxShot.Dud),
        new(16, 1, 60, 53, DraygonHitboxTouch.Body, DraygonHitboxShot.Dud),
        new(-28, -37, 61, -10, DraygonHitboxTouch.Body, DraygonHitboxShot.Dud),
    ];

    /// <summary>Mirrored right-facing body rectangles with the same shootable-eye rule.</summary>
    internal static readonly DraygonCollisionHitbox[] SecondBodyHitboxes =
    [
        new(-18, -7, 18, 30, DraygonHitboxTouch.Harmless, DraygonHitboxShot.Eye),
        new(-10, -63, 62, -26, DraygonHitboxTouch.Body, DraygonHitboxShot.Dud),
        new(-55, 0, -21, 57, DraygonHitboxTouch.Body, DraygonHitboxShot.Dud),
        new(-62, -33, 15, -5, DraygonHitboxTouch.Body, DraygonHitboxShot.Dud),
    ];

    /// <summary>The empty list of the BG2 frames in which the body cannot be hit or touched.</summary>
    internal static readonly DraygonCollisionHitbox[] EmptyHitboxes = [];

    /// <summary>Ordinary OAM art has no collision components.</summary>
    internal static readonly ImmutableArray<DraygonCollisionComponent> NoComponents = [];
    /// <summary>The left-facing body's one zero-offset component.</summary>
    internal static readonly ImmutableArray<DraygonCollisionComponent> FirstBodyComponents = [new(Offset16<RoomPixels>.Zero, FirstBodyHitboxes)];
    /// <summary>The right-facing body's one zero-offset component.</summary>
    internal static readonly ImmutableArray<DraygonCollisionComponent> SecondBodyComponents = [new(Offset16<RoomPixels>.Zero, SecondBodyHitboxes)];
    /// <summary>The one zero-offset component of a BG2 frame in which the body cannot be hit or touched.</summary>
    internal static readonly ImmutableArray<DraygonCollisionComponent> EmptyBodyComponents = [new(Offset16<RoomPixels>.Zero, EmptyHitboxes)];

    /// <summary>
    /// Each Draygon frame's collision components. Every BG2 body frame has one zero-offset component: the first eight and
    /// final frame of each facing use that facing's body hitboxes and the frames between use the empty list. Ordinary OAM
    /// frames have no collision components.
    /// </summary>
    internal static readonly Dictionary<EnemyFrameId, ImmutableArray<DraygonCollisionComponent>> FrameComponents = new()
    {
        [EnemyFrameId.DraygonFrame0] = NoComponents,
        [EnemyFrameId.DraygonFrame1] = NoComponents,
        [EnemyFrameId.DraygonFrame2] = NoComponents,
        [EnemyFrameId.DraygonFrame3] = NoComponents,
        [EnemyFrameId.DraygonFrame4] = NoComponents,
        [EnemyFrameId.DraygonFrame5] = NoComponents,
        [EnemyFrameId.DraygonFrame6] = FirstBodyComponents,
        [EnemyFrameId.DraygonFrame7] = FirstBodyComponents,
        [EnemyFrameId.DraygonFrame8] = FirstBodyComponents,
        [EnemyFrameId.DraygonFrame9] = FirstBodyComponents,
        [EnemyFrameId.DraygonFrame10] = FirstBodyComponents,
        [EnemyFrameId.DraygonFrame11] = FirstBodyComponents,
        [EnemyFrameId.DraygonFrame12] = FirstBodyComponents,
        [EnemyFrameId.DraygonFrame13] = FirstBodyComponents,
        [EnemyFrameId.DraygonFrame14] = EmptyBodyComponents,
        [EnemyFrameId.DraygonFrame15] = EmptyBodyComponents,
        [EnemyFrameId.DraygonFrame16] = EmptyBodyComponents,
        [EnemyFrameId.DraygonFrame17] = EmptyBodyComponents,
        [EnemyFrameId.DraygonFrame18] = EmptyBodyComponents,
        [EnemyFrameId.DraygonFrame19] = EmptyBodyComponents,
        [EnemyFrameId.DraygonFrame20] = EmptyBodyComponents,
        [EnemyFrameId.DraygonFrame21] = EmptyBodyComponents,
        [EnemyFrameId.DraygonFrame22] = FirstBodyComponents,
        [EnemyFrameId.DraygonFrame23] = NoComponents,
        [EnemyFrameId.DraygonFrame24] = NoComponents,
        [EnemyFrameId.DraygonFrame25] = NoComponents,
        [EnemyFrameId.DraygonFrame26] = NoComponents,
        [EnemyFrameId.DraygonFrame27] = NoComponents,
        [EnemyFrameId.DraygonFrame28] = NoComponents,
        [EnemyFrameId.DraygonFrame29] = NoComponents,
        [EnemyFrameId.DraygonFrame30] = NoComponents,
        [EnemyFrameId.DraygonFrame31] = NoComponents,
        [EnemyFrameId.DraygonFrame32] = NoComponents,
        [EnemyFrameId.DraygonFrame33] = NoComponents,
        [EnemyFrameId.DraygonFrame34] = NoComponents,
        [EnemyFrameId.DraygonFrame35] = NoComponents,
        [EnemyFrameId.DraygonFrame36] = NoComponents,
        [EnemyFrameId.DraygonFrame37] = NoComponents,
        [EnemyFrameId.DraygonFrame38] = NoComponents,
        [EnemyFrameId.DraygonFrame39] = NoComponents,
        [EnemyFrameId.DraygonFrame40] = NoComponents,
        [EnemyFrameId.DraygonFrame41] = NoComponents,
        [EnemyFrameId.DraygonFrame42] = NoComponents,
        [EnemyFrameId.DraygonFrame43] = NoComponents,
        [EnemyFrameId.DraygonFrame44] = NoComponents,
        [EnemyFrameId.DraygonFrame45] = NoComponents,
        [EnemyFrameId.DraygonFrame46] = NoComponents,
        [EnemyFrameId.DraygonFrame47] = SecondBodyComponents,
        [EnemyFrameId.DraygonFrame48] = SecondBodyComponents,
        [EnemyFrameId.DraygonFrame49] = SecondBodyComponents,
        [EnemyFrameId.DraygonFrame50] = SecondBodyComponents,
        [EnemyFrameId.DraygonFrame51] = SecondBodyComponents,
        [EnemyFrameId.DraygonFrame52] = SecondBodyComponents,
        [EnemyFrameId.DraygonFrame53] = SecondBodyComponents,
        [EnemyFrameId.DraygonFrame54] = SecondBodyComponents,
        [EnemyFrameId.DraygonFrame55] = EmptyBodyComponents,
        [EnemyFrameId.DraygonFrame56] = EmptyBodyComponents,
        [EnemyFrameId.DraygonFrame57] = EmptyBodyComponents,
        [EnemyFrameId.DraygonFrame58] = EmptyBodyComponents,
        [EnemyFrameId.DraygonFrame59] = EmptyBodyComponents,
        [EnemyFrameId.DraygonFrame60] = EmptyBodyComponents,
        [EnemyFrameId.DraygonFrame61] = EmptyBodyComponents,
        [EnemyFrameId.DraygonFrame62] = EmptyBodyComponents,
        [EnemyFrameId.DraygonFrame63] = SecondBodyComponents,
        [EnemyFrameId.DraygonFrame64] = NoComponents,
        [EnemyFrameId.DraygonFrame65] = NoComponents,
        [EnemyFrameId.DraygonFrame66] = NoComponents,
        [EnemyFrameId.DraygonFrame67] = NoComponents,
        [EnemyFrameId.DraygonFrame68] = NoComponents,
        [EnemyFrameId.DraygonFrame69] = NoComponents,
        [EnemyFrameId.DraygonFrame70] = NoComponents,
        [EnemyFrameId.DraygonFrame71] = NoComponents,
        [EnemyFrameId.DraygonFrame72] = NoComponents,
        [EnemyFrameId.DraygonFrame73] = NoComponents,
        [EnemyFrameId.DraygonFrame74] = NoComponents,
        [EnemyFrameId.DraygonFrame75] = NoComponents,
        [EnemyFrameId.DraygonFrame76] = NoComponents,
        [EnemyFrameId.DraygonFrame77] = NoComponents,
        [EnemyFrameId.DraygonFrame78] = NoComponents,
        [EnemyFrameId.DraygonFrame79] = NoComponents,
        [EnemyFrameId.DraygonFrame80] = NoComponents,
        [EnemyFrameId.DraygonFrame81] = NoComponents,
    };
}
