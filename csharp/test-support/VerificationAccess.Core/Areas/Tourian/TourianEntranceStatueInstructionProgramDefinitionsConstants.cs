
namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Cartridge values of <see cref="TourianEntranceStatueInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class TourianEntranceStatueInstructionProgramDefinitionsConstants
{
    /// <summary>First unused visible-loop list immediately after the live programs.</summary>
    public const ushort AdjacentUnusedProgram = 0xd7bb;

    /// <summary>Provides the adjacent unused program that bounds Tourian entrance statue instructions.</summary>
    extension(TourianEntranceStatueInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="TourianEntranceStatueInstructionProgramDefinitionsConstants.AdjacentUnusedProgram"/>
        internal static ushort AdjacentUnusedProgram => TourianEntranceStatueInstructionProgramDefinitionsConstants.AdjacentUnusedProgram;
    }
}
