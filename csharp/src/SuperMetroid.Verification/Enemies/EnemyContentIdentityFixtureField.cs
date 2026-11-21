namespace SuperMetroid.Verification.Enemies;

/// <summary>The name, rows and colors of one enemy content identity fixture field.</summary>
/// <param name="Name">The name.</param>
/// <param name="Rows">The rows.</param>
/// <param name="Colors">The colors.</param>
/// <param name="Keys">
/// What each row colours, by the key the installed document names it with, for a field whose rows are keyed (#2554); none
/// for a field whose rows are listed in order.
/// </param>
/// <param name="Members">
/// The members each row names its colours under, for a field whose rows are objects of several palettes (#2554); none for
/// a field whose rows are palettes.
/// </param>
internal readonly record struct EnemyContentIdentityFixtureField(string Name, int Rows, int Colors, IReadOnlyList<string>? Keys = null,
    IReadOnlyList<string>? Members = null);
