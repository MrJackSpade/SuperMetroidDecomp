namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Verification view of one stored loading-palette channel input.</summary>
internal static class LoadingPaletteChannelsAccess
{
    /// <summary>Reads the stored channel inputs.</summary>
    extension(LoadingPaletteInputView.Channels self)
    {
        /// <summary>The stored red (0), green (1) or blue (2) input; null when that channel is calculated.</summary>
        internal int? Component(int channel) => channel switch
        {
            0 => self.red,
            1 => self.green,
            2 => self.blue,
            _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "A palette channel is 0..2."),
        };
    }
}
