namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Whether Phantoon's body shows, hidden or translucent (#1545), and what the display latched of it with his body's
/// pixelation.
/// </summary>
/// <remarks>
/// One mutable instance belongs to the encounter. Its end persists for its lifetime. It reads
/// <see cref="PhantoonEnemyState.Translucent"/> and <see cref="PhantoonEnemyState.BodyVisibilityRequest"/> and changes
/// neither, nor any other gameplay state: no gameplay reads what it shows.
/// </remarks>
public sealed class PhantoonBodyVisibilityEffect
{
    /// <summary>Whether a <see cref="PhantoonBodyVisibilityRequest.HideForGood"/> body has ended the effect for good.</summary>
    private bool _ended;
    /// <summary>How his body shows as of the last update; not necessarily what the display shows.</summary>
    private PhantoonBodyVisibility Visibility { get; set; }
    /// <summary>How his body shows as the last display latch captured it, unaffected by later updates until latched again.</summary>
    public PhantoonBodyVisibility DisplayedVisibility { get; private set; }
    /// <summary>His body's pixelation block size in screen pixels latched for display, before this frame's enemy AI changes it; one means none.</summary>
    public int DisplayedPixelationBlockSize { get; private set; } = 1;

    /// <summary>
    /// Advances one ordinary-gameplay update: his body shows what the boss asks from the first update (#2263), as the room
    /// shows it once the effect has ended. Translucency takes precedence; otherwise
    /// <see cref="PhantoonEnemyState.BodyVisibilityRequest"/> hides his body, hides it and ends the effect for good, or shows it.
    /// </summary>
    /// <param name="boss">Encounter state read without mutation or retention.</param>
    /// <remarks>Translucency takes precedence over ending. This does not publish display state, write colours, or update the boss.</remarks>
    public void Step(PhantoonEnemyState boss)
    {
        Ensure.NotNull(boss);
        Visibility = PhantoonBodyVisibility.Shown;
        if (_ended) return;
        if (boss.Translucent)
            Visibility = PhantoonBodyVisibility.Translucent;
        else if (boss.BodyVisibilityRequest is PhantoonBodyVisibilityRequest.Hide or PhantoonBodyVisibilityRequest.HideForGood)
        {
            Visibility = PhantoonBodyVisibility.Hidden;
            // A cleared body still shows hidden on this update; the effect ends with it.
            _ended = boss.BodyVisibilityRequest == PhantoonBodyVisibilityRequest.HideForGood;
        }
    }

    /// <summary>Copies the live visibility and the supplied pixelation size to displayed state at the display latch, without advancing the effect.</summary>
    /// <param name="pixelationBlockSize">His body's pixelation block size in screen pixels, 1..16; one means none.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelationBlockSize"/> is outside 1..16.</exception>
    public void LatchDisplay(int pixelationBlockSize = 1)
    {
        if (pixelationBlockSize is < 1 or > MaximumPixelationBlockSize)
            throw new ArgumentOutOfRangeException(nameof(pixelationBlockSize), pixelationBlockSize, "Pixelation blocks are one through sixteen pixels.");
        DisplayedVisibility = Visibility;
        DisplayedPixelationBlockSize = pixelationBlockSize;
    }

    /// <summary>The largest pixelation block, in screen pixels.</summary>
    private const int MaximumPixelationBlockSize = 16;
}
