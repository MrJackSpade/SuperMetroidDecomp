using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using static SuperMetroid.Verification.Enemies.Projectiles.EnemyProjectileProgramOracleVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Verification.Enemies.Projectiles;
using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled bank-$86 projectile mechanics against the ROM and guards production program execution against live source reads.</summary>
internal static class EnemyProjectileInstructionMechanicsDefinitionsVerification
{
    /// <summary>
    /// Compares every compiled Mother Brain/misc-dust mechanics word to the pinned ROM,
    /// then runs all 39 production programs while rejecting reads from those source bytes.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for enemy projectile instruction mechanics definitions")]
    internal static void VerifyEnemyProjectileInstructionMechanicsDefinitions()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  Enemy-projectile instruction mechanics: cartridge comparison skipped " +
                "(private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        for (int index = 0;
             index < EnemyProjectileInstructionMechanicsDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                EnemyProjectileInstructionMechanicsDefinitionsTooling.MechanicsWord(index);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x860000 | definition.Address));
            AssertEqual(definition.Value, native,
                $"bank-$86 projectile mechanics word ${definition.Address:X4}");

        }

        var presentationReadBytes = new HashSet<int>();
        var guarded = NewEnemyProjectileMechanicsReadGuard(rom, presentationReadBytes);
        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(0x4000, 0x4000)) };

        Suite(nameof(VerifyCompiledRoomSharedPrograms), () => VerifyCompiledRoomSharedPrograms(guarded, samus, rom));

        _ = EnemyProjectileInstructionMechanicsDefinitions.ReadMechanicsWord(
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBombInitial);
        // Tiered JIT can promote these warmed methods during a long, shared suite.
        // A one-time promotion is not a per-frame allocation. Repeat a bounded
        // measurement until the steady-state pass is reached; persistent allocation
        // still fails after all four passes.
        long allocated = long.MaxValue;
        int checksum = 0;
        for (int pass = 0; pass < 4 && allocated != 0; pass++)
        {
            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 65536; index++)
            {
                checksum += EnemyProjectileInstructionMechanicsDefinitions.ReadMechanicsWord(
                    EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBombInitial);
            }
            allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        }
        AssertTrue(checksum != 0, "projectile mechanics allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed projectile mechanics lookups allocate no per-frame storage");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "all production programs avoid compiled mechanics source bytes");
        AssertEqual(0, presentationReadBytes.Count,
            "all projectile programs select installed frames without live presentation reads");

        Console.WriteLine(
            $"  Enemy-projectile instruction mechanics: " +
            $"{EnemyProjectileInstructionMechanicsDefinitionsTooling.MechanicsWordCount} words and " +
            "all 39 Mother Brain/misc-dust programs pass with mechanics reads forbidden.");
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
        Action<RoomEnemyProjectileSlot, RoomEnemyProjectileKind, ushort> initialize =
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition;

        for (ushort animation = 0;
             animation < EnemyProjectileInstructionMechanicsDefinitions.MiscDustProgramCount;
             animation++)
        {
            RoomEnemySystem enemies = CreateRoomEnemySystem();
            enemies.Projectiles.SpawnRoomGraphicsDustExplosion(new Position16<RoomPixels>((ushort)128, (ushort)96), (MiscDustAnimation)animation);
            RoomEnemyProjectileSlot dust = enemies.Projectiles.EnemyProjectiles.Single(
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
        smokeEnemies.EyeDoorBehavior.SpawnEyeDoorProjectile(
            new EyeDoorProjectileRequest(
                EyeDoorProjectileDefinition.Smoke,
                Parameter: 3,
                PlmBlockIndex: 4 * 16 + 7,
                DoorBit: 0),
            roomWidthInBlocks: 16,
            new Bank80SystemState());
        RoomEnemyProjectileSlot smoke = smokeEnemies.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.EyeDoorSmoke);
        RunForcedTicks(smokeEnemies, smoke, 7);
        AssertTrue(!smoke.IsActive,
            "Eye Door smoke reaches delete through the shared compiled mechanics owner");

        RoomEnemySystem bombEnemies = CreateRoomEnemySystem();
        RoomEnemyProjectileSlot bomb = bombEnemies.Projectiles.EnemyProjectiles[^1];
        initialize(bomb, RoomEnemyProjectileKind.MotherBrainBomb, 0x0400);
        RunForcedTicks(bombEnemies, bomb, 10);
        AssertTrue(bomb.IsActive,
            "room-system Mother Brain bomb survives its compiled animation loop");
        AssertTrue(bomb.InstructionPointer is >= 0xc772 and <= 0xc792,
            "room-system Mother Brain bomb remains inside its compiled loop");

        RoomEnemySystem breathEnemies = CreateRoomEnemySystem();
        RoomEnemyProjectileSlot breath = breathEnemies.Projectiles.EnemyProjectiles[^1];
        initialize(breath, RoomEnemyProjectileKind.MotherBrainPurpleBreathBig, 0);
        RunForcedTicks(breathEnemies, breath, 9);
        AssertTrue(!breath.IsActive,
            "room-system Mother Brain purple breath reaches compiled deletion");

        RoomEnemySystem rainbowEnemies = CreateRoomEnemySystem();
        var motherBrainBody = new RoomEnemySlot(0);
        var motherBrainHead = new RoomEnemySlot(1)
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(0x0137, 0x008b)),
        };
        var motherBrainState = new MotherBrainEnemyState(motherBrainBody)
        {
            Head = motherBrainHead,
        };
        rainbowEnemies.MotherBrainEncounter.MotherBrain = motherBrainState;
        rainbowEnemies.MotherBrainBehavior.SpawnMotherBrainRainbowChargingProjectile();
        RoomEnemyProjectileSlot rainbow = rainbowEnemies.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind ==
                RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging);
        AssertEqual(0x0137, rainbow.Position.X.Whole,
            "real rainbow-charge producer pins its initial X to Mother Brain's head");
        AssertEqual(0x008b, rainbow.Position.Y.Whole,
            "real rainbow-charge producer pins its initial Y to Mother Brain's head");
        for (int frame = 0; frame < 30; frame++)
            Process(rainbowEnemies, rainbow, samus);
        AssertTrue(rainbow.IsActive,
            "rainbow charge survives all six exact five-frame animation stages");
        Process(rainbowEnemies, rainbow, samus);
        AssertTrue(!rainbow.IsActive,
            "rainbow charge deletes on the frame after its exact 30-frame lifetime");

        RoomEnemySystem explosionEnemies = CreateRoomEnemySystem();
        explosionEnemies.MotherBrainBehavior.SpawnMotherBrainRainbowExplosion(new Position16<RoomPixels>(0x0100, 0x0080),
                new MotherBrainRainbowExplosionRequest(Offset: new(-9, 7)));
        RoomEnemyProjectileSlot explosion = explosionEnemies.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind ==
                RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion);
        AssertEqual(0x00f7, explosion.Position.X.Whole,
            "real rainbow-impact producer applies its signed X offset");
        AssertEqual(0x0087, explosion.Position.Y.Whole,
            "real rainbow-impact producer applies its signed Y offset");
        for (int frame = 0; frame < 18; frame++)
            Process(explosionEnemies, explosion, samus);
        AssertTrue(explosion.IsActive,
            "rainbow impact survives its exact five-stage 18-frame lifetime");
        Process(explosionEnemies, explosion, samus);
        AssertTrue(!explosion.IsActive,
            "rainbow impact deletes on the frame after its exact lifetime");

        int[] deathExplosionLifetimes = [31, 32, 30];
        for (ushort parameter = 0; parameter < deathExplosionLifetimes.Length; parameter++)
        {
            RoomEnemySystem deathExplosionEnemies = CreateRoomEnemySystem();
            var deathBody = new RoomEnemySlot(0)
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0070)),
            };
            deathExplosionEnemies.MotherBrainEncounter.MotherBrain = new MotherBrainEnemyState(deathBody);
            deathExplosionEnemies.MotherBrainBehavior.SpawnMotherBrainDeathExplosion(new MotherBrainDeathExplosionRequest(
                        Offset: new(-5, 9),
                        ProjectileParameter: parameter,
                        SoundEffect: 0));
            RoomEnemyProjectileSlot deathExplosion =
                deathExplosionEnemies.Projectiles.EnemyProjectiles.Single(
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
            Position = FixedPosition<RoomPixels>.FromWhole(new(0x0070, 0x0090)),
        };
        var handBeamState = new MotherBrainEnemyState(handBeamBody);
        handBeamEnemies.MotherBrainEncounter.MotherBrain = handBeamState;
        var handBeamTarget = new SamusState
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(0x00c0, 0x0060)),
        };
        handBeamEnemies.MotherBrainBehavior.SpawnMotherBrainHandBeamCharging(handBeamState, handBeamTarget);
        RoomEnemyProjectileSlot handBeam = handBeamEnemies.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind ==
                RoomEnemyProjectileKind.MotherBrainHandBeamCharging);
        ushort referenceCursor = MotherBrainNativeListAddresses.HandBeamProgram;
        ushort referenceOperand = 0;
        int referenceTimer = 1;
        var observedHandBeamOperands = new HashSet<ushort>();
        for (int frame = 0; frame < 39; frame++)
        {
            if (--referenceTimer == 0)
            {
                ushort word = CartridgeImportSource.Require(nativeReference).ReadCartridgeWord((0x860000 | referenceCursor));
                if (word >= 0x8000)
                {
                    AssertEqual((ushort)EnemyProjectileInstruction.CallExternalFunctionInY,
                        word, "hand-beam native stage transition calls the child-spawn callback");
                    referenceCursor += 5;
                    word = CartridgeImportSource.Require(nativeReference).ReadCartridgeWord((0x860000 | referenceCursor));
                }
                referenceTimer = word;
                referenceOperand = (ushort)(referenceCursor + 2);
                referenceCursor += 4;
            }
            handBeamEnemies.Projectiles.ProcessEnemyProjectileInstructions(handBeam, handBeamTarget, Position16<RoomPixels>.Zero);
            AssertEqual(referenceOperand, handBeam.PresentationOperandAddress,
                $"hand-beam frame {frame} selects the native visual operand");
            AssertEqual((ushort)referenceTimer, handBeam.InstructionTimer.RawValue,
                $"hand-beam frame {frame} preserves the native remaining duration");
            observedHandBeamOperands.Add(handBeam.PresentationOperandAddress);
        }
        AssertEqual(MotherBrainHandBeamVisualDefinitions.ProjectileFrameCount,
            observedHandBeamOperands.Count, "hand-beam selects all 21 installed visual operands");
        AssertTrue(handBeam.IsActive,
            "hand-beam charge survives all three exact 13-frame stages");
        AssertEqual(3, handBeamEnemies.Projectiles.EnemyProjectiles.Count(
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
            Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0060)),
        };
        var ringState = new MotherBrainEnemyState(ringBody)
        {
            Head = ringHead,
        };
        ringEnemies.MotherBrainEncounter.MotherBrain = ringState;
        ringEnemies.MotherBrainBehavior.SpawnMotherBrainOnionRing(ringState, (byte)0);
        RoomEnemyProjectileSlot ring = ringEnemies.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainOnionRing);
        for (int frame = 0; frame < 52; frame++)
            Process(ringEnemies, ring, samus);
        AssertEqual(6, ring.Radius.X,
            "real room onion ring reaches its compiled final X radius");
        AssertEqual(6, ring.Radius.Y,
            "real room onion ring reaches its compiled final Y radius");
        Process(ringEnemies, ring, samus);
        AssertEqual(0xc462, ring.InstructionPointer,
            "real room onion ring sleeps at its authored initial-program terminal");

        ring.InstructionPointer =
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBlueRingTouch;
        ring.InstructionTimer = NativeCountdown.Start(1);
        ring.PreInstruction =
            (ushort)EnemyProjectilePreInstruction.MotherBrainsOnionRings;
        ring.GraphicsIndex = 0x0400;
        for (int frame = 0; frame < 30; frame++)
            Process(ringEnemies, ring, samus);
        AssertTrue(ring.IsActive,
            "onion-ring impact survives all six exact five-frame stages");
        AssertEqual(0, ring.GraphicsIndex,
            "onion-ring impact's duplicate native opcode selects palette zero");
        AssertEqual((ushort)EnemyProjectilePreInstruction.RTS_868170, ring.PreInstruction,
            "onion-ring impact clears its movement pre-instruction");
        Process(ringEnemies, ring, samus);
        AssertTrue(!ring.IsActive,
            "onion-ring impact deletes on the frame after its exact 30-frame lifetime");

        Suite(nameof(VerifyDroolVariant), () => VerifyDroolVariant(RoomEnemyProjectileKind.MotherBrainDrool, 0x007f));
        Suite(nameof(VerifyDroolVariant), () => VerifyDroolVariant(RoomEnemyProjectileKind.MotherBrainDyingDrool, 0x0080));

        RoomEnemySystem fragmentEnemies = CreateRoomEnemySystem();
        RoomEnemyProjectileSlot fragment = fragmentEnemies.Projectiles.EnemyProjectiles[^1];
        initialize(fragment, RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment, 0);
        RunForcedTicks(fragmentEnemies, fragment, 9);
        AssertTrue(fragment.IsActive,
            "room-system Mother Brain escape fragment survives its compiled loop");
        AssertTrue(fragment.InstructionPointer is >= 0xca26 and <= 0xca42,
            "room-system Mother Brain escape fragment remains inside its compiled loop");

        RoomEnemySystem subtitleEnemies = CreateRoomEnemySystem();
        RoomEnemyProjectileSlot subtitle = subtitleEnemies.Projectiles.EnemyProjectiles[^1];
        initialize(subtitle, RoomEnemyProjectileKind.MotherBrainEscapeSubtitle, 0);
        RunForcedTicks(subtitleEnemies, subtitle, 2);
        AssertTrue(subtitle.IsActive,
            "room-system Mother Brain escape subtitle survives compiled sleep");
        AssertEqual(0xcb11, subtitle.InstructionPointer,
            "room-system Mother Brain escape subtitle retains its sleep opcode");

        void Process(RoomEnemySystem enemies, RoomEnemyProjectileSlot projectile, SamusState target)
        {
            bool selectsFrame = projectile.InstructionTimer.RawValue == 1;
            enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, target, Position16<RoomPixels>.Zero);
            if (!projectile.IsActive || !selectsFrame || projectile.InstructionTimer.IsZero || projectile.PresentationOperandAddress == 0)
                return;
            ushort operand = projectile.PresentationOperandAddress;
            AssertEqual(unchecked((ushort)(projectile.InstructionPointer - 2)), operand,
                "real projectile interpreter selects its exact native operand address");
            AssertTrue(EnemyProjectilePresentationFrameDefinitions.Contains(operand),
                "real projectile interpreter selects an installed-art identity");
            AssertEqual(EnemyProjectileSpritemapDefinitions.BlankSpritemap, projectile.SpritemapPointer,
                "real projectile interpreter leaves spritemap resolution to installed content");
            if (bus is CartridgeReadGuard)
            {
                ushort native = ReadNativeEnemyProjectileProgramWord(nativeReference, unchecked((ushort)(operand - 2)));
                AssertEqual(native, projectile.InstructionTimer.RawValue, "real projectile frame retains exact native duration");
            }
        }
        RoomEnemySystem CreateRoomEnemySystem()
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(bus);
            enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => 0);
            enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0);
            return enemies;
        }

        void RunForcedTicks(
            RoomEnemySystem enemies,
            RoomEnemyProjectileSlot projectile,
            int count)
        {
            for (int tick = 0; tick < count; tick++)
            {
                projectile.InstructionTimer = NativeCountdown.Start(1);
                Process(enemies, projectile, samus);
            }
        }

        void VerifyDroolVariant(RoomEnemyProjectileKind expectedKind, ushort neckAngleDelta)
        {
            RoomEnemySystem droolEnemies = CreateRoomEnemySystem();
            var body = new RoomEnemySlot(0);
            var head = new RoomEnemySlot(1)
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(0x0100, 0x0080)),
            };
            var state = new MotherBrainEnemyState(body)
            {
                Head = head,
                DroolGenerationEnabled = true,
                NeckAngleDelta = neckAngleDelta,
            };
            droolEnemies.MotherBrainEncounter.MotherBrain = state;
            droolEnemies.MotherBrainBehavior.SpawnMotherBrainDrool(state);
            RoomEnemyProjectileSlot drool = droolEnemies.Projectiles.EnemyProjectiles.Single(
                projectile => projectile.Kind == expectedKind);
            ushort attachedY = drool.Position.Y.Whole;

            for (int frame = 0; frame < 50; frame++)
                Process(droolEnemies, drool, samus);
            AssertEqual(
                (ushort)EnemyProjectilePreInstruction.MotherBrainsDrool,
                drool.PreInstruction,
                $"{expectedKind} remains attached for five exact ten-frame stages");
            Process(droolEnemies, drool, samus);
            AssertEqual(
                (ushort)EnemyProjectilePreInstruction.MotherBrainsDrool_Falling,
                drool.PreInstruction,
                $"{expectedKind} switches to falling after its fifth attached stage");
            AssertEqual(unchecked((ushort)(attachedY + 12)), drool.Position.Y.Whole,
                $"{expectedKind} release command moves the actor down twelve pixels");
            AssertEqual(0xc8ce, drool.InstructionPointer,
                $"{expectedKind} release frame reaches its terminal sleep");

            drool.Position = drool.Position.WithWholeY(0x00d7);
            MotherBrainBehavior.RunMotherBrainFallingDroolPreInstruction(drool);
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

    /// <summary>Rejects reads of compiled mechanics operands while recording any remaining live projectile presentation reads.</summary>
    private static CartridgeReadGuard NewEnemyProjectileMechanicsReadGuard(ISnesAddressSpace source,
        HashSet<int> presentationReadBytes) =>
        new CartridgeReadGuard(source)
            .Forbid(address =>
                    EnemyProjectileInstructionMechanicsDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled enemy-projectile mechanics byte ${address:X6}.")
            .Observe(address =>
                    (SnesAddressMath.BankBase(address)) == 0x860000 &&
                    (EnemyProjectileInstructionMechanicsDefinitions.IsVisualOperand((ushort)address) ||
                     EnemyProjectileInstructionMechanicsDefinitions.IsVisualOperand(unchecked((ushort)(address - 1)))),
                address => presentationReadBytes.Add(address));

    /// <summary>Runs the ROM oracle, guarded program execution, and allocation checks for projectile mechanics.</summary>
    [VerificationCommand("--enemy-projectile-instruction-mechanics")]
    internal static int EnemyProjectileInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyEnemyProjectileInstructionMechanicsDefinitions), () => VerifyEnemyProjectileInstructionMechanicsDefinitions());
        return 0;
    }
}
