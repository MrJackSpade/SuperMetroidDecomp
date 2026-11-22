using SuperMetroid.Core;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>Reads the elements of a packed run back (#1696), in the order <see cref="PackedWriter"/> wrote them.</summary>
/// <param name="input">The run's values.</param>
/// <param name="types">The registry.</param>
/// <param name="tables">The run's enum tables.</param>
/// <param name="objects">The run's table of the objects its elements hold.</param>
internal sealed class PackedReader(BinaryReader input, StateGraphTypes types, PackedEnumTables tables, PackedObjects objects) : IStateGraphReader
{
}
