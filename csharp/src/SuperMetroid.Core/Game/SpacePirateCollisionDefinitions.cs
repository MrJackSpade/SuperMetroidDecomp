namespace SuperMetroid.Core.Game;

/// <summary>Engine-owned offset and hitbox identity in one extended frame component.</summary>
/// <param name="X">Signed horizontal offset from the enemy position before hitbox bounds are applied.</param>
/// <param name="Y">Signed vertical offset from the enemy position before hitbox bounds are applied.</param>
/// <param name="HitboxPointer">Bank-$B2 pointer selecting this component's collision rectangles.</param>
internal readonly record struct SpacePirateCollisionComponent(short X, short Y, ushort HitboxPointer);
/// <summary>Engine-owned signed collision bounds and touch/shot callbacks.</summary>
/// <param name="Left">Signed left bound relative to the component position.</param>
/// <param name="Top">Signed top bound relative to the component position.</param>
/// <param name="Right">Signed right bound relative to the component position.</param>
/// <param name="Bottom">Signed bottom bound relative to the component position.</param>
/// <param name="TouchAi">Native touch-collision callback pointer selected when the rectangle overlaps Samus.</param>
/// <param name="ShotAi">Native shot-collision callback pointer selected when the rectangle overlaps a projectile.</param>
internal readonly record struct SpacePirateCollisionHitbox(short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);

