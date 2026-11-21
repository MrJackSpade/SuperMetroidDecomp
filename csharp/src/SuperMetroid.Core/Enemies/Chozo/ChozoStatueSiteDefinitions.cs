using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>The two shipped Chozo statues' sites.</summary>
internal static class ChozoStatueSiteDefinitions
{
    /// <summary>
    /// Each site: the Wrecked Ship's statue, whose placement puts its hand and the closing slope access and whose hand
    /// admits Samus once Phantoon is defeated; Lower Norfair's, whose placement puts its hand and whose hand admits her once
    /// she has Space Jump.
    /// </summary>
    internal static readonly IReadOnlyDictionary<ChozoStatueSite, ChozoStatueSiteDefinition> Sites = new Dictionary<ChozoStatueSite, ChozoStatueSiteDefinition>
    {
        [ChozoStatueSite.WreckedShip] = new(ChozoAndTubeShades.WreckedShipStatue, ChozoStatueSegment.WreckedShipIdle, ChozoStatueSegment.WreckedShipActivated,
            ChozoStatueHandObject.WreckedShip, ChozoStatueRoomDefinitions.WreckedShipHandBlock,
            [new(ChozoStatueRoomDefinitions.WreckedShipSlopeAccessBlock, ChozoStatueRoomObject.SlopeAccessClosing)],
            AreaId.WreckedShip, ChozoStatueRoomDefinitions.WreckedShipHandBehavior, ChozoHandAdmission.AreaBossDefeated),
        [ChozoStatueSite.LowerNorfair] = new(ChozoAndTubeShades.LowerNorfairStatue, ChozoStatueSegment.LowerNorfairIdle, ChozoStatueSegment.LowerNorfairActivated,
            ChozoStatueHandObject.LowerNorfair, ChozoStatueRoomDefinitions.LowerNorfairHandBlock, [],
            AreaId.Norfair, ChozoStatueRoomDefinitions.LowerNorfairHandBehavior, ChozoHandAdmission.SpaceJumpCollected),
    };
}
