using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Kraid fingernail launch velocities and sibling-flag selection against native tables.</summary>
internal static class KraidNailSiblingVerification
{
    /// <summary>Compares the fractional X launch word across sibling velocity signs and RNG offsets.</summary>
    internal static void VerifyKraidNailLaunchXFraction(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyKraidNailLaunchField), () => VerifyKraidNailLaunchField(rom, 0, value => value.X.Fraction));

    /// <summary>Compares the whole X launch word across sibling velocity signs and RNG offsets.</summary>
    internal static void VerifyKraidNailLaunchXWhole(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyKraidNailLaunchField), () => VerifyKraidNailLaunchField(rom, 2, value => value.X.UnsignedWholePixels));

    /// <summary>Compares the fractional Y launch word across sibling velocity signs and RNG offsets.</summary>
    internal static void VerifyKraidNailLaunchYFraction(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyKraidNailLaunchField), () => VerifyKraidNailLaunchField(rom, 4, value => value.Y.Fraction));

    /// <summary>Compares the whole Y launch word across sibling velocity signs and RNG offsets.</summary>
    internal static void VerifyKraidNailLaunchYWhole(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyKraidNailLaunchField), () => VerifyKraidNailLaunchField(rom, 6, value => value.Y.UnsignedWholePixels));

    /// <summary>Reads the native indirect launch record selected by sibling sign and RNG bits for one field.</summary>
    private static void VerifyKraidNailLaunchField(CartridgeImportAddressSpace rom, int fieldOffset,
        Func<FixedVector<RoomPixels>, ushort> field)
    {
        ushort Word(int address) => LittleEndianBytes.Word(KraidNailLaunchFieldRomByte(rom, address), KraidNailLaunchFieldRomByte(rom, address + 1));
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            // Native CMP #0 / BPL and RNG & 6 select independently. Cover their
            // Cartesian product, including every duplicate indirect record.
            int table = (raw & 0x8000) != 0 ? 0xa7be3e : 0xa7be46;
            for (int randomOffset = 0; randomOffset <= 6; randomOffset += 2)
            {
                int record = 0xa70000 | Word(table + randomOffset);
                AssertEqual(Word(record + fieldOffset),
                    field(KraidNailLaunch.FromSiblingVelocity((ushort)raw)),
                    $"Kraid launch field {fieldOffset}, sibling {raw}, RNG offset {randomOffset}");
            }
        }
    }

    /// <summary>Reads one native launch-table byte for the field comparison.</summary>
    private static byte KraidNailLaunchFieldRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>
    /// Checks both nail slots, sibling flags, RNG preservation, the actual launch or waiting phase, and that a launch
    /// after its sibling was killed reads the sibling as stopped (<see cref="KilledFingernailStops"/>).
    /// </summary>
    internal static void VerifyKraidNailSibling(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyKraidNailLaunchXFraction), () => VerifyKraidNailLaunchXFraction(rom));
        Suite(nameof(VerifyKraidNailLaunchXWhole), () => VerifyKraidNailLaunchXWhole(rom));
        Suite(nameof(VerifyKraidNailLaunchYFraction), () => VerifyKraidNailLaunchYFraction(rom));
        Suite(nameof(VerifyKraidNailLaunchYWhole), () => VerifyKraidNailLaunchYWhole(rom));
        ushort Word(int a) => LittleEndianBytes.Word(KraidNailSiblingRomByte(rom, a), KraidNailSiblingRomByte(rom, a + 1));
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        var state = new KraidEnemyState();
        enemies.KraidBehavior.Access.Kraid = state;
        enemies.KraidBehavior.Parts.JoinInRoomOrder(enemies.SlotTable);
        enemies.SlotTable.Slots[0].Bind(EnemyDefinitionId.Kraid, enemies.SlotTable.Slots[0].Definition);
        ushort random = 0;
        enemies.VerificationServices().ReadRandom = () => random;
        enemies.VerificationServices().NextRandom = () => throw new InvalidOperationException("Nail initialization must not advance RNG.");
        Action<RoomEnemySlot, KraidNailState> initialize = enemies.KraidBehavior.Projectiles.InitializeKraidNailFlight;
        // Both fingernails live in their places, so each launch reads its sibling's speed.
        foreach (int place in new[] { 6, 7 })
        {
            RoomEnemySlot living = enemies.SlotTable.Slots[place];
            living.SetSpeciesState<KraidPartState>(enemies.KraidBehavior.Parts.NailStateOf(state, living));
        }
        foreach (int slot in new[] { 6, 7 })
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        foreach (bool siblingAlternate in new[] { false, true })
        {
            var nail = enemies.SlotTable.Slots[slot];
            var sibling = enemies.SlotTable.Slots[13 - slot];
            var part = enemies.KraidBehavior.Parts.NailStateOf(state, nail);
            random = (ushort)raw;
            enemies.KraidBehavior.Parts.NailStateOf(state, sibling).NailVelocity = enemies.KraidBehavior.Parts.NailStateOf(state, sibling).NailVelocity.WithY(enemies.KraidBehavior.Parts.NailStateOf(state, sibling).NailVelocity.Y.WithWholePixels(unchecked((short)raw)));
            enemies.KraidBehavior.Parts.NailStateOf(state, nail).NailVelocity = enemies.KraidBehavior.Parts.NailStateOf(state, nail).NailVelocity.WithY(enemies.KraidBehavior.Parts.NailStateOf(state, nail).NailVelocity.Y.WithWholePixels(unchecked((short)~raw)));
            enemies.KraidBehavior.Parts.NailStateOf(state, sibling).AlternateSpawn = siblingAlternate;
            part.AlternateSpawn = !siblingAlternate;
            bool diagonal = (random & 1) == 0 || siblingAlternate;
            int table = enemies.KraidBehavior.Parts.NailStateOf(state, sibling).NailVelocity.Y.WholePixels < 0 ? 0xa7be3e : 0xa7be46;
            int pointer = 0xa70000 | Word(table + (random & 6));
            enemies.KraidBehavior.Parts.NailStateOf(state, nail).NailVelocity = enemies.KraidBehavior.Parts.NailStateOf(state, nail).NailVelocity.WithX(enemies.KraidBehavior.Parts.NailStateOf(state, nail).NailVelocity.X.WithFraction(ushort.MaxValue)).WithFractionY(ushort.MaxValue);
            initialize(nail, part);
            AssertEqual((ushort)0, enemies.KraidBehavior.Parts.NailStateOf(state, nail).NailVelocity.X.Fraction, "Launch clears old X fraction");
            AssertEqual((ushort)0, enemies.KraidBehavior.Parts.NailStateOf(state, nail).NailVelocity.Y.Fraction, "Launch clears old Y fraction");
            AssertEqual(diagonal ? Word(pointer + 6) : (ushort)0, enemies.KraidBehavior.Parts.NailStateOf(state, nail).NailVelocity.Y.UnsignedWholePixels, "Nail launch Y velocity comes from sibling sign");
            AssertEqual(diagonal ? Word(pointer + 2) : (ushort)1, enemies.KraidBehavior.Parts.NailStateOf(state, nail).NailVelocity.X.UnsignedWholePixels, "Nail launch X velocity and horizontal override");
            AssertEqual(!diagonal, part.AlternateSpawn, "Nail spawn mode consults sibling flag, writes own flag");
            AssertEqual(siblingAlternate, enemies.KraidBehavior.Parts.NailStateOf(state, sibling).AlternateSpawn, "Sibling flag remains unchanged");
            AssertEqual((ushort)raw, enemies.KraidBehavior.Parts.NailStateOf(state, sibling).NailVelocity.Y.UnsignedWholePixels, "Sibling velocity remains unchanged");
            AssertEqual((diagonal ? KraidPhase.FingernailFire : KraidPhase.FingernailWaitForLint), enemies.KraidBehavior.Parts.NailStateOf(state, nail).Phase, "Sibling choice selects actual launch phase");
        }

        // A killed sibling has stopped: the launch reads no speed from it, whatever speed it last moved at.
        RoomEnemySlot firstNail = enemies.SlotTable.Slots[6];
        RoomEnemySlot secondNail = enemies.SlotTable.Slots[7];
        KraidNailState killedSibling = enemies.KraidBehavior.Parts.NailStateOf(state, secondNail);
        killedSibling.NailVelocity = FixedVector<RoomPixels>.FromStoredParts(0, 0, 0x8000, 0);
        killedSibling.AlternateSpawn = false;
        random = 0;
        secondNail.Clear();
        initialize(firstNail, enemies.KraidBehavior.Parts.NailStateOf(state, firstNail));
        AssertEqual(KraidNailLaunch.FromSiblingVelocity(0).Y.UnsignedWholePixels, enemies.KraidBehavior.Parts.NailStateOf(state, firstNail).NailVelocity.Y.UnsignedWholePixels,
            "a launch after its sibling was killed reads the sibling as stopped");
        AssertTrue(KraidNailLaunch.FromSiblingVelocity(0).Y.UnsignedWholePixels != KraidNailLaunch.FromSiblingVelocity(0x8000).Y.UnsignedWholePixels,
            "a stopped sibling launches its pair differently from one moving down");

        // A killed sibling took no alternate spawn, whatever its mark last held, so a draw that allows it sends the launch
        // to wait for the lint (#1620).
        killedSibling.AlternateSpawn = true;
        random = 1;
        initialize(firstNail, enemies.KraidBehavior.Parts.NailStateOf(state, firstNail));
        AssertEqual(KraidPhase.FingernailWaitForLint, enemies.KraidBehavior.Parts.NailStateOf(state, firstNail).Phase,
            "a launch after its sibling was killed reads the sibling as having taken no alternate spawn");
        Console.WriteLine("Kraid nail sibling: both slots, all RNG/sign words and both sibling spawn modes match native launch handoff without ROM access, and a killed sibling reads as stopped.");
    }

    /// <summary>Reads native nail launch words solely as the verification oracle.</summary>
    private static byte KraidNailSiblingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
