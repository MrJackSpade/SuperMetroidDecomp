using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Input;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Verification.Frontend.CeresCinematics;
using SuperMetroid.Verification.Fx.PaletteFx;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>
/// Checks the old Mother Brain fight's background lights (#1733) against the cartridge: the crossfade into a flashback
/// spawns them at its end while the Samus display flag has Samus draw first, which the Mother Brain flashback's setup sets
/// and the discovery's does not; their own check deletes them on the update the cinematic starts page two. The intro
/// starts them on the update the crossfade into the fight ends, shows the cartridge list's record on every update until
/// page two's update, holds that record through it, shows none from page two's crossfade on, and shades the fight's room
/// with the record; each installed record shade shows the record's colours.
/// </summary>
internal static class IntroFightLightsVerification
{
    /// <summary>The crossfade-into-gameplay function's test of the Samus display flag's sign, <c>$8B:B2C8</c>.</summary>
    private const int FlagTest = 0x8bb2c8;
    /// <summary>The crossfade-into-gameplay function's spawn of the lights, <c>$8B:B2CA</c>.</summary>
    private const int LightsSpawn = 0x8bb2ca;
    /// <summary>The operand of the Mother Brain flashback setup's store of the Samus display flag, at <c>$8B:AF62</c>.</summary>
    private const int MotherBrainFlagOperand = 0x8baf63;
    /// <summary>The operand of the discovery setup's store of the Samus display flag, at <c>$8B:B00F</c>.</summary>
    private const int DiscoveryFlagOperand = 0x8bb010;
    /// <summary>The operand of the shared crossfade setup's selection of the crossfade into gameplay, at <c>$8B:B031</c>.</summary>
    private const int CrossfadeSelectionOperand = 0x8bb032;
    /// <summary>The crossfade-into-gameplay cinematic function, which spawns the lights at its end.</summary>
    private const ushort CrossfadeIntoGameplay = 0xb250;
    /// <summary>The operand of page two's start instruction's selection of page two's function, at <c>$8B:B336</c>.</summary>
    private const int PageTwoSelectionOperand = 0x8bb337;
    /// <summary>The cinematic function of page two.</summary>
    private const ushort PageTwoFunction = 0xb35f;
    /// <summary>The lights' definition in bank $8D.</summary>
    private const ushort LightsDefinition = 0xe1bc;
    /// <summary>The lights' setup, which installs their check.</summary>
    private const ushort LightsSetup = 0xe204;
    /// <summary>The operand of the setup's load of the check, at <c>$8D:E204</c>.</summary>
    private const int CheckLoadOperand = 0x8de205;
    /// <summary>The lights' check, which deletes them while page two is the cinematic function.</summary>
    private const ushort Check = 0xe20b;
    /// <summary>The operand of the check's comparison with page two's function, at <c>$8D:E20E</c>.</summary>
    private const int CheckComparisonOperand = 0x8de20f;
    /// <summary>The Samus display flag Mother Brain's flashback sets: negative, Samus drawn first.</summary>
    private const ushort SamusDrawnFirst = 0xffff;
    /// <summary>The Samus display flag the discovery sets: positive, its objects drawn first.</summary>
    private const ushort ObjectsDrawnFirst = 0x0001;
    /// <summary>The branch-if-plus opcode the flag's sign test is.</summary>
    private const byte BranchIfPlus = 0x10;

    /// <summary>
    /// The fight lights follow the cartridge from the intro's first update through the start of the discovery: the spawn's
    /// wiring and condition, their start as the crossfade into the fight ends, the record on every update and the room's
    /// shading, their end at page two, and no lights through the discovery's crossfade; then the installed record shades.
    /// </summary>
    internal static void VerifyOldMotherBrainFightLights(CartridgeImportAddressSpace bus)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        AssertEqual(CrossfadeIntoGameplay, cartridge.ReadCartridgeWord(CrossfadeSelectionOperand), "both flashbacks' setups select the crossfade into gameplay");
        AssertEqual(BranchIfPlus, CeresColourEffectsVerification.Byte(cartridge, FlagTest), "the crossfade's end skips the lights while the flag is positive");
        CeresColourEffectsVerification.AssertSpawn(cartridge, LightsSpawn, LightsDefinition, IntroCinematicDefinitions.FightLights, "the crossfade's end");
        AssertEqual(SamusDrawnFirst, cartridge.ReadCartridgeWord(MotherBrainFlagOperand), "Mother Brain's flashback has Samus drawn first");
        AssertEqual(ObjectsDrawnFirst, cartridge.ReadCartridgeWord(DiscoveryFlagOperand), "the discovery has its objects drawn first");
        AssertEqual(LightsSetup, cartridge.ReadCartridgeWord(PaletteFxListWords.ListBank | LightsDefinition), "the lights' definition runs their setup");
        AssertEqual(Check, cartridge.ReadCartridgeWord(CheckLoadOperand), "the lights' setup installs their check");
        AssertEqual(PageTwoFunction, cartridge.ReadCartridgeWord(CheckComparisonOperand), "the lights' check compares with page two's function");
        AssertEqual(PageTwoFunction, cartridge.ReadCartridgeWord(PageTwoSelectionOperand), "Mother Brain's page-two instruction selects page two's function");

