
namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>Cartridge values of <see cref="EvirInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class EvirInstructionProgramDefinitionsConstants
{
    /// <summary>First native Evir instruction callback at $A8:878F.</summary>
    public const ushort AdjacentCallbackCode = 0x878f;

    /// <summary>Provides the callback-code boundary adjacent to Evir instruction programs.</summary>
    extension(EvirInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="EvirInstructionProgramDefinitionsConstants.AdjacentCallbackCode"/>
        internal static ushort AdjacentCallbackCode => EvirInstructionProgramDefinitionsConstants.AdjacentCallbackCode;
    }
}
