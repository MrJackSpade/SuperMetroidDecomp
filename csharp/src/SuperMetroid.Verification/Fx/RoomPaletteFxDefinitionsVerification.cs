using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Fx.PaletteFx.LookupPaletteFxAreasVerification;
using static SuperMetroid.Verification.Fx.PaletteFx.LookupPaletteFxDispatchVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.CartridgeTables;
namespace SuperMetroid.Verification.Fx;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Progression;

/// <summary>Checks compiled palette-FX definitions, area selection, and production spawn metadata.</summary>
internal static class RoomPaletteFxDefinitionsVerification
{
    /// <summary>Audits 63 entries and eight area lists, then checks spawns and room loads without metadata reads.</summary>
    internal static void VerifyRoomPaletteFxDefinitions(CartridgeImportAddressSpace rom)
    {

        ushort[] pointers =
        [
            .. DefinitionRange(
                RoomPaletteFxDefinitions.CinematicDefinitionsBegin,
                RoomPaletteFxDefinitions.CinematicDefinitionsEnd),
            .. DefinitionRange(
                RoomPaletteFxDefinitions.RoomDefinitionsBegin,
                RoomPaletteFxDefinitions.RoomDefinitionsEnd),
            .. DefinitionRange(
                RoomPaletteFxDefinitions.TourianDefinitionsBegin,
                RoomPaletteFxDefinitions.TourianDefinitionsEnd),
        ];
        Suite(nameof(VerifyPaletteFxDispatchDomain), () => VerifyPaletteFxDispatchDomain());
        Suite(nameof(VerifyPaletteFxSetupSelection), () => VerifyPaletteFxSetupSelection(rom));
        Suite(nameof(VerifyPaletteFxInitialListSelection), () => VerifyPaletteFxInitialListSelection(rom));
        AssertEqual(63, pointers.Length, "compiled palette-FX definition count");

        var forbidden = new HashSet<int>();
        int genericCompiledPrograms = 0;
        int specializedCompiledPrograms = 0;
        foreach (ushort pointer in pointers)
        {
            int address = RoomFxBankSourceDefinitions.PaletteFx | pointer;
            for (int index = 0; index < RoomPaletteFxDefinitions.DefinitionByteCount; index++)
                forbidden.Add(address + index);

            RoomPaletteFxDefinition definition = RoomPaletteFxSelection.Get(pointer);
            if (definition.InitialAnimation == PaletteFxSegment.HyperBeam0)
            {
                specializedCompiledPrograms++;
            }
            else
            {
                AssertTrue(Enum.IsDefined(definition.InitialAnimation),
                    $"palette-FX ${pointer:X4} begins a typed program");
                genericCompiledPrograms++;
            }
        }
        AssertEqual(62, genericCompiledPrograms,
            "generic palette-FX definitions with typed initial programs");
        AssertEqual(1, specializedCompiledPrograms,
            "specialized Hyper Beam palette-FX definition count");

        Suite(nameof(VerifyPaletteFxAreaListPointers), () => VerifyPaletteFxAreaListPointers(rom));
        Suite(nameof(VerifyPaletteFxAreaSelections), () => VerifyPaletteFxAreaSelections(rom));
        for (int area = 0; area < RoomPaletteFxDefinitions.AreaCount; area++)
        {
            int pointerAddress = RoomFxNativeSources.AreaPaletteFxObjectListPointers + area * 2;
            forbidden.Add(pointerAddress);
            forbidden.Add(pointerAddress + 1);
            ushort nativeList = CartridgeImportSource.Require(rom).ReadCartridgeWord(pointerAddress);
            for (int bit = 0; bit < RoomPaletteFxDefinitions.DefinitionsPerArea; bit++)
            {
                int entryAddress = RoomFxRomDataBanksTooling.RoomDefinitions |
                    unchecked((ushort)(nativeList + bit * 2));
                forbidden.Add(entryAddress);
                forbidden.Add(entryAddress + 1);
            }
        }

        var guardedRom = NewRoomPaletteFxDefinitionGuard(rom, forbidden);
        Suite(nameof(VerifyPaletteFxDispatchSpawns), () => VerifyPaletteFxDispatchSpawns(pointers));
        Suite(nameof(VerifyColourEffectTurnOrder), () => VerifyColourEffectTurnOrder());

        for (int area = 0; area < SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Count; area++)
        for (int bit = 0; bit < RoomPaletteFxDefinitions.DefinitionsPerArea; bit++)
        {
            PaletteFxObject effect = AreaPaletteFxObjects.GetAreaDefinition(area, bit);
            var definition = new RoomFxRecordDefinition(BaseYPosition: 0, TargetYPosition: 0, YVelocity: default, Timer: 0,
                Type: RoomFxType.None, VisorAnimation: RoomVisorAnimation.Steady, EffectBlend: null, Liquid: null,
                PaletteEffects: [effect], AnimatedTiles: [], PaletteBlend: RoomFxPaletteBlend.None);
            var systemSession = new PaletteFxSessionState();
            var system = RoomPaletteFxSystem.ForDefinition(systemSession, definition,
                equippedItems: 0, areaMiniBossDefeated: false);
            AssertEqual(
                PaletteFxObjectAccess.NativeAddress(effect),
                ActiveColourEffect(system).Id,
                $"area {area} bit {bit} production room load");
        }

        var hyperBeam = new HyperBeamPaletteFxState();
        hyperBeam.Spawn();
        hyperBeam.Step(new SamusColours());
        AssertEqual(0, hyperBeam.CurrentFrameIndex,
            "specialized Hyper Beam owner executes with its definition bytes forbidden");

        AssertThrows<InvalidDataException>(
            () => PaletteFxObjectAccess.FromNative(0),
            "zero palette-FX definition is outside the authored domain");
        AssertThrows<InvalidDataException>(
            () => RoomPaletteFxSelection.Get(
                unchecked((ushort)(RoomPaletteFxDefinitions.RoomDefinitionsBegin + 1))),
            "unaligned palette-FX definition is outside the authored domain");
        AssertThrows<ArgumentOutOfRangeException>(
            () => AreaPaletteFxObjects.GetAreaDefinition(-1, 0),
            "negative palette-FX area index");
        AssertThrows<ArgumentOutOfRangeException>(
            () => AreaPaletteFxObjects.GetAreaDefinition(0, 8),
            "out-of-range palette-FX bit index");

        Console.WriteLine(
            "Palette-FX definitions: 63 setup/list records, eight native area lists, " +
            "64 area selections, every production spawn, all retail room-load selections, " +
            "62 compiled generic program entries, and the specialized Hyper Beam owner pass " +
            "with fixed metadata reads forbidden.");
    }

