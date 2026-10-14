namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Bank-$A3 movement pointer stored in Yard's native enemy variable F.</summary>
public enum YardMovementFunction : ushort
{
    /// <summary>$A3:CF5F, RTL stub: main movement remains idle until an animation instruction installs a movement pointer.</summary>
    InstructionPending = 0xcf5f,
    /// <summary>$A3:CF60, Function_Yard_Movement_Hiding: probes the current attachment surface while hidden and drops Yard if support disappears.</summary>
    Hiding = 0xcf60,
    /// <summary>$A3:CFA6, Function_Yard_Movement_Crawling_UpsideUp_MovingLeft: floor-attached leftward crawl using horizontal surface and corner probes.</summary>
    CrawlingUpsideUpMovingLeft = 0xcfa6,
    /// <summary>$A3:CFB7, Function_Yard_Movement_Crawling_UpsideLeft_MovingDown: downward crawl with Yard's top facing left, using vertical surface and corner probes.</summary>
    CrawlingUpsideLeftMovingDown = 0xcfb7,
    /// <summary>$A3:CFBD, Function_Yard_Movement_Crawling_UpsideDown_MovingRight: ceiling-attached rightward crawl using horizontal surface and corner probes.</summary>
    CrawlingUpsideDownMovingRight = 0xcfbd,
    /// <summary>$A3:CFCE, Function_Yard_Movement_Crawling_UpsideRight_MovingUp: upward crawl with Yard's top facing right, using vertical surface and corner probes.</summary>
    CrawlingUpsideRightMovingUp = 0xcfce,
    /// <summary>$A3:CFD4, Function_Yard_Movement_Crawling_UpsideUp_MovingRight: floor-attached rightward crawl using horizontal surface and corner probes.</summary>
    CrawlingUpsideUpMovingRight = 0xcfd4,
    /// <summary>$A3:CFE5, Function_Yard_Movement_Crawling_UpsideRight_MovingDown: downward crawl with Yard's top facing right, using vertical surface and corner probes.</summary>
    CrawlingUpsideRightMovingDown = 0xcfe5,
    /// <summary>$A3:CFEB, Function_Yard_Movement_Crawling_UpsideDown_MovingLeft: ceiling-attached leftward crawl using horizontal surface and corner probes.</summary>
    CrawlingUpsideDownMovingLeft = 0xcfeb,
    /// <summary>$A3:CFFC, Function_Yard_Movement_Crawling_UpsideLeft_MovingUp: upward crawl with Yard's top facing left, using vertical surface and corner probes.</summary>
    CrawlingUpsideLeftMovingUp = 0xcffc,
    /// <summary>$A3:D1B3, Function_Yard_Movement_Airborne: applies 16.16 shell motion, horizontal decay, gravity, native per-word collision bounces, and low-speed floor landing.</summary>
    Airborne = 0xd1b3,
}
