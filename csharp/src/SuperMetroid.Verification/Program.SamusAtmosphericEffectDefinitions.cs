using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySamusAtmosphericEffectDefinitions(SuperMetroidAddressSpace rom)
    {
        const int waterSplashTypes = 0x9081a4;
        const int runningFootstepFrames = 0x90a424;
        const int crateriaFootstepTypes = 0x90edc9;
        const int crateriaLandingTypes = 0x91f0f3;

        for (int movement = 0; movement <= (byte)SamusMovementType.Special; movement++)
        {
            WaterSplashKind expected = rom.ReadByte(waterSplashTypes + movement) == 0
                ? WaterSplashKind.Diving
                : WaterSplashKind.GroundedPair;
            AssertEqual(expected,
                SamusAtmosphericEffectDefinitions.WaterSplashFor((SamusMovementType)movement),
                $"water-splash movement selector ${movement:X2}");
        }

        for (ushort frame = 0; frame < 10; frame++)
        {
            AssertEqual(rom.ReadByte(runningFootstepFrames + frame) != 0,
                SamusAtmosphericEffectDefinitions.IsRunningFootContact(frame),
                $"running foot-contact frame {frame}");
        }

        for (byte room = 0; room < 16; room++)
        {
            byte footstep = rom.ReadByte(crateriaFootstepTypes + room);
            byte landing = rom.ReadByte(crateriaLandingTypes + room);
            AssertEqual(footstep, landing,
                $"duplicated Crateria atmospheric policy ${room:X2}");
            AssertEqual(footstep,
                (byte)SamusAtmosphericEffectDefinitions.ForCrateriaRoom(room),
                $"compiled Crateria atmospheric policy ${room:X2}");
        }

        var source = new TestAddressSpace();
        const byte testPose = SamusPoseIds.MovingRightNormalPose;
        WriteTestWord(source, 0x91b010 + testPose * 2, 0xc000);
        source.WriteBytes(0x91c000, [1, 1, 1, 1, 1, 1, 1, 1, 1, 1]);
        var guarded = new SamusAtmosphericPolicyReadGuard(source);

        VerifyProductionWaterSplashSelection();
        VerifyProductionRunningContacts(guarded, testPose);
        VerifyProductionCrateriaPolicies(guarded, testPose);

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

    private static void VerifyProductionWaterSplashSelection()
    {
        MethodInfo spawn = typeof(SamusLiquidPhysicsState).GetMethod(
            "SpawnWaterSplash", BindingFlags.Instance | BindingFlags.NonPublic)!;
        for (int movement = 0; movement <= (byte)SamusMovementType.Special; movement++)
        {
            var samus = new SamusState
            {
                XPosition = 100,
                YPosition = 100,
                Kinematics = { XRadius = 5, YRadius = 12 },
            };
            spawn.Invoke(samus.LiquidPhysics,
                [samus, (SamusMovementType)movement, samus.Kinematics.BottomPixel]);

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

    private static void VerifyProductionRunningContacts(
        ISnesAddressSpace guarded,
        byte pose)
    {
        for (ushort frame = 0; frame < 10; frame++)
        {
            var samus = new SamusState
            {
                Pose = pose,
                XPosition = 100,
                YPosition = 100,
                Kinematics = { XRadius = 5, YRadius = 12 },
            };
            samus.LiquidPhysics.RoomIdentity = new RoomIdentity(AreaId.Maridia, 0);
            samus.InitializeAnimation(guarded, frame);
            samus.LiquidPhysics.PrepareAnimationFrame(guarded, samus, nmiFrameCounter: 1);

            byte expectedType = SamusAtmosphericEffectDefinitions.IsRunningFootContact(frame)
                ? (byte)1
                : (byte)0;
            AssertEqual(expectedType,
                samus.LiquidPhysics.AtmosphericEffects.Slots[0].Type,
                $"production running contact frame {frame}");
        }
    }

    private static void VerifyProductionCrateriaPolicies(
        ISnesAddressSpace guarded,
        byte pose)
    {
        for (byte room = 0; room < 16; room++)
        {
            CrateriaAtmosphericEffectFlags policy =
                SamusAtmosphericEffectDefinitions.ForCrateriaRoom(room);
            bool wet = policy != CrateriaAtmosphericEffectFlags.None;

            var runner = new SamusState
            {
                Pose = pose,
                XPosition = 100,
                YPosition = 0x03b0,
                Kinematics = { XRadius = 5, YRadius = 12 },
            };
            runner.LiquidPhysics.RoomIdentity = new RoomIdentity(AreaId.Crateria, room);
            if ((policy & CrateriaAtmosphericEffectFlags.LandingSite) != 0)
                runner.LiquidPhysics.ConfigureNonLiquidRoomFx(RoomFxType.Rain);
            runner.InitializeAnimation(guarded, initialFrame: 2);
            runner.LiquidPhysics.PrepareAnimationFrame(guarded, runner, nmiFrameCounter: 1);
            AssertEqual(wet ? 1 : 0,
                runner.LiquidPhysics.AtmosphericEffects.Slots[0].Type,
                $"production Crateria running policy ${room:X2}");

            var landing = new SamusState
            {
                Pose = pose,
                XPosition = 100,
                YPosition = 0x03b0,
                Kinematics = { XRadius = 5, YRadius = 12 },
            };
            landing.LiquidPhysics.RoomIdentity = new RoomIdentity(AreaId.Crateria, room);
            if ((policy & CrateriaAtmosphericEffectFlags.LandingSite) != 0)
                landing.LiquidPhysics.ConfigureNonLiquidRoomFx(RoomFxType.Rain);
            landing.LiquidPhysics.BeginFrameSoundRequests();
            landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
                guarded,
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

    private sealed class SamusAtmosphericPolicyReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0x9081a4 and < 0x9081c0 or
                >= 0x90a424 and < 0x90a42e or
                >= 0x90edc9 and < 0x90edd9 or
                >= 0x91f0f3 and < 0x91f103
                ? throw new InvalidOperationException(
                    $"Samus atmospheric policy attempted migrated source read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
