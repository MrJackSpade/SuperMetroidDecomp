using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The post-credits shooting stars, independent of the explosion's stars.</summary>
internal sealed class EndingShootingStars
{
    /// <summary>Each authored star's flight, retained across scene updates.</summary>
    internal readonly EndingShootingStar[] stars = new EndingShootingStar[EndingShootingStarDefinitions.Count];

    /// <summary>Seeds each star at the common origin and arms any authored start delay.</summary>
    internal EndingShootingStars()
    {
        for (int index = 0; index < stars.Length; index++)
        {
            Reset(ref stars[index]);
            int delay = EndingShootingStarDefinitions.Stars[index].Delay;
            if (delay == 0) continue;
            stars[index].Delayed = true;
            stars[index].Timer = delay;
        }
    }

    /// <summary>Advances delayed or active stars, accelerates later frames, and resets those leaving the visible bounds.</summary>
    internal void Step()
    {
        for (int index = 0; index < stars.Length; index++)
        {
            ref EndingShootingStar star = ref stars[index];
            var definition = EndingShootingStarDefinitions.Stars[index];
            if (star.Delayed)
            {
                if (--star.Timer >= 0) continue;
                star.Timer = EndingShootingStarDefinitions.InitialTimer;
                star.Delayed = false;
                star.AnimationFrame = 0;
                // Expiring delay skips motion, but joins the same call's drawing pass.
            }
            else
            {
                int multiplier = star.AnimationFrame >= EndingShootingStarDefinitions.DoubleAccelerationFrame ? 2 : 1;
                star.Velocity += definition.Acceleration * multiplier;
                star.Position += star.Velocity.ToSixteenSixteen();
            }

            Offset16<ScreenPixels> whole = star.Position.Whole;
            if (whole.X is < EndingShootingStarDefinitions.SpriteOffset or > EndingShootingStarDefinitions.FieldFarEdge ||
                whole.Y is < EndingShootingStarDefinitions.SpriteOffset or > EndingShootingStarDefinitions.FieldFarEdge)
            {
                Reset(ref star);
                continue;
            }
            // A still star keeps its first visible frame for the rest of its flight (#1983).
            if (star.AnimationFrame > 0 && definition.Period is null)
                continue;
            if (--star.Timer <= 0)
            {
                if (definition.Period is { } period)
                    star.Timer = period;
                star.AnimationFrame++;
            }
        }
    }

    /// <summary>Draws the stars past their delay and lead-in among <paramref name="sprites"/>, in <paramref name="shading"/> (#1512).</summary>
    internal void Draw(ScreenSprites sprites, SpriteSheet sheet, PictureShading shading)
    {
        foreach (var star in stars)
        {
            if (star.Delayed || star.AnimationFrame == 0) continue;
            Offset16<ScreenPixels> origin = star.Position.Whole - EndingShootingStarDefinitions.SpriteCenter;
            sprites.AddImage(sheet.Frame(EndingShootingStarFrames.SheetFrameName(star.AnimationFrame)), origin, shading);
        }
    }

    /// <summary>Returns a star to the shared origin, undelayed, at the start of its animation lead-in.</summary>
    private static void Reset(ref EndingShootingStar star) => star = new()
    {
        Position = FixedVector<ScreenPixels>.FromWholePixels(
            Offset16<ScreenPixels>.Of(EndingShootingStarDefinitions.Origin, EndingShootingStarDefinitions.Origin)),
        Timer = EndingShootingStarDefinitions.InitialTimer
    };
}
