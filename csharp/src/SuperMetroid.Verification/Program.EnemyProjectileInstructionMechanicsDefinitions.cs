using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>
    /// Compares every compiled Mother Brain/misc-dust mechanics word to the pinned ROM,
    /// then runs all 39 production programs while rejecting reads from those source bytes.
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

        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(romPath);
        for (int index = 0;
             index < EnemyProjectileInstructionMechanicsDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                EnemyProjectileInstructionMechanicsDefinitions.MechanicsWord(index);
            ushort native = ReadEnemyProjectileMechanicsWord(
                rom,
                0x860000 | definition.Address);
            AssertEqual(definition.Value, native,
                $"bank-$86 projectile mechanics word ${definition.Address:X4}");

        }

        for (int index = 0;
             index < MotherBrainHandBeamInstructionProgramDefinitions.NativeWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                MotherBrainHandBeamInstructionProgramDefinitions.NativeWord(index);
            ushort native = ReadEnemyProjectileMechanicsWord(
                rom,
                EnemyProjectileCodePointers.BankBase | definition.Address);
            AssertEqual(definition.Value, native,
                $"Mother Brain hand-beam mechanics word ${definition.Address:X4}");
        }
        for (int index = 0;
             index < MotherBrainHandBeamInstructionProgramDefinitions.ExternalCallCount;
             index++)
        {
            ushort instruction =
                MotherBrainHandBeamInstructionProgramDefinitions.ExternalCallInstruction(index);
            int operandAddress = EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(instruction + 2));
            int native = rom.ReadByte(operandAddress) |
                rom.ReadByte(operandAddress + 1) << 8 |
                rom.ReadByte(operandAddress + 2) << 16;
            AssertEqual(MotherBrainHandBeamInstructionProgramDefinitions.SpawnNextCallback,
                native,
                $"Mother Brain hand-beam callback at ${instruction:X4}");
        }

        var guarded = new EnemyProjectileMechanicsReadGuard(rom);
        var samus = new SamusState { XPosition = 0x4000, YPosition = 0x4000 };

        Suite(nameof(VerifyCompiledRoomSharedPrograms), () => VerifyCompiledRoomSharedPrograms(guarded, samus, rom));

        AssertThrows<InvalidDataException>(
            () => MotherBrainHandBeamInstructionProgramDefinitions.ReadExternalFunction(
                unchecked((ushort)(
                    MotherBrainHandBeamInstructionProgramDefinitions.ExternalCallInstruction(0) +
                    1))),
            "non-opcode hand-beam callback address fails loudly");

        _ = EnemyProjectileInstructionMechanicsDefinitions.ReadMechanicsWord(
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBombInitial);
        _ = MotherBrainHandBeamInstructionProgramDefinitions.ReadMechanicsWord(
            MotherBrainHandBeamInstructionProgramDefinitions.Initial);
        _ = MotherBrainHandBeamInstructionProgramDefinitions.ReadExternalFunction(
            MotherBrainHandBeamInstructionProgramDefinitions.ExternalCallInstruction(0));
        // Tiered JIT can promote these warmed methods during a long, shared suite.
        // A one-time promotion is not a per-frame allocation. Repeat a bounded
        // measurement until the steady-state pass is reached; persistent allocation
        // still fails after all four passes.
        // The callback operand address is a fixture constant; only the production lookups are measured.
        ushort externalCall = MotherBrainHandBeamInstructionProgramDefinitions.ExternalCallInstruction(0);
        long allocated = long.MaxValue;
        int checksum = 0;
        for (int pass = 0; pass < 4 && allocated != 0; pass++)
        {
            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 65536; index++)
            {
                checksum += EnemyProjectileInstructionMechanicsDefinitions.ReadMechanicsWord(
                    EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBombInitial);
                checksum += MotherBrainHandBeamInstructionProgramDefinitions.ReadMechanicsWord(
                    MotherBrainHandBeamInstructionProgramDefinitions.Initial);
                checksum += MotherBrainHandBeamInstructionProgramDefinitions.ReadExternalFunction(externalCall);
            }
            allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        }
        AssertTrue(checksum != 0, "projectile mechanics allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed projectile mechanics lookups allocate no per-frame storage");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "all production programs avoid compiled mechanics source bytes");
        AssertEqual(0, guarded.PresentationReadBytes,
            "all projectile programs select installed frames without live presentation reads");

        Console.WriteLine(
            $"  Enemy-projectile instruction mechanics: " +
            $"{EnemyProjectileInstructionMechanicsDefinitions.MechanicsWordCount} words and " +
            "all 39 Mother Brain/misc-dust programs plus the 25-word recursive hand-beam " +
            "program pass with mechanics and callback reads forbidden.");
    }

    /// <summary>
    /// Exercises the same thirty shared programs through the ordinary room-projectile
    /// interpreter. Mother Brain owns a separate slot pool and interpreter, so its complete
    /// catalog audit alone cannot prove that Eye Door smoke and room-graphics dust use the
    /// compiled mechanics resolver.
    /// </summary>
    private static void VerifyCompiledRoomSharedPrograms(
        ISnesAddressSpace bus,
        SamusState samus,
        SuperMetroidAddressSpace nativeReference)
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

        RoomEnemySystem rainbowEnemies = CreateRoomEnemySystem();
        var motherBrainBody = new RoomEnemySlot(0);
        var motherBrainHead = new RoomEnemySlot(1)
        {
            XPosition = 0x0137,
            YPosition = 0x008b,
        };
        var motherBrainState = new MotherBrainEnemyState(motherBrainBody)
        {
            Head = motherBrainHead,
        };
        typeof(RoomEnemySystem).GetField("_motherBrain", instanceFlags)!
            .SetValue(rainbowEnemies, motherBrainState);
        MethodInfo spawnRainbowMethod = typeof(RoomEnemySystem).GetMethod(
            "SpawnMotherBrainRainbowChargingProjectile",
            instanceFlags)!;
        spawnRainbowMethod.Invoke(rainbowEnemies, [motherBrainState]);
        RoomEnemyProjectileSlot rainbow = rainbowEnemies.EnemyProjectiles.Single(
            projectile => projectile.Kind ==
                RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging);
        AssertEqual(0x0137, rainbow.XPosition,
            "real rainbow-charge producer pins its initial X to Mother Brain's head");
        AssertEqual(0x008b, rainbow.YPosition,
            "real rainbow-charge producer pins its initial Y to Mother Brain's head");
        for (int frame = 0; frame < 30; frame++)
            Process(rainbowEnemies, rainbow, samus);
        AssertTrue(rainbow.IsActive,
            "rainbow charge survives all six exact five-frame animation stages");
        Process(rainbowEnemies, rainbow, samus);
        AssertTrue(!rainbow.IsActive,
            "rainbow charge deletes on the frame after its exact 30-frame lifetime");

        RoomEnemySystem explosionEnemies = CreateRoomEnemySystem();
        MethodInfo spawnRainbowExplosionMethod = typeof(RoomEnemySystem).GetMethod(
            "SpawnMotherBrainRainbowExplosion",
            instanceFlags)!;
        spawnRainbowExplosionMethod.Invoke(
            explosionEnemies,
            [
                (ushort)0x0100,
                (ushort)0x0080,
                new MotherBrainRainbowExplosionRequest(
                    XOffset: -9,
                    YOffset: 7),
            ]);
        RoomEnemyProjectileSlot explosion = explosionEnemies.EnemyProjectiles.Single(
            projectile => projectile.Kind ==
                RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion);
        AssertEqual(0x00f7, explosion.XPosition,
            "real rainbow-impact producer applies its signed X offset");
        AssertEqual(0x0087, explosion.YPosition,
            "real rainbow-impact producer applies its signed Y offset");
        for (int frame = 0; frame < 18; frame++)
            Process(explosionEnemies, explosion, samus);
        AssertTrue(explosion.IsActive,
            "rainbow impact survives its exact five-stage 18-frame lifetime");
        Process(explosionEnemies, explosion, samus);
        AssertTrue(!explosion.IsActive,
            "rainbow impact deletes on the frame after its exact lifetime");

        MethodInfo spawnDeathExplosionMethod = typeof(RoomEnemySystem).GetMethod(
            "SpawnMotherBrainDeathExplosion",
            instanceFlags)!;
        int[] deathExplosionLifetimes = [31, 32, 30];
        for (ushort parameter = 0; parameter < deathExplosionLifetimes.Length; parameter++)
        {
            RoomEnemySystem deathExplosionEnemies = CreateRoomEnemySystem();
            var deathBody = new RoomEnemySlot(0)
            {
                XPosition = 0x0080,
                YPosition = 0x0070,
            };
            typeof(RoomEnemySystem).GetField("_motherBrain", instanceFlags)!
                .SetValue(deathExplosionEnemies, new MotherBrainEnemyState(deathBody));
            spawnDeathExplosionMethod.Invoke(
                deathExplosionEnemies,
                [
                    new MotherBrainDeathExplosionRequest(
                        XOffset: -5,
                        YOffset: 9,
                        ProjectileParameter: parameter,
                        SoundEffect: 0),
                ]);
            RoomEnemyProjectileSlot deathExplosion =
                deathExplosionEnemies.EnemyProjectiles.Single(
                    projectile => projectile.Kind ==
                        RoomEnemyProjectileKind.MotherBrainDeathExplosion);
            for (int frame = 0; frame < deathExplosionLifetimes[parameter]; frame++)
            {
                Process(deathExplosionEnemies, deathExplosion, samus);
            }
            AssertTrue(deathExplosion.IsActive,
                $"Mother Brain death explosion {parameter} survives its exact lifetime");
            Process(deathExplosionEnemies, deathExplosion, samus);
            AssertTrue(!deathExplosion.IsActive,
                $"Mother Brain death explosion {parameter} deletes on its following frame");
        }

        RoomEnemySystem handBeamEnemies = CreateRoomEnemySystem();
        var handBeamBody = new RoomEnemySlot(0)
        {
            XPosition = 0x0070,
            YPosition = 0x0090,
        };
        var handBeamState = new MotherBrainEnemyState(handBeamBody);
        typeof(RoomEnemySystem).GetField("_motherBrain", instanceFlags)!
            .SetValue(handBeamEnemies, handBeamState);
        var handBeamTarget = new SamusState
        {
            XPosition = 0x00c0,
            YPosition = 0x0060,
        };
        MethodInfo spawnHandBeamMethod = typeof(RoomEnemySystem).GetMethod(
            "SpawnMotherBrainHandBeamCharging",
            instanceFlags)!;
        spawnHandBeamMethod.Invoke(handBeamEnemies, [handBeamState, handBeamTarget]);
        RoomEnemyProjectileSlot handBeam = handBeamEnemies.EnemyProjectiles.Single(
            projectile => projectile.Kind ==
                RoomEnemyProjectileKind.MotherBrainHandBeamCharging);
        ushort referenceCursor = MotherBrainHandBeamInstructionProgramDefinitions.Initial;
        ushort referenceOperand = 0;
        int referenceTimer = 1;
        var observedHandBeamOperands = new HashSet<ushort>();
        for (int frame = 0; frame < 39; frame++)
        {
            if (--referenceTimer == 0)
            {
                ushort word = ReadEnemyProjectileMechanicsWord(nativeReference, 0x860000 | referenceCursor);
                if (word >= 0x8000)
                {
                    AssertEqual(EnemyProjectileCodePointers.Instruction_EnemyProjectile_CallExternalFunctionInY,
                        word, "hand-beam native stage transition calls the child-spawn callback");
                    referenceCursor += 5;
                    word = ReadEnemyProjectileMechanicsWord(nativeReference, 0x860000 | referenceCursor);
                }
                referenceTimer = word;
                referenceOperand = (ushort)(referenceCursor + 2);
                referenceCursor += 4;
            }
            processMethod.Invoke(
                handBeamEnemies,
                [handBeam, handBeamTarget, (ushort)0, (ushort)0]);
            AssertEqual(referenceOperand, handBeam.PresentationOperandAddress,
                $"hand-beam frame {frame} selects the native visual operand");
            AssertEqual((ushort)referenceTimer, handBeam.InstructionTimer,
                $"hand-beam frame {frame} preserves the native remaining duration");
            observedHandBeamOperands.Add(handBeam.PresentationOperandAddress);
        }
        AssertEqual(MotherBrainHandBeamInstructionProgramDefinitions.PresentationWordCount,
            observedHandBeamOperands.Count, "hand-beam selects all 21 installed visual operands");
        AssertTrue(handBeam.IsActive,
            "hand-beam charge survives all three exact 13-frame stages");
        AssertEqual(3, handBeamEnemies.EnemyProjectiles.Count(
                projectile => projectile.Kind ==
                    RoomEnemyProjectileKind.MotherBrainHandBeamFired),
            "three compiled external callbacks spawn three fired hand-beam children");
        Process(handBeamEnemies, handBeam, handBeamTarget);
        AssertTrue(!handBeam.IsActive,
            "hand-beam charge deletes on the frame after its exact 39-frame lifetime");

        RoomEnemySystem ringEnemies = CreateRoomEnemySystem();
        var ringBody = new RoomEnemySlot(0);
        var ringHead = new RoomEnemySlot(1)
        {
            XPosition = 0x0080,
            YPosition = 0x0060,
        };
        var ringState = new MotherBrainEnemyState(ringBody)
        {
            Head = ringHead,
        };
        typeof(RoomEnemySystem).GetField("_motherBrain", instanceFlags)!
            .SetValue(ringEnemies, ringState);
        MethodInfo spawnRingMethod = typeof(RoomEnemySystem).GetMethod(
            "SpawnMotherBrainOnionRing",
            instanceFlags)!;
        spawnRingMethod.Invoke(ringEnemies, [ringState, (byte)0]);
        RoomEnemyProjectileSlot ring = ringEnemies.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainOnionRing);
        for (int frame = 0; frame < 52; frame++)
            Process(ringEnemies, ring, samus);
        AssertEqual(6, ring.XRadius,
            "real room onion ring reaches its compiled final X radius");
        AssertEqual(6, ring.YRadius,
            "real room onion ring reaches its compiled final Y radius");
        Process(ringEnemies, ring, samus);
        AssertEqual(0xc462, ring.InstructionPointer,
            "real room onion ring sleeps at its authored initial-program terminal");

        ring.InstructionPointer =
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBlueRingTouch;
        ring.InstructionTimer = 1;
        ring.PreInstruction =
            EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_MotherBrainsOnionRings;
        ring.GraphicsIndex = 0x0400;
        for (int frame = 0; frame < 30; frame++)
            Process(ringEnemies, ring, samus);
        AssertTrue(ring.IsActive,
            "onion-ring impact survives all six exact five-frame stages");
        AssertEqual(0, ring.GraphicsIndex,
            "onion-ring impact's duplicate native opcode selects palette zero");
        AssertEqual(EnemyProjectileCodePointers.RTS_868170, ring.PreInstruction,
            "onion-ring impact clears its movement pre-instruction");
        Process(ringEnemies, ring, samus);
        AssertTrue(!ring.IsActive,
            "onion-ring impact deletes on the frame after its exact 30-frame lifetime");

        Suite(nameof(VerifyDroolVariant), () => VerifyDroolVariant(RoomEnemyProjectileKind.MotherBrainDrool, 0x007f));
        Suite(nameof(VerifyDroolVariant), () => VerifyDroolVariant(RoomEnemyProjectileKind.MotherBrainDyingDrool, 0x0080));

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

        void Process(RoomEnemySystem enemies, RoomEnemyProjectileSlot projectile, SamusState target)
        {
            bool selectsFrame = projectile.InstructionTimer == 1;
            processMethod.Invoke(enemies, [projectile, target, (ushort)0, (ushort)0]);
            if (!projectile.IsActive || !selectsFrame || projectile.InstructionTimer == 0 || projectile.PresentationOperandAddress == 0)
                return;
            ushort operand = projectile.PresentationOperandAddress;
            AssertEqual(unchecked((ushort)(projectile.InstructionPointer - 2)), operand,
                "real projectile interpreter selects its exact native operand address");
            AssertTrue(EnemyProjectilePresentationFrameDefinitions.Contains(operand),
                "real projectile interpreter selects an installed-art identity");
            AssertEqual(EnemyProjectileSpritemapDefinitions.BlankSpritemap, projectile.SpritemapPointer,
                "real projectile interpreter leaves spritemap resolution to installed content");
            if (bus is EnemyProjectileMechanicsReadGuard guard)
                guard.VerifySelectedDuration(operand, projectile.InstructionTimer);
        }
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
                Process(enemies, projectile, samus);
            }
        }

        void VerifyDroolVariant(RoomEnemyProjectileKind expectedKind, ushort neckAngleDelta)
        {
            RoomEnemySystem droolEnemies = CreateRoomEnemySystem();
            var body = new RoomEnemySlot(0);
            var head = new RoomEnemySlot(1)
            {
                XPosition = 0x0100,
                YPosition = 0x0080,
            };
            var state = new MotherBrainEnemyState(body)
            {
                Head = head,
                DroolGenerationEnabled = true,
                NeckAngleDelta = neckAngleDelta,
            };
            typeof(RoomEnemySystem).GetField("_motherBrain", instanceFlags)!
                .SetValue(droolEnemies, state);
            MethodInfo spawnDroolMethod = typeof(RoomEnemySystem).GetMethod(
                "SpawnMotherBrainDrool",
                instanceFlags)!;
            spawnDroolMethod.Invoke(droolEnemies, [state]);
            RoomEnemyProjectileSlot drool = droolEnemies.EnemyProjectiles.Single(
                projectile => projectile.Kind == expectedKind);
            ushort attachedY = drool.YPosition;

            for (int frame = 0; frame < 50; frame++)
                Process(droolEnemies, drool, samus);
            AssertEqual(
                EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_MotherBrainsDrool,
                drool.PreInstruction,
                $"{expectedKind} remains attached for five exact ten-frame stages");
            Process(droolEnemies, drool, samus);
            AssertEqual(
                EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_MotherBrainsDrool_Falling,
                drool.PreInstruction,
                $"{expectedKind} switches to falling after its fifth attached stage");
            AssertEqual(unchecked((ushort)(attachedY + 12)), drool.YPosition,
                $"{expectedKind} release command moves the actor down twelve pixels");
            AssertEqual(0xc8ce, drool.InstructionPointer,
                $"{expectedKind} release frame reaches its terminal sleep");

            drool.YPosition = 0x00d7;
            typeof(RoomEnemySystem).GetMethod(
                "RunMotherBrainFallingDroolPreInstruction",
                BindingFlags.Static | BindingFlags.NonPublic)!
                .Invoke(null, [drool]);
            AssertEqual(EnemyProjectileInstructionMechanicsDefinitions.MotherBrainDroolFalling,
                drool.InstructionPointer,
                $"{expectedKind} floor impact enters the compiled splash program");
            for (int frame = 0; frame < 40; frame++)
                Process(droolEnemies, drool, samus);
            AssertTrue(drool.IsActive,
                $"{expectedKind} splash survives all four exact ten-frame stages");
            Process(droolEnemies, drool, samus);
            AssertTrue(!drool.IsActive,
                $"{expectedKind} splash deletes on the frame after its 40-frame lifetime");
        }
    }

    private static ushort ReadEnemyProjectileMechanicsWord(
        SuperMetroidAddressSpace source,
        int address) =>
        unchecked((ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8));

    private sealed class EnemyProjectileMechanicsReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        private readonly HashSet<int> _presentationReadBytes = [];

        public int ForbiddenReadAttempts { get; private set; }
        public int PresentationReadBytes => _presentationReadBytes.Count;

        public byte ReadByte(int address)
        {
            RejectCompiledMechanicsRead(address);
            return source.ReadByte(address);
        }

        public byte ReadCartridgeByte(int address)
        {
            RejectCompiledMechanicsRead(address);
            return CartridgeImportSource.Require(source).ReadCartridgeByte(address);
        }

        private void RejectCompiledMechanicsRead(int address)
        {
            if (EnemyProjectileInstructionMechanicsDefinitions.IsCompiledMechanicsByte(address) ||
                MotherBrainHandBeamInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled enemy-projectile mechanics byte ${address:X6}.");
            }

            if (MotherBrainHandBeamInstructionProgramDefinitions.IsPresentationByte(address) ||
                (address & 0xff0000) == 0x860000 &&
                (EnemyProjectileInstructionMechanicsDefinitions.IsVisualOperand((ushort)address) ||
                 EnemyProjectileInstructionMechanicsDefinitions.IsVisualOperand(unchecked((ushort)(address - 1)))))
                _presentationReadBytes.Add(address);
        }

        internal void VerifySelectedDuration(ushort operand, ushort duration)
        {
            int address = 0x860000 | unchecked((ushort)(operand - 2));
            ushort native = (ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8);
            AssertEqual(native, duration, "real projectile frame retains exact native duration");
        }
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
