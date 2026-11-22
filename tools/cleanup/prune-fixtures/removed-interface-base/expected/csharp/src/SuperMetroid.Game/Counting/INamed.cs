namespace SuperMetroid.Game.Counting;

/// <summary>A thing with a name, which production reads through this interface.</summary>
internal interface INamed
{
    /// <summary>The name.</summary>
    string Name { get; }
}
