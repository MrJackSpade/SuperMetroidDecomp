#include "native-bounded-cpu.h"

// #564: acquire charge by running, store by crouching, then try adjacent jump
// timings. No stored-shine or launch pose is injected.
static int DiagnosticDiagonalInputMode(const char *rom, const char *output, bool complete) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "left,settle,frame,input,pose,boost,shine,windup,x,y\n");
  for (int left = 0; left < 2; left++) for (int settle = 0; settle <= 15; settle++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_width_in_blocks = 96; room_height_in_blocks = 32;
    room_width_in_scrolls = 6; room_height_in_scrolls = 2; room_size_in_blocks = 6144;
    interactive_enemy_indexes[0] = 0xffff;
    for (int x = 0; x < 96; x++) level_data[16 * 96 + x] = 0x8000;
    if (complete) for (int x = 0; x < 96; x++) level_data[x] = 0x8000;
    fx_y_pos = lava_acid_y_pos = 0xffff;
    equipped_items = 0x2000; enable_horiz_slope_coll = 3;
    samus_health = samus_max_health = 999;
    samus_x_pos = samus_prev_x_pos = left ? 1200 : 128;
    samus_y_pos = samus_prev_y_pos = 235;
    samus_pose = samus_prev_pose = left ? 2 : 1;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_x_radius = 5; samus_y_radius = 21;
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337; grapple_beam_function = 0xc4f0;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
    button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
    button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
    uint16 previous = 0;
    bool launched = false;
    for (int frame = 0; frame < (complete ? 260 : 150); frame++) {
      nmi_frame_counter_word = nmi_frame_counter_byte = frame + 2;
      uint16 input = frame < 90 ? 0x8000 | (left ? 0x200 : 0x100) :
        frame == 90 ? 0x400 : frame <= 90 + settle ? 0 : 0x90;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      ProbeRunBounded(0x90e695);
      ProbeRunBounded(0xa09785); samus_contact_damage_index = 0;
      ProbeRunBounded(0x900000 | samus_movement_handler);
      uint32 stages[] = { 0x908000,0x90dde9,0x91e8b6,0x91eb88,0x90eab3,0x90e9ce,0xa09169,0x91d6f7 };
      for (int stage = 0; stage < 8; stage++) ProbeRunBounded(stages[stage]);
      fprintf(f, "%d,%d,%d,%04X,%04X,%04X,%04X,%04X,%04X%04X,%04X%04X\n",
        left, settle, frame, input, samus_pose, speed_boost_counter, samus_shine_timer,
        timer_for_shinesparks_startstop, samus_x_pos, samus_x_subpos, samus_y_pos, samus_y_subpos);
      if (samus_pose >= 0xc9 && samus_pose <= 0xce) {
        launched = true;
        if (!complete) break;
      }
      if (complete && launched && samus_movement_handler == 0xa337 && samus_pose < 0xc9) break;
    }
  }
  fclose(f); return 0;
}

int DiagnosticDiagonalInput(const char *rom, const char *output) {
  return DiagnosticDiagonalInputMode(rom, output, false);
}
int DiagnosticDiagonalInputComplete(const char *rom, const char *output) {
  return DiagnosticDiagonalInputMode(rom, output, true);
}
