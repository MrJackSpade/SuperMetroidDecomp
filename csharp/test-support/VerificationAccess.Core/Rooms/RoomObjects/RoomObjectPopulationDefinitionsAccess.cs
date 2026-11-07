using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification's view of the import pipeline's population pointers (<see cref="RoomObjectPopulationSources"/>).</summary>
internal static class RoomObjectPopulationDefinitionsAccess
{
    /// <summary>Resolves and enumerates compiled populations by list pointer.</summary>
    extension(RoomObjectPopulationDefinitions)
    {
        /// <inheritdoc cref="RoomObjectPopulationSources.Population"/>
        internal static RoomObjectPopulation Get(ushort pointer) => RoomObjectPopulationSources.Population(pointer);

        /// <inheritdoc cref="RoomObjectPopulationSources.Contains"/>
        internal static bool Contains(ushort pointer) => RoomObjectPopulationSources.Contains(pointer);

        /// <inheritdoc cref="RoomObjectPopulationSources.Pointers"/>
        internal static IEnumerable<ushort> Pointers => RoomObjectPopulationSources.Pointers;
    }

    /// <summary>The list pointer of a compiled population.</summary>
    extension(RoomObjectPopulation self)
    {
        /// <summary>The bank-$8F list pointer of this population.</summary>
        internal ushort Pointer => RoomObjectPopulationSources.PointerOf(self);
    }

    /// <summary>The population pointer of a room state.</summary>
    extension(RoomStateSettings self)
    {
        /// <summary>The bank-$8F list pointer of this state's population.</summary>
        internal ushort RoomObjectPointer => RoomObjectPopulationSources.PointerOf(self.ObjectPopulation);
    }
}
