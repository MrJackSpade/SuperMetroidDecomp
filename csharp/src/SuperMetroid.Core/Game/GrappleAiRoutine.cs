namespace SuperMetroid.Core.Game;

/// <summary>
/// Header +$1A grapple-AI entry points, relative to the enemy's own bank. Every retail
/// enemy selects one of the seven shared bank-$A0 reactions except the elevator, whose
/// main AI doubles as its grapple AI.
/// </summary>
public enum GrappleAiRoutine : ushort
{
    /// <summary>No definition: the zero word of an empty enemy slot's default header.</summary>
    None = 0,

    /// <summary><c>Enemy_GrappleAI_NoInteraction</c> at $A0:8000.</summary>
    NoInteraction = 0x8000,

    /// <summary><c>Enemy_GrappleAI_SamusLatchesOn</c> at $A0:8005.</summary>
    Attach = 0x8005,

    /// <summary><c>Enemy_GrappleAI_KillEnemy</c> at $A0:800A.</summary>
    Kill = 0x800a,

    /// <summary><c>Enemy_GrappleAI_CancelBeam</c> at $A0:800F.</summary>
    Cancel = 0x800f,

    /// <summary><c>Enemy_GrappleAI_LatchWithoutInvincibility</c> at $A0:8014.</summary>
    AttachWithoutInvincibility = 0x8014,

    /// <summary><c>Enemy_GrappleAI_LatchAndParalyse</c> at $A0:8019.</summary>
    AttachAndParalyze = 0x8019,

    /// <summary><c>Enemy_GrappleAI_HurtSamus</c> at $A0:801E.</summary>
    HurtSamus = 0x801e,

    /// <summary><c>MainAI_GrappleAI_FrozenAI_Elevator</c> at $A3:952A, the elevator's main AI.</summary>
    ElevatorMainAi = 0x952a,
}
