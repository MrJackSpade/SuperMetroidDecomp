namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Cartridge values of <see cref="LoadStationRomData"/> that only verification reads.</summary>
internal static class LoadStationRomDataConstants
{
    /// <summary>$80:CC19, exclusive end of the Ceres list.</summary>
    public const int DataEnd = 0x80cc19;
    /// <summary>$80:C4B5, seven area pointers followed by the end pointer for the Ceres list.</summary>
    public const int PointerTable = 0x80c4b5;

    /// <summary>Provides the load-station pointer table and data end used to decode native save station records.</summary>
    extension(LoadStationRomData)
    {
        /// <inheritdoc cref="LoadStationRomDataConstants.DataEnd"/>
        internal static int DataEnd => LoadStationRomDataConstants.DataEnd;
        /// <inheritdoc cref="LoadStationRomDataConstants.PointerTable"/>
        internal static int PointerTable => LoadStationRomDataConstants.PointerTable;
    }
}
