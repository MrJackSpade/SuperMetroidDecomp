using SuperMetroid.Verification.Bosses.Botwoon;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Hud;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>
/// #1872: Mother Brain's death explosions own their position cycle. They start from their own first group whatever place
/// her ascent dust last took, and every burst reads the random number once per explosion whichever group it bursts at.
/// </summary>
internal static class MotherBrainDeathExplosionStartVerification
{
    /// <summary>
    /// Leaves the ascent dust at its third place, starts the rainbow beam and checks that the first smoky burst uses the
    /// death explosions' last group, the one their cycle starts from; then checks that a burst draws two random numbers
    /// from every group.
    /// </summary>
    internal static void VerifyMotherBrainDeathExplosionStart()
    {
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrainEncounter.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");

        state.AscentDustPlace = 2;
        runtime.Enemies.MotherBrainBehavior.Attacks.StartLiveMotherBrainRainbowBeam(state, runtime.Player.IntroducedSamus, null);
        MotherBrainRainbowBeamAttackSequence sequence = state.RainbowBeamSequence
            ?? throw new InvalidOperationException("Verification requires state.RainbowBeamSequence.");
        AssertEqual(0, sequence.ThirdPhaseDeath.DeathAndEscapeExplosionIndex, "the death explosions start from their own cycle");

        var requests = new List<MotherBrainDeathExplosionRequest>();
        sequence.ThirdPhaseDeath.GenerateDeathExplosions(MotherBrainDeathExplosionStyle.Smoky, () => 0, requests);
        AssertEqual(2, requests.Count, "a smoky burst spawns two explosions");
        AssertEqual(MotherBrainDeathExplosionDefinitions.GroupCount - 1, sequence.ThirdPhaseDeath.DeathAndEscapeExplosionIndex,
            "the first burst takes the last group");
        AssertEqual(new OctantVector(10, -31), new OctantVector(requests[0].Offset.X, requests[0].Offset.Y), "the last group's first offset");
        AssertEqual(new OctantVector(-20, -8), new OctantVector(requests[1].Offset.X, requests[1].Offset.Y), "the last group's second offset");
        AssertEqual(2, state.AscentDustPlace, "the death explosions leave the ascent dust's place alone");

        // The group selects positions only: every group draws one random number per explosion.
        for (int group = 0; group < MotherBrainDeathExplosionDefinitions.GroupCount; group++)
        foreach (MotherBrainDeathExplosionStyle style in Enum.GetValues<MotherBrainDeathExplosionStyle>())
        {
            sequence.ThirdPhaseDeath.DeathExplosionIntervalTimer = 0;
            sequence.ThirdPhaseDeath.DeathAndEscapeExplosionIndex = group;
            int draws = 0;
            var burst = new List<MotherBrainDeathExplosionRequest>();
            sequence.ThirdPhaseDeath.GenerateDeathExplosions(style, () => { draws++; return 0; }, burst);
            AssertEqual(burst.Count, draws, $"a {style} burst from group {group} draws one random number per explosion");
            AssertEqual(style == MotherBrainDeathExplosionStyle.Mixed ? 4 : 2, burst.Count, $"a {style} burst's size from group {group}");
        }
        Console.WriteLine("Mother Brain death explosion start: the death explosions start their own position cycle, and every group draws one random number per explosion.");
    }

    /// <summary>Runs the focused Mother Brain death-explosion start check.</summary>
    [VerificationCommand("--mother-brain-death-explosion-start")]
    internal static int MotherBrainDeathExplosionStartCommand()
    {
        VerifyMotherBrainDeathExplosionStart();
        return 0;
    }
}
