namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// Yard's movement routine.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum YardMovementFunction
{
    /// <summary>$A3:CF5F, RTL stub: main movement remains idle until an animation instruction installs a movement pointer.</summary>
    InstructionPending = 1,
    /// <summary>$A3:CF60, Function_Yard_Movement_Hiding: probes the current attachment surface while hidden and drops Yard if support disappears.</summary>
    Hiding,
    /// <summary>$A3:CFA6, Function_Yard_Movement_Crawling_UpsideUp_MovingLeft: floor-attached leftward crawl using horizontal surface and corner probes.</summary>
    CrawlingUpsideUpMovingLeft,
    /// <summary>$A3:CFB7, Function_Yard_Movement_Crawling_UpsideLeft_MovingDown: downward crawl with Yard's top facing left, using vertical surface and corner probes.</summary>
    CrawlingUpsideLeftMovingDown,
    /// <summary>$A3:CFBD, Function_Yard_Movement_Crawling_UpsideDown_MovingRight: ceiling-attached rightward crawl using horizontal surface and corner probes.</summary>
    CrawlingUpsideDownMovingRight,
    /// <summary>$A3:CFCE, Function_Yard_Movement_Crawling_UpsideRight_MovingUp: upward crawl with Yard's top facing right, using vertical surface and corner probes.</summary>
    CrawlingUpsideRightMovingUp,
    /// <summary>$A3:CFD4, Function_Yard_Movement_Crawling_UpsideUp_MovingRight: floor-attached rightward crawl using horizontal surface and corner probes.</summary>
    CrawlingUpsideUpMovingRight,
    /// <summary>$A3:CFE5, Function_Yard_Movement_Crawling_UpsideRight_MovingDown: downward crawl with Yard's top facing right, using vertical surface and corner probes.</summary>
    CrawlingUpsideRightMovingDown,
    /// <summary>$A3:CFEB, Function_Yard_Movement_Crawling_UpsideDown_MovingLeft: ceiling-attached leftward crawl using horizontal surface and corner probes.</summary>
    CrawlingUpsideDownMovingLeft,
    /// <summary>$A3:CFFC, Function_Yard_Movement_Crawling_UpsideLeft_MovingUp: upward crawl with Yard's top facing left, using vertical surface and corner probes.</summary>
    CrawlingUpsideLeftMovingUp,
    /// <summary>$A3:D1B3, Function_Yard_Movement_Airborne: applies 16.16 shell motion, horizontal decay, gravity, native per-word collision bounces, and low-speed floor landing.</summary>
    Airborne,
}
