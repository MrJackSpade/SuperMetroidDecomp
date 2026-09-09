// #485: full native EnemyMain, normal bomb producer and Samus pose/movement phases.
int DiagnosticMetroidController(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  FILE *f = fopen(output, "wx");
  if (!f) return 4;
  fprintf(f, "left,travel,schedule,frame,input,x,y,pose,jump,enemyX,enemyY,state,escape,health,bombs\n");
  for (int left = 0; left < 2; left++)
  for (int travel = 0; travel < 5; travel++)
  for (int schedule = 0; schedule < 4; schedule++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_width_in_blocks = 96; room_height_in_blocks = 16;
    room_width_in_scrolls = 6; room_height_in_scrolls = 1;
    for (int i = 10 * 96; i < 16 * 96; i++) level_data[i] = 0x8000;
    fx_y_pos = lava_acid_y_pos = 0xffff; equipped_items = 0x1004;
    samus_health = samus_max_health = 999;
    samus_x_pos = samus_prev_x_pos = 128; samus_y_pos = samus_prev_y_pos = 153;
    samus_pose = samus_prev_pose = left ? 0x41 : 0x1d;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_movement_type = samus_prev_movement_type = samus_prev_movement_type2 = 4;
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
    Enemy_Metroid *enemy = Get_Metroid(0);
    EnemyDef *definition = get_EnemyDef_A2(0xdd7f);
    enemy->base.enemy_ptr = 0xdd7f; enemy->base.bank = 0xa3;
    enemy->base.x_pos = 128; enemy->base.y_pos = 145;
    enemy->base.x_width = definition->x_radius; enemy->base.y_height = definition->y_radius;
    enemy->base.health = definition->health; enemy->base.properties = 0x2000;
    enemy->base.instruction_timer = 1; enemy->base.spritemap_pointer = 0x8000;
    cur_enemy_index = 0; RunAsmCode(0xa3ea4f, 0, 0, 0, 0);
    first_free_enemy_index = 64; enemy_index_to_shake = 0xffff;
    uint16 previous = 0;
    for (int frame = 0; frame < 180; frame++) {
      const int gaps[] = { 0, 16, 24, 48 };
      int gap = gaps[schedule];
      uint16 input = frame == 1 || gap && (frame == 1 + gap || frame == 1 + 2 * gap) ? 0x40 : 0;
      if (travel && frame >= 46 && frame < 46 + travel * 4) input |= left ? 0x200 : 0x100;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      RunAsmCode(0x90ec22, 0, 0, 0, 0);
      active_enemy_indexes[0] = 0; active_enemy_indexes[1] = 0xffff;
      interactive_enemy_indexes[0] = 0xffff;
      RunAsmCode(0xa09785, 0, 0, 0, 0);
      RunAsmCode(0xa08fd4, 0, 0, 0, 0);
      samus_new_pose = samus_new_pose_interrupted = samus_new_pose_transitional = 0xffff;
      samus_momentum_routine_index = samus_special_transgfx_index = samus_hurt_switch_index = 0;
      RunAsmCode(0x90e90f, 0, 0, 0, 0); RunAsmCode(0x909c5b, 0, 0, 0, 0);
      RunAsmCode(0x90ac1c, 0, 0, 0, 0); RunAsmCode(0x90bf9d, 0, 0, 0, 0);
      RunAsmCode(0x90aece, 0, 0, 0, 0);
      samus_contact_damage_index = 0;
      RunAsmCode(0x900000 | samus_movement_handler, 0, 0, 0, 0);
      RunAsmCode(0x908000, 0, 0, 0, 0); RunAsmCode(0x90dde9, 0, 0, 0, 0);
      RunAsmCode(0x91e8b6, 0, 0, 0, 0); RunAsmCode(0x91eb88, 0, 0, 0, 0);
      RunAsmCode(0x90eab3, 0, 0, 0, 0); RunAsmCode(0x90e9ce, 0, 0, 0, 0);
      RunAsmCode(0xa09169, 0, 0, 0, 0);
      fprintf(f, "%d,%d,%d,%d,%04X,%04X%04X,%04X%04X,%02X,%04X,%04X%04X,%04X%04X,%04X,%04X,%04X,%04X\n",
        left, travel, schedule, frame, input, samus_x_pos, samus_x_subpos, samus_y_pos, samus_y_subpos,
        samus_pose,bomb_jump_dir,enemy->base.x_pos,enemy->base.x_subpos,enemy->base.y_pos,enemy->base.y_subpos,
        enemy->metroid_var_F,enemy->metroid_var_E,samus_health,bomb_counter);
    }
  }
  fclose(f); return 0;
}
