
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The four falling ceiling-tube lists' entries in Mother Brain's fake-death sequence;
/// <see cref="MotherBrainTopTubeAnimation"/> plays them typed.
/// </summary>
internal abstract class MotherBrainTopTubeInstructionProgramDefinitions
{
    /// <summary>Top-right ceiling tube instruction list at $86:CC43.</summary>
    internal const ushort TopRight = 0xcc43;

    /// <summary>Top-left ceiling tube instruction list at $86:CC49.</summary>
    internal const ushort TopLeft = 0xcc49;

    /// <summary>Top-middle-left ceiling tube instruction list at $86:CC4F.</summary>
    internal const ushort TopMiddleLeft = 0xcc4f;

    /// <summary>Top-middle-right ceiling tube instruction list at $86:CC55.</summary>
    internal const ushort TopMiddleRight = 0xcc55;

    /// <summary>Native program bank $86.</summary>
    internal const byte Bank = 0x86;

}
