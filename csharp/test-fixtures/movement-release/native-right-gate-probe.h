#include "native-release-probe.h"
#include "native-bounded-cpu.h"

// #404: original-CPU right-facing gate-origin sweep in the authored Pink
// Brinstar Hopper Room. The bounds and frame ordering match
// GateGlitchRoomAudit.SweepRightFacingRetailGate exactly.
int DiagnosticRightGateOrigins(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;

  const uint8 *state = RomFixedPtr(0x8fa13d);
  uint32 level = state[0] | state[1] << 8 | state[2] << 16;
  uint16 population = state[20] | state[21] << 8;
  enum {
    kRoomWidth = 32,
    kRoomHeight = 32,
    kRoomBlockCount = kRoomWidth * kRoomHeight,
    kGateX = 272,
    kGateY = 64,
    kCameraX = 144,
    kCameraY = 0,
  };

  fprintf(f, "x,y,hitFrame,shotX,shotY\n");
  for (int x = kGateX - 192; x < kGateX; x++)
  for (int y = kGateY - 32; y <= kGateY + 96; y++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0;
    room_width_in_blocks = kRoomWidth; room_height_in_blocks = kRoomHeight;
    room_width_in_scrolls = 2; room_height_in_scrolls = 2;
    room_size_in_blocks = kRoomBlockCount * 2; area_index = 1;
    DecompressToMem(level, g_ram + 0x10000);
    memcpy(BTS, (uint8 *)level_data + kRoomBlockCount * 2, kRoomBlockCount);
    samus_pose = samus_prev_pose = 1;
    samus_pose_x_dir = samus_prev_pose_x_dir = 8;
    samus_x_pos = samus_prev_x_pos = x;
    samus_y_pos = samus_prev_y_pos = y;
    hud_item_index = 2; samus_super_missiles = 10;
    button_config_shoot_x = 0x40;
    layer1_x_pos = kCameraX; layer1_y_pos = kCameraY; plm_flag = 0x8000;

    for (uint16 entry = population; ; entry += 6) {
      uint32 address = 0x8f0000 | entry;
      if (!GET_WORD(RomFixedPtr(address))) break;
      ProbeRunBoundedRegisters(0x84846a, 0, entry, 0);
    }
    int gate = -1;
    for (int i = 0; i < 40; i++) if (plm_header_ptr[i] == 0xc82a) gate = i;
    if (gate < 0) { fclose(f); return 8; }
    ProbeRunBounded(0x8485b4); ProbeRunBounded(0x8485b4);

    for (int frame = 0; frame < 30; frame++) {
      joypad1_lastkeys = joypad1_newkeys = frame ? 0 : 0x40;
      ProbeRunBounded(0x90ac1c);
      ProbeRunBounded(0x90be62);
      ProbeRunBounded(0x90aece);
      if (plm_timers[gate]) {
        fprintf(f, "%d,%d,%d,%d,%d\n", x, y, frame,
          projectile_x_pos[0], projectile_y_pos[0]);
        break;
      }
      ProbeRunBounded(0x8485b4); vram_write_queue_tail = 0;
    }
  }
  fclose(f); return 0;
}

