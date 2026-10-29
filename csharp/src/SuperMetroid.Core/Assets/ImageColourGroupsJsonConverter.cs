using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Assets;

/// <summary>Reads and writes a manifest's <see cref="ImageColourGroups"/> as the one number it stores them as (#1464).</summary>
internal sealed class ImageColourGroupsJsonConverter : JsonConverter<ImageColourGroups>
{
    /// <inheritdoc />
    /// <exception cref="JsonException">The value is not a number naming only the image's groups.</exception>
    public override ImageColourGroups Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number || !reader.TryGetInt32(out int members)
            || members < 0 || members > ImageColourGroups.EveryMember)
        {
            throw new JsonException($"A frame's colour groups are a number from 0 to {ImageColourGroups.EveryMember}.");
        }
        return ImageColourGroups.FromManifest(members);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, ImageColourGroups value, JsonSerializerOptions options) =>
        Ensure.NotNull(writer).WriteNumberValue(value.ManifestMembers);
}
