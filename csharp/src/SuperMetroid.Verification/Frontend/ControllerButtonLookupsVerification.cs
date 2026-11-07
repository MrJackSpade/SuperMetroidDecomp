using SuperMetroid.Diagnostics.Input;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend;

/// <summary>Compares controller button lookup and default-binding behavior with retail code and tables.</summary>
internal static class ControllerButtonLookupsVerification
{
    /// <summary>Checks seven assignable buttons, inverse lookup, swaps, and rejection of invalid permutations.</summary>
    internal static void VerifyAssignableControllerButtons(ISnesAddressSpace rom)
    {
        // Independent original bytes, never the replacement cases, supply both directions.
        ControllerButton[] original = Enumerable.Range(0, 9)
            .Select(index => ControllerWordCodec.FromJoypadWord(ReadVerificationWord(rom, 0x82f575 + index * 2), "native assignable button")).ToArray();
        AssertEqual(7, ControllerBindings.AssignableButtonCount, "native set-binding scan count");
        AssertEqual((ushort)12, ReadVerificationWord(rom, 0x82f6ba), "native reverse scan byte offset");
        for (int index = 0; index < 7; index++)
            AssertEqual(original[index], ControllerBindings.AssignableButton(index),
                $"native assignable button {index}");
        int buttonBits = ControllerWordCodec.ToJoypadWord(ControllerButtons.All);
        for (int word = 0; word <= ushort.MaxValue; word++)
        {
            // A word with bits no button produces names no set of buttons.
            if ((word & ~buttonBits) != 0) continue;
            ControllerButton buttons = ControllerWordCodec.FromJoypadWord((ushort)word, "assignable inverse word");
            int nativeIndex = Array.IndexOf(original, buttons, 0, 7);
            AssertEqual(nativeIndex < 0 ? null : nativeIndex,
                ControllerBindings.AssignableButtonIndex(buttons),
                $"assignable inverse {buttons}");
        }
        foreach (int index in new[] { int.MinValue, -1, 7, 8, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => ControllerBindings.AssignableButton(index),
                $"unsupported button index {index}");

        var bindings = new ControllerBindings(original[0], original[1], original[2],
            original[3], original[4], original[5], original[6]);
        for (int action = 0; action < 7; action++)
        for (int requested = 0; requested < 7; requested++)
        {
            ControllerBindings result = bindings.AssignAndSwap(action, original[requested]);
            for (int row = 0; row < 7; row++)
                AssertEqual(original[row == action ? requested : row == requested ? action : row],
                    result[row], $"button swap {action}/{requested} row {row}");
            AssertTrue(result.IsRetailPermutation, "swapped buttons remain a permutation");
        }
        foreach (ControllerButton rejected in new ControllerButton[] { ControllerButton.None, original[7], original[8],
                     original[0] | original[1], ControllerButton.Start, ControllerButtons.All })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => bindings.AssignAndSwap(0, rejected),
                $"reject nonassignable {rejected}");
            AssertTrue(!(bindings with { Shoot = rejected }).IsRetailPermutation,
                "nonassignable word invalidates permutation");
        }
        AssertTrue(!(bindings with { Shoot = bindings.Jump }).IsRetailPermutation,
            "duplicate button invalidates permutation");
    }

    /// <summary>Reads native default binding instructions and checks their action-to-WRAM mapping.</summary>
    internal static void VerifyDefaultControllerButtons(ISnesAddressSpace rom)
    {
        // Parse the seven original LDA-immediate/STA-absolute pairs, including their
        // actual WRAM destinations. Menu order differs from native instruction order.
        var original = new Dictionary<ushort, ushort>();
        for (int address = 0x81b324; address <= 0x81b348; address += 6)
        {
            AssertEqual((byte)0xa9, rom.ReadCartridgeByte(address), "default binding LDA immediate");
            AssertEqual((byte)0x8d, rom.ReadCartridgeByte(address + 3), "default binding STA absolute");
            original.Add(ReadVerificationWord(rom, address + 4), ReadVerificationWord(rom, address + 1));
        }
        ushort[] destinations = [0x09b2, 0x09b4, 0x09b6, 0x09ba, 0x09b8, 0x09be, 0x09bc];
        for (int row = 0; row < destinations.Length; row++)
            AssertEqual(ControllerWordCodec.FromJoypadWord(original[destinations[row]], "native default binding"), ControllerBindings.Default[row],
                $"default action row {row} WRAM {destinations[row]:X4}");
        foreach (int row in new[] { int.MinValue, -1, 7, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = ControllerBindings.Default[row]; },
                $"invalid default action row {row}");
        AssertTrue(ControllerBindings.Default.IsRetailPermutation, "native defaults form a permutation");
    }
}
