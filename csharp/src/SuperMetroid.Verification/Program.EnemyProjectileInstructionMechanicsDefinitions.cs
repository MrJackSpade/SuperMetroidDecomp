using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Compares every compiled Mother Brain/misc-dust mechanics word to the pinned ROM,
    /// then runs all 35 production programs while rejecting reads from those source bytes.
    /// </summary>
    private static void VerifyEnemyProjectileInstructionMechanicsDefinitions()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  Enemy-projectile instruction mechanics: cartridge comparison skipped " +
                "(private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        for (int index = 0;
             index < EnemyProjectileInstructionMechanicsDefinitions.NativeWordCount;
             index++)
        {
            EnemyProjectileMechanicsWordDefinition definition =
                EnemyProjectileInstructionMechanicsDefinitions.NativeWord(index);
            ushort native = ReadEnemyProjectileMechanicsWord(
                rom,
                0x860000 | definition.Address);
            AssertEqual(definition.Value, native,
                $"bank-$86 projectile mechanics word ${definition.Address:X4}");

        }

        var guarded = new EnemyProjectileMechanicsReadGuard(rom);
        var motherBrain = new MotherBrainRainbowBeamAttackSequence
        {
            BrainXPosition = 0x0080,
            BrainYPosition = 0x0060,
        };
        var samus = new SamusState { XPosition = 0x4000, YPosition = 0x4000 };

        VerifyCompiledBlueRingProgram(guarded, motherBrain, samus);
        VerifyCompiledBombProgram(guarded, motherBrain, samus);
        VerifyCompiledPurpleBreathProgram(guarded, motherBrain, samus);
        VerifyCompiledEscapeDoorProgram(guarded, motherBrain, samus);
        VerifyCompiledSubtitleProgram(guarded, motherBrain, samus);
        VerifyCompiledMiscDustPrograms(guarded, motherBrain, samus);
        VerifyCompiledRoomSharedPrograms(guarded, samus);

        var invalid = new MotherBrainEnemyProjectileSystem();
        int invalidSlotIndex = invalid.SpawnTimeBombSetSubtitle() ??
            throw new InvalidDataException("Fresh subtitle pool rejected its first slot.");
        MotherBrainEnemyProjectileSlot invalidSlot = invalid.Slots[invalidSlotIndex];
        invalidSlot.InstructionPointer = 0xcb13;
        invalidSlot.InstructionTimer = 1;
        AssertThrows<InvalidDataException>(
            () => invalid.StepFrame(guarded, motherBrain, baby: null, samus, layer1X: 0),
            "restored projectile pointer after the translated subtitle fails loudly");

        _ = EnemyProjectileInstructionMechanicsDefinitions.ReadMechanicsWord(
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBombInitial);
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += EnemyProjectileInstructionMechanicsDefinitions.ReadMechanicsWord(
                EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBombInitial);
        }
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "projectile mechanics allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed projectile mechanics lookups allocate no per-frame storage");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "all production programs avoid compiled mechanics source bytes");

        Console.WriteLine(
            $"  Enemy-projectile instruction mechanics: " +
            $"{EnemyProjectileInstructionMechanicsDefinitions.NativeWordCount} words and " +
            "all 35 Mother Brain/misc-dust programs pass with mechanics reads forbidden.");
    }

    /// <summary>
    /// Exercises the same thirty shared programs through the ordinary room-projectile
    /// interpreter. Mother Brain owns a separate slot pool and interpreter, so its complete
    /// catalog audit alone cannot prove that Eye Door smoke and room-graphics dust use the
    /// compiled mechanics resolver.
    /// </summary>
    private static void VerifyCompiledRoomSharedPrograms(
        ISnesAddressSpace bus,
        SamusState samus)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo processMethod = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions",
            instanceFlags)!;
        MethodInfo spawnDustMethod = typeof(RoomEnemySystem).GetMethod(
            "SpawnRoomGraphicsDustExplosion",
            instanceFlags)!;
        var initialize = typeof(RoomEnemySystem).GetMethod(
                "InitializeEnemyProjectileFromDefinition",
                BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Action<RoomEnemyProjectileSlot, RoomEnemyProjectileKind, ushort>>();

        for (ushort animation = 0;
             animation < EnemyProjectileInstructionMechanicsDefinitions.MiscDustProgramCount;
             animation++)
        {
            RoomEnemySystem enemies = CreateRoomEnemySystem();
            spawnDustMethod.Invoke(enemies, [(ushort)128, (ushort)96, animation]);
            RoomEnemyProjectileSlot dust = enemies.EnemyProjectiles.Single(
                projectile => projectile.Kind == RoomEnemyProjectileKind.MiscDustExplosion);

            if (animation == 28)
            {
                RunForcedTicks(enemies, dust, 4);
                AssertTrue(dust.IsActive,
                    "room-system looping misc-dust animation remains active");
                AssertTrue(dust.InstructionPointer is 0xe200 or 0xe204,
                    "room-system looping misc-dust pointer remains in `$E1FC` program");
                continue;
            }

            int calls = 0;
            while (dust.IsActive && calls < 40)
            {
                RunForcedTicks(enemies, dust, 1);
                calls++;
            }
            AssertTrue(!dust.IsActive,
                $"room-system finite misc-dust animation {animation} reaches delete");
        }

        RoomEnemySystem smokeEnemies = CreateRoomEnemySystem();
        smokeEnemies.SpawnEyeDoorProjectile(
            new EyeDoorProjectileRequest(
                EyeDoorEnemyProjectileRomData.SmokeDefinition,
                Parameter: 3,
                PlmBlockIndex: 4 * 16 + 7,
                DoorBit: 0),
            roomWidthInBlocks: 16,
            new Bank80SystemState());
        RoomEnemyProjectileSlot smoke = smokeEnemies.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.EyeDoorSmoke);
        RunForcedTicks(smokeEnemies, smoke, 7);
        AssertTrue(!smoke.IsActive,
            "Eye Door smoke reaches delete through the shared compiled mechanics owner");

        RoomEnemySystem bombEnemies = CreateRoomEnemySystem();
        RoomEnemyProjectileSlot bomb = bombEnemies.EnemyProjectiles[^1];
        initialize(bomb, RoomEnemyProjectileKind.MotherBrainBomb, 0x0400);
        RunForcedTicks(bombEnemies, bomb, 10);
        AssertTrue(bomb.IsActive,
            "room-system Mother Brain bomb survives its compiled animation loop");
        AssertTrue(bomb.InstructionPointer is >= 0xc772 and <= 0xc792,
            "room-system Mother Brain bomb remains inside its compiled loop");

        RoomEnemySystem breathEnemies = CreateRoomEnemySystem();
        RoomEnemyProjectileSlot breath = breathEnemies.EnemyProjectiles[^1];
        initialize(breath, RoomEnemyProjectileKind.MotherBrainPurpleBreathBig, 0);
        RunForcedTicks(breathEnemies, breath, 9);
        AssertTrue(!breath.IsActive,
            "room-system Mother Brain purple breath reaches compiled deletion");

        RoomEnemySystem fragmentEnemies = CreateRoomEnemySystem();
        RoomEnemyProjectileSlot fragment = fragmentEnemies.EnemyProjectiles[^1];
        initialize(fragment, RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment, 0);
        RunForcedTicks(fragmentEnemies, fragment, 9);
        AssertTrue(fragment.IsActive,
            "room-system Mother Brain escape fragment survives its compiled loop");
        AssertTrue(fragment.InstructionPointer is >= 0xca26 and <= 0xca42,
            "room-system Mother Brain escape fragment remains inside its compiled loop");

        RoomEnemySystem subtitleEnemies = CreateRoomEnemySystem();
        RoomEnemyProjectileSlot subtitle = subtitleEnemies.EnemyProjectiles[^1];
        initialize(subtitle, RoomEnemyProjectileKind.MotherBrainEscapeSubtitle, 0);
        RunForcedTicks(subtitleEnemies, subtitle, 2);
        AssertTrue(subtitle.IsActive,
            "room-system Mother Brain escape subtitle survives compiled sleep");
        AssertEqual(0xcb11, subtitle.InstructionPointer,
            "room-system Mother Brain escape subtitle retains its sleep opcode");

        RoomEnemySystem CreateRoomEnemySystem()
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, bus);
            typeof(RoomEnemySystem).GetField("_readRandomNumber", instanceFlags)!
                .SetValue(enemies, (Func<ushort>)(() => 0));
            typeof(RoomEnemySystem).GetField("_nextRandom", instanceFlags)!
                .SetValue(enemies, (Func<ushort>)(() => 0));
            return enemies;
        }

        void RunForcedTicks(
            RoomEnemySystem enemies,
            RoomEnemyProjectileSlot projectile,
            int count)
        {
            for (int tick = 0; tick < count; tick++)
            {
                projectile.InstructionTimer = 1;
                processMethod.Invoke(enemies, [projectile, samus, (ushort)0, (ushort)0]);
            }
        }
    }

    private static void VerifyCompiledBlueRingProgram(
        ISnesAddressSpace bus,
        MotherBrainRainbowBeamAttackSequence motherBrain,
        SamusState samus)
    {
        var projectiles = new MotherBrainEnemyProjectileSystem();
        int slotIndex = projectiles.Spawn(
                bus,
                motherBrain,
                new MotherBrainOnionRingSpawnRequest(SnesAngle.Zero)) ??
            throw new InvalidDataException("Fresh blue-ring pool rejected its first slot.");
        MotherBrainEnemyProjectileSlot slot = projectiles.Slots[slotIndex];

        // Keep the ring in the native head-follow delay so room collision cannot truncate
        // the complete animation program before its final sleep instruction is reached.
        slot.DelayTimer = ushort.MaxValue;
        for (int frame = 0; frame < 60; frame++)
            projectiles.StepFrame(bus, motherBrain, baby: null, samus, layer1X: 0);

        AssertEqual(6, slot.XRadius, "blue-ring program reaches final X radius");
        AssertEqual(6, slot.YRadius, "blue-ring program reaches final Y radius");
        AssertEqual(0xc462, slot.InstructionPointer,
            "blue-ring program sleeps at its authored terminal pointer");
    }

    private static void VerifyCompiledBombProgram(
        ISnesAddressSpace bus,
        MotherBrainRainbowBeamAttackSequence motherBrain,
        SamusState samus)
    {
        var projectiles = new MotherBrainEnemyProjectileSystem();
        int slotIndex = projectiles.SpawnBomb(motherBrain, new(0)) ??
            throw new InvalidDataException("Fresh bomb pool rejected its first slot.");
        MotherBrainEnemyProjectileSlot slot = projectiles.Slots[slotIndex];
        for (int frame = 0; frame < 40; frame++)
            projectiles.StepFrame(bus, motherBrain, baby: null, samus, layer1X: 0);

        AssertTrue(slot.IsActive, "bomb survives one complete compiled animation loop");
        AssertTrue(slot.InstructionPointer is >= 0xc772 and <= 0xc792,
            "bomb loop remains inside its translated pointer domain");
    }

    private static void VerifyCompiledPurpleBreathProgram(
        ISnesAddressSpace bus,
        MotherBrainRainbowBeamAttackSequence motherBrain,
        SamusState samus)
    {
        var projectiles = new MotherBrainEnemyProjectileSystem();
        int slotIndex = projectiles.SpawnPurpleBreathBig(motherBrain) ??
            throw new InvalidDataException("Fresh purple-breath pool rejected its first slot.");
        MotherBrainEnemyProjectileSlot slot = projectiles.Slots[slotIndex];
        for (int frame = 0; frame < 77; frame++)
            projectiles.StepFrame(bus, motherBrain, baby: null, samus, layer1X: 0);
        AssertTrue(!slot.IsActive, "purple breath deletes after its full compiled lifetime");
    }

    private static void VerifyCompiledEscapeDoorProgram(
        ISnesAddressSpace bus,
        MotherBrainRainbowBeamAttackSequence motherBrain,
        SamusState samus)
    {
        var projectiles = new MotherBrainEnemyProjectileSystem();
        int slotIndex = projectiles.SpawnEscapeDoorParticle(new(0)) ??
            throw new InvalidDataException("Fresh escape-fragment pool rejected its first slot.");
        MotherBrainEnemyProjectileSlot slot = projectiles.Slots[slotIndex];
        for (int frame = 0; frame < 20; frame++)
            projectiles.StepFrame(bus, motherBrain, baby: null, samus, layer1X: 0);

        AssertTrue(slot.IsActive, "escape fragment survives one compiled animation loop");
        AssertTrue(slot.InstructionPointer is >= 0xca26 and <= 0xca42,
            "escape fragment loop remains inside its translated pointer domain");
    }

    private static void VerifyCompiledSubtitleProgram(
        ISnesAddressSpace bus,
        MotherBrainRainbowBeamAttackSequence motherBrain,
        SamusState samus)
    {
        var projectiles = new MotherBrainEnemyProjectileSystem();
        int slotIndex = projectiles.SpawnTimeBombSetSubtitle() ??
            throw new InvalidDataException("Fresh subtitle pool rejected its first slot.");
        MotherBrainEnemyProjectileSlot slot = projectiles.Slots[slotIndex];
        projectiles.StepFrame(bus, motherBrain, baby: null, samus, layer1X: 0);
        projectiles.StepFrame(bus, motherBrain, baby: null, samus, layer1X: 0);
        AssertTrue(slot.IsActive, "subtitle sleep retains the projectile");
        AssertEqual(0xcb11, slot.InstructionPointer,
            "subtitle sleeps at its authored terminal pointer");
    }

    private static void VerifyCompiledMiscDustPrograms(
        ISnesAddressSpace bus,
        MotherBrainRainbowBeamAttackSequence motherBrain,
        SamusState samus)
    {
        for (ushort animation = 0;
             animation < EnemyProjectileInstructionMechanicsDefinitions.MiscDustProgramCount;
             animation++)
        {
            var projectiles = new MotherBrainEnemyProjectileSystem();
            int slotIndex = projectiles.SpawnMiscDust(bus, 0x0080, 0x0080, animation) ??
                throw new InvalidDataException(
                    $"Fresh misc-dust pool rejected animation {animation}.");
            MotherBrainEnemyProjectileSlot slot = projectiles.Slots[slotIndex];

            if (animation == 28)
            {
                // `$E1FC` is the one persistent two-frame misc program. It exposed the
                // missing native goto support in the formerly finite-only shared handler.
                for (int frame = 0; frame < 12; frame++)
                    projectiles.StepFrame(bus, motherBrain, baby: null, samus, 0, 0);
                AssertTrue(slot.IsActive, "looping misc-dust animation remains active");
                AssertTrue(slot.InstructionPointer is 0xe200 or 0xe204,
                    "looping misc-dust pointer remains in `$E1FC` program");
                continue;
            }

            int calls = 0;
            while (slot.IsActive && calls < 200)
            {
                projectiles.StepFrame(bus, motherBrain, baby: null, samus, 0, 0);
                calls++;
            }

            AssertTrue(!slot.IsActive,
                $"finite misc-dust animation {animation} reaches delete");
        }
    }

    private static ushort ReadEnemyProjectileMechanicsWord(
        SuperMetroidAddressSpace source,
        int address) =>
        unchecked((ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8));

    private sealed class EnemyProjectileMechanicsReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (EnemyProjectileInstructionMechanicsDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled enemy-projectile mechanics byte ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
