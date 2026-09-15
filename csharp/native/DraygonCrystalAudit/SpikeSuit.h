/* #435: original-CPU morphed spike damage, unmorph and Shinespark timing sweep. */
static void spike_suit_matrix(void) {
  printf("unmorph,launch,frame,input,pose,x,y,ysub,yspeed,ysubspeed,ydir,handler,knockback,knockdir,hurt,shine,palette,special,invincible,health\n");
  for (int unmorph = 0; unmorph <= 2; unmorph++)
  for (int launch = 6; launch <= 12; launch++) {
    memset(g_ram, 0, sizeof(g_ram));
    memset(dma_channel_registers, 0, sizeof(dma_channel_registers));
    room_width_in_blocks = 32; room_height_in_blocks = 32;
    room_width_in_scrolls = 2; room_height_in_scrolls = 2;
    room_size_in_blocks = 32 * 32 * 2;
    interactive_enemy_indexes[0] = 0xffff;
    fx_y_pos = lava_acid_y_pos = 0xffff;
    samus_pose = samus_prev_pose = 0x1d;
    samus_pose_x_dir = samus_prev_pose_x_dir = 8;
    samus_movement_type = samus_prev_movement_type = 4;
    samus_x_pos = samus_prev_x_pos = 200;
    samus_y_pos = samus_prev_y_pos = 166;
    samus_health = samus_max_health = 199;
    equipped_items = collected_items = 0x2004;
    samus_input_handler = 0xe913;
    samus_movement_handler = 0xa337;
    samus_x_speed_table_pointer = 0x9f55;
    grapple_beam_function = 0xc4f0;
    button_config_shoot_x = 0x40; button_config_jump_a = 0x80;
    button_config_run_b = 0x8000; button_config_aim_up_R = 0x10;
    button_config_aim_down_L = 0x20; button_config_itemcancel_y = 0x4000;
    button_config_itemswitch = 0x2000; game_state = 8;
    samus_shine_timer = 180; timer_for_shine_timer = 1;
    run(RefreshRadius);
    int block = (samus_y_pos >> 4) * room_width_in_blocks + (samus_x_pos >> 4);
    level_data[block] = 0x2000; BTS[block] = 2;
    uint16 previous = 0;
    for (int frame = 0; frame < 15; frame++) {
      uint16 input = frame == unmorph || frame == launch ? 0x80 : 0;
      joypad1_lastkeys = input;
      joypad1_newkeys = input & ~previous;
      previous = input;
      nmi_frame_counter_word = frame;
      run(InsideBlockPhase);
      run(InputPhase);
      run(0x900000 | samus_movement_handler);
      run(AnimationPhase);
      unsigned stages[] = { TransitionPhase, CollisionPosePhase, ApplyPosePhase,
                            PoseHistoryPhase, HurtPhase, CollisionPhase, Palette };
      for (int i = 0; i < 7; i++) run(stages[i]);
      printf("%d,%d,%d,%04X,%02X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X\n",
        unmorph,launch,frame,input,samus_pose,samus_x_pos,samus_y_pos,samus_y_subpos,
        samus_y_speed,samus_y_subspeed,samus_y_dir,samus_movement_handler,
        samus_knockback_timer,knockback_dir,samus_hurt_switch_index,samus_shine_timer,
        timer_for_shine_timer,samus_special_transgfx_index,samus_invincibility_timer,samus_health);
    }
  }
}

/* #435: automatic Reserve recovery freezes Samus's beta handler while the
   global hurt timers continue. Sixty reserve points let spike invincibility
   expire; the completion frame restores normal alpha early enough for the
   persistent spike field to publish a fresh ten-frame knockback window. */
