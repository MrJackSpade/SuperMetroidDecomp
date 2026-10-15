namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The growing, vertical and horizontal shutters' poses and Kamer's platform loop as typed programs (#641), one per bank-$A2 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="ShutterAnimations"/>.
/// </summary>
internal enum ShutterProgram
{
    /// <summary><c>InstructionList_ShutterGrowing_10px</c> at $A2:E998.</summary>
    GrowingTenPixels,
    /// <summary><c>InstructionList_ShutterGrowing_20px</c> at $A2:E99E.</summary>
    GrowingTwentyPixels,
    /// <summary><c>InstructionList_ShutterGrowing_30px</c> at $A2:E9A4.</summary>
    GrowingThirtyPixels,
    /// <summary><c>InstructionList_ShutterGrowing_40px</c> at $A2:E9AA.</summary>
    FortyPixels,
    /// <summary><c>InstList_ShutterHorizontal</c> at $A2:E9D4.</summary>
    Horizontal,
    /// <summary><c>InstructionList_KamerPlatform</c> at $A2:EDE7.</summary>
    KamerPlatform,
}
