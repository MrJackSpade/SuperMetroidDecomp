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
            Reset(ref stars[index], (ushort)index);
            ushort delay = EndingShootingStarDefinitions.Records[index].Delay;
            if (delay == 0) continue;
            stars[index].IndexAndFrame |= EndingShootingStarDefinitions.Delayed;
            stars[index].Timer = NativeCountdown.FromRaw(delay);
        }
    }

    /// <summary>Advances delayed or active stars, accelerates later frames, and resets those leaving the visible bounds.</summary>
    internal void Step()
    {
        for (int index = 0; index < stars.Length; index++)
        {
            ref EndingShootingStar star = ref stars[index];
            var definition = EndingShootingStarDefinitions.Records[index];
            if ((short)star.IndexAndFrame < 0)
            {
                star.Timer = star.Timer.Decrement().Remaining;
                if (!star.Timer.IsNegative) continue;
                star.Timer = NativeCountdown.Start(EndingShootingStarDefinitions.InitialTimer);
                star.IndexAndFrame = (ushort)index;
                // Expiring delay skips motion, but joins the same call's drawing pass.
            }
            else
            {
                int multiplier = (SnesLittleEndian.HighByteInPlace(star.IndexAndFrame)) >= EndingShootingStarDefinitions.DoubleAccelerationFrame ? 2 : 1;
                star.Velocity += definition.Acceleration * multiplier;
                star.Position = star.Position.Advance(star.Velocity.ToSixteenSixteen());
            }

            Position16<ScreenPixels> whole = star.Position.Whole;
            if (whole.X is < EndingShootingStarDefinitions.SpriteOffset or > byte.MaxValue ||
                whole.Y is < EndingShootingStarDefinitions.SpriteOffset or > byte.MaxValue)
            {
                Reset(ref star, (ushort)index);
                continue;
            }
            star.Timer = star.Timer.Decrement().Remaining;
            if ((short)star.Timer.RawValue <= 0)
            {
                star.Timer = NativeCountdown.FromRaw(definition.Period);
                star.IndexAndFrame = unchecked((ushort)(star.IndexAndFrame + EndingShootingStarDefinitions.AnimationIncrement));
            }
        }
    }

    /// <summary>Draws only active stars with a nonzero animation frame into the supplied OAM buffer.</summary>
    internal void Draw(OamBuffer oam)
    {
        foreach (var star in stars)
        {
            if ((short)star.IndexAndFrame < 0 || (SnesLittleEndian.HighByteInPlace(star.IndexAndFrame)) == 0) continue;
            int frame = star.IndexAndFrame >> 9;
            Position16<ScreenPixels> origin = star.Position.Whole - EndingShootingStarDefinitions.SpriteCenter;
            oam.AddRawSmallSprite(origin.ToOam(), EndingShootingStarDefinitions.Attribute(frame));
        }
    }

    /// <summary>Returns a star to the shared origin with its initial animation timer and original index.</summary>
    private static void Reset(ref EndingShootingStar star, ushort index) => star = new()
    {
        IndexAndFrame = index,
        Position = new(new(EndingShootingStarDefinitions.Origin, 0), new(EndingShootingStarDefinitions.Origin, 0)),
        Timer = NativeCountdown.Start(EndingShootingStarDefinitions.InitialTimer)
    };
}
