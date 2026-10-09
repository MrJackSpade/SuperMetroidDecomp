using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// State $12 ($82:93A1) runs the whole state-eight routine, so a Samus unpaused at zero
    /// health with auto reserve enters the refill (state $1B) at once, leaving the screen black
    /// ($51 never finished fading in). A later pause darkens from that black register: state
    /// $0C ends on its first frame. The 13% movie does both after switching reserve to auto.
    /// </summary>
    private static void VerifyUnpauseReserveBlackout()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        PropertyInfo state = typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!;
        FieldInfo brightness = typeof(SuperMetroidGame).GetField("pauseBrightness", flags)!;
        (SuperMetroidGame Game, SuperMetroidRuntime Runtime) Create()
        {
            SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
            runtime.Samus!.InputLocked = false;
            var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
            typeof(SuperMetroidGame).GetField("runtime", flags)!.SetValue(game, runtime);
            typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", flags)!.SetValue(game, runtime.ActiveRoom!.State.Pointer);
            return (game, runtime);
        }

        var (unpausing, unpausingRuntime) = Create();
        SamusState samus = unpausingRuntime.Samus!;
        samus.Health = 0;
        samus.MaxReserveEnergy = samus.ReserveEnergy = 100;
        samus.ReserveTankMode = 1;
        brightness.SetValue(unpausing, (byte)0);
        state.SetValue(unpausing, SuperMetroidGameState.Unpausing);
        unpausing.Step(0);
        AssertEqual(SuperMetroidGameState.ReserveTanksAuto, unpausing.GameState,
            "state $12's gameplay pass routes zero health into the auto reserve refill");
        AssertEqual((byte)0, (byte)brightness.GetValue(unpausing)!, "the interrupted fade-in leaves $51 black");

        // Once gameplay resumes the screen is still black; a pause darkens from there.
        var (pausing, _) = Create();
        state.SetValue(pausing, SuperMetroidGameState.MainGameplay);
        brightness.SetValue(pausing, (byte)0);
        pausing.Step(0);
        pausing.Step((ushort)SnesButton.Start);
        AssertEqual(SuperMetroidGameState.PausingDarkening, pausing.GameState, "Start pauses");
        pausing.Step(0);
        AssertEqual(SuperMetroidGameState.Pausing, pausing.GameState,
            "darkening from a black screen ends on its first frame");
        Console.WriteLine("  Unpause reserve blackout: the refill starts in state $12 and the next pause darkens from black.");
    }
}