// #404: original-CPU frozen-Boyon setup at East Tunnel. The managed seed is a
// private local carrier for the authored room collision words and exact Samus
// fixed-point state; the published CSV contains only the resulting mechanics.
int DiagnosticEastTunnelGate(const char *rom, const char *seed_path, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  size_t size = 0; uint8 *seed = ReadWholeFile(seed_path, &size);
  if (!seed || size < 128) { free(seed); return 10; }
  uint32 *w = (uint32 *)seed;
  if (w[0] != 0x31564f4d || w[1] != 0xcf80 || w[2] != 64 || w[3] != 32 ||
      size != 128 + w[2] * w[3] * 3 || w[6] != 1) {
    free(seed); return 11;
  }

  FILE *f = fopen(output, "wx"); if (!f) { free(seed); return 12; }
  fprintf(f, "enemyX,shootFrame,opened,openFrame,samusX,samusSubX,samusY,pose,shotX,shotY\n");
  const int enemy_xs[] = { 363, 364, 365 };
  const int shoot_frames[] = { 4, 5, 6 };
  for (int exi = 0; exi < 3; exi++)
  for (int sfi = 0; sfi < 3; sfi++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_ptr = w[1]; room_width_in_blocks = w[2]; room_height_in_blocks = w[3];
    room_size_in_blocks = w[2] * w[3] * 2;
    room_width_in_scrolls = w[2] / 16; room_height_in_scrolls = w[3] / 16;
    area_index = 4; up_scroller = 1; down_scroller = 1;
    memset(scrolls, 1, room_width_in_scrolls * room_height_in_scrolls);
    for (uint32 i = 0; i < w[2] * w[3]; i++) {
      level_data[i] = seed[128 + i * 3] | seed[129 + i * 3] << 8;
      BTS[i] = seed[130 + i * 3];
    }
    samus_x_pos = w[4] >> 16; samus_x_subpos = w[4];
    samus_y_pos = w[5] >> 16; samus_y_subpos = w[5];
    samus_pose = samus_prev_pose = w[6]; samus_x_radius = w[7]; samus_y_radius = w[8];
    samus_x_base_speed = w[9] >> 16; samus_x_base_subspeed = w[9];
    samus_x_extra_run_speed = w[10] >> 16; samus_x_extra_run_subspeed = w[10];
    samus_x_accel_mode = w[11]; samus_has_momentum_flag = w[12];
    samus_x_speed_divisor = w[13]; samus_x_decel_mult = w[14];
    equipped_items = w[15]; equipped_beams = w[16]; fx_type = w[17];
    fx_y_pos = w[18]; lava_acid_y_pos = w[19]; fx_liquid_options = w[20];
    liquid_physics_type = w[21]; samus_anim_frame = w[22];
    samus_anim_frame_timer = w[23]; samus_anim_frame_buffer = w[24];
    samus_y_speed = w[25] >> 16; samus_y_subspeed = w[25]; samus_y_dir = w[26];
    samus_total_x_speed = w[27] >> 16; samus_total_x_subspeed = w[27];
    extra_samus_x_displacement = w[28] >> 16; extra_samus_x_subdisplacement = w[28];
    extra_samus_y_displacement = w[29] >> 16; extra_samus_y_subdisplacement = w[29];
    samus_y_accel = w[30]; samus_y_subaccel = w[31];
    samus_pose_x_dir = samus_prev_pose_x_dir = samus_last_different_pose_x_dir = 8;
    samus_last_different_pose = 1; samus_prev_x_pos = samus_x_pos; samus_prev_y_pos = samus_y_pos;
    samus_x_speed_table_pointer = 0x9f55; samus_input_handler = 0xe913;
    samus_movement_handler = 0xa337; grapple_beam_function = 0xc4f0;
    samus_health = samus_max_health = 99; hud_item_index = 2;
    samus_super_missiles = samus_max_super_missiles = 10;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80;
    button_config_shoot_x = 0x40; button_config_aim_up_R = 0x10;
    button_config_aim_down_L = 0x20; button_config_itemcancel_y = 0x4000;
    button_config_itemswitch = 0x2000; layer1_x_pos = 128; layer1_y_pos = 0;
    plm_flag = 0x8000; first_free_enemy_index = 64; enemy_index_to_shake = 0xffff;

    for (uint16 entry = 0xc3e1; ; entry += 6) {
      uint32 address = 0x8f0000 | entry;
      if (!GET_WORD(RomFixedPtr(address))) break;
      ProbeRunBoundedRegisters(0x84846a, 0, entry, 0);
    }
    int gate = -1;
    for (int i = 0; i < 40; i++) if (plm_header_ptr[i] == 0xc82a) gate = i;
    if (gate < 0) { fclose(f); free(seed); return 13; }
    ProbeRunBounded(0x8485b4); ProbeRunBounded(0x8485b4);

    EnemyData *enemy = gEnemyData(0);
    enemy->enemy_ptr = 0xcebf; enemy->bank = 0xa2;
    enemy->x_pos = enemy_xs[exi]; enemy->y_pos = 139;
    enemy->x_width = enemy->y_height = 8; enemy->health = 1000;
    enemy->properties = 0x2800; enemy->extra_properties = 0x8000;
    enemy->ai_handler_bits = 4; enemy->frozen_timer = 1000; enemy->layer = 5;

    uint16 previous = 0;
    int opened = 0, open_frame = -1;
    for (int frame = 0; frame < 71; frame++) {
      int resumed = frame - 31;
      uint16 input = frame < 31 ? 0x8100 : 0x8090;
      if (resumed == shoot_frames[sfi]) input |= 0x40;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      nmi_frame_counter_word = frame + 1; nmi_frame_counter_byte = (uint8)(frame + 1);
      interactive_enemy_indexes_write_ptr = 2;
      interactive_enemy_indexes[0] = 0; interactive_enemy_indexes[1] = 0xffff;
      memset(enemy_drawing_queue_sizes, 0, 16);
      RunAsmCode(0x90e695, 0, 0, 0, 0);
      samus_contact_damage_index = 0;
      RunAsmCode(0x900000 | samus_movement_handler, 0, 0, 0, 0);
      uint32 stages[] = { 0x908000, 0x90dde9, 0x91e8b6, 0x91eb88,
        0x90eab3, 0x90e9ce, 0x9094ec, 0xa09169, 0xa08687 };
      for (int i = 0; i < 9; i++) RunAsmCode(stages[i], 0, 0, 0, 0);
      if (plm_timers[gate]) { opened = 1; open_frame = resumed; break; }
      ProbeRunBounded(0x8485b4); vram_write_queue_tail = 0;
    }
    fprintf(f, "%d,%d,%d,%d,%u,%u,%u,%u,%u,%u\n",
      enemy_xs[exi], shoot_frames[sfi], opened, open_frame,
      samus_x_pos, samus_x_subpos, samus_y_pos, samus_pose,
      projectile_x_pos[0], projectile_y_pos[0]);
  }
  fclose(f); free(seed); return 0;
}
