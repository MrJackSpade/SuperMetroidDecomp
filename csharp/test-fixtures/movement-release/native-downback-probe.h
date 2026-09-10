#include "native-bounded-cpu.h"

// #461: down/back input following fall, jump, wall jump, unmorph and ordinary damage.
int DiagnosticDownback(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "left,entry,geometry,pattern,delay,frame,input,x,y,pose,movement,anim,timer,base,extra,accel,facing,yspeed,ydir,chargecounter,health,invincibility,hurt,direction,xradius,yradius\n");
  for (int left = 0; left < 2; left++)
  for (int entry = 0; entry < 5; entry++)
  for (int geometry = 0; geometry < 2; geometry++)
  for (int pattern = 0; pattern < 4; pattern++)
  for (int delay = 0; delay < 17; delay++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_width_in_blocks = 144; room_height_in_blocks = 80;
    room_width_in_scrolls = 9; room_height_in_scrolls = 5; room_size_in_blocks = 144 * 80 * 2;
    interactive_enemy_indexes[0] = 0xffff;
    for (int x = 0; x < 144; x++) level_data[48 * 144 + x] = 0x8000;
    if (geometry) {
      level_data[29 * 144 + (left ? 61 : 66)] = 0x8000;
      level_data[32 * 144 + (left ? 61 : 66)] = 0x8000;
    }
    fx_y_pos = lava_acid_y_pos = 0xffff;
    fx_liquid_options = 0x80;
    equipped_items = 0x2004;
    enable_horiz_slope_coll = 3;
    samus_health = samus_max_health = 99;
    samus_x_pos = samus_prev_x_pos = 1024;
    samus_pose = samus_prev_pose = entry == 0 ? (left ? 0x2a : 0x29) :
      entry == 2 ? (left ? 0x84 : 0x83) : entry == 3 ? (left ? 0x32 : 0x31) : (left ? 0x52 : 0x51);
    samus_movement_type = samus_prev_movement_type2 = entry == 0 ? 6 : entry == 2 ? 20 : entry == 3 ? 8 : 2;
    ProbeRunBounded(0x90ec22);
    samus_y_pos = samus_prev_y_pos = entry == 3 ? 484 : 472;
    samus_y_dir = 2;
    samus_x_base_speed = 1; samus_x_base_subspeed = 0x4000;
    samus_x_extra_run_speed = 4; samus_x_accel_mode = 2;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337; grapple_beam_function = 0xc4f0;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
    button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
    button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
    uint16 previous = left ? 0x200 : 0x100;
    for (int frame = 0; frame < 112; frame++) {
      nmi_frame_counter_word = nmi_frame_counter_byte = frame + 2;
      uint16 forward = left ? 0x200 : 0x100, back = left ? 0x100 : 0x200;
      uint16 input = forward;
      int down_frame = 8 + delay;
      if (pattern && frame >= down_frame) {
        input = 0x400;
        if (frame > down_frame && pattern == 2) input |= back;
        if (frame > down_frame && pattern == 3) input |= forward;
      }
      if (entry == 3 && frame == 0) input = 0x800;
      if (frame >= 40) input = 0;
      if (entry == 4 && frame == 0) {
        EnemyData *enemy = gEnemyData(0);
        enemy->enemy_ptr = 0xdcff; enemy->x_pos = samus_x_pos + (left ? 8 : -8);
        enemy->y_pos = samus_y_pos; cur_enemy_index = 0;
        ProbeRunBounded(0xa08023); enemy->enemy_ptr = 0;
      }
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
      fprintf(f, "%d,%d,%d,%d,%d,%d,%04X,%04X%04X,%04X%04X,%02X,%02X,%04X,%04X,%04X%04X,%04X%04X,%04X,%02X,%04X%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X\n",
        left,entry,geometry,pattern,delay,frame,input,samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
        samus_pose,samus_movement_type,samus_anim_frame,samus_anim_frame_timer,samus_x_base_speed,samus_x_base_subspeed,
        samus_x_extra_run_speed,samus_x_extra_run_subspeed,samus_x_accel_mode,samus_pose_x_dir,
        samus_y_speed,samus_y_subspeed,samus_y_dir,flare_counter,samus_health,samus_invincibility_timer,samus_knockback_timer,knockback_dir,movement_x_radius,movement_y_radius);
    }
  }
  fclose(f); return 0;
}
