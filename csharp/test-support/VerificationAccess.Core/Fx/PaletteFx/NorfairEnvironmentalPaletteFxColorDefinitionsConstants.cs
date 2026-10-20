
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="NorfairEnvironmentalPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class NorfairEnvironmentalPaletteFxColorDefinitionsConstants
{
    /// <summary>The shared complete-cycle duration.</summary>
    public const int CycleFrames = 116;

    /// <summary>Provides the native Norfair environmental palette cycle period for effect comparisons.</summary>
    extension(NorfairEnvironmentalPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="NorfairEnvironmentalPaletteFxColorDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => NorfairEnvironmentalPaletteFxColorDefinitionsConstants.CycleFrames;
    }
    /// <summary>Shared duration for one bounded Norfair palette phase.</summary>
    /// <remarks>
    /// For i=0..15, let d=min(i,15-i). The duration is 16 when d=0;
    /// otherwise min(8,max(4,d+2)). This gives the exact symmetric
    /// 16,4,4,5,6,7,8,8,8,8,7,6,5,4,4,16 ROM schedule, totaling 116 frames.
    /// </remarks>
    internal static ushort Duration(int frame)
    {
        if ((uint)frame >= NorfairEnvironmentalPaletteFxColorDefinitions.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        int distance = Math.Min(frame, NorfairEnvironmentalPaletteFxColorDefinitions.FrameCount - 1 - frame);
        return (ushort)(distance == 0 ? 16 : Math.Clamp(distance + 2, 4, 8));
    }
}
