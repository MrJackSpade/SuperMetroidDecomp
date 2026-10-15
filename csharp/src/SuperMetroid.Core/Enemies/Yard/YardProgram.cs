namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// The Yard's crawl, turn, hiding and airborne animations as typed programs (#641), one per bank-$A3 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="YardAnimations"/>.
/// </summary>
internal enum YardProgram
{
    /// <summary><c>InstList_Yard_OutsideTurn_UpsideRight_MovingUp</c> at $A3:C8C6.</summary>
    OutsideTurnUpsideRightMovingUp,
    /// <summary><c>InstList_Yard_Crawling_UpsideUp_MovingLeft</c> at $A3:C8E0.</summary>
    CrawlingUpsideUpMovingLeft,
    /// <summary><c>InstList_Yard_OutsideTurn_UpsideUp_MovingLeft</c> at $A3:C8FC.</summary>
    OutsideTurnUpsideUpMovingLeft,
    /// <summary><c>InstList_Yard_Crawling_UpsideLeft_MovingDown</c> at $A3:C916.</summary>
    CrawlingUpsideLeftMovingDown,
    /// <summary><c>InstList_Yard_OutsideTurn_UpsideLeft_MovingDown</c> at $A3:C932.</summary>
    OutsideTurnUpsideLeftMovingDown,
    /// <summary><c>InstList_Yard_Crawling_UpsideDown_MovingRight</c> at $A3:C94C.</summary>
    CrawlingUpsideDownMovingRight,
    /// <summary><c>InstList_Yard_OutsideTurn_UpsideDown_MovingRight</c> at $A3:C968.</summary>
    OutsideTurnUpsideDownMovingRight,
    /// <summary><c>InstList_Yard_Crawling_UpsideRight_MovingUp</c> at $A3:C982.</summary>
    CrawlingUpsideRightMovingUp,
    /// <summary><c>InstList_Yard_OutsideTurn_UpsideLeft_MovingUp</c> at $A3:C99E.</summary>
    OutsideTurnUpsideLeftMovingUp,
    /// <summary><c>InstList_Yard_Crawling_UpsideUp_MovingRight</c> at $A3:C9B8.</summary>
    CrawlingUpsideUpMovingRight,
    /// <summary><c>InstList_Yard_OutsideTurn_UpsideUp_MovingRight</c> at $A3:C9D4.</summary>
    OutsideTurnUpsideUpMovingRight,
    /// <summary><c>InstList_Yard_Crawling_UpsideRight_MovingDown</c> at $A3:C9EE.</summary>
    CrawlingUpsideRightMovingDown,
    /// <summary><c>InstList_Yard_OutsideTurn_UpsideRight_MovingDown</c> at $A3:CA0A.</summary>
    OutsideTurnUpsideRightMovingDown,
    /// <summary><c>InstList_Yard_Crawling_UpsideDown_MovingLeft</c> at $A3:CA24.</summary>
    CrawlingUpsideDownMovingLeft,
    /// <summary><c>InstList_Yard_OutsideTurn_UpsideDown_MovingLeft</c> at $A3:CA40.</summary>
    OutsideTurnUpsideDownMovingLeft,
    /// <summary><c>InstList_Yard_Crawling_UpsideLeft_MovingUp</c> at $A3:CA5A.</summary>
    CrawlingUpsideLeftMovingUp,
    /// <summary><c>InstList_Yard_InsideTurn_UpsideUp_MovingLeft</c> at $A3:CA76.</summary>
    InsideTurnUpsideUpMovingLeft,
    /// <summary><c>InstList_Yard_InsideTurn_UpsideRight_MovingUp</c> at $A3:CA8E.</summary>
    InsideTurnUpsideRightMovingUp,
    /// <summary><c>InstList_Yard_InsideTurn_UpsideDown_MovingRight</c> at $A3:CAA6.</summary>
    InsideTurnUpsideDownMovingRight,
    /// <summary><c>InstList_Yard_InsideTurn_UpsideLeft_MovingDown</c> at $A3:CABE.</summary>
    InsideTurnUpsideLeftMovingDown,
    /// <summary><c>InstList_Yard_InsideTurn_UpsideUp_MovingRight</c> at $A3:CAD6.</summary>
    InsideTurnUpsideUpMovingRight,
    /// <summary><c>InstList_Yard_InsideTurn_UpsideLeft_MovingUp</c> at $A3:CAEE.</summary>
    InsideTurnUpsideLeftMovingUp,
    /// <summary><c>InstList_Yard_InsideTurn_UpsideDown_MovingLeft</c> at $A3:CB06.</summary>
    InsideTurnUpsideDownMovingLeft,
    /// <summary><c>InstList_Yard_InsideTurn_UpsideRight_MovingDown</c> at $A3:CB1E.</summary>
    InsideTurnUpsideRightMovingDown,
    /// <summary><c>InstList_Yard_Hiding_UpsideUp_MovingLeft</c> at $A3:CB36.</summary>
    HidingUpsideUpMovingLeft,
    /// <summary><c>InstList_Yard_Hidden_UpsideUp_MovingLeft</c> at $A3:CB44.</summary>
    HiddenUpsideUpMovingLeft,
    /// <summary><c>InstList_Yard_Hiding_UpsideDown_MovingLeft</c> at $A3:CB50.</summary>
    HidingUpsideDownMovingLeft,
    /// <summary><c>InstList_Yard_Hiding_UpsideDown_MovingRight</c> at $A3:CB6A.</summary>
    HidingUpsideDownMovingRight,
    /// <summary><c>InstList_Yard_Hiding_UpsideUp_MovingRight</c> at $A3:CB84.</summary>
    HidingUpsideUpMovingRight,
    /// <summary><c>InstList_Yard_Hidden_UpsideUp_MovingRight</c> at $A3:CB92.</summary>
    HiddenUpsideUpMovingRight,
    /// <summary><c>InstList_Yard_Hiding_UpsideRight_MovingUp</c> at $A3:CB9E.</summary>
    HidingUpsideRightMovingUp,
    /// <summary><c>InstList_Yard_Hiding_UpsideLeft_MovingUp</c> at $A3:CBB8.</summary>
    HidingUpsideLeftMovingUp,
    /// <summary><c>InstList_Yard_Hiding_UpsideLeft_MovingDown</c> at $A3:CBD2.</summary>
    HidingUpsideLeftMovingDown,
    /// <summary><c>InstList_Yard_Hiding_UpsideRight_MovingDown</c> at $A3:CBEC.</summary>
    HidingUpsideRightMovingDown,
    /// <summary><c>InstList_Yard_Airborne_FacingLeft_0</c> at $A3:CC06.</summary>
    AirborneFacingLeft,
    /// <summary><c>InstList_Yard_Airborne_FacingLeft_1</c> at $A3:CC0E.</summary>
    AirborneFacingLeftLoop,
    /// <summary><c>InstList_Yard_Airborne_FacingRight_0</c> at $A3:CC1E.</summary>
    AirborneFacingRight,
    /// <summary><c>InstList_Yard_Airborne_FacingRight_1</c> at $A3:CC26.</summary>
    AirborneFacingRightLoop,
}
