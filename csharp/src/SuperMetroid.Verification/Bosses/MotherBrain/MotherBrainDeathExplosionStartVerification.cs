using SuperMetroid.Verification.Bosses.Botwoon;
using SuperMetroid.Core;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Hud;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>
/// #1872: Mother Brain's death explosions and the escape door's dust burst where the cartridge's do. Her ascent's dust, her
/// death explosions and the door's dust continue one place (<see cref="MotherBrainBurstCycle"/>): the first death burst
/// takes the group before the place the ascent's last dust cloud took, the door's first dust the position before the place
/// the last death burst took, and the door's dust continues the death explosions' interval. Every burst reads the random
/// number once per explosion whichever group it bursts at.
/// </summary>
internal static class MotherBrainDeathExplosionStartVerification
{
    /// <summary>
    /// Leaves the ascent dust at its third place, starts the rainbow beam and checks that the first smoky burst uses the
    /// group before it, with the cartridge's offsets; that the escape door's dust continues the place and the interval the
    /// death explosions left; and that a burst draws one random number per explosion from every place.
    /// </summary>
    internal static void VerifyMotherBrainDeathExplosionStart()
    {
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.BossEncounters.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");

        state.Bursts.Place = 2;
        MotherBrainAttacks.StartLiveMotherBrainRainbowBeam(state);
        MotherBrainRainbowBeamAttackSequence sequence = state.RainbowBeamSequence
            ?? throw new InvalidOperationException("Verification requires state.RainbowBeamSequence.");

        var requests = new List<MotherBrainDeathExplosionRequest>();
        sequence.ThirdPhaseDeath.GenerateDeathExplosions(MotherBrainDeathExplosionStyle.Smoky, () => 0, requests);
        AssertEqual(2, requests.Count, "a smoky burst spawns two explosions");
        AssertEqual(1, state.Bursts.Place, "the first death burst takes the group before the ascent dust's place");
        AssertEqual(new OctantVector(17, -55), new OctantVector(requests[0].Offset.X, requests[0].Offset.Y), "the second group's first offset");
        AssertEqual(new OctantVector(30, -22), new OctantVector(requests[1].Offset.X, requests[1].Offset.Y), "the second group's second offset");

        // The door's dust continues the place and the interval the last death burst left.
        sequence.State.DeathPhase = MotherBrainDeathPhase.EscapeDoorBursts;
        sequence.Encounter.PhaseTimer.LastFor(33);
        state.Bursts.Place = 2;
        state.Bursts.burstWait.PlaceAt(UpdateCountdown.For(0 + 1));
        var frame = MotherBrainLateFightFixtures.QuietFrame(0, 0) with { NextRandomNumber = () => 0 };
        MotherBrainRainbowBeamAttackStepResult first = sequence.Step(runtime.Player.Samus, frame);
        AssertEqual(MotherBrainDeathDefinitions.EscapeDoorDustPositions[2],
            (first.EscapeDoorExplosion ?? throw new InvalidOperationException("The door's first dust did not burst.")).Position,
            "the door's first dust takes the position before the death explosions' place");
        AssertEqual(4, (state.Bursts.burstWait.UpdatesLeft - 1), "the door's dust waits four updates after a burst");
        state.Bursts.burstWait.PlaceAt(UpdateCountdown.For(1 + 1));
        AssertTrue(sequence.Step(runtime.Player.Samus, frame).EscapeDoorExplosion is null,
            "the door's dust waits while the interval it continues has not run out");

        // The group selects positions only: every group draws one random number per explosion.
        for (int place = 0; place < MotherBrainDeathExplosionDefinitions.Groups.Count; place++)
        foreach (MotherBrainDeathExplosionStyle style in Enum.GetValues<MotherBrainDeathExplosionStyle>())
        {
            state.Bursts.burstWait.PlaceAt(UpdateCountdown.For(0 + 1));
            state.Bursts.Place = place;
            int draws = 0;
            var burst = new List<MotherBrainDeathExplosionRequest>();
            sequence.ThirdPhaseDeath.GenerateDeathExplosions(style, () => { draws++; return 0; }, burst);
            AssertEqual(burst.Count, draws, $"a {style} burst after place {place} draws one random number per explosion");
            AssertEqual(style == MotherBrainDeathExplosionStyle.Mixed ? 4 : 2, burst.Count, $"a {style} burst's size after place {place}");
        }
        Console.WriteLine("Mother Brain death explosion start: the death explosions and the door's dust continue the place the effect before left, and every group draws one random number per explosion.");
    }

    /// <summary>Runs the focused Mother Brain death-explosion start check.</summary>
    [VerificationCommand("--mother-brain-death-explosion-start")]
    internal static int MotherBrainDeathExplosionStartCommand()
    {
        VerifyMotherBrainDeathExplosionStart();
        return 0;
    }
}
