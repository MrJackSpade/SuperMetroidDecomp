namespace SuperMetroid.Game.Counting;

/// <summary>A box, which lists only an interface nothing reads it through.</summary>
internal sealed class Box
{
    /// <summary>How big the box is.</summary>
    private readonly int size = 2;

    /// <summary>How big the box is.</summary>
    public int Size => size;
}
