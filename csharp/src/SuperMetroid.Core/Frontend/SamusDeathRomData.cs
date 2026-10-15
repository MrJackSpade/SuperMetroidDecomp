namespace SuperMetroid.Core.Frontend;

/// <summary>Cartridge data the frontend uses when Samus dies.</summary>
public static class SamusDeathRomData
{
    /// <summary>The death music data set and track, loaded after every sound and the current music stop.</summary>
    public static class Music
    {
        /// <summary>Music data-set selector $39, loaded through a delayed-eight command.</summary>
        public const byte DataIndex = 0x39;
        /// <summary>Track selector five, the death fanfare.</summary>
        public const byte TrackIndex = 5;
    }
}
