namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>One Choot falling path: the samples it follows and how far its loop origin advances per loop.</summary>
/// <param name="Samples">The samples the path follows; the update after its last sample ends one loop of the path.</param>
/// <param name="LoopYDistance">
/// How far, in pixels, the path's loop origin advances per loop. The advance is not always the path's final offset, so it
/// stays an authored input of the drawn trajectory.
/// </param>
internal sealed partial record ChootFallingPathDefinition(ChootFallSample[] Samples, int LoopYDistance);