static void spike_suit_reserve_matrix(void) {
  enum { ReserveRefill = 0x82dc31, ReserveLock = 0x90f411, ReserveUnlock = 0x90f2e0 };
  printf("freeze_after,launch_after,frame,input,pose,x,y,ysub,yspeed,ysubspeed,ydir,handler,knockback,knockdir,shine,palette,invincible,health,reserve,anim,animtimer\n");
  for (int freeze_after = 6; freeze_after <= 12; freeze_after++)
  for (int launch_after = 4; launch_after <= 8; launch_after++) {
    memset(g_ram, 0, sizeof(g_ram));
    memset(dma_channel_registers, 0, sizeof(dma_channel_registers));
    room_width_in_blocks = 32; room_height_in_blocks = 32;
    room_width_in_scrolls = 2; room_height_in_scrolls = 2;
    room_size_in_blocks = 32 * 32 * 2;
    interactive_enemy_indexes[0] = 0xffff;
    fx_type = 6; fx_y_pos = 0; lava_acid_y_pos = 0xffff; fx_liquid_options = 0x80;
    samus_pose = samus_prev_pose = 0x1d;
    samus_pose_x_dir = samus_prev_pose_x_dir = 8;
    samus_movement_type = samus_prev_movement_type = 4;
    samus_x_pos = samus_prev_x_pos = 200;
    samus_y_pos = samus_prev_y_pos = 166;
    samus_health = samus_max_health = 199;
    equipped_items = collected_items = 0x2004;
    samus_input_handler = 0xe913;
    samus_movement_handler = 0xa337;
    samus_x_speed_table_pointer = 0x9f55;
    grapple_beam_function = 0xc4f0;
    button_config_shoot_x = 0x40; button_config_jump_a = 0x80;
    button_config_run_b = 0x8000; button_config_aim_up_R = 0x10;
    button_config_aim_down_L = 0x20; button_config_itemcancel_y = 0x4000;
    button_config_itemswitch = 0x2000; game_state = 8;
    samus_shine_timer = 180; timer_for_shine_timer = 1;
    for (int block = 0; block < 32 * 32; block++) {
      level_data[block] = 0x2000; BTS[block] = 2;
    }
    run(RefreshRadius);
    uint16 previous = 0;
    for (int frame = 0; frame < freeze_after; frame++) {
      uint16 input = frame == 1 ? 0x80 : 0;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      nmi_frame_counter_word = frame;
      run(InsideBlockPhase); run(InputPhase); run(0x900000 | samus_movement_handler); run(AnimationPhase);
      unsigned stages[] = { TransitionPhase, CollisionPosePhase, ApplyPosePhase,
                            PoseHistoryPhase, HurtPhase, CollisionPhase, Palette };
      for (int i = 0; i < 7; i++) run(stages[i]);
    }

    samus_health = 0; samus_max_health = 99;
    samus_reserve_health = 60; samus_max_reserve_health = 100; reserve_health_mode = 1;
    time_is_frozen_flag = 0x8000; run(ReserveLock);
    previous = 0;
    for (int refill = 0; refill < 59; refill++) {
      nmi_frame_counter_word++;
      run(ReserveRefill);
      run(CollisionPhase);
    }
    nmi_frame_counter_word++;
    run(ReserveRefill);
    time_is_frozen_flag = 0; run(ReserveUnlock);

    for (int frame = 0; frame < 25; frame++) {
      uint16 input = frame == launch_after ? 0x80 : 0;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      nmi_frame_counter_word++;
      run(InsideBlockPhase); run(InputPhase); run(0x900000 | samus_movement_handler); run(AnimationPhase);
      unsigned stages[] = { TransitionPhase, CollisionPosePhase, ApplyPosePhase,
                            PoseHistoryPhase, HurtPhase, CollisionPhase, Palette };
      for (int i = 0; i < 7; i++) run(stages[i]);
      printf("%d,%d,%d,%04X,%02X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X\n",
        freeze_after,launch_after,frame,input,samus_pose,samus_x_pos,samus_y_pos,samus_y_subpos,
        samus_y_speed,samus_y_subspeed,samus_y_dir,samus_movement_handler,
        samus_knockback_timer,knockback_dir,samus_shine_timer,timer_for_shine_timer,
        samus_invincibility_timer,samus_health,samus_reserve_health,
        samus_anim_frame,samus_anim_frame_timer);
    }
  }
}
