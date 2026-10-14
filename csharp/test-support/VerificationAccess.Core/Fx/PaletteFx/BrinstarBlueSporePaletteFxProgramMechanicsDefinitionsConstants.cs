
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="BrinstarBlueSporePaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class BrinstarBlueSporePaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>Frames from the first record through the next first record.</summary>
    public const int CycleFrames = 140;

    /// <summary>Provides the blue spore palette cycle period used to check the imported Brinstar effect.</summary>
    extension(BrinstarBlueSporePaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="BrinstarBlueSporePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => BrinstarBlueSporePaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
    }
}
