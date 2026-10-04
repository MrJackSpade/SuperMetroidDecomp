namespace SuperMetroid.Core.Game;

/// <summary>
/// Mutually exclusive authored roles selected by the escape Etecoon's even parameter-one
/// byte offset. Odd restored values select the same role after the cartridge clears bit zero.
/// </summary>
public enum EscapeEtecoonRole : ushort
{
    LeftWalker = 0,
    RightWalker = 2,
    WaitForEscapeEvent = 4,
}

/// <summary>Fixed initialization definitions for the three escape Etecoon actors.</summary>
internal static class EscapeEtecoonDefinitions
{
    /// <summary>
    /// $B3:E718..E735 InitializationAI_EtecoonEscape: left walker, right walker
    /// and event-waiting role select position, action, program and horizontal speed.
    /// The cartridge clears parameter bit zero before dispatching.
    /// </summary>
    internal static EscapeEtecoonInitialization Initialization(ushort parameter1) =>
        (EscapeEtecoonRole)(parameter1 & 0xfffe) switch
        {
            EscapeEtecoonRole.LeftWalker => new(128, 200, EscapeEtecoonPreInstruction.WalkAndFall,
                EscapeEtecoonInstructionProgramDefinitions.RunningLeftLowTide, unchecked((ushort)-512)),
            EscapeEtecoonRole.RightWalker => new(160, 200, EscapeEtecoonPreInstruction.WalkAndFall,
                EscapeEtecoonInstructionProgramDefinitions.RunningRightLowTide, 640),
            EscapeEtecoonRole.WaitForEscapeEvent => new(232, 200, EscapeEtecoonPreInstruction.WaitForEscapeEvent,
                EscapeEtecoonInstructionProgramDefinitions.Stationary, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(parameter1), parameter1,
                "Escape Etecoon parameter must select one of the three authored roles."),
        };
}

/// <summary>One escape Etecoon's initial position, behavior, animation, and speed.</summary>
internal readonly record struct EscapeEtecoonInitialization(
    ushort XPosition,
    ushort YPosition,
    EscapeEtecoonPreInstruction PreInstruction,
    ushort InstructionList,
    ushort HorizontalSpeed);
