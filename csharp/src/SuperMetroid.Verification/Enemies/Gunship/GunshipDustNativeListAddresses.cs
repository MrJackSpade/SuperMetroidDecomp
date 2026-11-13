using SuperMetroid.Core.Enemies.Gunship;

namespace SuperMetroid.Verification.Enemies.Gunship;

/// <summary>The native bank-$86 list each typed gunship dust program replaces, for the #641 ROM oracle.</summary>
internal static class GunshipDustNativeListAddresses
{
    /// <summary>
    /// Each spawned program's <c>InstList_EnemyProjectile_GunshipLiftoffDustClouds_Index*_0</c> list; the spawn table at
    /// $86:A2E2 names these.
    /// </summary>
    internal static ushort Entry(GunshipDustSegment program) => program switch
    {
        GunshipDustSegment.RightInner => 0xa197,
        GunshipDustSegment.RightMiddle => 0xa1c1,
        GunshipDustSegment.RightOuter => 0xa1eb,
        GunshipDustSegment.LeftInner => 0xa211,
        GunshipDustSegment.LeftMiddle => 0xa23b,
        GunshipDustSegment.LeftOuter => 0xa265,
        GunshipDustSegment.RightInnerLoop or
            GunshipDustSegment.RightMiddleLoop or
            GunshipDustSegment.RightOuterLoop or
            GunshipDustSegment.LeftInnerLoop or
            GunshipDustSegment.LeftMiddleLoop or
            GunshipDustSegment.LeftOuterLoop => throw new ArgumentOutOfRangeException(nameof(program), program, "Not a spawned gunship dust program."),
        _ => throw new ArgumentOutOfRangeException(nameof(program), program, "Not a spawned gunship dust program."),
    };

    /// <summary>
    /// The word of InitAI_EnemyProjectile_GunshipLiftoffDustClouds' .InstListPointers table ($86:A2E2) naming the list of
    /// the gunship's liftoff cloud <paramref name="cloud"/>, in spawn order.
    /// </summary>
    internal static int SpawnListWord(int cloud) => cloud switch
    {
        0 => 0x86a2e2,
        1 => 0x86a2e4,
        2 => 0x86a2e6,
        3 => 0x86a2e8,
        4 => 0x86a2ea,
        5 => 0x86a2ec,
        _ => throw new ArgumentOutOfRangeException(nameof(cloud), cloud, "The gunship raises six liftoff dust clouds."),
    };

    /// <summary>The dust lists' span, $86:A197..A28D, which production must never read.</summary>
    internal const int ListsStart = 0x86a197;
    /// <summary>The first word past the last dust list.</summary>
    internal const int ListsEnd = 0x86a28d;
}
