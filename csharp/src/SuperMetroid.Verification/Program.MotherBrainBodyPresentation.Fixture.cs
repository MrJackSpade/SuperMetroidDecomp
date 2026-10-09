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
        /// <summary>Reflection flags for accessing the room system's instance implementation details.</summary>
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

        /// <summary>Gets the underlying animation fixture that supplies the room, enemy, and drawing state.</summary>
        internal EnemyAnimationFixture Inner { get; }

        /// <summary>Gets Mother Brain's body slot, which the fixture drives through its instruction list.</summary>
        internal RoomEnemySlot Body => Inner.Actor;

        /// <summary>Gets the Mother Brain state attached to the body and consulted by presentation logic.</summary>
        internal MotherBrainEnemyState State { get; }

        /// <summary>Gets the BG2 scroll coordinates emitted by the body's presentation updates.</summary>
        internal List<(ushort X, ushort Y)> ScrollCalls { get; } = [];

        /// <summary>Delegate to the production instruction processor for advancing the body slot.</summary>
        private readonly Action<RoomEnemySlot, SamusState?, RoomLevelData?, ushort, ushort, ushort, byte> instructions;

        /// <summary>Builds a mutable fixture around the production Mother Brain body and drawing code.</summary>
        /// <param name="artwork">Tile artwork catalog supplied to the underlying enemy animation fixture.</param>
        internal MotherBrainPresentationFixture(EnemyTileArtworkCatalog artwork)
        {
            Inner = new EnemyAnimationFixture(artwork, golden: false);
            ushort bodyId = (ushort)typeof(RoomEnemySystem).GetField("MotherBrainBodyDefinition",
                BindingFlags.Static | BindingFlags.NonPublic)!.GetRawConstantValue()!;
            Body.EnemyDefinitionPointer = bodyId;
            Body.Definition = RoomEnemyDefinitionCatalog.Get(bodyId);
            Body.AiBank = Body.Definition.Bank;
            Body.XPosition = Body.YPosition = 128;
            Body.PaletteIndex = Body.VramTilesIndex = 0;
            State = new MotherBrainEnemyState(Body) { Head = Inner.Enemies.Slots[1], Form = 3 };
            typeof(RoomEnemySystem).GetField("_motherBrain", Private)!.SetValue(Inner.Enemies, State);
            typeof(RoomEnemySystem).GetField("_setMotherBrainBg2Scroll", Private)!.SetValue(Inner.Enemies,
                (Action<ushort, ushort>)((x, y) => ScrollCalls.Add((x, y))));
            var queues = (List<ushort>[])typeof(RoomEnemySystem).GetField("_drawQueues", Private)!.GetValue(Inner.Enemies)!;
            queues[0].Add(Body.NativeIndex);
            instructions = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", Private)!
                .CreateDelegate<Action<RoomEnemySlot, SamusState?, RoomLevelData?, ushort, ushort, ushort, byte>>(Inner.Enemies);
        }

        /// <summary>Sets the body's spritemap pointer and marks whether this is a newly loaded instruction frame.</summary>
        /// <param name="frame">Spritemap pointer installed on the body slot.</param>
        /// <param name="fresh">Whether the extended-spritemap metadata should signal a new instruction frame.</param>
        internal void SetFrame(ushort frame, bool fresh)
        {
            Body.SpritemapPointer = frame;
            Body.ExtraProperties = (ushort)(EnemyExtraProperties.UsesExtendedSpritemap |
                (fresh ? EnemyExtraProperties.NewInstructionFrame : 0));
        }

        /// <summary>Selects an instruction-list entry and resets its countdown timers for the next step.</summary>
        /// <param name="pointer">Instruction address to install on the body slot.</param>
        internal void SetProgram(ushort pointer)
        {
            Body.CurrentInstruction = pointer; Body.InstructionTimer = 1; Body.Timer = 0;
        }

        /// <summary>Advances the body's production instruction processor once using the fixture's game state.</summary>
        /// <param name="frame">Frame value passed to the processor, narrowed to the native byte argument.</param>
        internal void Step(int frame) => instructions(Body, Inner.Samus, Inner.Level, 0, 0, 0, unchecked((byte)frame));

        /// <summary>Runs the underlying fixture's public enemy drawing path and returns its OAM output.</summary>
        /// <returns>OAM entries produced while drawing the current fixture state.</returns>
        internal OamBuffer Draw() => Inner.Draw();

        /// <summary>Clears the BG2 tilemap through the fixture VRAM transfer path.</summary>
        internal void ClearBg2() => Inner.Vram.ExecuteWordTransfer(
            new ushort[EnemyBg2FrameLayout.TilemapWidth * EnemyBg2FrameLayout.TilemapHeight], EnemyBg2FrameLayout.VramBase, 1);
    }
}
