using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>One compiled boss actor with RAM only, exercising the public queued draw path.</summary>
    private sealed class BossDisplayFixture
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        internal SuperMetroidAddressSpace Memory { get; } = SuperMetroidAddressSpace.CreateWithoutCartridge();
        internal SnesVram Vram { get; } = new();
        internal RoomEnemySystem Enemies { get; }
        internal RoomEnemySlot Actor => Enemies.Slots[0];
        private readonly Action<RoomEnemySlot, SamusState?, RoomLevelData?, ushort, ushort, ushort> instructions;
        private readonly HitboxQuery hitbox;
        private delegate bool HitboxQuery(RoomEnemySlot actor, ushort x, ushort y,
            ushort radiusX, ushort radiusY, bool shot, out ushort callback);

        internal BossDisplayFixture(EnemyDefinitionId definition, EnemyTileArtworkCatalog artwork)
        {
            Enemies = new RoomEnemySystem { TileArtwork = artwork };
            Bind("_bus", Memory); Bind("_vram", Vram); Bind("_cgram", new SnesCgram());
            Actor.EnemyDefinitionPointer = definition;
            Actor.Definition = RoomEnemyDefinitionCatalog.Get(definition);
            Actor.AiBank = Actor.Definition.Bank;
            Actor.XPosition = Actor.YPosition = 128;
            Actor.Health = Actor.Definition.Health;
            Actor.XRadius = Actor.Definition.XRadius; Actor.YRadius = Actor.Definition.YRadius;
            Actor.Properties = (ushort)EnemyProperties.ProcessInstructions;
            var queues = (List<ushort>[])typeof(RoomEnemySystem).GetField("_drawQueues", Private)!.GetValue(Enemies)!;
            queues[0].Add(Actor.NativeIndex);
            instructions = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", Private)!
                .CreateDelegate<Action<RoomEnemySlot, SamusState?, RoomLevelData?, ushort, ushort, ushort>>(Enemies);
            hitbox = typeof(RoomEnemySystem).GetMethod("TryFindExtendedHitboxCallback", Private)!
                .CreateDelegate<HitboxQuery>(Enemies);
        }

        internal void SetFrame(ushort frame, bool fresh)
        {
            Actor.SpritemapPointer = frame;
            Actor.ExtraProperties = (ushort)(EnemyExtraProperties.UsesExtendedSpritemap |
                (fresh ? EnemyExtraProperties.NewInstructionFrame : 0));
        }

        internal OamBuffer Draw()
        {
            var result = new OamBuffer(); result.BeginFrame();
            Enemies.DrawLayers(result, 0, 0, 0, 0);
            return result;
        }

        internal void SetProgram(ushort pointer)
        { Actor.CurrentInstruction = pointer; Actor.InstructionTimer = 1; Actor.Timer = 0; }
        internal void Step() => instructions(Actor, null, null, 0, 0, 0);
        internal (bool Hit, ushort Callback) Hitbox(ushort x, ushort y, bool shot)
        { bool hit = hitbox(Actor, x, y, 2, 2, shot, out ushort callback); return (hit, callback); }
        internal void ClearBg2() => Vram.ExecuteWordTransfer(
            new ushort[EnemyBg2FrameLayout.TilemapWidth * EnemyBg2FrameLayout.TilemapHeight], EnemyBg2FrameLayout.VramBase, 1);
        private void Bind(string name, object value) => typeof(RoomEnemySystem).GetField(name, Private)!.SetValue(Enemies, value);
    }
}
