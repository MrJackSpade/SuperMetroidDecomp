using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyXrayRoomDisplayRules()
    {
        // Keep the literal native comparisons independent of the production catalog.
        // Check every room pointer and every boss word, not only named examples.
        for (int room = 0; room <= ushort.MaxValue; room++)
        {
            bool excluded = room == 0xA66A || room == 0xCEFB;
            AssertEqual(excluded ? XrayRoomBlendMode.PreserveBackgrounds : XrayRoomBlendMode.RevealBlocks,
                XrayRoomDisplayRules.Select((ushort)room, (RoomFxType)0, 0), "native room reveal admission");
        }
        for (int boss = 0; boss <= ushort.MaxValue; boss++)
        {
            bool excluded = boss == 3 || boss == 6 || boss == 7 || boss == 8 || boss == 10;
            AssertEqual(excluded ? XrayRoomBlendMode.PreserveBackgrounds : XrayRoomBlendMode.RevealBlocks,
                XrayRoomDisplayRules.Select(0, (RoomFxType)0, (ushort)boss), "native boss reveal admission");
            AssertEqual(XrayRoomBlendMode.Fireflea,
                XrayRoomDisplayRules.Select(0xCEFB, (RoomFxType)0x24, (ushort)boss), "Fireflea mode takes precedence over exclusions");
        }
        AssertEqual((ushort)3171, XrayRoomDisplayRules.ActiveBackdrop, "native stage-eight backdrop");
        AssertEqual((byte)0x73, (byte)XrayRoomDisplayRules.ColorMath(XrayRoomBlendMode.RevealBlocks, false), "reveal CGADSUB");
        AssertEqual((byte)0xF3, (byte)XrayRoomDisplayRules.ColorMath(XrayRoomBlendMode.RevealBlocks, true), "subtractive reveal CGADSUB");
        AssertEqual((byte)0x61, (byte)XrayRoomDisplayRules.ColorMath(XrayRoomBlendMode.PreserveBackgrounds, false), "excluded CGADSUB");
        AssertEqual((byte)0xE1, (byte)XrayRoomDisplayRules.ColorMath(XrayRoomBlendMode.PreserveBackgrounds, true), "subtractive excluded CGADSUB");
        AssertEqual((byte)0xB3, (byte)XrayRoomDisplayRules.ColorMath(XrayRoomBlendMode.Fireflea, false), "Fireflea forces subtraction without halving");
        Console.WriteLine("  X-ray room rules: all room/boss words and Fireflea precedence agree with native comparisons.");
    }
}
