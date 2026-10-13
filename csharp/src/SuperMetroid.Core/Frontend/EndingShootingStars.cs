using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Frontend;

/// <summary>$8B:E7BB/E812: bounded post-credits star records, independent of explosion stars.</summary>
internal sealed class EndingShootingStars
{
    internal readonly EndingShootingStar[] stars = new EndingShootingStar[EndingShootingStarDefinitions.Count];

    internal EndingShootingStars()
    {
        for (int index = 0; index < stars.Length; index++)
        {
            Reset(ref stars[index], (ushort)index);
            ushort delay = EndingShootingStarDefinitions.Records[index].Delay;
            if (delay == 0) continue;
            stars[index].IndexAndFrame |= 0x8000;
            stars[index].Timer = delay;
        }
    }

    internal void Step()
    {
        for (int index = 0; index < stars.Length; index++)
        {
            ref EndingShootingStar star = ref stars[index];
            var definition = EndingShootingStarDefinitions.Records[index];
            if ((short)star.IndexAndFrame < 0)
            {
                star.Timer = unchecked((ushort)(star.Timer - 1));
                if ((short)star.Timer >= 0) continue;
                star.Timer = EndingShootingStarDefinitions.InitialTimer;
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
            star.Timer = unchecked((ushort)(star.Timer - 1));
            if ((short)star.Timer <= 0)
            {
                star.Timer = definition.Period;
                star.IndexAndFrame = unchecked((ushort)(star.IndexAndFrame + EndingShootingStarDefinitions.AnimationIncrement));
            }
        }
    }

    internal void Draw(OamBuffer oam)
    {
        foreach (var star in stars)
        {
            if ((short)star.IndexAndFrame < 0 || (SnesLittleEndian.HighByteInPlace(star.IndexAndFrame)) == 0) continue;
            int frame = star.IndexAndFrame >> 9;
            Position16<ScreenPixels> origin = star.Position.Whole - EndingShootingStarDefinitions.SpriteCenter;
            oam.AddRawSmallSprite(origin.ToOam(), EndingShootingStarDefinitions.Attributes[frame]);
        }
    }

    private static void Reset(ref EndingShootingStar star, ushort index) => star = new()
    {
        IndexAndFrame = index,
        Position = new(new(EndingShootingStarDefinitions.Origin, 0), new(EndingShootingStarDefinitions.Origin, 0)),
        Timer = EndingShootingStarDefinitions.InitialTimer
    };
}

internal struct EndingShootingStar
{
    internal ushort IndexAndFrame, Timer;
    internal FixedPosition<ScreenPixels> Position;
    internal EightEightVector<ScreenPixels> Velocity;
}
