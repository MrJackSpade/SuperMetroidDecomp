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
    private static readonly DraygonCollisionHitbox[] FirstBodyHitboxes =
    [
        new(-17, -16, 18, 30, DraygonHitboxTouch.Harmless, DraygonHitboxShot.Eye),
        new(-59, -66, 3, -30, DraygonHitboxTouch.Body, DraygonHitboxShot.Dud),
        new(16, 1, 60, 53, DraygonHitboxTouch.Body, DraygonHitboxShot.Dud),
        new(-28, -37, 61, -10, DraygonHitboxTouch.Body, DraygonHitboxShot.Dud),
    ];

    /// <summary>Mirrored right-facing body rectangles with the same shootable-eye rule.</summary>
    private static readonly DraygonCollisionHitbox[] SecondBodyHitboxes =
    [
        new(-18, -7, 18, 30, DraygonHitboxTouch.Harmless, DraygonHitboxShot.Eye),
        new(-10, -63, 62, -26, DraygonHitboxTouch.Body, DraygonHitboxShot.Dud),
        new(-55, 0, -21, 57, DraygonHitboxTouch.Body, DraygonHitboxShot.Dud),
        new(-62, -33, 15, -5, DraygonHitboxTouch.Body, DraygonHitboxShot.Dud),
    ];

    /// <summary>The empty list of the BG2 frames in which the body cannot be hit or touched.</summary>
    private static readonly DraygonCollisionHitbox[] EmptyHitboxes = [];

    /// <summary>Ordinary OAM art has no collision components.</summary>
    private static readonly ImmutableArray<DraygonCollisionComponent> NoComponents = [];
    /// <summary>The left-facing body's one zero-offset component.</summary>
    private static readonly ImmutableArray<DraygonCollisionComponent> FirstBodyComponents = [new(Offset16<RoomPixels>.Zero, FirstBodyHitboxes)];
    /// <summary>The right-facing body's one zero-offset component.</summary>
    private static readonly ImmutableArray<DraygonCollisionComponent> SecondBodyComponents = [new(Offset16<RoomPixels>.Zero, SecondBodyHitboxes)];
    /// <summary>The one zero-offset component of a BG2 frame in which the body cannot be hit or touched.</summary>
    private static readonly ImmutableArray<DraygonCollisionComponent> EmptyBodyComponents = [new(Offset16<RoomPixels>.Zero, EmptyHitboxes)];

    /// <summary>
    /// Each Draygon frame's collision components. Every BG2 body frame has one zero-offset component: the first eight and
    /// final frame of each facing use that facing's body hitboxes and the frames between use the empty list. Ordinary OAM
    /// frames have no collision components.
    /// </summary>
    private static readonly Dictionary<EnemyFrameId, ImmutableArray<DraygonCollisionComponent>> FrameComponents = new()
    {
        [EnemyFrameId.DraygonOamA2DF] = NoComponents,
        [EnemyFrameId.DraygonOamA2E9] = NoComponents,
        [EnemyFrameId.DraygonOamA2F3] = NoComponents,
        [EnemyFrameId.DraygonOamA2FD] = NoComponents,
        [EnemyFrameId.DraygonOamA307] = NoComponents,
        [EnemyFrameId.DraygonOamA311] = NoComponents,
        [EnemyFrameId.DraygonBg2A31B] = FirstBodyComponents,
        [EnemyFrameId.DraygonBg2A325] = FirstBodyComponents,
        [EnemyFrameId.DraygonBg2A32F] = FirstBodyComponents,
        [EnemyFrameId.DraygonBg2A339] = FirstBodyComponents,
        [EnemyFrameId.DraygonBg2A343] = FirstBodyComponents,
        [EnemyFrameId.DraygonBg2A34D] = FirstBodyComponents,
        [EnemyFrameId.DraygonBg2A357] = FirstBodyComponents,
        [EnemyFrameId.DraygonBg2A361] = FirstBodyComponents,
        [EnemyFrameId.DraygonBg2A36B] = EmptyBodyComponents,
        [EnemyFrameId.DraygonBg2A375] = EmptyBodyComponents,
        [EnemyFrameId.DraygonBg2A37F] = EmptyBodyComponents,
        [EnemyFrameId.DraygonBg2A389] = EmptyBodyComponents,
        [EnemyFrameId.DraygonBg2A393] = EmptyBodyComponents,
        [EnemyFrameId.DraygonBg2A39D] = EmptyBodyComponents,
        [EnemyFrameId.DraygonBg2A3A7] = EmptyBodyComponents,
        [EnemyFrameId.DraygonBg2A3B1] = EmptyBodyComponents,
        [EnemyFrameId.DraygonBg2A3BB] = FirstBodyComponents,
        [EnemyFrameId.DraygonOamA3C5] = NoComponents,
        [EnemyFrameId.DraygonOamA3CF] = NoComponents,
        [EnemyFrameId.DraygonOamA3D9] = NoComponents,
        [EnemyFrameId.DraygonOamA3E3] = NoComponents,
        [EnemyFrameId.DraygonOamA40B] = NoComponents,
        [EnemyFrameId.DraygonOamA41D] = NoComponents,
        [EnemyFrameId.DraygonOamA42F] = NoComponents,
        [EnemyFrameId.DraygonOamA441] = NoComponents,
        [EnemyFrameId.DraygonOamA453] = NoComponents,
        [EnemyFrameId.DraygonOamA465] = NoComponents,
        [EnemyFrameId.DraygonOamA477] = NoComponents,
        [EnemyFrameId.DraygonOamA489] = NoComponents,
        [EnemyFrameId.DraygonOamA4A3] = NoComponents,
        [EnemyFrameId.DraygonOamA4C5] = NoComponents,
        [EnemyFrameId.DraygonOamA4EF] = NoComponents,
        [EnemyFrameId.DraygonOamA521] = NoComponents,
        [EnemyFrameId.DraygonOamA55B] = NoComponents,
        [EnemyFrameId.DraygonOamA59D] = NoComponents,
        [EnemyFrameId.DraygonOamA607] = NoComponents,
        [EnemyFrameId.DraygonOamA611] = NoComponents,
        [EnemyFrameId.DraygonOamA61B] = NoComponents,
        [EnemyFrameId.DraygonOamA625] = NoComponents,
        [EnemyFrameId.DraygonOamA62F] = NoComponents,
        [EnemyFrameId.DraygonOamA639] = NoComponents,
        [EnemyFrameId.DraygonBg2A643] = SecondBodyComponents,
        [EnemyFrameId.DraygonBg2A64D] = SecondBodyComponents,
        [EnemyFrameId.DraygonBg2A657] = SecondBodyComponents,
        [EnemyFrameId.DraygonBg2A661] = SecondBodyComponents,
        [EnemyFrameId.DraygonBg2A66B] = SecondBodyComponents,
        [EnemyFrameId.DraygonBg2A675] = SecondBodyComponents,
        [EnemyFrameId.DraygonBg2A67F] = SecondBodyComponents,
        [EnemyFrameId.DraygonBg2A689] = SecondBodyComponents,
        [EnemyFrameId.DraygonBg2A693] = EmptyBodyComponents,
        [EnemyFrameId.DraygonBg2A69D] = EmptyBodyComponents,
        [EnemyFrameId.DraygonBg2A6A7] = EmptyBodyComponents,
        [EnemyFrameId.DraygonBg2A6B1] = EmptyBodyComponents,
        [EnemyFrameId.DraygonBg2A6BB] = EmptyBodyComponents,
        [EnemyFrameId.DraygonBg2A6C5] = EmptyBodyComponents,
        [EnemyFrameId.DraygonBg2A6CF] = EmptyBodyComponents,
        [EnemyFrameId.DraygonBg2A6D9] = EmptyBodyComponents,
        [EnemyFrameId.DraygonBg2A6E3] = SecondBodyComponents,
        [EnemyFrameId.DraygonOamA6ED] = NoComponents,
        [EnemyFrameId.DraygonOamA6F7] = NoComponents,
        [EnemyFrameId.DraygonOamA701] = NoComponents,
        [EnemyFrameId.DraygonOamA70B] = NoComponents,
        [EnemyFrameId.DraygonOamA779] = NoComponents,
        [EnemyFrameId.DraygonOamA78B] = NoComponents,
        [EnemyFrameId.DraygonOamA79D] = NoComponents,
        [EnemyFrameId.DraygonOamA7AF] = NoComponents,
        [EnemyFrameId.DraygonOamA7C1] = NoComponents,
        [EnemyFrameId.DraygonOamA7D3] = NoComponents,
        [EnemyFrameId.DraygonOamA7E5] = NoComponents,
        [EnemyFrameId.DraygonOamA7F7] = NoComponents,
        [EnemyFrameId.DraygonOamA811] = NoComponents,
        [EnemyFrameId.DraygonOamA833] = NoComponents,
        [EnemyFrameId.DraygonOamA85D] = NoComponents,
        [EnemyFrameId.DraygonOamA88F] = NoComponents,
        [EnemyFrameId.DraygonOamA8C9] = NoComponents,
        [EnemyFrameId.DraygonOamA90B] = NoComponents,
    };

    /// <summary>The collision components of Draygon frame <paramref name="frame"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not one of Draygon's frames.</exception>
    internal static ImmutableArray<DraygonCollisionComponent> ComponentsAt(EnemyFrameId? frame) =>
        frame is { } shown && FrameComponents.TryGetValue(shown, out ImmutableArray<DraygonCollisionComponent> components)
            ? components
            : throw new InvalidDataException($"Draygon frame {frame} has no compiled collision identity.");
}
