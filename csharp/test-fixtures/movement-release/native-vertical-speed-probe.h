#include "native-bounded-cpu.h"

// #423: focused original-CPU oracle for launch-table selection, liquid surface
// crossings, fixed-point gravity, and the cartridge's equality-only fall cap.
enum VerticalSpeedProbeAddress {
  kVerticalSpeedMakeJump = 0x9098bc,
  kVerticalSpeedMakeWallJump = 0x909949,
  kVerticalSpeedMakeBombJump = 0x909a2c,
  kVerticalSpeedCheckFalling = 0x9090c4,
  kVerticalSpeedDetermineAcceleration = 0x909c5b,
  kVerticalSpeedMove = 0x9090e2,
};

static void VerticalSpeedProbeReset(int medium, int high, int bonus, int crossing) {
  cpu_reset(g_snes->cpu);
  memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false;
  g_snes->cpu->sp = 0x1ff0;
  g_snes->cpu->dp = 0;
  room_width_in_blocks = 16;
  room_height_in_blocks = 255;
  room_width_in_scrolls = 1;
  room_height_in_scrolls = 16;
  room_size_in_blocks = 16 * 255 * 2;
  interactive_enemy_indexes[0] = 0xffff;
  samus_x_pos = samus_prev_x_pos = 128;
  samus_y_pos = samus_prev_y_pos = 2048;
  samus_pose = samus_prev_pose = 1;
  samus_x_radius = 5;
  samus_y_radius = 21;
  samus_health = samus_max_health = 99;
  enable_horiz_slope_coll = 3;
  fx_liquid_options = 0;
  fx_type = medium == 1 ? 6 : medium == 2 ? 2 : 0;
  fx_y_pos = medium == 1 ? (crossing ? 2020 : 8) : 0xffff;
  lava_acid_y_pos = medium == 2 ? (crossing ? 2020 : 8) : 0xffff;
  liquid_physics_type = medium;
  equipped_items = (high ? 0x0100 : 0) | (bonus ? 0x2000 : 0);
  samus_x_extra_run_speed = bonus == 1 ? 3 : bonus == 2 ? 5 : 0;
  samus_x_extra_run_subspeed = bonus == 1 ? 0x9000 : bonus == 2 ? 0xf000 : 0;
}

static void VerticalSpeedProbeWrite(
    FILE *f, int kind, int family, int medium, int high, int bonus,
    int crossing, int frame) {
  fprintf(f, "%d,%d,%d,%d,%d,%d,%d,%04X%04X,%04X%04X,%04X,%04X%04X\n",
      kind, family, medium, high, bonus, crossing, frame,
      samus_y_pos, samus_y_subpos, samus_y_speed, samus_y_subspeed,
      samus_y_dir, samus_y_accel, samus_y_subaccel);
}

int DiagnosticVerticalSpeed(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  FILE *f = fopen(output, "wx");
  if (!f) return 4;
  fprintf(f, "kind,family,medium,high,bonus,crossing,frame,y,yspeed,ydir,yaccel\n");

  // Normal, wall and bomb launch routines share the same downstream vertical
  // mover but select distinct launch tables. High Jump and Speed Booster are
  // intentionally retained in bomb cases to prove that native ignores them.
  for (int family = 0; family < 3; family++)
  for (int medium = 0; medium < 3; medium++)
  for (int high = 0; high < 2; high++)
  for (int bonus = 0; bonus < 3; bonus++) {
    VerticalSpeedProbeReset(medium, high, bonus, 0);
    ProbeRunBounded(family == 0 ? kVerticalSpeedMakeJump :
                    family == 1 ? kVerticalSpeedMakeWallJump :
                                  kVerticalSpeedMakeBombJump);
    ProbeRunBounded(kVerticalSpeedDetermineAcceleration);
    VerticalSpeedProbeWrite(f, 0, family, medium, high, bonus, 0, 0);
    for (int frame = 1; frame <= 320; frame++) {
      ProbeRunBounded(kVerticalSpeedCheckFalling);
      ProbeRunBounded(kVerticalSpeedDetermineAcceleration);
      ProbeRunBounded(kVerticalSpeedMove);
      VerticalSpeedProbeWrite(f, 0, family, medium, high, bonus, 0, frame);
    }
  }

  // Cross the water/lava surface during ascent. The launch uses liquid words,
  // then the real per-frame acceleration routine changes to air at the boundary.
  for (int medium = 1; medium < 3; medium++)
  for (int high = 0; high < 2; high++)
  for (int bonus = 0; bonus < 2; bonus++) {
    VerticalSpeedProbeReset(medium, high, bonus, 1);
    ProbeRunBounded(kVerticalSpeedMakeJump);
    ProbeRunBounded(kVerticalSpeedDetermineAcceleration);
    VerticalSpeedProbeWrite(f, 0, 0, medium, high, bonus, 1, 0);
    for (int frame = 1; frame <= 320; frame++) {
      ProbeRunBounded(kVerticalSpeedCheckFalling);
      ProbeRunBounded(kVerticalSpeedDetermineAcceleration);
      ProbeRunBounded(kVerticalSpeedMove);
      VerticalSpeedProbeWrite(f, 0, 0, medium, high, bonus, 1, frame);
    }
  }

  // Values on both sides of whole-speed five demonstrate that $90:9112 is an
  // equality test, not a >= clamp. Each case keeps the old-speed displacement.
  static const uint32 cap_speeds[] = {0x0004ffff, 0x00050000, 0x00050001, 0x00060000};
  for (int medium = 0; medium < 3; medium++)
  for (int cap = 0; cap < 4; cap++) {
    VerticalSpeedProbeReset(medium, 0, 0, 0);
    samus_y_speed = cap_speeds[cap] >> 16;
    samus_y_subspeed = cap_speeds[cap];
    samus_y_dir = 2;
    ProbeRunBounded(kVerticalSpeedDetermineAcceleration);
    VerticalSpeedProbeWrite(f, 1, 3, medium, cap, 0, 0, 0);
    for (int frame = 1; frame <= 8; frame++) {
      ProbeRunBounded(kVerticalSpeedDetermineAcceleration);
      ProbeRunBounded(kVerticalSpeedMove);
      VerticalSpeedProbeWrite(f, 1, 3, medium, cap, 0, 0, frame);
    }
  }

  fclose(f);
  return 0;
}
