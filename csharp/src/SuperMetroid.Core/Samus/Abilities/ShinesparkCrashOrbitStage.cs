namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The three stages of the two crash echoes' orbit around Samus after a shinespark crash.</summary>
public enum ShinesparkCrashOrbitStage
{
    /// <summary>The echoes move outward by <see cref="ShinesparkDefinitions.CrashRadiusStep"/> each update until their radius reaches <see cref="ShinesparkDefinitions.CrashOrbitStartRadius"/>.</summary>
    Expanding,

    /// <summary>The echoes circle Samus at full radius until their angular travel reaches its largest (<see cref="CrashOrbitTravelCarryover.LargestTravel"/>).</summary>
    Orbiting,

    /// <summary>The echoes move inward by the same step each update; reaching radius zero starts the centre hold.</summary>
    Contracting,
}
