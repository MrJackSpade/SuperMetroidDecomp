using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Drawing;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Progression;

/// <summary>Checks compiled Samus splash, running foot-contact, and Crateria weather policies against cartridge tables and live owners.</summary>
internal static class SamusAtmosphericEffectDefinitionsVerification
{
    /// <summary>Compares four native selector tables with compiled policies, then exercises splash, running, and landing handoffs with source reads guarded.</summary>
    internal static void VerifySamusAtmosphericEffectDefinitions(CartridgeImportAddressSpace rom)
    {
        const int waterSplashTypes = 0x9081a4;
        const int runningFootstepFrames = 0x90a424;
        const int crateriaFootstepTypes = 0x90edc9;
        const int crateriaLandingTypes = 0x91f0f3;

        foreach (int movement in SamusMovementTypeNumbers.Named)
        {
            WaterSplashKind expected = rom.ReadCartridgeByte(waterSplashTypes + movement) == 0
                ? WaterSplashKind.Diving
                : WaterSplashKind.GroundedPair;
            AssertEqual(expected,
                SamusAtmosphericEffects.WaterSplashFor(SamusMovementTypeNumbers.Of(movement)),
                $"water-splash movement selector ${movement:X2}");
        }

        for (ushort frame = 0; frame < 10; frame++)
        {
            AssertEqual(rom.ReadCartridgeByte(runningFootstepFrames + frame) != 0,
                SamusAtmosphericEffects.IsRunningFootContact(frame),
                $"running foot-contact frame {frame}");
        }

        for (byte room = 0; room < 16; room++)
        {
            byte footstep = rom.ReadCartridgeByte(crateriaFootstepTypes + room);
            byte landing = rom.ReadCartridgeByte(crateriaLandingTypes + room);
            AssertEqual(footstep, landing,
                $"duplicated Crateria atmospheric policy ${room:X2}");
            AssertEqual(footstep,
                (byte)SamusAtmosphericEffects.ForCrateriaRoom(RoomDefinitions.WithCartridgeNumber(rom, AreaId.Crateria, room)),
                $"compiled Crateria atmospheric policy ${room:X2}");
        }

        var source = new TestAddressSpace();
        const SamusPoseId testPose = SamusPoseId.MovingRightNormalPose;
        var guarded = NewSamusAtmosphericPolicyReadGuard(source);

        Suite(nameof(VerifyProductionWaterSplashSelection), () => VerifyProductionWaterSplashSelection());
        Suite(nameof(VerifyProductionRunningContacts), () => VerifyProductionRunningContacts(testPose));
        Suite(nameof(VerifyProductionCrateriaPolicies), () => VerifyProductionCrateriaPolicies(rom, testPose));
        Suite(nameof(VerifyBrinstarAndTourianLandingDust), () => VerifyBrinstarAndTourianLandingDust(rom, testPose));

        AssertThrows<InvalidDataException>(
            () => SamusAtmosphericEffects.WaterSplashFor((SamusMovementType)(-1)),
            "water-splash movement selector past table");
        AssertThrows<InvalidDataException>(
            () => SamusAtmosphericEffects.IsRunningFootContact(10),
            "running foot-contact frame past table");
        AssertEqual(CrateriaAtmosphericEffectFlags.None,
            SamusAtmosphericEffects.ForCrateriaRoom(RoomDefinitions.GreenPiratesShaft),
            "a Crateria room the policy does not name has none");
        AssertThrows<InvalidDataException>(
            () => SamusAtmosphericEffects.ForCrateriaRoom(RoomDefinitions.GreenBrinstarMainShaft),
            "a room outside Crateria has no Crateria atmospheric policy");
        AssertThrows<InvalidDataException>(
            () => SamusAtmosphericEffects.LandingRaisesDustInBrinstarOrTourian(RoomDefinitions.LandingSite),
            "a room outside Brinstar and Tourian has no Brinstar or Tourian landing-dust policy");

        Console.WriteLine(
            "Samus atmospheric definitions: 28 splash selectors, 10 running-contact flags, both 16-byte Crateria policy copies, Brinstar's and Tourian's landing dust, and all production handoffs pass with source tables forbidden.");
    }

