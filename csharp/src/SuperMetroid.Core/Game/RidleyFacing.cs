namespace SuperMetroid.Core.Game;

/// <summary>Norfair Ridley's facing as the arena-target tables index it.</summary>
public enum RidleyFacing : ushort
{
    /// <summary>Native facing index zero, left-facing Ridley.</summary>
    Left = 0,
    /// <summary>Native facing index one, intermediate turning pose.</summary>
    Turning = 1,
    /// <summary>Native facing index two, right-facing Ridley.</summary>
    Right = 2,
}
