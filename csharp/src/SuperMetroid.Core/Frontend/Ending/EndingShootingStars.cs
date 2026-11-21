using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The post-credits shooting stars, independent of the explosion's stars.</summary>
internal sealed class EndingShootingStars
{
    /// <summary>Each authored star's flight, each holding its own definition (#2541), retained across scene updates.</summary>
    private readonly EndingShootingStar[] stars;

    /// <summary>Seeds each authored star at the common origin and arms any authored start delay.</summary>
    internal EndingShootingStars()
    {
        var seeded = new List<EndingShootingStar>();
        foreach (EndingShootingStarDefinition definition in EndingShootingStarDefinitions.Stars)
        {
            EndingShootingStar star = Started(definition);
            if (definition.Delay != 0)
                star.Delay = UpdateCountdown.Lasting(definition.Delay);
            seeded.Add(star);
        }
        stars = [.. seeded];
    }

    /// <summary>Advances delayed or active stars, accelerates later pictures, and resets those leaving the visible bounds.</summary>
    internal void Step()
    {
        for (int index = 0; index < stars.Length; index++)
        {
            ref EndingShootingStar star = ref stars[index];
            if (star.Delay is { } delay)
            {
                if (delay.IsRunning)
                {
                    delay.Count();
                    continue;
                }
                star.Delay = null;
                star.Flight.Show(EndingShootingStarAnimations.Flight(Pace(star.Definition)), EndingShootingStarDefinitions.LeadInDuration);
                // Expiring delay skips motion, but joins the same update's drawing pass.
            }
            else
            {
                int multiplier = EndingShootingStarDefinitions.SteadyLooks.Contains(star.Look) ? 1 : 2;
                star.Velocity += star.Definition.Acceleration * multiplier;
                star.Position += star.Velocity.ToDisplacement();
            }

            Offset16<ScreenPixels> whole = star.Position.Whole;
            if (whole.X is < EndingShootingStarDefinitions.SpriteOffset or > EndingShootingStarDefinitions.FieldFarEdge ||
                whole.Y is < EndingShootingStarDefinitions.SpriteOffset or > EndingShootingStarDefinitions.FieldFarEdge)
            {
                star = Started(star.Definition);
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
    private static EndingShootingStarPace Pace(EndingShootingStarDefinition definition) => definition.Pace;

    /// <summary>The star <paramref name="definition"/> authors at the shared origin, undelayed, at the start of its lead-in.</summary>
    private static EndingShootingStar Started(EndingShootingStarDefinition definition) => new()
    {
        Definition = definition,
        Position = FixedVector<ScreenPixels>.FromWholePixels(
            Offset16<ScreenPixels>.Of(EndingShootingStarDefinitions.Origin, EndingShootingStarDefinitions.Origin)),
        Flight = AnimationPlayer<EndingShootingStarPace, EndingShootingStarLook, NoAnimationCue>.StartShowing(
            EndingShootingStarAnimations.Segments, EndingShootingStarAnimations.Flight(Pace(definition)), EndingShootingStarDefinitions.LeadInDuration),
    };
}
