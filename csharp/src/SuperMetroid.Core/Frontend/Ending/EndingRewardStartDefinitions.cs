using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// Where each part of the ending's reward Samus starts and the colouring it draws in (#2205), stated once for the idle,
/// gesture and jump casts that show it.
/// </summary>
internal static partial class EndingRewardStartDefinitions
{
    /// <summary>The suitless Samus, body and legs alike, standing near the screen's centre in the suitless colours.</summary>
    internal static readonly EndingRewardStart Suitless = new(new(120, 136), EndingSpriteColouring.SuitlessSamus);

    /// <summary>The suited body and its arm, in the suit's colours.</summary>
    internal static readonly EndingRewardStart SuitedBody = new(new(120, 152), EndingSpriteColouring.SuitedSamus);

    /// <summary>The helmeted head above the suited body, in the suit's colours.</summary>
    internal static readonly EndingRewardStart HelmetedHead = new(new(124, 108), EndingSpriteColouring.SuitedSamus);

    /// <summary>The helmetless head above the suited body, in the suitless colours.</summary>
    internal static readonly EndingRewardStart HelmetlessHead = new(new(121, 107), EndingSpriteColouring.SuitlessSamus);
}
