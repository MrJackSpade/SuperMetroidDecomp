namespace SuperMetroid.Core.Rooms;

/// <summary>One native room-graphics enemy-projectile spawn emitted by PLM $D70C.</summary>
public readonly record struct NoobTubeProjectileRequest(
    NoobTubeProjectileDefinition DefinitionPointer,
    ushort Parameter,
    int PlmBlockIndex);
