using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The import pipeline's map between the bank-$8F population pointers room states store and the compiled populations
/// they address; the simulation knows only the named populations.
/// </summary>
public static class RoomPlmPopulationSources
{
    /// <summary>Every compiled population pointer, in ascending order.</summary>
    public static IEnumerable<ushort> Pointers => RoomPlmPopulationSourceDefinitions.ByPointer.Keys.Order();

    /// <summary>The compiled population whose list is at <paramref name="pointer"/>.</summary>
    public static RoomPlmPopulationDefinition Population(ushort pointer) =>
        RoomPlmPopulationSourceDefinitions.ByPointer.TryGetValue(pointer, out RoomPlmPopulationDefinition? found)
            ? found
            : throw new ArgumentOutOfRangeException(nameof(pointer), pointer, "Pointer is not one of the 284 retail room PLM populations.");

    /// <summary>Whether <paramref name="pointer"/> is exactly one of the compiled populations.</summary>
    public static bool Contains(ushort pointer) => RoomPlmPopulationSourceDefinitions.ByPointer.ContainsKey(pointer);

    /// <summary>The list pointer of <paramref name="population"/>.</summary>
    public static ushort PointerOf(RoomPlmPopulationDefinition population)
    {
        Ensure.NotNull(population);
        return RoomPlmPopulationSourceDefinitions.ByPointer.Single(pair => ReferenceEquals(pair.Value, population)).Key;
    }
}
