namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One native room-graphics enemy-projectile spawn emitted by PLM $D70C.</summary>
public readonly record struct NoobTubeProjectileRequest(
    NoobTubeProjectileDefinition DefinitionPointer,
    int Parameter,
    int PlmBlockIndex)
{
    /// <summary>The definition-specific operand: zero for a crack, else an even byte offset into its table, 0 through 65535.</summary>
    public int Parameter { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, Parameter);
}
