using SuperMetroid.Core.Enemies.Hibashi;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Development-tool members of <see cref="HibashiVisualDefinitions"/>; never linked by player hosts.</summary>
internal static class HibashiVisualDefinitionsTooling
{
    /// <summary>The spritemaps the Hibashi's programs show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps() => HibashiAnimations.Spritemaps(Enum.GetValues<HibashiProgram>());
}
