namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// How the posture-transition weapon admission treats a pose (#1517): whether it admits Samus's weapons outright, never
/// does, answers from its transition table, or answers from what it reads beyond that table.
/// </summary>
internal enum SamusPostureTransitionAdmission
{
    /// <summary>The aimed crouch and stand transitions: the standard weapon admission admits them.</summary>
    AlwaysAdmits,

    /// <summary>The landings, drained bodies and right-facing Draygon holds, and the compact transitions: never admitted.</summary>
    NeverAdmits,

    /// <summary>A crouch or stand transition the transition table admits.</summary>
    TableAdmits,

    /// <summary>A morph or unmorph transition the transition table answers by preserving the charge.</summary>
    TablePreservesCharge,

    /// <summary>
    /// Any other pose: the admission reads beyond its table, and
    /// <see cref="Compatibility.PostureAdmissionBeyondTransitions{TPose}"/> holds the answer.
    /// </summary>
    BeyondTable,
}
