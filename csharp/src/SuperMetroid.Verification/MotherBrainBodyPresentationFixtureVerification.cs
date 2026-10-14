using System.Reflection;
using SuperMetroid.Core.Hardware;
using static EnemyAnimationIsolationFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

/// <summary>Provides a mutable-RAM Mother Brain body fixture for instruction and drawing verification.</summary>
internal static class MotherBrainBodyPresentationFixtureVerification
{
    /// <summary>Runs the actual body instruction processor and public drawing path with mutable RAM only.</summary>
    internal sealed class MotherBrainPresentationFixture
    {
        /// <summary>Reflection flags for the production instruction processor.</summary>
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        /// <summary>Owns the enemy system, Samus, room, and installed artwork for this fixture.</summary>
        internal EnemyAnimationFixture Inner { get; }
        /// <summary>Body slot whose instruction pointer and OAM frame are exercised.</summary>
        internal RoomEnemySlot Body => Inner.Actor;
        /// <summary>Third-form body state bound to the fixture's encounter.</summary>
        internal MotherBrainEnemyState State { get; }
        /// <summary>BG2 scroll publications captured from the production callback.</summary>
        internal List<Position16<Bg2Pixels>> ScrollCalls { get; } = [];
        /// <summary>Production instruction processor bound to this fixture's enemy system.</summary>
        private readonly Action<RoomEnemySlot, SamusState?, RoomLevelData?, Position16<RoomPixels>, ushort> instructions;

        /// <summary>Creates and binds a third-form body with installed artwork and captured BG2 scroll calls.</summary>
        internal MotherBrainPresentationFixture(EnemyTileArtworkCatalog artwork)
        {
            Inner = new EnemyAnimationFixture(artwork, golden: false);
            EnemyDefinitionId bodyId = EnemyDefinitionId.MotherBrainBody;
            Body.EnemyDefinitionPointer = bodyId;
            Body.Definition = RoomEnemyDefinitionCatalog.Get(bodyId);
            Body.AiBank = Body.Definition.Bank;
            Body.Position = Body.Position.WithWhole(new(128, 128));
            Body.PaletteIndex = Body.VramTilesIndex = 0;
            State = new MotherBrainEnemyState(Body) { Head = Inner.Enemies.SlotTable.Slots[1], Form = 3 };
            Inner.Enemies.MotherBrainEncounter.MotherBrain = State;
            Inner.Enemies.VerificationServices().SetMotherBrainBg2Scroll = (Action<Position16<Bg2Pixels>>)ScrollCalls.Add;
            Inner.Enemies.Drawing.Enqueue(0, Body.NativeIndex);
            instructions = new Action<RoomEnemySlot, SamusState?, RoomLevelData?, Position16<RoomPixels>, ushort>(Inner.Enemies.Instructions.ProcessInstructions);
        }

        /// <summary>Selects a body frame and optionally marks it as a new instruction frame.</summary>
        internal void SetFrame(ushort frame, bool fresh)
        {
            Body.SpritemapPointer = frame;
            Body.ExtraProperties = (ushort)(EnemyExtraProperties.UsesExtendedSpritemap |
                (fresh ? EnemyExtraProperties.NewInstructionFrame : 0));
        }

        /// <summary>Starts a body program at its next one-tick dispatch.</summary>
        internal void SetProgram(MotherBrainBodyProgram program) => MotherBrainBehavior.SetMotherBrainBodyProgram(State, program);

        /// <summary>Advances the body's actual production instruction processor once.</summary>
        internal void Step() => instructions(Body, Inner.Samus, Inner.Level, Position16<RoomPixels>.Zero, 0);
        /// <summary>Runs the fixture's production enemy drawing path.</summary>
        internal OamBuffer Draw() => Inner.Draw();
        /// <summary>Clears the native BG2 tilemap region before checking fresh publication.</summary>
        [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
        internal void ClearBg2() => Inner.Vram.ExecuteWordTransfer(
            new ushort[EnemyBg2FrameLayout.TilemapWidth * EnemyBg2FrameLayout.TilemapHeight], EnemyBg2FrameLayout.VramBase, 1);
    }
}
