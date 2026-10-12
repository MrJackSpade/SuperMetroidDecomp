using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Runs the actual body instruction processor and public drawing path with mutable RAM only.</summary>
    private sealed class MotherBrainPresentationFixture
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        internal EnemyAnimationFixture Inner { get; }
        internal RoomEnemySlot Body => Inner.Actor;
        internal MotherBrainEnemyState State { get; }
        internal List<Position16<Bg2Pixels>> ScrollCalls { get; } = [];
        private readonly Action<RoomEnemySlot, SamusState?, RoomLevelData?, Position16<RoomPixels>, ushort> instructions;

        internal MotherBrainPresentationFixture(EnemyTileArtworkCatalog artwork)
        {
            Inner = new EnemyAnimationFixture(artwork, golden: false);
            EnemyDefinitionId bodyId = EnemyDefinitionId.MotherBrainBody;
            Body.EnemyDefinitionPointer = bodyId;
            Body.Definition = RoomEnemyDefinitionCatalog.Get(bodyId);
            Body.AiBank = Body.Definition.Bank;
            Body.Position = Body.Position.WithWhole(new(128, 128));
            Body.PaletteIndex = Body.VramTilesIndex = 0;
            State = new MotherBrainEnemyState(Body) { Head = Inner.Enemies.Slots[1], Form = 3 };
            typeof(RoomEnemySystem).GetField("<MotherBrain>k__BackingField", Private)!.SetValue(Inner.Enemies, State);
            Inner.Enemies.VerificationServices().SetMotherBrainBg2Scroll = (Action<Position16<Bg2Pixels>>)ScrollCalls.Add;
            var queues = (List<ushort>[])typeof(RoomEnemySystem).GetField("_drawQueues", Private)!.GetValue(Inner.Enemies)!;
            queues[0].Add(Body.NativeIndex);
            instructions = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", Private)!
                .CreateDelegate<Action<RoomEnemySlot, SamusState?, RoomLevelData?, Position16<RoomPixels>, ushort>>(Inner.Enemies);
        }

        internal void SetFrame(ushort frame, bool fresh)
        {
            Body.SpritemapPointer = frame;
            Body.ExtraProperties = (ushort)(EnemyExtraProperties.UsesExtendedSpritemap |
                (fresh ? EnemyExtraProperties.NewInstructionFrame : 0));
        }

        internal void SetProgram(ushort pointer)
        {
            Body.CurrentInstruction = pointer; Body.InstructionTimer = 1; Body.Timer = 0;
        }

        internal void Step() => instructions(Body, Inner.Samus, Inner.Level, Position16<RoomPixels>.Zero, 0);
        internal OamBuffer Draw() => Inner.Draw();
        [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
        internal void ClearBg2() => Inner.Vram.ExecuteWordTransfer(
            new ushort[EnemyBg2FrameLayout.TilemapWidth * EnemyBg2FrameLayout.TilemapHeight], EnemyBg2FrameLayout.VramBase, 1);
    }
}
