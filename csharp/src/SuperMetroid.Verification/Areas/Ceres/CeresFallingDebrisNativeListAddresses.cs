using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>The native bank-$86 list each typed Ceres falling tile program replaces, for the #641 ROM oracle.</summary>
internal static class CeresFallingDebrisNativeListAddresses
{
    /// <summary>Each program's projectile kind and native list: <c>InstList_EnemyProjectile_CeresFallingTile_Light/Dark</c>.</summary>
    internal static readonly CeresFallingDebrisNativeListAddressesEntry[] Entries =
    [
        new CeresFallingDebrisNativeListAddressesEntry(CeresFallingDebrisProgram.Light, RoomEnemyProjectileKind.CeresFallingDebrisLight, 0x9750),
        new CeresFallingDebrisNativeListAddressesEntry(CeresFallingDebrisProgram.Dark, RoomEnemyProjectileKind.CeresFallingDebrisDark, 0x9756),
    ];

    /// <summary>The tile lists' span, $86:9750..975C, which production must never read.</summary>
    internal const int ListsStart = 0x869750;
    /// <summary>The first word past the dark tile's list.</summary>
    internal const int ListsEnd = 0x86975c;
}
