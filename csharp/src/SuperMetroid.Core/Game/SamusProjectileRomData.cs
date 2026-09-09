namespace SuperMetroid.Core.Game;

/// <summary>Immutable cartridge tables and instruction pointers for Samus projectiles.</summary>
/// <remarks>
/// The native implementation spreads projectile metadata across banks $90, $91, $93,
/// $94, $9A, and $9B. Grouping by purpose keeps those cross-bank relationships explicit
/// while leaving live projectile slots, timers, and positions on the runtime system.
/// </remarks>
public static class SamusProjectileRomData
{
    /// <summary>Native banks used to expand same-bank projectile pointers.</summary>
    public static class Banks
    {
        /// <summary>Bank containing projectile producers, motion tables, and trails.</summary>
        public const int Movement = 0x900000;
        /// <summary>Eight-bit form of <see cref="Movement"/> for <c>SnesAddress</c>.</summary>
        public const byte MovementNumber = 0x90;
        /// <summary>Bank containing pose and projectile-driven suit-palette lists.</summary>
        public const int Pose = 0x910000;
        /// <summary>Bank containing projectile definitions and animation instructions.</summary>
        public const int Projectile = 0x930000;
        /// <summary>Eight-bit form of <see cref="Projectile"/> for <c>SnesAddress</c>.</summary>
        public const byte ProjectileNumber = 0x93;
        /// <summary>Bank containing terrain collision geometry.</summary>
        public const int Collision = 0x940000;
        /// <summary>Bank containing projectile character graphics.</summary>
        public const int CharacterData = 0x9a0000;
        /// <summary>Bank containing Samus palettes and beam-trail offsets.</summary>
        public const int PaletteAndTrailData = 0x9b0000;
    }

    /// <summary>Pose-relative projectile and charge-flare origins.</summary>
    public static class Origins
    {
        /// <summary>Ten direction-indexed X origins for non-running poses.</summary>
        public const int DefaultX = 0x90c204;
        /// <summary>Ten direction-indexed Y origins for non-running poses.</summary>
        public const int DefaultY = 0x90c218;
        /// <summary>Ten direction-indexed X origins for running poses.</summary>
        public const int RunningX = 0x90c22c;
        /// <summary>Ten direction-indexed Y origins for running poses.</summary>
        public const int RunningY = 0x90c240;
        /// <summary>Charge-flare X offsets for non-running poses.</summary>
        public const int FlareDefaultX = 0x90c1a8;
        /// <summary>Charge-flare Y offsets for non-running poses.</summary>
        public const int FlareDefaultY = 0x90c1c2;
        /// <summary>Charge-flare X offsets for running poses.</summary>
        public const int FlareRunningX = 0x90c1dc;
        /// <summary>Charge-flare Y offsets for running poses.</summary>
        public const int FlareRunningY = 0x90c1f0;
        /// <summary>Number of projectile directions represented by each origin table.</summary>
        public const int DirectionCount = 10;
    }

    /// <summary>Beam cooldown, audio, speed, acceleration, graphics, and palette tables.</summary>
    public static class Beams
    {
        /// <summary>Flare-counter value at which <c>$90:BAFC</c> starts SFX `$08`.</summary>
        public const ushort ChargeSoundStartCounter = 16;
        /// <summary>Flare-counter value at which release selects the charged beam family.</summary>
        public const ushort FullyChargedCounter = 60;
        /// <summary>Flare-counter clamp at which bank $90 begins testing special beam attacks.</summary>
        public const ushort SpecialAttackCounter = 120;
        /// <summary>Normal and cooldown-cancel delays indexed by beam combination.</summary>
        public const int UnchargedCooldowns = 0x90c254;
        /// <summary>Offset from normal delays to the cooldown-cancel delay row.</summary>
        public const int CooldownCancelRowOffset = 0x10;
        /// <summary>Held-Shot repeat delays indexed by beam combination.</summary>
        public const int AutoFireCooldowns = 0x90c283;
        /// <summary>Uncharged firing sound IDs indexed by beam combination.</summary>
        public const int UnchargedSounds = 0x90c28f;
        /// <summary>Charged firing sound IDs indexed by beam combination.</summary>
        public const int ChargedSounds = 0x90c2a7;
        /// <summary>Speed word for horizontal and vertical power-beam travel.</summary>
        public const int HorizontalVerticalSpeeds = 0x90c2d1;
        /// <summary>Speed word for diagonal power-beam travel.</summary>
        public const int DiagonalSpeeds = 0x90c2d3;
        /// <summary>$90:B197 indexes $C2D1 by four times the low-nibble beam combination: two speed words per row.</summary>
        public const int InitialSpeedRowBytes = 2 * sizeof(ushort);
        /// <summary>Direction-indexed X acceleration words.</summary>
        public const int XAccelerations = 0x90c353;
        /// <summary>Direction-indexed Y acceleration words.</summary>
        public const int YAccelerations = 0x90c367;
        /// <summary>Beam-combination-indexed character-data pointers.</summary>
        public const int TilePointers = 0x90c3b1;
        /// <summary>Beam-combination-indexed palette pointers.</summary>
        public const int PalettePointers = 0x90c3c9;
        /// <summary>Bank-$93 projectile-data pointers for uncharged combinations.</summary>
        public const int UnchargedDataPointers = 0x9383c1;
        /// <summary>Bank-$93 projectile-data pointers for charged combinations.</summary>
        public const int ChargedDataPointers = 0x9383d9;
        /// <summary>Pointers to the three charge-flare component delay lists.</summary>
        public const int ChargeFlareDelayListPointers = 0x90c481;
        /// <summary>Number of beam combinations present in each complete pointer table.</summary>
        public const int CombinationCount =
            (ChargedDataPointers - UnchargedDataPointers) / sizeof(ushort);

