namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>A step of the gunship's landing, boarding or takeoff that the runtime and the game act on after the enemy pass (#2562).</summary>
/// <param name="Kind">The step.</param>
public sealed partial record GunshipEvent(GunshipFrameEvent Kind) : EnemyPassEvent;
