namespace Fixture;

/// <summary>A box whose other part states its constraint and marks its type parameter.</summary>
/// <typeparam name="T">The kind of value.</typeparam>
public sealed partial class Box<T>
{
    /// <summary>The value.</summary>
    public T? Value { get; set; }
}
