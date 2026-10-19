using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core;

namespace SuperMetroid.Verification.Bosses;

/// <summary>Builds a RAM-only boss actor fixture for queued OAM and hitbox checks.</summary>
internal static class BossDisplayBindingsFixtureVerification
{
    /// <summary>One compiled boss actor with RAM only, exercising the public queued draw path.</summary>
    internal sealed class BossDisplayFixture
    {
        /// <summary>Cartridge-free address space for the boss actor.</summary>
        internal SuperMetroidAddressSpace Memory { get; } = SuperMetroidAddressSpace.CreateWithoutCartridge();
        /// <summary>VRAM receiving boss tilemap transfers during draw checks.</summary>
        internal SnesVram Vram { get; } = new();
        /// <summary>Enemy owner whose ordinary queue and presentation path the fixture exercises.</summary>
        internal RoomEnemySystem Enemies { get; }
        /// <summary>The physical slot initialized as the selected boss definition.</summary>
        internal RoomEnemySlot Actor => Enemies.SlotTable.Slots[_slot];
        /// <summary>The actor's physical slot.</summary>
        private readonly int _slot;
        /// <summary>Bound production instruction interpreter for one actor step.</summary>
        private readonly Action<RoomEnemySlot, SamusState?, RoomLevelData?, Position16<RoomPixels>, SnesButton> instructions;
        /// <summary>Bound extended-hitbox query used by the collision check.</summary>
        private readonly HitboxQuery hitbox;
        /// <summary>Queries an extended hitbox and returns its callback when touched.</summary>
        private delegate bool HitboxQuery(RoomEnemySlot actor, Position16<RoomPixels> position,
            Extent16<RoomPixels> radius, bool shot, out EnemyInteractionCallback? callback);

        /// <summary>Installs one centered boss actor in <paramref name="slot"/> and enqueues it for the production draw path.</summary>
        internal BossDisplayFixture(EnemyDefinitionId definition, EnemyTileArtworkCatalog artwork, int slot = 0)
        {
            _slot = slot;
            Enemies = new RoomEnemySystem { TileArtwork = artwork };
            Enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance); Enemies.Presentation._vram = Vram; Enemies.Presentation._cgram = new SnesCgram();
            Actor.Bind(definition, Actor.Definition);
            Actor.Bind(Actor.EnemyDefinitionPointer, RoomEnemyDefinitionCatalog.Get(definition));
            Actor.Position = Actor.Position.WithWhole(new(128, 128));
            Actor.Health = Actor.Definition.Health;
            Actor.Radius = new(Actor.Definition.Radius.X, Actor.Definition.Radius.Y);
            Actor.Properties = new EnemyPropertyWord(EnemyProperties.ProcessInstructions);
            Enemies.Drawing.Enqueue(0, Actor.NativeIndex);
            instructions = new Action<RoomEnemySlot, SamusState?, RoomLevelData?, Position16<RoomPixels>, SnesButton>(EnemyInstructionInterpreter.ProcessInstructions);
            hitbox = new HitboxQuery(Enemies.Collision.TryFindExtendedHitboxCallback);
        }

        /// <summary>Selects an extended spritemap and optionally marks it as a new instruction frame.</summary>
        internal void SetFrame(ushort frame, bool fresh)
        {
            Actor.SpritemapPointer = frame;
            Actor.ExtraProperties = EnemyExtraPropertyWord.FromNativeWord((ushort)((ushort)(EnemyExtraProperties.UsesExtendedSpritemap |
                (fresh ? EnemyExtraProperties.NewInstructionFrame : 0))));
        }

        /// <summary>Runs queued enemy drawing into a fresh OAM frame.</summary>
        internal OamBuffer Draw()
        {
            var result = new OamBuffer(); result.BeginFrame();
            Enemies.Drawing.DrawLayersAsOam(result, new(0, 0), 0, 0);
            return result;
        }

        /// <summary>Starts an instruction list with a one-tick countdown.</summary>
        internal void SetProgram(ushort pointer)
        { Actor.CurrentInstruction = pointer; Actor.InstructionTimer = NativeCountdown.Start(1); Actor.Timer = NativeCountdown.Start(0); }
        /// <summary>Advances the selected actor's production instruction interpreter once.</summary>
        internal void Step() => instructions(Actor, null, null, Position16<RoomPixels>.Zero, 0);
        /// <summary>Probes the actor's extended hitbox at a two-pixel radius.</summary>
        [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
        internal (bool Hit, ushort Callback) Hitbox(Position16<RoomPixels> probe, bool shot)
        {
            bool hit = hitbox(Actor, probe, new(2, 2), shot, out EnemyInteractionCallback? callback);
            return (hit, callback is { } selected ? EnemyInteractionCallbackAccess.Word(selected) : (ushort)0);
        }
        /// <summary>Clears the boss BG2 tilemap before a presentation check.</summary>
        [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
        internal void ClearBg2() => Vram.ExecuteWordTransfer(
            new ushort[EnemyBg2FrameLayout.TilemapWidth * EnemyBg2FrameLayout.TilemapHeight], EnemyBg2FrameLayout.VramBase, 1);
    }
}
