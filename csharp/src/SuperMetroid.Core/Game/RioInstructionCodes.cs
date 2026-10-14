namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$A2 instruction opcodes used by Rio.</summary>
internal static class RioInstructionCodes
{
    /// <summary>Marks Rio's current animation complete at $A2:BBC3.</summary>
    public const ushort SetAnimationFinished = 0xbbc3;
}
