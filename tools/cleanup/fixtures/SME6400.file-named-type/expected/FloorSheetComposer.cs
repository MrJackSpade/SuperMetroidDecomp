namespace Fixture;

/// <summary>Composes the floors.</summary>
public static class FloorSheetComposer
{
    /// <summary>The floors.</summary>
    public static IReadOnlyList<Floor> All { get; } = [new(1, sheet => sheet * 2)];
}
