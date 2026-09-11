using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyCrystalFlashRuntime()
    {
        VerifyCrystalFlashRuntimeRoute(capacity: 11, refill: false);
        VerifyCrystalFlashRuntimeRoute(capacity: 10, refill: true);
        VerifyCrystalFlashRuntimeRoute(capacity: 10, refill: false);
    }

    private static void VerifyCrystalFlashRuntimeRoute(ushort capacity, bool refill)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        // Keep the constructed clearing visible; the old mechanics-only fixture
        // left the displayed camera at zero, outside the tested Samus trajectory.
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite, cameraX: 384, cameraY: 384);
        for (int y = 1; y <= 2; y++)
        for (int x = 1; x <= 2; x++)
            runtime.Camera!.Scrolls.SetLogicalState(x, y, RoomScrollState.Green);
        var level = runtime.LevelData!;
        for (int y = 16; y < 36; y++)
        for (int x = 16; x < 48; x++)
        {
            int index = y * level.WidthInBlocks + x;
            level.SetForegroundEntry(index, RoomLevelWord.Create(0, 0,
                y >= 32 ? RoomCollisionType.SolidBlock : RoomCollisionType.Air).Raw);
            level.SetBehavior(index, 0);
        }
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.Pose = SamusPoseIds.MorphBallGroundRightPose;
        samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
        samus.Health = 49; samus.MaxHealth = 1499;
        samus.Missiles = samus.MaxMissiles = 10;
        samus.SuperMissiles = samus.MaxSuperMissiles = 10;
        samus.PowerBombs = samus.MaxPowerBombs = capacity;
        samus.SelectedHudItem = 3;
        samus.XPosition = 512;
        samus.RefreshCollisionRadii(bus);
        samus.YPosition = (ushort)(511 - samus.Kinematics.YRadius);
        samus.InitializeAnimation(bus);
        runtime.StepFrame(0);
        ushort startingY = samus.YPosition;
        runtime.StepFrame(runtime.ControllerBindings.Shoot);
        AssertEqual(capacity - 1, samus.PowerBombs, "normal placement consumes one Power Bomb");
        ushort chord = (ushort)(SnesButton.Down | SnesButton.L | SnesButton.R | SnesButton.X);
        int started = -1, finished = -1;
        bool bubble = false, drained = false;
        RoomEnemySlot? contactEnemy = null;
        bool contactVerified = false;
        int visibleBodyFrames = 0, visibleWindowFrames = 0;
        for (int frame = 0; frame < 1000; frame++)
        {
            if (refill && frame == 60)
            {
                // A constructed drop owner collides an actual Power Bomb pickup with
                // this runtime's Samus. No direct ammo write substitutes for collection.
                AssertEnemyPickupEffect(EnemyPickupKind.PowerBomb, samus, expectedSound: 5,
                    assertEffect: collected => AssertEqual(10, collected.PowerBombs,
                        "refill after placement reaches ten without increasing capacity"));
            }
            bool active = samus.CrystalFlash.Phase != CrystalFlashPhase.Inactive;
            bool testContact = capacity == 11 && started >= 0 && frame == started + 30;
            if (testContact)
            {
                AssertEqual(CrystalFlashPhase.DrainingAmmo, samus.CrystalFlash.Phase,
                    "contact fixture enters the intended ammo-drain phase");
                // Replace the population at the contact boundary; retain retail AI,
                // header damage and the runtime's ordinary collision ordering. A
                // zero-only RNG would hang native-style drop selection on death.
                var contactBus = new CrystalFlashContactPopulation(bus, samus.XPosition, samus.YPosition);
                runtime.Enemies.Load(contactBus, CrystalFlashContactDefinitions.Pointer,
                    CrystalFlashContactDefinitions.Pointer, runtime.Vram, runtime.Cgram, () => 1);
                contactEnemy = runtime.Enemies.Slots[0];
                contactEnemy.VariableC = contactEnemy.VariableD = 0;
                // Contact runs before AI publishes its first map. Seed that same
                // authored map so this fixture tests contact on this exact frame.
                contactEnemy.SpritemapPointer = (ushort)(bus.ReadByte(CrystalFlashContactDefinitions.AiBank | (contactEnemy.CurrentInstruction + 2)) |
                    bus.ReadByte(CrystalFlashContactDefinitions.AiBank | (contactEnemy.CurrentInstruction + 3)) << 8);
            }
            ushort beforeHealth = samus.Health;
            // Later projectile processing can delete the actor and clear its header.
            // Read the contact damage before stepping, not from a recycled slot.
            ushort contactDamage = testContact ? contactEnemy!.Definition.Damage : (ushort)0;
            // Once activated, challenge the inert input handler with movement/jump.
            ushort input = active ? (ushort)(SnesButton.Right | SnesButton.A) : chord;
            runtime.StepFrame(input);
            if (testContact)
            {
                int afterDamage = Math.Max(0, beforeHealth - contactDamage);
                int expected = Math.Min(samus.MaxHealth, afterDamage + ((runtime.NmiFrameCounter & 7) == 0 ? 50 : 0));
                AssertTrue(contactDamage > 0, "constructed contact actor has nonzero retail damage");
                AssertEqual(expected, samus.Health, "enemy contact damages Crystal Flash before its energy refill");
                AssertEqual(0, samus.InvincibilityTimer, "Crystal Flash clears immunity after actual contact");
                AssertEqual(0, samus.KnockbackTimer, "Crystal Flash suppresses the contact knockback request");
                AssertTrue(!samus.KnockbackActive, "contact does not install ordinary knockback during ammo drain");
                contactEnemy!.XPosition += 128;
                contactVerified = true;
                Console.WriteLine($"Runtime Crystal Flash contact: health {beforeHealth}->{samus.Health}, retail damage={contactDamage}; no retained immunity or knockback.");
            }
            if (started < 0 && samus.CrystalFlash.Phase != CrystalFlashPhase.Inactive) started = frame;
            bubble |= runtime.BombProjectiles.PowerBombExplosion.Phase == PowerBombExplosionPhase.CrystalFlashExplosion;
            drained |= samus.CrystalFlash.Phase == CrystalFlashPhase.DrainingAmmo;
            if (started >= 0)
            {
                var visible = VerifyCrystalFlashVisualFrame(runtime, frame - started);
                if (visible.Body) visibleBodyFrames++;
                if (visible.Window) visibleWindowFrames++;
                AssertEqual(512, samus.XPosition, "Crystal Flash owns horizontal movement");
                AssertEqual(startingY - Math.Min(frame - started + 1, 10) * 2, samus.YPosition,
                    "held Jump cannot replace the native Crystal Flash vertical trajectory");
                if (samus.CrystalFlash.Phase == CrystalFlashPhase.Inactive) { finished = frame; break; }
            }
        }
        if (capacity == 10 && !refill)
        {
            AssertEqual(-1, started, "nine remaining Power Bombs reject Crystal Flash");
            AssertEqual(9, samus.PowerBombs, "failed activation preserves remaining Power Bombs");
            AssertEqual(49, samus.Health, "failed activation restores no energy");
            AssertTrue(!runtime.BombProjectiles.PowerBombExplosion.IsArmed, "failed activation releases the Power Bomb lock");
            Console.WriteLine("Runtime Crystal Flash: ten-capacity no-refill control rejects activation.");
            return;
        }
        AssertTrue(started >= 0 && finished > started && bubble && drained, "runtime completes the activated Crystal Flash and bubble");
        if (capacity == 11) AssertTrue(contactVerified, "runtime exercised real enemy contact during Crystal Flash");
        AssertEqual(1499, samus.Health, "runtime restores energy");
        AssertEqual(0, samus.Missiles, "runtime consumes ten missiles");
        AssertEqual(0, samus.SuperMissiles, "runtime consumes ten supers");
        AssertEqual(0, samus.PowerBombs, "runtime consumes ten remaining Power Bombs");
        Console.WriteLine($"Crystal Flash visual coverage: body={visibleBodyFrames}, window={visibleWindowFrames} frames.");
        AssertEqual(250, visibleBodyFrames, "complete observed Crystal Flash body visibility duration");
        AssertEqual(34, visibleWindowFrames, "native Crystal Flash color window visibility duration");
        for (int frame = 0; frame < 30; frame++) runtime.StepFrame((ushort)SnesButton.Right);
        AssertTrue(samus.XPosition > 512, "normal movement resumes after Crystal Flash");
        Console.WriteLine($"Runtime Crystal Flash: capacity={capacity}, refill={refill}, activation={started}, completion={finished}; placement, bubble, resources and movement ownership pass.");
    }

    private static void VerifyCrystalFlashLifetime(string rom, string nativeCsv)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        AssertEqual("CA77210D138C654AEF79E44AAA897B5BE0F79244E2F72AB362D46303521BC043",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(nativeCsv))),
            "accepted original-CPU lifetime capture");
        string[] rows = File.ReadAllLines(nativeCsv);
        AssertEqual("left,offset,frame,phase,pose,anim,timer,y,health,missiles,supers,pbs,immunity,knockback", rows[0], "native lifetime schema");
        int row = 1;
        for (int left = 0; left < 2; left++)
        for (int offset = 0; offset < 8; offset++)
        {
            var samus = new SamusState
            {
                Pose = left != 0 ? SamusPoseIds.MorphBallGroundLeftPose : SamusPoseIds.MorphBallGroundRightPose,
                XPosition = 128, YPosition = 128, Health = 49, MaxHealth = 1499,
                Missiles = 10, SuperMissiles = 10, PowerBombs = 10,
            };
            samus.RefreshCollisionRadii(bus);
            ushort input = (ushort)(SnesButton.Down | SnesButton.L | SnesButton.R | SnesButton.X);
            AssertTrue(samus.CrystalFlash.TryBegin(bus, samus, input), "lifetime starts from valid activation");
            int frame = 0;
            do
            {
                samus.InvincibilityTimer = 77;
                samus.KnockbackTimer = 5;
                samus.CrystalFlash.Step(bus, samus, (ushort)(frame + offset));
                samus.AnimateNoFx(bus, input);
                if (samus.PendingTransitionalPose is not null)
                    samus.ApplyPendingVerifiedAnimationTransition(bus);
                string actual = $"{left},{offset},{frame},{(int)samus.CrystalFlash.Phase},{samus.Pose:X4},{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4},{samus.YPosition:X4},{samus.Health:X4},{samus.Missiles:X4},{samus.SuperMissiles:X4},{samus.PowerBombs:X4},{samus.InvincibilityTimer:X4},{samus.KnockbackTimer:X4}";
                AssertTrue(row < rows.Length, "native lifetime capture is not truncated");
                AssertEqual(rows[row++], actual, $"native lifetime left={left}, offset={offset}, frame={frame}");
                if (++frame > 400) throw new InvalidDataException("Crystal Flash did not finish.");
            } while (samus.CrystalFlash.Phase != CrystalFlashPhase.Inactive);
        }
        AssertEqual(rows.Length, row, "native lifetime has no unconsumed rows");
        Console.WriteLine($"Crystal Flash: {row - 1} original-CPU lifetime frames match across both facings and all eight NMI phases.");
    }

    /// <summary>Compare real Power Bomb cleanup admission with the original bank-$88 CPU probe.</summary>
    private static void VerifyCrystalFlashCleanup(string rom, string nativeCsv)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        string hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(nativeCsv)));
        int cases = hash switch
        {
            "CCD507BE8423FEC78122CD95458577F21F58624510578184BF51EE25FF22A5F3" => 18,
            "4A6DEB28F4CE497B73D45EFFC7164C78D62F4A6B12A113FE8CE581A30AE3A223" => 44,
            _ => throw new InvalidDataException("Unrecognized original-CPU Crystal Flash cleanup capture."),
        };
        string[] rows = File.ReadAllLines(nativeCsv);
        AssertEqual(cases * 2 + 1, rows.Length, "complete native Crystal Flash matrix");
        AssertEqual("case,left,pose,flag,immunity,knockback,health,missiles,supers,pbs", rows[0], "native cleanup trace schema");
        int row = 1;
        for (int test = 0; test < cases; test++)
        for (int left = 0; left < 2; left++)
        {
            var samus = new SamusState
            {
                Pose = left != 0 ? SamusPoseIds.MorphBallGroundLeftPose : SamusPoseIds.MorphBallGroundRightPose,
                XPosition = 128, YPosition = 128, Health = 49, MaxHealth = 99,
                Missiles = 10, SuperMissiles = 10, PowerBombs = 10,
                InvincibilityTimer = 96, KnockbackTimer = 5,
            };
            samus.RefreshCollisionRadii(bus);
            ushort input = (ushort)(SnesButton.Down | SnesButton.L | SnesButton.R | SnesButton.X);
            switch (test)
            {
                case 1: samus.Health = 50; break;
                case 2: samus.Health = 51; break;
                case 3: samus.Missiles = 9; break;
                case 4: samus.SuperMissiles = 9; break;
                case 5: samus.PowerBombs = 9; break;
                case 6: samus.ReserveEnergy = 1; break;
                case 7: samus.Kinematics.YSpeed = 1; break;
                case 8: samus.Kinematics.YSubspeed = 1; break;
                case 9: samus.XPosition++; break;
                case 10: samus.YPosition++; break;
                case 11: samus.Kinematics.XSubposition = 0xffff; break;
                case 12: samus.Kinematics.YSubposition = 0xffff; break;
                case 13: input ^= (ushort)SnesButton.Down; break;
                case 14: input |= (ushort)SnesButton.A; break;
                case 15: samus.MaxReserveEnergy = 100; break;
                case 16: samus.MaxPowerBombs = 10; break;
                case 17: samus.Health = 0; break;
                case >= 18 and < 34:
                    int chord = test - 18;
                    input = (ushort)(((chord & 1) != 0 ? SnesButton.Down : 0) |
                        ((chord & 2) != 0 ? SnesButton.L : 0) |
                        ((chord & 4) != 0 ? SnesButton.R : 0) |
                        ((chord & 8) != 0 ? SnesButton.X : 0));
                    break;
                case 34:
                case 35:
                    // The runtime normalizes physical bindings before bomb alpha.
                    // Exercise that actual bridge instead of changing TryBegin's
                    // canonical Shoot argument to accommodate the test.
                    input = ControllerBindings.Default.AssignAndSwap(0, (ushort)SnesButton.B).Normalize(
                        (ushort)(SnesButton.Down | SnesButton.L | SnesButton.R |
                            (test == 34 ? SnesButton.B : SnesButton.X)));
                    break;
                case 36: samus.XPosition--; break;
                case 37: samus.YPosition--; break;
                case 38: samus.Missiles = 11; break;
                case 39: samus.SuperMissiles = 11; break;
                case 40: samus.PowerBombs = 11; break;
                case 41: samus.Missiles = 0; break;
                case 42: samus.SuperMissiles = 0; break;
                case 43: samus.PowerBombs = 0; break;
            }
            var level = new RoomLevelData(16, 16, new ushort[256], new byte[256], new ushort[256], new byte[0x2000]);
            var bombs = new SamusBombProjectileSystem();
            bombs.PowerBombExplosion.Arm();
            bombs.PowerBombExplosion.Spawn(128, 128);
            int frames = 0;
            do
            {
                bombs.StepFrame(bus, level, samus, input, 0, deferSamusOverlap: true);
                if (++frames > 1000) throw new InvalidDataException("Power Bomb never reached cleanup.");
            } while (bombs.PowerBombExplosion.Phase != PowerBombExplosionPhase.Inactive);
            string actual = $"{test},{left},{samus.Pose:X4},{bombs.PowerBombExplosion.Flag:X4},{samus.InvincibilityTimer:X4},{samus.KnockbackTimer:X4},{samus.Health:X4},{samus.Missiles:X4},{samus.SuperMissiles:X4},{samus.PowerBombs:X4}";
            AssertEqual(rows[row++], actual, $"original-CPU cleanup case {test}, left={left}");
        }
        Console.WriteLine($"Crystal Flash: {cases * 2} original-CPU cleanup admission/resource/timer comparisons match.");
    }
}

