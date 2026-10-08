using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    // #1269: native word $0FF2 is one word under three names: the fake-death ascent dust's
    // body sub-function timer, the death-explosion index ($A9:B046) and the escape-door dust
    // index ($A9:B355). The ascent dust leaves it at 2, so the first death burst decrements it
    // to row 1. The port's sequence kept its own index starting at 0, wrapped to row 6, and in
    // the 100% movie spawned both death explosions at the wrong offsets.
    private static void VerifyMotherBrainInheritedExplosionIndex()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");

        state.BodySubFunctionTimer = 2;
        typeof(RoomEnemySystem).GetMethod("StartLiveMotherBrainRainbowBeam", flags)!
            .Invoke(runtime.Enemies, [state, runtime.Samus!, null]);
        MotherBrainRainbowBeamAttackSequence sequence = state.RainbowBeamSequence!;
        AssertEqual((ushort)2, sequence.DeathAndEscapeExplosionIndex, "the sequence inherits word $0FF2");

        var requests = new List<MotherBrainDeathExplosionRequest>();
        typeof(MotherBrainRainbowBeamAttackSequence).GetMethod("GenerateDeathExplosions", flags)!
            .Invoke(sequence, [false, (Func<ushort>)(() => 0), requests]);
        AssertEqual(2, requests.Count, "a smoky burst spawns two explosions");
        AssertEqual((ushort)1, sequence.DeathAndEscapeExplosionIndex, "the first burst decrements to row 1");
        AssertEqual(((short)0x0011, unchecked((short)0xffc9)), (requests[0].XOffset, requests[0].YOffset),
            "row 1's first offset pair");
        AssertEqual(((short)0x001e, unchecked((short)0xffea)), (requests[1].XOffset, requests[1].YOffset),
            "row 1's second offset pair");
        Console.WriteLine("Mother Brain inherited explosion index: the death explosions continue word $0FF2.");
    }
}
