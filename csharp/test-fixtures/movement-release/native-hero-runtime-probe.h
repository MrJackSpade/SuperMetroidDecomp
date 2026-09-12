#include "native-bounded-cpu.h"

// Room-local input/movement/scrolling/projectile comparison; no rendered assets.
int DiagnosticHeroRuntime(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "walking,frame,samusX,samusSubX,samusY,samusSubY,pose,cameraX,cameraY,shotX,shotSubX,shotY,shotSubY,vx,vy,type,instruction\n");
  for (int walking = 0; walking < 2; walking++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_width_in_blocks = 144; room_height_in_blocks = 80;
    room_width_in_scrolls = 9; room_height_in_scrolls = 5; room_size_in_blocks = 144*80*2;
    for (int y = 16; y < 36; y++) for (int x = 16; x < 64; x++)
      level_data[y*144+x] = y >= 32 || x == 54 ? 0x8000 : 0;
    memcpy(scrolls, RomFixedPtr(0x8f9283), 50);
    up_scroller = *RomFixedPtr(0x8f91fe);
    down_scroller = *RomFixedPtr(0x8f91ff);
    interactive_enemy_indexes[0] = 0xffff;
    samus_pose = samus_prev_pose = 1; samus_pose_x_dir = samus_prev_pose_x_dir = 8;
    samus_x_pos = samus_prev_x_pos = 512; samus_y_pos = samus_prev_y_pos = 490;
    samus_x_radius = 5; samus_y_radius = 21;
    samus_x_speed_table_pointer = 0x9f55; samus_input_handler = 0xe913;
    samus_anim_frame_timer = 5; samus_health = 99;
    button_config_shoot_x = 0x40; button_config_run_b = 0x8000; button_config_jump_a = 0x80;
    fx_y_pos = lava_acid_y_pos = 0xffff;
    layer1_x_pos = 400; layer1_y_pos = 350;
    uint16 previous_input = 0;
    for (int frame = -64; frame < 100; frame++) {
      uint16 input = frame == 0 ? 0x40 : frame > 0 && walking ? 0x100 : 0;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous_input; previous_input = input;
      samus_new_pose = samus_new_pose_interrupted = samus_new_pose_transitional = 0xffff;
      samus_momentum_routine_index = samus_special_transgfx_index = samus_hurt_switch_index = 0;
      ProbeRunBounded(0x90ec22);
      ProbeRunBounded(0x918000);
      ProbeRunBounded(0x909c5b);
      ProbeRunBounded(0x90b80d);
      ProbeRunBounded(0x90aece);
      ProbeRunBounded(0x90eb02);
      ProbeRunBounded(0x90a337);
      ProbeRunBounded(0x908000);
      ProbeRunBounded(0x91e8b6);
      ProbeRunBounded(0x91eb88);
      ProbeRunBounded(0x90eab3);
      ProbeRunBounded(0x9094ec);
      fprintf(f, "%d,%d,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X\n",
        walking, frame, samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,samus_pose,
        layer1_x_pos,layer1_y_pos,projectile_x_pos[0],projectile_bomb_x_subpos[0],projectile_y_pos[0],
        projectile_bomb_y_subpos[0],projectile_bomb_x_speed[0],projectile_bomb_y_speed[0],
        projectile_type[0],projectile_bomb_instruction_ptr[0]);
      if (frame >= 0 && (!projectile_bomb_instruction_ptr[0] || (projectile_type[0] & 0xf00))) break;
    }
  }
  fclose(f); return 0;
}
