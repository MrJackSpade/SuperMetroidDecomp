using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>What every ending scene draws on: brightness, the phase timer, the Mode 7 view, palettes, the shared sprites and stars, and the artwork they share.</summary>
[RoomScopeBoundary("A cinematic scene that owns its own palette FX for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class EndingStage
{
    /// <summary>Saved clear-time hours, as the game clock or the configured override supplies them, used to select the post-credits reward.</summary>
    private readonly int gameTimeHours;
    /// <summary>The ending's screen: its palette and the graphics transfers its scenes have made (#1358).</summary>
    private readonly EndingScreen screen ;
    /// <summary>Dynamic cinematic actors in native slot traversal order.</summary>
    private readonly List<EndingSprite> sprites ;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal EndingStage(int gameTimeHours, EndingScreen screen, List<EndingSprite> sprites)
    {
        this.gameTimeHours = gameTimeHours;
        this.screen = screen;
        this.sprites = sprites;
    }

    /// <summary>Countdown interpreted by the currently installed cinematic function.</summary>
    internal int PhaseTimer { get; set; }

    /// <summary>Shared fade progression through scene and palette handoffs.</summary>
    private int fadeCounter;

    /// <summary>Master brightness during cinematic fades, from 0 (black) through 15 (full); every fade step clamps to that range.</summary>
    [IntegerQuantity("screen brightness level, zero through fifteen")]
    internal int Brightness { get; set; }

    /// <summary>Mode-7 translation; only X uses a subpixel because native Y has no fractional word.</summary>
    internal FixedPosition<FloorPixels> FloorPosition { get; set; }

    /// <summary>
    /// Mode-7 scale in 8.8 units (256 is unity), initially the identity. Every scene starts it at an authored
    /// scale and moves it by a few units per call toward a bound it stops at, so it stays positive and small.
    /// </summary>
    internal int FloorZoom { get; set; } = EndingMotionDefinitions.IdentityScale;

    /// <summary>Mode-7 byte-angle shared by the escape and planet scenes.</summary>
    internal RotationAngle FloorAngle { get; set; }

    /// <summary>Scroll of the post-credits text map; only the vertical axis moves, during the item-percentage scroll-down.</summary>
    internal Position16<BgPlanePixels> PostCreditsScroll { get; set; }

    /// <summary>Reloadable object character art for clouds, explosion and reward actors.</summary>
    [field: NonSerialized] internal EndingObjectArtwork? ObjectArtwork { get; set; }
    /// <summary>Current bank-$8B cinematic-function replacement, including terminal display states.</summary>
    public EndingCreditsPhase Phase { get; internal set; }

    /// <summary>Selects suitless, helmetless or armored presentation from native clear-time hour thresholds.</summary>
    public EndingReward EndingReward =>
        gameTimeHours < EndingSequenceDefinitions.Rewards.SuitlessMaximumHoursExclusive
        ? EndingReward.Suitless
        : gameTimeHours < EndingSequenceDefinitions.Rewards.HelmetlessMaximumHoursExclusive
            ? EndingReward.Helmetless
            : EndingReward.Armored;

    /// <summary>Requires installed ending font art before a native VRAM transfer.</summary>
    internal EndingFontAtlas ResolveEndingFont()
    {
        return EndingFont ?? throw new InvalidOperationException(
            "Ending font requires installed presentation assets.");
    }

    /// <summary>Clears VRAM and installs the selected ending scene's Mode-7 map and characters.</summary>
    internal void LoadFloor(EndingFloorSceneId scene)
    {
        _ = FloorArtwork ?? throw new InvalidOperationException(
            "Ending Mode-7 scene requires installed artwork.");
        screen.Clear();
        screen.Record(scene);
    }

    /// <summary>Allocates or replaces a native cinematic actor slot, reserving fixed slots for stars, afterglow and animals.</summary>
    internal void SpawnSprite(
        Position16<ScreenPixels> position,
        int palette,
        CinematicSpriteProgram program,
        EndingSpriteRole role)
    {
        int slot = role switch
        {
            EndingSpriteRole.ExplosionStarsRight => EndingSpriteSlots.RightStars,
            EndingSpriteRole.ExplosionStarsLeft => EndingSpriteSlots.LeftStars,
            EndingSpriteRole.ExplosionAfterglow => EndingSpriteSlots.Afterglow,
            EndingSpriteRole.AnimalEscape => EndingAnimalEscapeDefinitions.ForcedSlot,
            // Every other actor takes the highest free dynamic slot.
            EndingSpriteRole.CloudRightA or EndingSpriteRole.CloudLeftA or EndingSpriteRole.CloudRightB or
            EndingSpriteRole.CloudLeftB or EndingSpriteRole.CloudTopA or EndingSpriteRole.CloudTopB or
            EndingSpriteRole.CloudBottomA or EndingSpriteRole.CloudBottomB or EndingSpriteRole.ExplodingZebes or
            EndingSpriteRole.ExplosionLava or EndingSpriteRole.ExplosionGlow or EndingSpriteRole.ExplosionStars or
            EndingSpriteRole.ExplosionSilhouette or EndingSpriteRole.OperationWasText or
            EndingSpriteRole.CompletedSuccessfullyText or EndingSpriteRole.ClearTimeText or
            EndingSpriteRole.ClearTimeDigit or EndingSpriteRole.RewardSamus => Enumerable.Range(0, EndingSpriteSlots.Count).Reverse()
                .FirstOrDefault(candidate => !sprites.Any(actor => actor.TraversalSlot == candidate && actor.Sprite.IsActive), -1),
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Undefined ending sprite role."),
        };
        if (slot < 0) throw new InvalidOperationException("Ending cinematic actor slots exhausted.");
        sprites.RemoveAll(actor => actor.TraversalSlot == slot);
        sprites.Add(new EndingSprite(new IntroDiscoverySprite(position, palette, program), role)
            { TraversalSlot = slot });
        if (role == EndingSpriteRole.ExplosionStarsLeft)
            sprites[^1].Sprite.InstallPreInstruction(CinematicSpritePreInstruction.ZebesExplosionStarsWaitForFlyaway);
    }

    /// <summary>Installs a sprite from its authored position, attributes and program.</summary>
    internal void SpawnSprite(EndingSpriteDefinition definition, EndingSpriteRole role) =>
        SpawnSprite(
            definition.Position,
            definition.Palette,
            definition.Program,
            role);

    /// <summary>Rejects decoded artwork too short for its native VRAM transfer span.</summary>
    internal static void RequireMinimum(ReadOnlyMemory<byte> data, int minimum, string name)
    {
        if (data.Length < minimum)
            throw new InvalidDataException($"{name} expanded to ${data.Length:X}, expected at least ${minimum:X}.");
    }

    /// <summary>Rejects installed characters whose <paramref name="length"/> bytes do not reach a native VRAM transfer span.</summary>
    internal static void RequireMinimum(int length, int minimum, string name)
    {
        if (length < minimum)
            throw new InvalidDataException($"{name} expanded to ${length:X}, expected at least ${minimum:X}.");
    }

    /// <summary>Rejects an installed font whose characters do not reach a native VRAM transfer span of <paramref name="minimum"/> bytes.</summary>
    internal static void RequireMinimum(EndingFontAtlas font, int minimum, string name)
    {
        RequireMinimum(Ensure.NotNull(font).CharacterCount * (EndingFontAtlasFormat.ByteCount / EndingFontAtlasFormat.TileCount), minimum, name);
    }

    /// <summary>Native scene boundaries clear slots but retain installed color ownership.</summary>
    internal void ResetPaletteFx()
    {
        PaletteFxSystem = new RoomPaletteFxSystem(new PaletteFxSessionState());
    }

    /// <summary>Copies a range of authored ending colors into the live CGRAM destination.</summary>
    internal void LoadStaticPalette(EndingPaletteId id, ColourRun destination) =>
        LoadStaticPalette(id, destination, Palette(id).FirstColorFor(destination));

    /// <summary>Loads <paramref name="destination"/> from <paramref name="id"/>'s colours from <paramref name="sourceColor"/>.</summary>
    internal void LoadStaticPalette(EndingPaletteId id, ColourRun destination, int sourceColor) =>
        Palette(id).LoadTo(destination, sourceColor);

    /// <summary>The installed ending palette <paramref name="id"/>.</summary>
    private EndingPalette Palette(EndingPaletteId id) =>
        (PaletteArtwork ?? throw new InvalidOperationException("Ending palettes require installed artwork."))[id];

    /// <summary>Moves animal and star actors and deletes subordinate explosion layers at their native handoffs.</summary>
    internal void StepEndingSpritePreInstruction(EndingSprite wrapper)
    {
        IntroDiscoverySprite sprite = wrapper.Sprite;
        switch (wrapper.Role)
        {
            case EndingSpriteRole.AnimalEscape:
                uint y = IntroCinematicMotion.Packed(sprite.Position.Y) + EndingAnimalEscapeDefinitions.YFractionVelocity;
                sprite.Position = new(
                    sprite.Position.X.WithWhole(unchecked((ushort)(sprite.Position.X.Whole + EndingAnimalEscapeDefinitions.XVelocity))),
                    IntroCinematicMotion.Unpacked(y));
                if (sprite.Position.X.Whole >= EndingAnimalEscapeDefinitions.DeleteX)
                    sprite.Delete();
                break;
            case EndingSpriteRole.ExplosionGlow:
            case EndingSpriteRole.ExplosionStars:
            case EndingSpriteRole.ExplosionLava:
                // F2A5 observes the original planet actor's instruction pointer, not a
                // cinematic timer. These subordinate layers disappear with that actor.
                if (!sprites.Any(actor => actor.Role == EndingSpriteRole.ExplodingZebes && actor.Sprite.IsActive))
                    sprite.Delete();
                break;
            case EndingSpriteRole.ExplosionStarsRight:
            case EndingSpriteRole.ExplosionStarsLeft:
                // The right starfield runs no flyaway pre-instruction until its list installs $F35A.
                if (sprite.PreInstruction is CinematicSpritePreInstruction.ZebesExplosionStarsWaitForFlyaway or CinematicSpritePreInstruction.ZebesExplosionStarsMoving)
                    StepFlyawayStars(sprite, sprite.PreInstruction.Value);
                if (Phase == EndingCreditsPhase.OperationSuccessfulText)
                    sprite.Delete();
                break;
            case EndingSpriteRole.ExplosionAfterglow:
                // F39B deletes the afterglow at the same native null-function handoff.
                if (Phase == EndingCreditsPhase.OperationSuccessfulText)
                    sprite.Delete();
                break;
            // These actors run no pre-instruction.
            case EndingSpriteRole.CloudRightA:
            case EndingSpriteRole.CloudLeftA:
            case EndingSpriteRole.CloudRightB:
            case EndingSpriteRole.CloudLeftB:
            case EndingSpriteRole.CloudTopA:
            case EndingSpriteRole.CloudTopB:
            case EndingSpriteRole.CloudBottomA:
            case EndingSpriteRole.CloudBottomB:
            case EndingSpriteRole.ExplodingZebes:
            case EndingSpriteRole.ExplosionSilhouette:
            case EndingSpriteRole.OperationWasText:
            case EndingSpriteRole.CompletedSuccessfullyText:
            case EndingSpriteRole.ClearTimeText:
            case EndingSpriteRole.ClearTimeDigit:
            case EndingSpriteRole.RewardSamus:
                break;
            default:
                throw new InvalidOperationException($"Undefined ending sprite role {wrapper.Role}.");
        }
    }

    /// <summary>Starts starfield motion at flyaway and advances its aliased Y-fraction velocity word.</summary>
    private void StepFlyawayStars(IntroDiscoverySprite sprite, CinematicSpritePreInstruction preInstruction)
    {
        switch (preInstruction)
        {
            case CinematicSpritePreInstruction.ZebesExplosionStarsWaitForFlyaway:
                if (Phase == EndingCreditsPhase.PlanetEscapeFast)
                {
                    sprite.InstallPreInstruction(CinematicSpritePreInstruction.ZebesExplosionStarsMoving);
                    // The Y subposition word doubles as the low velocity word.
                    sprite.Position = sprite.Position.WithY(sprite.Position.Y.WithFraction((ushort)(SixteenSixteen.FromWhole(1) / 4)));
                    sprite.GeneralTimer = 0;
                }
                break;
            case CinematicSpritePreInstruction.ZebesExplosionStarsMoving:
                int velocity = unchecked((int)(SixteenSixteen.Compose(sprite.GeneralTimer, sprite.Position.Y.Fraction))) - 32;
                sprite.GeneralTimer = unchecked((ushort)(SixteenSixteen.WholeOf(velocity)));
                uint position = unchecked(IntroCinematicMotion.Packed(sprite.Position.X) + (uint)velocity);
                sprite.Position = new(
                    IntroCinematicMotion.Unpacked(position),
                    sprite.Position.Y.WithFraction(unchecked((ushort)velocity)));
                break;
            case CinematicSpritePreInstruction.NoOp:
            case CinematicSpritePreInstruction.MetroidEgg:
            case CinematicSpritePreInstruction.MetroidEggDeleteAfterCrossFade:
            case CinematicSpritePreInstruction.MetroidEggParticle:
            case CinematicSpritePreInstruction.MetroidEggSlimeDrops:
            case CinematicSpritePreInstruction.BabyMetroidBeingDelivered:
            case CinematicSpritePreInstruction.BabyMetroidBeingExamined:
            case CinematicSpritePreInstruction.IntroMotherBrainCrossFading:
            case CinematicSpritePreInstruction.IntroRinkaMovingHitsSamus:
            case CinematicSpritePreInstruction.IntroRinkaMovingMissesSamus:
            case CinematicSpritePreInstruction.ConfusedBabyWaitingForHatch:
            case CinematicSpritePreInstruction.ConfusedBabyHatched:
            case CinematicSpritePreInstruction.ConfusedBabyIdling:
            case CinematicSpritePreInstruction.ConfusedBabyDancing:
            case CinematicSpritePreInstruction.CeresStars:
            case CinematicSpritePreInstruction.CeresLargeAsteroids:
            case CinematicSpritePreInstruction.CeresUnderAttack:
            case CinematicSpritePreInstruction.CeresSmallAsteroid:
            case CinematicSpritePreInstruction.CeresPurpleSpaceVortex:
            case CinematicSpritePreInstruction.CeresStationaryVortexNoOp:
            case CinematicSpritePreInstruction.CeresExplosionSpawnerRepeating:
            case CinematicSpritePreInstruction.CeresExplosion:
            case CinematicSpritePreInstruction.Zebes:
            case CinematicSpritePreInstruction.ZebesSlideSceneAway:
            case CinematicSpritePreInstruction.ZebesStarsCompletionWait:
            case CinematicSpritePreInstruction.ZebesStarsCompletionSlideSceneAway:
            case CinematicSpritePreInstruction.ZebesStarsWait:
            case CinematicSpritePreInstruction.ZebesStarsSlideSceneAway:
            case CinematicSpritePreInstruction.SuperMetroidIconTopHalf:
            case CinematicSpritePreInstruction.SuperMetroidIconBottomHalf:
            case CinematicSpritePreInstruction.RewardSuitlessUpperNoOp:
            case CinematicSpritePreInstruction.RewardBodyFlight:
            case CinematicSpritePreInstruction.RewardHeadFlight:
            case CinematicSpritePreInstruction.RewardLanding:
            default:
                throw new InvalidOperationException(
                    $"Flyaway stars do not run {preInstruction}.");
        }
    }

    /// <summary>The palette effects the ending runs.</summary>
    internal RoomPaletteFxSystem PaletteFxSystem { get; private set; } = new(new PaletteFxSessionState());

    /// <summary>The ending's installed font.</summary>
    [field: NonSerialized]
    internal EndingFontAtlas? EndingFont { get; private set; }

    /// <summary>The ending's installed Mode 7 artwork.</summary>
    [field: NonSerialized]
    internal EndingFloorArtwork? FloorArtwork { get; private set; }

    /// <summary>The ending's installed palettes.</summary>
    [field: NonSerialized]
    internal EndingPaletteSet? PaletteArtwork { get; private set; }

    /// <summary>Raises native brightness one step every two calls until full brightness.</summary>
    internal bool StepFastFadeIn()
    {
        if (fadeCounter-- <= 0)
        {
            fadeCounter = 1;
            Brightness = Math.Min(15, Brightness + 1);
        }
        return Brightness == 15;
    }

    /// <summary>Lowers native brightness one step every two calls until blank.</summary>
    internal bool StepFastFadeOut()
    {
        if (fadeCounter-- <= 0)
        {
            fadeCounter = 1;
            Brightness = Math.Max(0, Brightness - 1);
        }
        return Brightness == 0;
    }

    /// <summary>Raises native brightness one step every four calls for the post-credits reveal.</summary>
    internal bool StepSlowFadeIn()
    {
        if (fadeCounter-- <= 0)
        {
            fadeCounter = 3;
            Brightness = Math.Min(15, Brightness + 1);
        }
        return Brightness == 15;
    }

    /// <summary>Restarts the fade cadence: the next fade step lands two calls from now.</summary>
    internal void RestartFadeCadence() => fadeCounter = 1;

    /// <summary>Rebinds the host-owned ending font after catalog reload or restoration.</summary>
    internal void BindEndingFont(EndingFontAtlas? value)
    {
        EndingFont = value;
        screen.Invalidate();
    }

    /// <summary>
    /// Selects current host artwork after a debugger-state restore. The active
    /// backdrop is refreshed without resetting actors, palette, transform or music.
    /// During the native flyaway DMA, already-uploaded chunks are reapplied last.
    /// </summary>
    internal void BindFloorArtwork(EndingFloorArtwork? value)
    {
        FloorArtwork = value;
        screen.Invalidate();
    }

    /// <summary>Selects the palettes later native palette transfers and target-color reads use.</summary>
    internal void BindPaletteArtwork(EndingPaletteSet? value) => PaletteArtwork = value;
}
