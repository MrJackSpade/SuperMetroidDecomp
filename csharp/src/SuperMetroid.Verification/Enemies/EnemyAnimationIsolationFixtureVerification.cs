using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.Core.Programs;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Torizo.GoldenTorizoRightOrbFramesVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Fx;

/// <summary>Provides matching cartridge-free enemy rooms and mechanical state comparisons.</summary>
internal static class EnemyAnimationIsolationFixtureVerification
{
    /// <summary>
    /// Small RAM-only room for the statically identified ordinary/extended display-binding
    /// boundary. It deliberately starts at a known compiled AI phase, not a controller
    /// playthrough, and cannot provide cartridge bytes to production code.
    /// </summary>
    internal sealed class EnemyAnimationFixture
    {
        /// <summary>Y coordinate where solid blocks begin in the synthetic room.</summary>
        internal const int FloorY = 224;
        /// <summary>RAM-only bus that cannot supply cartridge instructions.</summary>
        internal SuperMetroidAddressSpace Memory { get; } = SuperMetroidAddressSpace.CreateWithoutCartridge();
        /// <summary>Enemy runtime fed by this fixture's room, RNG, and artwork.</summary>
        internal RoomEnemySystem Enemies { get; }
        /// <summary>The Boyon or Golden Torizo placed in the first enemy slot.</summary>
        internal RoomEnemySlot Actor => Enemies.SlotTable.Slots[0];
        /// <summary>Samus position used for the actor's proximity and hit interactions.</summary>
        internal SamusState Samus { get; } = new() { Position = FixedPosition<RoomPixels>.FromWhole(new(128, 16)) };
        /// <summary>Room collision plane with a flat floor below the actor.</summary>
        internal RoomLevelData Level { get; }
        /// <summary>Fixture VRAM receiving enemy tile presentation.</summary>
        internal SnesVram Vram { get; } = new();
        /// <summary>Fixture palette memory receiving enemy colors.</summary>
        internal SnesCgram Colors { get; } = new();
        /// <summary>Current deterministic RNG word supplied to enemy behavior.</summary>
        internal ushort RandomWord { get; private set; } = 1;
        /// <summary>Number of RNG steps consumed by the running actor.</summary>
        internal int RandomCalls { get; private set; }
        /// <summary>Number of boss-defeated event publications.</summary>
        internal int BossBitCalls { get; private set; }
        /// <summary>Golden Torizo's state when this fixture contains the boss.</summary>
        internal TorizoEnemyState? Torizo => Enemies.BombTorizoBehavior.GoldenTorizo;

        /// <summary>Creates matching terrain and services, then initializes either Boyon or a fixed Torizo attack phase.</summary>
        internal EnemyAnimationFixture(EnemyTileArtworkCatalog artwork, bool golden)
        {
            const int roomBlocks = 32;
            var layer = new ushort[roomBlocks * roomBlocks];
            for (int y = FloorY / 16; y < roomBlocks; y++)
            for (int x = 0; x < roomBlocks; x++)
                layer[y * roomBlocks + x] = RoomLevelWord.Create(0, 0, RoomCollisionType.SolidBlock).Raw;
            Level = new RoomLevelData(new Extent16<RoomBlocks>(roomBlocks, roomBlocks), new RoomLevelPlanes(layer, new byte[layer.Length], new ushort[layer.Length]), []);
            Enemies = new RoomEnemySystem { TileArtwork = artwork };
            Enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
            Enemies.Presentation._vram = Vram;
            Enemies.Presentation._cgram = Colors;
            Enemies.VerificationServices().NextRandom = (Func<ushort>)NextRandom;
            Enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => RandomWord);
            Enemies.VerificationServices().SeedRandom = (Action<ushort>)(value => RandomWord = value);
            Enemies.VerificationServices().IsAreaTorizoDefeated = (Func<bool>)(() => BossBitCalls != 0);
            Enemies.VerificationServices().SetAreaTorizoDefeated = (Action)(() => BossBitCalls++);

            Actor.Bind(golden ? EnemyDefinitionId.GoldenTorizo : EnemyDefinitionId.Boyon, Actor.Definition);
            Actor.Bind(Actor.EnemyDefinitionPointer, RoomEnemyDefinitionCatalog.Get(Actor.EnemyDefinitionPointer));
            Actor.Health = Actor.Definition.Health;
            Actor.Radius = new(Actor.Definition.Radius.X, Actor.Definition.Radius.Y);
            Actor.Layer = Actor.Definition.Layer;
            Actor.Position = Actor.Position.WithWhole(new(128, (ushort)(FloorY - Actor.Radius.Y - 12)));
            Actor.Properties = new EnemyPropertyWord(EnemyProperties.ProcessInstructions | EnemyProperties.ProcessOffScreen);
            Actor.InstructionTimer = NativeCountdown.Start(1);
            if (golden)
            {
                Actor.ExtraProperties = EnemyExtraPropertyWord.FromNativeWord((ushort)((ushort)EnemyExtraProperties.UsesExtendedSpritemap));
                Actor.SpritemapPointer = GoldenTorizoRightOrbFrames()[0];
                Enemies.BombTorizoBehavior._torizoState = new TorizoEnemyState(Actor, isGolden: true)
                {
                    Function = TorizoFunction.GoldenPreInstruction,
                    PreInstruction = TorizoPreInstruction.Idle,
                    BodyProgram = AnimationProgramCursor<TorizoBodyProgram>.Start(TorizoBodyProgram.GoldenSpewChozoOrbFacingLeftRightFootFwd0),
                    ReturnCursor = AnimationProgramCursor<TorizoBodyProgram>.Start(TorizoBodyProgram.GoldenSpewChozoOrbFacingLeftRightFootFwd0),
                    Velocity = new EightEightVector<RoomPixels>(0, 256),
                };
            }
            else
            {
                // Choose valid nonzero bounce-height and multiplier indexes; these are
                // fixture inputs, not authored animation/gameplay tuning.
                Actor.Parameter1 = (3 << 8) | 2;
                Actor.Parameter2 = Actor.Position.Y.Whole;
                new Action<RoomEnemySlot>(BoyonBehavior.InitializeBoyon)(Actor);
            }
        }

