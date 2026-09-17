/// <summary>Native WRAM identities used only to import the issue-638 floor-clip fixture.</summary>
internal static class EtecoonFloorClipMemory
{
    /// <summary>Room-local X block containing the movie's authored shootable floor.</summary>
    public const int FloorBlockX = 7;
    /// <summary>Room-local Y block containing the movie's authored shootable floor.</summary>
    public const int FloorBlockY = 106;
    /// <summary>World-space top edge of the floor crossed by the successful setup.</summary>
    public const ushort FloorTopY = 0x06a0;

    /// <summary>$05B6: nmi_frame_counter_word's independent low-byte animation counter.</summary>
    public const int NmiFrameCounter8 = 0x05b6;
    /// <summary>$05B8: nmi_frame_counter_word.</summary>
    public const int NmiFrameCounter = 0x05b8;
    /// <summary>$079B: room_ptr.</summary>
    public const int Room = 0x079b;
    /// <summary>$0911: layer1_x_pos.</summary>
    public const int CameraX = 0x0911;
    /// <summary>$0915: layer1_y_pos.</summary>
    public const int CameraY = 0x0915;
    /// <summary>$0998: game_state.</summary>
    public const int GameState = 0x0998;
    /// <summary>$09A2: equipped_items.</summary>
    public const int Items = 0x09a2;
    /// <summary>$09A4: collected_items.</summary>
    public const int CollectedItems = 0x09a4;
    /// <summary>$09A6: equipped_beams.</summary>
    public const int Beams = 0x09a6;
    /// <summary>$09A8: collected_beams.</summary>
    public const int CollectedBeams = 0x09a8;
    /// <summary>$09C0: reserve tank mode.</summary>
    public const int ReserveMode = 0x09c0;
    /// <summary>$09C2: samus_health.</summary>
    public const int Health = 0x09c2;
    /// <summary>$09C4: samus_max_health.</summary>
    public const int MaxHealth = 0x09c4;
    /// <summary>$09D4: samus_max_reserve_health.</summary>
    public const int MaxReserve = 0x09d4;
    /// <summary>$09D6: samus_reserve_health.</summary>
    public const int Reserve = 0x09d6;
    /// <summary>$0A1C: samus_pose.</summary>
    public const int Pose = 0x0a1c;
    /// <summary>$0A20: samus_prev_pose.</summary>
    public const int PreviousPose = 0x0a20;
    /// <summary>$0A22: previous direction/movement byte pair.</summary>
    public const int PreviousDirection = 0x0a22;
    /// <summary>$0A24: samus_last_different_pose.</summary>
    public const int LastDifferentPose = 0x0a24;
    /// <summary>$0A26: last-different direction/movement byte pair.</summary>
    public const int LastDifferentDirection = 0x0a26;
    /// <summary>$0A48: hurt-flash counter.</summary>
    public const int HurtFlashCounter = 0x0a48;
    /// <summary>$0A52: knockback direction selected by the cartridge hit interruption.</summary>
    public const int KnockbackDirection = 0x0a52;
    /// <summary>$0A54: horizontal direction published by the damage source.</summary>
    public const int KnockbackXDirection = 0x0a54;
    /// <summary>$0A94: samus_anim_frame_timer.</summary>
    public const int AnimationTimer = 0x0a94;
    /// <summary>$0A96: samus_anim_frame.</summary>
    public const int Animation = 0x0a96;
    /// <summary>$0AF6: samus_x_pos.</summary>
    public const int X = 0x0af6;
    /// <summary>$0AF8: samus_x_subpos.</summary>
    public const int XFraction = 0x0af8;
    /// <summary>$0AFA: samus_y_pos.</summary>
    public const int Y = 0x0afa;
    /// <summary>$0AFC: samus_y_subpos.</summary>
    public const int YFraction = 0x0afc;
    /// <summary>$0AFE: samus_x_radius.</summary>
    public const int XRadius = 0x0afe;
    /// <summary>$0B00: samus_y_radius.</summary>
    public const int YRadius = 0x0b00;
    /// <summary>$0B2C: samus_y_subspeed.</summary>
    public const int VerticalFraction = 0x0b2c;
    /// <summary>$0B2E: samus_y_speed.</summary>
    public const int VerticalSpeed = 0x0b2e;
    /// <summary>$0B36: samus_y_dir.</summary>
    public const int VerticalDirection = 0x0b36;
    /// <summary>$0B3C: samus_has_momentum_flag.</summary>
    public const int Momentum = 0x0b3c;
    /// <summary>$0B3E: speed_boost_counter.</summary>
    public const int BoostCounter = 0x0b3e;
    /// <summary>$0B42: samus_x_extra_run_speed.</summary>
    public const int ExtraSpeed = 0x0b42;
    /// <summary>$0B44: samus_x_extra_run_subspeed.</summary>
    public const int ExtraFraction = 0x0b44;
    /// <summary>$0B46: samus_x_base_speed.</summary>
    public const int BaseSpeed = 0x0b46;
    /// <summary>$0B48: samus_x_base_subspeed.</summary>
    public const int BaseFraction = 0x0b48;
    /// <summary>$0B4A: samus_x_accel_mode.</summary>
    public const int AccelerationMode = 0x0b4a;
    /// <summary>$18A8: Samus invincibility timer.</summary>
    public const int InvincibilityTimer = 0x18a8;
    /// <summary>$18AA: Samus knockback timer.</summary>
    public const int KnockbackTimer = 0x18aa;
    /// <summary>$D870: persistent collected-item bits.</summary>
    public const int CollectedItemBits = 0xd870;
    /// <summary>$7F:0002: decompressed foreground collision words.</summary>
    public const int Level = 0x10002;
    /// <summary>$7F:6402: decompressed BTS bytes.</summary>
    public const int Bts = 0x16402;
}
