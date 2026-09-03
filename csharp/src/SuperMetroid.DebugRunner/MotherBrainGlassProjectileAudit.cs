using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed lifecycle audit for the glass shards and sparkles emitted while Mother Brain's
/// tank breaks. The room PLM producer is covered by <see cref="MotherBrainAudit"/>; this audit
/// isolates the two bank-$86 actors so all sixteen RNG-selected shard lists, fixed-point
/// movement, child allocation order, rendering, interaction flags, and deletion can be proven
/// without twelve room turrets competing for the same eighteen physical slots.
/// </summary>
internal static class MotherBrainGlassProjectileAudit
{
    private const ushort RoomPointer = 0xdd58;
    private const ushort ShardDefinition = 0xcefc;
    private const ushort SparkleDefinition = 0xcf0a;
    private const ushort ShardInstructionTable = 0xce41;
    private const ushort ShardGraphicsIndex = 0x0640;

    private static readonly ushort[] ExpectedShardLists =
    [
        0xcc93, 0xccb7, 0xccb7, 0xccdb,
        0xccdb, 0xccdb, 0xccff, 0xccff,
        0xcd23, 0xcd47, 0xcd47, 0xcd6b,
        0xcd6b, 0xcd6b, 0xcd8f, 0xcd8f,
    ];

