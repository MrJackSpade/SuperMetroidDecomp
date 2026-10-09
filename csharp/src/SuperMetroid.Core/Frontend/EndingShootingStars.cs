using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>$8B:E7BB/E812: bounded post-credits star records, independent of explosion stars.</summary>
internal sealed class EndingShootingStars
{
    /// <summary>Mutable state for the fixed bank-$8B post-credits star slots.</summary>
    private readonly EndingShootingStar[] stars = new EndingShootingStar[EndingShootingStarDefinitions.Count];

    /// <summary>Creates all star slots at their origin and arms each authored launch delay.</summary>
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

    /// <summary>Advances delays, fixed-point motion, animation cadence, and out-of-bounds resets for one update.</summary>
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
                int multiplier = (star.IndexAndFrame & 0xff00) >= EndingShootingStarDefinitions.DoubleAccelerationFrame ? 2 : 1;
                star.XVelocity = unchecked((ushort)(star.XVelocity + multiplier * definition.XAcceleration));
                star.YVelocity = unchecked((ushort)(star.YVelocity + multiplier * definition.YAcceleration));
                Move(ref star.X, ref star.XSubposition, star.XVelocity);
                Move(ref star.Y, ref star.YSubposition, star.YVelocity);
            }

            if (star.X is < EndingShootingStarDefinitions.SpriteOffset or > byte.MaxValue ||
                star.Y is < EndingShootingStarDefinitions.SpriteOffset or > byte.MaxValue)
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

    /// <summary>Adds active, visible stars to the current OAM buffer.</summary>
    /// <param name="oam">Destination sprite list receiving each visible star's small-sprite entry.</param>
    internal void Draw(OamBuffer oam)
    {
        foreach (var star in stars)
        {
            if ((short)star.IndexAndFrame < 0 || (star.IndexAndFrame & 0xff00) == 0) continue;
            int frame = star.IndexAndFrame >> 9;
            oam.AddRawSmallSprite((ushort)(star.X - EndingShootingStarDefinitions.SpriteOffset),
                (ushort)(star.Y - EndingShootingStarDefinitions.SpriteOffset), EndingShootingStarDefinitions.Attributes[frame]);
        }
    }

    /// <summary>Adds a signed 8.8 velocity to a split 16.16 position, retaining the wrapped whole and fractional words.</summary>
    /// <param name="whole">Whole-pixel position word updated in place.</param>
    /// <param name="fraction">Fractional position word updated in place.</param>
    /// <param name="velocity">Signed 8.8 velocity word added to the position.</param>
    private static void Move(ref ushort whole, ref ushort fraction, ushort velocity)
    {
        uint position = unchecked((((uint)whole << 16) | fraction) + (uint)((short)velocity << 8));
        whole = (ushort)(position >> 16);
        fraction = (ushort)position;
    }

    /// <summary>Restores one star slot to its authored launch origin and initial animation timer.</summary>
    /// <param name="star">State slot replaced with a fresh star record.</param>
    /// <param name="index">Stable slot index retained in the record's index/frame word.</param>
    private static void Reset(ref EndingShootingStar star, ushort index) => star = new()
    {
        IndexAndFrame = index, X = EndingShootingStarDefinitions.Origin, Y = EndingShootingStarDefinitions.Origin,
        Timer = EndingShootingStarDefinitions.InitialTimer
    };
}

/// <summary>Per-slot position, velocity, delay, and animation words for one post-credits star.</summary>
/// <remarks><see cref="IndexAndFrame"/> uses its sign bit for the initial delay state and its upper byte for animation progression.</remarks>
internal struct EndingShootingStar
{
    /// <summary>Compact state words: slot index plus frame, whole and fractional X/Y positions, timer, and signed 8.8 velocities.</summary>
    internal ushort IndexAndFrame, X, XSubposition, Y, YSubposition, Timer, XVelocity, YVelocity;
}
