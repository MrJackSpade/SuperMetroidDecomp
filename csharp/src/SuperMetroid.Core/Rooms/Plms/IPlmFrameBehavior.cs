namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The frame behaviour a PLM slot runs before its frame hold on each handler pass. A slot holds one shared
/// behavior object; installs and clears
/// replace it, and "is it X?" compares against the shared instance.
/// </summary>
internal interface IPlmFrameBehavior
{
    /// <summary>Runs the routine for <paramref name="slot"/> on this handler pass.</summary>
    /// <param name="slot">The slot whose pre-instruction this is.</param>
    /// <param name="context">The handler pass.</param>
    void Run(PlmSlot slot, PlmStepContext context);
}