    private static readonly short[] ExpectedShardXOffsets = [8, -40, -16];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);

        VerifyDefinitionRecords(bus);
        OrientationResult orientations = VerifyAllShardOrientations(bus, room, assets);
        SparkleResult sparkle = VerifySparkleSamePassLifecycle(bus, room, assets);

        Console.WriteLine(
            "Mother Brain glass projectile audit passed: exact $CEFC/$CF0A definitions, " +
            $"all 16 RNG orientation selectors, {orientations.UniqueLists} unique shard " +
            $"loops/{orientations.UniqueMaps} authored maps, 8.8 flight/gravity and natural " +
            $"page deletion matched; sparkle slot ${sparkle.NativeSlot:X2} ran in its " +
            $"parent's physical pass, rendered {sparkle.ObjPieces} OBJ pieces, exposed all " +
            "four finite maps, ignored beams, and deleted on its exact ROM timer.");
        return 0;
    }

    /// <summary>
    /// Pins the fourteen bytes of each projectile header. These are adjacent records in the
    /// cartridge, so checking every word catches a shifted pointer just as readily as a wrong
    /// initializer, pre-instruction, list, collision property, or touch/shot list.
    /// </summary>
    private static void VerifyDefinitionRecords(ISnesAddressSpace bus)
    {
        ushort[] expectedShard = [0xcdc5, 0xce9b, 0xcc93, 0x0000, 0x3000, 0x0000, 0x84fc];
        ushort[] expectedSparkle = [0xce6d, 0x84fb, 0xcdb3, 0x0000, 0x3000, 0x0000, 0x84fc];
        VerifyDefinition(bus, ShardDefinition, expectedShard);
        VerifyDefinition(bus, SparkleDefinition, expectedSparkle);

        for (int index = 0; index < ExpectedShardLists.Length; index++)
        {
            ushort actual = ReadWord(bus, 0x860000 | (ShardInstructionTable + index * 2));
            if (actual != ExpectedShardLists[index])
            {
                throw new InvalidDataException(
                    $"Mother Brain shard list selector {index} is $86:{actual:X4}, " +
                    $"expected $86:{ExpectedShardLists[index]:X4}.");
            }
        }
    }

    private static void VerifyDefinition(
        ISnesAddressSpace bus,
        ushort definition,
        ushort[] expectedWords)
    {
        for (int wordIndex = 0; wordIndex < expectedWords.Length; wordIndex++)
        {
            ushort actual = ReadWord(
                bus,
                0x860000 | unchecked((ushort)(definition + wordIndex * 2)));
            if (actual != expectedWords[wordIndex])
            {
                throw new InvalidDataException(
                    $"Enemy-projectile definition $86:{definition:X4} word {wordIndex} " +
                    $"is ${actual:X4}, expected ${expectedWords[wordIndex]:X4}.");
            }
        }
    }

    /// <summary>
    /// Drives every value of <c>((random * 2) &amp; $1FE) >> 5</c>. A fresh empty bank-$86
    /// pool keeps physical allocation fixed at `$22`, while the request still uses the real
    /// Mother Brain room coordinates and the production PLM-to-projectile entry point.
    /// </summary>
    private static OrientationResult VerifyAllShardOrientations(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var selectedLists = new HashSet<ushort>();
        var allMaps = new HashSet<ushort>();

        for (int orientation = 0; orientation < 16; orientation++)
        {
            var random = new ScriptedRandom(defaultValue: 0x0020);
            LoadedGlass loaded = LoadEmptyPool(bus, room, assets, random);

            // First sample selects the orientation bin. Jitter samples eight/eight cancel
            // the initializer's `-8`, leaving a coordinate that can be checked directly.
            random.Enqueue(unchecked((ushort)(orientation << 4)), 8, 8);
            ushort parameter = unchecked((ushort)((orientation % 3) * 2));
            loaded.Enemies.SpawnMotherBrainGlassProjectile(
                new MotherBrainGlassProjectileRequest(
                    ShardDefinition,
                    parameter,
                    PlmBlockX: 9,
                    PlmBlockY: 5));

            RoomEnemyProjectileSlot shard = loaded.Enemies.EnemyProjectiles[17];
            ushort expectedAngleOffset = unchecked((ushort)(orientation << 5));
            int angle = expectedAngleOffset >> 1;
            // Native uses the even angle offset directly as X into two table bases:
            // `$B443,x` is cosine and `$B3C3,x` is sine. Keeping both addresses here
            // catches either a missing quarter-turn or a doubled one in production.
            ushort expectedXVelocity = ReadWord(bus, 0xa0b443 + angle * 2);
            ushort expectedYVelocity = unchecked((ushort)(
                unchecked((short)ReadWord(bus, 0xa0b3c3 + angle * 2)) * 4));
            ushort expectedList = ExpectedShardLists[orientation];
            int offsetIndex = parameter >> 1;
            ushort expectedX = unchecked((ushort)(
                9 * 16 + ExpectedShardXOffsets[offsetIndex]));
            ushort expectedY = unchecked((ushort)(5 * 16 + 32));

            if (loaded.Enemies.ActiveEnemyProjectileCount != 1 ||
                shard.Kind != RoomEnemyProjectileKind.MotherBrainGlassShard ||
                shard.SlotIndex != 17 || shard.PreInstruction != 0xce9b ||
                shard.InstructionPointer != expectedList || shard.InstructionTimer != 1 ||
                shard.SpritemapPointer != 0x8000 || shard.GraphicsIndex != ShardGraphicsIndex ||
                shard.Variable0 != expectedAngleOffset || shard.Variable1 != 0 ||
                shard.XPosition != expectedX || shard.YPosition != expectedY ||
                shard.XSubposition != 0 || shard.YSubposition != 0 ||
                shard.XVelocity != expectedXVelocity || shard.YVelocity != expectedYVelocity ||
                shard.XRadius != 0 || shard.YRadius != 0 || shard.Damage != 0 ||
                shard.CanDamageSamus || shard.PersistsOnSamusContact ||
                shard.BlocksSamusProjectiles || random.CallCount != 3)
            {
                throw new InvalidDataException(
                    $"Mother Brain shard orientation {orientation} initialization diverged: " +
                    $"slot={shard.SlotIndex}, kind=$86:{(ushort)shard.Kind:X4}, list=" +
                    $"$86:{shard.InstructionPointer:X4}, timer/map={shard.InstructionTimer}/" +
                    $"${shard.SpritemapPointer:X4}, angle=${shard.Variable0:X4}, position=" +
                    $"(${shard.XPosition:X4}.{shard.XSubposition:X4}," +
                    $"${shard.YPosition:X4}.{shard.YSubposition:X4}), velocity=" +
                    $"(${shard.XVelocity:X4},${shard.YVelocity:X4}), RNG={random.CallCount}.");
            }

            selectedLists.Add(shard.InstructionPointer);
            HashSet<ushort> expectedMaps = ReadLoopingShardMaps(bus, expectedList);
            uint expectedFixedX = PackFixed(shard.XPosition, shard.XSubposition);
            uint expectedFixedY = PackFixed(shard.YPosition, shard.YSubposition);
            ushort fallingVelocity = shard.YVelocity;
            var observedMaps = new HashSet<ushort>();

            // Twenty-four frames are one complete eight-map loop and leave even the fastest
            // downward orientation on page zero. Default RNG `$20` fails the sparkle mask, so
            // a child cannot perturb either the expected call count or OAM observation.
            for (int frame = 0; frame < 24; frame++)
            {
                expectedFixedX = AddEightBitVelocityReference(expectedFixedX, shard.XVelocity);
                expectedFixedY = AddEightBitVelocityReference(expectedFixedY, fallingVelocity);
                fallingVelocity = unchecked((ushort)(fallingVelocity + 0x0020));

                loaded.Enemies.StepEnemyProjectiles(
                    assets.LevelData,
                    samus: null,
                    cameraX: 0,
                    cameraY: 0,
                    nmiFrameCounter8: unchecked((byte)frame));
                if (!shard.IsActive ||
                    PackFixed(shard.XPosition, shard.XSubposition) != expectedFixedX ||
                    PackFixed(shard.YPosition, shard.YSubposition) != expectedFixedY ||
                    shard.YVelocity != fallingVelocity ||
                    loaded.Enemies.ActiveEnemyProjectileCount != 1)
                {
                    throw new InvalidDataException(
                        $"Mother Brain shard orientation {orientation} motion diverged on " +
                        $"frame {frame + 1}: active={shard.IsActive}, position=" +
                        $"${PackFixed(shard.XPosition, shard.XSubposition):X8}/" +
                        $"${PackFixed(shard.YPosition, shard.YSubposition):X8}, expected=" +
                        $"${expectedFixedX:X8}/${expectedFixedY:X8}, Y velocity=" +
                        $"${shard.YVelocity:X4}/${fallingVelocity:X4}.");
                }
                observedMaps.Add(shard.SpritemapPointer);
            }

            if (!observedMaps.SetEquals(expectedMaps) || random.CallCount != 27)
            {
                throw new InvalidDataException(
                    $"Mother Brain shard orientation {orientation} animation/RNG diverged: " +
                    $"maps=[{string.Join(',', observedMaps.Select(map => map.ToString("X4")))}], " +
                    $"expected=[{string.Join(',', expectedMaps.Select(map => map.ToString("X4")))}], " +
                    $"RNG={random.CallCount}/27.");
            }
            allMaps.UnionWith(observedMaps);

            var oam = new OamBuffer();
            oam.BeginFrame();
            loaded.Enemies.DrawEnemyProjectiles(oam, cameraX: 0, cameraY: 0);
            oam.FinalizeFrame();
            if (oam.LastFinalizedSpriteCount == 0)
                throw new InvalidDataException($"Mother Brain shard orientation {orientation} emitted no OBJ.");

            // Continue the exact fixed-point reference until the whole-coordinate high byte
            // leaves zero. The pre-instruction clears before applying gravity or animation
            // on that terminal frame, which the reference deliberately checks separately.
            int disposalFrame = 24;
            while (shard.IsActive && disposalFrame < 512)
            {
                disposalFrame++;
                expectedFixedX = AddEightBitVelocityReference(expectedFixedX, shard.XVelocity);
                expectedFixedY = AddEightBitVelocityReference(expectedFixedY, fallingVelocity);
                bool shouldDelete = ((expectedFixedY >> 16) & 0xff00) != 0;
                if (!shouldDelete)
                    fallingVelocity = unchecked((ushort)(fallingVelocity + 0x0020));

                loaded.Enemies.StepEnemyProjectiles(
                    assets.LevelData,
                    samus: null,
                    cameraX: 0,
                    cameraY: 0,
                    nmiFrameCounter8: unchecked((byte)disposalFrame));
                if (shard.IsActive == shouldDelete)
                {
                    throw new InvalidDataException(
                        $"Mother Brain shard orientation {orientation} page disposal " +
                        $"diverged on frame {disposalFrame}: active={shard.IsActive}, " +
                        $"expected delete={shouldDelete}, Y=${expectedFixedY:X8}.");
                }
                if (shard.IsActive &&
                    (PackFixed(shard.XPosition, shard.XSubposition) != expectedFixedX ||
                     PackFixed(shard.YPosition, shard.YSubposition) != expectedFixedY ||
                     shard.YVelocity != fallingVelocity))
                {
                    throw new InvalidDataException(
                        $"Mother Brain shard orientation {orientation} late motion diverged " +
                        $"on frame {disposalFrame}.");
                }
            }
            if (shard.IsActive || disposalFrame >= 512)
                throw new InvalidDataException($"Mother Brain shard orientation {orientation} never left page zero.");
        }

        return new OrientationResult(selectedLists.Count, allMaps.Count);
    }

    /// <summary>
    /// Forces the shard's one-in-sixteen sparkle branch. Because the shard owns physical slot
    /// `$22`, allocation returns `$20`; the repaired descending scheduler must reach that
    /// newborn before returning from the same <c>StepEnemyProjectiles</c> call.
    /// </summary>
    private static SparkleResult VerifySparkleSamePassLifecycle(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var random = new ScriptedRandom(defaultValue: 0x0020);
        LoadedGlass loaded = LoadEmptyPool(bus, room, assets, random);
        random.Enqueue(0, 8, 8);
        loaded.Enemies.SpawnMotherBrainGlassProjectile(
            new MotherBrainGlassProjectileRequest(
                ShardDefinition,
                Parameter: 0,
                PlmBlockX: 9,
                PlmBlockY: 5));
        RoomEnemyProjectileSlot shard = loaded.Enemies.EnemyProjectiles[17];

        // Zero passes `(sample & $0420) == 0`; sixteen/sixteen cancel the sparkle's
        // independent `-16` position jitter. The child must therefore be born exactly at
        // the shard's post-movement coordinate.
        random.Enqueue(0, 16, 16);
        loaded.Enemies.StepEnemyProjectiles(
            assets.LevelData,
            samus: null,
            cameraX: 0,
            cameraY: 0,
            nmiFrameCounter8: 0);

        RoomEnemyProjectileSlot sparkle = loaded.Enemies.EnemyProjectiles[16];
        if (loaded.Enemies.ActiveEnemyProjectileCount != 2 ||
            sparkle.Kind != RoomEnemyProjectileKind.MotherBrainGlassSparkle ||
            sparkle.SlotIndex != 16 || sparkle.PreInstruction != 0x84fb ||
            sparkle.InstructionPointer != 0xcdb7 || sparkle.InstructionTimer != 6 ||
            sparkle.SpritemapPointer != 0x9909 || sparkle.GraphicsIndex != ShardGraphicsIndex ||
            sparkle.XPosition != shard.XPosition || sparkle.YPosition != shard.YPosition ||
            sparkle.XSubposition != 0 || sparkle.YSubposition != 0 ||
            sparkle.XVelocity != 0 || sparkle.YVelocity != 0 ||
            sparkle.XRadius != 0 || sparkle.YRadius != 0 || sparkle.Damage != 0 ||
            sparkle.CanDamageSamus || sparkle.PersistsOnSamusContact ||
            sparkle.BlocksSamusProjectiles || random.CallCount != 6)
        {
            throw new InvalidDataException(
                $"Mother Brain sparkle same-pass initialization diverged: active=" +
                $"{loaded.Enemies.ActiveEnemyProjectileCount}, slot={sparkle.SlotIndex}, " +
                $"kind=$86:{(ushort)sparkle.Kind:X4}, pre/list/timer/map=" +
                $"${sparkle.PreInstruction:X4}/${sparkle.InstructionPointer:X4}/" +
                $"{sparkle.InstructionTimer}/${sparkle.SpritemapPointer:X4}, position=" +
                $"(${sparkle.XPosition:X4},${sparkle.YPosition:X4}) vs shard " +
                $"(${shard.XPosition:X4},${shard.YPosition:X4}), RNG={random.CallCount}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawEnemyProjectiles(oam, cameraX: 0, cameraY: 0);
        oam.FinalizeFrame();
        int objPieces = oam.LastFinalizedSpriteCount;
        if (objPieces < 2)
            throw new InvalidDataException($"Mother Brain shard/sparkle emitted only {objPieces} OBJ pieces.");

        // Property `$3000` makes both visual actors intangible and non-blocking. Exercise the
        // actual beam collision dispatcher so a future property-decoding regression cannot
        // turn the sparkle into an invisible wall despite the header check above.
        var shots = new SamusProjectileSystem();
        SamusProjectileSlot beam = shots.Slots[0];
        beam.Type = 0;
        beam.Damage = 20;
        beam.Direction = (ushort)SamusProjectileDirection.Right;
        beam.XPosition = sparkle.XPosition;
        beam.YPosition = sparkle.YPosition;
        beam.XRadius = 4;
        beam.YRadius = 4;
        beam.InstructionPointer = 0x9000;
        beam.InstructionTimer = 1;
        int beamHits = loaded.Enemies.ResolveEnemyProjectileSamusProjectileHits(
            bus,
            shots,
            new SamusBombProjectileSystem());
        if (beamHits != 0 || !beam.IsActive || beam.InstructionPointer != 0x9000)
        {
            throw new InvalidDataException(
                $"Mother Brain glass visuals blocked a beam: hits={beamHits}, " +
                $"active={beam.IsActive}, list=${beam.InstructionPointer:X4}.");
        }

        var sparkleMaps = new HashSet<ushort> { sparkle.SpritemapPointer };
        int additionalFrames = 0;
        while (sparkle.IsActive && additionalFrames < 64)
        {
            additionalFrames++;
            loaded.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                cameraX: 0,
                cameraY: 0,
                nmiFrameCounter8: unchecked((byte)additionalFrames));
            if (sparkle.IsActive)
                sparkleMaps.Add(sparkle.SpritemapPointer);
        }
        ushort[] expectedSparkleMaps = [0x9909, 0x9910, 0x9917, 0x991e];
        if (sparkle.IsActive || additionalFrames != 28 ||
            !sparkleMaps.SetEquals(expectedSparkleMaps) || !shard.IsActive)
        {
            throw new InvalidDataException(
                $"Mother Brain sparkle finite lifecycle diverged: active={sparkle.IsActive}, " +
                $"frames={additionalFrames}/28, shard active={shard.IsActive}, maps=" +
                $"[{string.Join(',', sparkleMaps.Select(map => map.ToString("X4")))}].");
        }

        return new SparkleResult(unchecked((ushort)(sparkle.SlotIndex * 2)), objPieces);
    }

    private static HashSet<ushort> ReadLoopingShardMaps(ISnesAddressSpace bus, ushort list)
    {
        var maps = new HashSet<ushort>();
        ushort cursor = list;
        for (int operation = 0; operation < 16; operation++)
        {
            ushort word = ReadWord(bus, 0x860000 | cursor);
            if (word == 0x81ab)
            {
                ushort target = ReadWord(bus, 0x860000 | unchecked((ushort)(cursor + 2)));
                if (target != list || maps.Count != 8)
                {
                    throw new InvalidDataException(
                        $"Mother Brain shard list $86:{list:X4} looped to $86:{target:X4} " +
                        $"after {maps.Count} maps.");
                }
                return maps;
            }
            if ((word & 0x8000) != 0 || word == 0)
            {
                throw new InvalidDataException(
                    $"Mother Brain shard list $86:{list:X4} reached unexpected " +
                    $"instruction ${word:X4} at $86:{cursor:X4}.");
            }
            maps.Add(ReadWord(bus, 0x860000 | unchecked((ushort)(cursor + 2))));
            cursor = unchecked((ushort)(cursor + 4));
        }
        throw new InvalidDataException($"Mother Brain shard list $86:{list:X4} did not loop.");
    }

    private static LoadedGlass LoadEmptyPool(
        SuperMetroidAddressSpace retailBus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ScriptedRandom random)
    {
        // Retaining zero records replaces only the first population record with the native
        // `$FFFF,quota` terminator. Projectile definitions, instruction lists, sine table,
        // level data, and every draw map remain byte-for-byte reads from the user's ROM.
        var bus = new PopulationPrefixAddressSpace(
            retailBus,
            room.State.EnemyPopulationPointer,
            retainedRecordCount: 0,
            deathQuota: 0);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            new SnesVram(),
            new SnesCgram(),
            random.Next,
            readRandomNumber: random.Read,
            level: assets.LevelData,
            samus: samus);
        if (enemies.EnemyCount != 0 || enemies.ActiveEnemyProjectileCount != 0)
            throw new InvalidDataException("Empty Mother Brain projectile fixture was not empty.");
        return new LoadedGlass(enemies);
    }

    private static uint AddEightBitVelocityReference(uint packedPosition, ushort velocity) =>
        unchecked(packedPosition + (uint)(unchecked((short)velocity) << 8));

    private static uint PackFixed(ushort position, ushort subposition) =>
        ((uint)position << 16) | subposition;

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private sealed class ScriptedRandom
    {
        private readonly Queue<ushort> _values = new();
        private readonly ushort _defaultValue;

        public ScriptedRandom(ushort defaultValue) => _defaultValue = defaultValue;

        public int CallCount { get; private set; }

        public void Enqueue(params ushort[] values)
        {
            foreach (ushort value in values)
                _values.Enqueue(value);
        }

        public ushort Next()
        {
            CallCount++;
            return _values.Count == 0 ? _defaultValue : _values.Dequeue();
        }

        public ushort Read() => _values.Count == 0 ? _defaultValue : _values.Peek();
    }

    private readonly record struct LoadedGlass(RoomEnemySystem Enemies);
    private readonly record struct OrientationResult(int UniqueLists, int UniqueMaps);
    private readonly record struct SparkleResult(ushort NativeSlot, int ObjPieces);
}
