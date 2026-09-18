using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySamusAtmosphericAnimationDefinitions(
        SuperMetroidAddressSpace rom)
    {
        static ushort Word(ISnesAddressSpace bus, int address) =>
            unchecked((ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8));

        AssertEqual(0, Word(rom, 0x908b93), "inactive atmospheric timer pointer");
        AssertEqual(0, Word(rom, 0x908ba3), "trailing atmospheric timer pointer");
        AssertEqual(0, Word(rom, 0x908bef), "inactive atmospheric frame count");

        for (byte type = 1; type <= 7; type++)
        {
            ushort pointer = Word(rom, 0x908b93 + type * 2);
            byte frameCount = checked((byte)Word(rom, 0x908bef + type * 2));
            AssertEqual(frameCount,
                SamusAtmosphericAnimationDefinitions.FrameCount(type),
                $"atmospheric type {type} frame count");

            for (byte frame = 0; frame < frameCount; frame++)
            {
                AssertEqual(Word(rom, 0x900000 | (pointer + frame * 2)),
                    SamusAtmosphericAnimationDefinitions.FrameTimer(type, frame),
                    $"atmospheric type {type} frame {frame} timer");
            }
        }

        AssertEqual(Word(rom, 0x909e8b), SamusLiquidDamageDefinitions.Lava.SubDamage,
            "lava fractional damage rate");
        AssertEqual(Word(rom, 0x909e8d), SamusLiquidDamageDefinitions.Lava.WholeDamage,
            "lava whole damage rate");
        AssertEqual(Word(rom, 0x909e8f), SamusLiquidDamageDefinitions.Acid.SubDamage,
            "acid fractional damage rate");
        AssertEqual(Word(rom, 0x909e91), SamusLiquidDamageDefinitions.Acid.WholeDamage,
            "acid whole damage rate");

        var guarded = new SamusAtmosphericAnimationReadGuard(rom);
        VerifyProductionAtmosphericCadence(guarded);
        VerifyProductionLiquidDamage(guarded, RoomFxType.Lava,
            SamusLiquidDamageDefinitions.Lava);
        VerifyProductionLiquidDamage(guarded, RoomFxType.Acid,
            SamusLiquidDamageDefinitions.Acid);

        AssertThrows<InvalidDataException>(
            () => SamusAtmosphericAnimationDefinitions.FrameCount(0),
            "inactive atmospheric type has no animation definition");
        AssertThrows<InvalidDataException>(
            () => SamusAtmosphericAnimationDefinitions.FrameCount(8),
            "atmospheric type past active domain");
        AssertThrows<InvalidDataException>(
            () => SamusAtmosphericAnimationDefinitions.FrameTimer(1, 4),
            "atmospheric frame past authored type-one cadence");

        Console.WriteLine(
            "Samus atmospheric animation: 37 timers, seven frame counts, four liquid-damage words, and all real cadence/damage consumers pass with mechanics ranges forbidden.");
    }

    private static void VerifyProductionAtmosphericCadence(ISnesAddressSpace guarded)
    {
        for (byte type = 1; type <= 7; type++)
        {
            byte frameCount = SamusAtmosphericAnimationDefinitions.FrameCount(type);
            for (byte frame = 0; frame < frameCount; frame++)
            {
                ushort expectedTimer =
                    SamusAtmosphericAnimationDefinitions.FrameTimer(type, frame);

                var effects = new SamusAtmosphericEffectsState();
                effects.SetSlot(0, type, frame, animationTimer: 1, worldX: 100, worldY: 100);
                var oam = new OamBuffer();
                oam.BeginFrame();
                effects.UpdateAndDraw(guarded, oam, 0, 0, fxYPosition: 100);
                AssertEqual(expectedTimer, effects.Slots[0].AnimationTimer,
                    $"production atmospheric type {type} frame {frame} expiry timer");
                AssertEqual(frame + 1 == frameCount ? 0 : frame + 1,
                    effects.Slots[0].AnimationFrame,
                    $"production atmospheric type {type} frame {frame} expiry handoff");

                effects.Clear();
                effects.SetSlot(0, type, frame, animationTimer: 0x8001,
                    worldX: 100, worldY: 100);
                oam.BeginFrame();
                effects.UpdateAndDraw(guarded, oam, 0, 0, fxYPosition: 100);
                AssertEqual(expectedTimer, effects.Slots[0].AnimationTimer,
                    $"production atmospheric type {type} frame {frame} delayed timer");
                AssertEqual(frame, effects.Slots[0].AnimationFrame,
                    $"production atmospheric type {type} frame {frame} delayed handoff");
            }
        }
    }

    private static void VerifyProductionLiquidDamage(
        ISnesAddressSpace guarded,
        RoomFxType fxType,
        SamusLiquidDamageRate expected)
    {
        var samus = new SamusState
        {
            Pose = SamusPoseIds.FacingRightNormalPose,
            Health = 99,
            XPosition = 100,
            YPosition = 100,
            Kinematics = { XRadius = 5, YRadius = 12 },
        };
        samus.LiquidPhysics.ConfigureLavaAcid(
            surfaceY: 100,
            acid: fxType == RoomFxType.Acid);
        samus.LiquidPhysics.PrepareAnimationFrame(guarded, samus, nmiFrameCounter: 1);
        AssertEqual(expected.SubDamage, samus.LiquidPhysics.PeriodicSubDamage,
            $"production {fxType} fractional damage accumulation");
        AssertEqual(expected.WholeDamage, samus.LiquidPhysics.PeriodicDamage,
            $"production {fxType} whole damage accumulation");
    }

    private sealed class SamusAtmosphericAnimationReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0x908b93 and < 0x908bff or >= 0x909e8b and < 0x909e93
                ? throw new InvalidOperationException(
                    $"Samus atmosphere attempted migrated mechanics read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