        /// <summary>Advances enemy projectiles before the enemy frame using the same frame counter.</summary>
        internal void Step(int frame)
        {
            Enemies.StepEnemyProjectiles(Level, Samus, nmiFrameCounter8: unchecked((byte)frame));
            Enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = Samus, Level = Level, NmiFrameCounter8 = unchecked((byte)frame) });
        }

        /// <summary>Captures a complete enemy and projectile OAM draw for artwork comparison.</summary>
        internal OamBuffer Draw()
        {
            var oam = new OamBuffer();
            oam.BeginFrame();
            Enemies.Drawing.DrawLayers(oam, new(0, 0), 0, 7);
            Enemies.DrawEnemyProjectiles(oam, Position16<RoomPixels>.Zero);
            return oam;
        }

        /// <summary>Starts Golden Torizo's compiled death sequence after its orb attack.</summary>
        internal void BeginDeath() => new Action<RoomEnemySlot, TorizoEnemyState>(BombTorizoBehavior.BeginBombTorizoDeath)(Actor, (Torizo ?? throw new InvalidOperationException("Verification requires Torizo.")));

        /// <summary>Advances the fixture's deterministic RNG and records each consumption.</summary>
        private ushort NextRandom()
        {
            RandomCalls++;
            RandomWord = unchecked((ushort)(RandomWord * 5 + 1));
            return RandomWord;
        }
    }

    /// <summary>Compares per-frame enemy, projectile, Samus, memory, audio, RNG, and boss-event state.</summary>
    internal static void AssertEnemyAnimationMechanics(EnemyAnimationFixture baseline,
        EnemyAnimationFixture edited, int frame)
    {
        string context = $"enemy animation replacement at frame {frame}";
        for (int index = 0; index < baseline.Enemies.SlotTable.Slots.Count; index++)
            AssertAnimationValues(baseline.Enemies.SlotTable.Slots[index], edited.Enemies.SlotTable.Slots[index], context);
        for (int index = 0; index < baseline.Enemies.Projectiles.EnemyProjectiles.Count; index++)
            AssertAnimationValues(baseline.Enemies.Projectiles.EnemyProjectiles[index], edited.Enemies.Projectiles.EnemyProjectiles[index], context);
        AssertAnimationValues(baseline.Samus, edited.Samus, context);
        AssertAnimationValues(baseline.Samus.Kinematics, edited.Samus.Kinematics, context);
        AssertAnimationValues(baseline.Enemies, edited.Enemies, context);
        // EnemyBG2Tilemap is the cartridge's staging image for the visible BG2 frame, so an
        // edited BG2 presentation legitimately changes it; every other WRAM byte must not.
        int bg2Start = SnesAddressMath.OffsetInBank(MotherBrainBg2Definitions.WorkAddress);
        int bg2End = bg2Start + MotherBrainBg2Definitions.ClearWordCount * sizeof(ushort);
        AssertSameBytes(baseline.Memory.WorkRam[..bg2Start], edited.Memory.WorkRam[..bg2Start],
            context + " preserves WRAM below the enemy BG2 staging image");
        AssertSameBytes(baseline.Memory.WorkRam[bg2End..], edited.Memory.WorkRam[bg2End..],
            context + " preserves WRAM above the enemy BG2 staging image");
        AssertTrue(baseline.Enemies.Audio.SoundRequests.SequenceEqual(edited.Enemies.Audio.SoundRequests), context + " preserves audio calls");
        AssertTrue(baseline.Enemies.Audio.MusicRequests.SequenceEqual(edited.Enemies.Audio.MusicRequests), context + " preserves music calls");
        AssertEqual(baseline.RandomWord, edited.RandomWord, context + " preserves RNG state");
        AssertEqual(baseline.RandomCalls, edited.RandomCalls, context + " preserves RNG consumption");
        AssertEqual(baseline.BossBitCalls, edited.BossBitCalls, context + " preserves boss-event publication");
    }

    /// <summary>
    /// Compares public scalar state, including instruction identities, timers, positions, and health.
    /// Reference owners and editable catalogs differ by design; collections are compared separately.
    /// </summary>
    internal static void AssertAnimationValues<T>(T baseline, T edited, string context) where T : class
    {
        foreach (PropertyInfo property in AnimationValueProperties<T>.All)
            AssertEqual(property.GetValue(baseline), property.GetValue(edited), context + " " + typeof(T).Name + "." + property.Name);
    }

    /// <summary>Selects scalar properties that can be compared without traversing fixture-owned references.</summary>
    internal static class AnimationValueProperties<T> where T : class
    {
        /// <summary>Public non-indexer value and string properties of the state type.</summary>
        internal static readonly PropertyInfo[] All = MetadataShape.Properties(typeof(T), BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.GetIndexParameters().Length == 0 &&
                (property.PropertyType.IsValueType && !property.PropertyType.IsByRefLike || property.PropertyType == typeof(string))).ToArray();
    }
}
