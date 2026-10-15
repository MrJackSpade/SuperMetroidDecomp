
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Verification access to <see cref="DachoraColorRomData"/> members production does not use.</summary>
internal static class DachoraColorRomDataAccess
{
    /// <summary>Resolves Dachora palette source addresses by phase and animation frame.</summary>
    extension(DachoraColorRomData)
    {
        /// <summary>Returns the authored color source for a cartridge-selected phase/frame.</summary>
        internal static int Source(DachoraPalettePhase phase, int frame) => phase switch
        {
            DachoraPalettePhase.Default when frame == 0 => DachoraColorRomData.DefaultSource,
            DachoraPalettePhase.Speed when (uint)frame < DachoraColorRomData.AnimatedFrameCount =>
                DachoraColorRomData.SpeedSource + frame * DachoraColorRomData.FrameByteCount,
            DachoraPalettePhase.Shine when (uint)frame < DachoraColorRomData.AnimatedFrameCount =>
                DachoraColorRomData.ShineSource + frame * DachoraColorRomData.FrameByteCount,
            _ => throw new ArgumentOutOfRangeException(nameof(frame),
                $"Dachora phase {phase} has no frame {frame}."),
        };
    }
}
