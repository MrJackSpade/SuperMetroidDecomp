using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification's view of the import pipeline's population pointers (<see cref="RoomPlmPopulationSources"/>).</summary>
internal static class RoomPlmPopulationDefinitionsAccess
{
    /// <summary>Resolves and enumerates compiled populations by list pointer.</summary>
    extension(RoomPlmPopulationDefinitions)
    {
        /// <inheritdoc cref="RoomPlmPopulationSources.Population"/>
        internal static RoomPlmPopulationDefinition Get(ushort pointer) => RoomPlmPopulationSources.Population(pointer);

        /// <inheritdoc cref="RoomPlmPopulationSources.Contains"/>
        internal static bool Contains(ushort pointer) => RoomPlmPopulationSources.Contains(pointer);

        /// <inheritdoc cref="RoomPlmPopulationSources.Pointers"/>
        internal static IEnumerable<ushort> Pointers => RoomPlmPopulationSources.Pointers;
    }

    /// <summary>The list pointer of a compiled population.</summary>
    extension(RoomPlmPopulationDefinition self)
    {
        /// <summary>The bank-$8F list pointer of this population.</summary>
        internal ushort Pointer => RoomPlmPopulationSources.PointerOf(self);
    }

    /// <summary>The population pointer of a room state.</summary>
    extension(RoomStateSettings self)
    {
        /// <summary>The bank-$8F list pointer of this state's population.</summary>
        internal ushort PlmPointer => RoomPlmPopulationSources.PointerOf(self.PlmPopulation);
    }
}
