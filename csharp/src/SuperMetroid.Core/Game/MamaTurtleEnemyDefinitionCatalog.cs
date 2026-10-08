namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled bank-$A0 enemy headers for the Mama Turtle family. Gameplay statistics,
/// callback identities, and presentation bindings stay application-owned while the
/// referenced artwork and animation programs can be separated into editable assets.
/// </summary>
internal static class MamaTurtleEnemyDefinitionCatalog
{
    /// <summary><c>EnemyHeaders_MamaTurtle</c> at <c>$A0:CF3F</c>.</summary>
    internal const ushort MamaPointer = 0xcf3f;

    /// <summary><c>EnemyHeaders_BabyTurtle</c> at <c>$A0:CF7F</c>.</summary>
    internal const ushort BabyPointer = 0xcf7f;
}
