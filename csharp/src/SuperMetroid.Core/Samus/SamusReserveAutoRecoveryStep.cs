namespace SuperMetroid.Core.Samus;

/// <summary>What one update of the reserve tanks' automatic refill did: whether it asked for the refill sound and whether it finished.</summary>
[IndependentFlag("independent outcomes of one recovery step: a refill sound, and completion")]
public readonly partial record struct SamusReserveAutoRecoveryStep(
    bool RefillSoundRequested,
    bool Completed);
