using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Represents boss display bindings fixture verification.</summary>
internal static class BossDisplayBindingsFixtureVerification
{
    /// <summary>One compiled boss actor with RAM only, exercising the public queued draw path.</summary>
    internal sealed class BossDisplayFixture
    {
        /// <summary>Stores private.</summary>
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        /// <summary>Gets memory.</summary>
        internal SuperMetroidAddressSpace Memory { get; } = SuperMetroidAddressSpace.CreateWithoutCartridge();
        /// <summary>Gets vram.</summary>
        internal SnesVram Vram { get; } = new();
        /// <summary>Gets enemies.</summary>
        internal RoomEnemySystem Enemies { get; }
        /// <summary>Gets actor.</summary>
        internal RoomEnemySlot Actor => Enemies.SlotTable.Slots[0];
        /// <summary>Stores instructions.</summary>
        private readonly Action<RoomEnemySlot, SamusState?, RoomLevelData?, Position16<RoomPixels>, ushort> instructions;
        /// <summary>Stores hitbox.</summary>
        private readonly HitboxQuery hitbox;
        /// <summary>Represents the hitbox query callback.</summary>
        private delegate bool HitboxQuery(RoomEnemySlot actor, Position16<RoomPixels> position,
            Extent16<RoomPixels> radius, bool shot, out ushort callback);

        /// <summary>Initializes a new boss display fixture instance.</summary>
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

        /// <summary>Sets frame.</summary>
        internal void SetFrame(ushort frame, bool fresh)
        {
            Actor.SpritemapPointer = frame;
            Actor.ExtraProperties = (ushort)(EnemyExtraProperties.UsesExtendedSpritemap |
                (fresh ? EnemyExtraProperties.NewInstructionFrame : 0));
        }

        /// <summary>Draws.</summary>
        internal OamBuffer Draw()
        {
            var result = new OamBuffer(); result.BeginFrame();
            Enemies.Drawing.DrawLayers(result, new(0, 0), 0, 0);
            return result;
        }

        /// <summary>Sets program.</summary>
        internal void SetProgram(ushort pointer)
        { Actor.CurrentInstruction = pointer; Actor.InstructionTimer = 1; Actor.Timer = 0; }
        /// <summary>Steps.</summary>
        internal void Step() => instructions(Actor, null, null, Position16<RoomPixels>.Zero, 0);
        /// <summary>Hitbox.</summary>
        [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
        internal (bool Hit, ushort Callback) Hitbox(Position16<RoomPixels> probe, bool shot)
        { bool hit = hitbox(Actor, probe, new(2, 2), shot, out ushort callback); return (hit, callback); }
        /// <summary>Clear bg2.</summary>
        [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
        internal void ClearBg2() => Vram.ExecuteWordTransfer(
            new ushort[EnemyBg2FrameLayout.TilemapWidth * EnemyBg2FrameLayout.TilemapHeight], EnemyBg2FrameLayout.VramBase, 1);
        /// <summary>Binds.</summary>
        private void Bind(string name, object value) => typeof(RoomEnemySystem).GetField(name, Private)!.SetValue(Enemies, value);
    }
}
