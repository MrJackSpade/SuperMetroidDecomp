// #471: native equipment input toggles Bombs around an earned charged soft morph.
// Menu category input executes on the CPU; full pause fade/NMI timing is not modeled.
static void ChargeProbeToggleBombs(void) {
  RunAsmCode(0x82ab47, 0, 0, 0, 0);
  const uint16 inputs[] = { 0x100, 0, 0x400, 0, 0x80, 0 };
  for (int i = 0; i < 6; i++) {
    joypad1_lastkeys = inputs[i]; joypad1_newkeys = inputs[i];
    oam_next_ptr = vram_write_queue_tail = 0;
    RunAsmCode(0x82ac4f, 0, 0, 0, 0);
  }
}
int DiagnosticChargeEquipment(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "left,delay,frame,input,x,y,pose,movement,anim,timer,base,extra,accel,yspeed,ydir,charge,spread,bombs,type,pointer0,x0,y0,vx0,vy0,bounce,equipped");
  for (int b = 0; b < 5; b++) fprintf(f, ",type%d,list%d,x%d,y%d,vx%d,vy%d,fuse%d", b,b,b,b,b,b,b);
  fprintf(f, "\n");
  const int delays[] = { 0, 63, 64, 127, 128, 191, 192, 193 };
  for (int left = 0; left < 2; left++) for (int d = 0; d < 8; d++) {
    int delay = delays[d];
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_width_in_blocks = 144; room_height_in_blocks = 80;
    room_width_in_scrolls = 9; room_height_in_scrolls = 5; room_size_in_blocks = 144 * 80 * 2;
    interactive_enemy_indexes[0] = 0xffff;
    for (int y = 0; y <= 16; y++) for (int x = 0; x < 144; x++)
      if (y == 16 || x == 1 || x == 142 || x == (left ? 61 : 66)) level_data[y * 144 + x] = 0x8000;
    fx_y_pos = lava_acid_y_pos = 0xffff;
    equipped_items = collected_items = 0x1004; equipped_beams = collected_beams = 0x1000;
    samus_health = samus_max_health = 99;
    samus_x_pos = samus_prev_x_pos = left ? 1047 : 1000; samus_y_pos = samus_prev_y_pos = 235;
    samus_pose = samus_prev_pose = left ? 2 : 1;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337; grapple_beam_function = 0xc4f0;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
    button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
    button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
    uint16 previous = 0, forward = left ? 0x200 : 0x100, back = left ? 0x100 : 0x200;
    for (int frame = 0; frame < 500; frame++) {
      if (frame == 150 || frame == 260) ChargeProbeToggleBombs();
      nmi_frame_counter_word = frame + 2; nmi_frame_counter_byte = frame + 2;
      uint16 input = frame < 80 ? 0x40 : 0;
      if (frame >= 60) input |= frame < 92 ? forward : back;
      if (frame >= 60 && frame < 73) input |= 0x8000;
      if (frame >= 70 && frame < 92 || frame >= 94) input |= 0x80;
      if (frame >= 195) input |= 0x40;
      if (frame >= 195 && frame < 200 || frame >= 260 && frame < 260 + delay) input |= 0x400;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      RunAsmCode(0x90e695, 0, 0, 0, 0); RunAsmCode(0xa09785, 0, 0, 0, 0);
      samus_contact_damage_index = 0;
      RunAsmCode(0x900000 | samus_movement_handler, 0, 0, 0, 0);
      RunAsmCode(0x908000, 0, 0, 0, 0); RunAsmCode(0x90dde9, 0, 0, 0, 0);
      RunAsmCode(0x91e8b6, 0, 0, 0, 0); RunAsmCode(0x91eb88, 0, 0, 0, 0);
      RunAsmCode(0x90eab3, 0, 0, 0, 0); RunAsmCode(0x90e9ce, 0, 0, 0, 0);
      RunAsmCode(0xa09169, 0, 0, 0, 0);
      fprintf(f, "%d,%d,%d,%04X,%04X%04X,%04X%04X,%02X,%02X,%04X,%04X,%04X%04X,%04X%04X,%04X,%04X%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X",
        left,delay,frame,input,samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
        samus_pose,samus_movement_type,samus_anim_frame,samus_anim_frame_timer,samus_x_base_speed,samus_x_base_subspeed,
        samus_x_extra_run_speed,samus_x_extra_run_subspeed,samus_x_accel_mode,samus_y_speed,samus_y_subspeed,samus_y_dir,
        flare_counter,bomb_spread_charge_timeout_counter,bomb_counter,projectile_type[5],projectile_bomb_instruction_ptr[5],
        projectile_x_pos[5],projectile_y_pos[5],projectile_bomb_x_speed[5],projectile_bomb_y_speed[5],used_for_ball_bounce_on_landing);
      fprintf(f, ",%04X", equipped_items);
      for (int b = 5; b < 10; b++) fprintf(f, ",%04X,%04X,%04X,%04X,%04X,%04X,%04X",
        projectile_type[b], projectile_bomb_instruction_ptr[b], projectile_x_pos[b], projectile_y_pos[b],
        projectile_bomb_x_speed[b], projectile_bomb_y_speed[b], projectile_variables[b]);
      fprintf(f, "\n");
    }
  }
  fclose(f); return 0;
}
