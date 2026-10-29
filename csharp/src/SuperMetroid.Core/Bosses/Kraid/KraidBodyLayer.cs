using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's body layer (#1352, #1466): his body frame drawn from his installed body sheet, the upper body over the
/// top-left page with his head over it, and the lower body over the bottom-left page.
/// </summary>
[RoomLifetime]
internal sealed class KraidBodyLayer
{
    /// <summary>Kraid's state and part lookups, part programs, function timer, random numbers and the sound he last requested.</summary>
    private readonly KraidEncounterAccess _access;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;

    /// <summary>The last body frame drawn, and its layer.</summary>
    [NonSerialized]
    private KraidShownBody? shown;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidBodyLayer(KraidEncounterAccess access, BossArtworkBinding bossArtwork)
    {
        _bossArtwork = bossArtwork;
        _access = access;
    }

    /// <summary>
    /// The body layer Kraid shows while he owns it (#1352); null when he does not. Drawn from his installed body sheet once
    /// per distinct body frame.
    /// </summary>
    /// <exception cref="InvalidOperationException">No boss body sheets are installed.</exception>
    internal BossBodyLayer? CaptureBodyLayer()
    {
        if (_access.Kraid is not { OwnsBg2Tilemap: true } state)
            return null;
        BossBodySheet sheet = _bossArtwork.Body(BossBodyOwner.Kraid);
        if (shown is not { } cached || cached.Frame != state.DisplayedBody || !ReferenceEquals(cached.Sheet, sheet))
            shown = cached = new KraidShownBody(state.DisplayedBody, sheet, Draw(state.DisplayedBody, sheet));
        return cached.Layer;
    }

    /// <summary>Where Kraid's body layer is viewed from while he owns it (#1352); otherwise null.</summary>
    internal Position16<BackgroundPixels>? BodyScroll => _access.Kraid is { OwnsBg2Tilemap: true } state ? state.Bg2Scroll : null;

    /// <summary>The layer <paramref name="frame"/> shows: its pages and head drawn from <paramref name="sheet"/> over his blank.</summary>
    /// <exception cref="InvalidDataException">The sheet lacks a frame the body frame shows.</exception>
    private static BossBodyLayer Draw(KraidBodyFrame frame, BossBodySheet sheet)
    {
        var canvas = new BossBodyCanvas(sheet, KraidBodyLayout.LayerRows);
        if (KraidBodyFrameNames.Of(frame.Top, KraidBodyHalf.Upper) is { } upper)
            canvas.Draw(sheet.Frame(upper));
        if (frame.Head is { } head)
            canvas.Draw(sheet.Frame(KraidBodyFrameNames.Head(head.MouthStage)));
        if (KraidBodyFrameNames.Of(frame.Bottom, KraidBodyHalf.Lower) is { } lower)
            canvas.Draw(sheet.Frame(lower));
        return canvas.Publish(sheet);
    }
}
