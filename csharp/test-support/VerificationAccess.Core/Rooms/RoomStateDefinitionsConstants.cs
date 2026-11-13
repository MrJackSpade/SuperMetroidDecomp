using SuperMetroid.AssetExtraction.World.RoomStates;

namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge values of <see cref="RoomStateDefinitions"/> that only verification reads.</summary>
internal static class RoomStateDefinitionsConstants
{
    /// <summary>Number of distinct room states selected by the 261 retail rooms.</summary>
    public const int RetailStateCount = 322;

    /// <summary>Provides the retail room-state count for imported state completeness checks.</summary>
    extension(RoomStateDefinitions)
    {
        /// <inheritdoc cref="RoomStateDefinitionsConstants.RetailStateCount"/>
        internal static int RetailStateCount => RoomStateDefinitionsConstants.RetailStateCount;
    }
}
