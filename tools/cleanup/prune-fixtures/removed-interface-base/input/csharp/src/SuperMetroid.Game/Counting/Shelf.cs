namespace SuperMetroid.Game.Counting;

/// <summary>A shelf, which production reads the name of through its interface.</summary>
internal sealed class Shelf : INamed
{
    /// <summary>The shelf's name.</summary>
    private readonly string name = "shelf";

    /// <summary>The shelf's name.</summary>
    public string Name => name;
}
