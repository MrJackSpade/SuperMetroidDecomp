using System.Text.Json.Nodes;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Verification.Harness;

/// <summary>The installed JSON shape of one PLM visual cell, for checks that author an override file by hand.</summary>
internal static class InstalledAppearanceJson
{
    /// <summary>One unmirrored cell showing <paramref name="block"/>, as the visual files serialize a block appearance.</summary>
    internal static JsonObject Cell(int block) =>
        new() { ["block"] = block, ["mirroring"] = nameof(BlockMirroring.None) };
}
