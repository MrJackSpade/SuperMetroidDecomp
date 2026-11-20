namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy that may refuse Samus's shots before the common shot reaction runs.</summary>
internal interface IEnemyShotAdmission
{
    /// <summary>Whether shots reach the occupant this frame; false returns before any impact or reaction.</summary>
    bool AdmitsShots();
}
