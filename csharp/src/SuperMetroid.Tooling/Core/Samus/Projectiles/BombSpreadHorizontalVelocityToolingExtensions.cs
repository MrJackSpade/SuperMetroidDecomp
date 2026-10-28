using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Development-tool instance members of <see cref="BombSpreadHorizontalVelocity"/>.</summary>
internal static class BombSpreadHorizontalVelocityToolingExtensions
{
    /// <summary>The high-byte bit of the packed launch word that sends the bomb left.</summary>
    internal const int LeftwardBit = 128;

    /// <summary>Development-tool instance members of <see cref="BombSpreadHorizontalVelocity"/>.</summary>
    extension(BombSpreadHorizontalVelocity self)
    {

        public int LaunchWord =>
            LittleEndianBytes.Word(checked((byte)self.FractionSteps), checked((byte)(self.WholePixels + (self.MovingLeft ? LeftwardBit : 0))));
    }
}
