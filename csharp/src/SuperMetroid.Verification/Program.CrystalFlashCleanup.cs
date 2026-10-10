using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyCrystalFlashRuntime()
    {
        Suite(nameof(VerifyCrystalFlashRuntimeRoute), () => VerifyCrystalFlashRuntimeRoute(capacity: 11, refill: false));
        Suite(nameof(VerifyCrystalFlashRuntimeRoute), () => VerifyCrystalFlashRuntimeRoute(capacity: 10, refill: true));
        Suite(nameof(VerifyCrystalFlashRuntimeRoute), () => VerifyCrystalFlashRuntimeRoute(capacity: 10, refill: false));
    }

    private static void VerifyCrystalFlashRuntimeRoute(ushort capacity, bool refill)
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
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
        // Power Bomb allocation consumes its own HDMA setup pass. Activation is one
        // NMI later than the former eager-setup fixture; refill remains aligned to the
        // global eight-frame cadence, shortening this particular body's lifetime by one.
        AssertEqual(249, visibleBodyFrames, "complete observed Crystal Flash body visibility duration");
        AssertEqual(34, visibleWindowFrames, "native Crystal Flash color window visibility duration");
        for (int frame = 0; frame < 30; frame++) runtime.StepFrame((ushort)SnesButton.Right);
        AssertTrue(samus.XPosition > 512, "normal movement resumes after Crystal Flash");
        Console.WriteLine($"Runtime Crystal Flash: capacity={capacity}, refill={refill}, activation={started}, completion={finished}; placement, bubble, resources and movement ownership pass.");
    }

}

/// <summary>One stationary retail Ripper in a constructed population; its AI/header remain ROM-authored.</summary>
internal sealed class CrystalFlashContactPopulation(ISnesAddressSpace inner, ushort x, ushort y) :
    ISnesAddressSpace, IRoomEnemyFixtureSource
{
    public RoomEnemyDefinition ReadEnemyDefinition(EnemyDefinitionId pointer) =>
        RoomEnemyDefinitionCatalog.Get(pointer);

    public RoomEnemyPopulationDefinition ReadEnemyPopulation(ushort pointer)
    {
        if (pointer != CrystalFlashContactDefinitions.Pointer)
            throw new ArgumentOutOfRangeException(nameof(pointer));
        return new RoomEnemyPopulationDefinition(pointer,
        [
            new RoomEnemyPopulationRecord(
                CrystalFlashContactDefinitions.RipperHeader, x, y, 0,
                CrystalFlashContactDefinitions.Properties, 0, 0, 0),
        ], 0);
    }

    public RoomEnemyGraphicsSetDefinition ReadEnemyGraphicsSet(ushort pointer)
    {
        if (pointer != CrystalFlashContactDefinitions.Pointer)
            throw new ArgumentOutOfRangeException(nameof(pointer));
        return new RoomEnemyGraphicsSetDefinition(pointer,
        [
            new RoomEnemyGraphicsSetHeader(CrystalFlashContactDefinitions.RipperHeader, 0),
        ]);
    }

    public byte ReadByte(int address)
    {
        ReadOnlySpan<ushort> population = [(ushort)CrystalFlashContactDefinitions.RipperHeader, x, y, 0,
            CrystalFlashContactDefinitions.Properties, 0, 0, 0, 0xffff, 0];
        ReadOnlySpan<ushort> tileset = [(ushort)CrystalFlashContactDefinitions.RipperHeader, 0, 0xffff];
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
    public const EnemyDefinitionId RipperHeader = EnemyDefinitionId.Ripper;
    /// <summary>Ripper AI and instruction-list bank $A2.</summary>
    public const int AiBank = 0xa20000;
    /// <summary>Population flags: process instructions and process off screen.</summary>
    public const ushort Properties = 0x2800;
}
