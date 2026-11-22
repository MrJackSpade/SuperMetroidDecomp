namespace SuperMetroid.Game.Counting;

/// <summary>A thing with a size, which nothing reads through this interface.</summary>
internal interface ISized
{
    /// <summary>The size.</summary>
    int Size { get; }
}
