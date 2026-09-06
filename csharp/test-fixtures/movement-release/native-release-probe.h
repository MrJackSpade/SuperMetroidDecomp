// Headless cartridge-CPU experiment for #312/#313. This constructs an empty room
// with a flat floor; it contains no player recording, SRAM, or cartridge bytes.
// Include from sm_rtl.c and dispatch DiagnosticMovementRelease before SDL starts.
#include "snes/cart.h"
#ifdef _WIN32
// Avoid windows.h's ScreenOff/IDA macro collisions in the translated game unit.
__declspec(dllimport) unsigned int __stdcall SetErrorMode(unsigned int mode);
enum { ProbeNoCriticalErrorDialog = 1, ProbeNoFaultDialog = 2 };
#endif
static int ProbeLoadRetailMovementRom(const char *rom) {
#ifdef _WIN32
  SetErrorMode(ProbeNoCriticalErrorDialog | ProbeNoFaultDialog);
#endif
  if (!SnesInit(rom)) return 2;
  // SnesInit patches carry instructions for the C/CPU comparison harness. Those
  // patches are inappropriate for a cartridge golden trace: restore the bytes.
  size_t rom_length = 0;
  uint8 *retail = ReadWholeFile(rom, &rom_length);
  size_t header = rom_length & 0x7fff;
  if (!retail || (header != 0 && header != 512) ||
      rom_length - header > g_snes->cart->romSize) {
    free(retail);
    fprintf(stderr, "Cartridge size mismatch in movement probe.\n");
    return 3;
  }
  memcpy(g_snes->cart->rom, retail + header, rom_length - header);
  free(retail);
  return 0;
}
int DiagnosticMovementRelease(const char *rom) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  for (int water = 0; water < 2; water++)
  for (int keep_direction = 0; keep_direction < 2; keep_direction++) {
    cpu_reset(g_snes->cpu);
    memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false;
    g_snes->cpu->sp = 0x1ff0;
    g_snes->cpu->dp = 0;
    room_width_in_blocks = 16;
    room_height_in_blocks = 32;
    interactive_enemy_indexes[0] = 0xffff;
    for (int i = 256; i < 272; i++) level_data[i] = 0x8000;
    fx_y_pos = water ? 8 : 0xffff;
    lava_acid_y_pos = 0xffff;
    fx_liquid_options = 0x80;
    fx_type = water ? 6 : 0;
    liquid_physics_type = water;
    samus_x_pos = 200;
    samus_y_pos = 235;
    samus_x_radius = 5;
    samus_y_radius = 21;
    samus_pose = samus_prev_pose = 10; // Running left.
    samus_pose_x_dir = samus_prev_pose_x_dir = 4;
    samus_movement_type = samus_prev_movement_type = samus_prev_movement_type2 = 1;
    samus_x_base_speed = 2;
    samus_x_base_subspeed = 0xc000; // Full running base speed, no extra run speed.
    samus_x_speed_table_pointer = 0x9f55; // Retail normal table, not preceding grapple entry.
    samus_input_handler = 0xe913;
    samus_health = 99;
    button_config_run_b = 0x8000;
    samus_anim_frame_timer = 5;
    int32 travelled = 0;
    for (int frame = 0; frame < 100; frame++) {
      // Recenter between samples to keep the finite synthetic floor from ending.
      // Accumulate accepted movement separately; do not reset velocity or pose.
      samus_x_pos = 200;
      samus_x_subpos = 0;
      samus_new_pose = samus_new_pose_interrupted = samus_new_pose_transitional = 0xffff;
      samus_momentum_routine_index = samus_special_transgfx_index = samus_hurt_switch_index = 0;
      joypad1_lastkeys = keep_direction ? 0x200 : 0;
      joypad1_newkeys = 0;
      // Retail alpha input, beta movement/animation, then collision and pose
      // transitions. Execute the actual 65816 instructions, not translated C.
      RunAsmCode(0x918000, 0, 0, 0, 0);
      RunAsmCode(0x90a337, 0, 0, 0, 0);
      printf("MOVE water=%d x=%04X.%04X base=%04X.%04X extra=%04X.%04X external=%04X.%04X table=%04X collision=%d\n", water,
        samus_x_pos, samus_x_subpos, samus_x_base_speed, samus_x_base_subspeed,
        samus_x_extra_run_speed, samus_x_extra_run_subspeed, extra_samus_x_displacement,
        extra_samus_x_subdisplacement, samus_x_speed_table_pointer, samus_collision_flag);
      RunAsmCode(0x908000, 0, 0, 0, 0);
      RunAsmCode(0x91e8b6, 0, 0, 0, 0);
      RunAsmCode(0x91eb88, 0, 0, 0, 0);
      travelled += (200 << 16) - ((samus_x_pos << 16) | samus_x_subpos);
      printf("RELEASE water=%d frame=%d x=%04X.%04X speed=%04X.%04X mode=%d pose=%04X\n",
        water, frame, samus_x_pos, samus_x_subpos, samus_x_base_speed,
        samus_x_base_subspeed, samus_x_accel_mode, samus_pose);
      if (frame == 99) printf("STOP water=%d keep-direction=%d distance=%08X pose=%04X speed=%04X.%04X\n", water, keep_direction, travelled, samus_pose, samus_x_base_speed, samus_x_base_subspeed);
    }
  }
  // Isolate the post-ledge falling speed calculation from collision and rendering.
  // Mode two can enter this calculator, whose signed fractional cap can alternate.
  // This is arithmetic-only evidence: falling-pose initialization resets the mode
  // without extra dash speed, so a normal no-input walk-off must not retain it.
  for (int water = 0; water < 2; water++) {
    memset(g_ram, 0, sizeof(g_ram));
    samus_x_accel_mode = 2;
    samus_x_base_speed = 2;
    samus_x_base_subspeed = 0x9800;
    uint16 entry = (water ? 0xa08d : 0x9f55) + 12 * 6;
    for (int frame = 0; frame < 20; frame++) {
      RunAsmCode(0x909b1f, 0, entry, 0, 0);
      printf("FALL_SPEED water=%d frame=%d base=%04X.%04X mode=%d\n",
        water, frame, samus_x_base_speed, samus_x_base_subspeed, samus_x_accel_mode);
    }
  }
  // Standing short taps exercise a different input/animation path from running
  // release. Warm both liquid animation state and the standing pose before the
  // press, then retain the complete trajectory rather than only final facing.
  for (int water = 0; water < 2; water++)
  for (int left = 0; left < 2; left++)
  for (int duration = 1; duration <= 3; duration++) {
    cpu_reset(g_snes->cpu);
    memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false;
    g_snes->cpu->sp = 0x1ff0;
    room_width_in_blocks = 16;
    room_height_in_blocks = 32;
    interactive_enemy_indexes[0] = 0xffff;
    for (int i = 256; i < 272; i++) level_data[i] = 0x8000;
    fx_y_pos = water ? 8 : 0xffff;
    lava_acid_y_pos = 0xffff;
    fx_liquid_options = 0x80;
    fx_type = water ? 6 : 0;
    liquid_physics_type = water;
    samus_x_pos = 200;
    samus_y_pos = 235;
    samus_x_radius = 5;
    samus_y_radius = 21;
    samus_pose = samus_prev_pose = left ? 1 : 2;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 8 : 4;
    samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913;
    samus_health = 99;
    button_config_run_b = 0x8000;
    samus_anim_frame_timer = 5;
    for (int frame = -24; frame < 60; frame++) {
      samus_new_pose = samus_new_pose_interrupted = samus_new_pose_transitional = 0xffff;
      samus_momentum_routine_index = samus_special_transgfx_index = samus_hurt_switch_index = 0;
      joypad1_lastkeys = frame >= 0 && frame < duration ? (left ? 0x200 : 0x100) : 0;
      joypad1_newkeys = frame == 0 ? joypad1_lastkeys : 0;
      RunAsmCode(0x918000, 0, 0, 0, 0);
      RunAsmCode(0x90a337, 0, 0, 0, 0);
      RunAsmCode(0x908000, 0, 0, 0, 0);
      RunAsmCode(0x91e8b6, 0, 0, 0, 0);
      RunAsmCode(0x91eb88, 0, 0, 0, 0);
      if (frame >= 0)
        printf("TAP water=%d left=%d duration=%d frame=%d x=%04X%04X pose=%02X base=%04X%04X anim=%d timer=%d\n",
          water, left, duration, frame, samus_x_pos, samus_x_subpos, samus_pose,
          samus_x_base_speed, samus_x_base_subspeed, samus_anim_frame, samus_anim_frame_timer);
    }
  }
  return 0;
}

