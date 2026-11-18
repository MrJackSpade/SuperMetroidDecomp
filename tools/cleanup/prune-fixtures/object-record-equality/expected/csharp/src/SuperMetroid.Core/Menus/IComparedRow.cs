namespace SuperMetroid.Core.Menus;

/// <summary>A row the host holds by this interface and compares with <see cref="object.Equals(object)"/>.</summary>
public interface IComparedRow
{
    /// <summary>The page the row is on.</summary>
    int Page { get; }
}
