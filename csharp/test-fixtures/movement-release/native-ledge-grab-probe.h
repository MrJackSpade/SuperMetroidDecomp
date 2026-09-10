#include "native-bounded-cpu.h"

// #459: early landing and ascending edge grabs through controller-driven expansion.
int DiagnosticLedgeGrab(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "left,family,scenario,speed,gap,delay,frame,input,x,y,pose,movement,anim,timer,base,extra,accel,facing,yspeed,ydir,chargecounter,xradius,yradius\n");
  for (int left = 0; left < 2; left++)
  for (int family = 0; family < 2; family++)
  for (int scenario = 0; scenario < 2; scenario++)
  for (int speed = 0; speed < 3; speed++)
  for (int gap = 0; gap < 9; gap++)
  for (int delay = -1; delay <= 6; delay++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_width_in_blocks = 144; room_height_in_blocks = 80;
    room_width_in_scrolls = 9; room_height_in_scrolls = 5; room_size_in_blocks = 144 * 80 * 2;
    interactive_enemy_indexes[0] = 0xffff;
    for (int x = 0; x < 144; x++) level_data[48 * 144 + x] = 0x8000;
    level_data[32 * 144 + (left ? 63 : 64)] = 0x8000;
    fx_y_pos = lava_acid_y_pos = 0xffff;
    fx_liquid_options = 0x80;
    equipped_items = 4;
    enable_horiz_slope_coll = 3;
    samus_health = samus_max_health = 99;
    samus_x_pos = samus_prev_x_pos = left ? (scenario ? 1029 : 1028) : (scenario ? 1019 : 1020);
    samus_pose = samus_prev_pose = family ? (left ? 0x1a : 0x19) : (scenario ? (left ? 0x18 : 0x17) : (left ? 0x2e : 0x2d));
    samus_movement_type = samus_prev_movement_type2 = family ? 3 : (scenario ? 2 : 6);
    ProbeRunBounded(0x90ec22);
    samus_y_pos = samus_prev_y_pos = 512 - samus_y_radius + (scenario ? gap - 4 : -gap);
    samus_y_speed = speed == 2 ? 3 : speed; samus_y_dir = scenario ? 1 : 2;
    samus_x_base_speed = scenario ? 2 : 0; samus_x_accel_mode = scenario ? 2 : 0;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337; grapple_beam_function = 0xc4f0;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
    button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
    button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
    // Preserve the held input which established the seeded airborne posture.
    // A fresh Down press would request Morph Ball rather than retain down-aim.
    uint16 previous = family ? 0x80 : (scenario ? 0x480 : 0x400);
    for (int frame = 0; frame < 128; frame++) {
      nmi_frame_counter_word = nmi_frame_counter_byte = frame + 2;
      uint16 input = family ? 0x80 : (scenario ? 0x480 : 0x400);
      if (delay >= 0 && frame >= delay)
        input = family ? 0x90 : (scenario ? 0x80 : 0) | (frame < delay + 4 ? (left ? 0x200 : 0x100) : 0);
      if (frame >= 24) input = 0;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      ProbeRunBounded(0x90e695);
      // Radius latches are refreshed by alpha. Pose-change collision later restores
      // the old radius, so compare the active movement hitbox, not that scratch latch.
      uint16 movement_x_radius = samus_x_radius, movement_y_radius = samus_y_radius;
      ProbeRunBounded(0xa09785); samus_contact_damage_index = 0;
      ProbeRunBounded(0x900000 | samus_movement_handler);
      uint32 stages[] = {0x908000,0x90dde9,0x91e8b6,0x91eb88,0x90eab3,0x90e9ce,0xa09169};
      for (int stage=0;stage<7;stage++) {
        ProbeRunBounded(stages[stage]);
      }
      fprintf(f, "%d,%d,%d,%d,%d,%d,%d,%04X,%04X%04X,%04X%04X,%02X,%02X,%04X,%04X,%04X%04X,%04X%04X,%04X,%02X,%04X%04X,%04X,%04X,%04X,%04X\n",
        left,family,scenario,speed,gap,delay,frame,input,samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
        samus_pose,samus_movement_type,samus_anim_frame,samus_anim_frame_timer,samus_x_base_speed,samus_x_base_subspeed,
        samus_x_extra_run_speed,samus_x_extra_run_subspeed,samus_x_accel_mode,samus_pose_x_dir,
        samus_y_speed,samus_y_subspeed,samus_y_dir,flare_counter,movement_x_radius,movement_y_radius);
    }
  }
  fclose(f); return 0;
}
