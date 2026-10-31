using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Rooms;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The gunship's flight from Zebes: the planet escape's speeds, the stars, the fade and the flight artwork's uploads.</summary>
[RoomScopeBoundary("A cinematic scene that owns its own palette FX for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class EndingFlyaway
{
    /// <summary>Controls the animals' escape detail in the planet cinematic.</summary>
    private readonly bool crittersEscaped;
    /// <summary>The ending's screen: its palette and the graphics transfers its scenes have made (#1358).</summary>
    private readonly EndingScreen screen ;
    /// <summary>Dynamic cinematic actors in native slot traversal order.</summary>
    private readonly List<EndingSprite> sprites ;
    /// <summary>What every ending scene draws on: brightness, the phase timer, the Mode 7 view, palettes, the shared sprites and stars, and the artwork they share.</summary>
    private readonly EndingStage _stage;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal EndingFlyaway(bool crittersEscaped, EndingScreen screen, List<EndingSprite> sprites, EndingStage stage)
    {
        this.crittersEscaped = crittersEscaped;
        this.screen = screen;
        this.sprites = sprites;
        _stage = stage;
    }

    /// <summary>
    /// Cinematic calls made so far. Only its parity and quarter phase are read (the alternating
    /// atmospheric rotation and the planet turn cadence), so a growing count reproduces the
    /// cartridge's wrapping word exactly.
    /// </summary>
    internal int CinematicFrame { get; set; }

    /// <summary>Position in the current planet-escape shake pattern, cycling through its entries.</summary>
    internal int PlanetMotionIndex { get; set; }

    /// <summary>Planet-escape velocity: a signed 16.16 displacement that grows more negative every accelerating update.</summary>
    internal SignedSixteenSixteen PlanetVelocity { get; private set; }

    /// <summary>Clears flash backdrop colors and starts the accelerating planet flyaway with its authored palette effects.</summary>
    internal void SetupPlanetEscape()
    {
        // Func120 clears the explosion flash's backdrop and transparent palette entries.
        screen.Palette.Backdrop = RgbColor.Black;
        screen.Palette.Colours(ScreenColourGroup.EndingFlashBackgroundTransparent).Fill(RgbColor.Black);
        screen.Palette.Colours(ScreenColourGroup.EndingFlashSpriteTransparent).Fill(RgbColor.Black);
        _stage.PaletteFxSystem.SpawnDefinition(EndingPaletteFxDefinitions.PlanetAfterglow, 0);
        _stage.PaletteFxSystem.SpawnDefinition(EndingPaletteFxDefinitions.GunshipEmergence, 0);
        _stage.FloorPosition = new(_stage.FloorPosition.X.WithWhole(unchecked((ushort)-72)), new(unchecked((ushort)-104), 0));
        _stage.FloorZoom = EndingMotionDefinitions.PlanetEscapeInitialScale;
        _stage.FloorAngle = RotationAngle.NormalizeTableIndex(-112);
        _stage.PhaseTimer = 192;
        flyawayWhite = EndingFlyawayFadeDefinitions.InitialWhite;
        flyawayFadeCountdown = EndingFlyawayFadeDefinitions.InitialCountdown;
        PlanetMotionIndex = 0;
        _stage.Phase = EndingCreditsPhase.PlanetEscapeFast;
    }

    /// <summary>Runs the first rotating planet-escape motion pattern until the fast zoom threshold.</summary>
    internal void StepPlanetEscapeFast()
    {
        if (_stage.PhaseTimer > 0)
            _stage.PhaseTimer--;
        else StepFlyawayFade();
        _stage.FloorAngle = _stage.FloorAngle.AddTableUnits(-4);
        _stage.FloorPosition = _stage.FloorPosition.WithX(AddSignedFixed(_stage.FloorPosition.X, EndingShipShake.PlanetFastDelta(PlanetMotionIndex)));
        PlanetMotionIndex = (PlanetMotionIndex + 1) % EndingMotionDefinitions.PlanetFastPatternLength;
        _stage.FloorZoom -= 8;
        if (_stage.FloorZoom < EndingMotionDefinitions.PlanetFastEndScale)
        {
            PlanetMotionIndex = 0;
            _stage.Phase = EndingCreditsPhase.PlanetEscapeSlow;
        }
    }

    /// <summary>Runs the slower motion pattern and starts the accelerating leg, optionally spawning the escaped animals.</summary>
    internal void StepPlanetEscapeSlow()
    {
        StepFlyawayFade();
        if (_stage.FloorAngle != EndingMotionDefinitions.PlanetSlowTargetAngle)
            _stage.FloorAngle = _stage.FloorAngle.AddTableUnits(-1);
        _stage.FloorPosition = _stage.FloorPosition.WithX(AddSignedFixed(_stage.FloorPosition.X, EndingShipShake.PlanetSlowDelta(PlanetMotionIndex)));
        PlanetMotionIndex = (PlanetMotionIndex + 1) % EndingMotionDefinitions.PlanetSlowPatternLength;
        _stage.FloorZoom -= 2;
        if (_stage.FloorZoom < EndingMotionDefinitions.PlanetSlowEndScale)
        {
            PlanetVelocity = EndingMotionDefinitions.PlanetInitialVelocity;
            _stage.Phase = EndingCreditsPhase.PlanetEscapeAccelerating;
            if (crittersEscaped)
            {
                _stage.SpawnSprite(new(EndingAnimalEscapeDefinitions.Origin, EndingAnimalEscapeDefinitions.Origin),
                    ScreenColourGroup.EndingAnimalEscape, CinematicSpriteProgram.EndingAnimalEscape,
                    EndingSpriteRole.AnimalEscape);
                IntroDiscoverySprite pod = sprites[^1].Sprite;
                pod.GeneralTimer = EndingAnimalEscapeDefinitions.GeneralTimer;
                // Native cinematic dispatch precedes actors, so a newly spawned pod
                // moves and selects its first animation frame on this same call.
                _stage.StepEndingSpritePreInstruction(sprites[^1]);
                pod.Step();
            }
        }
    }

    /// <summary>Accumulates split planet velocity and turns toward the final gunship text at the exit scale.</summary>
    internal void StepPlanetEscapeAccelerating()
    {
        StepFlyawayFade();
        PlanetVelocity -= EndingMotionDefinitions.PlanetAcceleration;
        _stage.FloorPosition = _stage.FloorPosition.WithX(AddSignedFixed(_stage.FloorPosition.X, PlanetVelocity.RawValue));
        if (_stage.FloorZoom < EndingMotionDefinitions.PlanetTurnStartScale &&
            (CinematicFrame & 3) == 0 &&
            _stage.FloorAngle != EndingMotionDefinitions.PlanetExitTargetAngle)
        {
            _stage.FloorAngle = _stage.FloorAngle.AddTableUnits(2);
        }
        if (_stage.FloorZoom < EndingMotionDefinitions.PlanetExitScale)
        {
            _stage.LoadStaticPalette(EndingPaletteId.FinalGunship,
                screen.Palette.Colours(ScreenColourGroup.EndingFinalGunship), 0);
            _stage.SpawnSprite(
                EndingActorDefinitions.OperationWasText,
                EndingSpriteRole.OperationWasText);
            _stage.Phase = EndingCreditsPhase.OperationSuccessfulText;
        }
        else
        {
            _stage.FloorZoom -= 4;
        }
    }

    /// <summary>Adds a signed 16.16 motion delta to a Mode-7 coordinate with native word wrapping.</summary>
    private static FixedCoordinate AddSignedFixed(FixedCoordinate position, int delta) =>
        SignedSixteenSixteen
            .FromRaw(delta)
            .AddTo(position.Whole, position.Fraction);

    /// <summary>Fixed-color white component over the planet escape, from full white (31) fading to zero, where it stays.</summary>
    private int flyawayWhite;

    /// <summary>Countdown between native flyaway fixed-color decrements.</summary>
    private int flyawayFadeCountdown;

    /// <summary>Fixed-color addition active only during visible planet flyaway phases.</summary>
    internal ScreenTintLayer? FlyawayFixedColor => flyawayWhite != 0
        && EndingCreditsPhaseGroups.PlanetEscape.Contains(_stage.Phase)
        ? WhiteAddition(flyawayWhite) : null;

    /// <summary>Adds the same five-bit <paramref name="white"/> component to all three channels of the composed screen.</summary>
    internal static ScreenTintLayer WhiteAddition(int white)
    {
        return new(checked((byte)white), checked((byte)white), checked((byte)white));
    }

    /// <summary>Decreases the flyaway white component at its cartridge interval.</summary>
    private void StepFlyawayFade()
    {
        if (--flyawayFadeCountdown > 0) return;
        if (flyawayWhite > 0) flyawayWhite--;
        flyawayFadeCountdown = EndingFlyawayFadeDefinitions.Interval;
    }

    /// <summary>
    /// The ending's first flyaway reuses the Ceres approach's cartridge source.
    /// Rebind only chunks already loaded by the sixteen-update load;
    /// future chunks must still arrive on their original scheduled frames.
    /// </summary>
    internal void BindFlightArtwork(CeresFlightArtwork? value)
    {
        FlightArtwork = value;
    }

    /// <summary>Restores the explosion's BG palette half before the flyaway's floor loads; the floor is an installed picture (#1467).</summary>
    internal void PrepareFlyawayUploads()
    {
        // Func117 restores the BG half before disabling BG1. OBJ VRAM and palettes
        // remain live while the two floor lanes are replaced over sixteen updates.
        _stage.StaticScreen(EndingPaletteId.Explosion).LoadBackgroundsTo(screen.Palette);
    }

    /// <summary>Transfers one scheduled flyaway chunk to its Mode-7 character or map lane.</summary>
    internal void UploadFlyawayChunk(int index) => screen.LoadFlyawayChunk(index);

    /// <summary>The gunship flight's installed artwork.</summary>
    [field: NonSerialized]
    internal CeresFlightArtwork? FlightArtwork { get; private set; }
}
