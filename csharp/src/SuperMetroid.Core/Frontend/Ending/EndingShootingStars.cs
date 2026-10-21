using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>$8B:E7BB/E812: bounded post-credits star records, independent of explosion stars.</summary>
internal sealed class EndingShootingStars
{
    /// <summary>One state record per authored post-credits star, retained across scene updates.</summary>
    internal readonly EndingShootingStar[] stars = new EndingShootingStar[EndingShootingStarDefinitions.Count];

    /// <summary>Seeds each star at the common origin and arms any authored start delay.</summary>
    internal EndingShootingStars()
    {
        for (int index = 0; index < stars.Length; index++)
        {
            Reset(ref stars[index]);
            ushort delay = EndingShootingStarDefinitions.Records[index].Delay;
            if (delay == 0) continue;
            stars[index].Delayed = true;
            stars[index].Timer = WrappingCountdown.FromRaw(delay);
        }
    }

    /// <summary>Advances delayed or active stars, accelerates later frames, and resets those leaving the visible bounds.</summary>
    internal void Step()
    {
        for (int index = 0; index < stars.Length; index++)
        {
            ref EndingShootingStar star = ref stars[index];
            var definition = EndingShootingStarDefinitions.Records[index];
            if (star.Delayed)
            {
                star.Timer = star.Timer.Decrement().Remaining;
                if (!star.Timer.IsNegative) continue;
                star.Timer = WrappingCountdown.Start(EndingShootingStarDefinitions.InitialTimer);
                star.Delayed = false;
                star.AnimationFrame = 0;
                // Expiring delay skips motion, but joins the same call's drawing pass.
            }
            else
            {
                int multiplier = star.AnimationFrame >= EndingShootingStarDefinitions.DoubleAccelerationFrame ? 2 : 1;
                star.Velocity += definition.Acceleration * multiplier;
                star.Position = star.Position.Advance(star.Velocity.ToSixteenSixteen());
            }

            Position16<ScreenPixels> whole = star.Position.Whole;
            if (whole.X is < EndingShootingStarDefinitions.SpriteOffset or > byte.MaxValue ||
                whole.Y is < EndingShootingStarDefinitions.SpriteOffset or > byte.MaxValue)
            {
                Reset(ref star);
                continue;
            }
            star.Timer = star.Timer.Decrement().Remaining;
            if ((short)star.Timer.RawValue <= 0)
            {
                star.Timer = WrappingCountdown.FromRaw(definition.Period);
                star.AnimationFrame++;
            }
        }
    }

    /// <summary>Draws only active stars with a nonzero animation frame as the supplied sprite entries.</summary>
    internal void Draw(ScreenSprites objects)
    {
        foreach (var star in stars)
        {
            if (star.Delayed || star.AnimationFrame == 0) continue;
            Position16<ScreenPixels> origin = star.Position.Whole - EndingShootingStarDefinitions.SpriteCenter;
            objects.AddSmallObject(origin.ToOam(), EndingShootingStarDefinitions.Attribute(star.AnimationFrame));
        }
    }

    /// <summary>Returns a star to the shared origin, undelayed, at the start of its animation lead-in.</summary>
    private static void Reset(ref EndingShootingStar star) => star = new()
    {
        Position = new(new(EndingShootingStarDefinitions.Origin, 0), new(EndingShootingStarDefinitions.Origin, 0)),
        Timer = WrappingCountdown.Start(EndingShootingStarDefinitions.InitialTimer)
    };
}
