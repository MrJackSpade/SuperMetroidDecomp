namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>Which Ripper occupant a slot holds, one per definition the species installs.</summary>
internal enum RipperPart
{
    /// <summary>The <see cref="EnemyDefinitionId.GRipper"/> occupant.</summary>
    GRipper,
    /// <summary>The <see cref="EnemyDefinitionId.Ripper"/> occupant.</summary>
    Ripper,
    /// <summary>The <see cref="EnemyDefinitionId.Ripper2"/> occupant.</summary>
    Ripper2,
}
