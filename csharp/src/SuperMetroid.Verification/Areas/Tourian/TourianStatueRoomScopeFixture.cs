using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Areas.Tourian;

/// <summary>
/// The Statues room's scope, loaded outside the running game through the room load's own steps (#1512, #2554): the room's
/// assets and pictures, its effect, the statues' room its setup composes over the effect's water, its colour effects, its
/// enemies loaded from its population over that statues' room, and the statues' sequence the load builds over them. A check
/// reads the room's shown colours from the scope's pictures under <see cref="RoomScope.Shading"/>, without a runtime or a
/// published frame.
/// </summary>
internal static class TourianStatueRoomScopeFixture
{
    /// <summary>
    /// The Statues room after the four area bosses are defeated and each statue's grey event is set, as a re-entry after
    /// every statue greyed loads it, entered through an inert door at the room's top-left camera; its enemies draw from
    /// <paramref name="artwork"/>. No update has run: the statues' first block-animation step starts their already-grey
    /// finish.
    /// </summary>
    /// <param name="artwork">The installed enemy and boss artwork the room's enemies draw from.</param>
    /// <exception cref="InvalidOperationException">The room's enemies built no statues.</exception>
    internal static RoomScope GreyStatues(ImportEnemyArtwork artwork)
    {
        Ensure.NotNull(artwork);
        var system = new SharedGameState(new SharedRandom());
        foreach (AreaId area in new[] { AreaId.Brinstar, AreaId.Norfair, AreaId.WreckedShip, AreaId.Maridia })
            system.RecordBossesDefeated(area, DefeatedBosses.AreaBoss);
        foreach (StoryEvent greyEvent in new[] { StoryEvent.PhantoonStatueGrey, StoryEvent.RidleyStatueGrey, StoryEvent.DraygonStatueGrey, StoryEvent.KraidStatueGrey })
            system.Progress.Events.Add(greyEvent);
        SamusState samus = RoomEnemyFrameFixtures.StandaloneSamus();
        var player = new SamusFrameState(samus);
        RoomDefinition definition = RoomDefinitions.Statues;
        // The room's state is chosen from the progress, as every door chooses it, for a Samus carrying nothing.
        SelectedRoom room = SelectedRoom.ForProgress(definition, new RoomStateSelectionContext(system.Progress.Events.Copy(),
            system.BossesDefeatedIn(definition.Area), HasMorphBallAndMissiles: false, HasPowerBombs: false));
        var door = new DoorDefinition(
            Name: "statue room fixture entry",
            DestinationRoom: definition,
            Orientation: new(DoorDirection.Right, DoorClosingBehavior.None),
            RoomObjectBlock: default,
            DestinationScreen: default(Position16<RoomPixels>).ToRoomScreen(),
            SamusSpeed: DoorSamusSpeed.None,
            Setup: DoorSetupDefinitions.None);
        GameplayAudioRequests audio = GameplayAudioRequests.Standalone();

        // The room load's steps, in its order: the assets, the scope's construction, the block animations, the effect, the
        // room's setup and main, the colour effects, the enemies, the statues' sequence.
        LoadedRoomAssets assets = LoadedRoomAssets.Load(RoomScrollGrid.PowerOnCells(), room, InstalledFixtures.RoomAssets);
        var camera = new ScrollBoundaryCamera(assets.Scrolls);
        var scope = new RoomScopeConstruction(door, room, assets, RoomEntry.Plain, camera, InstalledFixtures.CreateBombSystem(),
            new RoomMainHandOvers());
        var blockAnimations = new RoomBlockAnimations(room.State.Fx, door);
        var fxOverlay = RoomFxOverlayState.ForRoom(room.State.Fx, door, system.RandomNumber, audio, definition);
        room.State.SetupBehavior.Compose(new RoomSetupComposition
        {
            Scope = scope,
            System = system,
            FxOverlay = fxOverlay,
            ViewportLoadMode = RoomViewportLoadMode.DisplayInitialViewport,
        });
        room.State.MainBehavior.Compose(scope);
        var colourEffects = ColourEffectSystem.ForRoom(new ColourEffectSessionState(), room.State.Fx, door, samus.EquippedItems,
            system.HasDefeatedAnyOf(room.Area, DefeatedBosses.AreaMiniBoss), audio);
        RoomEnemySystem enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(artwork.Enemies),
            RoomEnemyCompatibilityFixtures.ForNewRoom(), installedBosses: artwork.Bosses, area: room.Area, roomFx: fxOverlay,
            statueRoom: scope.TourianStatueRoom);
        _ = enemies.LoadRoom(room.State.EnemyPopulation, samus, assets.LevelData);
        RoomTourianStatues statues = enemies.TourianStatue
            ?? throw new InvalidOperationException("The Statues room's enemies built no statues.");
        // The statues' sequence reaches the room objects only to open the way into Tourian, which no grey event opens.
        RoomObjectSystem roomObjects = RoomObjectFixtures.Unbound();
        var sequence = new TourianStatueSequence(
            new TourianStatueSequenceDependencies
            {
                Player = player,
                System = system,
                Assists = new ActiveAssists(player, system, new GameplayAssists()),
                Freeze = new GameplayTimeFreeze(player),
            },
            new TourianStatueSequenceRoom
            {
                StatueRoom = statues.Room,
                Statue = statues.Actors,
                Level = assets.LevelData,
                Scrolls = camera.Scrolls,
                ArtworkFrames = scope.ArtworkFrames,
                FxOverlay = fxOverlay,
                ColourEffects = colourEffects,
                Earthquake = enemies.Earthquake,
                RoomObjects = roomObjects,
            });
        return scope.Complete(blockAnimations, fxOverlay, colourEffects, InstalledFixtures.CreateProjectileSystem(), roomObjects,
            enemies, sequence);
    }
}
