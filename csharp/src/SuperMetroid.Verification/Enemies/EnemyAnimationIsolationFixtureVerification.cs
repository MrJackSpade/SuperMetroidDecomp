using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Assets;

/// <summary>Provides matching cartridge-free enemy rooms and mechanical state comparisons.</summary>
internal static class EnemyAnimationIsolationFixtureVerification
{
    /// <summary>
    /// Small RAM-only room for the statically identified ordinary/extended display-binding
    /// boundary. It deliberately starts at a known compiled AI phase, not a controller
    /// playthrough, and cannot provide cartridge bytes to production code.
    /// </summary>
    internal sealed class EnemyAnimationFixture
    {
        /// <summary>Y coordinate where solid blocks begin in the synthetic room.</summary>
        internal const int FloorY = 224;

        /// <summary>
        /// Whether <paramref name="place"/>'s current frame's finished picture shows nothing, so that it draws nothing (#1538)
        /// where the cartridge writes its transparent sprites.
        /// </summary>
        internal bool FrameShowsNothing(RoomEnemyPlace place) =>
            place.Frame is { } frame && Enemies.Presentation.Artwork.Enemy(place.HeldOccupant().DefinitionId).Sheet.ShowsNothing(EnemySheetDefinitions.FrameName(frame));
        /// <summary>Enemy runtime fed by this fixture's room, RNG, and artwork.</summary>
        internal RoomEnemySystem Enemies { get; }
        /// <summary>The Boyon placed in the first enemy slot.</summary>
        internal RoomEnemyPlace Actor => Enemies.Places.InUpdateOrder[0];
        /// <summary>Samus position used for the actor's proximity and hit interactions.</summary>
        internal SamusState Samus { get; } = new(GameplayAudioRequests.Standalone(), SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), artwork: global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(128, 16)) };
        /// <summary>Room collision plane with a flat floor below the actor.</summary>
        internal RoomLevelData Level { get; }
        /// <summary>Fixture VRAM receiving enemy tile presentation.</summary>
        internal SnesVram Vram { get; } = new();
        /// <summary>Current deterministic RNG word supplied to enemy behavior.</summary>
        internal ushort RandomWord { get; private set; } = 1;
        /// <summary>Number of boss-defeated event publications.</summary>
        internal int BossBitCalls { get; private set; }

        /// <summary>Creates matching terrain and services, then initializes Boyon.</summary>
        /// <param name="artwork">The enemy artwork the fixture draws.</param>
        /// <param name="graphicsSet">
        /// The graphics set the actor's room draws under, which gives it its tile base and palette before it initializes;
        /// none draws the standard artwork, for an actor that draws from characters at index zero.
        /// </param>
        internal EnemyAnimationFixture(ImportEnemyArtwork artwork, RoomEnemyGraphicsSetDefinition? graphicsSet = null)
        {
            const int roomBlocks = 32;
            const int floorRow = FloorY / 16;
            Level = CreateSolidRowsRoom(new(roomBlocks, roomBlocks), floorRow, roomBlocks - floorRow);
            Enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(artwork.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null), installedBosses: artwork.Bosses);
            Enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(Vram));
            Enemies.VerificationServices().NextRandom = NextRandom;
            Enemies.VerificationServices().ReadRandom = () => RandomWord;
            Enemies.VerificationServices().SeedRandom = value => RandomWord = value;
            Enemies.VerificationServices().IsAreaTorizoDefeated = () => BossBitCalls != 0;
            Enemies.VerificationServices().SetAreaTorizoDefeated = () => BossBitCalls++;

            Actor.Bind(EnemyDefinitionId.Boyon, Actor.KeptDefinition);
            Actor.Bind(Actor.HeldOccupant().DefinitionId, RoomEnemyDefinitionLookup.Get(Actor.HeldOccupant().DefinitionId));
            Actor.Health = Actor.Definition.Health;
            Actor.Radius = new(Actor.Definition.Radius.X, Actor.Definition.Radius.Y);
            Actor.Position = Actor.Position.WithWhole(new(128, (ushort)(FloorY - Actor.Radius.Y - 12)));
            Actor.Contact = EnemyContact.None;
            Actor.Processing = EnemyProcessing.Animates | EnemyProcessing.OffScreen;
            Actor.Presence = EnemyPresence.None;
            Actor.AnimationClock.StepOnNextUpdate();
            if (graphicsSet is not null)
            {
                Enemies.DrawUnder(graphicsSet);
                Actor.Shading = PictureShading.Standard;
            }
            // Choose valid nonzero bounce-height and multiplier indexes; these are
            // fixture inputs, not authored animation/gameplay tuning.
            Actor.SetSpawnParameter1(EnemyDefinitionId.Boyon, (3 << 8) | 2);
            Actor.SetSpawnParameter2(EnemyDefinitionId.Boyon, Actor.Position.Y.Whole);
            new Action<RoomEnemyPlace>(fixturePlace => BoyonBehavior.InitializeBoyon(fixturePlace))(Actor);
        }

        /// <summary>Advances enemy projectiles before the enemy frame using the same frame counter.</summary>
        internal void Step(int frame)
        {
            Enemies.StepEnemyProjectiles(Level, Samus, frameCounterByte: unchecked((byte)frame));
            Enemies.StepStandaloneFrame(Enemies.StandaloneFrame(Level) with { Camera = new(0, 0), TimeIsFrozen = false, Samus = Samus, ShortCounter = new SuperMetroid.Core.Compatibility.ShortFrameCount(unchecked((byte)frame)) });
        }

        /// <summary>Captures a complete enemy and projectile OAM draw for artwork comparison.</summary>
        internal OamBuffer Draw()
        {
            var oam = new OamBuffer();
            oam.BeginFrame();
            Enemies.Drawing.DrawAsOam(oam, new(0, 0), Samus);
            Enemies.DrawEnemyProjectiles(oam, Position16<RoomPixels>.Zero);
            return oam;
        }

        /// <summary>Runs the same drawing as <see cref="Draw"/> once, publishing into <paramref name="frame"/>.</summary>
        internal void DrawInto(SpriteFrameBuffer frame)
        {
            Enemies.Drawing.Advance(new(0, 0));
            Enemies.Drawing.Draw(Enemies.Drawing.Advance(new(0, 0)), frame, new(0, 0), Samus);
            Enemies.EffectActors.DrawRoomEffectActors(frame, Position16<RoomPixels>.Zero, EnemyFormatDrawAccess.HitBurstSheets);
            Enemies.Projectiles.DrawEnemyProjectilesAt(frame, Position16<RoomPixels>.Zero, EnemyProjectileDepth.InFrontOfSamus, timeIsFrozen: false);
            Enemies.Projectiles.DrawEnemyProjectilesAt(frame, Position16<RoomPixels>.Zero, EnemyProjectileDepth.BehindSamus, timeIsFrozen: false);
        }
        /// <summary>Advances the fixture's deterministic RNG.</summary>
        private ushort NextRandom()
        {
            RandomWord = unchecked((ushort)(RandomWord * 5 + 1));
            return RandomWord;
        }
    }

    /// <summary>
    /// Compares two fixtures' states through their whole debugger-state graphs (#2602): instruction identities, timers,
    /// positions, health and every other value the compiler writes for the state, both naming the installed content by path.
    /// </summary>
    internal static void AssertAnimationValues<T>(T baseline, T edited, string context) where T : class =>
        AssertTrue(GraphDigestVerification.SameGraph(baseline, edited), context + " " + typeof(T).Name + " state");
}