        var audio = AudioQueues.Standalone();
        IntroCinematicState intro = InstalledFixtures.CreateIntro(audio);
        NativeColourEffectRun? lights = null;
        ColourEffectFrame? held = null;
        bool pageTwo = false, discovered = false;
        for (int update = 0; !discovered; update++)
        {
            AssertTrue(update < 20_000, "the intro reaches the discovery");
            IntroCinematicPhase before = intro.Phase;
            // A new press every so often turns each page once its text has typed.
            intro.Step(update % 47 == 0 ? ControllerButton.A : ControllerButton.None);
            audio.AdvanceFrame(default);
            discovered = intro.Phase == IntroCinematicPhase.BabyDiscovery;
            if (before == IntroCinematicPhase.MotherBrainCrossfade && intro.Phase == IntroCinematicPhase.MotherBrainFlashback)
                lights = new NativeColourEffectRun(cartridge, LightsDefinition);
            bool pageTwoUpdate = !pageTwo && intro.CurrentFlashbackMotherBrain?.PageTwoRequested == true;
            pageTwo |= pageTwoUpdate;
            ColourEffectFrame? shown = intro.ColourEffects.FrameOf(ColourEffectColours.OldMotherBrainBackgroundLights);
            if (lights is null || (pageTwo && !pageTwoUpdate))
            {
                AssertTrue(shown is null, $"intro update {update} ({intro.Phase}) shows no fight lights");
                AssertTrue(!intro.FightRoomShading.Shades.Any(shade => shade.Effect.Contains(nameof(ColourEffectColours.OldMotherBrainBackgroundLights), StringComparison.Ordinal)),
                    $"intro update {update} shades the fight's room without the lights");
                continue;
            }
            ColourEffectFrame frame = shown ?? throw new InvalidOperationException($"Intro update {update} shows no fight lights.");
            if (pageTwoUpdate)
            {
                // The check deletes the lights before their countdown runs: the record they showed stays.
                AssertEqual(held, frame, $"page two's update {update} holds the lights' last record");
            }
            else
            {
                lights.Step();
                AssertEqual(lights.Record, NativeColourEffectRun.RecordOf(frame), $"intro update {update} shows the record the cartridge's lights list shows");
            }
            held = frame;
            AssertEqual(1, intro.FightRoomShading.Shades.Count(shade => shade == ColourEffectShades.Of(ColourEffectColours.OldMotherBrainBackgroundLights, frame)),
                $"intro update {update} shades the fight's room with the lights' record");
        }
        AssertTrue(lights is not null && pageTwo, "the intro runs the fight lights from the fight to page two");

        VerifyInstalledLightShades(bus);
        Console.WriteLine("  Old Mother Brain fight lights: they start as the crossfade into the fight ends, show the cartridge list's record " +
            "on every update, hold it on page two's update and show none from page two's crossfade through the discovery; their shades show the records' colours.");
    }

    /// <summary>Runs the fight lights check against the retail cartridge.</summary>
    [VerificationCommand("--intro-fight-lights")]
    internal static int IntroFightLightsCommand()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyOldMotherBrainFightLights), () => VerifyOldMotherBrainFightLights(bus));
        return 0;
    }

    /// <summary>The installed scene pictures hold every record shade of the lights, coloured as the cartridge's records.</summary>
    private static void VerifyInstalledLightShades(CartridgeImportAddressSpace bus)
    {
        var native = new ReferencePaletteFxColorSource(bus);
        SuperMetroid.AssetExtraction.Fx.Artwork.IPaletteFxColorSource installed = InstalledFixtures.ImportMaps.RoomPaletteFx;
        int recoloured = CeresColourEffectsVerification.InstalledShadeRecolourings(InstalledFixtures.SpriteSheets.Get(SpriteSheetId.IntroScenePictures).Image,
            PaletteFxScreenShades.Of([IntroCinematicDefinitions.FightLights], ColourEffectColours.OldMotherBrainBackgroundLights, IntroSheetInks.FightLights, installed),
            PaletteFxScreenShades.Of([IntroCinematicDefinitions.FightLights], ColourEffectColours.OldMotherBrainBackgroundLights, IntroSheetInks.FightLights, native),
            "the fight's lights");
        AssertTrue(recoloured > 0, "the installed Mother Brain room shows its fight's lights");
    }
}
