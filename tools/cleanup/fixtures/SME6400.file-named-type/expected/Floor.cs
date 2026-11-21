namespace Fixture;

/// <summary>A floor sheet and how it composes.</summary>
/// <param name="Sheet">The sheet.</param>
/// <param name="Compose">How the sheet composes.</param>
public sealed record Floor(int Sheet, Func<int, int> Compose);
