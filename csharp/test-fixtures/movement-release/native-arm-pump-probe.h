#include "native-bounded-cpu.h"

// #452: input-driven arm transitions, including airborne and terrain controls.
int DiagnosticArmPump(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "left,charge,terrain,pattern,frame,input,x,y,pose,movement,anim,timer,base,extra,accel,facing,yspeed,ydir,chargecounter\n");
  for (int left = 0; left < 2; left++)
  for (int charge = 0; charge < 2; charge++)
  for (int terrain = 0; terrain < 5; terrain++)
  for (int pattern = 0; pattern < 6; pattern++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_width_in_blocks = 144; room_height_in_blocks = 80;
    room_width_in_scrolls = 9; room_height_in_scrolls = 5; room_size_in_blocks = 144 * 80 * 2;
    interactive_enemy_indexes[0] = 0xffff;
    for (int x = 0; x < 144; x++) {
      bool slope = (terrain == 2 || terrain == 3) && (left ? x < 61 : x > 66);
      level_data[16 * 144 + x] = slope ? 0x1000 : 0x8000;
      BTS[16 * 144 + x] = slope ? (terrain == 2 ? 0x12 : 0x1b) | (left ? 0x40 : 0) : 0;
    }
    if (terrain == 4) for (int y = 0; y < 16; y++) level_data[y * 144 + (left ? 57 : 70)] = 0x8000;
    fx_y_pos = lava_acid_y_pos = 0xffff; fx_liquid_options = 0x80;
    equipped_items = 4; equipped_beams = 0x1000; enable_horiz_slope_coll = 3;
    samus_health = samus_max_health = 99;
    samus_x_pos = samus_prev_x_pos = 1024; samus_y_pos = samus_prev_y_pos = 235;
    samus_pose = samus_prev_pose = left ? 2 : 1;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337; grapple_beam_function = 0xc4f0;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
    button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
    button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
    uint16 previous = 0;
    for (int frame = 0; frame < 120; frame++) {
      nmi_frame_counter_word = nmi_frame_counter_byte = frame + 2;
      uint16 input = charge ? 0x40 : 0;
      if (frame >= 40) input |= left ? 0x200 : 0x100;
      if (terrain == 1 && frame >= 55) input |= 0x80;
      if (frame >= 60) {
        int phase = (frame - 60) % 8;
        if (pattern == 1 && phase < 4) input |= 0x10;
        if (pattern == 2 && phase < 4) input |= 0x20;
        if (pattern == 3) input |= phase < 4 ? 0x10 : 0x20;
        if (pattern == 4 && phase < 4) input |= 0x40;
        if (pattern == 5 && phase < 4) input |= 0x30;
      }
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      ProbeRunBounded(0x90e695); ProbeRunBounded(0xa09785); samus_contact_damage_index = 0;
      ProbeRunBounded(0x900000 | samus_movement_handler);
      uint32 stages[] = {0x908000,0x90dde9,0x91e8b6,0x91eb88,0x90eab3,0x90e9ce,0xa09169};
      for (int stage=0;stage<7;stage++) {
        ProbeRunBounded(stages[stage]);
      }
      fprintf(f, "%d,%d,%d,%d,%d,%04X,%04X%04X,%04X%04X,%02X,%02X,%04X,%04X,%04X%04X,%04X%04X,%04X,%02X,%04X%04X,%04X,%04X\n",
        left,charge,terrain,pattern,frame,input,samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
        samus_pose,samus_movement_type,samus_anim_frame,samus_anim_frame_timer,samus_x_base_speed,samus_x_base_subspeed,
        samus_x_extra_run_speed,samus_x_extra_run_subspeed,samus_x_accel_mode,samus_pose_x_dir,
        samus_y_speed,samus_y_subspeed,samus_y_dir,flare_counter);
    }
  }
  fclose(f); return 0;
}
