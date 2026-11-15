namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The three stages of the two crash echoes' orbit around Samus after a shinespark crash.</summary>
public enum ShinesparkCrashOrbitStage
{
    /// <summary>The echoes move outward four pixels an update until their radius reaches sixteen.</summary>
    Expanding,

    /// <summary>The echoes circle Samus at full radius until their angular travel reaches 128.</summary>
    Orbiting,

    /// <summary>The echoes move inward four pixels an update; reaching radius zero starts the centre hold.</summary>
    Contracting,
}
