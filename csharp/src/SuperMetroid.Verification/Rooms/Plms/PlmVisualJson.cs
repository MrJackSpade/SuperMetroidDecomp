using System.Text.Json.Nodes;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>Builds the installed visual JSON's block appearance objects for override fixtures.</summary>
internal static class PlmVisualJson
{
    /// <summary>The JSON object of an unmirrored block, as the installed visual files store it.</summary>
    internal static JsonObject Appearance(int block) => Appearance(RoomBlockAppearance.Of(block));

    /// <summary>The JSON object of <paramref name="appearance"/>, as the installed visual files store it.</summary>
    internal static JsonObject Appearance(RoomBlockAppearance appearance) => new()
    {
        ["block"] = appearance.Block,
        ["mirroring"] = appearance.Mirroring.ToString(),
    };
}
