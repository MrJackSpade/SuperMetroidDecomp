using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    // #1269: Function_MotherBrainBody_FakeDeath_Ascent_RaiseMotherBrain ($A9:8E4D) raises
    // Mother Brain only when NMI_FrameCounter ($05B6) & 3 is zero. The port gated on the
    // separate 8-bit $05B5, which is not aligned with it: in the 100% movie the ascent began
    // one update early, at $05B5=$68 rather than $05B6=$200.
    private static void VerifyMotherBrainRaiseCounter()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");
        state.Function = MotherBrainBodyFunction.FakeDeathAscentRaiseMotherBrain;
        state.Body.XPosition = 0x003b;
        state.Body.YPosition = 0x0117;
        var camera = runtime.Camera!;

        void Step(byte counter8, ushort counter) => runtime.Enemies.StepFrame(
            camera.XPosition, camera.YPosition, timeIsFrozen: false, runtime.Samus,
            level: runtime.LevelData, nmiFrameCounter8: counter8, nmiFrameCounter: counter);

        Step(0x68, 0x01ff);
        AssertEqual((ushort)0x0117, state.Body.YPosition, "no raise while $05B6 & 3 is nonzero, whatever $05B5 holds");
        Step(0x69, 0x0200);
        AssertEqual((ushort)0x0115, state.Body.YPosition, "the raise follows $05B6 & 3 == 0");
        Console.WriteLine("Mother Brain raise counter: the ascent gates on NMI_FrameCounter ($05B6).");
    }
}
