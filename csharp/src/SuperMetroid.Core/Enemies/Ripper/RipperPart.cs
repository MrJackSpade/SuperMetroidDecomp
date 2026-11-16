namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>Which Ripper occupant an enemy place holds, one per definition the species installs.</summary>
internal enum RipperPart
{
    /// <summary>The <see cref="EnemyDefinitionId.GrappleRipper"/> occupant.</summary>
    GrappleRipper,
    /// <summary>The <see cref="EnemyDefinitionId.Ripper"/> occupant.</summary>
    Ripper,
    /// <summary>The <see cref="EnemyDefinitionId.JetRipper"/> occupant.</summary>
    JetRipper,
}
