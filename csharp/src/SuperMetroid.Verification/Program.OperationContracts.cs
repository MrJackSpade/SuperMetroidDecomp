using System.Reflection;
using System.Runtime.CompilerServices;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// #633: operation contexts are supersets of their children's narrow contracts, child operations accept
    /// only those contracts, the grapple wrapper's masked frame withholds exactly four capabilities, and
    /// concrete contexts reject missing required values at construction instead of during the operation.
    /// </summary>
    private static void VerifyOperationContracts()
    {
        const BindingFlags instance = BindingFlags.Instance | BindingFlags.NonPublic;
        const BindingFlags shared = BindingFlags.Static | BindingFlags.NonPublic;

        // The enemy frame is the superset main AI's narrow frame describes.
        AssertTrue(typeof(IEnemyAiFrame).IsAssignableFrom(typeof(RoomEnemyFrame)), "the enemy frame satisfies main AI's frame");
        AssertTrue(typeof(IEnemyAiFrame).GetProperties().Select(property => property.Name).Order(StringComparer.Ordinal).SequenceEqual(
            ["Camera", "ControllerInput", "Level", "Mode7Transform", "NewlyPressedControllerInput", "Samus", "SamusProjectiles",
             "SharedProjectiles", "VramWriteQueue"]),
            "main AI's frame exposes only the nine values main AI reads");
        AssertEqual(typeof(IEnemyAiFrame), typeof(RoomEnemySystem).GetMethod("RunMainAi", instance)!.GetParameters()[1].ParameterType,
            "main AI accepts its narrow frame, not the enemy pass's whole frame");
        AssertEqual(typeof(IEnemyAiFrame), typeof(RoomEnemySystem).GetMethod("RunCommonGrappleAi", instance)!.GetParameters()[1].ParameterType,
            "the common grapple wrapper accepts main AI's frame");

        // The grapple wrapper hands main AI the enemy's frame without projectiles, Mode 7, bombs or a VRAM queue.
        var samus = new SamusState();
        var frame = new RoomEnemyFrame
        {
            Camera = new(0x0120, 0x0040),
            TimeIsFrozen = false,
            Samus = samus,
            NewlyPressedControllerInput = 0x0080,
            ControllerInput = 0x0100,
            SamusProjectiles = new SamusProjectileSystem(),
            Mode7Transform = new SamusMode7Transform(240, 16, 65520, new(128, 112)),
            SharedProjectiles = new SamusBombProjectileSystem(),
            VramWriteQueue = new VramWriteQueue(),
        };
        Type wrapperType = typeof(RoomEnemySystem).GetNestedType("GrappleWrappedAiFrame", BindingFlags.NonPublic)!;
        var masked = (IEnemyAiFrame)Activator.CreateInstance(wrapperType, BindingFlags.Instance | BindingFlags.Public, null, [frame], null)!;
        AssertTrue(masked.Camera == frame.Camera && ReferenceEquals(masked.Samus, samus) &&
            masked.NewlyPressedControllerInput == frame.NewlyPressedControllerInput && masked.ControllerInput == frame.ControllerInput &&
            ReferenceEquals(masked.Level, frame.Level), "the grapple wrapper passes the enemy's camera, Samus, input and room");
        AssertTrue(masked.SamusProjectiles is null && masked.Mode7Transform is null && masked.SharedProjectiles is null &&
            masked.VramWriteQueue is null, "the grapple wrapper withholds projectiles, Mode 7, bombs and the VRAM queue");

        // Children that read a subset take that subset, not the parent's record.
        AssertEqual(typeof(PaletteFxConditions),
            typeof(RoomPaletteFxSystem).GetMethod("RunPreInstruction", instance)!.GetParameters()[2].ParameterType,
            "palette pre-instructions take only their conditions; the power bomb stays with the step");
        AssertEqual(typeof(VerticalTerrainProbe),
            typeof(SamusBlockCollision).GetMethod("ScanVerticalTerrain", shared)!.GetParameters()[2].ParameterType,
            "the vertical terrain scan takes its probe, not the whole vertical move");
        AssertEqual(typeof(IAreaBossFlagReader),
            typeof(RoomPlmSystem).GetMethod("TryRunRoomPopulationSetup", instance)!.GetParameters()[1].ParameterType,
            "PLM setup reads boss flags without being able to set them");

        // Concrete contexts validate required values at construction.
        AssertThrows<ArgumentNullException>(() => RuntimeFrameAudio.Gameplay(null!, () => { }), "gameplay audio requires its echo queue");
        AssertThrows<ArgumentNullException>(() => RuntimeFrameAudio.Gameplay(() => 0, null!), "gameplay audio requires its low-health check");
        AssertThrows<ArgumentNullException>(() => RuntimeFrameAudio.EchoOnly(null!), "attract audio requires its echo queue");
        AssertThrows<ArgumentNullException>(() => _ = new DemoInputListProgram(null!), "a list-only demo owner requires its list");
        var bus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var level = new RoomLevelData(new(1, 1), new RoomLevelPlanes(new ushort[1], new byte[1], new ushort[1]), new byte[8]);
        var streamer = level.CreateBackgroundStreamer();
        AssertThrows<ArgumentNullException>(() => _ = new PlmRoom(null!, level, streamer, new SnesVram(), new Bank80SystemState(), AreaId.Crateria),
            "a PLM room requires its bus");
        AssertThrows<ArgumentNullException>(() => _ = new PlmRoom(bus, level, streamer, null!, new Bank80SystemState(), AreaId.Crateria),
            "a PLM room requires its VRAM");
        var services = (RoomPlmServices)RuntimeHelpers.GetUninitializedObject(typeof(RoomPlmServices));
        AssertThrows<ArgumentNullException>(() => new RoomPlmSystem().LoadRoomPopulation(
                new PlmRoom(bus, level, streamer, new SnesVram(), new Bank80SystemState(), AreaId.Crateria),
                new RoomPlmPopulationDefinition(0, []), services),
            "a PLM load rejects services missing their required Samus access");

        // Explicit optional scenarios: the ordinary frame and retail overrides are named values.
        var ordinary = new RuntimeFrameRequest();
        AssertTrue(ordinary.Policy.AllowCeresElevatorDeparture && ordinary.Policy.AdvanceGameTime &&
            ReferenceEquals(ordinary.Audio, RuntimeFrameAudio.Deferred), "a default frame request is state-eight gameplay with deferred audio");
        var tester = new SuperMetroidGameOptions { Invincibility = true, InfiniteAmmo = true, GrantAllEquipment = true };
        AssertTrue(ReferenceEquals(HostGameplayOverrides.None, tester.RuntimeOverrides(forAttractDemo: true)),
            "attract playback runs retail behaviour whatever the host configured");
        AssertTrue(tester.RuntimeOverrides(forAttractDemo: false) is { PlayerInvincibilityEnabled: true, InfiniteAmmoEnabled: true, GrantAllEquipment: true },
            "a session receives the host's configured overrides");
        Console.WriteLine("#633 operation contracts: narrow child contracts, the masked grapple frame and construction-time validation pass.");
    }
}
