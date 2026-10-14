
namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>Cartridge values of <see cref="PowampInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class PowampInstructionProgramDefinitionsConstants
{
    /// <summary>The terminal common sleep word at $A8:C19D.</summary>
    public const ushort BalloonDeflatedSleep = 0xc19d;
    /// <summary><c>InstList_Powamp_Balloon_Inflate_2</c> at $A8:C18B.</summary>
    public const ushort BalloonInflate2 = 0xc18b;
    /// <summary>The first non-program word after Powamp's instruction streams, at $A8:C19F.</summary>
    public const ushort FirstAdjacentConstant = 0xc19f;

    /// <summary>Provides Powamp deflated and inflation programs and the adjacent constant boundary.</summary>
    extension(PowampInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="PowampInstructionProgramDefinitionsConstants.BalloonDeflatedSleep"/>
        internal static ushort BalloonDeflatedSleep => PowampInstructionProgramDefinitionsConstants.BalloonDeflatedSleep;
        /// <inheritdoc cref="PowampInstructionProgramDefinitionsConstants.BalloonInflate2"/>
        internal static ushort BalloonInflate2 => PowampInstructionProgramDefinitionsConstants.BalloonInflate2;
        /// <inheritdoc cref="PowampInstructionProgramDefinitionsConstants.FirstAdjacentConstant"/>
        internal static ushort FirstAdjacentConstant => PowampInstructionProgramDefinitionsConstants.FirstAdjacentConstant;
    }
}
