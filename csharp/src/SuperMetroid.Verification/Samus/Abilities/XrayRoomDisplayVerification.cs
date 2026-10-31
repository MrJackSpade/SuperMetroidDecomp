using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Samus.Abilities.XrayExtensionsVerification;
using static SuperMetroid.Verification.Samus.Abilities.XrayOverlaysVerification;
using static SuperMetroid.Verification.Samus.Abilities.XrayRevealVerification;
using static SuperMetroid.Verification.Samus.Abilities.XrayTilemapVerification;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Checks X-Ray reveal admission, Fireflea precedence, and room color math.</summary>
internal static class XrayRoomDisplayVerification
{
    /// <summary>Compares every room and boss with native X-Ray exclusions and expected CGADSUB modes.</summary>
    internal static void VerifyXrayRoomDisplayRules()
    {
        // Keep the literal native comparisons independent of the production catalog.
        // Check every room and every boss word, not only named examples.
        foreach (RoomHeaderDefinition room in RoomHeaderDefinitions.All)
        {
            bool excluded = RoomHeaderDefinitions.PointerOf(room) is 0xA66A or 0xCEFB;
            AssertEqual(excluded ? XrayRoomBlendMode.PreserveBackgrounds : XrayRoomBlendMode.RevealBlocks,
                XrayRoomDisplayRules.Select(room, 0, null), "native room reveal admission");
        }
        // The cartridge compares the boss word with 3, 6, 7, 8 and 10; each boss is checked through its imported word.
        foreach (AreaBoss? boss in Enum.GetValues<AreaBoss>().Select(boss => (AreaBoss?)boss).Append(null))
        {
            int word = boss is { } named ? AreaBossWords.Bosses.Word(named) : 0;
            bool excluded = word is 3 or 6 or 7 or 8 or 10;
            AssertEqual(excluded ? XrayRoomBlendMode.PreserveBackgrounds : XrayRoomBlendMode.RevealBlocks,
                XrayRoomDisplayRules.Select(RoomHeaderDefinitions.LandingSite, 0, boss), "native boss reveal admission");
            AssertEqual(XrayRoomBlendMode.Fireflea,
                XrayRoomDisplayRules.Select(RoomHeaderDefinitions.GlassTunnel, (RoomFxType)0x24, boss), "Fireflea mode takes precedence over exclusions");
        }
        AssertEqual(3171, XrayRoomDisplayRules.ActiveBackdrop, "native stage-eight backdrop");
        const ColorBlend revealLayers = ColorBlend.Bg1 | ColorBlend.Bg2 | ColorBlend.Sprites | ColorBlend.Backdrop;
        const ColorBlend excludedLayers = ColorBlend.Bg1 | ColorBlend.Backdrop;
        AssertEqual(revealLayers | ColorBlend.Half, XrayRoomDisplayRules.Blend(XrayRoomBlendMode.RevealBlocks, false), "reveal blend");
        AssertEqual(revealLayers | ColorBlend.Half | ColorBlend.Subtract, XrayRoomDisplayRules.Blend(XrayRoomBlendMode.RevealBlocks, true), "subtractive reveal blend");
        AssertEqual(excludedLayers | ColorBlend.Half, XrayRoomDisplayRules.Blend(XrayRoomBlendMode.PreserveBackgrounds, false), "excluded blend");
        AssertEqual(excludedLayers | ColorBlend.Half | ColorBlend.Subtract, XrayRoomDisplayRules.Blend(XrayRoomBlendMode.PreserveBackgrounds, true), "subtractive excluded blend");
        AssertEqual(revealLayers | ColorBlend.Subtract, XrayRoomDisplayRules.Blend(XrayRoomBlendMode.Fireflea, false), "Fireflea forces subtraction without halving");
        Console.WriteLine("  X-ray room rules: all rooms, bosses and Fireflea precedence agree with native comparisons.");
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
