using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's body layer (#1352, #1466, #1515): his whole body as he now appears, from his installed body sheet, with his
/// head at its current pose over it.
/// </summary>
[RoomLifetime]
internal sealed class KraidBodyLayer
{
    /// <summary>Kraid's state and part lookups, part programs, function timer, random numbers and the sound he last requested.</summary>
    private readonly KraidEncounterAccess _access;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;

    /// <summary>The body last shown, and its layer.</summary>
    private KraidShownBody? shown;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidBodyLayer(KraidEncounterAccess access, BossArtworkBinding bossArtwork)
    {
        _bossArtwork = bossArtwork;
        _access = access;
    }

    /// <summary>
    /// The body layer Kraid shows while he owns it (#1352); null when he does not. Drawn from his installed body sheet once
    /// per distinct appearance and head pose.
    /// </summary>
    /// <exception cref="InvalidOperationException">No boss body sheets are installed.</exception>
    /// <exception cref="InvalidDataException">The sheet lacks a frame his body shows.</exception>
    internal BossBodyLayer? CaptureBodyLayer()
    {
        if (_access.Kraid is not { ShowsBodyLayer: true } state)
            return null;
        BossBodySheet sheet = _bossArtwork.Body(BossBodyOwner.Kraid);
        if (shown is not { } cached || cached.Appearance != state.BodyAppearance || cached.Head != state.DisplayedHeadFrame
            || !ReferenceEquals(cached.Sheet, sheet))
        {
            shown = cached = new KraidShownBody(state.BodyAppearance, state.DisplayedHeadFrame, sheet,
                Draw(state.BodyAppearance, state.DisplayedHeadFrame, sheet));
        }
        return cached.Layer;
    }

    /// <summary>Where Kraid's body layer is placed while it is shown (#1352); otherwise null.</summary>
    internal Position16<BackgroundPixels>? BodyPlacement => _access.Kraid is { ShowsBodyLayer: true } state ? state.BodyLayerPlacement : null;

    /// <summary>
    /// The body <paramref name="appearance"/> shows, drawn from <paramref name="sheet"/>: nothing while it is hidden, and
    /// otherwise his whole body with <paramref name="head"/>, when his head has shown a pose, over it.
    /// </summary>
    /// <exception cref="InvalidDataException">The sheet lacks a frame his body shows.</exception>
    private static BossBodyLayer Draw(KraidBodyAppearance appearance, KraidHeadFrame? head, BossBodySheet sheet)
    {
        if (KraidBodyFrameNames.Of(appearance) is not { } body)
            return BossBodyLayer.Empty;
        return head is { } pose
            ? new BossBodyLayer([sheet.Frame(body), sheet.Frame(KraidBodyFrameNames.Head(pose.MouthStage))])
            : new BossBodyLayer([sheet.Frame(body)]);
    }
}
