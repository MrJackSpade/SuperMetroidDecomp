using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.EnemyAnimationIsolationFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies.BossBodies;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
/// <summary>Provides a mutable-RAM Mother Brain body fixture for instruction and drawing verification.</summary>
internal static class MotherBrainBodyPresentationFixtureVerification
{
    /// <summary>Runs the actual body instruction processor and public drawing path with mutable RAM only.</summary>
    internal sealed class MotherBrainPresentationFixture
    {
        /// <summary>Owns the enemy system, Samus, room, and installed artwork for this fixture.</summary>
        internal EnemyAnimationFixture Inner { get; }
        /// <summary>Body slot whose instruction pointer and OAM frame are exercised.</summary>
        internal RoomEnemySlot Body => Inner.Actor;
        /// <summary>Whether the body's current frame's finished picture shows nothing, so that it draws nothing (#1538).</summary>
        internal bool BodyFrameShowsNothing => Inner.FrameShowsNothing(Body);
        /// <summary>Third-form body state bound to the fixture's encounter.</summary>
        internal MotherBrainEnemyState State { get; }
        /// <summary>The BG2 scroll placing the body after each production step that has taken BG2 over (#1356).</summary>
        internal List<Position16<BackgroundPixels>> ScrollCalls { get; } = [];
        /// <summary>Production instruction processor bound to this fixture's enemy system.</summary>
        private readonly Action<RoomEnemySlot, SamusState?, RoomLevelData?, Position16<RoomPixels>, ControllerButton> instructions;

        /// <summary>Creates and binds a third-form body with installed artwork and the enemy BG2 image it owns since phase two.</summary>
        internal MotherBrainPresentationFixture(ImportEnemyArtwork artwork)
        {
            Inner = new EnemyAnimationFixture(artwork);
            EnemyDefinitionId bodyId = EnemyDefinitionId.MotherBrainBody;
            Body.Bind(bodyId, Body.Definition);
            Body.Bind(Body.DefinitionId, RoomEnemyHeaders.Get(bodyId));
            Body.Position = Body.Position.WithWhole(new(128, 128));
            // She draws under the retail graphics set her room loads, whose sheets hold her frames, at the tile base and
            // palette row it gives her (#1459).
            SpawnedObjectFixtures.LoadSetDrawing(Inner.Enemies, bodyId, Body);
            State = new MotherBrainEnemyState(Body) { Head = Inner.Enemies.SlotTable.Slots[1], Form = MotherBrainForm.DrainedByBabyMetroid };
            Inner.Enemies.MotherBrainEncounter.MotherBrain = State;
            // Her phase-two transition attaches the body layer her body draws on (#1356, #1466), showing her blank. Her
            // body draws from sheets composed from the fixture's own frames, as an installation of them would hold.
            bodies = BossBodySheetFixtures.For(artwork.Bosses, BossBodyOwner.MotherBrain);
            Inner.Enemies.BossArtworkBinding.BodySheets = Sheets;
            ClearBg2();
            Inner.Enemies.Drawing.Enqueue(Body, reached: true);
            instructions = new Action<RoomEnemySlot, SamusState?, RoomLevelData?, Position16<RoomPixels>, ControllerButton>(EnemyAnimationPlayer.AdvanceAnimation);
        }

        /// <summary>Selects a body frame and optionally marks it as a new instruction frame.</summary>
        internal void SetFrame(ushort frame, bool fresh)
        {
            Body.SpritemapPointer = frame;
            Body.CollidesByFrameHitboxes = true;
            Body.AnimationClock.ShowsNewFrame = fresh;
        }

        /// <summary>Starts a body program at its next one-tick dispatch.</summary>
        internal void SetProgram(MotherBrainBodySegment program) => MotherBrainEncounterAccess.SetMotherBrainBodyAnimation(State, program);

        /// <summary>Advances the body's actual production instruction processor once and samples the scroll placing her body.</summary>
        internal void Step()
        {
            instructions(Body, Inner.Samus, Inner.Level, Position16<RoomPixels>.Zero, 0);
            if (State.PlacesBodyLayer)
                ScrollCalls.Add(State.BodyLayerPlacement);
        }

        /// <summary>The tilemap entry each cell of the body layer her body draws on shows, from the frames her artwork installs (#1466).</summary>
        internal ushort[] Bg2Words => BossBodyWords.Of(Inner.Enemies.Presentation.RequireBossBody().Publish(), BossBodyOwner.MotherBrain,
            bodies.Frames);

        /// <summary>Clears her body, which a later frame shown on it would replace (#1515).</summary>
        internal void MarkBg2() => Inner.Enemies.Presentation.RequireBossBody().Clear();

        /// <summary>The fixture's boss frames and the body sheets composed from them, which her body draws from.</summary>
        private readonly BossBodyArtworkFixture bodies;

        /// <summary>The body sheets composed from the fixture's boss frames.</summary>
        private BossBodySheets Sheets => bodies.Sheets;
        /// <summary>Runs the fixture's production enemy drawing path.</summary>
        internal OamBuffer Draw() => Inner.Draw();
        /// <summary>Runs the fixture's production enemy drawing path once, publishing into <paramref name="frame"/>.</summary>
        internal void DrawInto(SpriteFrameBuffer frame) => Inner.DrawInto(frame);
        /// <summary>Attaches a fresh body showing nothing, as her phase-two transition reveals it, before checking fresh publication.</summary>
        internal void ClearBg2() => Inner.Enemies.Presentation.AttachBossBody(new BossBodyParts(BossBodyOwner.MotherBrain));
    }
}