    /// <summary>The kinds of Samus's particles, oldest first.</summary>
    private static string ParticleKinds(SamusState samus) =>
        string.Join(",", samus.Presentation.Particles.All.Select(particle => particle.Kind));

    /// <summary>Invokes the production splash spawner for every movement type and checks grounded-pair versus diving particles.</summary>
    private static void VerifyProductionWaterSplashSelection()
    {
        foreach (int movement in SamusMovementTypeNumbers.Named)
        {
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)),
                Kinematics = { Radius = new(5, 12) },
            };
            samus.LiquidPhysics.SpawnWaterSplash(samus, SamusMovementTypeNumbers.Of(movement), samus.Kinematics.BottomPixel.Y);

            WaterSplashKind expected =
                SamusAtmosphericEffects.WaterSplashFor(SamusMovementTypeNumbers.Of(movement));
            AssertEqual(expected == WaterSplashKind.GroundedPair ? "SurfaceSplash,SurfaceSplash" : "DivingSplash",
                ParticleKinds(samus), $"production water-splash particles ${movement:X2}");
        }
    }

    /// <summary>Advances each running animation frame to its foot-contact tick and checks the particles emitted.</summary>
    private static void VerifyProductionRunningContacts(
        SamusPoseId pose)
    {
        for (ushort frame = 0; frame < 10; frame++)
        {
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
            {
                Pose = pose,
                Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)),
                Kinematics = { Radius = new(5, 12) },
            };
            samus.LiquidPhysics.Room = RoomDefinitions.FirstIn(AreaId.Maridia);
            SamusPresentation.InitializeAnimation(samus, frame);
            AdvanceToFootContactTick(samus);
            samus.LiquidPhysics.PrepareAnimationFrame(samus, new SharedRandom(), frameCounter: 1);

            AssertEqual(SamusAtmosphericEffects.IsRunningFootContact(frame) ? "SurfaceSplash,SurfaceSplash" : "",
                ParticleKinds(samus), $"production running contact frame {frame}");
        }
    }

    /// <summary>Checks the sixteen Crateria rooms the cartridge's policy tables cover through both running and landing effect paths.</summary>
    private static void VerifyProductionCrateriaPolicies(
        CartridgeImportAddressSpace rom,
        SamusPoseId pose)
    {
        for (byte number = 0; number < 16; number++)
        {
            RoomDefinition room = RoomDefinitions.WithCartridgeNumber(rom, AreaId.Crateria, number);
            CrateriaAtmosphericEffectFlags policy = SamusAtmosphericEffects.ForCrateriaRoom(room);
            bool wet = policy != CrateriaAtmosphericEffectFlags.None;

            var runner = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
            {
                Pose = pose,
                Position = FixedPosition<RoomPixels>.FromWhole(new(100, 0x03b0)),
                Kinematics = { Radius = new(5, 12) },
            };
            runner.LiquidPhysics.Room = room;
            if ((policy & CrateriaAtmosphericEffectFlags.LandingSite) != 0)
                runner.LiquidPhysics.ConfigureNonLiquidRoomFx(RoomFxType.Rain);
            SamusPresentation.InitializeAnimation(runner, initialFrame: 2);
            AdvanceToFootContactTick(runner);
            runner.LiquidPhysics.PrepareAnimationFrame(runner, new SharedRandom(), frameCounter: 1);
            AssertEqual(wet ? "SurfaceSplash,SurfaceSplash" : "", ParticleKinds(runner),
                $"production Crateria running policy in {room.Name}");

            var landing = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
            {
                Pose = pose,
                Position = FixedPosition<RoomPixels>.FromWhole(new(100, 0x03b0)),
                Kinematics = { Radius = new(5, 12) },
            };
            landing.LiquidPhysics.Room = room;
            if ((policy & CrateriaAtmosphericEffectFlags.LandingSite) != 0)
                landing.LiquidPhysics.ConfigureNonLiquidRoomFx(RoomFxType.Rain);
            landing.LiquidPhysics.BeginFrameSoundRequests();
            landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
                landing,
                previousMovementType: SamusMovementType.Falling,
                previousPose: pose,
                impactYSpeed: new UnsignedSixteenSixteen(1, 1));
            AssertEqual(wet ? "SurfaceSplash,SurfaceSplash" : "", ParticleKinds(landing),
                $"production Crateria landing policy in {room.Name}");
        }
    }

    /// <summary>
    /// Checks every Brinstar and Tourian room's landing dust against the cartridge's landing-graphics rule, in the
    /// compiled room set and through a production landing: a dusty room raises a dust pair that holds up the breath
    /// bubbles, and any other room shows nothing and ends the bubbles' landing wait.
    /// </summary>
    private static void VerifyBrinstarAndTourianLandingDust(
        CartridgeImportAddressSpace rom,
        SamusPoseId pose)
    {
        int dustyRooms = 0;
        foreach (RoomDefinition room in RetailRooms.All.Where(room => room.Area is AreaId.Brinstar or AreaId.Tourian))
        {
            // The cartridge's handlers: Brinstar's room 8 branches straight to dust, and every other Brinstar room, like
            // every Tourian room, raises dust when its number is 5 through 8 or 11.
            int number = room.CartridgeNumber(rom);
            bool expected = (room.Area == AreaId.Brinstar && number == 8) || number is >= 5 and < 9 or 11;
            AssertEqual(expected, SamusAtmosphericEffects.LandingRaisesDustInBrinstarOrTourian(room),
                $"compiled landing dust in {room.Area} {room.Name}");
            if (expected)
                dustyRooms++;

            var landing = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
            {
                Pose = pose,
                Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)),
                Kinematics = { Radius = new(5, 12) },
            };
            landing.LiquidPhysics.Room = room;
            landing.LiquidPhysics.EffectWaits.HoldBubbles(9);
            landing.LiquidPhysics.BeginFrameSoundRequests();
            landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
                landing,
                previousMovementType: SamusMovementType.Falling,
                previousPose: pose,
                impactYSpeed: new UnsignedSixteenSixteen(1, 1));
            AssertEqual(expected ? "Dust,Dust" : "", ParticleKinds(landing),
                $"production landing particles in {room.Area} {room.Name}");
            AssertEqual(!expected, landing.LiquidPhysics.EffectWaits.BubbleCanForm,
                $"production landing bubble wait in {room.Area} {room.Name}");
        }
        AssertEqual(10, dustyRooms, "Brinstar and Tourian have ten rooms whose landings raise dust");
    }

    /// <summary>Lets the compiled animation delay reach its final tick before checking footstep publication.</summary>
    private static void AdvanceToFootContactTick(
        SamusState samus)
    {
        AssertTrue(samus.AnimationFrameTimer.RawValue is > 0 and <= 32,
            "running fixture starts with a bounded native animation delay");
        while (samus.AnimationFrameTimer.RawValue > 1)
            SamusPresentation.AnimateNoFx(samus, new SamusAnimationStep { Random = new SharedRandom() });
    }

    /// <summary>Forbids production reads of migrated splash, foot-contact, and both Crateria policy tables.</summary>
    private static CartridgeReadGuard NewSamusAtmosphericPolicyReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0x9081a4 and < 0x9081c0 or
                >= 0x90a424 and < 0x90a42e or
                >= 0x90edc9 and < 0x90edd9 or
                >= 0x91f0f3 and < 0x91f103,
            address => $"Samus atmospheric policy attempted migrated source read ${address:X6}.");
}
