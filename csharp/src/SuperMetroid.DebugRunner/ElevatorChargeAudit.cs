using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>#551: board the real elevator after a charged jump; verify palette cleanup.</summary>
internal static class ElevatorChargeAudit
{
    public static int Run(string rom)
    {
        foreach (ushort suit in new ushort[] { 0, (ushort)SamusEquipmentFlags.VariaSuit, (ushort)SamusEquipmentFlags.GravitySuit })
            VerifyBoarding(rom, suit);
        Console.WriteLine("Three charged-jump upward elevator cases restore the suit palette and clear charge through travel.");
        return 0;
    }

    private static void VerifyBoarding(string rom, ushort suit)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.MorphBallRoom);
        var platform = runtime.Enemies.Slots.Single(s => s.EnemyDefinitionPointer == RoomEnemySystem.ElevatorDefinition);
        var samus = runtime.Samus!;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.InputLocked = false;
        samus.XPosition = platform.XPosition;
        samus.YPosition = (ushort)(platform.YPosition - ElevatorActorDefinitions.SamusYOffset);
        samus.EquippedBeams = samus.CollectedBeams = (ushort)SamusBeamFlags.Charge;
        samus.EquippedItems = samus.CollectedItems = suit;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.PrimeGraphics(bus);
        samus.LoadSuitPalette(bus, runtime.Cgram);
        var normal = new SnesCgram();
        samus.LoadSuitPalette(bus, normal);
        for (int frame = 0; frame < 100; frame++) runtime.StepFrame((ushort)SnesButton.X);
        if (runtime.Projectiles.FlareCounter < 60)
            throw new InvalidDataException("Fixture never acquired full beam charge.");
        ushort startY = samus.YPosition;
        bool rose = false, boarded = false;
        for (int frame = 0; frame < 180; frame++)
        {
            ushort input = (ushort)SnesButton.X;
            if (frame < 20) input |= (ushort)SnesButton.A;
            if (frame > 20 && frame % 2 == 0) input |= (ushort)SnesButton.Up;
            runtime.StepFrame(input);
            rose |= samus.YPosition < startY;
            if (runtime.Enemies.LastElevatorEvent != ElevatorFrameEvent.DepartureStarted) continue;
            boarded = true;
            Console.WriteLine($"Charged jump boarding: suit={suit:X4}, frame={frame}; rose={rose}; flare={runtime.Projectiles.FlareCounter}.");
            var differences = Enumerable.Range(SamusProjectileRomData.Palettes.SamusCgramIndex,
                SamusProjectileRomData.Palettes.ColorCount)
                .Where(i => runtime.Cgram.Colors[i] != normal.Colors[i]).ToArray();
            if (differences.Length != 0)
                throw new InvalidDataException("Elevator retained non-suit palette colors: " + string.Join(',', differences));
            if (runtime.Projectiles.FlareCounter != 0)
                throw new InvalidDataException("Elevator retained beam charge.");
            break;
        }
        if (!rose || !boarded) throw new InvalidDataException("Fixture did not jump and board the real elevator.");
        for (int frame = 0; frame < 20; frame++)
        {
            runtime.StepFrame((ushort)SnesButton.X);
            if (runtime.Projectiles.FlareCounter != 0 ||
                !runtime.Cgram.Colors.Slice(SamusProjectileRomData.Palettes.SamusCgramIndex,
                    SamusProjectileRomData.Palettes.ColorCount).SequenceEqual(
                    normal.Colors.Slice(SamusProjectileRomData.Palettes.SamusCgramIndex,
                        SamusProjectileRomData.Palettes.ColorCount)))
                throw new InvalidDataException($"Charge or stale palette returned on elevator travel frame {frame}.");
            var display = GameplayDisplayCapture.TryCaptureFrame(runtime)
                ?? throw new InvalidDataException("Elevator travel has no display packet.");
            if (!display.Memory.Cgram.Slice(SamusProjectileRomData.Palettes.SamusCgramIndex,
                    SamusProjectileRomData.Palettes.ColorCount).SequenceEqual(
                    normal.Colors.Slice(SamusProjectileRomData.Palettes.SamusCgramIndex,
                        SamusProjectileRomData.Palettes.ColorCount)))
                throw new InvalidDataException($"Displayed elevator palette is stale at travel frame {frame}.");
        }
    }
}
