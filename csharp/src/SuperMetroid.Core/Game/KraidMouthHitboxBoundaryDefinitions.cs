namespace SuperMetroid.Core.Game;

/// <summary>Instruction identities visible at the bounded Kraid hitbox low-half alias.</summary>
internal static class KraidMouthHitboxBoundaryDefinitions
{
    /// <summary>$A0:9F6D, GrappleAI_SwitchEnemyAIToMainAI, target of $A7:8000's JSL.</summary>
    internal const int ResumeMainAiTarget = 0xa09f6d;
    /// <summary>$A0:9F7D, GrappleAI_SamusLatchesOnWithGrapple, target of $A7:8005's JSL.</summary>
    internal const int GrappleLatchTarget = 0xa09f7d;
    /// <summary>65816 JSL long-address opcode at $A7:8000 and $A7:8005.</summary>
    internal const byte LongCallOpcode = 0x22;
    /// <summary>65816 RTL opcode at $A7:8004, ending the no-interaction grapple stub.</summary>
    internal const byte LongReturnOpcode = 0x6b;

}
