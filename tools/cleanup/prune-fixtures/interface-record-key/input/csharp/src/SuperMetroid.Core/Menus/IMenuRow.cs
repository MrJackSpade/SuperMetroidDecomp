namespace SuperMetroid.Core.Menus;

/// <summary>A row a menu's cursor can be on, which the pages' row records implement.</summary>
public interface IMenuRow
{
    /// <summary>The page the row is on.</summary>
    int Page { get; }
}
