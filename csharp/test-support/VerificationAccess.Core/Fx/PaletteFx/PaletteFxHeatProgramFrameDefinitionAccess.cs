
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Verification access to <see cref="PaletteFxHeatProgramFrameDefinition"/> members production does not use.</summary>
internal static class PaletteFxHeatProgramFrameDefinitionAccess
{
    /// <summary>Finds the wait instruction following one compiled heat-palette frame.</summary>
    extension(PaletteFxHeatProgramFrameDefinition self)
    {
        /// <summary>The terminal wait command after fifteen live BGR555 colors.</summary>
        internal ushort WaitInstructionPointer => unchecked((ushort)(
            self.FirstColorPointer + PaletteFxHeatProgramDefinition.ColorsPerFrame * sizeof(ushort)));
    }
}
