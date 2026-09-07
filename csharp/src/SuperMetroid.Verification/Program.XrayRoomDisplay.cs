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
        Console.WriteLine("  X-ray room rules: all room/boss words and Fireflea precedence agree with native comparisons.");
    }
}
