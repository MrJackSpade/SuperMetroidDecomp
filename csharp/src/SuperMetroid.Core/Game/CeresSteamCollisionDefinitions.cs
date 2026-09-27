namespace SuperMetroid.Core.Game;

/// <summary>Immutable physical component of a Ceres steam extended frame.</summary>
internal readonly record struct CeresSteamCollisionComponent(short X, short Y, ushort HitboxPointer);

/// <summary>Immutable steam rectangle and native touch/shot callback identities.</summary>
internal readonly record struct CeresSteamCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);

/// <summary>
/// Bank-$A6 physical collision for all four Ceres steam directions. Each of the
/// 28 extended frames has one zero-offset component. Its OAM pointer is deliberately
/// absent: installed sprite edits cannot change the damaging plume geometry.
/// </summary>
internal static class CeresSteamCollisionDefinitions
{
    /// <summary>Native bank of the extended maps at $A6:F142-$F250.</summary>
    internal const byte Bank = 0xa6;

    private const ushort Touch = EnemyAiCodePointers.BankA6.CeresSteamTouch;
    private const ushort Shot = EnemyAiCodePointers.BankA0.NoOp;

    // The four first-frame addresses and their consecutive ten-byte records
    // come from ExtendedSpritemap_CeresSteam_* in the pinned bank-$A6 source.
    private static readonly ushort[] DirectionBases = [0xf142, 0xf188, 0xf1ce, 0xf214];
    private static readonly ushort[][] DirectionLists =
    [
        [0xf25c, 0xf26a, 0xf278, 0xf286, 0xf294, 0xf25a, 0xf25a],
        [0xf2a2, 0xf2b0, 0xf2be, 0xf2cc, 0xf2da, 0xf25a, 0xf25a],
        [0xf2e8, 0xf2f6, 0xf304, 0xf312, 0xf320, 0xf25a, 0xf25a],
        [0xf32e, 0xf33c, 0xf34a, 0xf358, 0xf366, 0xf25a, 0xf25a],
    ];

    private static readonly ushort[] FrameKeys = BuildFrameKeys();
    private static readonly Dictionary<ushort, CeresSteamCollisionComponent[]> Frames =
        BuildFrames();
    private static readonly Dictionary<ushort, CeresSteamCollisionHitbox[]> Lists = new()
    {
        [0xf25a] = [],
        [0xf25c] = [new(-8, -16, 7, -1, Touch, Shot)],
        [0xf26a] = [new(-8, -23, 7, -2, Touch, Shot)],
        [0xf278] = [new(-8, -32, 7, -8, Touch, Shot)],
        [0xf286] = [new(-8, -40, 7, -16, Touch, Shot)],
        [0xf294] = [new(-8, -40, 6, -24, Touch, Shot)],
        [0xf2a2] = [new(-16, -8, -1, 7, Touch, Shot)],
        [0xf2b0] = [new(-24, -8, -2, 7, Touch, Shot)],
        [0xf2be] = [new(-32, -7, -9, 7, Touch, Shot)],
        [0xf2cc] = [new(-40, -9, -17, 5, Touch, Shot)],
        [0xf2da] = [new(-40, -11, -26, 2, Touch, Shot)],
        [0xf2e8] = [new(-8, 0, 7, 14, Touch, Shot)],
        [0xf2f6] = [new(-8, 0, 7, 23, Touch, Shot)],
        [0xf304] = [new(-8, 8, 7, 31, Touch, Shot)],
        [0xf312] = [new(-8, 15, 7, 39, Touch, Shot)],
        [0xf320] = [new(-8, 23, 6, 38, Touch, Shot)],
        [0xf32e] = [new(0, -8, 15, 7, Touch, Shot)],
        [0xf33c] = [new(1, -8, 23, 7, Touch, Shot)],
        [0xf34a] = [new(9, -8, 31, 7, Touch, Shot)],
        [0xf358] = [new(18, -9, 38, 5, Touch, Shot)],
        [0xf366] = [new(25, -11, 40, 3, Touch, Shot)],
    };

    internal static ReadOnlySpan<ushort> FramePointers => FrameKeys;
    internal static IEnumerable<ushort> HitboxPointers => Lists.Keys;
    internal static bool HasFrame(ushort frame) => Frames.ContainsKey(frame);

    internal static ReadOnlySpan<CeresSteamCollisionComponent> ComponentsAt(ushort frame) =>
        Frames.TryGetValue(frame, out CeresSteamCollisionComponent[]? components)
            ? components
            : throw new InvalidDataException(
                $"Ceres steam frame $A6:{frame:X4} has no compiled collision.");

    internal static ReadOnlySpan<CeresSteamCollisionHitbox> HitboxesAt(ushort list) =>
        Lists.TryGetValue(list, out CeresSteamCollisionHitbox[]? hitboxes)
            ? hitboxes
            : throw new InvalidDataException(
                $"Ceres steam hitbox list $A6:{list:X4} is not compiled.");

    private static ushort[] BuildFrameKeys()
    {
        var keys = new ushort[28];
        for (int direction = 0; direction < DirectionBases.Length; direction++)
        for (int frame = 0; frame < DirectionLists[direction].Length; frame++)
            keys[direction * 7 + frame] =
                unchecked((ushort)(DirectionBases[direction] + frame * 10));
        return keys;
    }

    private static Dictionary<ushort, CeresSteamCollisionComponent[]> BuildFrames()
    {
        var frames = new Dictionary<ushort, CeresSteamCollisionComponent[]>();
        for (int direction = 0; direction < DirectionBases.Length; direction++)
        for (int frame = 0; frame < DirectionLists[direction].Length; frame++)
            frames.Add(FrameKeys[direction * 7 + frame],
                [new CeresSteamCollisionComponent(0, 0, DirectionLists[direction][frame])]);
        return frames;
    }
}
