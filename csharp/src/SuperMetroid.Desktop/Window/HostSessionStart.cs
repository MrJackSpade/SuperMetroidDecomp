using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Desktop.Window;

/// <summary>What the host prepared when it (re)starts a session that a <see cref="IHostFrameDriver"/> creates.</summary>
/// <param name="Content">The installed artwork and audio the host binds to every game it runs.</param>
/// <param name="Assists">The gameplay assists from the host options.</param>
/// <param name="Maps">The installed map art, which decodes cartridge-format save RAM.</param>
/// <param name="ContentIdentity">The digest of the selected installed content.</param>
internal sealed record HostSessionStart(
    IInstalledSessionContent Content,
    GameplayAssists Assists,
    AreaMapPresentation Maps,
    GameContentIdentity ContentIdentity);
