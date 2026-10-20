using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Which MotherBrain occupant a slot holds, one per definition the species installs.</summary>
internal enum MotherBrainPart
{
    /// <summary>The <see cref="EnemyDefinitionId.MotherBrainBody"/> occupant.</summary>
    Body,
    /// <summary>The <see cref="EnemyDefinitionId.MotherBrainHead"/> occupant.</summary>
    Head,
    /// <summary>The <see cref="EnemyDefinitionId.MotherBrainTubes"/> occupant.</summary>
    Tubes,
}
