using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// One of the ending's character and map transfers the import executes once to compose the pages its sprite sheets are
/// cut from (#1467); the game selects the composed sheets by scene state and keeps no transfer history.
/// </summary>
/// <param name="Kind">The transfer.</param>
/// <param name="Chunk">The post-shot part a <see cref="EndingTransferKind.PostShotPart"/> transfer makes; zero otherwise.</param>
/// <param name="Fragment">The explosion map an <see cref="EndingTransferKind.ExplosionMap"/> transfer loads; null otherwise.</param>
internal sealed record EndingTransfer(EndingTransferKind Kind, int Chunk = 0, EndingObjectFragmentId? Fragment = null);
