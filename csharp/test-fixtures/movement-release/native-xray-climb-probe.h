#include "native-bounded-cpu.h"

// #438: original-CPU oracle for the X-Ray stand-up primitive. Door, gate,
// frozen-enemy and slope entry remain ordinary collision concerns; this probe
// deliberately establishes whether teardown itself inspects depth or medium.
enum XrayClimbProbeAddress {
  kXrayClimbFinish = 0x91e2ad,
  kXrayClimbRefreshRadius = 0x90ec22,
};

static void XrayClimbProbeReset(int kind, int left, int medium, int depth) {
  cpu_reset(g_snes->cpu);
  memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false;
  g_snes->cpu->sp = 0x1ff0;
  g_snes->cpu->dp = 0;
  room_width_in_blocks = 16;
  room_height_in_blocks = 64;
  room_size_in_blocks = 16 * 64 * 2;
  interactive_enemy_indexes[0] = 0xffff;
  samus_x_pos = samus_prev_x_pos = left ? 0x0100 - depth : 0x0100 + depth;
  samus_y_pos = samus_prev_y_pos = 0x0200;
  samus_health = samus_max_health = 99;
  equipped_items = 0x8000;
  fx_type = medium == 1 ? 6 : medium == 2 ? 2 : 0;
  fx_y_pos = medium == 1 ? 8 : 0xffff;
  lava_acid_y_pos = medium == 2 ? 8 : 0xffff;
  liquid_physics_type = medium;

  // kind 0: crouched turn (the climb); 1: stable crouched X-Ray;
  // 2: standing turn; 3: stable standing X-Ray.
  if (kind == 0) {
    samus_pose = left ? 0x43 : 0x44;
    samus_y_radius = 16;
    samus_movement_type = 14;
  } else if (kind == 1) {
    samus_pose = left ? 0xda : 0xd9;
    samus_y_radius = 16;
    samus_movement_type = 5;
  } else if (kind == 2) {
    samus_pose = left ? 0x25 : 0x26;
    samus_y_radius = 21;
    samus_movement_type = 14;
  } else {
    samus_pose = left ? 0xd6 : 0xd5;
    samus_y_radius = 21;
    samus_movement_type = 0;
  }
  samus_x_radius = 5;
  samus_pose_x_dir = left ? 4 : 8;
  samus_prev_pose = samus_pose;
  samus_prev_pose_x_dir = samus_pose_x_dir;
  samus_prev_movement_type = samus_movement_type;
  samus_prev_movement_type2 = samus_movement_type;
}

int DiagnosticXrayClimb(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  FILE *f = fopen(output, "wx");
  if (!f) return 4;
  fprintf(f, "kind,left,medium,depth,x,y,pose,xradius,yradius,xdir,movement\n");
  for (int kind = 0; kind < 4; kind++)
  for (int left = 0; left < 2; left++)
  for (int medium = 0; medium < 3; medium++)
  for (int depth = 1; depth <= 16; depth++) {
    XrayClimbProbeReset(kind, left, medium, depth);
    ProbeRunBounded(kXrayClimbFinish);
    // The direct teardown computes displacement from the old radius, then the
    // ordinary frame boundary publishes the new pose radius separately.
    ProbeRunBounded(kXrayClimbRefreshRadius);
    fprintf(f, "%d,%d,%d,%d,%04X%04X,%04X%04X,%02X,%04X,%04X,%02X,%02X\n",
        kind, left, medium, depth, samus_x_pos, samus_x_subpos,
        samus_y_pos, samus_y_subpos, samus_pose, samus_x_radius,
        samus_y_radius, samus_pose_x_dir, samus_movement_type);
  }
  fclose(f);
  return 0;
}
