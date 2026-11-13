using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's body layer (#1352, #1466, #1515, #1540): his body as he now appears, from his installed body sheet, with his
/// head at its current pose above it, or the head his body rises with before his head shows a pose.
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
        KraidBodyAppearance appearance = state.BodyShown ? state.BodyAppearance : KraidBodyAppearance.Hidden;
        if (shown is not { } cached || cached.Appearance != appearance || cached.Head != state.DisplayedHeadFrame
            || !ReferenceEquals(cached.Sheet, sheet))
        {
            shown = cached = new KraidShownBody(appearance, state.DisplayedHeadFrame, sheet, Draw(appearance, state.DisplayedHeadFrame, sheet));
        }
        return cached.Layer;
    }

    /// <summary>Where Kraid's body lies in the room while it is shown (#1352, #1617); otherwise null.</summary>
    internal Position16<RoomPixels>? BodyOrigin => _access.Kraid is { ShowsBodyLayer: true } state ? state.BodyOrigin : null;

    /// <summary>
    /// The body <paramref name="appearance"/> shows, drawn from <paramref name="sheet"/>: nothing while it is hidden, and
    /// otherwise his body at the appearance's depth (#1634) with <paramref name="head"/> above it once his head has shown a
    /// pose, and before that the head his body rises with, at his body's depth. A head pose is drawn at its own depth.
    /// </summary>
    /// <exception cref="InvalidDataException">The sheet lacks a frame his body shows, or the appearance is undefined.</exception>
    private static BossBodyLayer Draw(KraidBodyAppearance appearance, KraidHeadFrame? head, BossBodySheet sheet)
    {
        if (DepthOf(appearance) is not { } depth)
            return BossBodyLayer.Empty;
        return new BossBodyLayer([sheet.Frame(KraidBodyFrameNames.Body).At(depth),
            head is { } pose ? sheet.Frame(KraidBodyFrameNames.Head(pose.MouthStage)) : sheet.Frame(KraidBodyFrameNames.RestingHead).At(depth)]);
    }

    /// <summary>
    /// The depth <paramref name="appearance"/> draws his body and the head it rises with at (#1634): behind the sprites as he
    /// rises and fights small, in front of them once he has grown; null while his body is hidden.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="appearance"/> is undefined.</exception>
    internal static BossBodyDepth? DepthOf(KraidBodyAppearance appearance) => appearance switch
    {
        KraidBodyAppearance.Hidden => null,
        KraidBodyAppearance.BehindSprites => BossBodyDepth.BehindSprites,
        KraidBodyAppearance.InFrontOfSprites => BossBodyDepth.InFrontOfSprites,
        _ => throw new InvalidDataException($"Undefined Kraid body appearance {appearance}."),
    };
}
