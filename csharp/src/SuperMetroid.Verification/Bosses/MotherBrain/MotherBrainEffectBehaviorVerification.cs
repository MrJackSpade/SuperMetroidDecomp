using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.Projectiles.MiscDustProjectileBehaviorVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>#1292: Mother Brain's effects, and the shared explosions her attacks and death spawn, run typed programs.</summary>
internal static class MotherBrainEffectBehaviorVerification
{
    /// <summary>
    /// Each real producer starts its typed program, and every effect lives exactly its native lifetime: the bomb and escape
    /// fragment loop, the breath, rainbow charge, rainbow impact and death explosions delete on time, the hand beam spawns
    /// its three fired beams, the onion ring grows to its final radii and its touched impact plays out, each drool releases
    /// and splashes, and the subtitle sleeps; every shown frame keeps its native duration, with the lists' bytes forbidden.
    /// </summary>
    internal static void VerifyMotherBrainEffectBehavior(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        CartridgeReadGuard guard = ForbiddingSharedLists(rom);
        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(16384, 16384)) };

        RoomEnemySystem bombEnemies = CreateRoomEnemySystem();
        RoomEnemyProjectileSlot bomb = bombEnemies.Projectiles.EnemyProjectiles[^1];
        FamilyProjectileStarts.Initialize(bomb, RoomEnemyProjectileKind.MotherBrainBomb, 1024);
        RunForcedTicks(rom, bombEnemies, bomb, samus, 10);
        AssertTrue(bomb.IsActive && bomb.Animation is MotherBrainEffectAnimation { Cursor.Program: MotherBrainEffectProgram.Bomb },
            "room-system Mother Brain bomb keeps looping its program");

        RoomEnemySystem breathEnemies = CreateRoomEnemySystem();
        RoomEnemyProjectileSlot breath = breathEnemies.Projectiles.EnemyProjectiles[^1];
        FamilyProjectileStarts.Initialize(breath, RoomEnemyProjectileKind.MotherBrainPurpleBreathBig, 0);
        RunForcedTicks(rom, breathEnemies, breath, samus, 9);
        AssertTrue(!breath.IsActive, "room-system Mother Brain purple breath reaches its deletion");

        RoomEnemySystem rainbowEnemies = CreateRoomEnemySystem();
        var rainbowState = new MotherBrainEnemyState(new RoomEnemySlot(0, EnemySlotFixtures.Registry))
        {
            Head = new RoomEnemySlot(1, EnemySlotFixtures.Registry) { Position = FixedPosition<RoomPixels>.FromWhole(new(311, 139)) },
        };
        rainbowEnemies.MotherBrainEncounter.MotherBrain = rainbowState;
        rainbowEnemies.MotherBrainBehavior.Head.SpawnMotherBrainRainbowChargingProjectile();
        RoomEnemyProjectileSlot rainbow = rainbowEnemies.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging);
        AssertEqual(311, rainbow.Position.X.Whole, "real rainbow-charge producer pins its initial X to Mother Brain's head");
        AssertEqual(139, rainbow.Position.Y.Whole, "real rainbow-charge producer pins its initial Y to Mother Brain's head");
        AssertLifetime(rainbowEnemies, rainbow, 30, "rainbow charge survives all six five-frame stages");

        RoomEnemySystem explosionEnemies = CreateRoomEnemySystem();
        explosionEnemies.MotherBrainBehavior.RainbowAndDeath.SpawnMotherBrainRainbowExplosion(new Position16<RoomPixels>(256, 128),
            new MotherBrainRainbowExplosionRequest(Offset: new(-9, 7)));
        RoomEnemyProjectileSlot explosion = explosionEnemies.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion);
        AssertEqual(247, explosion.Position.X.Whole, "real rainbow-impact producer applies its signed X offset");
        AssertEqual(135, explosion.Position.Y.Whole, "real rainbow-impact producer applies its signed Y offset");
        AssertTrue(explosion.Animation is MiscDustProjectileAnimation { Cursor.Program: MiscDustProjectileProgram.RainbowBeamExplosion },
            "the rainbow impact starts the shared misc-dust rainbow explosion");
        AssertLifetime(explosionEnemies, explosion, 18, "rainbow impact survives its five-stage 18-frame lifetime");

        MiscDustProjectileProgram[] deathPrograms =
            [MiscDustProjectileProgram.SmallExplosion, MiscDustProjectileProgram.Smoke, MiscDustProjectileProgram.BigExplosion];
        int[] deathExplosionLifetimes = [31, 32, 30];
        for (ushort parameter = 0; parameter < deathExplosionLifetimes.Length; parameter++)
        {
            RoomEnemySystem deathEnemies = CreateRoomEnemySystem();
            deathEnemies.MotherBrainEncounter.MotherBrain = new MotherBrainEnemyState(
                new RoomEnemySlot(0, EnemySlotFixtures.Registry) { Position = FixedPosition<RoomPixels>.FromWhole(new(128, 112)) });
            deathEnemies.MotherBrainBehavior.RainbowAndDeath.SpawnMotherBrainDeathExplosion(
                new MotherBrainDeathExplosionRequest(Offset: new(-5, 9), ProjectileParameter: parameter, SoundEffect: MotherBrainSoundDefinitions.Explosion));
            RoomEnemyProjectileSlot deathExplosion = deathEnemies.Projectiles.EnemyProjectiles.Single(
                projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainDeathExplosion);
            AssertTrue(deathExplosion.Animation is MiscDustProjectileAnimation started && started.Cursor.Program == deathPrograms[parameter],
                $"Mother Brain death explosion {parameter} starts {deathPrograms[parameter]}");
            AssertLifetime(deathEnemies, deathExplosion, deathExplosionLifetimes[parameter], $"Mother Brain death explosion {parameter} survives its lifetime");
        }

        VerifyHandBeam();
        VerifyOnionRing();
        VerifyDrool(RoomEnemyProjectileKind.MotherBrainDrool, 127);
        VerifyDrool(RoomEnemyProjectileKind.MotherBrainDyingDrool, 128);

        RoomEnemySystem fragmentEnemies = CreateRoomEnemySystem();
        RoomEnemyProjectileSlot fragment = fragmentEnemies.Projectiles.EnemyProjectiles[^1];
        FamilyProjectileStarts.Initialize(fragment, RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment, 0);
        RunForcedTicks(rom, fragmentEnemies, fragment, samus, 9);
        AssertTrue(fragment.IsActive && fragment.Animation is MotherBrainEffectAnimation { Cursor.Program: MotherBrainEffectProgram.EscapeDoorParticle },
            "room-system Mother Brain escape fragment keeps looping its program");

        RoomEnemySystem subtitleEnemies = CreateRoomEnemySystem();
        RoomEnemyProjectileSlot subtitle = subtitleEnemies.Projectiles.EnemyProjectiles[^1];
        FamilyProjectileStarts.Initialize(subtitle, RoomEnemyProjectileKind.MotherBrainEscapeSubtitle, 0);
        RunForcedTicks(rom, subtitleEnemies, subtitle, samus, 2);
        AssertTrue(subtitle.IsActive && subtitle.Animation is MotherBrainEffectAnimation { Cursor.Program: MotherBrainEffectProgram.EscapeSubtitle },
            "room-system Mother Brain escape subtitle holds at its sleep");

        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the Mother Brain effect or misc-dust list bytes");
        Console.WriteLine("  Mother Brain effect behavior: every effect starts its typed program and lives its native lifetime.");

        // Runs `frames` passes that keep the projectile alive, then the one that deletes it.
        void AssertLifetime(RoomEnemySystem enemies, RoomEnemyProjectileSlot projectile, int frames, string context)
        {
            for (int frame = 0; frame < frames; frame++)
                Process(rom, enemies, projectile, samus);
            AssertTrue(projectile.IsActive, context);
            Process(rom, enemies, projectile, samus);
            AssertTrue(!projectile.IsActive, $"{context}, then deletes on the following frame");
        }

        void VerifyHandBeam()
        {
            RoomEnemySystem handBeamEnemies = CreateRoomEnemySystem();
            var handBeamState = new MotherBrainEnemyState(new RoomEnemySlot(0, EnemySlotFixtures.Registry) { Position = FixedPosition<RoomPixels>.FromWhole(new(112, 144)) });
            handBeamEnemies.MotherBrainEncounter.MotherBrain = handBeamState;
            var handBeamTarget = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(192, 96)) };
            handBeamEnemies.MotherBrainBehavior.Body.SpawnMotherBrainHandBeamCharging(handBeamState, handBeamTarget);
            RoomEnemyProjectileSlot handBeam = handBeamEnemies.Projectiles.EnemyProjectiles.Single(
                projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainHandBeamCharging);
            ushort referenceCursor = MotherBrainNativeListAddresses.HandBeamProgram;
            ushort referenceOperand = 0;
            int referenceTimer = 1;
            var observedOperands = new HashSet<ushort>();
            for (int frame = 0; frame < 39; frame++)
            {
                if (--referenceTimer == 0)
                {
                    ushort word = cartridge.ReadCartridgeWord(NativeEnemyProjectileListDefinitions.ProjectileBank | referenceCursor);
                    if ((word & NativeEnemyListDefinitions.InstructionBit) != 0)
                    {
                        AssertEqual((ushort)NativeEnemyProjectileInstruction.CallExternalFunctionInY, word,
                            "hand-beam native stage transition calls the child-spawn callback");
                        referenceCursor += 5;
                        word = cartridge.ReadCartridgeWord(NativeEnemyProjectileListDefinitions.ProjectileBank | referenceCursor);
                    }
                    referenceTimer = word;
                    referenceOperand = (ushort)(referenceCursor + 2);
                    referenceCursor += 4;
                }
                handBeamEnemies.Projectiles.ProcessEnemyProjectileInstructions(handBeam, handBeamTarget, Position16<RoomPixels>.Zero);
                AssertEqual(referenceOperand, handBeam.PresentationOperandAddress, $"hand-beam frame {frame} selects the native visual operand");
                AssertEqual((ushort)referenceTimer, handBeam.InstructionTimer.RawValue, $"hand-beam frame {frame} keeps the native remaining duration");
                observedOperands.Add(handBeam.PresentationOperandAddress);
            }
            AssertEqual(MotherBrainHandBeamVisualDefinitions.ProjectileFrameCount,
                observedOperands.Count, "hand-beam selects all 21 installed visual operands");
            AssertTrue(handBeam.IsActive, "hand-beam charge survives all three 13-frame stages");
            AssertEqual(3, handBeamEnemies.Projectiles.EnemyProjectiles.Count(
                    projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainHandBeamFired),
                "three callbacks spawn three fired hand-beam children");
            Process(rom, handBeamEnemies, handBeam, handBeamTarget);
            AssertTrue(!handBeam.IsActive, "hand-beam charge deletes on the frame after its 39-frame lifetime");
        }

        void VerifyOnionRing()
        {
            RoomEnemySystem ringEnemies = CreateRoomEnemySystem();
            var ringState = new MotherBrainEnemyState(new RoomEnemySlot(0, EnemySlotFixtures.Registry))
            {
                Head = new RoomEnemySlot(1, EnemySlotFixtures.Registry) { Position = FixedPosition<RoomPixels>.FromWhole(new(128, 96)) },
            };
            ringEnemies.MotherBrainEncounter.MotherBrain = ringState;
            ringEnemies.MotherBrainBehavior.Head.SpawnMotherBrainOnionRing(ringState, new ByteAngle(0));
            RoomEnemyProjectileSlot ring = ringEnemies.Projectiles.EnemyProjectiles.Single(
                projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainOnionRing);
            for (int frame = 0; frame < 52; frame++)
                Process(rom, ringEnemies, ring, samus);
            AssertEqual(6, ring.Radius.X, "real room onion ring reaches its final X radius");
            AssertEqual(6, ring.Radius.Y, "real room onion ring reaches its final Y radius");
            Process(rom, ringEnemies, ring, samus);
            AssertTrue(ring.Animation is MotherBrainEffectAnimation { Cursor.Program: MotherBrainEffectProgram.OnionRing },
                "real room onion ring holds at its growth program's sleep");

            // The ring's own movement routine handles its Samus hits; its definition's touch list is reached through the
            // common contact path, opened and kept alive here as the definitions check does.
            ring.PreInstruction = EnemyProjectilePreInstruction.MotherBrainsOnionRings;
            ring.GraphicsIndex = 1024;
            ring.CanDamageSamus = true;
            ring.PersistsOnSamusContact = true;
            var touching = new SamusState { Position = ring.Position, Health = ushort.MaxValue };
            AssertTrue(ringEnemies.Projectiles.ResolveEnemyProjectileSamusCollision(ring, touching).HasValue, "Samus touches the onion ring");
            AssertTrue(ring.Animation is MotherBrainEffectAnimation { Cursor.Program: MotherBrainEffectProgram.OnionRingHit },
                "the touched onion ring starts its typed impact program");
            for (int frame = 0; frame < 30; frame++)
                Process(rom, ringEnemies, ring, samus);
            AssertTrue(ring.IsActive, "onion-ring impact survives all six five-frame stages");
            AssertEqual(0, ring.GraphicsIndex, "onion-ring impact selects palette zero");
            AssertEqual(EnemyProjectilePreInstruction.RTS_868170, ring.PreInstruction, "onion-ring impact clears its movement");
            Process(rom, ringEnemies, ring, samus);
            AssertTrue(!ring.IsActive, "onion-ring impact deletes on the frame after its 30-frame lifetime");
        }

        void VerifyDrool(RoomEnemyProjectileKind expectedKind, ushort neckAngleDelta)
        {
            RoomEnemySystem droolEnemies = CreateRoomEnemySystem();
            var state = new MotherBrainEnemyState(new RoomEnemySlot(0, EnemySlotFixtures.Registry))
            {
                Head = new RoomEnemySlot(1, EnemySlotFixtures.Registry) { Position = FixedPosition<RoomPixels>.FromWhole(new(256, 128)) },
                DroolGenerationEnabled = true,
                NeckAngleDelta = neckAngleDelta,
            };
            droolEnemies.MotherBrainEncounter.MotherBrain = state;
            droolEnemies.MotherBrainBehavior.Head.SpawnMotherBrainDrool(state);
            RoomEnemyProjectileSlot drool = droolEnemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.Kind == expectedKind);
            ushort attachedY = drool.Position.Y.Whole;

            for (int frame = 0; frame < 50; frame++)
                Process(rom, droolEnemies, drool, samus);
            AssertEqual(EnemyProjectilePreInstruction.MotherBrainsDrool, drool.PreInstruction,
                $"{expectedKind} remains attached for five ten-frame stages");
            Process(rom, droolEnemies, drool, samus);
            AssertEqual(EnemyProjectilePreInstruction.MotherBrainsDrool_Falling, drool.PreInstruction,
                $"{expectedKind} switches to falling after its fifth attached stage");
            AssertEqual(unchecked((ushort)(attachedY + 12)), drool.Position.Y.Whole, $"{expectedKind} release drops the drool twelve pixels");
            AssertTrue(drool.Animation is MotherBrainEffectAnimation { Cursor.Program: MotherBrainEffectProgram.Drool },
                $"{expectedKind} release frame holds at its program's sleep");

            drool.Position = drool.Position.WithWholeY(215);
            MotherBrainProjectiles.RunMotherBrainFallingDroolPreInstruction(drool);
            AssertTrue(drool.Animation is MotherBrainEffectAnimation { Cursor.Program: MotherBrainEffectProgram.DroolHitFloor },
                $"{expectedKind} floor impact starts the splash program");
            AssertLifetime(droolEnemies, drool, 40, $"{expectedKind} splash survives all four ten-frame stages");
        }
    }

    /// <summary>
    /// The live-bomb count rises with each spawned bomb and falls with each deleted one, and a
    /// deletion with no counted bomb fails instead of wrapping to the cartridge's $FFFF.
    /// </summary>
    internal static void VerifyMotherBrainBombCount()
    {
        var state = new MotherBrainEnemyState(new RoomEnemySlot(0, EnemySlotFixtures.Registry));
        state.RecordBombSpawned();
        state.RecordBombSpawned();
        AssertEqual(2, state.BombCounter, "two spawned bombs are counted");
        state.RecordBombRemoved();
        state.RecordBombRemoved();
        AssertEqual(0, state.BombCounter, "both deletions are uncounted");
        AssertThrows<InvalidOperationException>(state.RecordBombRemoved,
            "deleting a bomb that was never counted is rejected");
        AssertEqual(0, state.BombCounter, "the rejected deletion leaves the count at zero");
        Console.WriteLine("  Mother Brain bombs: the live count follows spawns and deletions and never wraps.");
    }

    /// <summary>Runs the Mother Brain live-bomb count check.</summary>
    [VerificationCommand("--mother-brain-bomb-count")]
    internal static int MotherBrainBombCountCommand()
    {
        Suite(nameof(VerifyMotherBrainBombCount), () => VerifyMotherBrainBombCount());
        return 0;
    }

    /// <summary>Runs Mother Brain's effect program oracle and behavior suite.</summary>
    [VerificationCommand("--mother-brain-effect-programs")]
    internal static int MotherBrainEffectProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(MotherBrainEffectAnimationsVerification.VerifyMotherBrainEffectAnimations), () => MotherBrainEffectAnimationsVerification.VerifyMotherBrainEffectAnimations(rom));
        Suite(nameof(VerifyMotherBrainEffectBehavior), () => VerifyMotherBrainEffectBehavior(rom));
        return 0;
    }
}
