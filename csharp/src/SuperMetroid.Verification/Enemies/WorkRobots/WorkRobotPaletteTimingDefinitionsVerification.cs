using SuperMetroid.Core.Enemies.WorkRobots.Artwork;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.WorkRobots;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>Checks the Work Robot's palette record timing and installed colors against bank A3.</summary>
internal static class WorkRobotPaletteTimingDefinitionsVerification
{
    /// <summary>Compares the six glow steps' durations and colors with their native records, then follows a full production cycle through its glow shades.</summary>
    internal static void VerifyWorkRobotPaletteTimingDefinitions(
        CartridgeImportAddressSpace rom)
    {
        for (int glowStep = 0; glowStep < WorkRobotGlowDefinitions.StepCount; glowStep++)
        {
            int address = WorkRobotGlowDefinitions.NativeFirstTimerAddress +
                glowStep * WorkRobotGlowDefinitions.RecordByteCount;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(address),
                WorkRobotGlowDefinitions.Durations[glowStep],
                $"Work Robot glow step {glowStep} duration");
        }
        AssertEqual((ushort)0xffff,
            CartridgeImportSource.Require(rom).ReadCartridgeWord(WorkRobotGlowDefinitions.NativeTerminatorAddress),
            "Work Robot palette native terminator restarts the cycle after the sixth step");

        byte[] extracted = WorkRobotPaletteCycleExtractor.Extract(rom);
        WorkRobotPaletteCycle stock = WorkRobotPaletteCycleFile.Read(
            new MemoryStream(extracted, writable: false));
        for (int record = 0; record < WorkRobotGlowDefinitions.StepCount; record++)
        for (int color = 0; color < WorkRobotPaletteDefinitions.ColorCount; color++)
        {
            int source = EnemyRomTablePointers.WorkRobot.PaletteAnimationRecords +
                record * WorkRobotGlowDefinitions.RecordByteCount +
                color * sizeof(ushort);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(source), stock.Resolve(record, color),
                $"Work Robot color record {record} slot {color} preserves ROM RGB5");
        }
        var guard = NewWorkRobotPaletteTimingReadGuard(rom);
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledFrames(EnemyTileSourceArtwork.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(), WorkRobotPaletteCycle = stock })), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        enemies.WorkRobotBehavior.RestartGlow();
        var step = new Action(enemies.WorkRobotBehavior.StepGlow);

        int[] loadCalls = [0, 64, 80, 96, 160, 176, 192];
        int nextLoad = 0;
        for (int call = 0; call <= loadCalls[^1]; call++)
        {
            step();
            if (call != loadCalls[nextLoad])
                continue;

            int record = nextLoad % WorkRobotGlowDefinitions.StepCount;
            AssertEqual(record,
                enemies.WorkRobotBehavior.GlowStep,
                $"production Work Robot glow load {nextLoad} shows the next step");
            AssertEqual(WorkRobotGlowDefinitions.Durations[record],
                enemies.WorkRobotBehavior.GlowStepCountdown,
                $"production Work Robot glow load {nextLoad} timer");
            // The robots draw in the record's glow shade, which the import pre-colours from the cycle's colours for the
            // record (#1512); those colours are compared with bank $A3 above.
            AssertEqual(PictureShading.Shaded(WorkRobotShades.Glow(record)), enemies.WorkRobotBehavior.Glow,
                $"production Work Robot palette load {nextLoad} shows the record's glow shade");
            nextLoad++;
        }

        AssertEqual(loadCalls.Length, nextLoad,
            "production Work Robot palette reaches every record and wraps");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed Work Robot palette performs no color, timing or terminator ROM reads");
        Console.WriteLine(
            "Work Robot palette: all six glow-step durations and 24 colors, the terminator and " +
            "the complete 193-call production cycle pass with ROM reads forbidden.");
    }

    /// <summary>Rejects live reads of Work Robot palette timer, terminator, and color bytes.</summary>
    private static CartridgeReadGuard NewWorkRobotPaletteTimingReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
            {
                int relative = address -
                    WorkRobotGlowDefinitions.NativeFirstTimerAddress;
                bool timerByte = relative >= 0 &&
                    relative < WorkRobotGlowDefinitions.StepCount *
                        WorkRobotGlowDefinitions.RecordByteCount &&
                    relative % WorkRobotGlowDefinitions.RecordByteCount < 2;
                bool terminatorByte = address is
                    WorkRobotPaletteTimingDefinitionsConstants.NativeTerminatorAddress or
                    WorkRobotPaletteTimingDefinitionsConstants.NativeTerminatorAddress + 1;
                int colorRelative = address -
                    EnemyRomTablePointers.WorkRobot.PaletteAnimationRecords;
                bool colorByte = colorRelative >= 0 &&
                    colorRelative < WorkRobotGlowDefinitions.StepCount *
                        WorkRobotGlowDefinitions.RecordByteCount &&
                    colorRelative % WorkRobotGlowDefinitions.RecordByteCount <
                        WorkRobotPaletteDefinitions.ColorCount * sizeof(ushort);
                return timerByte || terminatorByte || colorByte;
            },
            address => $"Work Robot palette handler attempted control read ${address:X6}.");

    /// <summary>Runs the Work Robot palette timing check with the retail cartridge as its oracle.</summary>
    [VerificationCommand("--work-robot-palette-timing-definitions")]
    internal static int WorkRobotPaletteTimingDefinitionsCommand()
    {
        Suite(nameof(VerifyWorkRobotPaletteTimingDefinitions), () => VerifyWorkRobotPaletteTimingDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
