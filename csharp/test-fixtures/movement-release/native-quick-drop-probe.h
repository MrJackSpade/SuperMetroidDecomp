#include "native-bounded-cpu.h"

// Focused handler boundary: an upward turn touching a removable ceiling.
// No animation/input dispatch is simulated here; the wider technique audit must
// separately cover entering and completing the turn through controller input.
int DiagnosticQuickDrop(const char *rom, const char *path) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  FILE *out = fopen(path, "wb");
  if (!out) return 3;
  fprintf(out, "left,remove,frame,x,y,speed,direction,collision\n");
  for (int left = 0; left < 2; left++)
  for (int remove = 0; remove < 2; remove++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0;
    room_width_in_blocks = 16; room_height_in_blocks = 32;
    interactive_enemy_indexes[0] = 0xffff;
    for (int x = 0; x < 16; x++) level_data[12 * 16 + x] = 0x8000;
    samus_pose = left ? 0x2f : 0x30;
    samus_pose_x_dir = left ? 4 : 8; samus_movement_type = 0x17;
    samus_x_pos = 128; samus_y_pos = 227;
    samus_x_radius = 5; samus_y_radius = 19;
    samus_y_speed = 2; samus_y_subaccel = 0x2800; samus_y_dir = 1;
    samus_x_speed_table_pointer = 0x9f55;
    fx_y_pos = lava_acid_y_pos = 0xffff;
    enable_horiz_slope_coll = 3;
    for (int frame = 0; frame < 3; frame++) {
      if (remove && frame == 1)
        for (int x = 0; x < 16; x++) level_data[12 * 16 + x] = 0;
      ProbeRunBounded(0x90a790);
      fprintf(out, "%d,%d,%d,%04X%04X,%04X%04X,%04X%04X,%04X,%04X\n",
        left, remove, frame, samus_x_pos, samus_x_subpos, samus_y_pos,
        samus_y_subpos, samus_y_speed, samus_y_subspeed, samus_y_dir, input_to_pose_calc);
    }
  }
  fclose(out); return 0;
}
