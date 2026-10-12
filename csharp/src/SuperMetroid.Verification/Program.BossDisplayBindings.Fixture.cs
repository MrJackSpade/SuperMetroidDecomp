using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static class BossDisplayBindingsFixtureVerification
{
    /// <summary>One compiled boss actor with RAM only, exercising the public queued draw path.</summary>
    internal sealed class BossDisplayFixture
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        internal SuperMetroidAddressSpace Memory { get; } = SuperMetroidAddressSpace.CreateWithoutCartridge();
        internal SnesVram Vram { get; } = new();
        internal RoomEnemySystem Enemies { get; }
        internal RoomEnemySlot Actor => Enemies.SlotTable.Slots[0];
        private readonly Action<RoomEnemySlot, SamusState?, RoomLevelData?, Position16<RoomPixels>, ushort> instructions;
        private readonly HitboxQuery hitbox;
        private delegate bool HitboxQuery(RoomEnemySlot actor, Position16<RoomPixels> position,
            Extent16<RoomPixels> radius, bool shot, out ushort callback);

        internal BossDisplayFixture(EnemyDefinitionId definition, EnemyTileArtworkCatalog artwork)
        {
            Enemies = new RoomEnemySystem { TileArtwork = artwork };
            Enemies.BindFixtureBus(Memory); Enemies.Presentation._vram = Vram; Enemies.Presentation._cgram = new SnesCgram();
            Actor.EnemyDefinitionPointer = definition;
            Actor.Definition = RoomEnemyDefinitionCatalog.Get(definition);
            Actor.AiBank = Actor.Definition.Bank;
            Actor.Position = Actor.Position.WithWhole(new(128, 128));
            Actor.Health = Actor.Definition.Health;
            Actor.Radius = new(Actor.Definition.Radius.X, Actor.Definition.Radius.Y);
            Actor.Properties = (ushort)EnemyProperties.ProcessInstructions;
            Enemies.Drawing.Enqueue(0, Actor.NativeIndex);
            instructions = new Action<RoomEnemySlot, SamusState?, RoomLevelData?, Position16<RoomPixels>, ushort>(Enemies.Instructions.ProcessInstructions);
            hitbox = new HitboxQuery(Enemies.Collision.TryFindExtendedHitboxCallback);
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
            Enemies.Drawing.DrawLayers(result, new(0, 0), 0, 0);
            return result;
        }

        internal void SetProgram(ushort pointer)
        { Actor.CurrentInstruction = pointer; Actor.InstructionTimer = 1; Actor.Timer = 0; }
        internal void Step() => instructions(Actor, null, null, Position16<RoomPixels>.Zero, 0);
        [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
        internal (bool Hit, ushort Callback) Hitbox(Position16<RoomPixels> probe, bool shot)
        { bool hit = hitbox(Actor, probe, new(2, 2), shot, out ushort callback); return (hit, callback); }
        [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
        internal void ClearBg2() => Vram.ExecuteWordTransfer(
            new ushort[EnemyBg2FrameLayout.TilemapWidth * EnemyBg2FrameLayout.TilemapHeight], EnemyBg2FrameLayout.VramBase, 1);
        private void Bind(string name, object value) => typeof(RoomEnemySystem).GetField(name, Private)!.SetValue(Enemies, value);
    }
}
