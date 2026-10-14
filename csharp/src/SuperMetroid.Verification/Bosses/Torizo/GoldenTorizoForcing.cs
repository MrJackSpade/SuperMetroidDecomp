using SuperMetroid.Core.Bosses.Torizo;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>Golden Torizo behaviours a ROM-free room comparison forces in both games (#633).</summary>
internal readonly record struct GoldenTorizoForcing
{
    /// <summary>Forced golden sonic start.</summary>
    public TorizoBodyProgram? ForcedGoldenSonicStart { get; init; }

    /// <summary>Forced golden stun.</summary>
    public bool ForcedGoldenStun { get; init; }

    /// <summary>Forced golden left turn start.</summary>
    public TorizoBodyProgram? ForcedGoldenLeftTurnStart { get; init; }

    /// <summary>Forced golden left foot orb.</summary>
    public bool ForcedGoldenLeftFootOrb { get; init; }
}
