using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Small RAM-only room for the statically identified ordinary/extended display-binding
    /// boundary. It deliberately starts at a known compiled AI phase, not a controller
    /// playthrough, and cannot provide cartridge bytes to production code.
    /// </summary>
    private sealed class EnemyAnimationFixture
    {
        /// <summary>Reflection scope used to inject dependencies into private room-enemy fields.</summary>
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        /// <summary>Floor row used to build the fixture's solid collision surface.</summary>
        internal const int FloorY = 224;
        /// <summary>RAM-only address space supplied to the enemy system.</summary>
        internal SuperMetroidAddressSpace Memory { get; } = SuperMetroidAddressSpace.CreateWithoutCartridge();
        /// <summary>Enemy system whose actor and animation behavior are compared by the fixture.</summary>
        internal RoomEnemySystem Enemies { get; }
        /// <summary>Slot-zero actor configured as Boyon or Golden Torizo for the scenario.</summary>
        internal RoomEnemySlot Actor => Enemies.Slots[0];
        /// <summary>Player state positioned beside the fixture actor for deterministic updates.</summary>
        internal SamusState Samus { get; } = new() { XPosition = 128, YPosition = 16 };
        /// <summary>Small synthetic room with a flat solid floor and no cartridge-backed data.</summary>
        internal RoomLevelData Level { get; }
        /// <summary>VRAM state used by the room enemy system during update and drawing.</summary>
        internal SnesVram Vram { get; } = new();
        /// <summary>Palette memory used by the room enemy system during update and drawing.</summary>
        internal SnesCgram Colors { get; } = new();
        /// <summary>Deterministic shared random word returned to the enemy system.</summary>
        internal ushort RandomWord { get; private set; } = 1;
        /// <summary>Number of random values consumed by the fixture delegate.</summary>
        internal int RandomCalls { get; private set; }
        /// <summary>Number of boss-bit writes observed through the injected callbacks.</summary>
        internal int BossBitCalls { get; private set; }
        /// <summary>Typed Golden Torizo state when this fixture creates that actor.</summary>
        internal TorizoEnemyState? Torizo => Enemies.GoldenTorizo;

        /// <summary>Creates an isolated floor-room actor and injects deterministic runtime services.</summary>
        /// <param name="artwork">Tile-art catalog installed for enemy rendering.</param>
        /// <param name="golden">Selects Golden Torizo instead of Boyon when <see langword="true"/>.</param>
        internal EnemyAnimationFixture(EnemyTileArtworkCatalog artwork, bool golden)
        {
            const int roomBlocks = 32;
            var layer = new ushort[roomBlocks * roomBlocks];
            for (int y = FloorY / 16; y < roomBlocks; y++)
            for (int x = 0; x < roomBlocks; x++)
                layer[y * roomBlocks + x] = RoomLevelWord.Create(0, 0, RoomCollisionType.SolidBlock).Raw;
            Level = new RoomLevelData(roomBlocks, roomBlocks, layer, new byte[layer.Length], new ushort[layer.Length], []);
            Enemies = new RoomEnemySystem { TileArtwork = artwork };
            Bind("_bus", Memory);
            Bind("_vram", Vram);
            Bind("_cgram", Colors);
            Bind("_nextRandom", (Func<ushort>)NextRandom);
            Bind("_readRandomNumber", (Func<ushort>)(() => RandomWord));
            Bind("_setRandomNumber", (Action<ushort>)(value => RandomWord = value));
            Bind("_isAreaTorizoDefeated", (Func<bool>)(() => BossBitCalls != 0));
            Bind("_setAreaTorizoDefeated", (Action)(() => BossBitCalls++));

            Actor.EnemyDefinitionPointer = golden ? RoomEnemySystem.GoldenTorizoDefinition : RoomEnemySystem.BoyonDefinition;
            Actor.Definition = RoomEnemyDefinitionCatalog.Get(Actor.EnemyDefinitionPointer);
            Actor.AiBank = Actor.Definition.Bank;
            Actor.Health = Actor.Definition.Health;
            Actor.XRadius = Actor.Definition.XRadius;
            Actor.YRadius = Actor.Definition.YRadius;
            Actor.Layer = Actor.Definition.Layer;
            Actor.XPosition = 128;
            Actor.YPosition = (ushort)(FloorY - Actor.YRadius - 12);
            Actor.Properties = (ushort)(EnemyProperties.ProcessInstructions | EnemyProperties.ProcessOffScreen);
            Actor.InstructionTimer = 1;
            if (golden)
            {
                Actor.ExtraProperties = (ushort)EnemyExtraProperties.UsesExtendedSpritemap;
                Actor.CurrentInstruction = GoldenTorizoRightOrbInstructionProgramDefinitions.Start;
                Actor.SpritemapPointer = GoldenTorizoRightOrbFrames()[0];
                Bind("_torizoState", new TorizoEnemyState(Actor, isGolden: true)
                {
                    Function = Code("GoldenTorizoFunctionPreInstruction"),
                    PreInstruction = Code("TorizoPreInstructionIdle"),
                    ReturnInstruction = GoldenTorizoRightOrbInstructionProgramDefinitions.Start,
                    VerticalVelocity = 256,
                });
            }
            else
            {
                // Choose valid nonzero bounce-height and multiplier indexes; these are
                // fixture inputs, not authored animation/gameplay tuning.
                Actor.Parameter1 = (3 << 8) | 2;
                Actor.Parameter2 = Actor.YPosition;
                typeof(RoomEnemySystem).GetMethod("InitializeBoyon", PrivateInstance)!
                    .CreateDelegate<Action<RoomEnemySlot>>(Enemies)(Actor);
            }
        }

        /// <summary>Advances enemy projectiles and the room enemy system by one deterministic frame.</summary>
        /// <param name="frame">Frame index supplied to the NMI counter for this update.</param>
        internal void Step(int frame)
        {
            Enemies.StepEnemyProjectiles(Level, Samus, nmiFrameCounter8: unchecked((byte)frame));
            Enemies.StepFrame(0, 0, false, Samus, level: Level, nmiFrameCounter8: unchecked((byte)frame));
        }

        /// <summary>Renders room layers and enemy projectiles into a fresh OAM frame.</summary>
        /// <returns>The composed OAM buffer for comparison by the isolation assertion.</returns>
        internal OamBuffer Draw()
        {
            var oam = new OamBuffer();
            oam.BeginFrame();
            Enemies.DrawLayers(oam, 0, 0, 0, 7);
            Enemies.DrawEnemyProjectiles(oam, 0, 0);
            return oam;
        }

        /// <summary>Invokes the private death transition used by Golden Torizo's animation scenario.</summary>
        internal void BeginDeath() => typeof(RoomEnemySystem).GetMethod("BeginBombTorizoDeath",
            BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Action<RoomEnemySlot, TorizoEnemyState>>()(Actor, Torizo!);

        /// <summary>Assigns an injected service to a private room-enemy field through reflection.</summary>
        /// <param name="field">Private field name on <see cref="RoomEnemySystem"/>.</param>
        /// <param name="value">Service instance or delegate assigned to that field.</param>
        private void Bind(string field, object value) => typeof(RoomEnemySystem)
            .GetField(field, PrivateInstance)!.SetValue(Enemies, value);

        /// <summary>Reads a private compile-time constant used as a native AI function word.</summary>
        /// <param name="name">Constant field name on <see cref="RoomEnemySystem"/>.</param>
        /// <returns>The constant truncated to the enemy function word.</returns>
        private static ushort Code(string name) => (ushort)typeof(RoomEnemySystem).GetField(name,
            BindingFlags.Static | BindingFlags.NonPublic)!.GetRawConstantValue()!;

        /// <summary>Advances and returns the fixture's deterministic linear-congruential random word.</summary>
        /// <returns>The next wrapped 16-bit pseudo-random value.</returns>
        private ushort NextRandom()
        {
            RandomCalls++;
            RandomWord = unchecked((ushort)(RandomWord * 5 + 1));
            return RandomWord;
        }
    }

    /// <summary>Confirms replacing enemy animation data preserves per-frame gameplay state, side effects, and random consumption.</summary>
    /// <param name="baseline">Fixture running the original animation definitions.</param>
    /// <param name="edited">Fixture running the replacement animation definitions.</param>
    /// <param name="frame">Frame index included in assertion context when values differ.</param>
    private static void AssertEnemyAnimationMechanics(EnemyAnimationFixture baseline,
        EnemyAnimationFixture edited, int frame)
    {
        string context = $"enemy animation replacement at frame {frame}";
        for (int index = 0; index < baseline.Enemies.Slots.Count; index++)
            AssertAnimationValues(baseline.Enemies.Slots[index], edited.Enemies.Slots[index], context);
        for (int index = 0; index < baseline.Enemies.EnemyProjectiles.Count; index++)
            AssertAnimationValues(baseline.Enemies.EnemyProjectiles[index], edited.Enemies.EnemyProjectiles[index], context);
        AssertAnimationValues(baseline.Samus, edited.Samus, context);
        AssertAnimationValues(baseline.Samus.Kinematics, edited.Samus.Kinematics, context);
        AssertAnimationValues(baseline.Enemies, edited.Enemies, context);
        // EnemyBG2Tilemap is the cartridge's staging image for the visible BG2 frame, so an
        // edited BG2 presentation legitimately changes it; every other WRAM byte must not.
        int bg2Start = MotherBrainBg2Definitions.WorkAddress & 0xffff;
        int bg2End = bg2Start + MotherBrainBg2Definitions.ClearWordCount * sizeof(ushort);
        AssertSameBytes(baseline.Memory.WorkRam[..bg2Start], edited.Memory.WorkRam[..bg2Start],
            context + " preserves WRAM below the enemy BG2 staging image");
        AssertSameBytes(baseline.Memory.WorkRam[bg2End..], edited.Memory.WorkRam[bg2End..],
            context + " preserves WRAM above the enemy BG2 staging image");
        AssertSameBytes(baseline.Memory.SaveRam, edited.Memory.SaveRam, context + " preserves all SRAM");
        AssertTrue(baseline.Enemies.SoundRequests.SequenceEqual(edited.Enemies.SoundRequests), context + " preserves audio calls");
        AssertTrue(baseline.Enemies.MusicRequests.SequenceEqual(edited.Enemies.MusicRequests), context + " preserves music calls");
        AssertEqual(baseline.RandomWord, edited.RandomWord, context + " preserves RNG state");
        AssertEqual(baseline.RandomCalls, edited.RandomCalls, context + " preserves RNG consumption");
        AssertEqual(baseline.BossBitCalls, edited.BossBitCalls, context + " preserves boss-event publication");
    }

    // Public value fields include native instruction/frame identities, timers, positions,
    // health and typed extra state. Do not compare reference owners or editable catalogs:
    // they intentionally belong to different instances. Non-value collections above are
    // compared explicitly, so a matching endpoint cannot hide a changed intermediate frame.
    /// <summary>Compares all public scalar properties exposed by the cached value-property selection for one object type.</summary>
    /// <typeparam name="T">Reference type whose value properties are compared.</typeparam>
    /// <param name="baseline">Object produced with the baseline animation data.</param>
    /// <param name="edited">Corresponding object produced with replacement animation data.</param>
    /// <param name="context">Description attached to any failed property comparison.</param>
    private static void AssertAnimationValues<T>(T baseline, T edited, string context) where T : class
    {
        foreach (PropertyInfo property in AnimationValueProperties<T>.All)
            AssertEqual(property.GetValue(baseline), property.GetValue(edited), context + " " + typeof(T).Name + "." + property.Name);
    }

    /// <summary>Caches the public, non-indexed scalar properties used by frame-by-frame isolation comparisons.</summary>
    /// <typeparam name="T">Object type whose observable value properties are selected.</typeparam>
    private static class AnimationValueProperties<T> where T : class
    {
        /// <summary>Public instance properties with value-type or string values, excluding indexers.</summary>
        internal static readonly PropertyInfo[] All = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.GetIndexParameters().Length == 0 &&
                (property.PropertyType.IsValueType && !property.PropertyType.IsByRefLike || property.PropertyType == typeof(string))).ToArray();
    }
}
