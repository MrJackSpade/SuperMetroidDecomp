using SuperMetroid.Diagnostics.Input;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend;

/// <summary>Compares controller button lookup and default-binding behavior with retail code and tables.</summary>
internal static class ControllerButtonLookupsVerification
{
    /// <summary>The actions in the order the bindings' constructor takes them, which the checks pair with native buttons.</summary>
    private static readonly ControllerAction[] ConstructorOrder =
    [
        ControllerAction.Shoot, ControllerAction.Jump, ControllerAction.Dash, ControllerAction.ItemSelect,
        ControllerAction.ItemCancel, ControllerAction.AimUp, ControllerAction.AimDown,
    ];

    /// <summary>Whether <paramref name="bindings"/> bind <paramref name="buttons"/> to Shoot, rather than refusing it as unassignable.</summary>
    private static bool BindingAccepts(ControllerBindings bindings, ControllerButton buttons)
    {
        try
        {
            _ = bindings.AssignAndSwap(ControllerAction.Shoot, buttons);
            return true;
        }
        catch (ArgumentOutOfRangeException refused) when (refused.ParamName == "physicalButton")
        {
            return false;
        }
    }

    /// <summary>Checks seven assignable buttons, inverse lookup, swaps, and rejection of invalid permutations.</summary>
    internal static void VerifyAssignableControllerButtons(ISnesAddressSpace rom)
    {
        // Independent original bytes, never the replacement cases, supply both directions.
        ControllerButton[] original = Enumerable.Range(0, 9)
            .Select(index => ControllerWordCodec.FromJoypadWord(ReadVerificationWord(rom, 0x82f575 + index * 2), "native assignable button")).ToArray();
        AssertEqual(7, ControllerBindings.AssignableButtons.Length, "native set-binding scan count");
        AssertEqual((ushort)12, ReadVerificationWord(rom, 0x82f6ba), "native reverse scan byte offset");
        for (int index = 0; index < 7; index++)
            AssertEqual(original[index], ControllerBindings.AssignableButtons[index],
                $"native assignable button {index}");
        var bindings = new ControllerBindings(new Dictionary<ControllerAction, ControllerButton>
        {
            [ControllerAction.Shoot] = original[0], [ControllerAction.Jump] = original[1], [ControllerAction.Dash] = original[2],
            [ControllerAction.ItemSelect] = original[3], [ControllerAction.ItemCancel] = original[4], [ControllerAction.AimUp] = original[5],
            [ControllerAction.AimDown] = original[6],
        });
        int buttonBits = ControllerWordCodec.ToJoypadWord(ControllerButtons.All);
        // A button set is assignable when the options menu can bind it: binding any other refuses it.
        for (int word = 0; word <= ushort.MaxValue; word++)
        {
            // A word with bits no button produces names no set of buttons.
            if ((word & ~buttonBits) != 0) continue;
            ControllerButton buttons = ControllerWordCodec.FromJoypadWord((ushort)word, "assignable inverse word");
            int nativeIndex = Array.IndexOf(original, buttons, 0, 7);
            AssertEqual(nativeIndex >= 0, BindingAccepts(bindings, buttons), $"assignable inverse {buttons}");
        }

        for (int action = 0; action < ConstructorOrder.Length; action++)
        for (int requested = 0; requested < 7; requested++)
        {
            ControllerBindings result = bindings.AssignAndSwap(ConstructorOrder[action], original[requested]);
            for (int other = 0; other < ConstructorOrder.Length; other++)
                AssertEqual(original[other == action ? requested : other == requested ? action : other],
                    result[ConstructorOrder[other]], $"button swap {ConstructorOrder[action]}/{requested} {ConstructorOrder[other]}");
            AssertEqual(result, result.RequireUniquePermutation(), "swapped buttons remain a permutation");
        }
        foreach (ControllerButton rejected in new ControllerButton[] { ControllerButton.None, original[7], original[8],
                     original[0] | original[1], ControllerButton.Start, ControllerButtons.All })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => bindings.AssignAndSwap(ControllerAction.Shoot, rejected),
                $"reject nonassignable {rejected}");
            AssertThrows<InvalidDataException>(() => bindings.With(ControllerAction.Shoot, rejected).RequireUniquePermutation(),
                "nonassignable button invalidates permutation");
        }
        AssertThrows<InvalidDataException>(() => bindings.With(ControllerAction.Shoot, bindings[ControllerAction.Jump]).RequireUniquePermutation(),
            "duplicate button invalidates permutation");
        // Bindings that leave a button to no action are not a permutation; a swap onto that button is refused rather than
        // leaving the action's old button bound twice.
        AssertThrows<InvalidOperationException>(() => bindings.With(ControllerAction.Shoot, bindings[ControllerAction.Jump]).AssignAndSwap(ControllerAction.Jump, original[0]),
            "a swap onto a button no action holds is refused");
    }

    /// <summary>Reads native default binding instructions and checks their action-to-WRAM mapping.</summary>
    internal static void VerifyDefaultControllerButtons(ISnesAddressSpace rom)
    {
        // Parse the seven original LDA-immediate/STA-absolute pairs, including their
        // actual WRAM destinations. The bindings' order differs from native instruction order.
        var original = new Dictionary<ushort, ushort>();
        for (int address = 0x81b324; address <= 0x81b348; address += 6)
        {
            AssertEqual((byte)0xa9, rom.ReadCartridgeByte(address), "default binding LDA immediate");
            AssertEqual((byte)0x8d, rom.ReadCartridgeByte(address + 3), "default binding STA absolute");
            original.Add(ReadVerificationWord(rom, address + 4), ReadVerificationWord(rom, address + 1));
        }
        var destinations = new Dictionary<ControllerAction, ushort>
        {
            [ControllerAction.Shoot] = 0x09b2,
            [ControllerAction.Jump] = 0x09b4,
            [ControllerAction.Dash] = 0x09b6,
            [ControllerAction.ItemSelect] = 0x09ba,
            [ControllerAction.ItemCancel] = 0x09b8,
            [ControllerAction.AimUp] = 0x09be,
            [ControllerAction.AimDown] = 0x09bc,
        };
        foreach (ControllerAction action in Enum.GetValues<ControllerAction>())
            AssertEqual(ControllerWordCodec.FromJoypadWord(original[destinations[action]], "native default binding"), ControllerBindings.Default[action],
                $"default action {action} WRAM {destinations[action]:X4}");
        AssertEqual(ControllerBindings.Default, ControllerBindings.Default.RequireUniquePermutation(), "native defaults form a permutation");
    }
}
