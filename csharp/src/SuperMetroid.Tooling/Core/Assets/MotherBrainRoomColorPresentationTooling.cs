using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Development-tool instance members of <see cref="MotherBrainRoomColorPresentation"/>.</summary>
internal static class MotherBrainRoomColorPresentationToolingExtensions
{
    /// <summary>Exposes the validated flash-entry pointers of a room-color presentation.</summary>
    extension(MotherBrainRoomColorPresentation self)
    {
        /// <summary>Timed-entry identities installed by the validated flash rows; exposes no color payload.</summary>
        internal IEnumerable<ushort> FlashEntryPointers => Enumerable.Range(0, self.flash.FrameCount)
            .Select(index => checked((ushort)(MotherBrainRoomPaletteProgramDefinitions.FlashStart +
                index * MotherBrainRoomColorRomData.TimedEntryByteCount)));
    }
}
