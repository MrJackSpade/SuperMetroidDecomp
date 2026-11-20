using SuperMetroid.Core.Gameplay;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Samus.Abilities.XrayExtensionsVerification;
using static SuperMetroid.Verification.Samus.Abilities.XrayOverlaysVerification;
using static SuperMetroid.Verification.Samus.Abilities.XrayRevealVerification;
using static SuperMetroid.Verification.Samus.Abilities.XrayTilemapVerification;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Rooms;

/// <summary>Checks X-Ray reveal admission, Fireflea precedence, and room color math.</summary>
internal static class XrayRoomDisplayVerification
{
    /// <summary>Compares every room and boss with native X-Ray exclusions and expected CGADSUB modes.</summary>
    internal static void VerifyXrayRoomDisplayRules()
    {
        // Keep the literal native comparisons independent of the production catalog.
        // Check every room and every boss word, not only named examples.
        foreach (RoomDefinition room in RetailRooms.All)
        {
            bool excluded = RoomDefinitions.PointerOf(room) is 0xA66A or 0xCEFB;
            AssertEqual(excluded ? XrayRoomBlendMode.PreserveBackgrounds : XrayRoomBlendMode.RevealBlocks,
                XrayRoomDisplayRules.Select(room, null, null), "native room reveal admission");
        }
        // The cartridge compares the boss word with 3, 6, 7, 8 and 10; each boss is checked through its imported word.
        foreach (AreaBoss? boss in Enum.GetValues<AreaBoss>().Select(boss => (AreaBoss?)boss).Append(null))
        {
            int word = boss is { } named ? AreaBossWords.Bosses.Word(named) : 0;
            bool excluded = word is 3 or 6 or 7 or 8 or 10;
            AssertEqual(excluded ? XrayRoomBlendMode.PreserveBackgrounds : XrayRoomBlendMode.RevealBlocks,
                XrayRoomDisplayRules.Select(RoomDefinitions.LandingSite, null, boss), "native boss reveal admission");
            AssertEqual(XrayRoomBlendMode.Fireflea,
                XrayRoomDisplayRules.Select(RoomDefinitions.GlassTunnel, new LatchedFirefleaDarkness(RgbColor.Black), boss), "Fireflea mode takes precedence over exclusions");
        }
        AssertEqual(3171, XrayWindowRenderDefinitions.ActiveBackdrop, "native stage-eight backdrop");
        AssertTrue(!XrayRoomBlendMode.RevealBlocks.KeepsBackgrounds && XrayRoomBlendMode.PreserveBackgrounds.KeepsBackgrounds
            && !XrayRoomBlendMode.Fireflea.KeepsBackgrounds, "only the excluded rooms keep their backgrounds");
        AssertTrue(XrayRoomBlendMode.RevealBlocks.RevealsBlocks && !XrayRoomBlendMode.PreserveBackgrounds.RevealsBlocks
            && !XrayRoomBlendMode.Fireflea.RevealsBlocks, "only the reveal blend reveals blocks");
        AssertTrue(!XrayRoomBlendMode.RevealBlocks.KeepsDarkness && !XrayRoomBlendMode.PreserveBackgrounds.KeepsDarkness
            && XrayRoomBlendMode.Fireflea.KeepsDarkness, "only Fireflea's blend keeps the room's darkness");
        AssertTrue(XrayRoomBlendMode.RevealBlocks.WashesGrey && !XrayRoomBlendMode.PreserveBackgrounds.WashesGrey
            && XrayRoomBlendMode.Fireflea.WashesGrey, "a reveal room and Fireflea's take the X-ray's grey; an excluded room takes no tint");
        foreach (RoomDefinition room in RetailRooms.All)
            AssertEqual(RoomDefinitions.PointerOf(room) is 0xCEFB, room.XrayHidesBackground, "only the Glass Tunnel hides its background under X-ray");
        Console.WriteLine("  X-ray room rules: all rooms, bosses and Fireflea precedence agree with native comparisons.");
    }

    /// <summary>Runs the focused X-Ray room display rule lookup.</summary>
    [VerificationCommand("--lookup-stream5-xray-room-rules")]
    internal static int LookupStream5XrayRoomRulesCommand()
    {
        Suite(nameof(VerifyXrayRoomDisplayRules), () => VerifyXrayRoomDisplayRules());
        return 0;
    }

    /// <summary>Runs room admission, reveal-shape, extension, tilemap, and overlay checks together.</summary>
    [VerificationCommand("--xray-reveal")]
    internal static int XrayRevealChecksCommand()
    {
        Suite(nameof(VerifyXrayRoomDisplayRules), () => VerifyXrayRoomDisplayRules());
        Suite(nameof(VerifyXrayRevealTable), () => VerifyXrayRevealTable());
        Suite(nameof(VerifyXrayExtensions), () => VerifyXrayExtensions());
        Suite(nameof(VerifyXrayTilemap), () => VerifyXrayTilemap());
        Suite(nameof(VerifyXrayOverlays), () => VerifyXrayOverlays());
        return 0;
    }
}
