namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>Which Evir occupant a slot holds, one per definition the species installs.</summary>
internal enum EvirPart
{
    /// <summary>The <see cref="EnemyDefinitionId.Evir"/> occupant.</summary>
    Evir,
    /// <summary>The <see cref="EnemyDefinitionId.EvirProjectile"/> occupant.</summary>
    Projectile,
}
