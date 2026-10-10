using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    /// <summary>
    /// Plays the 13% lsnes movie's power-on intro through the production game and checks the
    /// milestones its native bsnes capture records. Every press lands on the update native
    /// consumed it, so each scene must open its input window on native's exact dispatch.
    /// </summary>
    private static void VerifyLowPercentIntroTimeline()
    {
        // Converted controller words of the movie's first 7742 updates; all others are zero.
        var inputs = new Dictionary<int, ushort>
        {
            [2] = 0x0080, [19] = 0x0080, [85] = 0x0080, [86] = 0x1000, [134] = 0x0880,
            [165] = 0x0400, [166] = 0x0200, [167] = 0x0480, [198] = 0x0080, [1719] = 0x0080,
            [3012] = 0x0080, [4433] = 0x0080, [5501] = 0x0080, [6684] = 0x0080,
        };
        // Every phase handoff, keyed by the update after which the capture shows bank $8B's
        // matching cinematic function installed. Between them native changes nothing.
        var phases = new Dictionary<int, (string Phase, string Native)>
        {
            [214] = ("Initial", "$A395"),
            [215] = ("WaitForInitialMusicQueue", "$A5A7"),
            [232] = ("FadeInFirstNarration", "$A5BD"),
            [262] = ("LastMetroidIsInCaptivity", "$A5F8"),
            [322] = ("GalaxyIsAtPeace", "$A613"),
            [522] = ("WaitForSecondMusicQueue", "$A639"),
            [554] = ("FourSecondHold", "$A64C"),
            [794] = ("FadeOutFirstNarration", "$A663"),
            [824] = ("SetupPageOne", "$A66F"),
            [825] = ("WaitForPageOneMusicQueue", "$A82B"),
            [857] = ("FadeInPageOne", "$A84A"),
            [887] = ("PageOneText", "$A391"),
            [1718] = ("PageOneAwaitingInput", "$AEB8"),
            [1719] = ("MotherBrainCrossfade", "$B250"),
            [1847] = ("MotherBrainFlashback", "$A391"),
            [2235] = ("PageTwoCrossfade", "$B3F4"),
            [2363] = ("PageTwoText", "$A391"),
            [3011] = ("PageTwoAwaitingInput", "$AF6C"),
            [3012] = ("BabyDiscoveryCrossfade", "$B250"),
            [3140] = ("BabyDiscovery", "$A391"),
            [3816] = ("PageThreeCrossfade", "$B3F4"),
            [3944] = ("PageThreeText", "$A391"),
            [4432] = ("PageThreeAwaitingInput", "$B0F2"),
            [4433] = ("BabyMetroidDeliveryCrossfade", "$B2D2"),
            [4561] = ("BabyMetroidDelivery", "$A391"),
            [4834] = ("PageFourCrossfade", "$B458"),
            [4962] = ("PageFourText", "$A391"),
            [5500] = ("PageFourAwaitingInput", "$B123"),
            [5501] = ("BabyMetroidExaminationCrossfade", "$B2D2"),
            [5629] = ("BabyMetroidExamination", "$A391"),
            [5902] = ("PageFiveCrossfade", "$B458"),
            [6030] = ("PageFiveText", "$A391"),
            [6683] = ("PageFiveAwaitingInput", "$B1DA"),
            [6684] = ("PageSixText", "$A390"),
            [6968] = ("IntroFadeOut", "$B72F"),
            [6998] = ("CeresFlight/Initial", "$BCA0"),
            [6999] = ("CeresFlight/WaitForMusicQueue", "$BDE4"),
            [7023] = ("CeresFlight/FlyingIntoCamera", "$BDF9"),
            [7055] = ("CeresFlight/FlyingTowardCeres", "$BFDA"),
            [7407] = ("CeresFlight/SpaceColonyTitle", "$A38F"),
            [7711] = ("CeresFlight/FadeOut", "$C0C5"),
            [7741] = ("CeresFlight/StartGameAtCeres", "$C100"),
        };

        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        // lsnes leaves the cartridge RAM bsnes allocated, all $FF, for a movie with no SRAM.
        bus.SaveRam.Fill(0xff);
        SuperMetroidGame game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        string? previous = null;
        for (int update = 1; update <= 7742; update++)
        {
            game.Step(inputs.GetValueOrDefault(update));
            if (update == 7741)
                AssertEqual(SuperMetroidGameState.IntroCinematic, game.GameState,
                    "intro update 7741: $8B:C100 has not yet run");
            if (update == 7742)
            {
                AssertEqual(SuperMetroidGameState.SetUpNewGame, game.GameState,
                    "intro update 7742: $8B:C100 writes state $1F in its own dispatch");
                break;
            }

            // The title, options and file screens run until state $1E begins at update 214.
            IntroCinematicState? current = PrivateState.Field<IntroCinematicState?>(game, "intro");
            if (update < 214)
            {
                AssertTrue(current is null, $"intro update {update}: state $1E has not begun");
                continue;
            }
            IntroCinematicState intro = current
                ?? throw new InvalidOperationException($"No intro after update {update}.");
            string phase = intro.Phase.ToString();
            if (PrivateState.Field<object?>(intro, "ceresFlight") is { } flight)
                phase += "/" + PrivateState.Property<object>(flight, "Phase");
            if (phases.TryGetValue(update, out var expected))
                AssertEqual(expected.Phase, phase, $"intro update {update}: native installs $8B:{expected.Native[1..]}");
            else if (phase != previous && previous is not null)
                throw new InvalidOperationException($"Intro entered {phase} after update {update}, where native changes nothing.");
            previous = phase;

            CheckSamusMilestone(intro, update);
        }
        Console.WriteLine("  13% intro: all 42 native phase handoffs, seven Samus milestones and state $1F at update 7742 match.");
    }

    /// <summary>Native Samus values the 13% capture records at intro updates owned by one fix each.</summary>
    private static void CheckSamusMilestone(IntroCinematicState intro, int update)
    {
        SamusState? flashback = PrivateState.Field<SamusState?>(intro, "flashbackSamus");
        SamusState? discovery = PrivateState.Field<object?>(intro, "babyDiscovery") is { } scene
            ? PrivateState.Property<SamusState>(scene, "Samus") : null;
        switch (update)
        {
            case 1809:
                // $90:A3BA: the demo's held shot restarts and holds the standing animation.
                AssertEqual((ushort)0x000f, flashback!.AnimationFrameTimer, "update 1809 standing-shot animation timer");
                AssertEqual((ushort)0, flashback.AnimationFrame, "update 1809 standing-shot animation frame");
                break;
            case 1884:
                // $8B:B90B: Rinka zero, stepped in its spawn pass, strikes Samus on native's update.
                AssertEqual((byte)0x54, (byte)flashback!.Pose, "update 1884 Rinka knockback pose");
                break;
            case 2050:
                // $90:EB02 cleared the run's movement records, so the missile inherits nothing.
                var slots = PrivateState.Field<object>(intro, "flashbackProjectiles");
                SamusProjectileSlot missile = PrivateState.Field<SamusProjectileSlot[]>(slots, "_slots")[0];
                AssertEqual(unchecked((short)0xff00), missile.XVelocity, "update 2050 third missile launch velocity");
                AssertEqual((ushort)0x0088, missile.XPosition, "update 2050 third missile X");
                break;
            case 2111:
                // $91:874B: the demo-end lock keeps the running animation of an unchanged pose.
                AssertEqual((ushort)0x000e, flashback!.AnimationFrameTimer, "update 2111 locked Samus animation timer");
                break;
            case 2400:
                // $8B:B842 cleared $1A57, so Samus is not processed during the page-two text.
                AssertEqual((ushort)0x0003, flashback!.AnimationFrameTimer, "update 2400 frozen Samus animation timer");
                AssertEqual((ushort)0, flashback.AnimationFrame, "update 2400 frozen Samus animation frame");
                break;
            case 3012:
                // $8B:AF99: pose two was already set, so the discovery keeps her animation running.
                AssertEqual((ushort)0x0002, discovery!.AnimationFrameTimer, "update 3012 discovery Samus animation timer");
                AssertEqual((ushort)0, discovery.AnimationFrame, "update 3012 discovery Samus animation frame");
                break;
            case 3990:
                // $91:8682 pointed both state handlers at an RTL during update 3943.
                AssertEqual((ushort)0x0028, discovery!.XPosition, "update 3990 stopped Samus X");
                AssertEqual((ushort)0x5000, discovery.Kinematics.XSubposition, "update 3990 stopped Samus X fraction");
                break;
        }
    }
}
