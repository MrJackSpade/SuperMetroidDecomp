namespace SuperMetroid.Core.Samus;

/// <summary>Inspectable publication from one native reserve-refill call.</summary>
public readonly record struct SamusReserveAutoRecoveryStep(
    bool RefillSoundRequested,
    bool Completed);
