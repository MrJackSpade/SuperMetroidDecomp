using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// #1275: game-over indexes zero ($81:8D0F) and one ($81:91A4) each end in their own NMI
    /// wait, so each spans two updates and the second resumes without a main-loop dispatch.
    /// Index zero cancels library-three sounds; index two neither animates the Baby nor
    /// draws the selection missile.
    /// </summary>
    private static void VerifyGameOverMenuNativeIndexes(ISnesAddressSpace bus, AreaMapPresentationCatalog presentation)
    {
        var audio = new CartridgeAudioState();
        var menu = new GameOverMenuState(bus, audio, presentation);
        (GameOverMenuPhase Phase, bool Resumes)[] expected =
        [
            (GameOverMenuPhase.ConfigureGraphics, true),
            (GameOverMenuPhase.Initialize, false),
            (GameOverMenuPhase.Initialize, true),
            (GameOverMenuPhase.WaitForInitialMusic, false),
        ];
        AssertEqual(GameOverMenuPhase.ConfigureGraphics, menu.Phase, "game-over menu starts at index zero");
        for (int update = 0; update < expected.Length; update++)
        {
            menu.Step(0);
            AssertEqual(expected[update].Phase, menu.Phase, $"game-over index after update {update}");
            AssertEqual(expected[update].Resumes, menu.ResumesAfterNmiWait, $"game-over NMI wait after update {update}");
        }
        int library3 = (int)SoundEffectLibrary3Sounds.CancelAll.Library - 1;
        byte read = PrivateState.Field<byte[]>(audio, "_soundReadPositions")[library3];
        AssertEqual(1, PrivateState.Field<byte[]>(audio, "_soundWritePositions")[library3] - read,
            "game-over index zero queues one library-three request");
        AssertEqual(SoundEffectLibrary3Sounds.CancelAll.Value, PrivateState.Field<byte[,]>(audio, "_soundQueues")[library3, read],
            "game-over index zero queues library-three cancel");
        ushort babyPointer = menu.BabyInstructionPointer;
        int babyTimer = PrivateState.Field<ushort>(menu, "babyInstructionTimer");
        int missileTimer = PrivateState.Field<int>(menu, "missileTimer");
        menu.Step(0);
        AssertEqual(GameOverMenuPhase.WaitForInitialMusic, menu.Phase, "music wait holds while the queue is occupied");
        AssertEqual(babyPointer, menu.BabyInstructionPointer, "music wait does not animate the Baby");
        AssertEqual(babyTimer, (int)PrivateState.Field<ushort>(menu, "babyInstructionTimer"), "music wait keeps the Baby timer");
        AssertEqual(missileTimer, PrivateState.Field<int>(menu, "missileTimer"), "music wait does not draw the missile");

        // $81:915D: a Yes answer on a save whose loading state is $1F publishes the Ceres
        // loader on the accepting frame; any other save fades into the area map.
        foreach (bool ceres in new[] { true, false })
        {
            var answered = new GameOverMenuState(bus, new CartridgeAudioState(), presentation, continueLoadsCeresArrival: ceres);
            PrivateState.SetProperty(answered, nameof(GameOverMenuState.Phase), GameOverMenuPhase.Main);
            answered.Step(0);
            answered.Step((ushort)SuperMetroid.Core.Input.SnesButton.A);
            AssertEqual(ceres, answered.CeresArrivalRequested, $"Yes on loading state $1F={ceres} requests the Ceres loader");
            AssertEqual(ceres ? GameOverMenuPhase.Main : GameOverMenuPhase.FadeOutToContinue, answered.Phase,
                "only an area-map continue fades out");
        }
    }
}
