# What the project asks of the assets

Written by `tools/godot-rooms/asset_manifest.py`; do not edit it by hand. It lists every asset file the
project's scenes and scripts name. What lies inside each kind of file is in
[asset-formats.md](asset-formats.md).

## Data files given to things

Each is read by the type shown, which takes the listed values from it by name, besides whatever the
type's own remarks say it reads (timelines, colliders, shots, lists of its own).

| File | Read by | Values by name |
| --- | --- | --- |
| `bosses/bomb_torizo.json` | `Torizo` | awakening_timeline, close_behind, close_in_front, contact_damage, death_drops, eggs_health, face_health, fade_steps, first_attack_missiles, gut_health, health, landing_shake_strength, orb_speed_across, orb_speed_up, orbs_per_burst, repeated_jump_steps, space_jump_updates, stagger_health, start_look, start_timeline, start_tint, turn_delay |
| `bosses/bomb_torizo_hand.json` | `BombTorizoHand` | debris_look |
| `bosses/botwoon.json` | `Botwoon` | contact_damage, dud_sound, explosion_effect, explosion_sound, floor_y, health, opening_delay, pre_fall_wait, segment_damage, smoke_effect, speed_curve, wall_drops, wall_explosion_effect, wall_updates, wall_x |
| `bosses/ceres_ridley.json` | `Ridley` | arena_reveal_step_updates, arena_reveal_updates, baby_cry_sound, before_roar_updates, body_fade_step_updates, bounce_sound, contact_damage, death_drops, death_lunges, dust_effect, explosion_effect, explosion_sound, eye_fade_step_updates, eye_hold_updates, fade_steps, health, hit_flash_updates, hits_to_drive_off, piece_end_effect, reveal_delay, roar_hold_updates, roar_sound, samus_low_energy, tail_damage, tail_tip_arc_start, wounded_health |
| `bosses/ceres_ridley_fireball.json` | `RidleyFireball` | final_timeline, flame, flame_speed, flare_across, flare_down, is |
| `bosses/ceres_ridley_flame.json` | `RidleyFireball` | final_timeline, flame, flame_speed, flare_across, flare_down, is |
| `bosses/ceres_ridley_flare.json` | `RidleyFireball` | final_timeline, flame, flame_speed, flare_across, flare_down, is |
| `bosses/crocomire.json` | `Crocomire` | acid_sound, acid_sound_updates, big_dud_effect, break_through_patch, bridge_approach_x, bridge_collapse_x, bridge_sound, bridge_wall_patch, charged_beam_steps, claw_push, contact_damage, crumble_sound, death_drops, dud_effect, dust_effect, explosion_sound, fall_delay, health, melt_updates, missile_steps, mouth_shot_flash, mouth_shot_hold, mouth_shot_screen_reach, power_bomb_flash, power_bomb_steps, risen_y, rumble_sound, samus_in_view_at_break_out, samus_in_view_facing_left, samus_in_view_facing_right, shake_sound, skeleton_wall_patch, spike_wall_breaking_patch, spike_wall_charge_x, spiked_wall_updates, spit_sound, step_back_screen_reach, submerged_pause, sunken_y, super_missile_steps, tongue_across, wake_distance, wall_approach_x, wall_break_sound |
| `bosses/draygon.json` | `Draygon` | bubble_effect, chase_speed, contact_damage, dancer_effect, death_drops, dust_effect, escape_presses, eye_bubble_effect, foam_effect, goop_range, goops_per_pass, grapple_shock_damage, health, intro_dance_updates, intro_updates, right_side_distance, smoke_effect, swim_line_y, swoop_quickening, swoop_quickening_limit, swoop_quickening_per_hit, whip_shake_strength |
| `bosses/draygon_cannon.json` | `DraygonCannon` | contact_damage, grapple_drain, health |
| `bosses/draygon_cannon_shot.json` | `DraygonCannonShot` | chamber_size |
| `bosses/draygon_goop.json` | `DraygonGoop` | chamber_size, most_stuck, reach_across, reach_down, stack_spacing, stack_top, stuck_timeline, stuck_updates |
| `bosses/golden_torizo.json` | `Torizo` | awakening_timeline, close_behind, close_in_front, contact_damage, death_drops, eggs_health, face_health, fade_steps, first_attack_missiles, gut_health, health, landing_shake_strength, orb_speed_across, orb_speed_up, orbs_per_burst, repeated_jump_steps, space_jump_updates, stagger_health, start_look, start_timeline, start_tint, turn_delay |
| `bosses/golden_torizo_drool.json` | `TorizoDrool` | drag, drift, gravity |
| `bosses/golden_torizo_orb.json` | `TorizoOrb` | bounce_drag, bounces, gravity |
| `bosses/golden_torizo_sonic_boom.json` | `TorizoSonicBoom` | quickening |
| `bosses/kraid.json` | `Kraid` | backdrop_fade_steps, big_dud_effect, contact_damage, death_drops, dud_effect, dud_sound, health, lunge_delay_updates, mouth_dud_effect, rise_wait_updates, rock_sound, samus_in_view_facing_left, samus_in_view_facing_right, spit_cadence_updates, thinking_step_updates |
| `bosses/kraid_ceiling_rock.json` | `KraidRock` | drift, drift_limit, gravity |
| `bosses/kraid_lint.json` | `KraidLint` | charge_updates, contact_damage, emerge_pixels, fire_sound, fire_speed, harmless_x, health, point_x, reload_updates, return_x |
| `bosses/kraid_nail.json` | `KraidNail` | below_arm, contact_damage, health, launch_delay, launch_reach, past_ledges_left |
| `bosses/kraid_rock.json` | `KraidRock` | drift, drift_limit, gravity |
| `bosses/kraid_second_nail.json` | `KraidNail` | below_arm, contact_damage, health, launch_delay, launch_reach, past_ledges_left |
| `bosses/mother_brain.json` | `MotherBrain` | ascent_dust_sound, attack_cry_sound, big_breath_effect, big_dust_effect, big_explosion_effect, charge_sound, charge_stop_sound, contact_damage, corpse_dust_effect, corpse_dust_sound, door_fragment_effect, drained_grey_step_updates, dust_effect, explosion_effect, explosion_sound, fake_death_grey_step_updates, finish_off_energy, first_phase_health, footstep_sound, from_grey_step_updates, full_strength_health, grey_steps, hand_beam_sound, health, hurt_sound, laser_sound, rainbow_beam_sound, rainbow_beam_stop_sound, samus_stands_energy, second_phase_health, small_breath_effect, smoke_effect, third_phase_health |
| `bosses/mother_brain_baby.json` | `MotherBrainBaby` | contact_damage, cry_sound, death_sound, dust_effect, explosion_effect, explosion_sound, heal_sound, health, remorse_cry_sound, shielding_health |
| `bosses/mother_brain_bomb.json` | `MotherBrainBomb` | dust_effect, explosion_effect, explosion_sound, flare, floor, right_wall |
| `bosses/mother_brain_charge_particle.json` | `MotherBrainDrool` | floor, release_drop, splash_rise, splash_timeline |
| `bosses/mother_brain_drool.json` | `MotherBrainDrool` | floor, release_drop, splash_rise, splash_timeline |
| `bosses/mother_brain_dying_drool.json` | `MotherBrainDrool` | floor, release_drop, splash_rise, splash_timeline |
| `bosses/mother_brain_glass.json` | `MotherBrainGlass` | burst_updates, contact_damage, health, shard_arcs, shard_drop, shatter_sound, shatter_updates, shattered_patch |
| `bosses/mother_brain_glass_shard.json` | `MotherBrainGlassShard` | bottom, gravity, sparkle_centre, sparkle_effect, sparkle_spread |
| `bosses/mother_brain_ring.json` | `MotherBrainRing` | floor, mouth_updates, top, view_right |
| `bosses/mother_brain_turret.json` | `MotherBrainTurret` | contact_damage, health, least_fire_wait, least_turn_wait, smoke_effect, view_across, view_down, view_height, view_width |
| `bosses/mother_brain_turret_bullet.json` | `MotherBrainTurretBullet` | smoke_timeline |
| `bosses/phantoon.json` | `Phantoon` | big_explosion_effect, big_explosion_sound, contact_damage, death_drops, dying_cry, fade_steps, flame_sound, health, rage_sound, round_damage, shot_cry, small_explosion_effect, small_explosion_sound |
| `bosses/phantoon_flame.json` | `PhantoonFlame` | burning_timeline, drift_speed, falling_timeline, gravity, landing_drop, landing_pause, landing_timeline, leap_speed, rain_impact_timeline, rain_sound, resting_timeline, ring_hold_updates, ring_radius |
| `bosses/ridley.json` | `Ridley` | arena_reveal_step_updates, arena_reveal_updates, baby_cry_sound, before_roar_updates, body_fade_step_updates, bounce_sound, contact_damage, death_drops, death_lunges, dust_effect, explosion_effect, explosion_sound, eye_fade_step_updates, eye_hold_updates, fade_steps, health, hit_flash_updates, hits_to_drive_off, piece_end_effect, reveal_delay, roar_hold_updates, roar_sound, samus_low_energy, tail_damage, tail_tip_arc_start, wounded_health |
| `bosses/ridley_fireball.json` | `RidleyFireball` | final_timeline, flame, flame_speed, flare_across, flare_down, is |
| `bosses/ridley_flame.json` | `RidleyFireball` | final_timeline, flame, flame_speed, flare_across, flare_down, is |
| `bosses/ridley_flare.json` | `RidleyFireball` | final_timeline, flame, flame_speed, flare_across, flare_down, is |
| `bosses/spore_spawn.json` | `SporeSpawn` | ceiling_above, ceiling_dust_effect, contact_damage, death_drops, descent_speed, dust_effect, explosion_effect, explosion_sound, fast_sway_step, fast_swing_below, hardening_sound, health, stalk_anchor_above, stalk_body_gap, start_rise, sway_widening, vertical_sway_narrowing, widened_sway_limit |
| `bosses/spore_spawn_spore.json` | `SporeSpawnSpore` | burst_timeline, fall_distance |
| `bosses/torizo_drool.json` | `TorizoDrool` | drag, drift, gravity |
| `bosses/torizo_egg.json` | `TorizoEgg` | bounce_drag, hatch_updates, quickening |
| `bosses/torizo_eye_beam.json` | `TorizoEyeBeam` |  |
| `bosses/torizo_orb.json` | `TorizoOrb` | bounce_drag, bounces, gravity |
| `bosses/torizo_shot.json` | `CuedProjectile` |  |
| `bosses/torizo_sonic_boom.json` | `TorizoSonicBoom` | quickening |
| `bosses/torizo_super_missile.json` | `TorizoSuperMissile` | gravity, throw_speed |
| `cinematics/intro_egg.json` | `IntroEgg` | cry_sound, hatch_sound, slime, slime_splash |
| `cinematics/intro_mother_brain.json` | `IntroMotherBrain` | big_explosion, contact_damage, health, ring, small_explosion |
| `enemies/alcoon.json` | `Alcoon` | contact_damage, emerge_across_distance, emerge_down_distance, emerge_jump_speed, emerge_sound, gravity, health, hide_distance, hide_jump_speed, muzzle_across, muzzle_up, stride |
| `enemies/alcoon_fireball.json` | `AlcoonFireball` | slowing, steady_speed |
| `enemies/armored_zoomer.json` | `Crawler` | contact_damage, health, landing_speed, pursues |
| `enemies/atomic.json` | `Atomic` | contact_damage, health |
| `enemies/baby_metroid.json` | `BabyMetroid` | beats_a_cry, calm_pulse_updates, contact_damage, cry_beat, cry_sound, feeding_cry_sound, feeding_pulse_updates, fly_away_cry_sound, health, pulse_beats, remorse_cry_sound, walls_down_patch, walls_up_patch |
| `enemies/baby_turtle.json` | `BabyTurtle` | contact_damage, crawl_speed, health, hide_updates, rider_left_delay, spin_fall_speed, spin_speed, travel_distance |
| `enemies/beetom.json` | `Beetom` | bite_every, cling_above, contact_damage, crawl_speed, crawl_updates, drop_speed, fastest_mark, feeding_sound, feeding_sound_every, feeding_sound_least_energy, flung_marks, flung_speed, health, hop_speed, idle_updates, ledge_reach, long_hop_height, long_hop_marks, lunge_aim_marks, lunge_distance, lunge_falling_marks, lunge_height, lunge_rising_marks, lunge_speed, shake_off_count, short_hop_height, short_hop_marks, speed_curve, thrown_distance |
| `enemies/boulder.json` | `Boulder` | bouncing_travel_step, break_effect, break_sound, contact_damage, fastest_bouncing_mark, fastest_rolling_mark, first_rebound_mark, ground_reach, health, impact_sound, rolling_travel_step, speed_curve |
| `enemies/boyon.json` | `Boyon` | contact_damage, health |
| `enemies/bull.json` | `Bull` | acceleration_step, contact_damage, health, kick_acceleration, kick_guard_updates, kick_speed, off_target_turn, wait_updates |
| `enemies/cacatac.json` | `Cacatac` | attack_chances, contact_damage, diagonal_spike_speed, health, spike_speed |
| `enemies/choot.json` | `Choot` | ascent_step, contact_damage, health, reach, speed_curve |
| `enemies/chozo_statue_lower_norfair.json` | `ChozoStatue` | contact_damage, footstep_depth, footstep_effect, hand_patch, health, lowering_delay, plug_patches, shut_patch, way_patch |
| `enemies/chozo_statue_wrecked_ship.json` | `ChozoStatue` | contact_damage, footstep_depth, footstep_effect, hand_patch, health, lowering_delay, plug_patches, shut_patch, way_patch |
| `enemies/corpse_ripper_level.json` | `Corpse` | contact_damage, disturbed_updates, dust_effect, dust_sound, health, look, rows |
| `enemies/corpse_ripper_tilted.json` | `Corpse` | contact_damage, disturbed_updates, dust_effect, dust_sound, health, look, rows |
| `enemies/corpse_skree.json` | `Corpse` | contact_damage, disturbed_updates, dust_effect, dust_sound, health, look, rows |
| `enemies/corpse_zoomer_on_its_back.json` | `Corpse` | contact_damage, disturbed_updates, dust_effect, dust_sound, health, look, rows |
| `enemies/corpse_zoomer_upright.json` | `Corpse` | contact_damage, disturbed_updates, dust_effect, dust_sound, health, look, rows |
| `enemies/covern.json` | `Covern` | bob_quickening, bob_start_speed, contact_damage, first_extra_delay, health, lead_distance, reappearance_delay, steady_course_updates, still_updates, visible_updates |
| `enemies/dead_sidehopper.json` | `DeadSidehopper` | contact_damage, disturbed_updates, drain_step_updates, dust_effect, dust_sound, health, pause_updates, rows |
| `enemies/dead_torizo.json` | `DeadTorizo` | contact_damage, disturbed_updates, dust_effect, dust_sound, health, look, rows, sand_line_updates, sand_lines, sand_rows |
| `enemies/dessgeega.json` | `Hopper` | across_speed, big_hop_height, big_hop_step, contact_damage, fastest_mark, health, small_hop_height, small_hop_step, speed_curve |
| `enemies/dessgeega_large.json` | `Hopper` | across_speed, big_hop_height, big_hop_step, contact_damage, fastest_mark, health, small_hop_height, small_hop_step, speed_curve |
| `enemies/dragon.json` | `Dragon` | contact_damage, fireball_rise, health, muzzle_across, muzzle_up, rise_updates, shots_per_attack, wait_before_rising, wait_before_sinking |
| `enemies/evir.json` | `Evir` | arms_across, arms_down, contact_damage, grip_across, grip_down, health |
| `enemies/evir_spit.json` | `EvirSpit` | contact_damage, health, regrow_distance, regrow_margin, speed, throw_distance |
| `enemies/eye_door_eye.json` | `EyeDoorEye` | contact_damage, dud_sound, health, hit_sound, hits_to_destroy, puff_effect, smoke_effect, spit_sound |
| `enemies/eye_door_spit.json` | `EyeDoorSpit` | burst_timeline, shot_effect |
| `enemies/eye_door_sweat.json` | `EyeDoorSweat` | gravity, splash_effect |
| `enemies/face_block.json` | `FaceBlock` | contact_damage, glow_updates, health, wakes_with |
| `enemies/fake_kraid.json` | `FakeKraid` | contact_damage, cry_sound, death_drops, health, spike_sound, step_pixels |
| `enemies/falling_spark.json` | `FallingSpark` | fall_speed_limit, gravity, skip_lift, skip_speed, skitter_timeline, skitter_times, trail_effect, trail_every |
| `enemies/fireflea.json` | `Fireflea` | contact_damage, health |
| `enemies/fune.json` | `Fune` | contact_damage, health, mouth_drop |
| `enemies/gamet.json` | `Gamet` | contact_damage, health, rise_speed |
| `enemies/geega.json` | `Geega` | contact_damage, emergence_delay, health, speed_curve, swoop_distance, swoop_start_mark, trigger_distance, trigger_height |
| `enemies/geruta.json` | `Swooper` | across_speed, contact_damage, dive_sound, dive_speed, health, idles_whenever_perched, lift, reach, return_sound, return_speed, slow_dive_speed |
| `enemies/grapple_ripper.json` | `Ripper` | can_be_latched, contact_damage, health |
| `enemies/hibashi.json` | `Hibashi` | contact_damage, first_wait, half_width, health |
| `enemies/holtz.json` | `Swooper` | across_speed, contact_damage, dive_sound, dive_speed, health, idles_whenever_perched, lift, reach, return_sound, return_speed, slow_dive_speed |
| `enemies/jet_ripper.json` | `Ripper` | can_be_latched, contact_damage, health |
| `enemies/kago.json` | `Kago` | contact_damage, health, shake_strength, shake_updates, shake_way |
| `enemies/kago_bug.json` | `KagoBug` | across_speed, clear_distance, contact_damage, fall_start_speed, first_sound_after, gravity, health, homing_distance, jump_speed, shortest_wait, sound |
| `enemies/kamer.json` | `Shutter` | activation_sound, contact_damage, health |
| `enemies/ki_hunter_acid.json` | `KiHunterAcid` | fastest_fall, gravity, launch_shift, splash |
| `enemies/ki_hunter_green.json` | `KiHunter` | contact_damage, gravity, health, hop_across_speed, hop_up_speed, landing_wait, patrol_depth, patrol_rise, patrol_speed, speed_curve, spit_height, spit_range, spit_reach, spit_wait, strike_after_steps, swoop_distance, swoop_fastest_turn, swoop_least_depth, swoop_quickening, wing_arc_climb, wing_arc_radius, wing_arc_step, wing_half_height, wing_half_width |
| `enemies/ki_hunter_red.json` | `KiHunter` | contact_damage, gravity, health, hop_across_speed, hop_up_speed, landing_wait, patrol_depth, patrol_rise, patrol_speed, speed_curve, spit_height, spit_range, spit_reach, spit_wait, strike_after_steps, swoop_distance, swoop_fastest_turn, swoop_least_depth, swoop_quickening, wing_arc_climb, wing_arc_radius, wing_arc_step, wing_half_height, wing_half_width |
| `enemies/ki_hunter_yellow.json` | `KiHunter` | contact_damage, gravity, health, hop_across_speed, hop_up_speed, landing_wait, patrol_depth, patrol_rise, patrol_speed, speed_curve, spit_height, spit_range, spit_reach, spit_wait, strike_after_steps, swoop_distance, swoop_fastest_turn, swoop_least_depth, swoop_quickening, wing_arc_climb, wing_arc_radius, wing_arc_step, wing_half_height, wing_half_width |
| `enemies/kzan.json` | `Kzan` | contact_damage, fall_quickening, fastest_fall, health, landing_sound, rise_speed, rise_wait |
| `enemies/magdollite.json` | `Magdollite` | attack_cooldown, attack_poll_updates, contact_damage, glow_steps, glow_updates, health, hidden_depth, lava_drop, maximum_rise, most_lengths, pillar_length, reach_past_hand, smallest_reach, sunk_depth, surfaced_depth, throw_sound |
| `enemies/mama_turtle.json` | `MamaTurtle` | awake_half_height, contact_damage, fall_quickening, fastest_fall, fastest_hover, health, hover_quickening, peak_pause, ridden_climb_speed, ridden_peak, rise_updates, wall_shake_strength, wall_shake_updates, wall_sound |
| `enemies/maridia_fly.json` | `Fly` | contact_damage, dive_across_speed, dive_down_speed, health, reach, rest_updates, wheel_step |
| `enemies/mella.json` | `Fly` | contact_damage, dive_across_speed, dive_down_speed, health, reach, rest_updates, wheel_step |
| `enemies/mellow.json` | `Fly` | contact_damage, dive_across_speed, dive_down_speed, health, reach, rest_updates, wheel_step |
| `enemies/metaree.json` | `Diver` | burrow_sound, burrow_updates, contact_damage, debris_at, distance_per_speed, dive_sound, dive_speed, drift, health, reach |
| `enemies/metroid.json` | `Metroid` | aim_above, contact_damage, drain_hundredths, drop_count, fastest_chase, feeding_sound, feeding_sound_every, feeding_sound_least_energy, frozen_updates, health, latch_distance, recoil_sound |
| `enemies/mochtroid.json` | `Mochtroid` | attached_speed, bite_every, contact_damage, fastest_flight, feeding_sound, feeding_sound_every, feeding_sound_least_energy, health |
| `enemies/multiviola.json` | `Multiviola` | contact_damage, health |
| `enemies/namihe.json` | `Fune` | contact_damage, health, mouth_drop |
| `enemies/ninja_pirate_gold.json` | `NinjaPirate` | activation_distance, claw_attack_every, contact_damage, dive_speed_change, dive_start_speed, dive_step, flinch_distance, health, kick_distance, landing_dust_drop, landing_dust_effect, landing_dust_spread, midpoint_distance, return_step, scattered_drops, spin_climb_step, spin_speed_change |
| `enemies/ninja_pirate_silver.json` | `NinjaPirate` | activation_distance, claw_attack_every, contact_damage, dive_speed_change, dive_start_speed, dive_step, flinch_distance, health, kick_distance, landing_dust_drop, landing_dust_effect, landing_dust_spread, midpoint_distance, return_step, scattered_drops, spin_climb_step, spin_speed_change |
| `enemies/nuclear_waffle.json` | `NuclearWaffle` | contact_damage, health, left_burst_effect, link_damage, link_half_size, link_spacing, right_burst_effect, splash_effect, surface_reach, surface_sound, sweep_middle, sweep_reach |
| `enemies/orange_zoomer.json` | `Crawler` | contact_damage, health, landing_speed, pursues |
| `enemies/oum.json` | `Oum` | attack_cooldown, attack_reach, bounce_step, contact_damage, fastest_fall_mark, health, impact_loss_marks, pushing_reach_across, pushing_reach_down, roll_speed, settling_bounces, shove_distance, speed_curve |
| `enemies/owtch.json` | `Owtch` | burial_depth, contact_damage, health, sink_chance |
| `enemies/pirate_claw.json` | `PirateClaw` | sink, speed_change |
| `enemies/polyp.json` | `Polyp` | contact_damage, first_launch_mark, health, reach |
| `enemies/polyp_rock.json` | `PolypRock` | curve_marks, fastest_fall_mark, speed_curve |
| `enemies/powamp.json` | `Powamp` | contact_damage, death_delay, health, inflation_updates, rest_updates, rise_distance, rise_speed, sink_speed, spike_quickening, wiggle_step_updates |
| `enemies/puyo.json` | `Puyo` | contact_damage, health, speed_curve |
| `enemies/rinka.json` | `Rinka` | aim_updates, contact_damage, health, puff_effect, speed, view_margin |
| `enemies/rio.json` | `Rio` | across_speed, contact_damage, dive_sound, dive_speed, health, lift, reach, return_speed |
| `enemies/ripper.json` | `Ripper` | can_be_latched, contact_damage, health |
| `enemies/sbug.json` | `Sbug` | contact_damage, dash_updates, health, zigzag_leg_updates |
| `enemies/sciser.json` | `Crawler` | contact_damage, health, landing_speed, pursues |
| `enemies/shaktool.json` | `Shaktool` | contact_damage, convergence_shares, health, part_half_size, straighten_at, straighten_restart, straightening_turn |
| `enemies/shutter_destroyable.json` | `Shutter` | activation_sound, contact_damage, health |
| `enemies/shutter_growing.json` | `GrowingShutter` | activation_sound, contact_damage, health |
| `enemies/shutter_shootable.json` | `Shutter` | activation_sound, contact_damage, health |
| `enemies/sidehopper.json` | `Hopper` | across_speed, big_hop_height, big_hop_step, contact_damage, fastest_mark, health, small_hop_height, small_hop_step, speed_curve |
| `enemies/sidehopper_large.json` | `Hopper` | across_speed, big_hop_height, big_hop_step, contact_damage, fastest_mark, health, small_hop_height, small_hop_step, speed_curve |
| `enemies/sidehopper_tourian.json` | `Hopper` | across_speed, big_hop_height, big_hop_step, contact_damage, fastest_mark, health, small_hop_height, small_hop_step, speed_curve |
| `enemies/skree.json` | `Diver` | burrow_sound, burrow_updates, contact_damage, debris_at, distance_per_speed, dive_sound, dive_speed, drift, health, reach |
| `enemies/skultera.json` | `Skultera` | contact_damage, health |
| `enemies/sova.json` | `Crawler` | contact_damage, health, landing_speed, pursues |
| `enemies/spark.json` | `Spark` | contact_damage, drip_below, extra_off_updates, health |
| `enemies/squeept.json` | `Squeept` | block_height, contact_damage, flip_speed, gravity, health, jump_sound, screen_height |
| `enemies/suspensor_platform.json` | `Tripper` | contact_damage, health, resting_depth, speed_curve |
| `enemies/tripper.json` | `Tripper` | contact_damage, health, resting_depth, speed_curve |
| `enemies/viola.json` | `Crawler` | contact_damage, health, landing_speed, pursues |
| `enemies/walking_pirate_gold.json` | `WalkingPirate` | contact_damage, firing_band, flinch_reach, health, ledge_reach_left, ledge_reach_right, muzzle_ahead, walk_speed_left, walk_speed_right |
| `enemies/walking_pirate_green.json` | `WalkingPirate` | contact_damage, firing_band, flinch_reach, health, ledge_reach_left, ledge_reach_right, muzzle_ahead, walk_speed_left, walk_speed_right |
| `enemies/walking_pirate_grey.json` | `WalkingPirate` | contact_damage, firing_band, flinch_reach, health, ledge_reach_left, ledge_reach_right, muzzle_ahead, walk_speed_left, walk_speed_right |
| `enemies/walking_pirate_magenta.json` | `WalkingPirate` | contact_damage, firing_band, flinch_reach, health, ledge_reach_left, ledge_reach_right, muzzle_ahead, walk_speed_left, walk_speed_right |
| `enemies/walking_pirate_red.json` | `WalkingPirate` | contact_damage, firing_band, flinch_reach, health, ledge_reach_left, ledge_reach_right, muzzle_ahead, walk_speed_left, walk_speed_right |
| `enemies/walking_pirate_silver.json` | `WalkingPirate` | contact_damage, firing_band, flinch_reach, health, ledge_reach_left, ledge_reach_right, muzzle_ahead, walk_speed_left, walk_speed_right |
| `enemies/wall_pirate_gold.json` | `WallPirate` | block_size, contact_damage, detection_band, fast_jump_turn, health, muzzle_above, muzzle_ahead, slow_jump_short_by, slow_jump_turn |
| `enemies/wall_pirate_grey.json` | `WallPirate` | block_size, contact_damage, detection_band, fast_jump_turn, health, muzzle_above, muzzle_ahead, slow_jump_short_by, slow_jump_turn |
| `enemies/wall_pirate_magenta.json` | `WallPirate` | block_size, contact_damage, detection_band, fast_jump_turn, health, muzzle_above, muzzle_ahead, slow_jump_short_by, slow_jump_turn |
| `enemies/wall_pirate_silver.json` | `WallPirate` | block_size, contact_damage, detection_band, fast_jump_turn, health, muzzle_above, muzzle_ahead, slow_jump_short_by, slow_jump_turn |
| `enemies/waver.json` | `Waver` | contact_damage, health, speed, spins_at, wave_height, wave_step |
| `enemies/work_robot.json` | `WorkRobot` | contact_damage, footstep_sound, gravity, health, laser_cooldown, laser_slant_speed, laser_sound, muzzle_above, muzzle_ahead, recoil_delay, turn_delay |
| `enemies/yapping_maw.json` | `YappingMaw` | bite_sound, contact_damage, grab_cooldown, health, longest_reach, retracted_hold, root_distance, safety_distance, speed_curve |
| `enemies/yard.json` | `Yard` | aggressive_crawl_speed, air_drag, bounce_sound, contact_damage, fall_speed_limit, falls_at_shake, gravity, health, hiding_probe_reach, hiding_reach_above, landing_speed_limit, shot_launch_speed, turn_lookahead, turn_rest_updates |
| `enemies/zeb.json` | `PipeBug` | contact_damage, emergence_height, flight_step, health, respawn_wait, trigger_top, trigger_width |
| `enemies/zebbo.json` | `PipeBug` | contact_damage, emergence_height, flight_step, health, respawn_wait, trigger_top, trigger_width |
| `enemies/zebetite.json` | `Zebetite` | contact_damage, fall_effect, health, pair_offset, shot_sound |
| `enemies/zeela.json` | `Crawler` | contact_damage, health, landing_speed, pursues |
| `enemies/zero.json` | `Crawler` | contact_damage, health, landing_speed, pursues |
| `enemies/zoa.json` | `Zoa` | contact_damage, health, reach, rise_speed |
| `enemies/zoomer.json` | `Crawler` | contact_damage, health, landing_speed, pursues |
| `hud/hud.json` | `Hud` | area_folder, select_sound |
| `objects/ceres_door_left.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/ceres_door_right.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/ceres_elevator_door.json` | `CeresElevatorDoor` | blocked, explosion, explosion_sound, smoke |
| `objects/ceres_ridley_room_door.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/ceres_steam_down.json` | `Hazard` | bite_damage, hit_damage, hits_without_damage, hold_updates, ongoing_hundredths, release_invincible_updates |
| `objects/ceres_steam_left.json` | `Hazard` | bite_damage, hit_damage, hits_without_damage, hold_updates, ongoing_hundredths, release_invincible_updates |
| `objects/ceres_steam_right.json` | `Hazard` | bite_damage, hit_damage, hits_without_damage, hold_updates, ongoing_hundredths, release_invincible_updates |
| `objects/ceres_steam_up.json` | `Hazard` | bite_damage, hit_damage, hits_without_damage, hold_updates, ongoing_hundredths, release_invincible_updates |
| `objects/dachora.json` | `Dachora` | activation_sound, ceiling_sound, charge_sound, echo_effect, launch_sound, speed_sound, speed_stop_sound |
| `objects/door_cap_blue_down.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/door_cap_blue_left.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/door_cap_blue_right.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/door_cap_blue_up.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/door_cap_eye_left.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/door_cap_eye_right.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/door_cap_green_down.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/door_cap_green_left.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/door_cap_green_right.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/door_cap_green_up.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/door_cap_grey_down.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/door_cap_grey_left.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/door_cap_grey_right.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/door_cap_grey_up.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/door_cap_red_left.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/door_cap_red_right.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/door_cap_yellow_down.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/door_cap_yellow_left.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/door_cap_yellow_right.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/door_cap_yellow_up.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/elevator.json` | `Elevator` |  |
| `objects/escape_dachora.json` | `EscapeDachora` |  |
| `objects/escape_etecoon.json` | `EscapeEtecoon` |  |
| `objects/etecoon.json` | `Etecoon` | jump_sound, wake_sound, wall_sound |
| `objects/gate.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/gate_switch_blue_left.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/gate_switch_blue_right.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/gate_switch_green_left.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/gate_switch_green_right.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/glass_tube.json` | `GlassTube` | break_sound, bubble, crack, cracking, dud_sound, intact, open_patch, shattered |
| `objects/gunship.json` | `Gunship` | bottom, bottom_below_top, glow, hover_sound, pad_above_top, pad_closed, pad_closing, pad_closing_sound, pad_opening, pad_opening_sound, saved_message_before_opening_updates, saved_message_hold_updates, saving_sound, saving_updates, top |
| `objects/item_shell.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, refused_sound, sealed_sound |
| `objects/morph_ball_eye.json` | `MorphBallEye` | activation_sound, beam_stop_sound, transition_updates |
| `objects/samus_eater.json` | `SamusEaters` |  |
| `rooms/tourian_statue_hall.json` | `TourianStatueHall` |  |
| `samus/power_bomb.json` | `PowerBomb` | blast_glows, blast_ovals, blast_reaches, flash_blast_glows, flash_blast_ovals, flash_blast_reaches, fuse_updates, height_parts, rehit_after, width_parts |
| `samus/samus.json` | `Samus` | bomb_jump_fuse_left, bomb_spread_fuses, bomb_spread_throws, bomb_spread_updates_per_pixel, bomb_spread_wind_up_updates, boost_stride_counts, boost_stride_updates, boosted_dash_cap, breath_bubble_below_top, breath_bubble_effect, breath_bubble_every, breath_bubble_hold_updates, breath_high_sound, breath_low_sound, breath_mouth_above_top, can_crawl, can_crouch, can_jump_while_crawling, can_wall_jump, change_pictures, charge_sound_updates, charged_spin_updates, climb_per_pixel, crawl_bounce_fall_speed, crawl_bounce_speed, crawl_enter_updates, crawl_leave_updates, crawl_needs, crawl_second_bounce_speed, crouch_jump_lift, crouch_needs, crouch_updates, crouch_while_running, crystal_flash_ammunition, crystal_flash_drain_every, crystal_flash_energy_below, crystal_flash_energy_each, dash_acceleration, dash_action, dash_boost_needs, dash_cap, dash_stride_updates, fast_fall_look_speed, forms, gravity_damage_over, half_width, hard_landing_speed, hazard_invincible_updates, hazard_knockback_updates, hurt_flash_coloured_counts, hurt_flash_end_count, hurt_flash_recovery_count, hurt_flash_sound_count, invincible_updates, jump_boost_needs, jump_buffer_updates, jump_cut_speed, jump_start_updates, jump_while_crawling_needs, knockback_updates, land_effect, landing_updates, low_energy, mediums, reserve_refill_per_update, room_damage_sound, room_damage_sound_every, room_damage_sound_least_energy, run_stride_updates, shinespark_crash_updates, shinespark_echo_pool, shinespark_echo_shot, shinespark_least_energy, shinespark_lift, shinespark_most_rise, shinespark_rise_gain, shinespark_speed, shinespark_top_speed, shinespark_windup_updates, spark_finished_glow_updates, spark_glow_updates, spark_hurt_flash_count, spark_windup_glow_updates, special_attack_trails, special_attack_updates, special_attacks, spin_landing_updates, spin_start_updates, stand_updates, starting_energy, stored_shine_sound_updates, stored_shine_updates, suit_pickup_updates, turn_updates, varia_damage_over, wall_jump_across_speed, wall_jump_from_any_jump, wall_jump_launch_updates, wall_jump_lock_updates, wall_jump_needs, wall_jump_needs_steering_away, wall_jump_reach, wall_jump_ready_updates, wall_jump_steered, weapons, xray_aim_bands, xray_closing_updates, xray_reach, xray_warm_up_updates, xray_widening_step, xray_widest_half_width |

## Room layouts

Each is a terrain layout. Those with patches listed must hold patches of those names.

| File | Patches |
| --- | --- |
| `cinematics/intro_discovery_room.json` |  |
| `cinematics/intro_mother_brain_room.json` |  |
| `rooms/brinstar/alpha_power_bomb_in_red_brinstar_kraid.json` |  |
| `rooms/brinstar/below_spazer_in_red_brinstar_kraid.json` |  |
| `rooms/brinstar/beta_power_bomb_in_red_brinstar_kraid.json` |  |
| `rooms/brinstar/big_pink_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/billy_mays_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/blue_brinstar_boulders_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/blue_brinstar_energy_tank_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/brinstar_pre_map_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/brinstar_reserve_tank_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/caterpillar_in_red_brinstar_kraid.json` |  |
| `rooms/brinstar/construction_zone_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/dachora_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/early_supers_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/energy_refill_room_door_on_right_in_blue_refill.json` |  |
| `rooms/brinstar/etecoon_energy_tank_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/etecoon_super_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/first_missile_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/green_brinstar_beetoms_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/green_brinstar_firefleas_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/green_brinstar_main_shaft_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/green_brinstar_missile_refill_in_yellow_refill.json` |  |
| `rooms/brinstar/green_hill_zone_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/hellway_in_red_brinstar_kraid.json` |  |
| `rooms/brinstar/hopper_energy_tank_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/kraid_eye_door_in_red_brinstar_kraid.json` |  |
| `rooms/brinstar/kraid_in_kraid.json` | ceiling_clear, spikes_clear |
| `rooms/brinstar/kraid_refill_in_yellow_refill.json` |  |
| `rooms/brinstar/map_station_room_door_on_right_in_map_statues.json` |  |
| `rooms/brinstar/mini_kraid_in_red_brinstar_kraid.json` |  |
| `rooms/brinstar/morph_ball_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/noob_bridge_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/pink_brinstar_hoppers_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/pink_brinstar_power_bombs_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/red_brinstar_firefleas_in_red_brinstar_kraid.json` |  |
| `rooms/brinstar/red_tower_in_red_brinstar_kraid.json` |  |
| `rooms/brinstar/save_room_door_on_left_in_save_station.json` |  |
| `rooms/brinstar/save_room_door_on_right_in_save_station.json` |  |
| `rooms/brinstar/skree_boost_in_red_brinstar_kraid.json` |  |
| `rooms/brinstar/spazer_in_red_brinstar_kraid.json` |  |
| `rooms/brinstar/spore_spawn_farming_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/spore_spawn_in_green_blue_brinstar.json` | ceiling_open |
| `rooms/brinstar/spore_spawn_kihunters_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/spore_spawn_super_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/varia_suit_in_red_brinstar_kraid.json` |  |
| `rooms/brinstar/warehouse_energy_tank_in_red_brinstar_kraid.json` |  |
| `rooms/brinstar/warehouse_entrance_in_red_brinstar_kraid.json` |  |
| `rooms/brinstar/warehouse_kihunter_in_red_brinstar_kraid.json` |  |
| `rooms/brinstar/warehouse_zeela_in_red_brinstar_kraid.json` |  |
| `rooms/brinstar/waterway_energy_tank_in_green_blue_brinstar.json` |  |
| `rooms/brinstar/xray_scope_in_red_brinstar_kraid.json` |  |
| `rooms/ceres/ceres_elevator_in_blue_ceres_elevator.json` |  |
| `rooms/ceres/ceres_elevator_in_white_ceres_elevator.json` |  |
| `rooms/ceres/ceres_ridley_hallway_in_blue_ceres.json` |  |
| `rooms/ceres/ceres_ridley_hallway_in_white_ceres.json` |  |
| `rooms/ceres/ceres_ridley_in_blue_ceres_ridley.json` |  |
| `rooms/ceres/ceres_ridley_in_white_ceres_ridley.json` |  |
| `rooms/ceres/dead_scientist_in_blue_ceres.json` |  |
| `rooms/ceres/dead_scientist_in_white_ceres.json` |  |
| `rooms/ceres/falling_tile_in_blue_ceres.json` |  |
| `rooms/ceres/falling_tile_in_white_ceres.json` |  |
| `rooms/ceres/magnet_stairs_in_blue_ceres.json` |  |
| `rooms/ceres/magnet_stairs_in_white_ceres.json` |  |
| `rooms/crateria/bomb_torizo_in_lower_crateria.json` | wall_shot_blocks |
| `rooms/crateria/bowling_alley_path_in_upper_crateria.json` |  |
| `rooms/crateria/climb_in_lower_crateria.json` | escape_wall_destroyed |
| `rooms/crateria/climb_in_old_tourian.json` | escape_wall_destroyed |
| `rooms/crateria/crab_maze_in_upper_crateria.json` |  |
| `rooms/crateria/crateria_kihunter_in_upper_crateria.json` |  |
| `rooms/crateria/crateria_power_bombs_in_upper_crateria.json` |  |
| `rooms/crateria/crateria_super_in_lower_crateria.json` |  |
| `rooms/crateria/crateria_tube_in_upper_crateria.json` |  |
| `rooms/crateria/east_ocean_in_upper_crateria.json` |  |
| `rooms/crateria/elevator_to_blue_brinstar_in_lower_crateria.json` |  |
| `rooms/crateria/final_missile_bombway_in_lower_crateria.json` |  |
| `rooms/crateria/final_missile_in_lower_crateria.json` |  |
| `rooms/crateria/flyway_in_lower_crateria.json` |  |
| `rooms/crateria/forgotten_highway_elbow_in_upper_crateria.json` |  |
| `rooms/crateria/forgotten_highway_elevator_in_lower_crateria.json` |  |
| `rooms/crateria/forgotten_highway_kagos_in_upper_crateria.json` |  |
| `rooms/crateria/gauntlet_energy_tank_in_upper_crateria.json` |  |
| `rooms/crateria/gauntlet_entrance_in_upper_crateria.json` |  |
| `rooms/crateria/green_brinstar_elevator_in_lower_crateria.json` |  |
| `rooms/crateria/green_pirates_shaft_in_lower_crateria.json` |  |
| `rooms/crateria/landing_site_in_upper_crateria.json` |  |
| `rooms/crateria/lower_mushrooms_in_lower_crateria.json` |  |
| `rooms/crateria/map_station_room_door_on_left_in_map_statues.json` |  |
| `rooms/crateria/moat_in_upper_crateria.json` |  |
| `rooms/crateria/parlor_in_upper_crateria.json` | animals_passage_open |
| `rooms/crateria/pit_in_lower_crateria.json` |  |
| `rooms/crateria/pit_in_old_tourian.json` |  |
| `rooms/crateria/pre_bowling_in_upper_crateria.json` |  |
| `rooms/crateria/pre_map_flyway_in_lower_crateria.json` |  |
| `rooms/crateria/red_brinstar_elevator_in_lower_crateria.json` |  |
| `rooms/crateria/save_room_door_on_right_in_save_station.json` |  |
| `rooms/crateria/statues_hallway_in_statues_hall.json` |  |
| `rooms/crateria/statues_in_map_statues.json` | floor_clear |
| `rooms/crateria/terminator_in_lower_crateria.json` |  |
| `rooms/crateria/west_ocean_in_upper_crateria.json` |  |
| `rooms/maridia/aqueduct_in_sandy_maridia.json` |  |
| `rooms/maridia/below_botwoon_energy_tank_in_sandy_maridia.json` |  |
| `rooms/maridia/botwoon_energy_tank_in_sandy_maridia.json` |  |
| `rooms/maridia/botwoon_hallway_in_sandy_maridia.json` |  |
| `rooms/maridia/botwoon_in_sandy_maridia.json` | wall_down |
| `rooms/maridia/botwoon_quicksand_in_sandy_maridia.json` |  |
| `rooms/maridia/bug_sand_hole_in_sandy_maridia.json` |  |
| `rooms/maridia/butterfly_in_sandy_maridia.json` |  |
| `rooms/maridia/colosseum_in_sandy_maridia.json` |  |
| `rooms/maridia/crab_hole_in_sandless_maridia.json` |  |
| `rooms/maridia/crab_shaft_in_sandless_maridia.json` |  |
| `rooms/maridia/crab_tunnel_in_sandless_maridia.json` |  |
| `rooms/maridia/draygon_in_draygon.json` |  |
| `rooms/maridia/east_aqueduct_quicksand_in_sandy_maridia.json` |  |
| `rooms/maridia/east_cactus_alley_in_sandy_maridia.json` |  |
| `rooms/maridia/east_pants_in_sandy_maridia.json` |  |
| `rooms/maridia/east_sand_hall_in_sandy_maridia.json` |  |
| `rooms/maridia/east_sand_hole_in_sandy_maridia.json` |  |
| `rooms/maridia/east_tunnel_in_sandless_maridia.json` |  |
| `rooms/maridia/energy_refill_room_door_on_left_in_blue_refill.json` |  |
| `rooms/maridia/fish_tank_in_sandless_maridia.json` |  |
| `rooms/maridia/glass_tunnel_default_in_sandless_maridia.json` |  |
| `rooms/maridia/glass_tunnel_when_maridia_noob_tube_broken_in_sandless_maridia.json` |  |
| `rooms/maridia/halfie_climb_in_sandy_maridia.json` |  |
| `rooms/maridia/kassiuz_in_sandless_maridia.json` |  |
| `rooms/maridia/main_street_in_sandless_maridia.json` |  |
| `rooms/maridia/mama_turtle_in_sandless_maridia.json` |  |
| `rooms/maridia/map_station_room_door_on_left_in_map_statues.json` |  |
| `rooms/maridia/maridia_elevator_in_sandless_maridia.json` |  |
| `rooms/maridia/maridia_missile_refill_in_blue_refill.json` |  |
| `rooms/maridia/mt_everest_in_sandless_maridia.json` |  |
| `rooms/maridia/northwest_maridia_bug_in_sandless_maridia.json` |  |
| `rooms/maridia/oasis_in_sandy_maridia.json` |  |
| `rooms/maridia/pants_in_sandy_maridia.json` |  |
| `rooms/maridia/plasma_beach_quicksand_in_sandy_maridia.json` |  |
| `rooms/maridia/plasma_in_sandless_maridia.json` |  |
| `rooms/maridia/plasma_spark_in_sandless_maridia.json` |  |
| `rooms/maridia/plasma_tutorial_in_sandless_maridia.json` |  |
| `rooms/maridia/pseudo_plasma_spark_in_sandless_maridia.json` |  |
| `rooms/maridia/red_fish_in_sandless_maridia.json` |  |
| `rooms/maridia/save_room_door_on_left_in_save_station.json` |  |
| `rooms/maridia/save_room_door_on_right_in_save_station.json` |  |
| `rooms/maridia/save_room_doors_on_both_sides_in_save_station.json` |  |
| `rooms/maridia/shaktool_default_in_sandy_maridia.json` |  |
| `rooms/maridia/shaktool_when_shaktool_cleared_path_in_sandy_maridia.json` |  |
| `rooms/maridia/space_jump_in_sandy_maridia.json` |  |
| `rooms/maridia/springball_in_sandy_maridia.json` |  |
| `rooms/maridia/the_precious_in_sandy_maridia.json` |  |
| `rooms/maridia/thread_the_needle_in_sandless_maridia.json` |  |
| `rooms/maridia/toilet_in_sandy_maridia.json` |  |
| `rooms/maridia/watering_hole_in_sandless_maridia.json` |  |
| `rooms/maridia/west_aqueduct_quicksand_in_sandy_maridia.json` |  |
| `rooms/maridia/west_cactus_alley_in_sandy_maridia.json` |  |
| `rooms/maridia/west_sand_hall_in_sandy_maridia.json` |  |
| `rooms/maridia/west_sand_hall_tunnel_in_sandless_maridia.json` |  |
| `rooms/maridia/west_sand_hole_in_sandy_maridia.json` |  |
| `rooms/maridia/west_tunnel_in_sandless_maridia.json` |  |
| `rooms/norfair/acid_snakes_tunnel_in_heated_norfair.json` |  |
| `rooms/norfair/acid_statue_in_heated_norfair.json` |  |
| `rooms/norfair/amphitheatre_in_heated_norfair.json` |  |
| `rooms/norfair/bat_cave_in_heated_norfair.json` |  |
| `rooms/norfair/bubble_mountain_in_heated_norfair.json` |  |
| `rooms/norfair/business_center_in_unheated_norfair.json` |  |
| `rooms/norfair/cathedral_entrance_in_heated_norfair.json` |  |
| `rooms/norfair/cathedral_in_heated_norfair.json` |  |
| `rooms/norfair/crocomire_escape_in_heated_norfair.json` |  |
| `rooms/norfair/crocomire_in_crocomire.json` | bridge_collapsed, bridge_crumbles_1, bridge_crumbles_2, spike_wall_open |
| `rooms/norfair/crocomire_speedway_in_heated_norfair.json` |  |
| `rooms/norfair/crumble_shaft_in_heated_norfair.json` |  |
| `rooms/norfair/double_chamber_in_heated_norfair.json` |  |
| `rooms/norfair/energy_refill_room_door_on_left_in_blue_refill.json` |  |
| `rooms/norfair/fast_pillars_setup_in_heated_norfair.json` |  |
| `rooms/norfair/fast_ripper_in_heated_norfair.json` |  |
| `rooms/norfair/frog_speedway_in_unheated_norfair.json` |  |
| `rooms/norfair/golden_torizo_in_heated_norfair.json` |  |
| `rooms/norfair/grapple_beam_in_unheated_norfair.json` |  |
| `rooms/norfair/grapple_tutorial1_in_unheated_norfair.json` |  |
| `rooms/norfair/grapple_tutorial2_in_unheated_norfair.json` |  |
| `rooms/norfair/grapple_tutorial3_in_unheated_norfair.json` |  |
| `rooms/norfair/green_bubbles_missiles_in_heated_norfair.json` |  |
| `rooms/norfair/hi_jump_boots_in_unheated_norfair.json` |  |
| `rooms/norfair/hi_jump_energy_tank_in_unheated_norfair.json` |  |
| `rooms/norfair/ice_beam_acid_in_heated_norfair.json` |  |
| `rooms/norfair/ice_beam_gate_in_unheated_norfair.json` |  |
| `rooms/norfair/ice_beam_in_unheated_norfair.json` |  |
| `rooms/norfair/ice_beam_snake_in_heated_norfair.json` |  |
| `rooms/norfair/ice_beam_tutorial_in_heated_norfair.json` |  |
| `rooms/norfair/kronic_boost_in_heated_norfair.json` |  |
| `rooms/norfair/lava_dive_in_heated_norfair.json` |  |
| `rooms/norfair/lower_norfair_elevator_in_heated_norfair.json` |  |
| `rooms/norfair/lower_norfair_escape_power_bombs_in_heated_norfair.json` |  |
| `rooms/norfair/lower_norfair_farming_in_heated_norfair.json` |  |
| `rooms/norfair/lower_norfair_fireflea_in_unheated_norfair.json` |  |
| `rooms/norfair/lower_norfair_spring_ball_maze_in_heated_norfair.json` |  |
| `rooms/norfair/magdollite_tunnel_in_heated_norfair.json` |  |
| `rooms/norfair/main_hall_in_heated_norfair.json` |  |
| `rooms/norfair/map_station_room_door_on_right_in_map_statues.json` |  |
| `rooms/norfair/metal_pirates_in_heated_norfair.json` |  |
| `rooms/norfair/mickey_mouse_in_heated_norfair.json` |  |
| `rooms/norfair/norfair_reserve_tank_in_heated_norfair.json` |  |
| `rooms/norfair/pillar_in_heated_norfair.json` |  |
| `rooms/norfair/plowerhouse_in_heated_norfair.json` |  |
| `rooms/norfair/post_crocomire_farming_in_heated_norfair.json` |  |
| `rooms/norfair/post_crocomire_jump_in_unheated_norfair.json` |  |
| `rooms/norfair/post_crocomire_missile_in_unheated_norfair.json` |  |
| `rooms/norfair/post_crocomire_power_bombs_in_heated_norfair.json` |  |
| `rooms/norfair/post_crocomire_shaft_in_unheated_norfair.json` |  |
| `rooms/norfair/purple_farming_in_heated_norfair.json` |  |
| `rooms/norfair/purple_shaft_in_heated_norfair.json` |  |
| `rooms/norfair/red_kihunter_shaft_in_heated_norfair.json` |  |
| `rooms/norfair/red_pirate_shaft_in_heated_norfair.json` |  |
| `rooms/norfair/ridley_energy_tank_in_heated_norfair.json` |  |
| `rooms/norfair/ridley_in_heated_norfair.json` |  |
| `rooms/norfair/rising_tide_in_heated_norfair.json` |  |
| `rooms/norfair/save_room_door_on_left_in_save_station.json` |  |
| `rooms/norfair/save_room_door_on_right_in_save_station.json` |  |
| `rooms/norfair/save_room_doors_on_both_sides_in_save_station.json` |  |
| `rooms/norfair/screw_attack_in_heated_norfair.json` |  |
| `rooms/norfair/single_chamber_in_heated_norfair.json` |  |
| `rooms/norfair/speed_booster_hall_in_heated_norfair.json` |  |
| `rooms/norfair/speed_booster_in_heated_norfair.json` |  |
| `rooms/norfair/spiky_acid_snakes_tunnel_in_heated_norfair.json` |  |
| `rooms/norfair/spiky_platforms_tunnel_in_heated_norfair.json` |  |
| `rooms/norfair/three_musketeers_in_heated_norfair.json` |  |
| `rooms/norfair/upper_norfair_farming_in_heated_norfair.json` |  |
| `rooms/norfair/upper_norfair_refill_in_blue_refill.json` |  |
| `rooms/norfair/volcano_in_heated_norfair.json` |  |
| `rooms/norfair/wasteland_in_heated_norfair.json` |  |
| `rooms/norfair/wave_beam_in_heated_norfair.json` |  |
| `rooms/norfair/worst_room_in_the_game_in_heated_norfair.json` |  |
| `rooms/tourian/big_boy_in_tourian.json` |  |
| `rooms/tourian/blue_hopper_in_tourian.json` |  |
| `rooms/tourian/dust_torizo_in_tourian.json` |  |
| `rooms/tourian/metroids1_in_tourian.json` |  |
| `rooms/tourian/metroids2_in_tourian.json` |  |
| `rooms/tourian/metroids3_in_tourian.json` |  |
| `rooms/tourian/metroids4_in_tourian.json` |  |
| `rooms/tourian/mother_brain_in_mother_brain.json` | glass_cleared, glass_rim_gone |
| `rooms/tourian/rinka_shaft_in_tourian.json` |  |
| `rooms/tourian/save_room_door_on_left_in_save_station.json` |  |
| `rooms/tourian/save_room_door_on_right_in_save_station.json` |  |
| `rooms/tourian/seaweed_in_tourian.json` |  |
| `rooms/tourian/tourian_escape1_in_tourian.json` |  |
| `rooms/tourian/tourian_escape2_in_tourian.json` |  |
| `rooms/tourian/tourian_escape3_in_tourian.json` |  |
| `rooms/tourian/tourian_escape4_in_tourian.json` |  |
| `rooms/tourian/tourian_eye_door_in_tourian.json` |  |
| `rooms/tourian/tourian_first_in_tourian.json` |  |
| `rooms/tourian/tourian_recharge_in_yellow_refill.json` |  |
| `rooms/wrecked_ship/assembly_line_in_wrecked_ship_power_off.json` |  |
| `rooms/wrecked_ship/assembly_line_in_wrecked_ship_power_on.json` |  |
| `rooms/wrecked_ship/attic_default_in_wrecked_ship_power_off.json` |  |
| `rooms/wrecked_ship/attic_when_area_boss_dead_in_wrecked_ship_power_on.json` |  |
| `rooms/wrecked_ship/basement_in_wrecked_ship_power_off.json` |  |
| `rooms/wrecked_ship/basement_in_wrecked_ship_power_on.json` |  |
| `rooms/wrecked_ship/bowling_alley_in_wrecked_ship_power_off.json` |  |
| `rooms/wrecked_ship/bowling_alley_in_wrecked_ship_power_on.json` |  |
| `rooms/wrecked_ship/electric_death_in_wrecked_ship_power_off.json` |  |
| `rooms/wrecked_ship/electric_death_in_wrecked_ship_power_on.json` |  |
| `rooms/wrecked_ship/gravity_suit_in_wrecked_ship_power_off.json` |  |
| `rooms/wrecked_ship/gravity_suit_in_wrecked_ship_power_on.json` |  |
| `rooms/wrecked_ship/map_station_room_door_on_right_in_map_statues.json` |  |
| `rooms/wrecked_ship/map_station_room_door_on_right_in_wrecked_ship_map_power_off.json` |  |
| `rooms/wrecked_ship/phantoon_in_wrecked_ship_power_off.json` |  |
| `rooms/wrecked_ship/phantoon_in_wrecked_ship_power_on.json` |  |
| `rooms/wrecked_ship/save_room_door_on_left_in_map_statues.json` |  |
| `rooms/wrecked_ship/save_room_door_on_left_in_wrecked_ship_map_power_off.json` |  |
| `rooms/wrecked_ship/spiky_death_in_wrecked_ship_power_off.json` |  |
| `rooms/wrecked_ship/spiky_death_in_wrecked_ship_power_on.json` |  |
| `rooms/wrecked_ship/sponge_bath_in_wrecked_ship_power_off.json` |  |
| `rooms/wrecked_ship/sponge_bath_in_wrecked_ship_power_on.json` |  |
| `rooms/wrecked_ship/wrecked_ship_east_super_default_in_wrecked_ship_power_off.json` |  |
| `rooms/wrecked_ship/wrecked_ship_east_super_when_area_boss_dead_in_wrecked_ship_power_on.json` |  |
| `rooms/wrecked_ship/wrecked_ship_energy_tank_in_wrecked_ship_power_off.json` |  |
| `rooms/wrecked_ship/wrecked_ship_energy_tank_in_wrecked_ship_power_on.json` |  |
| `rooms/wrecked_ship/wrecked_ship_entrance_in_wrecked_ship_power_off.json` |  |
| `rooms/wrecked_ship/wrecked_ship_entrance_in_wrecked_ship_power_on.json` |  |
| `rooms/wrecked_ship/wrecked_ship_main_shaft_default_in_wrecked_ship_power_off.json` |  |
| `rooms/wrecked_ship/wrecked_ship_main_shaft_when_area_boss_dead_in_wrecked_ship_power_on.json` |  |
| `rooms/wrecked_ship/wrecked_ship_west_super_in_wrecked_ship_power_off.json` |  |
| `rooms/wrecked_ship/wrecked_ship_west_super_in_wrecked_ship_power_on.json` |  |

## Other files named

| File | Named in |
| --- | --- |
| `audio/music_sets.json` | `game/compatibility/CartridgeSounds.cs` |
| `backdrops/scrolling_sky_bands.json` | `game/rooms/crateria/east_ocean.tscn`, `game/rooms/crateria/landing_site.tscn`, `game/rooms/crateria/pre_bowling.tscn` and 1 more |
| `backdrops/{graphics}/brinstar_blue_grid_blocks.png` | `game/rooms/brinstar/alpha_power_bomb.tscn`, `game/rooms/brinstar/spazer.tscn` |
| `backdrops/{graphics}/brinstar_dark_pattern.png` | `game/rooms/brinstar/big_pink.tscn` |
| `backdrops/{graphics}/brinstar_horizontal_glow_patches.png` | `game/rooms/brinstar/etecoon_energy_tank.tscn`, `game/rooms/brinstar/etecoon_super.tscn`, `game/rooms/brinstar/green_brinstar_beetoms.tscn` and 2 more |
| `backdrops/{graphics}/brinstar_large_horizontal_pattern.png` | `game/rooms/brinstar/brinstar_reserve_tank.tscn` |
| `backdrops/{graphics}/brinstar_mechanical_room.png` | `game/rooms/brinstar/warehouse_entrance.tscn` |
| `backdrops/{graphics}/brinstar_mechanical_room_spike_floor.png` | `game/rooms/brinstar/xray_scope.tscn` |
| `backdrops/{graphics}/brinstar_medium_horizontal_pattern.png` | `game/rooms/brinstar/brinstar_pre_map.tscn` |
| `backdrops/{graphics}/brinstar_narrow_vertical_tower_brick.png` | `game/rooms/brinstar/beta_power_bomb.tscn`, `game/rooms/brinstar/skree_boost.tscn` |
| `backdrops/{graphics}/brinstar_narrow_vertical_tower_brick_vines.png` | `game/rooms/brinstar/mini_kraid.tscn` |
| `backdrops/{graphics}/brinstar_small_pattern.png` | `game/rooms/brinstar/construction_zone.tscn`, `game/rooms/brinstar/first_missile.tscn`, `game/rooms/brinstar/spore_spawn_super.tscn` |
| `backdrops/{graphics}/brinstar_spheres_pattern.png` | `game/rooms/brinstar/green_hill_zone.tscn` |
| `backdrops/{graphics}/brinstar_thin_horizontal_pattern.png` | `game/rooms/brinstar/early_supers.tscn`, `game/rooms/brinstar/pink_brinstar_hoppers.tscn`, `game/rooms/brinstar/pink_brinstar_power_bombs.tscn` and 1 more |
| `backdrops/{graphics}/brinstar_varied_small_pattern.png` | `game/rooms/brinstar/billy_mays.tscn`, `game/rooms/brinstar/blue_brinstar_boulders.tscn`, `game/rooms/brinstar/blue_brinstar_energy_tank.tscn` and 1 more |
| `backdrops/{graphics}/brinstar_vertical_brick.png` | `game/rooms/brinstar/below_spazer.tscn` |
| `backdrops/{graphics}/brinstar_vertical_glow_patches.png` | `game/rooms/brinstar/green_brinstar_main_shaft.tscn` |
| `backdrops/{graphics}/brinstar_vertical_tower.png` | `game/rooms/brinstar/hellway.tscn` |
| `backdrops/{graphics}/brinstar_wide_vertical_tower_brick.png` | `game/rooms/brinstar/red_tower.tscn` |
| `backdrops/{graphics}/ceres_baby_horizontal.png` | `game/rooms/ceres/ceres_ridley.tscn` |
| `backdrops/{graphics}/ceres_baby_round.png` | `game/rooms/ceres/ceres_ridley.tscn` |
| `backdrops/{graphics}/ceres_baby_vertical.png` | `game/rooms/ceres/ceres_ridley.tscn` |
| `backdrops/{graphics}/ceres_capsule_hall.png` | `game/rooms/ceres/ceres_ridley_hallway.tscn`, `game/rooms/ceres/falling_tile.tscn`, `game/rooms/ceres/magnet_stairs.tscn` |
| `backdrops/{graphics}/ceres_elevator_dark.png` | `game/rooms/ceres/ceres_elevator.tscn` |
| `backdrops/{graphics}/ceres_elevator_light.png` | `game/rooms/ceres/ceres_elevator.tscn` |
| `backdrops/{graphics}/ceres_elevator_shaft.png` | `game/rooms/ceres/ceres_elevator.tscn` |
| `backdrops/{graphics}/ceres_ridley_floor.png` | `game/rooms/ceres/ceres_ridley.tscn` |
| `backdrops/{graphics}/ceres_wings_lowered.png` | `game/rooms/ceres/ceres_ridley.tscn` |
| `backdrops/{graphics}/ceres_wings_raised.png` | `game/rooms/ceres/ceres_ridley.tscn` |
| `backdrops/{graphics}/crateria_brick_room.png` | `game/rooms/crateria/final_missile_bombway.tscn`, `game/rooms/crateria/green_pirates_shaft.tscn` |
| `backdrops/{graphics}/crateria_brick_room_wall_art.png` | `game/rooms/crateria/pre_map_flyway.tscn` |
| `backdrops/{graphics}/crateria_brick_room_wall_art_dark.png` | `game/rooms/crateria/flyway.tscn` |
| `backdrops/{graphics}/crateria_elevator.png` | `game/rooms/crateria/forgotten_highway_elevator.tscn`, `game/rooms/crateria/red_brinstar_elevator.tscn` |
| `backdrops/{graphics}/crateria_horizontal_pattern_rocks.png` | `game/rooms/crateria/bowling_alley_path.tscn`, `game/rooms/crateria/gauntlet_energy_tank.tscn`, `game/rooms/crateria/gauntlet_entrance.tscn` |
| `backdrops/{graphics}/crateria_mechanical.png` | `game/rooms/crateria/bomb_torizo.tscn`, `game/rooms/crateria/climb.tscn`, `game/rooms/crateria/crateria_super.tscn` and 2 more |
| `backdrops/{graphics}/crateria_purple_rocks.png` | `game/rooms/crateria/lower_mushrooms.tscn`, `game/rooms/crateria/terminator.tscn` |
| `backdrops/{graphics}/crateria_rocks.png` | `game/rooms/crateria/crab_maze.tscn`, `game/rooms/crateria/crateria_kihunter.tscn`, `game/rooms/crateria/crateria_power_bombs.tscn` and 2 more |
| `backdrops/{graphics}/crateria_vertical_pattern_rocks.png` | `game/rooms/crateria/forgotten_highway_elbow.tscn`, `game/rooms/crateria/forgotten_highway_kagos.tscn` |
| `backdrops/{graphics}/level_aqueduct.png` | `game/rooms/maridia/aqueduct.tscn` |
| `backdrops/{graphics}/level_bat_cave.png` | `game/rooms/norfair/bat_cave.tscn` |
| `backdrops/{graphics}/level_below_botwoon_energy_tank.png` | `game/rooms/maridia/below_botwoon_energy_tank.tscn` |
| `backdrops/{graphics}/level_botwoon.png` | `game/rooms/maridia/botwoon.tscn` |
| `backdrops/{graphics}/level_botwoon_energy_tank.png` | `game/rooms/maridia/botwoon_energy_tank.tscn` |
| `backdrops/{graphics}/level_botwoon_hallway.png` | `game/rooms/maridia/botwoon_hallway.tscn` |
| `backdrops/{graphics}/level_botwoon_quicksand.png` | `game/rooms/maridia/botwoon_quicksand.tscn` |
| `backdrops/{graphics}/level_bowling_alley.png` | `game/rooms/wrecked_ship/bowling_alley.tscn` |
| `backdrops/{graphics}/level_bug_sand_hole.png` | `game/rooms/maridia/bug_sand_hole.tscn` |
| `backdrops/{graphics}/level_butterfly.png` | `game/rooms/maridia/butterfly.tscn` |
| `backdrops/{graphics}/level_caterpillar.png` | `game/rooms/brinstar/caterpillar.tscn` |
| `backdrops/{graphics}/level_cathedral.png` | `game/rooms/norfair/cathedral.tscn` |
| `backdrops/{graphics}/level_cathedral_entrance.png` | `game/rooms/norfair/cathedral_entrance.tscn` |
| `backdrops/{graphics}/level_ceres_ridley.png` | `game/rooms/ceres/ceres_ridley.tscn` |
| `backdrops/{graphics}/level_colosseum.png` | `game/rooms/maridia/colosseum.tscn` |
| `backdrops/{graphics}/level_crab_hole.png` | `game/rooms/maridia/crab_hole.tscn` |
| `backdrops/{graphics}/level_crab_shaft.png` | `game/rooms/maridia/crab_shaft.tscn` |
| `backdrops/{graphics}/level_crateria_tube.png` | `game/rooms/crateria/crateria_tube.tscn` |
| `backdrops/{graphics}/level_crocomire_escape.png` | `game/rooms/norfair/crocomire_escape.tscn` |
| `backdrops/{graphics}/level_dachora.png` | `game/rooms/brinstar/dachora.tscn` |
| `backdrops/{graphics}/level_dead_scientist.png` | `game/rooms/ceres/dead_scientist.tscn` |
| `backdrops/{graphics}/level_double_chamber.png` | `game/rooms/norfair/double_chamber.tscn` |
| `backdrops/{graphics}/level_east_aqueduct_quicksand.png` | `game/rooms/maridia/east_aqueduct_quicksand.tscn` |
| `backdrops/{graphics}/level_east_cactus_alley.png` | `game/rooms/maridia/east_cactus_alley.tscn` |
| `backdrops/{graphics}/level_east_pants.png` | `game/rooms/maridia/east_pants.tscn` |
| `backdrops/{graphics}/level_east_sand_hole.png` | `game/rooms/maridia/east_sand_hole.tscn` |
| `backdrops/{graphics}/level_east_tunnel.png` | `game/rooms/maridia/east_tunnel.tscn` |
| `backdrops/{graphics}/level_elevator_to_blue_brinstar.png` | `game/rooms/crateria/elevator_to_blue_brinstar.tscn` |
| `backdrops/{graphics}/level_energy_refill_room_door_on_left.png` | `game/rooms/maridia/maridia_energy_refill.tscn`, `game/rooms/norfair/golden_torizo_energy_refill.tscn` |
| `backdrops/{graphics}/level_energy_refill_room_door_on_right.png` | `game/rooms/brinstar/dachora_energy_refill.tscn`, `game/rooms/brinstar/red_tower_refill.tscn` |
| `backdrops/{graphics}/level_fish_tank.png` | `game/rooms/maridia/fish_tank.tscn` |
| `backdrops/{graphics}/level_glass_tunnel_default.png` | `game/rooms/maridia/glass_tunnel.tscn` |
| `backdrops/{graphics}/level_glass_tunnel_when_maridia_noob_tube_broken.png` | `game/rooms/maridia/glass_tunnel.tscn` |
| `backdrops/{graphics}/level_golden_torizo.png` | `game/rooms/norfair/golden_torizo.tscn` |
| `backdrops/{graphics}/level_green_brinstar_elevator.png` | `game/rooms/crateria/green_brinstar_elevator.tscn` |
| `backdrops/{graphics}/level_green_brinstar_firefleas.png` | `game/rooms/brinstar/green_brinstar_firefleas.tscn` |
| `backdrops/{graphics}/level_green_brinstar_missile_refill.png` | `game/rooms/brinstar/green_brinstar_missile_refill.tscn` |
| `backdrops/{graphics}/level_halfie_climb.png` | `game/rooms/maridia/halfie_climb.tscn` |
| `backdrops/{graphics}/level_hi_jump_energy_tank.png` | `game/rooms/norfair/hi_jump_energy_tank.tscn` |
| `backdrops/{graphics}/level_hopper_energy_tank.png` | `game/rooms/brinstar/hopper_energy_tank.tscn` |
| `backdrops/{graphics}/level_kassiuz.png` | `game/rooms/maridia/kassiuz.tscn` |
| `backdrops/{graphics}/level_kraid_eye_door.png` | `game/rooms/brinstar/kraid_eye_door.tscn` |
| `backdrops/{graphics}/level_kraid_refill.png` | `game/rooms/brinstar/kraid_refill.tscn` |
| `backdrops/{graphics}/level_kronic_boost.png` | `game/rooms/norfair/kronic_boost.tscn` |
| `backdrops/{graphics}/level_lower_norfair_fireflea.png` | `game/rooms/norfair/lower_norfair_fireflea.tscn` |
| `backdrops/{graphics}/level_main_street.png` | `game/rooms/maridia/main_street.tscn` |
| `backdrops/{graphics}/level_mama_turtle.png` | `game/rooms/maridia/mama_turtle.tscn` |
| `backdrops/{graphics}/level_map_station_room_door_on_left.png` | `game/rooms/crateria/crateria_map.tscn`, `game/rooms/maridia/maridia_map.tscn` |
| `backdrops/{graphics}/level_map_station_room_door_on_right.png` | `game/rooms/brinstar/green_brinstar_map.tscn`, `game/rooms/norfair/norfair_map.tscn`, `game/rooms/wrecked_ship/wrecked_ship_map.tscn` |
| `backdrops/{graphics}/level_maridia_elevator.png` | `game/rooms/maridia/maridia_elevator.tscn` |
| `backdrops/{graphics}/level_maridia_missile_refill.png` | `game/rooms/maridia/maridia_missile_refill.tscn` |
| `backdrops/{graphics}/level_mt_everest.png` | `game/rooms/maridia/mt_everest.tscn` |
| `backdrops/{graphics}/level_northwest_maridia_bug.png` | `game/rooms/maridia/northwest_maridia_bug.tscn` |
| `backdrops/{graphics}/level_oasis.png` | `game/rooms/maridia/oasis.tscn` |
| `backdrops/{graphics}/level_pants.png` | `game/rooms/maridia/pants.tscn` |
| `backdrops/{graphics}/level_plasma.png` | `game/rooms/maridia/plasma.tscn` |
| `backdrops/{graphics}/level_plasma_beach_quicksand.png` | `game/rooms/maridia/plasma_beach_quicksand.tscn` |
| `backdrops/{graphics}/level_plasma_spark.png` | `game/rooms/maridia/plasma_spark.tscn` |
| `backdrops/{graphics}/level_plasma_tutorial.png` | `game/rooms/maridia/plasma_tutorial.tscn` |
| `backdrops/{graphics}/level_post_crocomire_farming.png` | `game/rooms/norfair/post_crocomire_farming.tscn` |
| `backdrops/{graphics}/level_post_crocomire_jump.png` | `game/rooms/norfair/post_crocomire_jump.tscn` |
| `backdrops/{graphics}/level_pseudo_plasma_spark.png` | `game/rooms/maridia/pseudo_plasma_spark.tscn` |
| `backdrops/{graphics}/level_red_brinstar_firefleas.png` | `game/rooms/brinstar/red_brinstar_firefleas.tscn` |
| `backdrops/{graphics}/level_red_fish.png` | `game/rooms/maridia/red_fish.tscn` |
| `backdrops/{graphics}/level_save_room_door_on_left.png` | `game/rooms/brinstar/red_brinstar_save.tscn`, `game/rooms/brinstar/warehouse_save.tscn`, `game/rooms/maridia/forgotten_highway_save.tscn` and 6 more |
| `backdrops/{graphics}/level_save_room_door_on_right.png` | `game/rooms/brinstar/big_pink_save_room.tscn`, `game/rooms/brinstar/etecoon_save.tscn`, `game/rooms/brinstar/green_brinstar_save.tscn` and 5 more |
| `backdrops/{graphics}/level_save_room_doors_on_both_sides.png` | `game/rooms/maridia/draygon_save.tscn`, `game/rooms/norfair/frog_save.tscn` |
| `backdrops/{graphics}/level_shaktool_default.png` | `game/rooms/maridia/shaktool.tscn` |
| `backdrops/{graphics}/level_shaktool_when_shaktool_cleared_path.png` | `game/rooms/maridia/shaktool.tscn` |
| `backdrops/{graphics}/level_single_chamber.png` | `game/rooms/norfair/single_chamber.tscn` |
| `backdrops/{graphics}/level_space_jump.png` | `game/rooms/maridia/space_jump.tscn` |
| `backdrops/{graphics}/level_spore_spawn.png` | `game/rooms/brinstar/spore_spawn.tscn` |
| `backdrops/{graphics}/level_spore_spawn_farming.png` | `game/rooms/brinstar/spore_spawn_farming.tscn` |
| `backdrops/{graphics}/level_springball.png` | `game/rooms/maridia/springball.tscn` |
| `backdrops/{graphics}/level_the_precious.png` | `game/rooms/maridia/the_precious.tscn` |
| `backdrops/{graphics}/level_thread_the_needle.png` | `game/rooms/maridia/thread_the_needle.tscn` |
| `backdrops/{graphics}/level_toilet.png` | `game/rooms/maridia/toilet.tscn` |
| `backdrops/{graphics}/level_tourian_recharge.png` | `game/rooms/tourian/tourian_recharge.tscn` |
| `backdrops/{graphics}/level_upper_norfair_farming.png` | `game/rooms/norfair/upper_norfair_farming.tscn` |
| `backdrops/{graphics}/level_upper_norfair_refill.png` | `game/rooms/norfair/upper_norfair_refill.tscn` |
| `backdrops/{graphics}/level_varia_suit.png` | `game/rooms/brinstar/varia_suit.tscn` |
| `backdrops/{graphics}/level_volcano.png` | `game/rooms/norfair/volcano.tscn` |
| `backdrops/{graphics}/level_warehouse_energy_tank.png` | `game/rooms/brinstar/warehouse_energy_tank.tscn` |
| `backdrops/{graphics}/level_warehouse_kihunter.png` | `game/rooms/brinstar/warehouse_kihunter.tscn` |
| `backdrops/{graphics}/level_warehouse_zeela.png` | `game/rooms/brinstar/warehouse_zeela.tscn` |
| `backdrops/{graphics}/level_watering_hole.png` | `game/rooms/maridia/watering_hole.tscn` |
| `backdrops/{graphics}/level_west_aqueduct_quicksand.png` | `game/rooms/maridia/west_aqueduct_quicksand.tscn` |
| `backdrops/{graphics}/level_west_cactus_alley.png` | `game/rooms/maridia/west_cactus_alley.tscn` |
| `backdrops/{graphics}/level_west_sand_hall_tunnel.png` | `game/rooms/maridia/west_sand_hall_tunnel.tscn` |
| `backdrops/{graphics}/level_west_sand_hole.png` | `game/rooms/maridia/west_sand_hole.tscn` |
| `backdrops/{graphics}/level_west_tunnel.png` | `game/rooms/maridia/west_tunnel.tscn` |
| `backdrops/{graphics}/level_wrecked_ship_energy_tank.png` | `game/rooms/wrecked_ship/wrecked_ship_energy_tank.tscn` |
| `backdrops/{graphics}/maridia_brown_wall.png` | `game/rooms/maridia/crab_tunnel.tscn` |
| `backdrops/{graphics}/maridia_green_wall.png` | `game/rooms/maridia/east_sand_hall.tscn`, `game/rooms/maridia/west_sand_hall.tscn` |
| `backdrops/{graphics}/norfair_bubbles.png` | `game/rooms/norfair/bubble_mountain.tscn`, `game/rooms/norfair/green_bubbles_missiles.tscn` |
| `backdrops/{graphics}/norfair_cavern_horizontal_ruins.png` | `game/rooms/norfair/worst_room_in_the_game.tscn` |
| `backdrops/{graphics}/norfair_cavern_room_pillars.png` | `game/rooms/norfair/acid_statue.tscn`, `game/rooms/norfair/lower_norfair_escape_power_bombs.tscn`, `game/rooms/norfair/ridley.tscn` and 3 more |
| `backdrops/{graphics}/norfair_cavern_stalactites.png` | `game/rooms/norfair/acid_snakes_tunnel.tscn`, `game/rooms/norfair/grapple_tutorial1.tscn`, `game/rooms/norfair/lower_norfair_elevator.tscn` and 7 more |
| `backdrops/{graphics}/norfair_cavern_vertical.png` | `game/rooms/norfair/business_center.tscn`, `game/rooms/norfair/crumble_shaft.tscn`, `game/rooms/norfair/grapple_beam.tscn` and 3 more |
| `backdrops/{graphics}/norfair_cavern_vertical_ruins.png` | `game/rooms/norfair/fast_ripper.tscn`, `game/rooms/norfair/main_hall.tscn`, `game/rooms/norfair/pillar.tscn` |
| `backdrops/{graphics}/norfair_purple_patches.png` | `game/rooms/norfair/lower_norfair_spring_ball_maze.tscn`, `game/rooms/norfair/mickey_mouse.tscn`, `game/rooms/norfair/norfair_reserve_tank.tscn` and 3 more |
| `backdrops/{graphics}/norfair_room_statues.png` | `game/rooms/norfair/lower_norfair_farming.tscn`, `game/rooms/norfair/metal_pirates.tscn`, `game/rooms/norfair/plowerhouse.tscn` |
| `backdrops/{graphics}/norfair_small_pattern_brown_purple.png` | `game/rooms/norfair/amphitheatre.tscn`, `game/rooms/norfair/crocomire_speedway.tscn`, `game/rooms/norfair/grapple_tutorial3.tscn` and 6 more |
| `backdrops/{graphics}/norfair_small_pattern_brown_purple_bright.png` | `game/rooms/norfair/frog_speedway.tscn`, `game/rooms/norfair/hi_jump_boots.tscn`, `game/rooms/norfair/ice_beam_acid.tscn` and 2 more |
| `backdrops/{graphics}/norfair_vertical_pattern_brick.png` | `game/rooms/norfair/fast_pillars_setup.tscn` |
| `backdrops/{graphics}/scrolling_sky_east_ocean.png` | `game/rooms/crateria/east_ocean.tscn` |
| `backdrops/{graphics}/scrolling_sky_landing_site.png` | `game/rooms/crateria/landing_site.tscn` |
| `backdrops/{graphics}/scrolling_sky_pre_bowling.png` | `game/rooms/crateria/pre_bowling.tscn` |
| `backdrops/{graphics}/scrolling_sky_west_ocean.png` | `game/rooms/crateria/west_ocean.tscn` |
| `backdrops/{graphics}/statues_hallway_grey_tower_brick.png` | `game/rooms/crateria/statues_hallway.tscn` |
| `backdrops/{graphics}/tourian_mechanical_room.png` | `game/rooms/tourian/metroids2.tscn`, `game/rooms/tourian/metroids4.tscn`, `game/rooms/tourian/tourian_first.tscn` |
| `backdrops/{graphics}/tourian_mechanical_room_letterbox.png` | `game/rooms/tourian/metroids1.tscn`, `game/rooms/tourian/metroids3.tscn` |
| `backdrops/{graphics}/tourian_pipe_room.png` | `game/rooms/tourian/tourian_escape3.tscn`, `game/rooms/tourian/tourian_escape4.tscn` |
| `backdrops/{graphics}/tourian_pipe_room_dark.png` | `game/rooms/tourian/mother_brain.tscn` |
| `backdrops/{graphics}/tourian_pipe_room_letterbox.png` | `game/rooms/tourian/tourian_escape1.tscn` |
| `backdrops/{graphics}/tourian_pipe_room_vertical_separation.png` | `game/rooms/tourian/rinka_shaft.tscn`, `game/rooms/tourian/tourian_escape2.tscn` |
| `backdrops/{graphics}/tourian_statues.png` | `game/rooms/crateria/statues.tscn` |
| `backdrops/{graphics}/tourian_vertical_structures.png` | `game/rooms/tourian/big_boy.tscn`, `game/rooms/tourian/blue_hopper.tscn`, `game/rooms/tourian/dust_torizo.tscn` and 2 more |
| `backdrops/{graphics}/wrecked_ship_columns_tubes.png` | `game/rooms/wrecked_ship/basement.tscn`, `game/rooms/wrecked_ship/spiky_death.tscn` |
| `backdrops/{graphics}/wrecked_ship_entrance_hall.png` | `game/rooms/wrecked_ship/wrecked_ship_entrance.tscn` |
| `backdrops/{graphics}/wrecked_ship_experiment_room.png` | `game/rooms/wrecked_ship/attic.tscn` |
| `backdrops/{graphics}/wrecked_ship_panels_vents.png` | `game/rooms/wrecked_ship/gravity_suit.tscn`, `game/rooms/wrecked_ship/sponge_bath.tscn`, `game/rooms/wrecked_ship/wrecked_ship_east_super.tscn` and 1 more |
| `backdrops/{graphics}/wrecked_ship_panels_vents_horizontal.png` | `game/rooms/wrecked_ship/electric_death.tscn`, `game/rooms/wrecked_ship/wrecked_ship_east_super.tscn`, `game/rooms/wrecked_ship/wrecked_ship_main_shaft.tscn` |
| `backdrops/{graphics}/wrecked_ship_panels_vents_vertical.png` | `game/rooms/wrecked_ship/assembly_line.tscn` |
| `cinematics/ceres_destruction.json` | `game/cinematics/CeresDestruction.cs` |
| `cinematics/ending.json` | `game/cinematics/Ending.cs` |
| `cinematics/intro.json` | `game/cinematics/Intro.cs` |
| `cinematics/intro_mother_brain_room_lights.json` | `game/cinematics/intro_mother_brain_room.tscn` |
| `colours/{graphics}/beacon_flashing.json` | `game/rooms/brinstar/caterpillar.tscn`, `game/rooms/brinstar/red_tower.tscn`, `game/rooms/brinstar/warehouse_energy_tank.tscn` and 3 more |
| `colours/{graphics}/brinstar_blue_background_spores.json` | `game/rooms/brinstar/etecoon_energy_tank.tscn`, `game/rooms/brinstar/etecoon_super.tscn`, `game/rooms/brinstar/green_brinstar_main_shaft.tscn` and 3 more |
| `colours/{graphics}/brinstar_red_background_glow.json` | `game/rooms/brinstar/caterpillar.tscn`, `game/rooms/brinstar/hellway.tscn`, `game/rooms/brinstar/red_tower.tscn` |
| `colours/{graphics}/crateria_escape_yellow_lightning.json` | `game/rooms/crateria/climb.tscn`, `game/rooms/crateria/landing_site.tscn` |
| `colours/{graphics}/crateria_lightning.json` | `game/rooms/crateria/climb.tscn`, `game/rooms/crateria/landing_site.tscn` |
| `colours/{graphics}/maridia_background_waterfalls.json` | `game/rooms/maridia/colosseum.tscn`, `game/rooms/maridia/east_cactus_alley.tscn`, `game/rooms/maridia/halfie_climb.tscn` |
| `colours/{graphics}/maridia_sand_falls.json` | `game/rooms/maridia/aqueduct_save.tscn`, `game/rooms/maridia/botwoon.tscn`, `game/rooms/maridia/botwoon_quicksand.tscn` and 3 more |
| `colours/{graphics}/maridia_sand_pits.json` | `game/rooms/maridia/aqueduct.tscn`, `game/rooms/maridia/below_botwoon_energy_tank.tscn`, `game/rooms/maridia/botwoon_energy_tank.tscn` and 11 more |
| `colours/{graphics}/norfair_dark_foreground_heat_glow.json` | `game/rooms/norfair/acid_snakes_tunnel.tscn`, `game/rooms/norfair/acid_statue.tscn`, `game/rooms/norfair/amphitheatre.tscn` and 45 more |
| `colours/{graphics}/norfair_foreground_and_heat_phase.json` | `game/rooms/norfair/acid_snakes_tunnel.tscn`, `game/rooms/norfair/acid_statue.tscn`, `game/rooms/norfair/amphitheatre.tscn` and 45 more |
| `colours/{graphics}/norfair_green_foreground_heat_glow.json` | `game/rooms/norfair/acid_snakes_tunnel.tscn`, `game/rooms/norfair/acid_statue.tscn`, `game/rooms/norfair/amphitheatre.tscn` and 45 more |
| `colours/{graphics}/norfair_purple_foreground_heat_glow.json` | `game/rooms/norfair/acid_snakes_tunnel.tscn`, `game/rooms/norfair/acid_statue.tscn`, `game/rooms/norfair/amphitheatre.tscn` and 45 more |
| `colours/{graphics}/old_tourian_background_panels_flash_yellow.json` | `game/rooms/crateria/climb.tscn` |
| `colours/{graphics}/old_tourian_background_railings_flash_yellow.json` | `game/rooms/crateria/climb.tscn` |
| `colours/{graphics}/old_tourian_red_flashing.json` | `game/rooms/crateria/climb.tscn` |
| `colours/{graphics}/parlor_escape_sky_flicker.json` | `game/rooms/crateria/parlor.tscn` |
| `colours/{graphics}/spore_spawn_blue_background_spores.json` | `game/rooms/brinstar/spore_spawn.tscn` |
| `colours/{graphics}/tourian_background_red_flashing.json` | `game/rooms/tourian/tourian_escape1.tscn`, `game/rooms/tourian/tourian_escape2.tscn`, `game/rooms/tourian/tourian_escape3.tscn` and 1 more |
| `colours/{graphics}/tourian_general_level_red_flashing.json` | `game/rooms/tourian/tourian_escape1.tscn`, `game/rooms/tourian/tourian_escape2.tscn`, `game/rooms/tourian/tourian_escape3.tscn` and 1 more |
| `colours/{graphics}/tourian_glowing_arkanoid_blocks_and_red_orbs.json` | `game/rooms/tourian/big_boy.tscn`, `game/rooms/tourian/blue_hopper.tscn`, `game/rooms/tourian/dust_torizo.tscn` and 9 more |
| `colours/{graphics}/tourian_red_flashing_arkanoid_blocks_and_red_orbs.json` | `game/rooms/tourian/tourian_escape1.tscn`, `game/rooms/tourian/tourian_escape2.tscn`, `game/rooms/tourian/tourian_escape3.tscn` and 1 more |
| `colours/{graphics}/upper_crateria_red_flashing.json` | `game/rooms/crateria/climb.tscn`, `game/rooms/crateria/landing_site.tscn`, `game/rooms/crateria/parlor.tscn` |
| `colours/{graphics}/wrecked_ship_green_lights.json` | `game/rooms/wrecked_ship/assembly_line.tscn`, `game/rooms/wrecked_ship/attic.tscn`, `game/rooms/wrecked_ship/basement.tscn` and 10 more |
| `effects/big_dust_cloud.json` | `game/rooms/crateria/bomb_torizo.tscn`, `game/rooms/crateria/climb.tscn`, `game/rooms/crateria/flyway.tscn` and 6 more |
| `effects/ceres_debris_dark.json` | `game/rooms/ceres/falling_tile.tscn` |
| `effects/ceres_debris_light.json` | `game/rooms/ceres/falling_tile.tscn` |
| `effects/ceres_haze.json` | `game/rooms/ceres/ceres_elevator.tscn`, `game/rooms/ceres/ceres_ridley.tscn`, `game/rooms/ceres/ceres_ridley_hallway.tscn` and 3 more |
| `effects/ceres_haze_escape.json` | `game/rooms/ceres/ceres_elevator.tscn`, `game/rooms/ceres/ceres_ridley.tscn`, `game/rooms/ceres/ceres_ridley_hallway.tscn` and 3 more |
| `effects/fog.json` | `game/rooms/crateria/climb.tscn`, `game/rooms/crateria/pit.tscn`, `game/rooms/crateria/statues_hallway.tscn` |
| `effects/rain.json` | `game/rooms/crateria/landing_site.tscn` |
| `effects/short_big_dust_cloud.json` | `game/rooms/crateria/bomb_torizo.tscn`, `game/rooms/crateria/climb.tscn`, `game/rooms/crateria/flyway.tscn` and 6 more |
| `effects/small_explosion.json` | `game/rooms/crateria/bomb_torizo.tscn`, `game/rooms/crateria/climb.tscn`, `game/rooms/crateria/flyway.tscn` and 6 more |
| `effects/smoke.json` | `game/rooms/crateria/bomb_torizo.tscn`, `game/rooms/crateria/climb.tscn`, `game/rooms/crateria/flyway.tscn` and 6 more |
| `effects/smoke_puff.json` | `game/rooms/crateria/bomb_torizo.tscn`, `game/rooms/crateria/climb.tscn`, `game/rooms/crateria/flyway.tscn` and 6 more |
| `effects/spores.json` | `game/rooms/brinstar/big_pink.tscn`, `game/rooms/brinstar/dachora.tscn`, `game/rooms/brinstar/spore_spawn_farming.tscn` and 1 more |
| `items/bombs.json` | `game/rooms/crateria/bomb_torizo.tscn` |
| `items/charge_beam.json` | `game/rooms/brinstar/big_pink.tscn` |
| `items/energy_tank.json` | `game/rooms/brinstar/blue_brinstar_energy_tank.tscn`, `game/rooms/brinstar/etecoon_energy_tank.tscn`, `game/rooms/brinstar/hopper_energy_tank.tscn` and 11 more |
| `items/grapple_beam.json` | `game/rooms/norfair/grapple_beam.tscn` |
| `items/gravity_suit.json` | `game/rooms/wrecked_ship/gravity_suit.tscn` |
| `items/hi_jump_boots.json` | `game/rooms/norfair/hi_jump_boots.tscn` |
| `items/ice_beam.json` | `game/rooms/norfair/ice_beam.tscn` |
| `items/missile_tank.json` | `game/rooms/brinstar/alpha_power_bomb.tscn`, `game/rooms/brinstar/big_pink.tscn`, `game/rooms/brinstar/billy_mays.tscn` and 37 more |
| `items/morph_ball.json` | `game/rooms/brinstar/morph_ball.tscn` |
| `items/plasma_beam.json` | `game/rooms/maridia/plasma.tscn` |
| `items/power_bomb_tank.json` | `game/rooms/brinstar/alpha_power_bomb.tscn`, `game/rooms/brinstar/beta_power_bomb.tscn`, `game/rooms/brinstar/green_brinstar_main_shaft.tscn` and 7 more |
| `items/reserve_tank.json` | `game/rooms/brinstar/brinstar_reserve_tank.tscn`, `game/rooms/maridia/west_sand_hole.tscn`, `game/rooms/norfair/norfair_reserve_tank.tscn` and 1 more |
| `items/screw_attack.json` | `game/rooms/norfair/screw_attack.tscn` |
| `items/space_jump.json` | `game/rooms/maridia/space_jump.tscn` |
| `items/spazer_beam.json` | `game/rooms/brinstar/spazer.tscn` |
| `items/speed_booster.json` | `game/rooms/norfair/speed_booster.tscn` |
| `items/spring_ball.json` | `game/rooms/maridia/springball.tscn` |
| `items/super_missile_tank.json` | `game/rooms/brinstar/early_supers.tscn`, `game/rooms/brinstar/etecoon_super.tscn`, `game/rooms/brinstar/spore_spawn_super.tscn` and 7 more |
| `items/varia_suit.json` | `game/rooms/brinstar/varia_suit.tscn` |
| `items/wave_beam.json` | `game/rooms/norfair/wave_beam.tscn` |
| `items/xray_scope.json` | `game/rooms/brinstar/xray_scope.tscn` |
| `liquids/acid.json` | `game/rooms/crateria/bomb_torizo.tscn`, `game/rooms/crateria/climb.tscn`, `game/rooms/crateria/gauntlet_energy_tank.tscn` and 18 more |
| `liquids/acid_lava.json` | `game/rooms/norfair/crocomire.tscn`, `game/rooms/tourian/metroids1.tscn`, `game/rooms/tourian/metroids3.tscn` and 2 more |
| `liquids/acid_water.json` | `game/rooms/crateria/crateria_power_bombs.tscn`, `game/rooms/crateria/crateria_super.tscn` |
| `liquids/lava.json` | `game/rooms/crateria/climb.tscn`, `game/rooms/norfair/acid_snakes_tunnel.tscn`, `game/rooms/norfair/bat_cave.tscn` and 18 more |
| `liquids/water.json` | `game/rooms/brinstar/below_spazer.tscn`, `game/rooms/brinstar/blue_brinstar_boulders.tscn`, `game/rooms/brinstar/skree_boost.tscn` and 13 more |
| `liquids/water_green.json` | `game/rooms/maridia/east_pants.tscn`, `game/rooms/maridia/east_sand_hall.tscn`, `game/rooms/maridia/oasis.tscn` and 5 more |
| `liquids/water_indigo.json` | `game/rooms/maridia/aqueduct_save.tscn`, `game/rooms/maridia/bug_sand_hole.tscn`, `game/rooms/maridia/butterfly.tscn` and 7 more |
| `liquids/water_purple.json` | `game/rooms/maridia/aqueduct.tscn`, `game/rooms/maridia/below_botwoon_energy_tank.tscn`, `game/rooms/maridia/botwoon.tscn` and 11 more |
| `liquids/water_teal.json` | `game/rooms/maridia/crab_hole.tscn`, `game/rooms/maridia/crab_shaft.tscn`, `game/rooms/maridia/crab_tunnel.tscn` and 9 more |
| `menus/file_select.json` | `game/menus/FileSelectMode.cs` |
| `menus/game_over.json` | `game/menus/GameOverMode.cs` |
| `menus/options.json` | `game/menus/OptionsMode.cs` |
| `menus/save_map.json` | `game/menus/SaveMapMode.cs` |
| `menus/title.json` | `game/menus/TitleMode.cs` |
| `objects/ceres_arrival_pad.json` | `game/rooms/ceres/ceres_elevator.tscn` |
| `objects/ceres_arrival_platform.json` | `game/rooms/ceres/ceres_elevator.tscn` |
| `objects/ceres_doors_escaping.json` | `game/rooms/ceres/ceres_elevator.tscn`, `game/rooms/ceres/ceres_ridley.tscn`, `game/rooms/ceres/ceres_ridley_hallway.tscn` and 3 more |
| `objects/energy_station_arm_left.json` | `game/objects/station/energy_station.tscn` |
| `objects/energy_station_arm_right.json` | `game/objects/station/energy_station.tscn` |
| `objects/energy_station_rest.json` | `game/objects/station/energy_station.tscn` |
| `objects/energy_station_working.json` | `game/objects/station/energy_station.tscn` |
| `objects/map_station_arm_left.json` | `game/objects/station/map_station.tscn` |
| `objects/map_station_arm_right.json` | `game/objects/station/map_station.tscn` |
| `objects/map_station_done.json` | `game/objects/station/map_station.tscn` |
| `objects/map_station_rest.json` | `game/objects/station/map_station.tscn` |
| `objects/map_station_working.json` | `game/objects/station/map_station.tscn` |
| `objects/missile_station_arm_left.json` | `game/objects/station/missile_station.tscn` |
| `objects/missile_station_arm_right.json` | `game/objects/station/missile_station.tscn` |
| `objects/missile_station_rest.json` | `game/objects/station/missile_station.tscn` |
| `objects/missile_station_working.json` | `game/objects/station/missile_station.tscn` |
| `objects/save_station_electricity.json` | `game/objects/station/save_station.tscn` |
| `objects/save_station_rest.json` | `game/objects/station/save_station.tscn` |
| `objects/save_station_working.json` | `game/objects/station/save_station.tscn` |
| `pause/pause.json` | `game/pause/PauseScreen.cs` |
| `rooms/scroll_layouts.json` | `game/compatibility/KeptScrollCells.cs` |
| `samus/colours.json` | `game/samus/Samus.Colours.cs` |
| `samus/death.json` | `game/samus/DeathMode.cs` |
| `text/emergency.png` | `game/rooms/ceres/ceres_ridley.tscn` |
| `tiles/{graphics}/door_lit.json` | `game/rooms/doors/DoorTransitionMode.cs` |
