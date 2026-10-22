using System.Runtime.InteropServices;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Composes the gameplay OBJ character table's uploads into the page enemy-format sprites draw from: each upload's
/// resolved bytes over a blank table, in upload order. The page is recomposed only when an upload was recorded or an
/// upload's artwork resolves to different bytes, so a steady room keeps publishing one page.
/// </summary>
internal sealed class ObjCharacterTablePage
{
    /// <summary>The last composed page.</summary>
    internal SpriteArtwork? page;

    /// <summary>The upload record and the resolved source of each upload the cached page was composed from.</summary>
    internal ObjCharacterComposition? composedFrom;
}
