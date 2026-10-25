using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.EnemyAnimationIsolationFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
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
        /// <summary>Third-form body state bound to the fixture's encounter.</summary>
        internal MotherBrainEnemyState State { get; }
        /// <summary>The BG2 scroll placing the body after each production step that has taken BG2 over (#1356).</summary>
        internal List<Position16<Bg2Pixels>> ScrollCalls { get; } = [];
        /// <summary>Production instruction processor bound to this fixture's enemy system.</summary>
        private readonly Action<RoomEnemySlot, SamusState?, RoomLevelData?, Position16<RoomPixels>, ControllerButton> instructions;

        /// <summary>Creates and binds a third-form body with installed artwork and the enemy BG2 image it owns since phase two.</summary>
        internal MotherBrainPresentationFixture(InstalledEnemyArtwork artwork)
        {
            Inner = new EnemyAnimationFixture(artwork, golden: false);
            EnemyDefinitionId bodyId = EnemyDefinitionId.MotherBrainBody;
            Body.Bind(bodyId, Body.Definition);
            Body.Bind(Body.DefinitionId, RoomEnemyDefinitionCatalog.Get(bodyId));
            Body.Position = Body.Position.WithWhole(new(128, 128));
            Body.PaletteIndex = Body.TileIndexBase = 0;
            State = new MotherBrainEnemyState(Body) { Head = Inner.Enemies.SlotTable.Slots[1], Form = MotherBrainForm.DrainedByBabyMetroid };
            Inner.Enemies.MotherBrainEncounter.MotherBrain = State;
            // Her phase-two transition attaches the enemy BG2 image her body draws into (#1356).
            Inner.Enemies.Presentation.AttachBg2Image(new EnemyBg2Image(0));
            Inner.Enemies.Drawing.Enqueue(0, Body.EnemyIndex);
            instructions = new Action<RoomEnemySlot, SamusState?, RoomLevelData?, Position16<RoomPixels>, ControllerButton>(EnemyInstructionInterpreter.ProcessInstructions);
        }

        /// <summary>Selects a body frame and optionally marks it as a new instruction frame.</summary>
        internal void SetFrame(ushort frame, bool fresh)
        {
            Body.SpritemapPointer = frame;
            Body.ExtraProperties = EnemyExtraPropertyWord.FromNativeWord((ushort)(EnemyExtraProperties.UsesExtendedSpritemap |
                (fresh ? EnemyExtraProperties.NewInstructionFrame : 0)));
        }

        /// <summary>Starts a body program at its next one-tick dispatch.</summary>
        internal void SetProgram(MotherBrainBodyProgram program) => MotherBrainEncounterAccess.SetMotherBrainBodyProgram(State, program);

        /// <summary>Advances the body's actual production instruction processor once and samples the scroll placing her body.</summary>
        internal void Step()
        {
            instructions(Body, Inner.Samus, Inner.Level, Position16<RoomPixels>.Zero, 0);
            if (State.HasBg2ScrollOverride)
                ScrollCalls.Add(State.Bg2Scroll);
        }

        /// <summary>The words of the BG2 image her body draws into.</summary>
        internal ushort[] Bg2Words => [.. Inner.Enemies.Presentation.RequireBg2Image().Publish().Words];

        /// <summary>Writes <paramref name="word"/> at the start of her BG2 image.</summary>
        internal void MarkBg2(ushort word) => Inner.Enemies.Presentation.RequireBg2Image().Write(0, [word]);
        /// <summary>Runs the fixture's production enemy drawing path.</summary>
        internal OamBuffer Draw() => Inner.Draw();
        /// <summary>Clears her BG2 image before checking fresh publication.</summary>
        internal void ClearBg2() => Inner.Enemies.Presentation.RequireBg2Image().Write(0,
            new ushort[EnemyBg2FrameLayout.TilemapWidth * EnemyBg2FrameLayout.TilemapHeight]);
    }
}
