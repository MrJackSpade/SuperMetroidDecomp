using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectRunHeaderAssertions;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Rooms;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Areas.Maridia;

/// <summary>Checks the Maridia elevatube PLM's physical draw and short instruction program.</summary>
internal static class LookupElevatubeVerification
{
    /// <summary>Compares the single installed elevatube draw with its native run, tile, and displacement.</summary>
    internal static void VerifyElevatubePhysicalDraw(CartridgeImportAddressSpace rom)
    {
        // What the block shows is its installed picture, which the elevatube picture check compares with the cartridge.
        AssertEqual(RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom, 0x849369)).Collision, MaridiaElevatubeObjectDefinitions.Collision, "Elevatube native collision");
        var all = RoomObjectMaridiaElevatubeVisualFiles.AllDraws.ToArray();
        AssertEqual(1, all.Length, "Elevatube single export");
        RoomObjectShotBlockDrawDefinitions.DrawList selected = all.Single(draw => RoomObjectMaridiaElevatubeVisualFiles.Family.Ids[draw.Draw] == "elevatube-block");
        foreach (var draw in new[] {RoomObjectMaridiaElevatubeVisualFiles.BlockDrawList,all[0],selected})
        {
            AssertEqual((ushort)0x9367, RoomObjectDrawAddressCodec.Of(draw.Draw), "Elevatube original shared pointer");
            AssertEqual(1, draw.Runs.Length, "Elevatube one run");
            var run = draw.Runs.Span[0];
            AssertRunHeader(ReadSamusEaterObjectWord(rom, 0x849367), run, "Elevatube native run header");
            AssertEqual(1, run.Count, "Elevatube one word");
            AssertEqual(RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom, 0x849369)).Collision, run.Collisions.Span[0], "Elevatube DTO native collision");
            AssertEqual(unchecked((sbyte)ElevatubePhysicalDrawRomByte(rom, 0x84936b)), RoomObjectRunContinuationCodecConstants.X(draw, 0), "Elevatube original X");
            AssertEqual(unchecked((sbyte)ElevatubePhysicalDrawRomByte(rom, 0x84936c)), RoomObjectRunContinuationCodecConstants.Y(draw, 0), "Elevatube original Y");
        }
        foreach (string id in new[] {"", "ELEVATUBE-BLOCK", "elevatube-block "})
            AssertTrue(!RoomObjectMaridiaElevatubeVisualFiles.Family.Ids.Values.Contains(id, StringComparer.Ordinal), "Elevatube unknown visual ID");
    }

    /// <summary>Reads a native elevatube draw byte for the independent geometry comparison.</summary>
    private static byte ElevatubePhysicalDrawRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks the elevatube's three compiled control words.</summary>
    internal static void VerifyElevatubeProgramControls(CartridgeImportAddressSpace rom) => VerifyElevatubeProgramField(rom, false);
    /// <summary>Checks the elevatube's one drawing word.</summary>
    internal static void VerifyElevatubeProgramDraw(CartridgeImportAddressSpace rom) => VerifyElevatubeProgramField(rom, true);

    /// <summary>Compares either control or draw words over the complete program lookup domain.</summary>
    private static void VerifyElevatubeProgramField(CartridgeImportAddressSpace rom, bool draw)
    {
        ushort[] controls = [0xb8f0,0xb8f4,0xb8f7];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            bool owned = raw == 0xb8f2 || controls.Contains((ushort)raw);
            AssertEqual(owned, MaridiaElevatubeObjectDefinitionsConstants.TryReadMechanicsWord((ushort)raw, out ushort value), "Elevatube complete word domain");
            if (!owned) AssertEqual((ushort)0, value, "Elevatube missing word zero");
            else if (draw == (raw == 0xb8f2))
            {
                AssertEqual(ReadSamusEaterObjectWord(rom, 0x840000 | raw), value, "Elevatube native program field");
            }
        }
    }

    /// <summary>Checks the sole sound byte and rejection of every other sixteen-bit address.</summary>
    internal static void VerifyElevatubeProgramSound(CartridgeImportAddressSpace rom)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            AssertEqual(raw == 0xb8f6, MaridiaElevatubeObjectDefinitionsConstants.TryReadMechanicsByte((ushort)raw, out byte value), "Elevatube sole sound byte domain");
            AssertEqual(raw == 0xb8f6 ? ElevatubeProgramSoundRomByte(rom, 0x84b8f6) : (byte)0, value, "Elevatube native sound or missing zero");
        }
    }

    /// <summary>Reads the native elevatube sound byte for comparison.</summary>
    private static byte ElevatubeProgramSoundRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
