using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>Cartridge values of <see cref="WreckedShipTreadmillRomData"/> that only verification reads.</summary>
internal static class WreckedShipTreadmillRomDataConstants
{
    /// <summary>Provides the native Wrecked Ship treadmill frame sources for tile animation checks.</summary>
    extension(WreckedShipTreadmillRomData)
    {
        /// <summary>The source of 32-byte graphics frame <paramref name="frame"/>: frame 0's, then one transfer further per frame.</summary>
        /// <param name="frame">The frame, 0 through 3.</param>
        internal static int FrameSource(int frame) => WreckedShipTreadmillRomData.Frame0Source + frame * WreckedShipTreadmillRomData.FrameByteCount;
    }
}
