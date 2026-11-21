namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// A boss body frame a dissolve erases step by step (#1652, #2478), as the import found it: Crocomire's melt images. Its
/// sheet finds it as it is loaded, among the frames that show a picture and have a dissolve of that picture's size.
/// </summary>
public sealed class DissolvingBossBodyFrame
{
    /// <summary>The frame the dissolve erases.</summary>
    private readonly BossBodyFrame _frame;

    /// <summary>How the dissolve erases the frame's picture.</summary>
    private readonly PictureDissolve _dissolve;

    /// <summary>Holds <paramref name="frame"/> and the dissolve that erases its picture.</summary>
    /// <param name="frame">The frame, which shows a picture of the dissolve's size.</param>
    /// <param name="dissolve">How the dissolve erases the frame's picture.</param>
    internal DissolvingBossBodyFrame(BossBodyFrame frame, PictureDissolve dissolve)
    {
        _frame = frame;
        _dissolve = dissolve;
    }

    /// <summary>The frame as it shows before the dissolve erases any of it.</summary>
    public BossBodyFrame Whole => _frame;

    /// <summary>
    /// The frame as its dissolve leaves its picture once each band has shown the steps <paramref name="bandSteps"/> gives
    /// it, by band (#1652, #2478).
    /// </summary>
    public BossBodyFrame Dissolved(ReadOnlySpan<int> bandSteps) => _frame.Hiding(_dissolve.Hidden(bandSteps));
}
