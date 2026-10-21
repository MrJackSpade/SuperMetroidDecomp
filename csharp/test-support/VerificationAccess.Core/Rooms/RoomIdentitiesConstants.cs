using SuperMetroid.Core.Progression;


namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge values of <see cref="RoomIdentities"/> that only verification reads.</summary>
internal static class RoomIdentitiesConstants
{
    /// <summary>Crateria's Landing Site, room pair <c>$00/$00</c>.</summary>
    public static readonly RoomIdentity LandingSite = new(AreaId.Crateria, 0x00);

    /// <summary>Provides the Landing Site room identity used to verify the imported starting room.</summary>
    extension(RoomIdentities)
    {
        /// <inheritdoc cref="RoomIdentitiesConstants.LandingSite"/>
        internal static RoomIdentity LandingSite => RoomIdentitiesConstants.LandingSite;
    }
}
