using static SuperMetroid.Verification.Bosses.Phantoon.PhantoonSoundDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Phantoon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Phantoon's casual flame schedules and RNG selection against native timings.</summary>
internal static class PhantoonCasualFlamesVerification
{
    /// <summary>Compares four schedule word lists, every RNG selector, and complete mouth-animation transitions.</summary>
    internal static void VerifyCompiledPhantoonCasualFlames(CartridgeImportAddressSpace rom)
    {
        for (int pattern = 0; pattern < 4; pattern++)
        {
            int pointer = 0xa70000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7ccfd + pattern * 2));
            PhantoonCasualFlameSchedule compiled = PhantoonCasualFlames.Pattern(pattern);
            int flames = CartridgeImportSource.Require(rom).ReadCartridgeWord(pointer);
            AssertEqual(flames, compiled.FlameCount, "Native casual flame count");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(pointer + 2), compiled.Cooldown, "Native casual flame cooldown");
            // The native table stores the intervals last flame first; the burst lists them first flame first.
            for (int flame = 0; flame < flames; flame++)
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(pointer + (flames + 1 - flame) * 2),
                    compiled.Intervals[flame], "Native casual flame interval");
        }
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), services: new VerificationRoomEnemyServices(null));
        var body = enemies.SlotTable.Slots[0];
        var mouth = enemies.SlotTable.Slots[3];
        var state = new PhantoonEnemyState(body) { Eye = enemies.SlotTable.Slots[1], Tentacles = enemies.SlotTable.Slots[2], Mouth = mouth };
        enemies.PhantoonBehavior.Combat.Phantoon = state;
        void step(RoomEnemySlot _) => enemies.PhantoonBehavior.Patterns.StepPhantoonCasualFlameSchedule(state);
        ushort random = 0;
        int calls = 0;
        enemies.VerificationServices().NextRandom = () => { calls++; return random; };
        // All RNG words enter the actual new-pattern branch with no address space attached.
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            random = (ushort)raw;
            state.MouthState.CasualFlameHold = 0;
            state.MouthState.CasualFlameCount = null;
            int before = calls;
            step(mouth);
            int pointer = 0xa70000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7ccfd + (raw & 3) * 2));
            AssertEqual((ushort)(raw & 3), state.MouthState.CasualFlamePattern, "Actual casual RNG pattern selector");
            AssertEqual((int?)CartridgeImportSource.Require(rom).ReadCartridgeWord(pointer), state.MouthState.CasualFlameCount, "Actual casual initial remaining count");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((pointer + (CartridgeImportSource.Require(rom).ReadCartridgeWord(pointer) + 1) * 2)), state.MouthState.CasualFlameHold, "Actual casual reverse initial timer");
            AssertEqual(before + 1, calls, "Casual selection consumes exactly one RNG call");
        }
        // Trace multiple complete schedules. Native $CFFF branches on zero as well
        // as negative, forces FFFF, and selects the timer at pointer+2, not the header.
        for (int pattern = 0; pattern < 4; pattern++)
        {
            random = (ushort)pattern;
            state.MouthState.CasualFlamePattern = (ushort)pattern;
            state.MouthState.CasualFlameHold = 0;
            state.MouthState.CasualFlameCount = null;
            ushort timer = 0, count = 0xffff;
            int pointer = 0xa70000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7ccfd + pattern * 2));
            for (int frame = 0; frame < 2048; frame++)
            {
                // The mouth rests closed before each update; only the casual trigger installs its flame-spitting animation.
                state.PartPlayback(mouth) = PhantoonEnemyState.Starting(PhantoonPartProgram.MouthInitial);
                mouth.AnimationClock.HoldFor(0);
                bool draw = false;
                timer = WrappingCounter.Decrement(timer).Value;
                if ((short)timer <= 0)
                {
                    if ((short)count >= 0)
                    {
                        count = WrappingCounter.Decrement(count).Value;
                        if ((short)count <= 0)
                        {
                            count = 0xffff;
                            timer = CartridgeImportSource.Require(rom).ReadCartridgeWord((pointer + 2));
                        }
                        else timer = CartridgeImportSource.Require(rom).ReadCartridgeWord((pointer + (count + 1) * 2));
                        draw = true;
                    }
                    else
                    {
                        count = CartridgeImportSource.Require(rom).ReadCartridgeWord(pointer);
                        timer = CartridgeImportSource.Require(rom).ReadCartridgeWord((pointer + (count + 1) * 2));
                    }
                }
                step(mouth);
                AssertEqual(timer, state.MouthState.CasualFlameHold, "Actual casual frame timer");
                // The cartridge's word holds the count, or a value below zero while no burst is under way, which the
                // mouth holds as no count (#1627).
                AssertEqual(unchecked((short)count) < 0 ? null : (int?)unchecked((short)count), state.MouthState.CasualFlameCount,
                    "Actual casual frame remaining count");
                AssertEqual(PhantoonEnemyState.Starting(draw ? PhantoonPartProgram.MouthSpawnFlame : PhantoonPartProgram.MouthInitial), state.PartPlayback(mouth),
                    "Exact casual mouth animation trigger frame");
                AssertEqual(draw ? (ushort)1 : (ushort)0, mouth.AnimationClock.Hold.RawValue, "Exact casual instruction timer reset");
            }
        }
        Console.WriteLine("Phantoon casual schedules: 30 native words, 65536 RNG selections and 8192 frame-exact mouth transitions match without a bus.");
    }

    /// <summary>Runs the casual flame schedule and Phantoon sound-definition checks.</summary>
    [VerificationCommand("--lookup-stream5-phantoon-schedules")]
    internal static int LookupStream5PhantoonSchedulesCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyCompiledPhantoonCasualFlames), () => VerifyCompiledPhantoonCasualFlames(rom));
        Suite(nameof(VerifyPhantoonSoundDefinitions), () => VerifyPhantoonSoundDefinitions(rom));
        return 0;
    }
}
