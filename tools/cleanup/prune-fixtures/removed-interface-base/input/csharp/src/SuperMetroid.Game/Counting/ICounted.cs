namespace SuperMetroid.Game.Counting;

/// <summary>A thing with a count, which nothing reads through this interface.</summary>
internal interface ICounted
{
    /// <summary>The count.</summary>
    int Count { get; }
}
