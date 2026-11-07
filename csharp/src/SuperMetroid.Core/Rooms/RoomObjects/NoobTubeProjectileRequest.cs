namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One room-graphics enemy-projectile spawn emitted by the n00b-tube PLM.</summary>
public readonly record struct NoobTubeProjectileRequest(
    NoobTubeProjectileDefinition Definition,
    int Parameter,
    int RoomObjectBlockIndex)
{
    /// <summary>The definition-specific operand: zero for a crack, else an even byte offset into its table, 0 through 65535.</summary>
    public int Parameter { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, Parameter);
}
