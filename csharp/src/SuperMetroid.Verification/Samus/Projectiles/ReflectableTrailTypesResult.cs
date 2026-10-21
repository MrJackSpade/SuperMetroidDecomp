namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>The type, data table and index of one reflectable trail types result.</summary>
/// <param name="Type">The type.</param>
/// <param name="DataTable">The data table.</param>
/// <param name="Index">The index.</param>
internal readonly record struct ReflectableTrailTypesResult(ushort Type, int DataTable, int Index);
