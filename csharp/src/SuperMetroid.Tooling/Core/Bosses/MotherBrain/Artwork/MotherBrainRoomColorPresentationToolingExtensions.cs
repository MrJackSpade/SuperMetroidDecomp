namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Development-tool instance members of <see cref="MotherBrainRoomColorPresentation"/>.</summary>
internal static class MotherBrainRoomColorPresentationToolingExtensions
{
    /// <summary>Exposes the installed flash row identities for development audits without exposing palette colors.</summary>
    extension(MotherBrainRoomColorPresentation self)
    {
        /// <summary>Timed-entry identities installed by the validated flash rows; exposes no color payload.</summary>
        internal IEnumerable<ushort> FlashEntryPointers => Enumerable.Range(0, self.flash.FrameCount)
            .Select(index => checked((ushort)(MotherBrainRoomPaletteProgramDefinitions.FlashStart +
                index * MotherBrainRoomColorRomDataTooling.TimedEntryByteCount)));
    }
}
