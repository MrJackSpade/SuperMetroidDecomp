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
        /// <summary>Reflection scope used to connect the fixture to private runtime services.</summary>
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        /// <summary>Cartridge-free address space supplied to the enemy system.</summary>
        internal SuperMetroidAddressSpace Memory { get; } = SuperMetroidAddressSpace.CreateWithoutCartridge();
        /// <summary>Video memory receiving the actor's queued graphics transfers.</summary>
        internal SnesVram Vram { get; } = new();
        /// <summary>Enemy system initialized with the fixture actor and its selected artwork.</summary>
        internal RoomEnemySystem Enemies { get; }
        /// <summary>The first slot, which the constructor configures as the fixture boss.</summary>
        internal RoomEnemySlot Actor => Enemies.Slots[0];
        /// <summary>Delegate bound to the enemy system's native instruction processor.</summary>
        private readonly Action<RoomEnemySlot, SamusState?, RoomLevelData?, ushort, ushort, ushort, byte> instructions;
        /// <summary>Delegate bound to the extended-hitbox callback lookup used by production collision handling.</summary>
        private readonly HitboxQuery hitbox;

        /// <summary>Matches the runtime callback lookup signature for an actor point and collision mode.</summary>
        /// <param name="actor">Enemy slot whose extended hitbox is queried.</param>
        /// <param name="x">Horizontal point tested against the hitbox.</param>
        /// <param name="y">Vertical point tested against the hitbox.</param>
        /// <param name="radiusX">Horizontal collision radius supplied to the callback lookup.</param>
        /// <param name="radiusY">Vertical collision radius supplied to the callback lookup.</param>
        /// <param name="shot"><see langword="true"/> to select projectile collision; otherwise select touch collision.</param>
        /// <param name="callback">Receives the native AI callback pointer when a hit is found.</param>
        /// <returns><see langword="true"/> when the point intersects an applicable extended hitbox.</returns>
        private delegate bool HitboxQuery(RoomEnemySlot actor, ushort x, ushort y,
            ushort radiusX, ushort radiusY, bool shot, out ushort callback);

        /// <summary>Creates one queued boss actor and binds the fixture's RAM, VRAM, instruction, and collision services.</summary>
        /// <param name="definition">Native enemy definition pointer used to initialize the actor.</param>
        /// <param name="artwork">Compiled tile artwork catalog supplied to the enemy system.</param>
        internal BossDisplayFixture(ushort definition, EnemyTileArtworkCatalog artwork)
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
                .CreateDelegate<Action<RoomEnemySlot, SamusState?, RoomLevelData?, ushort, ushort, ushort, byte>>(Enemies);
            hitbox = typeof(RoomEnemySystem).GetMethod("TryFindExtendedHitboxCallback", Private)!
                .CreateDelegate<HitboxQuery>(Enemies);
        }

        /// <summary>Selects an extended spritemap and marks whether it begins a new instruction frame.</summary>
        /// <param name="frame">Extended spritemap pointer rendered by the actor.</param>
        /// <param name="fresh"><see langword="true"/> to set the new-instruction-frame flag.</param>
        internal void SetFrame(ushort frame, bool fresh)
        {
            Actor.SpritemapPointer = frame;
            Actor.ExtraProperties = (ushort)(EnemyExtraProperties.UsesExtendedSpritemap |
                (fresh ? EnemyExtraProperties.NewInstructionFrame : 0));
        }

        /// <summary>Draws the fixture's queued actor through the production enemy layer renderer.</summary>
        /// <returns>A completed OAM buffer containing the actor's emitted sprites.</returns>
        internal OamBuffer Draw()
        {
            var result = new OamBuffer(); result.BeginFrame();
            Enemies.DrawLayers(result, 0, 0, 0, 0);
            return result;
        }

        /// <summary>Sets the actor's instruction-list pointer and resets its instruction timers for the next step.</summary>
        /// <param name="pointer">Instruction-list pointer to execute.</param>
        internal void SetProgram(ushort pointer)
        { Actor.CurrentInstruction = pointer; Actor.InstructionTimer = 1; Actor.Timer = 0; }

        /// <summary>Runs the bound instruction processor once with the supplied frame counter.</summary>
        /// <param name="frame">Frame counter narrowed to the byte value consumed by the processor.</param>
        internal void Step(int frame) => instructions(Actor, null, null, 0, 0, 0, unchecked((byte)frame));

        /// <summary>Queries touch or projectile hitbox behavior at a point using the fixture's fixed two-pixel radii.</summary>
        /// <param name="x">Horizontal point to test.</param>
        /// <param name="y">Vertical point to test.</param>
        /// <param name="shot"><see langword="true"/> to query projectile collision; otherwise query touch collision.</param>
        /// <returns>Whether the point hit, together with the selected native callback pointer.</returns>
        internal (bool Hit, ushort Callback) Hitbox(ushort x, ushort y, bool shot)
        { bool hit = hitbox(Actor, x, y, 2, 2, shot, out ushort callback); return (hit, callback); }

        /// <summary>Clears the BG2 tilemap region in VRAM before exercising the boss display transfer.</summary>
        internal void ClearBg2() => Vram.ExecuteWordTransfer(
            new ushort[EnemyBg2FrameLayout.TilemapWidth * EnemyBg2FrameLayout.TilemapHeight], EnemyBg2FrameLayout.VramBase, 1);

        /// <summary>Assigns a private enemy-system service field for the isolated fixture.</summary>
        /// <param name="name">Private field name to locate on <see cref="RoomEnemySystem"/>.</param>
        /// <param name="value">Object assigned to the located field.</param>
        private void Bind(string name, object value) => typeof(RoomEnemySystem).GetField(name, Private)!.SetValue(Enemies, value);
    }
}
