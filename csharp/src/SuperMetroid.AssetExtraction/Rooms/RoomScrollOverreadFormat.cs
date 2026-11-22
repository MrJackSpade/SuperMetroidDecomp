using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Defines the installed file of what each explicit scroll layout's load writes past the room's screens.</summary>
internal static partial class RoomScrollOverreadFormat
{
    /// <summary>Supported schema revision. Revision one holds the cells past the screens of every explicit layout (#2356).</summary>
    internal const int Version = 1;

    /// <summary>JSON filename holding the decoded cells.</summary>
    internal const string FileName = "room-scroll-overreads.json";

    /// <summary>
    /// Shared camel-case, case-insensitive JSON options that reject unknown fields and name each cell's scroll state; the
    /// file is written compactly.
    /// </summary>
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new RoomScrollStateJsonConverter() },
    };
}
