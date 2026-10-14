
namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Cartridge values of <see cref="CeresDoorInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class CeresDoorInstructionProgramDefinitionsConstants
{
    /// <summary>
    /// <c>InstList_CeresDoor_Normal_FacingLeft_3</c> at $A6:F5EA
    /// makes the door tangible and visible before its closed wait.
    /// </summary>
    public const ushort ClosedFacingLeft = 0xf5ea;

    /// <summary>Provides the native left-facing closed door program used to verify Ceres door animation.</summary>
    extension(CeresDoorInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="CeresDoorInstructionProgramDefinitionsConstants.ClosedFacingLeft"/>
        internal static ushort ClosedFacingLeft => CeresDoorInstructionProgramDefinitionsConstants.ClosedFacingLeft;
    }
}
