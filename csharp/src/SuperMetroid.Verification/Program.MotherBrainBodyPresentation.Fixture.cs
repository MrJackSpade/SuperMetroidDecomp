using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static EnemyAnimationIsolationFixtureVerification;

/// <summary>Represents mother brain body presentation fixture verification.</summary>
internal static class MotherBrainBodyPresentationFixtureVerification
{
    /// <summary>Runs the actual body instruction processor and public drawing path with mutable RAM only.</summary>
    internal sealed class MotherBrainPresentationFixture
    {
        /// <summary>Stores private.</summary>
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        /// <summary>Gets inner.</summary>
        internal EnemyAnimationFixture Inner { get; }
        /// <summary>Gets body.</summary>
        internal RoomEnemySlot Body => Inner.Actor;
        /// <summary>Gets state.</summary>
        internal MotherBrainEnemyState State { get; }
        /// <summary>Gets scroll calls.</summary>
        internal List<Position16<Bg2Pixels>> ScrollCalls { get; } = [];
        /// <summary>Stores instructions.</summary>
        private readonly Action<RoomEnemySlot, SamusState?, RoomLevelData?, Position16<RoomPixels>, ushort> instructions;

        /// <summary>Initializes a new mother brain presentation fixture instance.</summary>
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

        /// <summary>Sets frame.</summary>
        internal void SetFrame(ushort frame, bool fresh)
        {
            Body.SpritemapPointer = frame;
            Body.ExtraProperties = (ushort)(EnemyExtraProperties.UsesExtendedSpritemap |
                (fresh ? EnemyExtraProperties.NewInstructionFrame : 0));
        }

        /// <summary>Sets program.</summary>
        internal void SetProgram(ushort pointer)
        {
            Body.CurrentInstruction = pointer; Body.InstructionTimer = 1; Body.Timer = 0;
        }

        /// <summary>Steps.</summary>
        internal void Step() => instructions(Body, Inner.Samus, Inner.Level, Position16<RoomPixels>.Zero, 0);
        /// <summary>Draws.</summary>
        internal OamBuffer Draw() => Inner.Draw();
        /// <summary>Clear bg2.</summary>
        [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
        internal void ClearBg2() => Inner.Vram.ExecuteWordTransfer(
            new ushort[EnemyBg2FrameLayout.TilemapWidth * EnemyBg2FrameLayout.TilemapHeight], EnemyBg2FrameLayout.VramBase, 1);
    }
}
