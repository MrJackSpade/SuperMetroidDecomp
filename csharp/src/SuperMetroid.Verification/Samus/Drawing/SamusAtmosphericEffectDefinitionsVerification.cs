using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Drawing;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled Samus splash, running foot-contact, and Crateria weather policies against cartridge tables and live owners.</summary>
internal static class SamusAtmosphericEffectDefinitionsVerification
{
    /// <summary>Compares four native selector tables with compiled policies, then exercises splash, running, and landing handoffs with source reads guarded.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus atmospheric effect definitions")]
    internal static void VerifySamusAtmosphericEffectDefinitions(SuperMetroidAddressSpace rom)
    {
        const int waterSplashTypes = 0x9081a4;
        const int runningFootstepFrames = 0x90a424;
        const int crateriaFootstepTypes = 0x90edc9;
        const int crateriaLandingTypes = 0x91f0f3;

        for (int movement = 0; movement <= (byte)SamusMovementType.Special; movement++)
        {
            WaterSplashKind expected = rom.ReadCartridgeByte(waterSplashTypes + movement) == 0
                ? WaterSplashKind.Diving
                : WaterSplashKind.GroundedPair;
            AssertEqual(expected,
                SamusAtmosphericEffectDefinitions.WaterSplashFor((SamusMovementType)movement),
                $"water-splash movement selector ${movement:X2}");
        }

        for (ushort frame = 0; frame < 10; frame++)
        {
            AssertEqual(rom.ReadCartridgeByte(runningFootstepFrames + frame) != 0,
                SamusAtmosphericEffectDefinitions.IsRunningFootContact(frame),
                $"running foot-contact frame {frame}");
        }

        for (byte room = 0; room < 16; room++)
        {
            byte footstep = rom.ReadCartridgeByte(crateriaFootstepTypes + room);
            byte landing = rom.ReadCartridgeByte(crateriaLandingTypes + room);
            AssertEqual(footstep, landing,
                $"duplicated Crateria atmospheric policy ${room:X2}");
            AssertEqual(footstep,
                (byte)SamusAtmosphericEffectDefinitions.ForCrateriaRoom(room),
                $"compiled Crateria atmospheric policy ${room:X2}");
        }

        var source = new TestAddressSpace();
        const SamusPoseId testPose = SamusPoseId.MovingRightNormalPose;
        var guarded = NewSamusAtmosphericPolicyReadGuard(source);

        Suite(nameof(VerifyProductionWaterSplashSelection), () => VerifyProductionWaterSplashSelection());
        Suite(nameof(VerifyProductionRunningContacts), () => VerifyProductionRunningContacts(guarded, testPose));
        Suite(nameof(VerifyProductionCrateriaPolicies), () => VerifyProductionCrateriaPolicies(guarded, testPose));

        AssertThrows<InvalidDataException>(
            () => SamusAtmosphericEffectDefinitions.WaterSplashFor((SamusMovementType)0x1c),
            "water-splash movement selector past table");
        AssertThrows<InvalidDataException>(
            () => SamusAtmosphericEffectDefinitions.IsRunningFootContact(10),
            "running foot-contact frame past table");
        AssertThrows<InvalidDataException>(
            () => SamusAtmosphericEffectDefinitions.ForCrateriaRoom(16),
            "Crateria atmospheric room past table");

        Console.WriteLine(
            "Samus atmospheric definitions: 28 splash selectors, 10 running-contact flags, both 16-byte Crateria policy copies, and all production handoffs pass with source tables forbidden.");
    }

