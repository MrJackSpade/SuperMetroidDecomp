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

enum XrayTimingProbeAddress {
  kXrayTimingInput = 0x91fcaf,
  kXrayTimingMovement = 0x90e94f,
  kXrayTimingAnimate = 0x908000,
  kXrayTimingPhase0 = 0x888732,
  kXrayTimingPhase1 = 0x888754,
  kXrayTimingPhase2 = 0x8887ab,
  kXrayTimingPhase3 = 0x888934,
  kXrayTimingPhase4 = 0x8889ba,
  kXrayTimingPhase5 = 0x888a08,
};

static void XrayTimingProbeReset(int left, int water) {
  cpu_reset(g_snes->cpu);
  memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false;
  g_snes->cpu->sp = 0x1ff0;
  g_snes->cpu->dp = 0;
  room_width_in_blocks = 16;
  room_height_in_blocks = 64;
  room_size_in_blocks = 16 * 64 * 2;
  interactive_enemy_indexes[0] = 0xffff;
  samus_x_pos = samus_prev_x_pos = 0x0100;
  samus_y_pos = samus_prev_y_pos = 0x0200;
  samus_pose = samus_prev_pose = left ? 0xda : 0xd9;
  samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
  samus_movement_type = samus_prev_movement_type = samus_prev_movement_type2 = 5;
  samus_x_radius = 5;
  samus_y_radius = 16;
  samus_health = samus_max_health = 99;
  equipped_items = 0x8000;
  fx_type = water ? 6 : 0;
  fx_y_pos = water ? 8 : 0xffff;
  lava_acid_y_pos = 0xffff;
  liquid_physics_type = water;
  samus_input_handler = 0xfcaf;
  samus_movement_handler = 0xe94f;
  samus_anim_frame = 2;
  samus_anim_frame_timer = 15;
  button_config_run_b = 0x8000;
  button_config_left = 0x0200;
  button_config_right = 0x0100;
  time_is_frozen_flag = 1;
  demo_input_pre_instr = 2;
  demo_input = 10;
  xray_angle = left ? 192 : 64;
  hdma_object_index = 0;
}

static void XrayTimingProbeStepHdma(void) {
  static const uint32 phases[] = {
    kXrayTimingPhase0, kXrayTimingPhase1, kXrayTimingPhase2,
    kXrayTimingPhase3, kXrayTimingPhase4, kXrayTimingPhase5,
  };
  // The held phase-2 routine only rebuilds the 512-byte HDMA aperture. That
  // expensive visual output cannot affect the pose timing measured here, so
  // do not interpret it until Run is released and the phase can advance.
  if (demo_input_pre_instr == 2 && (button_config_run_b & joypad1_lastkeys))
    return;
  if (demo_input_pre_instr < 6)
    ProbeRunBounded(phases[demo_input_pre_instr]);
}

int DiagnosticXrayTiming(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  FILE *f = fopen(output, "wx");
  if (!f) return 4;
  fprintf(f, "left,water,offset,frame,input,x,y,pose,xradius,yradius,xdir,movement,anim,timer,phase,frozen\n");
  const int turn_frame = 16;
  for (int left = 0; left < 2; left++)
  for (int water = 0; water < 2; water++)
  for (int offset = -8; offset <= 12; offset++) {
    XrayTimingProbeReset(left, water);
    uint16 previous_input = 0x8000;
    for (int frame = 0; frame < 48; frame++) {
      int release_frame = turn_frame + offset;
      uint16 input = frame < release_frame ? 0x8000 : 0;
      if (frame == turn_frame)
        input |= left ? 0x0100 : 0x0200;
      joypad1_lastkeys = input;
      joypad1_newkeys = input & ~previous_input;
      previous_input = input;
      // RunOneFrameOfGameInner dispatches HDMA before main gameplay. State five
      // can therefore restore ordinary Samus handlers before alpha samples input.
      XrayTimingProbeStepHdma();
      ProbeRunBounded(kXrayClimbRefreshRadius);
      if (time_is_frozen_flag) {
        ProbeRunBounded(kXrayTimingInput);
        ProbeRunBounded(kXrayTimingMovement);
      }
      ProbeRunBounded(kXrayTimingAnimate);
      fprintf(f, "%d,%d,%d,%d,%04X,%04X%04X,%04X%04X,%02X,%04X,%04X,%02X,%02X,%04X,%04X,%04X,%04X\n",
          left, water, offset, frame, input, samus_x_pos, samus_x_subpos,
          samus_y_pos, samus_y_subpos, samus_pose, samus_x_radius, samus_y_radius,
          samus_pose_x_dir, samus_movement_type, samus_anim_frame,
          samus_anim_frame_timer, demo_input_pre_instr, time_is_frozen_flag);
    }
  }
  fclose(f);
  return 0;
}
