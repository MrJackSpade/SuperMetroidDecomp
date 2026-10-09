using SuperMetroid.Core.Runtime;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    // #1269: a missile hitting Mother Brain's head zeroes walk counter $7E:780E ($A9:B57A).
    // Once the rainbow sequence is attached it owns that counter and republishes it every
    // body turn, but the port's missile branch wrote only the encounter projection. In the
    // 100% movie the phase-three counter stayed $40 after a missile where native reset it,
    // and Mother Brain later inched forward instead of retreating.
    private static void VerifyMotherBrainMissileWalkReset()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrain
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
        shot.XPosition = head.XPosition; shot.YPosition = head.YPosition;
        shot.XRadius = shot.YRadius = 8;
        typeof(RoomEnemySystem).GetMethod("ResolveMotherBrainLaterFormHeadShot", flags)!.Invoke(
            runtime.Enemies,
            [state, head, shot, runtime.Projectiles, new SamusProjectileTypeWord(0x8100), (ushort)100]);
        AssertEqual((ushort)0, sequence.Phase3WalkCounter, "a missile zeroes the sequence-owned walk counter");
        AssertEqual((ushort)0, state.WalkCounter, "the encounter projection publishes the reset");
        Console.WriteLine("Mother Brain missile walk reset: the reaction reaches the counter's owner.");
    }
}
