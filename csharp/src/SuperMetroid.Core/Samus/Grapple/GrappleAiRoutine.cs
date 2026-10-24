namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>
/// Header +$1A grapple-AI entry points, relative to the enemy's own bank. Every retail
/// enemy selects one of the seven shared bank-$A0 reactions except the elevator, whose
/// main AI doubles as its grapple AI.
/// </summary>
public enum GrappleAiRoutine : ushort
{
    /// <summary>No definition: the zero word of an empty enemy slot's default header.</summary>
    None,

    /// <summary><c>Enemy_GrappleAI_NoInteraction</c> at $A0:8000.</summary>
    NoInteraction,

    /// <summary><c>Enemy_GrappleAI_SamusLatchesOn</c> at $A0:8005.</summary>
    Attach,

    /// <summary><c>Enemy_GrappleAI_KillEnemy</c> at $A0:800A.</summary>
    Kill,

    /// <summary><c>Enemy_GrappleAI_CancelBeam</c> at $A0:800F.</summary>
    Cancel,

    /// <summary><c>Enemy_GrappleAI_LatchWithoutInvincibility</c> at $A0:8014.</summary>
    AttachWithoutInvincibility,

    /// <summary><c>Enemy_GrappleAI_LatchAndParalyse</c> at $A0:8019.</summary>
    AttachAndParalyze,

    /// <summary><c>Enemy_GrappleAI_HurtSamus</c> at $A0:801E.</summary>
    HurtSamus,

    /// <summary><c>MainAI_GrappleAI_FrozenAI_Elevator</c> at $A3:952A, the elevator's main AI.</summary>
    ElevatorMainAi,
}
