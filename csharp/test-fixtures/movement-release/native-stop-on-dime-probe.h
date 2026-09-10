#include "native-bounded-cpu.h"

// #453: release/aim stopping trajectories on dry and submerged flat terrain.
int DiagnosticStopOnDime(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "left,dash,medium,pattern,frame,input,x,y,pose,movement,anim,timer,base,extra,accel,facing,yspeed,ydir,chargecounter\n");
  for (int left = 0; left < 2; left++)
  for (int dash = 0; dash < 2; dash++)
  for (int medium = 0; medium < 3; medium++)
  for (int pattern = 0; pattern < 8; pattern++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_width_in_blocks = 144; room_height_in_blocks = 80;
    room_width_in_scrolls = 9; room_height_in_scrolls = 5; room_size_in_blocks = 144 * 80 * 2;
    interactive_enemy_indexes[0] = 0xffff;
    for (int x = 0; x < 144; x++) level_data[16 * 144 + x] = 0x8000;
    fx_y_pos = medium ? 8 : 0xffff; lava_acid_y_pos = 0xffff;
    fx_liquid_options = 0x80; fx_type = medium ? 6 : 0;
    equipped_items = medium == 2 ? 0x24 : 4; enable_horiz_slope_coll = 3;
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
    for (int frame = 0; frame < 160; frame++) {
      nmi_frame_counter_word = nmi_frame_counter_byte = frame + 2;
      uint16 input = dash && frame < 80 ? 0x8000 : 0;
      if (frame < 80 || pattern == 5) input |= left ? 0x200 : 0x100;
      if (frame >= 80 && pattern >= 1 && pattern <= 5)
        input |= pattern == 1 ? 0x800 : pattern == 2 || pattern == 5 ? 0x10 : pattern == 3 ? 0x20 : 0x30;
      if (pattern == 6 && frame >= 81 || pattern == 7 && frame >= 79) input |= 0x10;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      ProbeRunBounded(0x90e695); ProbeRunBounded(0xa09785); samus_contact_damage_index = 0;
      ProbeRunBounded(0x900000 | samus_movement_handler);
      uint32 stages[] = {0x908000,0x90dde9,0x91e8b6,0x91eb88,0x90eab3,0x90e9ce,0xa09169};
      for (int stage=0;stage<7;stage++) {
        ProbeRunBounded(stages[stage]);
      }
      fprintf(f, "%d,%d,%d,%d,%d,%04X,%04X%04X,%04X%04X,%02X,%02X,%04X,%04X,%04X%04X,%04X%04X,%04X,%02X,%04X%04X,%04X,%04X\n",
        left,dash,medium,pattern,frame,input,samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
        samus_pose,samus_movement_type,samus_anim_frame,samus_anim_frame_timer,samus_x_base_speed,samus_x_base_subspeed,
        samus_x_extra_run_speed,samus_x_extra_run_subspeed,samus_x_accel_mode,samus_pose_x_dir,
        samus_y_speed,samus_y_subspeed,samus_y_dir,flare_counter);
    }
  }
  fclose(f); return 0;
}