/// <summary>Fixed bank-$B2 walking/wall/ninja-Pirate collision data, separate from editable OAM art.</summary>
/// <remarks>
/// Reviewed for #1165. The 132 frame records lie in 8 contiguous runs and the 147 hitbox
/// lists in 22, so every frame and list address derives from its run start and the
/// preceding record sizes. Component placements and list rectangles are authored per drawing
/// and retained: no hitbox list is a facing mirror of another, and the 105 lists that
/// repeat another list's rectangles are separate native records at their own addresses.
/// </remarks>
internal static class SpacePirateCollisionDefinitions
{
    /// <summary>Authored Space Pirate frame roots mapped to their collision components and offsets.</summary>
    private static readonly CollisionRecordRuns<SpacePirateCollisionComponent> Frames =
        CollisionRecordRuns<SpacePirateCollisionComponent>.Frames(
        new(0x804f,
        [
            [new(0, 0, 0x8059)],
        ]),
        new(0x88a0,
        [
            [new(0, 0, 0x970e), new(0, 0, 0x9690)],
            [new(0, 0, 0x9700), new(0, 0, 0x969e)],
            [new(0, 0, 0x96f2), new(0, 0, 0x96ac)],
            [new(0, 0, 0x96ba), new(0, 0, 0x96e4)],
            [new(0, 0, 0x96c8), new(0, 0, 0x96d6)],
            [new(0, -2, 0x972a), new(0, 0, 0x970e)],
            [new(1, -2, 0x9738), new(0, 0, 0x96d6)],
            [new(0, 0, 0x9746)],
            [new(0, 0, 0x9754)],
            [new(0, 0, 0x97e0), new(0, 0, 0x9762)],
            [new(0, 0, 0x97d2), new(0, 0, 0x9770)],
            [new(0, 0, 0x97c4), new(0, 0, 0x977e)],
            [new(0, 0, 0x978c), new(0, 0, 0x97b6)],
            [new(0, 0, 0x979a), new(0, 0, 0x97a8)],
            [new(0, 0, 0x97ee), new(0, 2, 0x97a8)],
            [new(0, 0, 0x97fc), new(0, 2, 0x97a8)],
            [new(0, 0, 0x980a)],
            [new(0, 0, 0x9818)],
            [new(-5, 3, 0x9b60), new(0, 0, 0x9826)],
            [new(-5, 3, 0x9b7c), new(0, 0, 0x9834)],
            [new(-5, 3, 0x9b8a), new(0, 0, 0x9842)],
            [new(-5, 3, 0x9b98), new(2, 0, 0x9850)],
            [new(-5, 3, 0x9b98), new(2, 0, 0x985e)],
            [new(-5, 3, 0x9b8a), new(2, 0, 0x986c)],
            [new(-5, 3, 0x9b7c), new(0, 0, 0x987a)],
            [new(-5, 3, 0x9b60), new(0, 0, 0x9888)],
            [new(0, 5, 0x9b6e), new(0, 3, 0x9922)],
            [new(0, 5, 0x9ba6), new(0, 3, 0x9896)],
            [new(0, 5, 0x9bb4), new(0, 3, 0x9896)],
            [new(0, 4, 0x9bc2), new(0, 3, 0x9896)],
            [new(0, 3, 0x9bd0), new(0, 3, 0x9896)],
            [new(0, 5, 0x9bc2), new(0, 3, 0x9896)],
            [new(0, 5, 0x9ba6), new(0, 3, 0x9896)],
            [new(0, 6, 0x9b98), new(0, 3, 0x9896)],
            [new(0, 7, 0x9c5c), new(0, 3, 0x9922)],
            [new(5, 3, 0x9d5e), new(0, 0, 0x994c)],
            [new(5, 3, 0x9d7a), new(0, 0, 0x995a)],
            [new(5, 3, 0x9d88), new(0, 0, 0x9968)],
            [new(5, 3, 0x9d96), new(0, 0, 0x9976)],
            [new(5, 3, 0x9d96), new(-1, 0, 0x9984)],
            [new(5, 3, 0x9d88), new(0, 0, 0x9992)],
            [new(5, 3, 0x9d7a), new(1, 0, 0x99a0)],
            [new(5, 3, 0x9d5e), new(1, 0, 0x99ae)],
            [new(0, 5, 0x9d6c), new(0, 3, 0x9a3a)],
            [new(0, 5, 0x9da4), new(0, 3, 0x99bc)],
            [new(0, 5, 0x9db2), new(0, 3, 0x99bc)],
            [new(0, 4, 0x9dc0), new(0, 3, 0x99bc)],
            [new(0, 3, 0x9dce), new(0, 3, 0x99bc)],
            [new(0, 5, 0x9dc0), new(0, 3, 0x99bc)],
            [new(0, 5, 0x9d96), new(0, 3, 0x99bc)],
        ]),
        new(0x8c16,
        [
            [new(0, 7, 0x9e5a), new(0, 3, 0x9a3a)],
            [new(0, 3, 0x9a48), new(0, 3, 0x9896)],
            [new(0, 3, 0x9a56), new(0, 3, 0x9896)],
            [new(0, 3, 0x9a64), new(0, 3, 0x9896)],
            [new(0, 3, 0x9a72), new(0, 3, 0x9896)],
            [new(-1, 4, 0x9a80), new(0, 3, 0x9896)],
            [new(-2, 6, 0x9a8e), new(0, 3, 0x9922)],
            [new(0, 3, 0x9a9c), new(0, 3, 0x99bc)],
            [new(0, 3, 0x9aaa), new(0, 3, 0x99bc)],
            [new(0, 3, 0x9ab8), new(0, 3, 0x99bc)],
            [new(0, 3, 0x9ac6), new(0, 3, 0x99bc)],
            [new(1, 4, 0x9ad4), new(0, 3, 0x99bc)],
            [new(2, 6, 0x9ae2), new(0, 3, 0x9a3a)],
            [new(-5, -12, 0x9a48), new(0, 3, 0x9930), new(0, 3, 0x9896)],
            [new(0, 3, 0x9a48), new(0, 3, 0x9896)],
            [new(-5, -11, 0x9a48), new(0, 3, 0x9930), new(0, 3, 0x9896)],
            [new(5, -12, 0x9a48), new(0, 3, 0x993e), new(0, 3, 0x99bc)],
            [new(0, 3, 0x9a9c), new(0, 3, 0x99bc)],
            [new(5, -11, 0x9a48), new(0, 3, 0x993e), new(0, 3, 0x99bc)],
            [new(0, 1, 0x971c)],
            [new(0, 0, 0x9ede)],
            [new(0, 0, 0x9eec)],
            [new(0, 0, 0x9efa)],
            [new(0, 0, 0x9f08)],
            [new(0, 0, 0x9f16)],
            [new(0, 0, 0x9f24)],
            [new(0, 0, 0x9f32)],
            [new(0, 0, 0x9f40)],
            [new(0, 0, 0x9f78)],
            [new(0, 0, 0x9f86)],
            [new(0, 0, 0x9f94)],
            [new(0, 0, 0x9fa2)],
            [new(0, 0, 0x9fb0)],
            [new(0, 0, 0x9fbe)],
            [new(0, 0, 0x9fcc)],
            [new(0, 0, 0x9fda)],
            [new(-5, 1, 0x9b60), new(0, -2, 0x9af0)],
            [new(-5, 3, 0x9b7c), new(-1, 0, 0x9afe)],
            [new(-5, 4, 0x9b8a), new(0, 0, 0x9b0c)],
            [new(-5, 2, 0x9b98), new(-1, 0, 0x9b1a)],
            [new(-5, 1, 0x9b98), new(2, -2, 0x9b28)],
            [new(-5, 3, 0x9b8a), new(2, 0, 0x9b36)],
            [new(-5, 3, 0x9b7c), new(0, 0, 0x9b44)],
            [new(-5, 1, 0x9b60), new(0, 0, 0x9b52)],
            [new(5, 1, 0x9d5e), new(0, -2, 0x9cee)],
            [new(5, 3, 0x9d7a), new(0, 0, 0x9cfc)],
            [new(5, 4, 0x9d88), new(0, 0, 0x9d0a)],
            [new(5, 2, 0x9d96), new(0, 0, 0x9d18)],
            [new(5, 1, 0x9d96), new(-1, -1, 0x9d26)],
            [new(5, 3, 0x9d88), new(0, 0, 0x9d34)],
            [new(5, 3, 0x9d7a), new(1, 0, 0x9d42)],
            [new(5, 1, 0x9d5e), new(1, 0, 0x9d50)],
            [new(0, 3, 0x9bde)],
            [new(0, 3, 0x9bec)],
            [new(0, 3, 0x9bfa)],
            [new(0, 3, 0x9ddc)],
            [new(0, 3, 0x9dea)],
            [new(0, 3, 0x9df8)],
            [new(0, 8, 0x9c78)],
            [new(0, 8, 0x9c92)],
            [new(0, 8, 0x9e68)],
            [new(0, 8, 0x9e82)],
        ]),
        new(0x900a,
        [
            [new(0, 0, 0xa560)],
            [new(0, 0, 0xa612)],
            [new(0, 0, 0xa744)],
            [new(0, 0, 0xa7e8)],
            [new(5, 0, 0xa5ea)],
            [new(-5, 1, 0xa5f8)],
            [new(-5, 0, 0xa7c0)],
            [new(5, 1, 0xa7ce)],
        ]),
        new(0x90fe,
        [
            [new(0, 5, 0xa4b8), new(0, 3, 0xa27a)],
        ]),
        new(0x9280,
        [
            [new(0, 5, 0xa69c), new(0, 3, 0xa392)],
        ]),
        new(0x9372,
        [
            [new(2, 0, 0xa56e)],
            [new(-2, 0, 0xa752)],
        ]),
        new(0x93ea,
        [
            [new(-5, -12, 0xa3a0), new(0, 3, 0xa288), new(0, 3, 0xa1ee)],
            [new(0, 3, 0xa3a0), new(0, 3, 0xa1ee)],
            [new(-5, -11, 0xa3a0), new(0, 3, 0xa288), new(0, 3, 0xa1ee)],
            [new(5, -12, 0xa3a0), new(0, 3, 0xa296), new(0, 3, 0xa314)],
            [new(0, 3, 0xa3f4), new(0, 3, 0xa314)],
            [new(5, -11, 0xa3a0), new(0, 3, 0xa296), new(0, 3, 0xa314)],
            [new(0, 1, 0xa074)],
        ]));

