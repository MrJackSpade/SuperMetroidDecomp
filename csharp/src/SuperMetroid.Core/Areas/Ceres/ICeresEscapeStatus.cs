using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>How far the Ceres escape has progressed, as the station's doors read and advance it (#1395).</summary>
internal interface ICeresEscapeStatus
{
    /// <summary>The escape's progress.</summary>
    CeresEscapeStatus CeresStatus { get; set; }
}
