using SuperMetroid.Core;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// Writes the elements of an array of the game's value types as one packed run (#1696): each element's fields back to
/// back with no marker, type name or field name, which the run's schema names once; an enum member as its place in the
/// run's table of the enum.
/// </summary>
/// <param name="output">The run's values.</param>
/// <param name="types">The registry.</param>
/// <param name="tables">The run's enum tables.</param>
/// <param name="objects">The run's table of the objects its elements hold.</param>
internal sealed class PackedWriter(BinaryWriter output, StateGraphTypes types, PackedEnumTables tables, PackedObjects objects) : IStateGraphWriter
{
}
