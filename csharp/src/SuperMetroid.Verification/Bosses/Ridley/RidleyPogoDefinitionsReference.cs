namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>The x, y, up and down of one ridley pogo definitions reference.</summary>
/// <param name="X">The x.</param>
/// <param name="Y">The y.</param>
/// <param name="Up">The up.</param>
/// <param name="Down">The down.</param>
internal readonly record struct RidleyPogoDefinitionsReference(ushort X, ushort Y, ushort Up, ushort Down);
