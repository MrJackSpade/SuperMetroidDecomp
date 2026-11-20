using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The gunship's flight from Zebes: the planet escape's speeds, the white hold, the stars and the fade.</summary>
[RoomScopeBoundary("A cinematic scene that owns its own colour effects for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class EndingFlyaway
{
    /// <summary>Controls the animals' escape detail in the planet cinematic.</summary>
    private readonly bool crittersEscaped;
    /// <summary>The ending's screen: its colours and the final page's lower text (#1358, #1515).</summary>
    private readonly EndingScreen screen ;
    /// <summary>What every ending scene draws on: brightness, the turning-floor view, colours, the shared sprites and stars, and the artwork they share.</summary>
    private readonly EndingStage _stage;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal EndingFlyaway(bool crittersEscaped, EndingScreen screen, EndingStage stage)
    {
        this.crittersEscaped = crittersEscaped;
        this.screen = screen;
        _stage = stage;
    }

    /// <summary>Where the gunship's shake stands in the current planet-escape leg's authored sequence (#1902).</summary>
    private EndingShipShakeCursor ShipShake { get; set; }

    /// <summary>Planet-escape velocity: a displacement that grows more negative every accelerating update.</summary>
    private SignedSixteenSixteen PlanetVelocity { get; set; }

    /// <summary>
    /// The white wash over the planet escape (<see cref="WhiteWash"/>), from full white fading to none, where it stays. The
    /// planet escape's setup shows it; only the planet escape's legs fade it.
    /// </summary>
    internal WhiteWash WhiteWash { get; } = WhiteWash.FullWhite(EndingFlyawayFadeDefinitions.UpdatesPerStep, EndingFlyawayFadeDefinitions.FirstStepUpdate);

    /// <summary>
    /// The fast leg's hold of its full white (#2541): its first updates keep the wash whole, and the wash fades on each of
    /// the fast leg's updates after them.
    /// </summary>
    private readonly UpdateCountdown whiteHold = UpdateCountdown.Lasting(EndingFlyawayFadeDefinitions.WhiteHoldFrames);

    /// <summary>Clears the whiteout from the sky and sets up the accelerating planet flyaway with its authored colour effects.</summary>
    internal void SetupPlanetEscape()
    {
        // The flyaway's setup clears the explosion's whiteout from the sky.
        screen.Colours.Flash(EndingFlash.WhiteoutCleared, RgbColor.Black);
        _stage.ColourEffects.StartScreen(EndingColourEffectDefinitions.PlanetAfterglow);
        _stage.ColourEffects.StartScreen(EndingColourEffectDefinitions.GunshipEmergence);
        _stage.Floor.SetUp(new Offset16<FloorPixels>(EndingMotionDefinitions.PlanetEscapeFloorX, EndingMotionDefinitions.PlanetEscapeFloorY),
            EndingMotionDefinitions.PlanetEscapeInitialZoom, EndingMotionDefinitions.PlanetEscapeInitialAngle);
        ShipShake = EndingShipShakeCursor.StartOf(EndingShipShake.Fast);
    }

    /// <summary>Runs the first rotating planet-escape motion pattern until the fast zoom threshold.</summary>
    /// <returns>Whether the fast leg has reached its threshold, where the slow leg's shake starts.</returns>
    internal bool StepPlanetEscapeFast()
    {
        if (!whiteHold.IsOver)
            whiteHold.Count();
        else WhiteWash.Fade();
        _stage.Floor.Turn(EndingMotionDefinitions.PlanetFastTurnSteps);
        ShakeShip();
        _stage.Floor.Narrow(EndingMotionDefinitions.PlanetFastZoomStep);
        if (_stage.Floor.Zoom >= EndingMotionDefinitions.PlanetFastEndZoom)
            return false;
        ShipShake = EndingShipShakeCursor.StartOf(EndingShipShake.Slow);
        return true;
    }

    /// <summary>Runs the slower motion pattern until the slow zoom threshold, where the accelerating leg's speed starts.</summary>
    /// <returns>Whether the slow leg has reached its threshold.</returns>
    internal bool StepPlanetEscapeSlow()
    {
        WhiteWash.Fade();
        if (_stage.Floor.Angle != EndingMotionDefinitions.PlanetSlowTargetAngle)
            _stage.Floor.Turn(EndingMotionDefinitions.PlanetSlowTurnSteps);
        ShakeShip();
        _stage.Floor.Narrow(EndingMotionDefinitions.PlanetSlowZoomStep);
        if (_stage.Floor.Zoom >= EndingMotionDefinitions.PlanetSlowEndZoom)
            return false;
        PlanetVelocity = EndingMotionDefinitions.PlanetInitialVelocity;
        return true;
    }

    /// <summary>
    /// Releases the escaped animals' pod into <paramref name="scene"/> as the accelerating leg starts, when the animals
    /// escaped; the pod moves and shows its first picture on the same update.
    /// </summary>
    internal void ReleaseEscapedAnimals(IEndingActorScene scene)
    {
        if (!crittersEscaped)
            return;
        EndingSprite podActor = _stage.SpawnSprite(new(EndingAnimalEscapeDefinitions.Origin, EndingAnimalEscapeDefinitions.Origin),
            EndingAnimalEscapeDefinitions.Colouring, EndingAnimalEscapeDefinitions.Animation,
            EndingSpriteRole.AnimalEscape);
        // The cinematic phase runs before the actors, so a newly spawned pod
        // moves and selects its first animation frame on this same update.
        podActor.StepMotion(scene);
        podActor.Sprite.Step();
    }

    /// <summary>Moves the gunship by its shake's next displacement and moves the shake on.</summary>
    private void ShakeShip()
    {
        _stage.Floor.MoveAcross(ShipShake.Displacement);
        ShipShake = ShipShake.Advanced();
    }

    /// <summary>Speeds the planet up and moves it by its velocity, turning it toward the final gunship text at the exit
    /// scale.</summary>
    /// <param name="cinematicFrame">The ending's updates so far, whose quarter phase paces the planet's turn.</param>
    /// <returns>Whether the planet has reached its exit scale, where the completion text shows.</returns>
    internal bool StepPlanetEscapeAccelerating(int cinematicFrame)
    {
        WhiteWash.Fade();
        PlanetVelocity -= EndingMotionDefinitions.PlanetAcceleration;
        _stage.Floor.MoveAcross(PlanetVelocity);
        if (_stage.Floor.Zoom < EndingMotionDefinitions.PlanetTurnStartZoom &&
            EndingCadenceDefinitions.PlanetTurn.IsDue(cinematicFrame) &&
            _stage.Floor.Angle != EndingMotionDefinitions.PlanetExitTargetAngle)
        {
            _stage.Floor.Turn(EndingMotionDefinitions.PlanetExitTurnSteps);
        }
        if (_stage.Floor.Zoom >= EndingMotionDefinitions.PlanetExitZoom)
        {
            _stage.Floor.Narrow(EndingMotionDefinitions.PlanetFlyToCameraZoomStep);
            return false;
        }
        _stage.LoadColours(EndingColourLoad.FinalGunship);
        _stage.SpawnText(
            EndingActorDefinitions.OperationWasText,
            EndingSpriteRole.OperationWasText);
        return true;
    }

    /// <summary>The white wash's tint while it shows, or null once it has faded away.</summary>
    internal ScreenTintLayer? WhiteWashShown => WhiteWash.IsGone ? null : WhiteWash.Layer;

    /// <summary>Restores the explosion's background colours as the explosion's hold begins (#1467).</summary>
    internal void PrepareFlyaway()
    {
        // The explosion's hold restores the background colours before the escape backdrop goes off;
        // the sprites and their colours stay as they are through the hold.
        _stage.LoadColours(EndingColourLoad.ExplosionBackgrounds);
    }
}
