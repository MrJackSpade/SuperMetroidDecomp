using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Which Draygon occupant a slot holds, one per definition the species installs.</summary>
internal enum DraygonPart
{
    /// <summary>The <see cref="EnemyDefinitionId.DraygonArms"/> occupant.</summary>
    Arms,
    /// <summary>The <see cref="EnemyDefinitionId.DraygonBody"/> occupant.</summary>
    Body,
    /// <summary>The <see cref="EnemyDefinitionId.DraygonEye"/> occupant.</summary>
    Eye,
    /// <summary>The <see cref="EnemyDefinitionId.DraygonTail"/> occupant.</summary>
    Tail,
}
