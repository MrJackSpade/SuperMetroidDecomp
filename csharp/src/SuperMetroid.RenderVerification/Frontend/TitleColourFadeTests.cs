using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Rendering;
using SuperMetroid.RenderVerification.Harness;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.RenderVerification.Frontend;

/// <summary>
/// Renders the title's logo and copyright fades (#1714) on this device: a few of each fade's steps match the WARP
/// reference renderer, and each changes the composition's pixels, inside its own box only, from the colours its fade ends
/// on until the last step, which shows them.
/// </summary>
internal static class TitleColourFadeTests
{
    /// <summary>The updates into the logo's fade sampled: its first, fourth, seventh and last records.</summary>
    private static readonly int[] LogoSamples = [0, 9, 18, 21];
    /// <summary>The updates into the copyright's fade sampled: its first, fifth and last records.</summary>
    private static readonly int[] CopyrightSamples = [0, 12, 21];
    /// <summary>The updates each record of either fade shows for.</summary>
    private const int RecordUpdates = 3;
    /// <summary>The records of either fade; the last is the colours it ends on.</summary>
    private const int Records = 8;

    /// <summary>Runs the natural title to its idle screen, rendering each sampled fade step on <paramref name="renderer"/>.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        TitleSequenceState title = RenderRetailFixture.CreateTitle();
        SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(SpriteSheetId.Title);
        SpriteSheetFrame logo = sheet.Frame(TitleSpriteDefinitions.FrameName(TitleSprite.SuperMetroidLogo));
        SpriteSheetFrame copyright = sheet.Frame(TitleSpriteDefinitions.FrameName(TitleSprite.NintendoCopyright));
        // The colours each fade ends on: the logo's own, and the copyright's as the skip restores them.
        PictureShading logoEnd = PictureShading.Shaded(TitleShades.Drawn(TitleSpriteColouring.Logo));
        PictureShading copyrightEnd = PictureShading.Shaded(TitleShades.Drawn(TitleSpriteColouring.Copyright)).With(TitleShades.RestoredCopyright);
        int? logoStart = null, copyrightStart = null;
        int checks = 0;
        for (int update = 0; title.Phase != TitleSequencePhase.TitleScreen; update++)
        {
            if (update >= 3_000)
                throw new InvalidOperationException("The natural title did not reach its idle screen.");
            TitleSequencePhase before = title.Phase;
            title.Step(0);
            if (before != title.Phase && title.Phase == TitleSequencePhase.TitleLogoFade)
                logoStart = update;
            if (before != title.Phase && title.Phase == TitleSequencePhase.CopyrightFade)
                copyrightStart = update;
            if (logoStart is { } logoFrom && LogoSamples.Contains(update - logoFrom))
                checks += Check(device, renderer, title, update, logo, logoEnd, update - logoFrom, "logo");
            if (copyrightStart is { } copyrightFrom && CopyrightSamples.Contains(update - copyrightFrom))
                checks += Check(device, renderer, title, update, copyright, copyrightEnd, update - copyrightFrom, "copyright");
        }
        if (checks != LogoSamples.Length + CopyrightSamples.Length)
            throw new InvalidOperationException($"{device.Kind}: the title rendered {checks} fade steps, not every sampled one.");
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {checks} title logo and copyright fade steps match the WARP reference device, " +
            "each recolouring only its composition until its last record shows the colours the fade ends on.");
    }

    /// <summary>
    /// Renders the title's frame <paramref name="update"/>, <paramref name="into"/> updates into the fade of
    /// <paramref name="faded"/>: the frame matches the reference renderer, and the same frame with the composition drawn
    /// in <paramref name="end"/>, the colours its fade ends on, differs from it, within the composition's box only, exactly
    /// while the fade shows a record before its last.
    /// </summary>
    private static int Check(D3D11RenderDevice device, D3D11FrameRenderer renderer, TitleSequenceState title, int update,
        SpriteSheetFrame faded, PictureShading end, int into, string composition)
    {
        string context = $"{device.Kind}: title {composition} fade, {into} updates in (update {update})";
        FloorScene scene = title.CaptureRenderSnapshot();
        var packet = new DisplayPacket(update, scene);
        RgbaPixel[] shown = renderer.RenderForReadback(packet);
        PixelComparison.Verify(packet, title.Render(), shown, context);

        ImageSpriteDraw draw = scene.Paint.SpriteDraws.OfType<ImageSpriteDraw>().Single(candidate => candidate.Image == faded);
        ImmutableArray<SpriteDraw> ended = [.. scene.Paint.SpriteDraws.Select(candidate => ReferenceEquals(candidate, draw)
            ? draw with { Shading = end } : candidate)];
        RgbaPixel[] endShown = renderer.RenderForReadback(new DisplayPacket(update,
            new FloorScene(new FramePaint(scene.Paint.Backdrop, ended), scene.Background, scene.Brightness, scene.Gradient, scene.Floor)));
        int left = draw.Corner.X, top = draw.Corner.Y, right = left + faded.Size.X, bottom = top + faded.Size.Y;
        int differing = 0;
        for (int pixel = 0; pixel < shown.Length; pixel++)
        {
            if (shown[pixel] == endShown[pixel])
                continue;
            int x = pixel % packet.Size.X, y = pixel / packet.Size.X;
            if (x < left || x >= right || y < top || y >= bottom)
                throw new InvalidOperationException($"{context}: the fade changed pixel ({x},{y}), outside the {composition}'s box.");
            differing++;
        }
        bool lastRecord = into / RecordUpdates == Records - 1;
        if (lastRecord != (differing == 0))
            throw new InvalidOperationException(lastRecord
                ? $"{context}: the last record shows {differing} pixels unlike the colours the fade ends on."
                : $"{context}: a record before the last shows the colours the fade ends on.");
        return 1;
    }
}