// The optional seed is private diagnostic data: 32 little-endian uint32 fields,
// followed by one uint16 foreground word + one BTS byte for each room block.
// No room-specific speed or collision correction is applied here. Only the
// original cartridge's movement stages execute after these initial RAM writes.
int DiagnosticRoomRelease(const char *rom, const char *seed_path) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  size_t size = 0;
  uint8 *seed = ReadWholeFile(seed_path, &size);
  if (!seed || size < 128) { free(seed); return 4; }
  uint32 *w = (uint32 *)seed;
  if (w[0] != 0x31564f4d || (w[6] != 9 && w[6] != 10) || !w[2] || !w[3] || w[2] > 256 || w[3] > 256 ||
      w[2] * w[3] > 6400 || size != 128 + w[2] * w[3] * 3) {
    fprintf(stderr, "Invalid movement room seed.\n"); free(seed); return 4;
  }
  cpu_reset(g_snes->cpu);
  memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false;
  g_snes->cpu->sp = 0x1ff0;
  g_snes->cpu->dp = 0;
  room_ptr = w[1]; room_width_in_blocks = w[2]; room_height_in_blocks = w[3];
  room_size_in_blocks = w[2] * w[3] * 2;
  for (uint32 i = 0; i < w[2] * w[3]; i++) {
    level_data[i] = seed[128 + i * 3] | seed[129 + i * 3] << 8;
    BTS[i] = seed[130 + i * 3];
  }
  interactive_enemy_indexes[0] = 0xffff;
  samus_x_pos = w[4] >> 16; samus_x_subpos = w[4];
  samus_y_pos = w[5] >> 16; samus_y_subpos = w[5];
  samus_pose = samus_prev_pose = w[6];
  samus_x_radius = w[7]; samus_y_radius = w[8];
  samus_x_base_speed = w[9] >> 16; samus_x_base_subspeed = w[9];
  samus_x_extra_run_speed = w[10] >> 16; samus_x_extra_run_subspeed = w[10];
  samus_x_accel_mode = w[11]; samus_has_momentum_flag = w[12];
  samus_x_speed_divisor = w[13]; samus_x_decel_mult = w[14];
  equipped_items = w[15]; equipped_beams = w[16];
  fx_type = w[17]; fx_y_pos = w[18]; lava_acid_y_pos = w[19];
  fx_liquid_options = w[20]; liquid_physics_type = w[21];
  samus_anim_frame = w[22]; samus_anim_frame_timer = w[23]; samus_anim_frame_buffer = w[24];
  samus_y_speed = w[25] >> 16; samus_y_subspeed = w[25]; samus_y_dir = w[26];
  samus_total_x_speed = w[27] >> 16; samus_total_x_subspeed = w[27];
  extra_samus_x_displacement = w[28] >> 16; extra_samus_x_subdisplacement = w[28];
  extra_samus_y_displacement = w[29] >> 16; extra_samus_y_subdisplacement = w[29];
  samus_y_accel = w[30]; samus_y_subaccel = w[31];
  samus_pose_x_dir = samus_prev_pose_x_dir = (samus_pose & 1) ? 8 : 4;
  samus_movement_type = samus_prev_movement_type = samus_prev_movement_type2 = 1;
  samus_x_speed_table_pointer = 0x9f55;
  samus_input_handler = 0xe913;
  samus_health = 99;
  button_config_run_b = 0x8000;
  free(seed);
  for (int frame = 0; frame < 20; frame++) {
    samus_new_pose = samus_new_pose_interrupted = samus_new_pose_transitional = 0xffff;
    samus_momentum_routine_index = samus_special_transgfx_index = samus_hurt_switch_index = 0;
    joypad1_lastkeys = joypad1_newkeys = 0;
    RunAsmCode(0x918000, 0, 0, 0, 0);
    RunAsmCode(0x909c5b, 0, 0, 0, 0);
    RunAsmCode(0x90a337, 0, 0, 0, 0);
    RunAsmCode(0x908000, 0, 0, 0, 0);
    RunAsmCode(0x91e8b6, 0, 0, 0, 0);
    RunAsmCode(0x91eb88, 0, 0, 0, 0);
    printf("ROOM_RELEASE frame=%d x=%04X%04X y=%04X%04X pose=%02X base=%04X%04X mode=%d collision=%d\n",
      frame, samus_x_pos, samus_x_subpos, samus_y_pos, samus_y_subpos,
      samus_pose, samus_x_base_speed, samus_x_base_subspeed, samus_x_accel_mode, samus_collision_flag);
  }
  return 0;
}
