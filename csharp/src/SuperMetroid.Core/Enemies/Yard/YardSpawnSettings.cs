namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Which of its eight crawling directions a Yard starts in (#1471).</summary>
/// <param name="Direction">The starting direction's position among the Yard's eight directions.</param>
public sealed record YardSpawnSettings(int Direction) : EnemySpawnSettings;
