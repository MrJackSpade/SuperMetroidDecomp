using SuperMetroid.Core.Assets;
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
        Suite(nameof(VerifyProductionAtmosphericCadence), () => VerifyProductionAtmosphericCadence(guarded));
        Suite(nameof(VerifyNullAtmosphericPointerReadsMirroredWorkRam), () => VerifyNullAtmosphericPointerReadsMirroredWorkRam(rom, guarded));
        Suite(nameof(VerifyProductionLiquidDamage), () => VerifyProductionLiquidDamage(guarded, RoomFxType.Lava,
            SamusLiquidDamageDefinitions.Lava));
        Suite(nameof(VerifyProductionLiquidDamage), () => VerifyProductionLiquidDamage(guarded, RoomFxType.Acid,
            SamusLiquidDamageDefinitions.Acid));

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
        var atmosphericArt = new SamusAtmosphericArtworkCatalog(
            new ushort[SamusMovementRomData.Environment.DirectAtmosphericFrameCount],
            new ushort[SamusMovementRomData.Environment.DirectAtmosphericFrameCount]);
        for (byte type = 1; type <= 7; type++)
        {
            // This fixture measures timer ownership. Keep the Samus-spritemap
            // types below the native Y clip so their unrelated body-art catalog
            // is not needed; direct small sprites still exercise installed art.
            ushort drawY = type is 3 or 5 ? (ushort)0x0100 : (ushort)100;
            byte frameCount = SamusAtmosphericAnimationDefinitions.FrameCount(type);
            for (byte frame = 0; frame < frameCount; frame++)
            {
                ushort expectedTimer =
                    SamusAtmosphericAnimationDefinitions.FrameTimer(type, frame);

                var effects = new SamusAtmosphericEffectsState();
                effects.SetSlot(0, type, frame, animationTimer: 1, worldX: 100, worldY: drawY);
                var oam = new OamBuffer();
                oam.BeginFrame();
                effects.UpdateAndDraw(guarded, oam, 0, 0, fxYPosition: drawY,
                    directArtwork: atmosphericArt);
                AssertEqual(expectedTimer, effects.Slots[0].AnimationTimer,
                    $"production atmospheric type {type} frame {frame} expiry timer");
                AssertEqual(frame + 1 == frameCount ? 0 : frame + 1,
                    effects.Slots[0].AnimationFrame,
                    $"production atmospheric type {type} frame {frame} expiry handoff");

                effects.Clear();
                effects.SetSlot(0, type, frame, animationTimer: 0x8001,
                    worldX: 100, worldY: drawY);
                oam.BeginFrame();
                effects.UpdateAndDraw(guarded, oam, 0, 0, fxYPosition: drawY,
                    directArtwork: atmosphericArt);
                AssertEqual(expectedTimer, effects.Slots[0].AnimationTimer,
                    $"production atmospheric type {type} frame {frame} delayed timer");
                AssertEqual(frame, effects.Slots[0].AnimationFrame,
                    $"production atmospheric type {type} frame {frame} delayed handoff");
            }
        }
    }

    private static void VerifyNullAtmosphericPointerReadsMirroredWorkRam(
        SuperMetroidAddressSpace rom,
        ISnesAddressSpace guarded)
    {
        byte originalLow = rom.ReadWorkRamByte(0x900000);
        byte originalHigh = rom.ReadWorkRamByte(0x900001);
        try
        {
            rom.WriteByte(0x900000, 0x5a);
            rom.WriteByte(0x900001, 0x34);
            var effects = new SamusAtmosphericEffectsState();
            effects.SetSlot(0, 2, 0, animationTimer: 2,
                worldX: 100, worldY: 100);
            var oam = new OamBuffer();
            oam.BeginFrame();
            effects.UpdateAndDraw(guarded, oam, 0, 0, fxYPosition: 100);
            AssertEqual((byte)0x5a, oam.LowTable[2],
                "null atmospheric pointer reads low OBJ attribute from mirrored WRAM");
            AssertEqual((byte)0x34, oam.LowTable[3],
                "null atmospheric pointer reads high OBJ attribute from mirrored WRAM");
        }
        finally
        {
            rom.WriteByte(0x900000, originalLow);
            rom.WriteByte(0x900001, originalHigh);
        }
    }

    private static void VerifyProductionLiquidDamage(
        ISnesAddressSpace guarded,
        RoomFxType fxType,
        SamusLiquidDamageRate expected)
    {
        var samus = new SamusState
        {
            Pose = SamusPoseId.FacingRightNormalPose,
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
        ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadWorkRamByte(int address) =>
            ((ISnesMutableMemory)source).ReadWorkRamByte(address);

        public byte ReadSaveRamByte(int address) =>
            ((ISnesMutableMemory)source).ReadSaveRamByte(address);

        public byte ReadByte(int address) =>
            address is >= 0x908b93 and < 0x908bff or >= 0x909e8b and < 0x909e93
                ? throw new InvalidOperationException(
                    $"Samus atmosphere attempted migrated mechanics read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
