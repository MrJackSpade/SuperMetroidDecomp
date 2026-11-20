using SuperMetroid.Core.Animation;
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
    /// <summary>Compares the six glow steps' holds and colors with their native records, then follows a full production cycle through its glow shades.</summary>
    internal static void VerifyWorkRobotPaletteTimingDefinitions(
        CartridgeImportAddressSpace rom)
    {
        int[] holds = [.. WorkRobotGlowAnimations.Segments.Beats(WorkRobotGlowSegment.Glowing)
            .Select(beat => ((AnimationBeat<WorkRobotGlowPlace, NoAnimationCue>.Frame)beat).Duration)];
        AssertEqual(WorkRobotGlowAnimations.Cycle.Count, holds.Length, "every beat of the Work Robot glow shows a step for a hold");
        for (int glowStep = 0; glowStep < WorkRobotGlowAnimations.Cycle.Count; glowStep++)
        {
            int address = WorkRobotGlowDefinitions.NativeFirstTimerAddress +
                glowStep * WorkRobotGlowDefinitions.RecordByteCount;
            AssertEqual(rom.ReadCartridgeWord(address),
                holds[glowStep],
                $"Work Robot glow step {WorkRobotGlowAnimations.Cycle[glowStep]} hold");
        }
        AssertEqual((ushort)0xffff,
            rom.ReadCartridgeWord(WorkRobotGlowDefinitions.NativeTerminatorAddress),
            "Work Robot palette native terminator restarts the cycle after the sixth step");

        byte[] extracted = WorkRobotPaletteCycleExtractor.Extract(rom);
        WorkRobotPaletteCycle stock = WorkRobotPaletteCycleFile.Read(
            new MemoryStream(extracted, writable: false));
        for (int record = 0; record < WorkRobotGlowAnimations.Cycle.Count; record++)
        for (int color = 0; color < WorkRobotPaletteDefinitions.ColorCount; color++)
        {
            int source = EnemyRomTablePointers.WorkRobot.PaletteAnimationRecords +
                record * WorkRobotGlowDefinitions.RecordByteCount +
                color * sizeof(ushort);
            AssertEqual(rom.ReadCartridgeWord(source), stock.Resolve(record, color),
                $"Work Robot color record {record} slot {color} preserves ROM RGB5");
        }
        // The way back passes the places between the ends again in the same colours: records 1 and 5, and 2 and 4, are one
        // colouring each, so the glow installs four shades (#2393).
        int[] shadeOfRecord = [0, 1, 2, 3, 2, 1];
        AssertEqual(4, WorkRobotGlowAnimations.Places.Count, "the Work Robot glow shows four places, one shade each");
        for (int record = 0; record < WorkRobotGlowAnimations.Cycle.Count; record++)
        for (int color = 0; color < WorkRobotPaletteDefinitions.ColorCount; color++)
        {
            AssertEqual(stock.Resolve(shadeOfRecord[record], color), stock.Resolve(record, color),
                $"Work Robot color record {record} slot {color} matches the record of the place it shows");
        }
        var guard = NewWorkRobotPaletteTimingReadGuard(rom);
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledFrames(EnemyTileSourceArtwork.FromArtworkForVerification(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.EnemyArtworkParts with { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(), WorkRobotPaletteCycle = stock })), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        // A powered robot's initialization restarts the glow; the glow shows on the robot it placed and on no other place
        // (#2306).
        RoomEnemyPlace robot = enemies.Places.InUpdateOrder[0];
        RoomEnemyPlace other = enemies.Places.InUpdateOrder[1];
        enemies.WorkRobotBehavior.InitializePoweredWorkRobot(robot);
        var step = new Action(enemies.WorkRobotBehavior.StepGlow);

        int[] loadCalls = [0, 64, 80, 96, 160, 176, 192];
        int nextLoad = 0;
        for (int call = 0; call <= loadCalls[^1]; call++)
        {
            step();
            if (call != loadCalls[nextLoad])
                continue;

            int record = nextLoad % WorkRobotGlowAnimations.Cycle.Count;
            AssertEqual(WorkRobotGlowAnimations.Cycle[record],
                enemies.WorkRobotBehavior.GlowPlace,
                $"production Work Robot glow load {nextLoad} shows the next step");
            AssertEqual(holds[record],
                enemies.WorkRobotBehavior.UpdatesUntilNextGlowStep,
                $"production Work Robot glow load {nextLoad} timer");
            // The robots draw in the glow shade of the place the record shows, which the import pre-colours from the cycle's
            // colours for that place (#1512); those colours are compared with bank $A3 above. Records showing one place share
            // its shade (#2393).
            AssertEqual(WorkRobotShades.Glow(WorkRobotGlowAnimations.Places[shadeOfRecord[record]]), WorkRobotShades.Glow(WorkRobotGlowAnimations.Cycle[record]),
                $"production Work Robot glow load {nextLoad} keys its shade by the place it shows");
            AssertEqual(PictureShading.Shaded(WorkRobotShades.Glow(WorkRobotGlowAnimations.Cycle[record])), enemies.WorkRobotBehavior.Glow,
                $"production Work Robot palette load {nextLoad} shows the record's glow shade");
            AssertEqual(enemies.WorkRobotBehavior.Glow, robot.Shading,
                $"production Work Robot palette load {nextLoad} shows on the robot it placed");
            AssertEqual(PictureShading.Standard, other.Shading,
                $"production Work Robot palette load {nextLoad} leaves a place it did not set up in its standard colours");
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
                    relative < WorkRobotGlowAnimations.Cycle.Count *
                        WorkRobotGlowDefinitions.RecordByteCount &&
                    relative % WorkRobotGlowDefinitions.RecordByteCount < 2;
                bool terminatorByte = address is
                    WorkRobotPaletteTimingDefinitionsConstants.NativeTerminatorAddress or
                    WorkRobotPaletteTimingDefinitionsConstants.NativeTerminatorAddress + 1;
                int colorRelative = address -
                    EnemyRomTablePointers.WorkRobot.PaletteAnimationRecords;
                bool colorByte = colorRelative >= 0 &&
                    colorRelative < WorkRobotGlowAnimations.Cycle.Count *
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
