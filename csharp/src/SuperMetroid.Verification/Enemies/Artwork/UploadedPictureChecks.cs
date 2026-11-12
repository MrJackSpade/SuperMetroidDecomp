using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>What the checks of pictures drawn over characters an encounter uploads (#1392, #1538) read of a running room.</summary>
internal static class UploadedPictureChecks
{
    /// <summary>The looks the runtime's room encounters show other than their room's load (#1538).</summary>
    internal static IReadOnlyList<EncounterLook> Shown(SuperMetroidRuntime runtime) =>
        [.. Enum.GetValues<EncounterArtwork>().Select(runtime.EnemySession.Looks.Current).Where(look => look.Name != EncounterLook.LoadedName)];
}
