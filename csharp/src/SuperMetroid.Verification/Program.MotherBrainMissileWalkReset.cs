using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using static RetailRuntimeFixtureVerification;

/// <summary>Represents mother brain missile walk reset verification.</summary>
internal static class MotherBrainMissileWalkResetVerification
{
    // #1269: a missile hitting Mother Brain's head zeroes walk counter $7E:780E ($A9:B57A).
    // Once the rainbow sequence is attached it owns that counter and republishes it every
    // body turn, but the port's missile branch wrote only the encounter projection. In the
    // 100% movie the phase-three counter stayed $40 after a missile where native reset it,
    // and Mother Brain later inched forward instead of retreating.
    /// <summary>Verifies mother brain missile walk reset.</summary>
    internal static void VerifyMotherBrainMissileWalkReset()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrainEncounter.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");
        RoomEnemySlot head = state.Head ?? throw new InvalidOperationException("Mother Brain has no head enemy.");

        var sequence = new MotherBrainRainbowBeamAttackSequence();
        sequence.Body.Form = 4;
        typeof(MotherBrainRainbowBeamAttackSequence)
            .GetProperty(nameof(MotherBrainRainbowBeamAttackSequence.Phase3WalkCounter))!
            .SetValue(sequence, (ushort)0x0040);
        state.RainbowBeamSequence = sequence;
        state.Form = 4;
        state.WalkCounter = 0x0040;
        head.Health = 0x8bd8;

        var shot = runtime.Projectiles.Slots[0];
        shot.Type = 0x8100; shot.Damage = 100; shot.Direction = 8;
        shot.Position = shot.Position.WithWhole(head.Position.Whole);
        shot.Radius = new(8, 8);
        runtime.Enemies.MotherBrainBehavior.ResolveMotherBrainLaterFormHeadShot(state, head, shot, runtime.Projectiles, new SamusProjectileTypeWord(0x8100), (ushort)100);
        AssertEqual((ushort)0, sequence.Phase3WalkCounter, "a missile zeroes the sequence-owned walk counter");
        AssertEqual((ushort)0, state.WalkCounter, "the encounter projection publishes the reset");
        Console.WriteLine("Mother Brain missile walk reset: the reaction reaches the counter's owner.");
    }

    /// <summary>Mother brain missile walk reset command.</summary>
    [VerificationCommand("--mother-brain-missile-walk-reset")]
    internal static int MotherBrainMissileWalkResetCommand()
    {
        VerifyMotherBrainMissileWalkReset();
        return 0;
    }
}
