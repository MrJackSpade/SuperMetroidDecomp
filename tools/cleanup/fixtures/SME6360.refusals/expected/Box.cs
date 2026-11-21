namespace Fixture;

/// <summary>A box whose other part states its constraint and marks its type parameter.</summary>
/// <typeparam name="T">The kind of value.</typeparam>
public sealed class Box<[Marker] T> where T : class
{
    /// <summary>Whether the box is empty.</summary>
    public bool IsEmpty => Value is null;

    /// <summary>The value.</summary>
    public T? Value { get; set; }
}
