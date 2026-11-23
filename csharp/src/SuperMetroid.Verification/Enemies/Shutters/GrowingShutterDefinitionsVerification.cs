using SuperMetroid.Core;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Shutters;

/// <summary>
/// Checks the growing shutter's start selection and growth speeds against bank $A2 through the population codec and the
/// actual enemy initializer (#1519): the codec decodes each placement's growth speed, and the initializer starts the
/// wait the placement names.
/// </summary>
internal static class GrowingShutterDefinitionsVerification
{
    /// <summary>The number of growth speeds a placement can select.</summary>
    private const int GrowthSpeedCount = 24;

    /// <summary>
    /// Compares native words and all valid initializer combinations, and the section origins of a shutter spawned inside
    /// the room, as every placement is (#2449).
    /// </summary>
    internal static void VerifyCompiledGrowingShutters(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyGrowingShutterWholeSpeed), () => VerifyGrowingShutterWholeSpeed(rom));
        Suite(nameof(VerifyGrowingShutterFractionalSpeed), () => VerifyGrowingShutterFractionalSpeed(rom));
        ushort Word(int address) => LittleEndianBytes.Word(CompiledGrowingShuttersRomByte(rom, address), CompiledGrowingShuttersRomByte(rom, address + 1));
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        Func<RoomEnemyPlace, GrowingShutterEnemyState> initialize = fixturePlace => ShuttersBehavior.InitializeGrowingShutter(fixturePlace.HeldOccupant().SpawnSettings<GrowingShutterSpawnSettings>(), fixturePlace);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.ShutterGrowing, place.KeptDefinition);
        for (ushort selector = 0; selector < 4; selector++)
        {
            for (int high = 0; high < 256; high++)
            {
                for (int speed = 0; speed < GrowthSpeedCount; speed++)
                {
                    // The native selector's high bit is the extra-property word that names the growth direction, its low
                    // bit the initialization word that names the wait.
                    place.SetSpawnWords(EnemyDefinitionId.ShutterGrowing, _ => new SuperMetroid.AssetExtraction.Enemies.PopulationWords(
                        selector & 1, 0, selector >> 1, 0, (high << 8) | speed));
                    place.Position = place.Position.WithWholeY(296);
                    // The shutter's setup builds its state.
                    GrowingShutterEnemyState state = initialize(place);
                    AssertEqual(Word(0xa2ea4e + selector * 2), ShuttersNativeWords.GrowingShutterActions.Word((state.Action, state.Growth)), "Shutter real dispatch without bus");
                    AssertEqual(unchecked((short)Word(0xa2ea56 + speed * 4)), place.HeldOccupant().SpawnSettings<GrowingShutterSpawnSettings>().GrowthSpeed.WholePixels, "Shutter native whole speed");
                    AssertEqual(Word(0xa2ea58 + speed * 4), place.HeldOccupant().SpawnSettings<GrowingShutterSpawnSettings>().GrowthSpeed.Fraction, "Shutter native fractional speed");
                    int direction = selector < 2 ? 1 : -1;
                    for (ListStep<GrowingShutterSection>? section = GrowingShutterEnemyState.FirstSection; section is { } step; section = step.Following)
                        AssertEqual(place.Position.Y.Whole + direction * 8 * step.Entry.GrownBefore, state.OriginY(step), $"Shutter section origin {step.Entry.GrownBefore}");
                    AssertTrue(!place.CollidesByFrameHitboxes && !place.AnimationClock.ShowsNewFrame, "Shutter clears its additional properties");
                }
            }
        }
        Console.WriteLine("Growing shutter definitions: 52 native words, 24576 real initializers and section origins match without a bus.");
    }

    /// <summary>Reads cartridge bytes used as the compiled shutter comparison oracle.</summary>
    private static byte CompiledGrowingShuttersRomByte(CartridgeImportAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks signed whole-pixel shutter velocities across high-byte selector aliases.</summary>
    private static void VerifyGrowingShutterWholeSpeed(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyGrowingShutterSpeedField), () => VerifyGrowingShutterSpeedField(rom, fractional: false));

    /// <summary>Checks unsigned fractional shutter velocities across high-byte selector aliases.</summary>
    private static void VerifyGrowingShutterFractionalSpeed(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyGrowingShutterSpeedField), () => VerifyGrowingShutterSpeedField(rom, fractional: true));

    /// <summary>
    /// Compares one native speed word with the speed the codec decodes for each selectable growth speed, under every
    /// high byte, and confirms a placement selecting past the authored speeds fails to decode.
    /// </summary>
    private static void VerifyGrowingShutterSpeedField(CartridgeImportAddressSpace rom, bool fractional)
    {
        for (int index = 0; index < GrowthSpeedCount; index++)
        {
            int address = 0xa2ea56 + 4 * index + (fractional ? 2 : 0);
            ushort native = LittleEndianBytes.Word(GrowingShutterSpeedFieldRomByte(rom, address), GrowingShutterSpeedFieldRomByte(rom, address + 1));
            for (int high = 0; high < 256; high++)
            {
                SignedSixteenSixteen speed = DecodedGrowthSpeed((high << 8) | index);
                if (fractional)
                    AssertEqual(native, speed.Fraction, "growing shutter native unsigned fraction and high-byte aliases");
                else
                    AssertEqual(unchecked((short)native), speed.WholePixels, "growing shutter native signed whole word and high-byte aliases");
            }
        }
        for (int high = 0; high < 256; high++)
        {
            foreach (int low in new[] { 24, 25, 127, 255 })
            {
                AssertThrows<InvalidDataException>(() => DecodedGrowthSpeed((high << 8) | low),
                    "growing shutter speed rejects out-of-domain low bytes regardless of high byte");
            }
        }
    }

    /// <summary>The growth speed the codec decodes from a placement's second parameter word.</summary>
    private static SignedSixteenSixteen DecodedGrowthSpeed(int parameter2) =>
        ((GrowingShutterSpawnSettings)SuperMetroid.AssetExtraction.Enemies.EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.ShutterGrowing,
            SuperMetroid.AssetExtraction.Enemies.PopulationWords.Parameters(0, parameter2))).GrowthSpeed;

    /// <summary>Reads a native velocity byte for the shutter speed-field comparison.</summary>
    private static byte GrowingShutterSpeedFieldRomByte(CartridgeImportAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
