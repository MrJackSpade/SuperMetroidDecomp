using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyEscapeRunningFootsteps()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (bool able in new[] { false, true })
        foreach (byte pose in new[] { SamusPoseIds.MovingRightNormalPose, SamusPoseIds.MovingLeftNormalPose })
        {
            var samus = new SamusState { Pose = SamusPoseIds.FacingRightNormalPose, XPosition = 100, YPosition = 100 };
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            if (able) samus.Drained.SetupForRainbowBeamAbleToStand(bus, samus);
            else samus.Drained.SetupForRainbowBeamUnableToStand(bus, samus);
            samus.Drained.PutCrouchingOrFalling(bus, samus);
            if (!able)
            {
                AssertTrue(samus.Drained.StepGetUpHandler(samus, (ushort)SnesButton.Up), "cutscene still accepts failed stand-up");
                AssertEqual((ushort)18, samus.AnimationFrame, "native failed stand-up still selects frame eighteen");
            }

            // Begin at the identified native timer-handoff phase, not a boss playthrough.
            var sequence = new MotherBrainRainbowBeamAttackSequence();
            typeof(MotherBrainRainbowBeamAttackSequence).GetProperty(nameof(sequence.Phase))!
                .SetValue(sequence, MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceDoorExplodingStartTimer);
            var request = sequence.Step(bus, samus, 0, 0, nextRandomNumber: () => 0);
            AssertTrue(request.TimerHandlingEnableRequested && request.MotherBrainEscapeTimerStartRequested,
                "native death phase emits command F and timer start together");
            var runtime = new SuperMetroidRuntime(bus, initialPaletteArt: BombTorizoHandFixturePalette());
            typeof(SuperMetroidRuntime).GetProperty(nameof(runtime.Samus))!.SetValue(runtime, samus);
            var motherBrain = new MotherBrainEnemyState(runtime.Enemies.Slots[0])
            {
                // Area flags are outside this fixture. Preserve the actual timer requests.
                LastRainbowBeamStep = request with { MotherBrainBossBitRequested = false, ZebesTimebombEventRequested = false },
            };
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            typeof(RoomEnemySystem).GetField("_motherBrain", flags)!.SetValue(runtime.Enemies, motherBrain);
            typeof(SuperMetroidRuntime).GetMethod("ApplyPendingMotherBrainPlms", flags)!
                .CreateDelegate<Action>(runtime)();

            samus.Pose = pose;
            samus.InputLocked = false;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus, initialFrame: 8);
            samus.SetAnimationFrameFromSpecialHandler(8, 1);
            samus.LiquidPhysics.FxType = RoomFxType.Acid;
            samus.LiquidPhysics.LavaAcidYPosition = 0;
            samus.Drained.StepGetUpHandler(samus, (ushort)(SnesButton.Up | SnesButton.A));
            // Before the fix, the stale failed-stand callback selects 18/1 here and the
            // real animation/footstep path throws the exact player-reported exception.
            samus.AnimateNoFx(bus, (ushort)(SnesButton.Up | SnesButton.A));
            AssertEqual(DrainedGetUpHandler.Inactive, samus.Drained.GetUpHandler,
                "escape timer replaces the drained hack handler");
            AssertEqual((ushort)9, samus.AnimationFrame, "Up edge in acid advances the running animation normally");
            AssertTrue(samus.AnimationFrameTimer > 0, "running cadence receives its real next-frame delay");
        }
        Console.WriteLine("Escape running footsteps: native timer handoff displaces both drained handlers; Up/jump in acid preserves both running animations and the cutscene failed-stand branch.");
    }
}
