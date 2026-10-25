using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Verification access to <see cref="RoomFxBlendColors"/> members production does not use.</summary>
internal static class RoomFxBlendColorsAccess
{
    /// <summary>Expands pair colors and the optional third color into a fresh output array.</summary>
    extension(RoomFxBlendColors self)
    {
        /// <summary>Creates requested blend colors on demand; no stock-color array is retained.</summary>
        internal RgbColor[] CreateColors() => [self.primary.CreateColor(), self.secondary.CreateColor(), self.thirdOverride?.CreateColor() ?? RgbColor.Black];
    }
}
