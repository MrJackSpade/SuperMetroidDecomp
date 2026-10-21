namespace SuperMetroid.Verification.Enemies;

/// <summary>The name, rows and colors of one enemy content identity fixture field.</summary>
/// <param name="Name">The name.</param>
/// <param name="Rows">The rows.</param>
/// <param name="Colors">The colors.</param>
internal readonly record struct EnemyContentIdentityFixtureField(string Name, int Rows, int Colors);
