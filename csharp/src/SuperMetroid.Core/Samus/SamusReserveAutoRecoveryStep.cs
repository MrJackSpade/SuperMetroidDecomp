namespace SuperMetroid.Core.Samus;

/// <summary>What one update of the reserve tanks' automatic refill did: whether it asked for the refill sound and whether it finished.</summary>
public readonly partial record struct SamusReserveAutoRecoveryStep(
    bool RefillSoundRequested,
    bool Completed);
