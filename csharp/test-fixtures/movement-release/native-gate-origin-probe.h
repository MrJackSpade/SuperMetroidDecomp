#include "native-bounded-cpu.h"

// #403: original room PLM setup, firing and collision for the managed origin sweep.
int DiagnosticGateOrigins(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  const uint8 *state = RomFixedPtr(0x8fae81);
  uint32 level = state[0] | state[1] << 8 | state[2] << 16;
  uint16 population = state[20] | state[21] << 8;
  fprintf(f, "item,x,y,hitFrame,shotX,shotY\n");
  for (int item = 0; item <= 2; item++) for (int x = 128; x <= 152; x++) for (int y = 304; y <= 384; y++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0;
    room_width_in_blocks = 32; room_height_in_blocks = 48;
    room_width_in_scrolls = 2; room_height_in_scrolls = 3;
    room_size_in_blocks = 3072; area_index = 2;
    DecompressToMem(level, g_ram + 0x10000);
    memcpy(BTS, (uint8 *)level_data + 3072, 1536);
    samus_pose = 0x6a; samus_pose_x_dir = 4;
    samus_x_pos = x; samus_y_pos = y;
    hud_item_index = item; samus_missiles = samus_super_missiles = 10;
    button_config_shoot_x = 0x40;
    layer1_y_pos = 224; plm_flag = 0x8000;
    for (uint16 entry = population; ; entry += 6) {
      uint32 entryAddress = 0x8f0000 | entry;
      if (!GET_WORD(RomFixedPtr(entryAddress))) break;
      ProbeRunBoundedRegisters(0x84846a, 0, entry, 0);
    }
    int gate = -1;
    for (int i = 0; i < 40; i++) if (plm_header_ptr[i] == 0xc82a) gate = i;
    if (gate < 0) { fclose(f); return 8; }
    ProbeRunBounded(0x8485b4); ProbeRunBounded(0x8485b4);
    for (int frame = 0; frame < 24; frame++) {
      joypad1_lastkeys = joypad1_newkeys = frame ? 0 : 0x40;
      ProbeRunBounded(0x90ac1c);
      ProbeRunBounded(item ? 0x90be62 : 0x90b80d);
      if (item == 0 && x == 140 && y == 360 && frame < 2)
        fprintf(stderr, "Native fired frame=%d xy=%d,%d type=%04X list=%04X radii=%d,%d\n", frame,
          projectile_x_pos[0], projectile_y_pos[0], projectile_type[0], projectile_bomb_instruction_ptr[0], projectile_x_radius[0], projectile_y_radius[0]);
      ProbeRunBounded(0x90aece);
      if (item == 0 && x == 140 && y == 360 && frame < 2)
        fprintf(stderr, "Native moved frame=%d xy=%d,%d type=%04X list=%04X radii=%d,%d\n", frame,
          projectile_x_pos[0], projectile_y_pos[0], projectile_type[0], projectile_bomb_instruction_ptr[0], projectile_x_radius[0], projectile_y_radius[0]);
      if (plm_timers[gate]) {
        fprintf(f, "%d,%d,%d,%d,%d,%d\n", item, x, y, frame, projectile_x_pos[0], projectile_y_pos[0]);
        break;
      }
      ProbeRunBounded(0x8485b4); vram_write_queue_tail = 0;
    }
  }
  fclose(f); return 0;
}
