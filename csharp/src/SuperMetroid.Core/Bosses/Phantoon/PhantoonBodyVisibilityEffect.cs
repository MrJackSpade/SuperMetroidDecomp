namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Whether Phantoon's body shows, hidden or translucent (#1545), and what the display latched of it with his body's
/// pixelation.
/// </summary>
/// <remarks>
/// One mutable instance belongs to the encounter. Its setup progress and its end persist for its lifetime. It reads
/// <see cref="PhantoonEnemyState.Translucent"/> and the mouth's <see cref="PhantoonMouthState.BodyLayer"/> and changes
/// neither, nor any other gameplay state: no gameplay reads what it shows.
/// </remarks>
public sealed class PhantoonBodyVisibilityEffect
{
    /// <summary>
    /// The updates the effect spends starting before it shows what the boss asks; his body shows as the room shows it
    /// on both.
    /// </summary>
    internal const int SetupCalls = 2;
    /// <summary>Number of initial updates consumed before the effect shows what the boss asks.</summary>
    private int _setupCalls;
    /// <summary>Whether a <see cref="PhantoonBodyLayer.Cleared"/> body has ended the effect for good.</summary>
    private bool _ended;
    /// <summary>How his body shows as of the last update; not necessarily what the display shows.</summary>
    public PhantoonBodyVisibility Visibility { get; private set; }
    /// <summary>How his body shows as the last display latch captured it, unaffected by later updates until latched again.</summary>
    public PhantoonBodyVisibility DisplayedVisibility { get; private set; }
    /// <summary>His body's pixelation block size in screen pixels latched for display, before this frame's enemy AI changes it; one means none.</summary>
    public int DisplayedMosaicBlockSize { get; private set; } = 1;

    /// <summary>
    /// Advances one ordinary-gameplay update: his body shows as the room shows it during the two setup updates and after
    /// the effect has ended. Thereafter translucency takes precedence; otherwise the mouth's
    /// <see cref="PhantoonMouthState.BodyLayer"/> hides his body, hides it and ends the effect for good, or shows it.
    /// </summary>
    /// <param name="boss">Encounter state read without mutation or retention; its mouth must exist once the effect is set up.</param>
    /// <remarks>Translucency takes precedence over ending. This does not publish display state, write colours, or update the boss.</remarks>
    public void Step(PhantoonEnemyState boss)
    {
        Ensure.NotNull(boss);
        Visibility = PhantoonBodyVisibility.Shown;
        if (_ended) return;
        if (_setupCalls < SetupCalls) { _setupCalls++; return; }
        if (boss.Translucent)
            Visibility = PhantoonBodyVisibility.Translucent;
        else if (boss.MouthState.BodyLayer is PhantoonBodyLayer.Hidden or PhantoonBodyLayer.Cleared)
        {
            Visibility = PhantoonBodyVisibility.Hidden;
            // A cleared body still shows hidden on this update; the effect ends with it.
            _ended = boss.MouthState.BodyLayer == PhantoonBodyLayer.Cleared;
        }
    }

    /// <summary>Copies the live visibility and the supplied pixelation size to displayed state at the display latch, without advancing the effect.</summary>
    /// <param name="mosaicBlockSize">His body's pixelation block size in screen pixels, 1..16; one means none.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="mosaicBlockSize"/> is outside 1..16.</exception>
    public void LatchDisplay(int mosaicBlockSize = 1)
    {
        if (mosaicBlockSize is < 1 or > MaximumMosaicBlockSize)
            throw new ArgumentOutOfRangeException(nameof(mosaicBlockSize), mosaicBlockSize, "Pixelation blocks are one through sixteen pixels.");
        DisplayedVisibility = Visibility;
        DisplayedMosaicBlockSize = mosaicBlockSize;
    }

    /// <summary>The largest pixelation block, in screen pixels.</summary>
    private const int MaximumMosaicBlockSize = 16;
}
