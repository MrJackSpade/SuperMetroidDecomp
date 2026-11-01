namespace SuperMetroid.Core.Samus;

/// <summary>Inspectable publication from one reserve-refill call.</summary>
[IndependentFlag("independent outcomes of one recovery step: a refill sound, and completion")]
public readonly record struct SamusReserveAutoRecoveryStep(
    bool RefillSoundRequested,
    bool Completed);
