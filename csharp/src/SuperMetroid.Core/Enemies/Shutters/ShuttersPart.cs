namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>Which Shutters occupant a slot holds, one per definition the species installs.</summary>
internal enum ShuttersPart
{
    /// <summary>The <see cref="EnemyDefinitionId.Kamer"/> occupant.</summary>
    Kamer,
    /// <summary>The <see cref="EnemyDefinitionId.ShutterDestroyable"/> occupant.</summary>
    ShutterDestroyable,
    /// <summary>The <see cref="EnemyDefinitionId.ShutterGrowing"/> occupant.</summary>
    ShutterGrowing,
    /// <summary>The <see cref="EnemyDefinitionId.ShutterHorizShootable"/> occupant.</summary>
    ShutterHorizShootable,
    /// <summary>The <see cref="EnemyDefinitionId.ShutterShootable"/> occupant.</summary>
    ShutterShootable,
}
