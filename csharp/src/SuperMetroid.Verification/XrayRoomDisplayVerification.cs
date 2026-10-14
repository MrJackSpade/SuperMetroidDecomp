using static SuiteTimingVerification;
using static XrayExtensionsVerification;
using static XrayOverlaysVerification;
using static XrayRevealVerification;
using static XrayTilemapVerification;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Fx;

/// <summary>Checks X-Ray reveal admission, Fireflea precedence, and room color math.</summary>
internal static class XrayRoomDisplayVerification
{
    /// <summary>Compares every room and boss word with native X-Ray exclusions and expected CGADSUB modes.</summary>
    internal static void VerifyXrayRoomDisplayRules()
    {
        // Keep the literal native comparisons independent of the production catalog.
        // Check every room pointer and every boss word, not only named examples.
        for (int room = 0; room <= ushort.MaxValue; room++)
        {
            bool excluded = room is 0xA66A or 0xCEFB;
            AssertEqual(excluded ? XrayRoomBlendMode.PreserveBackgrounds : XrayRoomBlendMode.RevealBlocks,
                XrayRoomDisplayRules.Select((ushort)room, (RoomFxType)0, 0), "native room reveal admission");
        }
        for (int boss = 0; boss <= ushort.MaxValue; boss++)
        {
            bool excluded = boss is 3 or 6 or 7 or 8 or 10;
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

    /// <summary>Runs the focused X-Ray room display rule lookup.</summary>
    [VerificationCommand("--lookup-stream5-xray-room-rules")]
    internal static int LookupStream5XrayRoomRulesCommand()
    {
        Suite(nameof(VerifyXrayRoomDisplayRules), () => VerifyXrayRoomDisplayRules());
        return 0;
    }

    /// <summary>Runs room admission, reveal-table, extension, tilemap, and overlay checks together.</summary>
    [VerificationCommand("--xray-reveal")]
    internal static int XrayRevealCommand()
    {
        Suite(nameof(VerifyXrayRoomDisplayRules), () => VerifyXrayRoomDisplayRules());
        Suite(nameof(VerifyXrayRevealTable), () => VerifyXrayRevealTable());
        Suite(nameof(VerifyXrayExtensions), () => VerifyXrayExtensions());
        Suite(nameof(VerifyXrayTilemap), () => VerifyXrayTilemap());
        Suite(nameof(VerifyXrayOverlays), () => VerifyXrayOverlays());
        return 0;
    }
}