    /// <summary>Invokes the production splash spawner for every movement type and checks grounded-pair versus diving slots.</summary>
    private static void VerifyProductionWaterSplashSelection()
    {
        for (int movement = 0; movement <= (byte)SamusMovementType.Special; movement++)
        {
            var samus = new SamusState
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)),
                Kinematics = { Radius = new(5, 12) },
            };
            samus.LiquidPhysics.SpawnWaterSplash(samus, (SamusMovementType)movement, samus.Kinematics.BottomPixel);

            WaterSplashKind expected =
                SamusAtmosphericEffectDefinitions.WaterSplashFor((SamusMovementType)movement);
            AssertEqual(expected == WaterSplashKind.GroundedPair ? 1 : 3,
                samus.LiquidPhysics.AtmosphericEffects.Slots[0].Type,
                $"production water-splash type ${movement:X2}");
            AssertEqual(expected == WaterSplashKind.GroundedPair ? 1 : 0,
                samus.LiquidPhysics.AtmosphericEffects.Slots[1].Type,
                $"production second water-splash slot ${movement:X2}");
        }
    }

    /// <summary>Advances each running animation frame to its foot-contact tick and checks emitted atmospheric slots.</summary>
    private static void VerifyProductionRunningContacts(
        ISnesAddressSpace guarded,
        SamusPoseId pose)
    {
        for (ushort frame = 0; frame < 10; frame++)
        {
            var samus = new SamusState
            {
                Pose = pose,
                Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)),
                Kinematics = { Radius = new(5, 12) },
            };
            samus.LiquidPhysics.RoomIdentity = new RoomIdentity(AreaId.Maridia, 0);
            samus.Presentation.InitializeAnimation(guarded, frame);
            AdvanceToFootContactTick(samus, guarded);
            samus.LiquidPhysics.PrepareAnimationFrame(samus, nmiFrameCounter: 1);

            byte expectedType = SamusAtmosphericEffectDefinitions.IsRunningFootContact(frame)
                ? (byte)1
                : (byte)0;
            AssertEqual(expectedType,
                samus.LiquidPhysics.AtmosphericEffects.Slots[0].Type,
                $"production running contact frame {frame}");
        }
    }

    /// <summary>Checks all sixteen Crateria room identities through both running and landing effect paths.</summary>
    private static void VerifyProductionCrateriaPolicies(
        ISnesAddressSpace guarded,
        SamusPoseId pose)
    {
        for (byte room = 0; room < 16; room++)
        {
            CrateriaAtmosphericEffectFlags policy =
                SamusAtmosphericEffectDefinitions.ForCrateriaRoom(room);
            bool wet = policy != CrateriaAtmosphericEffectFlags.None;

            var runner = new SamusState
            {
                Pose = pose,
                Position = FixedPosition<RoomPixels>.FromWhole(new(100, 0x03b0)),
                Kinematics = { Radius = new(5, 12) },
            };
            runner.LiquidPhysics.RoomIdentity = new RoomIdentity(AreaId.Crateria, room);
            if ((policy & CrateriaAtmosphericEffectFlags.LandingSite) != 0)
                runner.LiquidPhysics.ConfigureNonLiquidRoomFx(RoomFxType.Rain);
            runner.Presentation.InitializeAnimation(guarded, initialFrame: 2);
            AdvanceToFootContactTick(runner, guarded);
            runner.LiquidPhysics.PrepareAnimationFrame(runner, nmiFrameCounter: 1);
            AssertEqual(wet ? 1 : 0,
                runner.LiquidPhysics.AtmosphericEffects.Slots[0].Type,
                $"production Crateria running policy ${room:X2}");

            var landing = new SamusState
            {
                Pose = pose,
                Position = FixedPosition<RoomPixels>.FromWhole(new(100, 0x03b0)),
                Kinematics = { Radius = new(5, 12) },
            };
            landing.LiquidPhysics.RoomIdentity = new RoomIdentity(AreaId.Crateria, room);
            if ((policy & CrateriaAtmosphericEffectFlags.LandingSite) != 0)
                landing.LiquidPhysics.ConfigureNonLiquidRoomFx(RoomFxType.Rain);
            landing.LiquidPhysics.BeginFrameSoundRequests();
            landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
                landing,
                previousMovementType: SamusMovementType.Falling,
                previousPose: pose,
                impactYSpeed: 1,
                impactYSubspeed: 0);
            AssertEqual(wet ? 1 : 0,
                landing.LiquidPhysics.AtmosphericEffects.Slots[2].Type,
                $"production Crateria landing policy ${room:X2}");
        }
    }

    /// <summary>Lets the compiled animation delay reach its final tick before checking footstep publication.</summary>
    private static void AdvanceToFootContactTick(
        SamusState samus,
        ISnesAddressSpace bus)
    {
        AssertTrue(samus.AnimationFrameTimer.RawValue is > 0 and <= 32,
            "running fixture starts with a bounded native animation delay");
        while (samus.AnimationFrameTimer.RawValue > 1)
            samus.Presentation.AnimateNoFx(bus, new SamusAnimationStep());
    }

    /// <summary>Forbids production reads of migrated splash, foot-contact, and both Crateria policy tables.</summary>
    private static CartridgeReadGuard NewSamusAtmosphericPolicyReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0x9081a4 and < 0x9081c0 or
                >= 0x90a424 and < 0x90a42e or
                >= 0x90edc9 and < 0x90edd9 or
                >= 0x91f0f3 and < 0x91f103,
            address => $"Samus atmospheric policy attempted migrated source read ${address:X6}.");
}
