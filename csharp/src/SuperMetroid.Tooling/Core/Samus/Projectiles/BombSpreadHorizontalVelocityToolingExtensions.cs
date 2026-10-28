using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Development-tool instance members of <see cref="BombSpreadHorizontalVelocity"/>.</summary>
internal static class BombSpreadHorizontalVelocityToolingExtensions
{
    /// <summary>Development-tool instance members of <see cref="BombSpreadHorizontalVelocity"/>.</summary>
    extension(BombSpreadHorizontalVelocity self)
    {
        /// <summary>The velocity in the launch definition's packed form, 0 through 65535, for traces that print it.</summary>
        public int LaunchWord =>
            LittleEndianBytes.Word(checked((byte)self.FractionSteps), checked((byte)(self.WholePixels + (self.MovingLeft ? BombSpreadHorizontalVelocity.LeftwardBit : 0))));
    }
}
