using SuperMetroid.Core.Enemies.Gunship;

namespace SuperMetroid.Verification.Enemies.Gunship;

/// <summary>The native bank-$86 list each typed gunship dust program replaces, for the #641 ROM oracle.</summary>
internal static class GunshipDustNativeListAddresses
{
    /// <summary>
    /// Each spawned program's <c>InstList_EnemyProjectile_GunshipLiftoffDustClouds_Index*_0</c> list; the spawn table at
    /// $86:A2E2 names these.
    /// </summary>
    internal static ushort Entry(GunshipDustProgram program) => program switch
    {
        GunshipDustProgram.RightInner => 0xa197,
        GunshipDustProgram.RightMiddle => 0xa1c1,
        GunshipDustProgram.RightOuter => 0xa1eb,
        GunshipDustProgram.LeftInner => 0xa211,
        GunshipDustProgram.LeftMiddle => 0xa23b,
        GunshipDustProgram.LeftOuter => 0xa265,
        GunshipDustProgram.RightInnerLoop or
            GunshipDustProgram.RightMiddleLoop or
            GunshipDustProgram.RightOuterLoop or
            GunshipDustProgram.LeftInnerLoop or
            GunshipDustProgram.LeftMiddleLoop or
            GunshipDustProgram.LeftOuterLoop => throw new ArgumentOutOfRangeException(nameof(program), program, "Not a spawned gunship dust program."),
        _ => throw new ArgumentOutOfRangeException(nameof(program), program, "Not a spawned gunship dust program."),
    };

    /// <summary>The word of InitAI_EnemyProjectile_GunshipLiftoffDustClouds' .InstListPointers table ($86:A2E2) naming the list of dust <paramref name="parameter"/>.</summary>
    internal static int SpawnListWord(ushort parameter) => parameter switch
    {
        0 => 0x86a2e2,
        2 => 0x86a2e4,
        4 => 0x86a2e6,
        6 => 0x86a2e8,
        8 => 0x86a2ea,
        10 => 0x86a2ec,
        _ => throw new ArgumentOutOfRangeException(nameof(parameter), parameter, "Gunship dust parameters are 0, 2, 4, 6, 8 and 10."),
    };

    /// <summary>The dust lists' span, $86:A197..A28D, which production must never read.</summary>
    internal const int ListsStart = 0x86a197;
    /// <summary>The first word past the last dust list.</summary>
    internal const int ListsEnd = 0x86a28d;
}
