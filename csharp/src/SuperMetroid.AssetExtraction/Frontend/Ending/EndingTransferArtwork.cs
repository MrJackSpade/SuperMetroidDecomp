using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>The installed artwork the ending's transfers read when the import executes them (#1467).</summary>
/// <param name="Objects">The ending's OBJ sheets, maps and post-credits art.</param>
/// <param name="Font">The ending font.</param>
/// <param name="Reward">The post-credits reward, which selects the reward characters.</param>
internal sealed record EndingTransferArtwork(EndingObjectArtwork? Objects, EndingFontAtlas? Font, EndingReward Reward);
