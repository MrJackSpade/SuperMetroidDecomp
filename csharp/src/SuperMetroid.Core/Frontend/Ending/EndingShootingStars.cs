using SuperMetroid.Core.Animation;
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
            Reset(ref stars[index], EndingShootingStarDefinitions.Stars[index]);
            int delay = EndingShootingStarDefinitions.Stars[index].Delay;
            if (delay == 0) continue;
            stars[index].Delayed = true;
            stars[index].DelayLeft = delay;
        }
    }

    /// <summary>Advances delayed or active stars, accelerates later pictures, and resets those leaving the visible bounds.</summary>
    internal void Step()
    {
        for (int index = 0; index < stars.Length; index++)
        {
            ref EndingShootingStar star = ref stars[index];
            var definition = EndingShootingStarDefinitions.Stars[index];
            if (star.Delayed)
            {
                if (--star.DelayLeft >= 0) continue;
                star.Delayed = false;
                star.Flight.Show(EndingShootingStarAnimations.Flight(Pace(definition)), EndingShootingStarDefinitions.LeadInDuration);
                // Expiring delay skips motion, but joins the same update's drawing pass.
            }
            else
            {
                int multiplier = star.Look is EndingShootingStarLook.LeadIn or EndingShootingStarLook.FaintSpeck ? 1 : 2;
                star.Velocity += definition.Acceleration * multiplier;
                star.Position += star.Velocity.ToSixteenSixteen();
            }

            Offset16<ScreenPixels> whole = star.Position.Whole;
            if (whole.X is < EndingShootingStarDefinitions.SpriteOffset or > EndingShootingStarDefinitions.FieldFarEdge ||
                whole.Y is < EndingShootingStarDefinitions.SpriteOffset or > EndingShootingStarDefinitions.FieldFarEdge)
            {
                Reset(ref star, definition);
                continue;
            }
            star.Flight.Step();
        }
    }

    /// <summary>Draws the stars past their delay and lead-in among <paramref name="sprites"/>, in <paramref name="shading"/> (#1512).</summary>
    internal void Draw(ScreenSprites sprites, SpriteSheet sheet, PictureShading shading)
    {
        foreach (var star in stars)
        {
            if (star.Delayed || star.Look == EndingShootingStarLook.LeadIn) continue;
            Offset16<ScreenPixels> origin = star.Position.Whole - EndingShootingStarDefinitions.SpriteCenter;
            sprites.AddImage(sheet.Frame(EndingShootingStarFrames.SheetFrameName(star.Look)), origin, shading);
        }
    }

    /// <summary>The pace <paramref name="definition"/>'s star flies at.</summary>
    private static EndingShootingStarPace Pace(EndingShootingStarDefinition definition) => new(definition.Period);

    /// <summary>Returns a star to the shared origin, undelayed, at the start of its lead-in.</summary>
    private static void Reset(ref EndingShootingStar star, EndingShootingStarDefinition definition) => star = new()
    {
        Position = FixedVector<ScreenPixels>.FromWholePixels(
            Offset16<ScreenPixels>.Of(EndingShootingStarDefinitions.Origin, EndingShootingStarDefinitions.Origin)),
        Flight = AnimationPlayer<EndingShootingStarPace, EndingShootingStarLook, NoAnimationCue>.StartShowing(
            EndingShootingStarAnimations.Segments, EndingShootingStarAnimations.Flight(Pace(definition)), EndingShootingStarDefinitions.LeadInDuration),
    };
}
