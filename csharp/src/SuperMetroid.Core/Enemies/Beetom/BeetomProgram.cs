namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// The Beetom's crawl, hop and drain animations as typed programs (#641), one per bank-$A8 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="BeetomAnimations"/>.
/// </summary>
internal enum BeetomProgram
{
    /// <summary><c>InstList_Beetom_Crawling_FacingLeft_0</c> at $A8:B696.</summary>
    CrawlingLeft,
    /// <summary>The loop at $A8:B698.</summary>
    CrawlingLeftLoop,
    /// <summary><c>InstList_Beetom_Hop_FacingLeft</c> at $A8:B6AC.</summary>
    HopLeft,
    /// <summary><c>InstList_Beetom_DrainingSamus_FacingLeft_0</c> at $A8:B6CC.</summary>
    DrainingLeft,
    /// <summary>The loop at $A8:B6DE.</summary>
    DrainingLeftLoop,
    /// <summary><c>InstList_Beetom_Crawling_FacingRight_0</c> at $A8:B6F2.</summary>
    CrawlingRight,
    /// <summary>The loop at $A8:B6F4.</summary>
    CrawlingRightLoop,
    /// <summary><c>InstList_Beetom_Hop_FacingRight</c> at $A8:B708.</summary>
    HopRight,
    /// <summary><c>InstList_Beetom_DrainingSamus_FacingRight_0</c> at $A8:B728.</summary>
    DrainingRight,
    /// <summary>The loop at $A8:B73A.</summary>
    DrainingRightLoop,
}
