namespace Engine.Shared;

/// <summary>What can be asked of a <see cref="Facing"/>.</summary>
public static class FacingExtensions
{
    /// <summary>The other way.</summary>
    public static Facing Opposite(this Facing facing) => facing switch
    {
        Facing.Left => Facing.Right,
        Facing.Right => Facing.Left,
        _ => throw new ArgumentOutOfRangeException(nameof(facing), facing, "Not a facing."),
    };

    /// <summary>A distance taken this way: unchanged rightward, reversed leftward.</summary>
    public static Pixels Toward(this Facing facing, Pixels distance) => facing switch
    {
        Facing.Left => -distance,
        Facing.Right => distance,
        _ => throw new ArgumentOutOfRangeException(nameof(facing), facing, "Not a facing."),
    };
}