/// <summary>One stationary retail Ripper in a constructed population; its AI/header remain ROM-authored.</summary>
internal sealed class CrystalFlashContactPopulation(ISnesAddressSpace inner, ushort x, ushort y) : ISnesAddressSpace
{
    public byte ReadByte(int address)
    {
        ReadOnlySpan<ushort> population = [CrystalFlashContactDefinitions.RipperHeader, x, y, 0,
            CrystalFlashContactDefinitions.Properties, 0, 0, 0, 0xffff, 0];
        ReadOnlySpan<ushort> tileset = [CrystalFlashContactDefinitions.RipperHeader, 0, 0xffff];
        int offset = address - CrystalFlashContactDefinitions.PopulationAddress;
        if ((uint)offset < population.Length * 2)
            return (byte)(population[offset / 2] >> ((offset & 1) * 8));
        offset = address - CrystalFlashContactDefinitions.TilesetAddress;
        if ((uint)offset < tileset.Length * 2)
            return (byte)(tileset[offset / 2] >> ((offset & 1) * 8));
        return inner.ReadByte(address);
    }
    public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
}

/// <summary>Address-space overrides for the controlled Crystal Flash contact fixture.</summary>
internal static class CrystalFlashContactDefinitions
{
    /// <summary>Constructed population/tileset offset in their respective retail banks.</summary>
    public const ushort Pointer = 0x8000;
    /// <summary>Constructed population occupies $A1:8000.</summary>
    public const int PopulationAddress = 0xa18000;
    /// <summary>Constructed tileset occupies $B4:8000.</summary>
    public const int TilesetAddress = 0xb48000;
    /// <summary>Retail Ripper enemy header $A0:D47F.</summary>
    public const ushort RipperHeader = 0xd47f;
    /// <summary>Ripper AI and instruction-list bank $A2.</summary>
    public const int AiBank = 0xa20000;
    /// <summary>Population flags: process instructions and process off screen.</summary>
    public const ushort Properties = 0x2800;
}
