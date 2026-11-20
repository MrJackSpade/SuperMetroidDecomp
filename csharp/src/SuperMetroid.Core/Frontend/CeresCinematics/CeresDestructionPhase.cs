namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// The destruction cinematic's phases from station destruction through Zebes arrival.
/// Each phase runs itself on the cinematic and enters its successor (#2571), so no switch over phase identities runs them.
/// </summary>
internal sealed class CeresDestructionPhase
{
    /// <summary>The phase's name, for diagnostics.</summary>
    private readonly string _name;

    /// <summary>Runs the phase for one update.</summary>
    private readonly Action<CeresDestructionCinematicState> _run;

    /// <summary>A phase named <paramref name="name"/> that runs <paramref name="run"/>.</summary>
    /// <param name="name">The phase's name, for diagnostics.</param>
    /// <param name="run">Runs the phase for one update.</param>
    private CeresDestructionPhase(string name, Action<CeresDestructionCinematicState> run)
    {
        _name = name;
        _run = run;
    }

    /// <summary>Holds the first Ceres scene until its queued music has drained.</summary>
    internal static CeresDestructionPhase WaitForMusicQueue { get; } = new(nameof(WaitForMusicQueue), cinematic => cinematic.RunWaitForMusicQueue());

    /// <summary>Fades the scene in from black while the camera drifts.</summary>
    internal static CeresDestructionPhase FadeInAndDrift { get; } = new(nameof(FadeInAndDrift), cinematic => cinematic.RunFadeInAndDrift());

    /// <summary>Continues camera drift and zoom until the final station blast is spawned.</summary>
    internal static CeresDestructionPhase ApproachExplosion { get; } = new(nameof(ApproachExplosion), cinematic => cinematic.RunApproachExplosion());

    /// <summary>Pans, rotates, and scales the gunship away from the station blast.</summary>
    internal static CeresDestructionPhase FlyingAwayFromExplosion { get; } = new(nameof(FlyingAwayFromExplosion), cinematic => cinematic.RunFlyingAwayFromExplosion());

    /// <summary>Holds the explosion scene for its authored post-flight interval.</summary>
    internal static CeresDestructionPhase HoldAfterExplosion { get; } = new(nameof(HoldAfterExplosion), cinematic => cinematic.RunHoldAfterExplosion());

    /// <summary>Darkens Ceres before Zebes reveal or the game-over exit with Samus aboard.</summary>
    internal static CeresDestructionPhase FadeOutCeres { get; } = new(nameof(FadeOutCeres), cinematic => cinematic.RunFadeOutCeres());

    /// <summary>Shows the planet-reveal art on the following update.</summary>
    internal static CeresDestructionPhase FlyToZebesInitial { get; } = new(nameof(FlyToZebesInitial), cinematic => cinematic.RunFlyToZebesInitial());

    /// <summary>Raises planet-scene brightness while reducing its pixelation.</summary>
    internal static CeresDestructionPhase FadeInZebes { get; } = new(nameof(FadeInZebes), cinematic => cinematic.RunFadeInZebes());

    /// <summary>Finishes reducing the planet's pixelation before creating the turning-floor planet actors.</summary>
    internal static CeresDestructionPhase RemoveZebesPixelation { get; } = new(nameof(RemoveZebesPixelation), cinematic => cinematic.RunRemoveZebesPixelation());

    /// <summary>Waits for the title actor's cue to start the camera approach.</summary>
    internal static CeresDestructionPhase PlanetZebesTitle { get; } = new(nameof(PlanetZebesTitle), cinematic => cinematic.RunPlanetZebesTitle());

    /// <summary>The gunship flies toward Zebes, the planet growing to the first approach limit (#1981).</summary>
    internal static CeresDestructionPhase FlyingTowardZebes { get; } = new(nameof(FlyingTowardZebes), cinematic => cinematic.RunFlyingTowardZebes());

    /// <summary>The gunship turns toward Zebes, the view rotating and growing until it has panned left past <see cref="CeresDestructionSceneDefinitions.Places.TurnTowardZebesEndX"/>.</summary>
    internal static CeresDestructionPhase TurningTowardZebes { get; } = new(nameof(TurningTowardZebes), cinematic => cinematic.RunTurningTowardZebes());

    /// <summary>The gunship closes in on Zebes, the view panning back and growing until the turning floor hides before the hold.</summary>
    internal static CeresDestructionPhase ClosingInOnZebes { get; } = new(nameof(ClosingInOnZebes), cinematic => cinematic.RunClosingInOnZebes());

    /// <summary>Holds the close planet scene while sprite actors continue.</summary>
    internal static CeresDestructionPhase HoldCloseZebes { get; } = new(nameof(HoldCloseZebes), cinematic => cinematic.RunHoldCloseZebes());

    /// <summary>Slides planet and stars upward until the completion star exits.</summary>
    internal static CeresDestructionPhase SlideZebesSceneAway { get; } = new(nameof(SlideZebesSceneAway), cinematic => cinematic.RunSlideZebesSceneAway());

    /// <summary>Signals the frontend owner to advance after scene completion.</summary>
    internal static CeresDestructionPhase Finished { get; } = new(nameof(Finished), _ => { });

    /// <summary>Runs the phase for one update of <paramref name="cinematic"/>.</summary>
    /// <param name="cinematic">What the phase runs on.</param>
    internal void Run(CeresDestructionCinematicState cinematic) => _run(cinematic);

    /// <summary>Names the phase.</summary>
    public override string ToString() => _name;
}
