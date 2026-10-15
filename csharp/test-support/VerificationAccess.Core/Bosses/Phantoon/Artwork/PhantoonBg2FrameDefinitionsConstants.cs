namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

/// <summary>Cartridge values of <see cref="PhantoonBg2FrameDefinitions"/> that only verification reads.</summary>
internal static class PhantoonBg2FrameDefinitionsConstants
{
    /// <summary>VRAM destination base for Phantoon's BG2 frame tiles.</summary>
    public const ushort VramBase = EnemyBg2FrameLayout.VramBase;
    /// <summary>WRAM staging base from which Phantoon's BG2 frame upload is copied.</summary>
    public const ushort WorkingRamBase = EnemyBg2FrameLayout.WorkingRamBase;

    /// <summary>Provides Phantoon BG2 frame bases in VRAM and working RAM for animation checks.</summary>
    extension(PhantoonBg2FrameDefinitions)
    {
        /// <inheritdoc cref="PhantoonBg2FrameDefinitionsConstants.VramBase"/>
        internal static ushort VramBase => PhantoonBg2FrameDefinitionsConstants.VramBase;
        /// <inheritdoc cref="PhantoonBg2FrameDefinitionsConstants.WorkingRamBase"/>
        internal static ushort WorkingRamBase => PhantoonBg2FrameDefinitionsConstants.WorkingRamBase;
    }
}
