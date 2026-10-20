using static SuperMetroid.Verification.Enemies.Magdollite.MagdollitePhaseDefinitionsVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.Magdollite;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Reproduces Magdollite's slot-indexed apex threshold read from the native overrun table.</summary>
internal static class MagdolliteApexThresholdVerification
{
    /// <summary>
    /// Confirms issue #1269: the $A8:B259 maximum-rise branch compares arm height with
    /// <c>MagdolliteArmHeightThreshold + EnemyIndex</c> ($A0:9073) before loading the growth
    /// index into Y. The recorded Crateria arm in slot two reads $AE9D and grows one final
    /// pillar segment at the apex. For every enemy slot, the cartridge's signed comparison of each rise an arm can
    /// reach the apex with (the maximum rise plus at most one step of the fastest speed record) against the overread
    /// word must give the production table's answer.
    /// </summary>
    internal static void VerifyMagdolliteApexThreshold()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        VerifyMagdollitePhaseDefinitions(rom);
        const int largestStep = 5;
        for (int slot = 0; slot < MagdollitePhaseDefinitions.ApexEnemySlotCount; slot++)
        {
            int address = 0xa80000 | MagdollitePhaseDefinitions.ArmHeightThresholdTable + slot * 0x40;
            ushort overread = ReadMagdolliteWord(rom, address);
            for (int rise = MagdollitePhaseDefinitions.MaximumRise; rise <= MagdollitePhaseDefinitions.MaximumRise + largestStep; rise++)
            {
                AssertEqual(unchecked((short)(rise - overread)) >= 0, MagdollitePhaseDefinitions.ApexGrowsFinalPhase(slot),
                    $"Magdollite apex growth for slot {slot} at a rise of {rise}");
            }
        }
        AssertThrows<InvalidDataException>(
            () => MagdollitePhaseDefinitions.ApexGrowsFinalPhase(MagdollitePhaseDefinitions.ApexEnemySlotCount),
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
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            Action<RoomEnemySlot, MagdolliteEnemyState, SamusState?> rise = enemies.MagdolliteBehavior.RunMagdolliteBodyRising;
            for (int part = 0; part < 3; part++)
            {
                RoomEnemySlot member = enemies.SlotTable.Slots[part];
                member.Bind(EnemyDefinitionId.Magdollite, member.Definition);
                member.SetSpawnParameter1((ushort)part);
                member.SetSpawnParameter2(0);
                member.Position = FixedPosition<RoomPixels>.FromWhole(new(472, 672));
                enemies.MagdolliteBehavior.InitializeMagdollite(member, null);
            }
            MagdolliteEnemyState head = enemies.MagdolliteStates[0] ?? throw new InvalidOperationException("Magdollite head has no state.");
            head.UpwardVelocity = SnesSignedSixteenSixteen.Zero - SnesSignedSixteenSixteen.FromRaw(onePixel + onePixel / 2);
            RoomEnemySlot body = enemies.SlotTable.Slots[1];
            MagdolliteEnemyState state = enemies.MagdolliteStates[1] ?? throw new InvalidOperationException("Magdollite arm has no state.");
            state.Function = MagdolliteEnemyFunction.BodyRising;
            state.BodyPhase = 1;
            state.RiseDisplacement = Offset16<RoomPixels>.Zero;
            body.Position = FixedPosition<RoomPixels>.FromWhole(new(472, 672));

            rise(body, state, null);
            AssertEqual(new SnesFixedPosition(670, onePixel / 2), body.Position.Y, "Magdollite arm's first rise leaves half a pixel");
            AssertEqual(new Offset16<RoomPixels>(0, -2), state.RiseDisplacement, "Magdollite arm's first rise is two whole pixels");
            rise(body, state, null);
            AssertEqual(new SnesFixedPosition(669, 0), body.Position.Y, "Magdollite arm's second rise carries the half pixel");
            AssertEqual(new Offset16<RoomPixels>(0, -3), state.RiseDisplacement, "Magdollite arm's rise displacement counts whole-pixel moves");
            AssertEqual(1, state.BodyPhase, "Magdollite arm stays in its first phase below sixteen pixels");
        }

        static void VerifyApex(int headSlot, bool grows)
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            Action<RoomEnemySlot, SamusState?> initialize = enemies.MagdolliteBehavior.InitializeMagdollite;
            Action<RoomEnemySlot, MagdolliteEnemyState, SamusState?> rise = enemies.MagdolliteBehavior.RunMagdolliteBodyRising;
            for (int part = 0; part < 3; part++)
            {
                RoomEnemySlot member = enemies.SlotTable.Slots[headSlot + part];
                member.Bind(EnemyDefinitionId.Magdollite, member.Definition);
                member.SetSpawnParameter1((ushort)part);
                member.SetSpawnParameter2(0);
                member.Position = member.Position.WithWhole(new(0x01d8, 0x02a0));
                initialize(member, null);
            }
            MagdolliteEnemyState head = (enemies.MagdolliteStates[headSlot] ?? throw new InvalidOperationException("Verification requires enemies.MagdolliteStates[headSlot]."));
            (head ?? throw new InvalidOperationException("Verification requires head.")).UpwardVelocity = SnesSignedSixteenSixteen.Zero;
            RoomEnemySlot body = enemies.SlotTable.Slots[headSlot + 1];
            MagdolliteEnemyState state = (enemies.MagdolliteStates[headSlot + 1] ?? throw new InvalidOperationException("Verification requires enemies.MagdolliteStates[headSlot + 1]."));
            (state ?? throw new InvalidOperationException("Verification requires state.")).Function = MagdolliteEnemyFunction.BodyRising;
            state.BodyPhase = 7;
            body.Position = body.Position.WithWholeY(0x02a0);
            state.RiseDisplacement = new(0, -MagdollitePhaseDefinitions.MaximumRise);

            rise(body, state, null);
            string context = $"Magdollite body in slot {body.SlotIndex}";
            AssertEqual(MagdolliteEnemyFunction.NoOpAtApex, state.Function, $"{context} reaches its apex");
            AssertEqual(grows ? 8 : 7, state.BodyPhase, $"{context} apex phase");
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
