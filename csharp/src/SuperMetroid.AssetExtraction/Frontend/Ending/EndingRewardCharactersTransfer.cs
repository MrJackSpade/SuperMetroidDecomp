using SuperMetroid.Core.Frontend.Ending;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// The post-credits reward's characters, which only a reward page loads after its post-credits art: the suitless art below
/// three hours, else the waiting scene's own characters (function 126).
/// </summary>
/// <param name="Reward">The reward shown.</param>
internal sealed record EndingRewardCharactersTransfer(EndingReward Reward) : EndingTransfer;
