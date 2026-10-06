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
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        internal const int FloorY = 224;
        internal SuperMetroidAddressSpace Memory { get; } = SuperMetroidAddressSpace.CreateWithoutCartridge();
        internal RoomEnemySystem Enemies { get; }
        internal RoomEnemySlot Actor => Enemies.Slots[0];
        internal SamusState Samus { get; } = new() { XPosition = 128, YPosition = 16 };
        internal RoomLevelData Level { get; }
        internal SnesVram Vram { get; } = new();
        internal SnesCgram Colors { get; } = new();
        internal ushort RandomWord { get; private set; } = 1;
        internal int RandomCalls { get; private set; }
        internal int BossBitCalls { get; private set; }
        internal TorizoEnemyState? Torizo => Enemies.GoldenTorizo;

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

        internal void Step(int frame)
        {
            Enemies.StepEnemyProjectiles(Level, Samus, nmiFrameCounter8: unchecked((byte)frame));
            Enemies.StepFrame(0, 0, false, Samus, level: Level, nmiFrameCounter8: unchecked((byte)frame));
        }

        internal OamBuffer Draw()
        {
            var oam = new OamBuffer();
            oam.BeginFrame();
            Enemies.DrawLayers(oam, 0, 0, 0, 7);
            Enemies.DrawEnemyProjectiles(oam, 0, 0);
            return oam;
        }

        internal void BeginDeath() => typeof(RoomEnemySystem).GetMethod("BeginBombTorizoDeath",
            BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Action<RoomEnemySlot, TorizoEnemyState>>()(Actor, Torizo!);

        private void Bind(string field, object value) => typeof(RoomEnemySystem)
            .GetField(field, PrivateInstance)!.SetValue(Enemies, value);

        private static ushort Code(string name) => (ushort)typeof(RoomEnemySystem).GetField(name,
            BindingFlags.Static | BindingFlags.NonPublic)!.GetRawConstantValue()!;

        private ushort NextRandom()
        {
            RandomCalls++;
            RandomWord = unchecked((ushort)(RandomWord * 5 + 1));
            return RandomWord;
        }
    }

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
        AssertTrue(baseline.Memory.WorkRam.SequenceEqual(edited.Memory.WorkRam), context + " preserves all WRAM");
        AssertTrue(baseline.Memory.SaveRam.SequenceEqual(edited.Memory.SaveRam), context + " preserves all SRAM");
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
    private static void AssertAnimationValues<T>(T baseline, T edited, string context) where T : class
    {
        foreach (PropertyInfo property in AnimationValueProperties<T>.All)
            AssertEqual(property.GetValue(baseline), property.GetValue(edited), context + " " + typeof(T).Name + "." + property.Name);
    }

    private static class AnimationValueProperties<T> where T : class
    {
        internal static readonly PropertyInfo[] All = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.GetIndexParameters().Length == 0 &&
                (property.PropertyType.IsValueType && !property.PropertyType.IsByRefLike || property.PropertyType == typeof(string))).ToArray();
    }
}
