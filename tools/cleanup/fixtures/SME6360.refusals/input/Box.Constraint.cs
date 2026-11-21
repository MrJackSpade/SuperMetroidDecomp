namespace Fixture;

public sealed partial class Box<[Marker] T>
    where T : class
{
    /// <summary>Whether the box is empty.</summary>
    public bool IsEmpty => Value is null;
}
