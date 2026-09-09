using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyWindowRegisterCache()
    {
        // Literal addresses independently match the native CPU STY probe. Check
        // every neighboring byte, not merely the selected field's final value.
        for (int slot = 0; slot < 5; slot++)
        {
            var cache = new GameplayWindowRegisterCache();
            for (ushort address = 0x60; address <= 0x6D; address++)
                cache.WriteByte(address, (byte)(address + 0x40));
            cache.WriteWord((ushort)(0x60 + slot * 2), 0x1234);
            for (ushort address = 0x60; address <= 0x6D; address++)
                AssertEqual((byte)(address == 0x60 + slot * 2 ? 0x34 :
                    address == 0x61 + slot * 2 ? 0x12 : address + 0x40),
                    cache.ReadByte(address), "native STY byte ownership");
        }

        var state = new GameplayWindowRegisterCache();
        for (ushort address = 0x60; address <= 0x6D; address++)
            state.WriteByte(address, (byte)(address + 0x40));
        state.InitializeWindowAndScreenSelection();
        byte[] expected = [0, 0, 0, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 19, 0xAA, 4, 0, 0];
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], state.ReadByte((ushort)(0x60 + index)),
                "blending initializes only its owned bytes");

        state.WriteWord(0x60, 0x1234);
        state.WriteWord(0x68, 0xABCD);
        state.WriteByte(0x6C, 0x91);
        state.LatchNmi(false);
        AssertEqual(default(GameplayWindowRegisterSnapshot), state.Displayed, "lag before first upload");
        AssertEqual((byte)0xAA, state.ReadByte(0x6A), "lag preserves gameplay_TM");
        state.LatchNmi(true);
        var first = state.Displayed;
        AssertEqual(new SnesWindowRegisters(0x34, 0x12, 0, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xCD),
            first.Windows, "all uploaded window bytes");
        AssertEqual((byte)0xAB, first.MainScreen, "upload retains unused TM bits");
        AssertEqual((byte)0xAB, state.ReadByte(0x6A), "accepted NMI copies gameplay_TM");
        AssertEqual((byte)0x91, first.MainScreenWindow, "upload retains literal TMW");
        AssertEqual((byte)4, first.Subscreen, "TS does not read gameplay_TM gap");
        AssertEqual((byte)0, first.SubscreenWindow, "TSW uploads independently");
        state.InitializeWindowAndScreenSelection();
        state.LatchNmi(false);
        AssertEqual(first, state.Displayed, "later cached writes and lag cannot mutate displayed frame");
        AssertEqual((byte)0xAB, state.ReadByte(0x6A), "initialization does not overwrite gameplay_TM");
        state.LatchNmi(true);
        AssertEqual((byte)19, state.Displayed.MainScreen, "next accepted NMI publishes new TM");
        AssertEqual((byte)0xCD, state.Displayed.Windows.ObjectColorLogic, "logic survives next initialization");
        AssertThrows<ArgumentOutOfRangeException>(() => state.WriteWord(0x6D, 0xFFFF), "reject cross-owner word");
        AssertEqual((byte)0, state.ReadByte(0x6D), "invalid word has no partial mutation");
        AssertThrows<ArgumentOutOfRangeException>(() => state.WriteByte(0x5F, 1), "reject prior owner");
        Console.WriteLine("Window cache: five native STY targets, selective initialization and accepted/lagged NMI publication verified.");
    }
}
