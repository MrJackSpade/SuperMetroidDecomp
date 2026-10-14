
namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Verification access to <see cref="CeresDoorInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class CeresDoorInstructionProgramDefinitionsAccess
{
    /// <summary>Reads the visual-frame operand paired with a Ceres door instruction word.</summary>
    extension(CeresDoorInstructionProgramDefinitions)
    {
        /// <summary>Reads the frame half of a Ceres door presentation pair.</summary>
        internal static ushort PresentationWordFrame(int index) => (((ushort Address, ushort Frame))(CeresDoorInstructionProgramDefinitions.PresentationWord((int)(index)))).Frame;
    }
}
