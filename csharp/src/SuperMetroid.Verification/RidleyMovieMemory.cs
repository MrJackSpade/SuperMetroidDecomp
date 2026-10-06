/// <summary>Native WRAM identities used only to import the supplied Ridley movie initial-state fixture.</summary>
internal static class RidleyMovieMemory
{
    /// <summary>$090F: layer-one X subposition used by horizontal scrolling.</summary>
    public const int CameraXFraction = 0x090f;
    /// <summary>$0913: layer-one Y subposition used by vertical scrolling.</summary>
    public const int CameraYFraction = 0x0913;
    /// <summary>$7E:7800: Ridley independent swoop phase timer.</summary>
    public const int RidleySwoopTimer = 0x7800;
    /// <summary>$09E4: native Moonwalk option preserved by the movie snapshot.</summary>
    public const int MoonwalkOption = 0x09e4;
    /// <summary>$099C: bank-$82 door-transition dispatcher, used to align completed loading owners.</summary>
    public const int DoorFunction = 0x099c;
    /// <summary>$82:E3C0: PlaceSamusLoadTiles; decompression overlaps IRQ movement.</summary>
    public const ushort PlaceSamusLoadTiles = 0xe3c0;
    /// <summary>$82:E4A9: LoadMoreThings; initializes destination owners and waits for scrolling.</summary>
    public const ushort LoadMoreThings = 0xe4a9;
    /// <summary>$82:E659: HandleAnimTiles; destination loading and its coroutine have completed.</summary>
    public const ushort HandleAnimTiles = 0xe659;
    /// <summary>$A6:E546: Ridley's first displayed instruction after the E737 fade owner runs.</summary>
    public const ushort RidleyFirstFadeInstruction = 0xe546;
    /// <summary>$A6:E9A5: native Ridley body sprite installed on the first fade update.</summary>
    public const ushort RidleyFirstFadeSpritemap = 0xe9a5;
    /// <summary>$0797: door_transition_flag_enemies; gates the shared Ridley reveal wait.</summary>
    public const int EnemyDoorTransition = 0x0797;
    /// <summary>$0FA8: Ridley's slot-zero var_A, the native AI function.</summary>
    public const int RidleyFunction = 0x0fa8;
    /// <summary>$0FB2: Ridley's slot-zero var_F, the native function timer.</summary>
    public const int RidleyFunctionTimer = 0x0fb2;
    /// <summary>$A6:E967: sleep cursor retained while right-facing Ridley already faces room middle.</summary>
    public const ushort RidleyRightFlyingSleep = 0xe967;
    /// <summary>$20A4: tilemap_stuff[82], solved Ridley tail tip X.</summary>
    public const int TailTipX = 0x20a4;
    /// <summary>$20A6: tilemap_stuff[83], solved Ridley tail tip Y.</summary>
    public const int TailTipY = 0x20a6;
    /// <summary>$0B64: projectile_x_pos, native ordinary projectile X array.</summary>
    public const int ProjectileX = 0x0b64;
    /// <summary>$0B78: projectile_y_pos, native ordinary projectile Y array.</summary>
    public const int ProjectileY = 0x0b78;
    /// <summary>$0BB4: projectile_x_radius.</summary>
    public const int ProjectileXRadius = 0x0bb4;
    /// <summary>$0BC8: projectile_y_radius.</summary>
    public const int ProjectileYRadius = 0x0bc8;
    /// <summary>$0C18: projectile_type.</summary>
    public const int ProjectileType = 0x0c18;
    /// <summary>$0C2C: projectile_damage.</summary>
    public const int ProjectileDamage = 0x0c2c;
    /// <summary>$0CCC: shared beam/bomb cooldown.</summary>
    public const int ProjectileCooldown = 0x0ccc;
    /// <summary>$0CD0: beam flare/charge counter.</summary>
    public const int BeamCharge = 0x0cd0;
    /// <summary>$1997: enemy projectile identities.</summary>
    public const int EnemyProjectileId = 0x1997;
    /// <summary>$1A4B: enemy projectile X array.</summary>
    public const int EnemyProjectileX = 0x1a4b;
    /// <summary>$1A93: enemy projectile Y array.</summary>
    public const int EnemyProjectileY = 0x1a93;
    /// <summary>$1BB3: packed enemy projectile radii.</summary>
    public const int EnemyProjectileRadius = 0x1bb3;
    /// <summary>$1BD7: enemy projectile properties; low twelve bits hold damage.</summary>
    public const int EnemyProjectileProperties = 0x1bd7;
    /// <summary>$079B: room_ptr.</summary>
    public const int Room = 0x079b;
    /// <summary>$0911: layer1_x_pos.</summary>
    public const int CameraX = 0x0911;
    /// <summary>$0915: layer1_y_pos.</summary>
    public const int CameraY = 0x0915;
    /// <summary>$09A2: equipped_items; imported without granting extra equipment.</summary>
    public const int Items = 0x09a2;
    /// <summary>$09A6: equipped_beams.</summary>
    public const int Beams = 0x09a6;
    /// <summary>$09C2: samus_health.</summary>
    public const int Health = 0x09c2;
    /// <summary>$09C4: samus_max_health.</summary>
    public const int MaxHealth = 0x09c4;
    /// <summary>$09C0: reserve_health_mode, initial reserve policy.</summary>
    public const int ReserveMode = 0x09c0;
    /// <summary>$09D4: samus_max_reserve_health, collected reserve capacity.</summary>
    public const int MaxReserve = 0x09d4;
    /// <summary>$09D6: samus_reserve_health, available reserve energy.</summary>
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
    /// <summary>$D870: persistent collected-item bits, including previously collected items.</summary>
    public const int CollectedItemBits = 0xd870;
    /// <summary>$7F:0002: decompressed foreground collision words.</summary>
    public const int Level = 0x10002;
    /// <summary>$7F:6402: decompressed BTS bytes.</summary>
    public const int Bts = 0x16402;
    /// <summary>$05E5: random_number, imported from the original movie.</summary>
    public const int Random = 0x5e5;
    /// <summary>$05B6: nmi_frame_counter_word, imported from the original movie.</summary>
    public const int NmiCounter = 0x5b6;
    /// <summary>$05B5: nmi_frame_counter_byte, imported from the original movie.</summary>
    public const int NmiCounterByte = 0x5b5;
    /// <summary>$008B: joypad1_lastkeys, imported from the original movie.</summary>
    public const int HeldInput = 0x8b;
    /// <summary>$09A4: collected_items, imported from the original movie.</summary>
    public const int CollectedItems = 0x9a4;
    /// <summary>$09A8: collected_beams, imported from the original movie.</summary>
    public const int CollectedBeams = 0x9a8;
    /// <summary>$09C6: samus_missiles, imported from the original movie.</summary>
    public const int Missiles = 0x9c6;
    /// <summary>$09C8: samus_max_missiles, imported from the original movie.</summary>
    public const int MaxMissiles = 0x9c8;
    /// <summary>$09CA: samus_super_missiles, imported from the original movie.</summary>
    public const int SuperMissiles = 0x9ca;
    /// <summary>$09CC: samus_max_super_missiles, imported from the original movie.</summary>
    public const int MaxSuperMissiles = 0x9cc;
    /// <summary>$09CE: samus_power_bombs, imported from the original movie.</summary>
    public const int PowerBombs = 0x9ce;
    /// <summary>$09D0: samus_max_power_bombs, imported from the original movie.</summary>
    public const int MaxPowerBombs = 0x9d0;
    /// <summary>$D828: boss_bits_for_area, imported from the original movie.</summary>
    public const int BossBits = 0xd828;
    /// <summary>$D820: events_that_happened, imported from the original movie.</summary>
    public const int Events = 0xd820;
    /// <summary>$D8B0: opened_door_bit_array, imported from the original movie.</summary>
    public const int OpenedDoors = 0xd8b0;
    /// <summary>$18F0: first HDMA object pre-instruction, imported from the original movie.</summary>
    public const int AcidHdmaPreInstruction = 0x18f0;
    /// <summary>$B3B0: bank-$88 lava/acid BG3 pre-instruction, imported from the original movie.</summary>
    public const int AcidHdmaCallback = 0xb3b0;
    /// <summary>$0F78: enemy_data, imported from the original movie.</summary>
    public const int EnemyBase = 0xf78;
    /// <summary>$7800: enemy extra variables, imported from the original movie.</summary>
    public const int EnemyExtra = 0x7800;
    /// <summary>$7802: Brinstar pipe bug variable 01, imported from the original movie.</summary>
    public const int EnemyExtraPreviousAnimation = 0x7802;
    /// <summary>$1C37: plm_header_ptr, imported from the original movie.</summary>
    public const int PlmHeaders = 0x1c37;
    /// <summary>$0998: game_state, imported from the original movie.</summary>
    public const int GameState = 0x998;
    /// <summary>$8F:B37A: room containing the movie's initial snapshot.</summary>
    public const ushort SourceRoom = 0xb37a;
    /// <summary>$8F:B32E: Ridley's room, entered in the supplied movie.</summary>
    public const ushort RidleyRoom = 0xb32e;
    /// <summary>$80:9459: ReadControllerInput, accepted-NMI input checkpoint.</summary>
    public const int ReadControllerInput = 0x809459;
    /// <summary>$B3:8A25: strong pipe-bug visual cursor in native update 157.</summary>
    public const ushort PipeBugBeforeFadeInstruction = 0x8a25;
    /// <summary>$B3:8A29: strong pipe-bug visual cursor after native update 158.</summary>
    public const ushort PipeBugAfterFadeInstruction = 0x8a29;
    /// <summary>$B3:8A89: strong pipe-bug spritemap before the first source fade step.</summary>
    public const ushort PipeBugBeforeFadeSpritemap = 0x8a89;
    /// <summary>$B3:8A90: strong pipe-bug spritemap after the first source fade step.</summary>
    public const ushort PipeBugAfterFadeSpritemap = 0x8a90;
}