        /// <summary>Damage word followed by one instruction pointer per direction.</summary>
        public const int DataRecordByteCount =
            sizeof(ushort) + Origins.DirectionCount * sizeof(ushort);

        /// <summary>Duration, spritemap, radii, and animation word in one instruction record.</summary>
        public const int InstructionRecordByteCount = 8;
    }

    /// <summary>Projectile-driven Samus and beam palette pointer tables.</summary>
    public static class Palettes
    {
        /// <summary>$90:ACCD writes the sixteen beam colors beginning at CGRAM index $E0.</summary>
        public const int BeamDestinationIndex = 0xe0;
        /// <summary>Suit-indexed pointers to ordinary Samus palettes.</summary>
        public const int NormalSuitPointers = 0x91d727;
        /// <summary>Suit-indexed pointers to charged-beam palette instruction lists.</summary>
        public const int BeamChargePointers = 0x91d7d5;
        /// <summary>Suit-indexed pointers to pseudo-Screw palette instruction lists.</summary>
        public const int PseudoScrewPointers = 0x91d7ff;
        /// <summary>Hyper Beam shot palette pointers indexed by flash frame.</summary>
        public const int HyperBeamShotPointers = 0x91d829;
        /// <summary>First CGRAM color occupied by Samus's OBJ palette.</summary>
        public const int SamusCgramIndex = 192;
        /// <summary>Number of colors copied by one projectile-driven palette update.</summary>
        public const int ColorCount = 16;
    }

    /// <summary>Missile, Super Missile, bomb, and linked-projectile definitions.</summary>
    public static class NonBeam
    {
        /// <summary>Bank-$93 data pointers for missiles, bombs, and special projectiles.</summary>
        public const int DataPointers = 0x9383f1;
        /// <summary>Bank-$93 definitions for Super Missile link projectiles.</summary>
        public const int SuperMissileLinkDataPointers = 0x93842b;
        /// <summary>Direction-indexed missile acceleration records.</summary>
        public const int MissileAccelerations = 0x90c303;
        /// <summary>Direction-indexed Super Missile acceleration records.</summary>
        public const int SuperMissileAccelerations = 0x90c32b;
        /// <summary>Pointer cell selecting ordinary beam explosion instructions.</summary>
        public const int BeamExplosionInstructionPointer = 0x93867b;
        /// <summary>Pointer cell selecting missile explosion instructions.</summary>
        public const int MissileExplosionInstructionPointer = 0x93867f;
        /// <summary>Pointer cell selecting bomb explosion instructions.</summary>
        public const int BombExplosionInstructionPointer = 0x938683;
        /// <summary>Pointer cell selecting Super Missile explosion instructions.</summary>
        public const int SuperMissileExplosionInstructionPointer = 0x938693;
        /// <summary>Bytes in one direction's acceleration/subacceleration record.</summary>
        public const int AccelerationRecordByteCount = 4;
    }

    /// <summary>Projectile collision geometry read from bank $94.</summary>
    public static class Collision
    {
        /// <summary>Sixteen height bytes for each non-square slope shape.</summary>
        public const int NonSquareSlopeDefinitions = 0x948b2b;
        /// <summary>Four-quadrant solidity bytes for square slope shapes.</summary>
        public const int SquareSlopeDefinitions = 0x948e54;
    }

    /// <summary>Beam-trail instructions and direction/frame offset families.</summary>
    public static class Trails
    {
        /// <summary>Left trail instruction-list pointers for every supported projectile.</summary>
        public const int LeftInstructionPointers = 0x90b5bb;
        /// <summary>Right trail instruction-list pointers for every supported projectile.</summary>
        public const int RightInstructionPointers = 0x90b609;
        /// <summary>Uncharged beam-combination offset-family pointers.</summary>
        public const int UnchargedOffsetFamilies = 0x9ba4b3;
        /// <summary>Charged beam-combination offset-family pointers.</summary>
        public const int ChargedOffsetFamilies = 0x9ba4cb;
        /// <summary>Spazer special-beam-attack offset-family pointers.</summary>
        public const int SpazerSbaOffsetFamilies = 0x9ba4e3;
        /// <summary>Left-stream instruction that moves its sprite one pixel down.</summary>
        public const ushort MoveLeftDown = 0xb525;
        /// <summary>Right-stream instruction that moves its sprite one pixel down.</summary>
        public const ushort MoveRightDown = 0xb587;
        /// <summary>Left-stream instruction that moves its sprite one pixel up.</summary>
        public const ushort MoveLeftUp = 0xb5b3;
        /// <summary>Number of trail pointer entries before the paired right table.</summary>
        public const int InstructionPointerCount =
            (RightInstructionPointers - LeftInstructionPointers) / sizeof(ushort);
    }

    /// <summary>Shared bank-$93 projectile instruction opcodes.</summary>
    public static class Instructions
    {
        /// <summary><c>$93:822F</c>, delete the current projectile slot.</summary>
        public const ushort Delete = 0x822f;
        /// <summary><c>$93:8239</c>, replace the instruction-list cursor.</summary>
        public const ushort GoTo = 0x8239;
    }
}
