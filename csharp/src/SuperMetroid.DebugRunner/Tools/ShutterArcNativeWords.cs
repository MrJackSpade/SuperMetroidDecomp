using SuperMetroid.Core.Enemies.Shutters;

namespace SuperMetroid.DebugRunner.Tools;

/// <summary>
/// The native words <see cref="ShutterNativeArc"/> prints and seeds for the issue #347 shutter (#1815, #1818): Core's
/// shutter actions and rests have arbitrary values, so the tool writes the cartridge's words through this codec.
/// </summary>
internal static class ShutterArcNativeWords
{
    /// <summary>The rest the cartridge stores for a shutter that stops at an end for good, in AI updates.</summary>
    private const ushort PermanentRestUpdates = 0x0FF0;

    /// <summary>
    /// The routine word the cartridge stores for <paramref name="action"/>. A still shutter is written as the permanent
    /// stop's word, not the reaction-triggered shutter's initial wait: the traced scenario's shutter is already moving
    /// up at the first compared frame (the checked-in seed holds the moving-up word), so the only still shutter its
    /// frames reach has stopped at its upper end for good.
    /// </summary>
    internal static ushort Action(VerticalShutterAction action) => action switch
    {
        VerticalShutterAction.Initial => 0xEF09,
        VerticalShutterAction.WaitForTimer => 0xEF15,
        VerticalShutterAction.WaitForHorizontalProximity => 0xEF28,
        VerticalShutterAction.Activate => 0xEF39,
        VerticalShutterAction.MovingUp => 0xEF68,
        VerticalShutterAction.MovingDown => 0xEFD4,
        VerticalShutterAction.StoppedAfterMovingUp => 0xF040,
        VerticalShutterAction.StoppedAfterMovingDown => 0xF072,
        VerticalShutterAction.Still => 0xF099,
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, "The shutter action has no native word."),
    };

    /// <summary>The rest the cartridge stores for <paramref name="rest"/>, in AI updates.</summary>
    internal static ushort Rest(ShutterRest rest) => rest.IsPermanent ? PermanentRestUpdates : checked((ushort)rest.Updates);
}
