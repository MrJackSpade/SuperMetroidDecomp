namespace Fixture;

/// <summary>A floor sheet and how it composes.</summary>
/// <param name="Sheet">The sheet.</param>
/// <param name="Compose">How the sheet composes.</param>
public sealed record Floor(int Sheet, Func<int, int> Compose);

/// <summary>Composes the floors.</summary>
public static class FloorSheetComposer
{
    /// <summary>The floors.</summary>
    public static IReadOnlyList<Floor> All { get; } = [new(1, sheet => sheet * 2)];
}
