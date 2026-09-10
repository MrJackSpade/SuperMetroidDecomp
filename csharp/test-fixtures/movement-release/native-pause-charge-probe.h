// #471: full native game-state dispatch through a pause-separated soft morph.
// Include after native-release-probe.h; dispatch before SDL initialization.
#include "native-bounded-cpu.h"

int DiagnosticPauseCharge(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "pauseAt,left,booster,frame,input,before,after,brightness,x,y,pose,movement,anim,timer,base,extra,accel,yspeed,ydir,charge,spread,bombs,bounce,momentum,boostCounter");
  for (int b = 0; b < 5; b++) fprintf(f, ",type%d,list%d,x%d,y%d,vx%d,vy%d,fuse%d", b,b,b,b,b,b,b);
  fprintf(f, "\n");
  fflush(f);
  for (int left = 0; left < 2; left++) for (int booster = 0; booster < 2; booster++)
  for (int pauseAt = booster ? 115 : 105; pauseAt <= (booster ? 145 : 135); pauseAt++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    // Retail metadata supplies valid palette/tile decompression and map pointers
    // to the real pause teardown. Only geometry and actors are synthetic.
    room_ptr = 0x91f8;
    ProbeRunBounded(0x82de6f); ProbeRunBounded(0x82def2);
    room_main_code_ptr = 0;
    for (int y = 0; y < room_height_in_blocks; y++)
      for (int x = 0; x < room_width_in_blocks; x++)
        level_data[y * room_width_in_blocks + x] = y == 16 ? 0x8000 : 0;
    interactive_enemy_indexes[0] = 0xffff;
    enemy_gfx_drawn_hook.bank = 0xa0;
    enemy_gfx_drawn_hook.addr = FUNC16(nullsub_170);
    fx_y_pos = lava_acid_y_pos = 0xffff;
    equipped_items = collected_items = 0x1004 | (booster ? 0x2000 : 0); equipped_beams = collected_beams = 0x1000;
    game_state = 8; reg_INIDISP = 15;
    samus_health = samus_max_health = 99;
    samus_x_pos = samus_prev_x_pos = 1000; samus_y_pos = samus_prev_y_pos = 235;
    samus_pose = samus_prev_pose = left ? 2 : 1; samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337; grapple_beam_function = 0xc4f0;
    frame_handler_alfa = 0xe695; frame_handler_beta = 0xe725;
    frame_handler_gamma = 0xe90e; samus_draw_handler = 0xeb52;
    *(uint16 *)&pause_hook = *(uint16 *)&unpause_hook = 0x83e1;
    *((uint8 *)&pause_hook + 2) = *((uint8 *)&unpause_hook + 2) = 0x88;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
    button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
    button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
    uint16 previous = 0, forward = left ? 0x200 : 0x100; int menuFrames = 0, resumedFrames = -1;
    for (int frame = 0; frame < 400; frame++) {
      nmi_frame_counter_word = frame + 2; nmi_frame_counter_byte = frame + 2;
      uint16 input = 0x40;
      if (frame >= 30 && frame < pauseAt) input |= 0x8000 | forward;
      if (frame >= 70) input |= 0x80;
      if (frame == pauseAt) input |= 0x1400;
      if (game_state == 12) input |= 0x400;
      if (game_state == 15 && menuFrames++ >= 1) input |= 0x1000;
      if (game_state == 18 && resumedFrames < 0) resumedFrames = 0;
      if (resumedFrames >= 0) {
        if (resumedFrames > 0) input |= forward;
        if (resumedFrames <= 50) input |= 0x400;
        resumedFrames++;
      }
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      oam_next_ptr = vram_write_queue_tail = 0;
      uint16 before = game_state;
      const uint32 entries[] = { 0x828b44, 0, 0, 0, 0x828ccf, 0x828cef, 0x8290c8,
        0x8290e8, 0x829324, 0x829367, 0x8293a1 };
      if (before < 8 || before > 18 || !entries[before - 8]) {
        fprintf(stderr, "Unexpected state %04X\n", before); fclose(f); return 7;
      }
      ProbeRunBounded(entries[before - 8]);
      fprintf(f, "%d,%d,%d,%d,%04X,%04X,%04X,%02X,%04X%04X,%04X%04X,%02X,%02X,%04X,%04X,%04X%04X,%04X%04X,%04X,%04X%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X",
        pauseAt,left,booster,frame,input,before,game_state,reg_INIDISP,samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
        samus_pose,samus_movement_type,samus_anim_frame,samus_anim_frame_timer,samus_x_base_speed,samus_x_base_subspeed,
        samus_x_extra_run_speed,samus_x_extra_run_subspeed,samus_x_accel_mode,samus_y_speed,samus_y_subspeed,samus_y_dir,
        flare_counter,bomb_spread_charge_timeout_counter,bomb_counter,used_for_ball_bounce_on_landing,samus_has_momentum_flag,speed_boost_counter);
      for (int b = 5; b < 10; b++) fprintf(f, ",%04X,%04X,%04X,%04X,%04X,%04X,%04X",
        projectile_type[b], projectile_bomb_instruction_ptr[b], projectile_x_pos[b], projectile_y_pos[b],
        projectile_bomb_x_speed[b], projectile_bomb_y_speed[b], projectile_variables[b]);
      fprintf(f, "\n");
      fflush(f);
      if (resumedFrames == 65) break;
    }
  }
  fclose(f); return 0;
}
