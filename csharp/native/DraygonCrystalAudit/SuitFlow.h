/* #434: suspended Flash through suit release, with a real timer-eight bomb overlap. */
enum { FlashSuitAlpha = 0x90e713 };
static void suit_flow_matrix(void) {
  printf("right,gravity,bomb,frame,pose,x,y,ysub,yspeed,ysubspeed,ydir,handler,shine,palette,counter,health,missiles,supers,pbs,suit\n");
  for (int right = 0; right < 2; right++)
  for (int gravity = 0; gravity < 2; gravity++)
  for (int bomb = 0; bomb < 3; bomb++) {
    memset(g_ram, 0, sizeof(g_ram));
    memset(dma_channel_registers, 0, sizeof(dma_channel_registers));
    room_width_in_blocks = 144; room_height_in_blocks = 80;
    room_width_in_scrolls = 9; room_height_in_scrolls = 5; room_size_in_blocks = 144 * 80 * 2;
    interactive_enemy_indexes[0] = 0xffff;
    for (int x = 0; x < 144; x++) level_data[32 * 144 + x] = level_data[16 * 144 + x] = 0x8000;
    samus_pose = samus_prev_pose = right ? 1 : 2;
    samus_pose_x_dir = samus_prev_pose_x_dir = right ? 8 : 4;
    samus_x_pos = samus_prev_x_pos = 256; samus_y_pos = samus_prev_y_pos = 491;
    samus_health = 49; samus_max_health = 99;
    samus_missiles = samus_super_missiles = samus_power_bombs = 10;
    samus_max_missiles = samus_max_super_missiles = samus_max_power_bombs = 10;
    fx_y_pos = lava_acid_y_pos = 0xffff;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337;
    samus_x_speed_table_pointer = 0x9f55; grapple_beam_function = 0xc4f0;
    button_config_shoot_x = 0x40; button_config_jump_a = 0x80;
    button_config_run_b = 0x8000; button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
    button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
    joypad1_lastkeys = 0x470; game_state = 8;
    run(RefreshRadius); run(FlashEntry);
    if (samus_movement_handler != FlashRaising) Die("Flow must start from actual Flash admission");
    equipped_items = collected_items = gravity ? 0x20 : 1;
    layer1_y_pos = 355;
    run(gravity ? FlashGravitySetup : FlashVariaSetup);
    bool active = true;
    for (int frame = 0; frame < 440; frame++) {
      bool released = active && substate == 6;
      if (active) run(gravity ? FlashGravityHdma : FlashVariaHdma);
      if (released) active = false;
      bomb_counter = 0; projectile_damage[5] = 0;
      if (released && bomb) {
        bomb_counter = 1; projectile_type[5] = 0x500; projectile_damage[5] = 30;
        projectile_x_pos[5] = samus_x_pos + (bomb == 2 ? 13 : 0);
        projectile_y_pos[5] = samus_y_pos;
        projectile_x_radius[5] = projectile_y_radius[5] = 8; projectile_variables[5] = 8;
      }
      uint16 input = frame < 400 ? 0 : frame == 400 ? 0x80 : 0x880;
      joypad1_newkeys = input & ~joypad1_lastkeys;
      joypad1_lastkeys = input;
      nmi_frame_counter_word = frame + 2;
      if (active) { run(FlashSuitAlpha); run(InteractionPhase); }
      else {
        run(InputPhase); run(InteractionPhase); samus_contact_damage_index = 0;
        run(0x900000 | samus_movement_handler);
        unsigned stages[] = {AnimationPhase,TransitionPhase,CollisionPosePhase,ApplyPosePhase,PoseHistoryPhase,HurtPhase,CollisionPhase,Palette};
        for (int i = 0; i < 8; i++) run(stages[i]);
      }
      if (frame == 399 && (samus_movement_handler != 0xa337 ||
          (bomb == 1 ? !samus_shine_timer || timer_for_shine_timer != 7 : samus_shine_timer || timer_for_shine_timer)))
        Die("Only the bomb hit may retain Flash after returning to normal movement");
      if (frame == 403 && bomb == 1 && (samus_movement_handler != 0xd0ab || samus_contact_damage_index != 2 || samus_health != 48))
        Die("Suit/Flash bomb retention must launch a damaging, energy-consuming spark");
      printf("%d,%d,%d,%d,%02X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%d\n",
        right,gravity,bomb,frame,samus_pose,samus_x_pos,samus_y_pos,samus_y_subpos,samus_y_speed,samus_y_subspeed,samus_y_dir,
        samus_movement_handler,samus_shine_timer,timer_for_shine_timer,substate,samus_health,samus_missiles,samus_super_missiles,samus_power_bombs,active);
    }
  }
}