    /// <summary>Authored bank-$B2 collision-list roots containing signed rectangles and native callbacks.</summary>
    private static readonly CollisionRecordRuns<SpacePirateCollisionHitbox> Lists =
        CollisionRecordRuns<SpacePirateCollisionHitbox>.HitboxLists(
        new(0x8059,
        [
            [new(0, 0, 0, 0, 0x8023, 0x802d)],
        ]),
        new(0x9690,
        [
            [new(-18, -19, 6, 0, 0x876c, 0x8779)],
            [new(-18, -19, 6, 0, 0x876c, 0x8779)],
            [new(-18, -19, 6, 0, 0x876c, 0x8779)],
            [new(-18, -19, 6, 0, 0x876c, 0x8779)],
            [new(-18, -19, 6, 0, 0x876c, 0x8779)],
            [new(-14, 0, 4, 30, 0x876c, 0x8779)],
            [new(-15, 0, -1, 30, 0x876c, 0x8779)],
            [new(-15, -6, 0, 23, 0x876c, 0x8779)],
            [new(-16, -5, -1, 25, 0x876c, 0x8779)],
            [new(-17, -8, 0, 30, 0x876c, 0x8779)],
            [new(-13, -19, 10, 30, 0x876c, 0x8779)],
            [new(-15, -19, 14, 6, 0x876c, 0x8779)],
            [new(-16, -19, 14, 3, 0x876c, 0x8779)],
            [new(-10, -21, 19, 22, 0x876c, 0x8779)],
            [new(-8, -19, 18, 16, 0x876c, 0x8779)],
            [new(-9, -23, 17, 0, 0x876c, 0x8779)],
            [new(-9, -19, 16, 0, 0x876c, 0x8779)],
            [new(-9, -19, 17, 0, 0x876c, 0x8779)],
            [new(-9, -19, 16, 0, 0x876c, 0x8779)],
            [new(-9, -19, 17, 0, 0x876c, 0x8779)],
            [new(-7, 0, 15, 30, 0x876c, 0x8779)],
            [new(-2, 0, 15, 30, 0x876c, 0x8779)],
            [new(-2, 0, 15, 23, 0x876c, 0x8779)],
            [new(0, 0, 15, 25, 0x876c, 0x8779)],
            [new(-1, 0, 15, 30, 0x876c, 0x8779)],
            [new(-15, -19, 15, 0, 0x876c, 0x8779)],
            [new(-15, -19, 14, 3, 0x876c, 0x8779)],
            [new(-20, -19, 10, 25, 0x876c, 0x8779)],
            [new(-20, -19, 6, 16, 0x876c, 0x8779)],
            [new(-11, 0, 8, 30, 0x876c, 0x8779)],
            [new(-11, 0, 8, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x883e)],
        ]),
        new(0x9922,
        [
            [new(-7, 0, 6, 30, 0x876c, 0x883e)],
            [new(-7, -19, 6, 0, 0x876c, 0x8779)],
            [new(-7, -19, 6, 0, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, -1, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x883e)],
        ]),
        new(0x9a3a,
        [
            [new(-7, 0, 6, 30, 0x876c, 0x883e)],
            [new(-7, -19, 6, 0, 0x876c, 0x8779)],
            [new(-7, -19, 6, 0, 0x876c, 0x8779)],
            [new(-7, -19, 6, 0, 0x876c, 0x8779)],
            [new(-7, -19, 6, 0, 0x876c, 0x8779)],
            [new(-7, -19, 6, 0, 0x876c, 0x8779)],
            [new(-7, -19, 6, 0, 0x876c, 0x8779)],
            [new(-7, -19, 6, 0, 0x876c, 0x8779)],
            [new(-7, -19, 6, 0, 0x876c, 0x8779)],
            [new(-7, -19, 6, 0, 0x876c, 0x8779)],
            [new(-7, -19, 6, 0, 0x876c, 0x8779)],
            [new(-7, -19, 6, 0, 0x876c, 0x8779)],
            [new(-7, -19, 6, 0, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, -19, 6, 0, 0x876c, 0x87c8)],
            [new(-7, -19, 6, 0, 0x876c, 0x883e)],
            [new(-7, -19, 6, 0, 0x876c, 0x8779)],
            [new(-7, -19, 6, 0, 0x876c, 0x8779)],
            [new(-7, -19, 6, 0, 0x876c, 0x883e)],
            [new(-7, -19, 6, 0, 0x876c, 0x883e)],
            [new(-7, -19, 6, 0, 0x876c, 0x883e)],
            [new(-7, -19, 6, 0, 0x876c, 0x883e)],
            [new(-7, -19, 6, 0, 0x876c, 0x883e)],
            [new(-7, -19, 6, 30, 0x876c, 0x883e)],
            [new(-7, -19, 6, 30, 0x876c, 0x883e)],
            [new(-7, -19, 6, 30, 0x876c, 0x883e)],
        ]),
        new(0x9c5c,
        [
            [new(-7, -19, 6, 0, 0x876c, 0x883e)],
        ]),
        new(0x9c78,
        [
            [new(-7, -19, 6, 23, 0x876c, 0x8779), new(-18, -18, -7, 2, 0x876c, 0x87c8)],
            [new(-7, -19, 6, 23, 0x876c, 0x883e), new(-18, -18, -7, 2, 0x876c, 0x883e)],
        ]),
        new(0x9cee,
        [
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, -19, 6, 0, 0x876c, 0x87c8)],
            [new(-7, -19, 6, 0, 0x876c, 0x883e)],
            [new(-7, -19, 6, 0, 0x876c, 0x87c8)],
            [new(-7, -19, 6, 0, 0x876c, 0x87c8)],
            [new(-7, -19, 6, 0, 0x876c, 0x87c8)],
            [new(-7, -19, 6, 0, 0x876c, 0x883e)],
            [new(-7, -19, 6, 0, 0x876c, 0x883e)],
            [new(-7, -19, 6, 0, 0x876c, 0x883e)],
            [new(-7, -19, 6, 0, 0x876c, 0x883e)],
            [new(-7, -19, 6, 30, 0x876c, 0x883e)],
            [new(-7, -19, 6, 30, 0x876c, 0x883e)],
            [new(-7, -19, 6, 30, 0x876c, 0x883e)],
        ]),
        new(0x9e5a,
        [
            [new(-7, -19, 6, 0, 0x876c, 0x883e)],
            [new(-7, -19, 6, 23, 0x876c, 0x8779), new(6, -19, 17, 1, 0x876c, 0x87c8)],
            [new(-7, -19, 6, 23, 0x876c, 0x883e), new(6, -19, 17, 1, 0x876c, 0x883e)],
        ]),
        new(0x9ede,
        [
            [new(-11, -13, 10, 10, 0x876c, 0x883e)],
            [new(-11, -13, 10, 10, 0x876c, 0x883e)],
            [new(-11, -13, 10, 10, 0x876c, 0x883e)],
            [new(-11, -13, 10, 10, 0x876c, 0x883e)],
            [new(-11, -13, 10, 10, 0x876c, 0x883e)],
            [new(-11, -13, 10, 10, 0x876c, 0x883e)],
            [new(-11, -13, 10, 10, 0x876c, 0x883e)],
            [new(-11, -13, 10, 10, 0x876c, 0x883e)],
        ]),
        new(0x9f78,
        [
            [new(-11, -13, 10, 10, 0x876c, 0x883e)],
            [new(-11, -13, 10, 10, 0x876c, 0x883e)],
            [new(-11, -13, 10, 10, 0x876c, 0x883e)],
            [new(-11, -13, 10, 10, 0x876c, 0x883e)],
            [new(-11, -13, 10, 10, 0x876c, 0x883e)],
            [new(-11, -13, 10, 10, 0x876c, 0x883e)],
            [new(-11, -13, 10, 10, 0x876c, 0x883e)],
            [new(-11, -13, 10, 10, 0x876c, 0x883e)],
        ]),
        new(0xa074,
        [
            [new(-7, -19, 6, 30, 0x876c, 0x883e)],
        ]),
        new(0xa1ee,
        [
            [new(-7, 0, 6, 30, 0x876c, 0x883e)],
        ]),
        new(0xa27a,
        [
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, -19, 6, 0, 0x876c, 0x883e)],
            [new(-7, -19, 6, 0, 0x876c, 0x883e)],
        ]),
        new(0xa314,
        [
            [new(-7, 0, 6, 30, 0x876c, 0x883e)],
        ]),
        new(0xa392,
        [
            [new(-7, 0, 6, 30, 0x876c, 0x8779)],
            [new(-7, -19, 6, 0, 0x876c, 0x883e)],
        ]),
        new(0xa3f4,
        [
            [new(-7, -19, 6, 0, 0x876c, 0x883e)],
        ]),
        new(0xa4b8,
        [
            [new(-7, -19, 6, 0, 0x876c, 0x8779)],
        ]),
        new(0xa560,
        [
            [new(-7, -19, 6, 16, 0x876c, 0x8779)],
            [new(-7, -19, 6, 30, 0x876c, 0x883e)],
        ]),
        new(0xa5ea,
        [
            [new(-7, -19, 6, 30, 0x876c, 0x883e)],
            [new(-7, -19, 6, 30, 0x876c, 0x883e), new(-34, -5, 3, 30, 0x876c, 0x883e)],
            [new(-7, -19, 6, 16, 0x876c, 0x8779), new(-33, 3, -7, 16, 0x876c, 0x8779)],
        ]),
        new(0xa69c,
        [
            [new(-7, -19, 6, 0, 0x876c, 0x8779)],
        ]),
        new(0xa744,
        [
            [new(-7, -19, 6, 16, 0x876c, 0x8779)],
            [new(-7, -19, 6, 30, 0x876c, 0x883e)],
        ]),
        new(0xa7c0,
        [
            [new(-7, -19, 6, 30, 0x876c, 0x883e)],
            [new(-7, -19, 6, 30, 0x876c, 0x883e), new(6, -6, 32, 30, 0x876c, 0x883e)],
            [new(-7, -19, 6, 16, 0x876c, 0x8779), new(6, 3, 31, 16, 0x876c, 0x8779)],
        ]));

    /// <summary>Gets the engine-owned collision components for a compiled Space Pirate frame.</summary>
    /// <param name="pointer">Bank-$B2 spritemap pointer identifying the frame.</param>
    /// <returns>Components with offsets and hitbox-list pointers for that frame.</returns>
    /// <exception cref="InvalidDataException">The pointer is not a compiled Space Pirate collision frame.</exception>
    internal static ReadOnlySpan<SpacePirateCollisionComponent> ComponentsAt(ushort pointer) =>
        Frames.TryGet(pointer, out SpacePirateCollisionComponent[] components)
            ? components
            : throw new InvalidDataException($"Space Pirate collision frame $B2:{pointer:X4} is not compiled.");

    /// <summary>Gets the engine-owned rectangles and touch/shot callbacks for a compiled collision list.</summary>
    /// <param name="pointer">Bank-$B2 hitbox-list pointer stored by a frame component.</param>
    /// <returns>The signed bounds and native callback words in the selected list.</returns>
    /// <exception cref="InvalidDataException">The pointer is not a compiled Space Pirate hitbox list.</exception>
    internal static ReadOnlySpan<SpacePirateCollisionHitbox> HitboxesAt(ushort pointer) =>
        Lists.TryGet(pointer, out SpacePirateCollisionHitbox[] hitboxes)
            ? hitboxes
            : throw new InvalidDataException($"Space Pirate hitbox list $B2:{pointer:X4} is not compiled.");
}
