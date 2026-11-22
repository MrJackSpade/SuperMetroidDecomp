namespace SuperMetroid.Game.Counting;

/// <summary>A basket, which lists an interface production reads it through and one nothing does.</summary>
internal sealed class Basket : INamed
{
    /// <summary>How many things the basket holds.</summary>
    private readonly int count = 3;

    /// <summary>How many things the basket holds.</summary>
    public int Count => count;

    /// <summary>The basket's name.</summary>
    public string Name => "basket";
}
