using static SuperMetroid.Verification.Enemies.Magdollite.MagdollitePhaseDefinitionsVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.Magdollite;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>Reproduces Magdollite's slot-indexed apex threshold read from the native overrun table.</summary>
internal static class MagdolliteApexThresholdVerification
{
    /// <summary>
    /// Confirms issue #1269: the $A8:B259 maximum-rise branch compares arm height with
    /// the threshold table plus the enemy's slot offset ($A0:9073) before loading the growth
    /// index into Y. The recorded Crateria arm in slot two reads $AE9D and grows one final
    /// pillar segment at the apex. For every enemy slot, the cartridge's signed comparison of each rise an arm can
    /// reach the apex with (the maximum rise plus at most one step of the fastest speed record) against the overread
    /// word must give the installed <see cref="MagdolliteApexGrowth"/> service's answer (#1500).
    /// </summary>
    internal static void VerifyMagdolliteApexThreshold()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        VerifyMagdollitePhaseDefinitions(rom);
        const int largestStep = 5;
        MagdolliteApexGrowth installed = InstalledFixtures.EnemyOverreads.MagdolliteApexGrowth;
        for (int slot = 0; slot < EnemyPlaceOrder.Capacity; slot++)
        {
            int address = 0xa80000 | MagdollitePhaseDefinitions.ArmHeightThresholdTable + slot * 0x40;
            ushort overread = ReadMagdolliteWord(rom, address);
            for (int rise = MagdollitePhaseDefinitions.MaximumRise; rise <= MagdollitePhaseDefinitions.MaximumRise + largestStep; rise++)
            {
                AssertEqual(unchecked((short)(rise - overread)) >= 0, installed.GrowsFinalSegment(slot),
                    $"Magdollite apex growth for slot {slot} at a rise of {rise}");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => installed.GrowsFinalSegment(EnemyPlaceOrder.Capacity),
            "Magdollite apex growth beyond the enemy slots");

        VerifyRiseCarry();
        // Slot two (the movie): $006C - $AE9D wraps positive, so BPL grows to phase eight.
        VerifyApex(headSlot: 1, grows: true);
        // Slot one: $006C - $20AF is negative, so the arm stops at phase seven.
        VerifyApex(headSlot: 0, grows: false);
        Console.WriteLine("Magdollite apex threshold: the maximum-rise branch compares against the enemy-index overread.");

        // The rising arm adds the head's 16.16 upward velocity of one and a half pixels: the first update moves two
        // whole pixels and leaves half a pixel, the second carries it and moves one, and the rise displacement
        // accumulates exactly the whole-pixel moves.
        static void VerifyRiseCarry()
        {
            const int onePixel = 65536;
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            // The rising arm's own main AI raises it.
            Action<RoomEnemyPlace, MagdolliteBodyState, SamusState> rise = static (body, _, samus) => body.RunMainAi(samus);
            for (int part = 0; part < 3; part++)
            {
                RoomEnemyPlace member = enemies.Places.InUpdateOrder[part];
                member.SetSpawnParameter1(EnemyDefinitionId.Magdollite, (ushort)part);
                // The head's placement names its arm's speed (#2474): record 24, one and a half pixels an update.
                member.SetSpawnParameter2(EnemyDefinitionId.Magdollite, part == 0 ? (ushort)(24 << 8) : (ushort)0);
                member.Bind(EnemyDefinitionId.Magdollite, member.KeptDefinition);
                member.Position = FixedPosition<RoomPixels>.FromWhole(new(472, 672));
            }
            // The room's population load sets up the three parts in order (#2554).
            _ = enemies.LoadAuthoredPopulation();
            AssertEqual(SixteenSixteenWords.ToDisplacement(onePixel + onePixel / 2),
                enemies.Places.InUpdateOrder[0].HeldOccupant().SpawnSettings<MagdolliteHeadSpawnSettings>().Speed, "Magdollite speed record 24 is one and a half pixels");
            RoomEnemyPlace body = enemies.Places.InUpdateOrder[1];
            var state = enemies.MagdolliteStates[1] as MagdolliteBodyState ?? throw new InvalidOperationException("Magdollite arm has no state.");
            state.Action = MagdolliteBodyAction.Rising;
            state.BodyPhase = MagdolliteRiseStage.OneSegment;
            state.RiseDisplacementY = 0;
            body.Position = FixedPosition<RoomPixels>.FromWhole(new(472, 672));

            rise(body, state, RoomEnemyFrameFixtures.StandaloneSamus());
            AssertEqual(new FixedCoordinate(670, onePixel / 2), body.Position.Y, "Magdollite arm's first rise leaves half a pixel");
            AssertEqual(-2, state.RiseDisplacementY, "Magdollite arm's first rise is two whole pixels");
            rise(body, state, RoomEnemyFrameFixtures.StandaloneSamus());
            AssertEqual(new FixedCoordinate(669, 0), body.Position.Y, "Magdollite arm's second rise carries the half pixel");
            AssertEqual(-3, state.RiseDisplacementY, "Magdollite arm's rise displacement counts whole-pixel moves");
            AssertEqual(MagdolliteRiseStage.OneSegment, state.BodyPhase, "Magdollite arm stays in its first phase below sixteen pixels");
        }

        static void VerifyApex(int headSlot, bool grows)
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            // The rising arm's own main AI raises it.
            Action<RoomEnemyPlace, MagdolliteBodyState, SamusState> rise = static (body, _, samus) => body.RunMainAi(samus);
            // A head in a later place follows an enemy that stands in the room's first place.
            if (headSlot > 0)
                _ = enemies.Places.InUpdateOrder[0].BindPlaced(EnemyPlaceFixtures.StandIn, EnemySpawnSettings.None);
            for (int part = 0; part < 3; part++)
            {
                RoomEnemyPlace member = enemies.Places.InUpdateOrder[headSlot + part];
                member.SetSpawnParameter1(EnemyDefinitionId.Magdollite, (ushort)part);
                member.SetSpawnParameter2(EnemyDefinitionId.Magdollite, 0);
                member.Bind(EnemyDefinitionId.Magdollite, member.KeptDefinition);
                member.Position = member.Position.WithWhole(new(0x01d8, 0x02a0));
            }
            // The room's population load sets up the three parts in order (#2554).
            _ = enemies.LoadAuthoredPopulation();
            // Speed record zero holds the arm where it stands (#2474).
            AssertEqual(SignedSixteenSixteen.Zero, enemies.Places.InUpdateOrder[headSlot].HeldOccupant().SpawnSettings<MagdolliteHeadSpawnSettings>().Speed,
                "Magdollite speed record zero stands still");
            RoomEnemyPlace body = enemies.Places.InUpdateOrder[headSlot + 1];
            var state = enemies.MagdolliteStates[headSlot + 1] as MagdolliteBodyState ?? throw new InvalidOperationException("Verification requires enemies.MagdolliteStates[headSlot + 1].");
            state.Action = MagdolliteBodyAction.Rising;
            state.BodyPhase = MagdolliteRiseStage.At(7);
            body.Position = body.Position.WithWholeY(0x02a0);
            state.RiseDisplacementY = -MagdollitePhaseDefinitions.MaximumRise;

            rise(body, state, RoomEnemyFrameFixtures.StandaloneSamus());
            string context = $"Magdollite body in slot {body.Ordinal}";
            AssertEqual(MagdolliteBodyAction.HoldingAtApex, state.Action, $"{context} reaches its apex");
            AssertEqual(MagdolliteRiseStage.At(grows ? 8 : 7), state.BodyPhase, $"{context} apex phase");
            AssertEqual((ushort)(grows ? 0x02a8 : 0x02a0), body.Position.Y.Whole, $"{context} apex Y");
        }
    }


    /// <summary>Runs the focused slot-indexed Magdollite apex comparison.</summary>
    [VerificationCommand("--magdollite-apex-threshold")]
    internal static int MagdolliteApexThresholdCommand()
    {
        VerifyMagdolliteApexThreshold();
        return 0;
    }
}
