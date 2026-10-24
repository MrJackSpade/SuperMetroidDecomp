using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Fatal-damage pose entry and state boundaries.</summary>
public static class SamusDeathSequenceDefinitions
{
    /// <summary>Sixteen calls display the ordinary death pose before flashing.</summary>
    public const ushort PreFlashFrameCount = 16;
    /// <summary>Sixty calls alternate palettes before the suit explodes.</summary>
    public const ushort FlashFrameCount = 60;
    /// <summary>Right-facing base index in Samus's death-explosion spritemap table.</summary>
    public const ushort RightExplosionSpritemap = 0x081c;
    /// <summary>Left-facing base index in Samus's death-explosion spritemap table.</summary>
    public const ushort LeftExplosionSpritemap = 0x0825;

    /// <summary>$9B:B420: ball, unused glitch-ball and ordinary movement select their death pose phase.</summary>
    public static InitialFrameSequence InitialFramesByMovementType => default;

    /// <summary>Allocation-free view of $9B:B420's initial death-animation index selected by the movement type in effect before fatal-damage pose setup.</summary>
    public readonly struct InitialFrameSequence : IReadOnlyList<byte>
    {
        /// <summary>Twenty-eight movement-type entries, covering native values $00 through Special ($1B).</summary>
        public int Count => (int)SamusMovementType.Special + 1;
        /// <summary>Movement-type entry count, equivalent to <see cref="Count"/>, used to reject unsupported death-entry movement identities.</summary>
        public int Length => Count;
        /// <summary>Returns the initial visible death-pose animation index for one prior movement type, without replacing the separately initialized animation timer.</summary>
        /// <param name="index">Native movement-type value $00..$1B, not a death-sequence phase or elapsed-update count.</param>
        /// <returns>One for ordinary ball movement, zero for the two unused glitch-ball types, or five for all other supported movement types.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The movement-type index is outside $00..$1B.</exception>
        public byte this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                return (SamusMovementType)index switch
                {
                    SamusMovementType.MorphBallGround or SamusMovementType.MorphBallFalling or
                    SamusMovementType.SpringBallGround or SamusMovementType.SpringBallInAir or
                    SamusMovementType.SpringBallFalling => 1,
                    SamusMovementType.UnusedGlitchBall or SamusMovementType.UnusedGlitchBallAlternate => 0,
                    SamusMovementType.Standing or SamusMovementType.Running or SamusMovementType.NormalJumping or
                        SamusMovementType.SpinJumping or SamusMovementType.Crouching or SamusMovementType.Falling or
                        SamusMovementType.Knockback or SamusMovementType.Unused0B or SamusMovementType.Unused0C or
                        SamusMovementType.Unused0D or SamusMovementType.TurningOnGround or
                        SamusMovementType.PostureTransition or SamusMovementType.Moonwalking or
                        SamusMovementType.WallJumping or SamusMovementType.RanIntoWall or SamusMovementType.Grappling or
                        SamusMovementType.TurningWhileJumping or SamusMovementType.TurningWhileFalling or
                        SamusMovementType.DamageBoost or SamusMovementType.DraygonHeld or SamusMovementType.Special => 5,
                    _ => throw new InvalidOperationException($"Undefined SamusMovementType {(SamusMovementType)index}."),
                };
            }
        }
        /// <summary>Enumerates the initial death-animation indices in ascending native movement-type order.</summary>
        /// <returns>An enumerator yielding all twenty-eight movement-type selections.</returns>
        public IEnumerator<byte> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Exposes movement-type death-frame selection to non-generic enumeration.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
