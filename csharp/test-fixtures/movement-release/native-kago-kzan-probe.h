#include "native-bounded-cpu.h"

// #455: falling Kzan pair versus controller-driven turn/morph/unmorph.
int DiagnosticKagoKzan(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "left,pattern,delay,frame,input,x,y,pose,movement,anim,timer,base,extra,accel,facing,yspeed,ydir,chargecounter,health,invincibility,hurt,direction,enemyY,enemyFunction,bottomY\n");
  for (int left = 0; left < 2; left++)
  for (int pattern = 0; pattern < 3; pattern++)
  for (int delay = 0; delay < 12; delay++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_width_in_blocks = 144; room_height_in_blocks = 80;
    room_width_in_scrolls = 9; room_height_in_scrolls = 5; room_size_in_blocks = 144 * 80 * 2;
    interactive_enemy_indexes[0] = 0xffff;
    for (int x = 0; x < 144; x++) level_data[32 * 144 + x] = 0x8000;
    fx_y_pos = lava_acid_y_pos = 0xffff;
    equipped_items = 4; enable_horiz_slope_coll = 3;
    samus_health = samus_max_health = 999;
    samus_x_pos = samus_prev_x_pos = 1024; samus_y_pos = samus_prev_y_pos = pattern == 2 ? 505 : 491;
    samus_pose = samus_prev_pose = pattern == 2 ? (left ? 0x41 : 0x1d) : (left ? 2 : 1);
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_movement_type = samus_prev_movement_type2 = pattern == 2 ? 4 : 0;
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337; grapple_beam_function = 0xc4f0;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
    button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
    button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
    ProbeRunBounded(0x90ec22);
    layer1_x_pos=896; layer1_y_pos=384;
    first_free_enemy_index=128; enemy_index_to_shake=0xffff;
    for(int part=0;part<2;part++) {
      cur_enemy_index=part*64;
      EnemyData *enemy=gEnemyData(cur_enemy_index);
      uint16 definition=part ? 0xe03f : 0xdfff;
      EnemyDef *header=get_EnemyDef_A2(definition);
      enemy->enemy_ptr=definition; enemy->bank=header->bank;
      enemy->health=header->health; enemy->x_width=header->x_radius; enemy->y_height=header->y_radius;
      enemy->x_pos=1024; enemy->y_pos=448;
      enemy->properties=part ? 0x0900 : 0xa800;
      enemy->layer=header->layer;
      enemy->instruction_timer=1;
      Get_SpikeyPlatform(cur_enemy_index)->spm_parameter_1=0x40;
      Get_SpikeyPlatform(cur_enemy_index)->spm_parameter_2=0x8008;
      ProbeRunBounded((header->bank << 16) | header->ai_init);
      enemy->spritemap_pointer=part ? 0 : 0x804d;
    }
    uint16 previous = 0;
    for (int frame = 0; frame < 80; frame++) {
      nmi_frame_counter_word = nmi_frame_counter_byte = frame + 2;
      uint16 input = 0;
      if (pattern == 0 && frame >= 8 + delay && frame < 40) input = left ? 0x100 : 0x200;
      if (pattern == 1 && (frame == 8 + delay || frame == 12 + delay)) input = 0x400;
      if (pattern == 2 && frame == 8 + delay) input = 0x800;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      extra_samus_x_displacement=extra_samus_x_subdisplacement=0;
      extra_samus_y_displacement=extra_samus_y_subdisplacement=0;
      ProbeRunBounded(0x90e695);
      ProbeRunBounded(0xa08eb6); ProbeRunBounded(0xa08fd4);
      ProbeRunBounded(0xa09785); samus_contact_damage_index = 0;
      ProbeRunBounded(0x900000 | samus_movement_handler);
      uint32 stages[] = {0x908000,0x90dde9,0x91e8b6,0x91eb88,0x90eab3,0x90e9ce,0xa09169};
      for (int stage=0;stage<7;stage++) {
        ProbeRunBounded(stages[stage]);
      }
      fprintf(f, "%d,%d,%d,%d,%04X,%04X%04X,%04X%04X,%02X,%02X,%04X,%04X,%04X%04X,%04X%04X,%04X,%02X,%04X%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X%04X,%04X,%04X\n",
        left,pattern,delay,frame,input,samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
        samus_pose,samus_movement_type,samus_anim_frame,samus_anim_frame_timer,samus_x_base_speed,samus_x_base_subspeed,
        samus_x_extra_run_speed,samus_x_extra_run_subspeed,samus_x_accel_mode,samus_pose_x_dir,
        samus_y_speed,samus_y_subspeed,samus_y_dir,flare_counter,samus_health,samus_invincibility_timer,samus_knockback_timer,knockback_dir, gEnemyData(0)->y_pos,gEnemyData(0)->y_subpos,Get_SpikeyPlatform(0)->spm_var_A,gEnemyData(64)->y_pos);
    }
  }
  fclose(f); return 0;
}
