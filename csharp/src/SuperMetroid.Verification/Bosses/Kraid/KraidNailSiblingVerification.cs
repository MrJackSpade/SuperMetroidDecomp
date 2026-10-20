using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Kraid fingernail launch velocities and sibling-flag selection against native tables.</summary>
internal static class KraidNailSiblingVerification
{
    /// <summary>Compares the fractional X launch word across sibling velocity signs and RNG offsets.</summary>
    internal static void VerifyKraidNailLaunchXFraction(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidNailLaunchField), () => VerifyKraidNailLaunchField(rom, 0, value => value.X.Fraction));

    /// <summary>Compares the whole X launch word across sibling velocity signs and RNG offsets.</summary>
    internal static void VerifyKraidNailLaunchXWhole(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidNailLaunchField), () => VerifyKraidNailLaunchField(rom, 2, value => value.X.WholeWord));

    /// <summary>Compares the fractional Y launch word across sibling velocity signs and RNG offsets.</summary>
    internal static void VerifyKraidNailLaunchYFraction(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidNailLaunchField), () => VerifyKraidNailLaunchField(rom, 4, value => value.Y.Fraction));

    /// <summary>Compares the whole Y launch word across sibling velocity signs and RNG offsets.</summary>
    internal static void VerifyKraidNailLaunchYWhole(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidNailLaunchField), () => VerifyKraidNailLaunchField(rom, 6, value => value.Y.WholeWord));

    /// <summary>Reads the native indirect launch record selected by sibling sign and RNG bits for one field.</summary>
    private static void VerifyKraidNailLaunchField(SuperMetroidAddressSpace rom, int fieldOffset,
        Func<FixedVector<RoomPixels>, ushort> field)
    {
        ushort Word(int address) => SnesLittleEndian.Word(KraidNailLaunchFieldRomByte(rom, address), KraidNailLaunchFieldRomByte(rom, address + 1));
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            // Native CMP #0 / BPL and RNG & 6 select independently. Cover their
            // Cartesian product, including every duplicate indirect record.
            int table = (raw & 0x8000) != 0 ? 0xa7be3e : 0xa7be46;
            for (int randomOffset = 0; randomOffset <= 6; randomOffset += 2)
            {
                int record = 0xa70000 | Word(table + randomOffset);
                AssertEqual(Word(record + fieldOffset),
                    field(KraidNailLaunchDefinitions.FromSiblingVelocity((ushort)raw)),
                    $"Kraid launch field {fieldOffset}, sibling {raw}, RNG offset {randomOffset}");
            }
        }
    }

    /// <summary>Reads one native launch-table byte for the field comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid nail launch field")]
    private static byte KraidNailLaunchFieldRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks both nail slots, sibling flags, RNG preservation, and the actual launch or waiting phase.</summary>
    internal static void VerifyKraidNailSibling(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyKraidNailLaunchXFraction), () => VerifyKraidNailLaunchXFraction(rom));
        Suite(nameof(VerifyKraidNailLaunchXWhole), () => VerifyKraidNailLaunchXWhole(rom));
        Suite(nameof(VerifyKraidNailLaunchYFraction), () => VerifyKraidNailLaunchYFraction(rom));
        Suite(nameof(VerifyKraidNailLaunchYWhole), () => VerifyKraidNailLaunchYWhole(rom));
        ushort Word(int a) => SnesLittleEndian.Word(KraidNailSiblingRomByte(rom, a), KraidNailSiblingRomByte(rom, a + 1));
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        var state = new KraidEnemyState();
        enemies.KraidBehavior.Access.Kraid = state;
        enemies.SlotTable.Slots[0].Bind(EnemyDefinitionId.Kraid, enemies.SlotTable.Slots[0].Definition);
        ushort random = 0;
        enemies.VerificationServices().ReadRandom = () => random;
        enemies.VerificationServices().NextRandom = () => throw new InvalidOperationException("Nail initialization must not advance RNG.");
        Action<RoomEnemySlot, KraidPartState> initialize = enemies.KraidBehavior.Projectiles.InitializeKraidNailFlight;
        foreach (int slot in new[] { 6, 7 })
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        foreach (bool siblingAlternate in new[] { false, true })
        {
            var nail = enemies.SlotTable.Slots[slot];
            var sibling = enemies.SlotTable.Slots[13 - slot];
            var part = state.Parts[slot];
            random = (ushort)raw;
            state.Parts[sibling.SlotIndex].NailVelocity = state.Parts[sibling.SlotIndex].NailVelocity.WithY(state.Parts[sibling.SlotIndex].NailVelocity.Y.WithWholePixels(unchecked((short)raw)));
            state.Parts[nail.SlotIndex].NailVelocity = state.Parts[nail.SlotIndex].NailVelocity.WithY(state.Parts[nail.SlotIndex].NailVelocity.Y.WithWholePixels(unchecked((short)~raw)));
            state.Parts[sibling.SlotIndex].AlternateSpawn = siblingAlternate;
            part.AlternateSpawn = !siblingAlternate;
            bool diagonal = (random & 1) == 0 || siblingAlternate;
            int table = state.Parts[sibling.SlotIndex].NailVelocity.Y.WholePixels < 0 ? 0xa7be3e : 0xa7be46;
            int pointer = 0xa70000 | Word(table + (random & 6));
            state.Parts[nail.SlotIndex].NailVelocity = state.Parts[nail.SlotIndex].NailVelocity.WithX(state.Parts[nail.SlotIndex].NailVelocity.X.WithFraction(ushort.MaxValue)).WithFractionY(ushort.MaxValue);
            initialize(nail, part);
            AssertEqual((ushort)0, state.Parts[nail.SlotIndex].NailVelocity.X.Fraction, "Launch clears old X fraction");
            AssertEqual((ushort)0, state.Parts[nail.SlotIndex].NailVelocity.Y.Fraction, "Launch clears old Y fraction");
            AssertEqual(diagonal ? Word(pointer + 6) : (ushort)0, state.Parts[nail.SlotIndex].NailVelocity.Y.WholeWord, "Nail launch Y velocity comes from sibling sign");
            AssertEqual(diagonal ? Word(pointer + 2) : (ushort)1, state.Parts[nail.SlotIndex].NailVelocity.X.WholeWord, "Nail launch X velocity and horizontal override");
            AssertEqual(!diagonal, part.AlternateSpawn, "Nail spawn mode consults sibling flag, writes own flag");
            AssertEqual(siblingAlternate, state.Parts[sibling.SlotIndex].AlternateSpawn, "Sibling flag remains unchanged");
            AssertEqual((ushort)raw, state.Parts[sibling.SlotIndex].NailVelocity.Y.WholeWord, "Sibling velocity remains unchanged");
            AssertEqual((diagonal ? KraidAiFunction.FingernailFire : KraidAiFunction.FingernailWaitForLint), state.Parts[nail.SlotIndex].Function, "Sibling choice selects actual launch phase");
        }
        Console.WriteLine("Kraid nail sibling: both slots, all RNG/sign words and both sibling spawn modes match native launch handoff without ROM access.");
    }

    /// <summary>Reads native nail launch words solely as the verification oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid nail sibling")]
    private static byte KraidNailSiblingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