    /// <summary>Checks each definition's started effect: its identity, countdown, condition and initial program.</summary>
    internal static void VerifyPaletteFxDispatchSpawns(IEnumerable<ushort> pointers)
    {
        foreach (ushort pointer in pointers)
        {
            RoomPaletteFxDefinition definition = RoomPaletteFxSelection.Get(pointer);
            var systemSession = new PaletteFxSessionState();
            var system = new RoomPaletteFxSystem(systemSession);
            system.Start(pointer, equippedItems: 0);
            RoomColourEffect effect = ActiveColourEffect(system);
            AssertEqual(pointer, effect.Id, $"${pointer:X4} production ID");
            AssertEqual(1, effect.UpdatesUntilNextStep,
                $"${pointer:X4} production countdown");
            // Every native start installs a pre-instruction that does nothing, the intro's included: no condition.
            AssertEqual(null, effect.Condition, $"${pointer:X4} production condition");
            Animation<PaletteFxSegment> expected = PaletteFxAnimations.StartingWith(definition.Start == ColourEffectStart.ChoosesHeatAnimationForSuit
                ? PaletteFxHeatAnimations.Start(PaletteFxHeatSuit.Power)
                : definition.InitialAnimation);
            AssertTrue(ReferenceEquals(expected, effect.Animation), $"${pointer:X4} production initial animation");
            AssertEqual(AnimationPlace.StartOfAnimation, effect.Place, $"${pointer:X4} production starts its animation");
        }

    }

