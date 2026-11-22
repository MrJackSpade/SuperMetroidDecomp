namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The one shake Mother Brain, the Baby Metroid and the Tourian Baby Metroid share (#1947).</summary>
internal static partial class MotherBrainShakeDefinitions
{
    /// <summary>
    /// The four offsets Mother Brain, the Baby Metroid and the Tourian Baby Metroid shake through, in order: her brain and neck
    /// while she shakes or is struck, the Baby while it latches onto her or Samus, is struck or takes the fatal blow, and the
    /// Baby Metroid while it feeds. Each shake's cadence (<see cref="MotherBrainCadenceDefinitions"/>) picks the step.
    /// </summary>
    internal static readonly Offset16<RoomPixels>[] Offsets =
        [new(0, 0), new(-1, 1), new(0, -1), new(1, 1)];
}
