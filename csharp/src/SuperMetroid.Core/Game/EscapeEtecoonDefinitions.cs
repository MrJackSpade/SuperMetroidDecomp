namespace SuperMetroid.Core.Game;

/// <summary>
/// Mutually exclusive authored roles selected by the escape Etecoon's even parameter-one
/// byte offset. Odd restored values select the same role after the cartridge clears bit zero.
/// </summary>
public enum EscapeEtecoonRole : ushort
{
    /// <summary>Native table byte selector $0000 at $B3:E718: starts at room pixel (128, 200), uses running-left list $B3:E556 and walking/falling function $E680, with signed 8.8 X velocity $FE00 (-2 pixels per update).</summary>
    LeftWalker = 0,
    /// <summary>Native table byte selector $0002 at $B3:E71A: starts at room pixel (160, 200), uses running-right list $B3:E582 and walking/falling function $E680, with signed 8.8 X velocity $0280 (+2.5 pixels per update).</summary>
    RightWalker = 2,
    /// <summary>Native table byte selector $0004 at $B3:E71C: starts stationary at room pixel (232, 200), list $B3:E5C6/function $E670, and waits for persistent event $0F before selecting gratitude-then-escape bytecode.</summary>
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