    /// <summary>
    /// Checks the colour effects' turn order (#1554, #1598): effects take turns in the order they started, an effect that
    /// ends leaves the turns, an effect started later joins after every running effect, and any number of effects run at
    /// once.
    /// </summary>
    internal static void VerifyColourEffectTurnOrder()
    {
        var conditions = new PaletteFxConditions { SamusY = 0, EquippedItems = 0, AreaMiniBossDefeated = false };
        var system = new RoomPaletteFxSystem(new PaletteFxSessionState());
        system.Start(PaletteFxObject.TitleScreenFlickeringDisplays, 0);
        system.Start(PaletteFxObject.FadeInSuperMetroidTitleLogo, 0);
        system.Start(PaletteFxObject.TitleScreenBabyMetroidTubeLight, 0);
        AssertTrue(system.effects.Select(effect => effect.Effect).SequenceEqual(
            [PaletteFxObject.TitleScreenFlickeringDisplays, PaletteFxObject.FadeInSuperMetroidTitleLogo, PaletteFxObject.TitleScreenBabyMetroidTubeLight]),
            "effects take turns in start order");

        // The title logo's fade ends by itself; the loops keep running.
        int updates = 0;
        while (system.IsRunning(PaletteFxObject.FadeInSuperMetroidTitleLogo))
        {
            AssertTrue(++updates <= 64, "the title logo's fade ends");
            system.StepScreen(conditions);
        }
        AssertTrue(system.effects.Select(effect => effect.Effect).SequenceEqual(
            [PaletteFxObject.TitleScreenFlickeringDisplays, PaletteFxObject.TitleScreenBabyMetroidTubeLight]),
            "an ended effect leaves the turns");

        system.Start(PaletteFxObject.FadeInNintendoCopyright, 0);
        AssertTrue(system.effects.Select(effect => effect.Effect).SequenceEqual(
            [PaletteFxObject.TitleScreenFlickeringDisplays, PaletteFxObject.TitleScreenBabyMetroidTubeLight, PaletteFxObject.FadeInNintendoCopyright]),
            "a later start joins after every running effect");
        system.StepScreen(conditions);
        PaletteFxColours[] shown = [.. system.Shown.Select(record => record.Colours)];
        // The title logo's fade ran to its end, so its final look lies under every running effect (#1597).
        AssertTrue(shown.SequenceEqual([PaletteFxColours.TitleLogo, PaletteFxColours.TitleScreenDisplays,
                PaletteFxColours.BabyMetroidTubeLight, PaletteFxColours.NintendoCopyright]),
            "the settled logo lies first, then the running effects' records in the order they started");
        AssertEqual<PaletteFxFrame?>(new PaletteFxFrame(PaletteFxSegment.CommonFadeInLogo, 0),
            system.FrameOf(PaletteFxColours.NintendoCopyright), "the copyright shows its first frame on its first turn");

        // No cap holds the effects back (#1598): a ninth effect starts like the first.
        var many = new RoomPaletteFxSystem(new PaletteFxSessionState());
        for (int started = 0; started < 9; started++)
            many.Start(PaletteFxObject.TitleScreenFlickeringDisplays, 0);
        AssertEqual(9, many.effects.Count, "any number of effects run at once");
    }

    /// <summary>Enumerates native fixed-size definition records within one bank-$8D range.</summary>
    private static IEnumerable<ushort> DefinitionRange(ushort first, ushort last)
    {
        for (int pointer = first; pointer <= last; pointer +=
            RoomPaletteFxDefinitions.DefinitionByteCount)
        {
            yield return unchecked((ushort)pointer);
        }
    }

    /// <summary>The one effect running after a production start or room load.</summary>
    private static RoomColourEffect ActiveColourEffect(RoomPaletteFxSystem system) => system.effects is [RoomColourEffect effect]
        ? effect
        : throw new InvalidOperationException($"Expected one running colour effect, found {system.effects.Count}.");

    /// <summary>Rejects runtime reads of compiled definition and area-list metadata bytes.</summary>
    internal static CartridgeReadGuard NewRoomPaletteFxDefinitionGuard(ISnesAddressSpace source, HashSet<int> forbidden) =>
        new CartridgeReadGuard(source).Forbid(forbidden.Contains,
            address => $"Palette-FX runtime reread compiled metadata byte ${address:X6}.");

    /// <summary>Loads retail data and runs the palette-FX metadata audit from the command dispatcher.</summary>
    [VerificationCommand("--room-palette-fx-definitions")]
    internal static int RoomPaletteFxDefinitionsCommand()
    {
        Suite(nameof(VerifyRoomPaletteFxDefinitions), () => VerifyRoomPaletteFxDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
