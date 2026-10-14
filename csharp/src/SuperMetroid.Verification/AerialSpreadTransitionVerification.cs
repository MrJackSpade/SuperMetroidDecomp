using static SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Samus.Poses;

/// <summary>Checks charged aerial bomb-spread release timing through turnaround and walljump routes.</summary>
internal static class AerialSpreadTransitionVerification
{
    /// <summary>
    /// Issue 415 turnaround and walljump spreads, earned from charge and input alone: only the
    /// native one-frame window releases the aerial spread, and the walljump retains its charge.
    /// </summary>
    internal static void VerifyAerialSpreadTransitions(bool wallRoute = false)
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (bool left in new[] { false, true })
        for (int timingCase = 0; timingCase < BombSpreadTransitionScenario.TimingCaseCount(wallRoute); timingCase++)
        {
            var scenario = new BombSpreadTransitionScenario(CreateRetailRuntimeFixture(bus), bus, left, timingCase, wallRoute);
            var runtime = scenario.Runtime;
            var samus = scenario.Samus;
            for (int frame = 0; frame < scenario.FrameCount; frame++)
            {
                uint previousY = new SnesFixedPosition(samus.Position.Y.Whole, samus.Kinematics.Position.Y.Fraction).RawValue;
                runtime.StepFrame(scenario.Input(frame));
                AssertEqual(samus.ProjectileFlareCounter, runtime.Projectiles.Charge.FlareCounter, "aerial charge mirror");
                if (wallRoute)
                    VerifyWallSpreadFrame(runtime, left, timingCase, scenario.Delay, frame, previousY);
                else
                    VerifyTurnaroundSpreadFrame(runtime, left, scenario.Delay, frame);
            }
        }
        Console.WriteLine(wallRoute
            ? "Charged-walljump spread: early/late morph success and held-Shoot/turn controls agree in both directions."
            : "Aerial down-aim spread: both directions and ten input timings, through expiry.");
    }

    /// <summary>Checks the single turnaround release window, charge retention during morph, and bomb expiry.</summary>
    private static void VerifyTurnaroundSpreadFrame(SuperMetroidRuntime runtime, bool left, int delay, int frame)
    {
        var samus = runtime.Player.IntroducedSamus;
        bool releasedSpread = delay == 6 && frame >= 105;
        if (frame < 125 || delay != 6)
            AssertEqual((ushort)(releasedSpread ? 5 : 0), runtime.BombProjectiles.BombCounter,
                $"only native one-frame window produces spread: left={left}, delay={delay}, frame={frame}");
        if (frame == 259) AssertEqual((ushort)0, runtime.BombProjectiles.BombCounter, "aerial spread expires completely");
        if (delay == 6 && frame is >= 92 and <= 104)
        {
            AssertEqual((ushort)80, samus.ProjectileFlareCounter, "airborne morph preserves earned charge while Down is held");
            AssertEqual((ushort)(frame - 91), samus.BombSpreadChargeTimeoutCounter, "airborne hold advances after morph completes");
        }
        if (releasedSpread)
            AssertEqual((ushort)0, samus.ProjectileFlareCounter, "airborne spread consumes charge");
    }

    /// <summary>Checks walljump pose, retained charge, held-Down release, and negative input controls frame by frame.</summary>
    private static void VerifyWallSpreadFrame(SuperMetroidRuntime runtime, bool left, int timingCase, int delay, int frame, uint previousY)
    {
        var samus = runtime.Player.IntroducedSamus;
        if (frame == 89)
            AssertEqual(left ? SamusPoseId.WallJumpRightPose : SamusPoseId.WallJumpLeftPose,
                samus.Pose, "real input must earn a walljump, including negative controls");
        if (timingCase < 12)
        {
            int morphFrame = 94 + delay;
            int releaseFrame = 115 + delay;
            if (frame >= 87 && frame < releaseFrame)
                AssertEqual((ushort)71, samus.ProjectileFlareCounter, "walljump retains charge through released Shoot and morph");
            if (frame >= morphFrame && frame < morphFrame + 6)
                AssertEqual(left ? SamusPoseId.MorphingTransitionRightPose : SamusPoseId.MorphingTransitionLeftPose,
                    samus.Pose, "six-frame airborne morph follows charged walljump");
            if (frame >= morphFrame + 7 && frame < releaseFrame)
                AssertEqual((ushort)(frame - morphFrame - 6), samus.BombSpreadChargeTimeoutCounter, "walljump spread hold cadence");
            if (frame < releaseFrame)
                AssertEqual((ushort)0, runtime.BombProjectiles.BombCounter, "Down retains the aerial spread");
            if (frame == releaseFrame)
                AssertEqual((ushort)5, runtime.BombProjectiles.BombCounter, "Down release launches all five bombs");
            if (frame >= releaseFrame)
                AssertEqual((ushort)0, samus.ProjectileFlareCounter, "walljump spread consumes charge");
            if (delay == 41 && frame == 134)
                AssertTrue((new SnesFixedPosition(samus.Position.Y.Whole, samus.Kinematics.Position.Y.Fraction).RawValue) > previousY,
                    "late success starts morph after descent has already begun, as native permits");
        }
        else
        {
            AssertEqual((ushort)0, runtime.BombProjectiles.BombCounter, "held Shoot or intervening turn prevents walljump spread");
            if (timingCase == 12 && frame == 90)
                AssertEqual(left ? SamusPoseId.NormalJumpGunExtendedRightPose : SamusPoseId.NormalJumpGunExtendedLeftPose, samus.Pose,
                    "held Shoot interrupts the newly earned walljump into normal jump");
            if (timingCase == 13 && frame == 95)
                AssertEqual((ushort)0, samus.ProjectileFlareCounter, "intervening turn fires the retained charge instead of morphing");
        }
        if (frame == 299) AssertEqual((ushort)0, runtime.BombProjectiles.BombCounter, "walljump spread expires");
    }

    /// <summary>Runs both-direction charged walljump spread timing checks.</summary>
    [VerificationCommand("--wall-spread-transition")]
    internal static int WallSpreadTransitionCommand()
    {
        Suite(nameof(VerifyAerialSpreadTransitions), () => VerifyAerialSpreadTransitions(wallRoute: true));
        return 0;
    }

    /// <summary>Runs both-direction aerial turnaround spread timing checks.</summary>
    [VerificationCommand("--aerial-spread-transition")]
    internal static int AerialSpreadTransitionCommand()
    {
        Suite(nameof(VerifyAerialSpreadTransitions), () => VerifyAerialSpreadTransitions());
        return 0;
    }
}
