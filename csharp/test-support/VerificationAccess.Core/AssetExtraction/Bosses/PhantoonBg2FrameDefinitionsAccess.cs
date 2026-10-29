using SuperMetroid.Core.Bosses.Phantoon.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Cartridge values of <see cref="PhantoonBg2FrameDefinitions"/> that only verification reads.</summary>
internal static class PhantoonBg2FrameDefinitionsAccess
{
    /// <summary>Provides Phantoon BG2 frame bases in VRAM and working RAM for animation checks.</summary>
    extension(PhantoonBg2FrameDefinitions)
    {
        /// <inheritdoc cref="PhantoonBg2FrameDefinitionsConstants.VramBase"/>
        internal static ushort VramBase => PhantoonBg2FrameDefinitionsConstants.VramBase;
        /// <inheritdoc cref="PhantoonBg2FrameDefinitionsConstants.WorkingRamBase"/>
        internal static ushort WorkingRamBase => PhantoonBg2FrameDefinitionsConstants.WorkingRamBase;
    }
}
