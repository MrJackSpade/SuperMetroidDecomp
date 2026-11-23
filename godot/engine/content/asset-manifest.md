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
| `bosses/botwoon.json` | `Botwoon` | contact_damage, explosion_effect, explosion_sound, floor_y, health, opening_delay, pre_fall_wait, smoke_effect, speed_curve, wall_drops, wall_updates, wall_x |
| `bosses/ceres_ridley.json` | `Ridley` | arena_reveal_updates, baby_cry_sound, before_roar_updates, bounce_sound, contact_damage, death_drops, death_lunges, dust_effect, explosion_effect, explosion_sound, fade_updates, health, hit_flash_updates, hits_to_drive_off, reveal_delay, roar_hold_updates, roar_sound, samus_low_energy, tail_damage, wounded_health |
| `bosses/crocomire.json` | `Crocomire` | acid_sound, bridge_approach_x, bridge_collapse_x, bridge_sound, contact_damage, death_drops, dissolve_updates, dud_effect, dust_effect, explosion_sound, health, melt_cry, risen_y, rumble_sound, shake_sound, spike_wall_charge_x, spit_sound, submerged_pause, sunken_y, wake_distance, wall_approach_x |
| `bosses/draygon.json` | `Draygon` | bubble_effect, chase_speed, contact_damage, dancer_effect, death_drops, dust_effect, escape_presses, foam_effect, goop_range, goops_per_pass, grapple_shock_damage, health, intro_dance_updates, intro_updates, right_side_distance, smoke_effect, swim_line_y, swoop_quickening, swoop_quickening_limit, swoop_quickening_per_hit, whip_shake_strength |
| `bosses/draygon_cannon.json` | `DraygonCannon` | contact_damage, grapple_drain, health |
| `bosses/draygon_goop.json` | `DraygonGoop` | chamber_size, most_stuck, reach_across, reach_down, stack_spacing, stack_top, stuck_timeline, stuck_updates |
| `bosses/golden_torizo.json` | `Torizo` | awakening_timeline, close_behind, close_in_front, contact_damage, death_drops, eggs_health, face_health, fade_steps, first_attack_missiles, gut_health, health, landing_shake_strength, orb_speed_across, orb_speed_up, orbs_per_burst, repeated_jump_steps, space_jump_updates, stagger_health, start_look, start_timeline, start_tint, turn_delay |
| `bosses/golden_torizo_orb.json` | `TorizoOrb` | bounce_drag, bounces, gravity |
| `bosses/kraid.json` | `Kraid` | backdrop_fade_steps, contact_damage, death_drops, death_spikes_patch, dud_effect, eye_glow_updates, health, lunge_delay_updates, rise_wait_updates, rock_sound, thinking_step_updates |
| `bosses/kraid_lint.json` | `KraidLint` | charge_updates, contact_damage, emerge_pixels, fire_sound, fire_speed, harmless_x, health, point_x, reload_updates, return_x |
| `bosses/kraid_nail.json` | `KraidNail` | below_arm, contact_damage, health, launch_delay |
| `bosses/kraid_rock.json` | `KraidRock` | drift, drift_limit, gravity |
| `bosses/mother_brain.json` | `MotherBrain` | ascent_dust_sound, attack_cry_sound, big_breath_effect, big_explosion_effect, charge_sound, charge_stop_sound, contact_damage, corpse_dust_effect, corpse_dust_sound, door_fragment_effect, drained_grey_step_updates, dust_effect, explosion_effect, explosion_sound, fake_death_grey_step_updates, finish_off_energy, first_phase_health, footstep_sound, from_grey_step_updates, full_strength_health, grey_steps, hand_beam_sound, health, hurt_sound, laser_sound, rainbow_beam_sound, rainbow_beam_stop_sound, samus_stands_energy, second_phase_health, small_breath_effect, smoke_effect, third_phase_health |
| `bosses/mother_brain_baby.json` | `MotherBrainBaby` | contact_damage, cry_sound, death_sound, dust_effect, explosion_effect, explosion_sound, heal_sound, health, remorse_cry_sound, shielding_health |
| `bosses/mother_brain_bomb.json` | `MotherBrainBomb` | dust_effect, explosion_effect, explosion_sound, flare, floor, right_wall |
| `bosses/mother_brain_charge_particle.json` | `MotherBrainDrool` | floor, release_drop, splash_rise, splash_timeline |
| `bosses/mother_brain_drool.json` | `MotherBrainDrool` | floor, release_drop, splash_rise, splash_timeline |
| `bosses/mother_brain_dying_drool.json` | `MotherBrainDrool` | floor, release_drop, splash_rise, splash_timeline |
| `bosses/mother_brain_glass.json` | `MotherBrainGlass` | contact_damage, health, shard_drop, shatter_sound |
| `bosses/mother_brain_ring.json` | `MotherBrainRing` | floor, mouth_updates, top, view_right |
| `bosses/mother_brain_turret.json` | `MotherBrainTurret` | contact_damage, health, least_fire_wait, least_turn_wait, smoke_effect |
| `bosses/phantoon.json` | `Phantoon` | big_explosion_effect, contact_damage, death_drops, dying_cry, explosion_sound, fade_steps, flame_sound, health, rage_sound, round_damage, shot_cry, small_explosion_effect |
| `bosses/phantoon_flame.json` | `PhantoonFlame` | burning_timeline, drift_speed, gravity, landing_drop, landing_pause, landing_timeline, leap_speed, rain_impact_timeline, rain_sound, resting_timeline, ring_hold_updates, ring_radius |
| `bosses/ridley.json` | `Ridley` | arena_reveal_updates, baby_cry_sound, before_roar_updates, bounce_sound, contact_damage, death_drops, death_lunges, dust_effect, explosion_effect, explosion_sound, fade_updates, health, hit_flash_updates, hits_to_drive_off, reveal_delay, roar_hold_updates, roar_sound, samus_low_energy, tail_damage, wounded_health |
| `bosses/ridley_fireball.json` | `RidleyFireball` | final_timeline, flame, flame_speed, flare_across, flare_down, is |
| `bosses/ridley_flame.json` | `RidleyFireball` | final_timeline, flame, flame_speed, flare_across, flare_down, is |
| `bosses/ridley_flare.json` | `RidleyFireball` | final_timeline, flame, flame_speed, flare_across, flare_down, is |
| `bosses/spore_spawn.json` | `SporeSpawn` | ceiling_above, contact_damage, death_drops, descent_speed, dust_effect, explosion_effect, explosion_sound, fast_sway_step, fast_swing_below, hardening_sound, health, stalk_anchor_above, stalk_body_gap, start_rise, sway_widening, vertical_sway_narrowing, widened_sway_limit |
| `bosses/spore_spawn_spore.json` | `SporeSpawnSpore` | fall_distance |
| `bosses/torizo_drool.json` | `TorizoDrool` | drag, drift, gravity |
| `bosses/torizo_egg.json` | `TorizoEgg` | bounce_drag, hatch_updates, quickening |
| `bosses/torizo_eye_beam.json` | `TorizoEyeBeam` |  |
| `bosses/torizo_fragment.json` | `TorizoFragment` | gravity |
| `bosses/torizo_orb.json` | `TorizoOrb` | bounce_drag, bounces, gravity |
| `bosses/torizo_shot.json` | `CuedProjectile` |  |
| `bosses/torizo_sonic_boom.json` | `TorizoSonicBoom` | quickening |
| `bosses/torizo_super_missile.json` | `TorizoSuperMissile` | gravity, throw_speed |
| `cinematics/intro_egg.json` | `IntroEgg` | cry_sound, hatch_sound, slime, slime_splash |
| `cinematics/intro_mother_brain.json` | `IntroMotherBrain` | big_explosion, contact_damage, health, ring, small_explosion |
| `enemies/alcoon.json` | `Alcoon` | contact_damage, emerge_across_distance, emerge_down_distance, emerge_jump_speed, emerge_sound, gravity, health, hide_distance, hide_jump_speed, muzzle_across, muzzle_up, stride |
| `enemies/alcoon_fireball.json` | `AlcoonFireball` | slowing, steady_speed |
| `enemies/armored_zoomer.json` | `Crawler` | contact_damage, falls_at_shake, health, landing_speed, pursues |
| `enemies/atomic.json` | `Atomic` | contact_damage, health |
| `enemies/baby_metroid.json` | `BabyMetroid` | calm_pulse_updates, contact_damage, cry_sound, feeding_cry_sound, feeding_pulse_updates, fly_away_cry_sound, health, remorse_cry_sound, walls_down_patch, walls_up_patch |
| `enemies/baby_turtle.json` | `BabyTurtle` | contact_damage, crawl_speed, health, hide_updates, rider_left_delay, spin_fall_speed, spin_speed, travel_distance |
| `enemies/beetom.json` | `Beetom` | bite_every, cling_above, contact_damage, crawl_speed, crawl_updates, drop_speed, fastest_mark, feeding_sound, feeding_sound_every, feeding_sound_least_energy, flung_marks, flung_speed, health, hop_speed, idle_updates, ledge_reach, long_hop_height, long_hop_marks, lunge_aim_marks, lunge_distance, lunge_falling_marks, lunge_height, lunge_rising_marks, lunge_speed, shake_off_count, short_hop_height, short_hop_marks, speed_curve, thrown_distance |
| `enemies/boulder.json` | `Boulder` | bouncing_travel_step, break_effect, break_sound, contact_damage, fastest_bouncing_mark, fastest_rolling_mark, first_rebound_mark, ground_reach, health, impact_sound, rolling_travel_step, speed_curve |
| `enemies/boyon.json` | `Boyon` | contact_damage, health |
| `enemies/bull.json` | `Bull` | acceleration_step, contact_damage, health, kick_acceleration, kick_guard_updates, kick_speed, off_target_turn, wait_updates |
| `enemies/cacatac.json` | `Cacatac` | attack_chances, contact_damage, diagonal_spike_speed, health, spike_speed |
| `enemies/choot.json` | `Choot` | ascent_step, contact_damage, health, reach, speed_curve |
| `enemies/chozo_statue_lower_norfair.json` | `ChozoStatue` | contact_damage, footstep_depth, footstep_effect, hand_patch, health, lowering_delay, way_patch |
| `enemies/chozo_statue_wrecked_ship.json` | `ChozoStatue` | contact_damage, footstep_depth, footstep_effect, hand_patch, health, lowering_delay, way_patch |
| `enemies/corpse_ripper_level.json` | `Corpse` | contact_damage, disturbed_updates, dust_effect, dust_sound, health, look, rows |
| `enemies/corpse_ripper_tilted.json` | `Corpse` | contact_damage, disturbed_updates, dust_effect, dust_sound, health, look, rows |
| `enemies/corpse_skree.json` | `Corpse` | contact_damage, disturbed_updates, dust_effect, dust_sound, health, look, rows |
| `enemies/corpse_zoomer_on_its_back.json` | `Corpse` | contact_damage, disturbed_updates, dust_effect, dust_sound, health, look, rows |
| `enemies/corpse_zoomer_upright.json` | `Corpse` | contact_damage, disturbed_updates, dust_effect, dust_sound, health, look, rows |
| `enemies/covern.json` | `Covern` | bob_quickening, bob_start_speed, brighten_updates, contact_damage, first_extra_delay, health, lead_distance, materialise_updates, reappearance_delay, steady_course_updates, still_updates, vanish_updates, visible_updates |
| `enemies/dead_sidehopper.json` | `DeadSidehopper` | contact_damage, disturbed_updates, drain_step_updates, dust_effect, dust_sound, health, pause_updates, rows |
| `enemies/dead_torizo.json` | `DeadTorizo` | contact_damage, disturbed_updates, dust_effect, dust_sound, health, look, rows, sand_line_updates, sand_lines, sand_rows |
| `enemies/dessgeega.json` | `Hopper` | across_speed, big_hop_height, big_hop_step, contact_damage, fastest_mark, health, small_hop_height, small_hop_step, speed_curve |
| `enemies/dessgeega_large.json` | `Hopper` | across_speed, big_hop_height, big_hop_step, contact_damage, fastest_mark, health, small_hop_height, small_hop_step, speed_curve |
| `enemies/dragon.json` | `Dragon` | contact_damage, fireball_rise, health, muzzle_across, muzzle_up, rise_updates, shots_per_attack, wait_before_rising, wait_before_sinking |
| `enemies/evir.json` | `Evir` | arms_across, arms_down, contact_damage, grip_across, grip_down, health |
| `enemies/evir_spit.json` | `EvirSpit` | contact_damage, health, regrow_distance, regrow_margin, speed, throw_distance |
| `enemies/eye_door_eye.json` | `EyeDoorEye` | contact_damage, dud_sound, health, hit_sound, hits_to_destroy, puff_effect, smoke_effect, spit_sound |
| `enemies/eye_door_spit.json` | `EyeDoorSpit` | burst_timeline |
| `enemies/face_block.json` | `FaceBlock` | contact_damage, glow_updates, health, wakes_with |
| `enemies/fake_kraid.json` | `FakeKraid` | contact_damage, cry_sound, death_drops, health, spike_sound, step_pixels |
| `enemies/falling_spark.json` | `FallingSpark` | fall_speed_limit, gravity, skip_lift, skip_speed, skitter_times, trail_effect, trail_every |
| `enemies/fireflea.json` | `Fireflea` | contact_damage, health |
| `enemies/fune.json` | `Fune` | contact_damage, health, mouth_drop |
| `enemies/gamet.json` | `Gamet` | contact_damage, health, rise_speed |
| `enemies/geega.json` | `Geega` | contact_damage, emergence_delay, health, speed_curve, swoop_distance, swoop_start_mark, trigger_distance, trigger_height |
| `enemies/geruta.json` | `Swooper` | across_speed, contact_damage, dive_sound, dive_speed, health, lift, reach, return_sound, return_speed, slow_dive_speed |
| `enemies/grapple_ripper.json` | `Ripper` | can_be_latched, contact_damage, health |
| `enemies/hibashi.json` | `Hibashi` | contact_damage, first_wait, half_width, health |
| `enemies/holtz.json` | `Swooper` | across_speed, contact_damage, dive_sound, dive_speed, health, lift, reach, return_sound, return_speed, slow_dive_speed |
| `enemies/jet_ripper.json` | `Ripper` | can_be_latched, contact_damage, health |
| `enemies/kago.json` | `Kago` | contact_damage, health, shake_strength, shake_updates |
| `enemies/kago_bug.json` | `KagoBug` | across_speed, clear_distance, contact_damage, fall_start_speed, gravity, health, homing_distance, jump_speed, shortest_wait |
| `enemies/kamer.json` | `Shutter` | activation_sound, contact_damage, health |
| `enemies/ki_hunter_acid.json` | `KiHunterAcid` | fastest_fall, gravity, launch_shift, splash_timeline |
| `enemies/ki_hunter_green.json` | `KiHunter` | contact_damage, gravity, health, hop_across_speed, hop_up_speed, landing_wait, patrol_depth, patrol_rise, patrol_speed, speed_curve, spit_height, spit_range, spit_reach, spit_wait, strike_after_steps, swoop_distance, swoop_fastest_turn, swoop_least_depth, swoop_quickening, wing_arc_climb, wing_arc_radius, wing_arc_step, wing_half_height, wing_half_width |
| `enemies/ki_hunter_red.json` | `KiHunter` | contact_damage, gravity, health, hop_across_speed, hop_up_speed, landing_wait, patrol_depth, patrol_rise, patrol_speed, speed_curve, spit_height, spit_range, spit_reach, spit_wait, strike_after_steps, swoop_distance, swoop_fastest_turn, swoop_least_depth, swoop_quickening, wing_arc_climb, wing_arc_radius, wing_arc_step, wing_half_height, wing_half_width |
| `enemies/ki_hunter_yellow.json` | `KiHunter` | contact_damage, gravity, health, hop_across_speed, hop_up_speed, landing_wait, patrol_depth, patrol_rise, patrol_speed, speed_curve, spit_height, spit_range, spit_reach, spit_wait, strike_after_steps, swoop_distance, swoop_fastest_turn, swoop_least_depth, swoop_quickening, wing_arc_climb, wing_arc_radius, wing_arc_step, wing_half_height, wing_half_width |
| `enemies/kzan.json` | `Kzan` | contact_damage, fall_quickening, fastest_fall, health, landing_sound, rise_speed, rise_wait |
| `enemies/magdollite.json` | `Magdollite` | attack_cooldown, contact_damage, health, hidden_depth, maximum_rise, most_lengths, pillar_length, smallest_reach, sunk_depth, surfaced_depth, throw_sound |
| `enemies/mama_turtle.json` | `MamaTurtle` | awake_half_height, contact_damage, fall_quickening, fastest_fall, fastest_hover, health, hover_quickening, peak_pause, ridden_climb_speed, ridden_peak, rise_updates, wall_shake_strength, wall_shake_updates, wall_sound |
| `enemies/maridia_fly.json` | `Fly` | contact_damage, dive_across_speed, dive_down_speed, health, reach, rest_updates, wheel_step |
| `enemies/mella.json` | `Fly` | contact_damage, dive_across_speed, dive_down_speed, health, reach, rest_updates, wheel_step |
| `enemies/mellow.json` | `Fly` | contact_damage, dive_across_speed, dive_down_speed, health, reach, rest_updates, wheel_step |
| `enemies/metaree.json` | `Diver` | burrow_sound, burrow_updates, contact_damage, debris_at, distance_per_speed, dive_sound, dive_speed, drift, health, reach |
| `enemies/metroid.json` | `Metroid` | aim_above, contact_damage, drain_hundredths, drop_count, fastest_chase, feeding_sound, feeding_sound_every, feeding_sound_least_energy, frozen_updates, health, latch_distance, recoil_sound |
| `enemies/mochtroid.json` | `Mochtroid` | attached_speed, bite_every, contact_damage, fastest_flight, feeding_sound, feeding_sound_every, feeding_sound_least_energy, health |
| `enemies/multiviola.json` | `Multiviola` | contact_damage, health |
| `enemies/namihe.json` | `Fune` | contact_damage, health, mouth_drop |
| `enemies/ninja_pirate_gold.json` | `NinjaPirate` | activation_distance, claw_attack_every, contact_damage, dive_speed_change, dive_start_speed, dive_step, flinch_distance, health, kick_distance, landing_dust_drop, landing_dust_effect, landing_dust_spread, midpoint_distance, return_step, spin_climb_step, spin_speed_change |
| `enemies/ninja_pirate_silver.json` | `NinjaPirate` | activation_distance, claw_attack_every, contact_damage, dive_speed_change, dive_start_speed, dive_step, flinch_distance, health, kick_distance, landing_dust_drop, landing_dust_effect, landing_dust_spread, midpoint_distance, return_step, spin_climb_step, spin_speed_change |
| `enemies/nuclear_waffle.json` | `NuclearWaffle` | contact_damage, health, left_burst_effect, link_half_size, link_spacing, right_burst_effect, splash_effect, surface_reach, surface_sound, sweep_middle, sweep_reach |
| `enemies/orange_zoomer.json` | `Crawler` | contact_damage, falls_at_shake, health, landing_speed, pursues |
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
| `enemies/sciser.json` | `Crawler` | contact_damage, falls_at_shake, health, landing_speed, pursues |
| `enemies/shaktool.json` | `Shaktool` | contact_damage, health, link_length, saw_half_size, straighten_at, straighten_restart |
| `enemies/shutter_destroyable.json` | `Shutter` | activation_sound, contact_damage, health |
| `enemies/shutter_growing.json` | `GrowingShutter` | activation_sound, contact_damage, health |
| `enemies/shutter_shootable.json` | `Shutter` | activation_sound, contact_damage, health |
| `enemies/sidehopper.json` | `Hopper` | across_speed, big_hop_height, big_hop_step, contact_damage, fastest_mark, health, small_hop_height, small_hop_step, speed_curve |
| `enemies/sidehopper_large.json` | `Hopper` | across_speed, big_hop_height, big_hop_step, contact_damage, fastest_mark, health, small_hop_height, small_hop_step, speed_curve |
| `enemies/sidehopper_tourian.json` | `Hopper` | across_speed, big_hop_height, big_hop_step, contact_damage, fastest_mark, health, small_hop_height, small_hop_step, speed_curve |
| `enemies/skree.json` | `Diver` | burrow_sound, burrow_updates, contact_damage, debris_at, distance_per_speed, dive_sound, dive_speed, drift, health, reach |
| `enemies/skultera.json` | `Skultera` | contact_damage, health |
| `enemies/sova.json` | `Crawler` | contact_damage, falls_at_shake, health, landing_speed, pursues |
| `enemies/spark.json` | `Spark` | contact_damage, drip_below, extra_off_updates, health |
| `enemies/squeept.json` | `Squeept` | block_height, contact_damage, flip_speed, gravity, health, jump_sound, screen_height |
| `enemies/suspensor_platform.json` | `Tripper` | contact_damage, health, resting_depth, speed_curve |
| `enemies/tripper.json` | `Tripper` | contact_damage, health, resting_depth, speed_curve |
| `enemies/viola.json` | `Crawler` | contact_damage, falls_at_shake, health, landing_speed, pursues |
| `enemies/walking_pirate_gold.json` | `WalkingPirate` | contact_damage, firing_band, flinch_reach, health, ledge_reach, muzzle_ahead, walk_speed |
| `enemies/walking_pirate_green.json` | `WalkingPirate` | contact_damage, firing_band, flinch_reach, health, ledge_reach, muzzle_ahead, walk_speed |
| `enemies/walking_pirate_grey.json` | `WalkingPirate` | contact_damage, firing_band, flinch_reach, health, ledge_reach, muzzle_ahead, walk_speed |
| `enemies/walking_pirate_magenta.json` | `WalkingPirate` | contact_damage, firing_band, flinch_reach, health, ledge_reach, muzzle_ahead, walk_speed |
| `enemies/walking_pirate_red.json` | `WalkingPirate` | contact_damage, firing_band, flinch_reach, health, ledge_reach, muzzle_ahead, walk_speed |
| `enemies/walking_pirate_silver.json` | `WalkingPirate` | contact_damage, firing_band, flinch_reach, health, ledge_reach, muzzle_ahead, walk_speed |
| `enemies/wall_pirate_gold.json` | `WallPirate` | block_size, contact_damage, detection_band, fast_jump_turn, health, muzzle_above, muzzle_ahead, slow_jump_short_by, slow_jump_turn |
| `enemies/wall_pirate_grey.json` | `WallPirate` | block_size, contact_damage, detection_band, fast_jump_turn, health, muzzle_above, muzzle_ahead, slow_jump_short_by, slow_jump_turn |
| `enemies/wall_pirate_magenta.json` | `WallPirate` | block_size, contact_damage, detection_band, fast_jump_turn, health, muzzle_above, muzzle_ahead, slow_jump_short_by, slow_jump_turn |
| `enemies/wall_pirate_silver.json` | `WallPirate` | block_size, contact_damage, detection_band, fast_jump_turn, health, muzzle_above, muzzle_ahead, slow_jump_short_by, slow_jump_turn |
| `enemies/waver.json` | `Waver` | contact_damage, health, speed, spins_at, wave_height, wave_step |
| `enemies/work_robot.json` | `WorkRobot` | contact_damage, footstep_sound, gravity, health, laser_cooldown, laser_slant_speed, muzzle_above, muzzle_ahead, recoil_delay, turn_delay |
| `enemies/yapping_maw.json` | `YappingMaw` | bite_sound, contact_damage, grab_cooldown, health, longest_reach, retracted_hold, root_distance, safety_distance, speed_curve |
| `enemies/yard.json` | `Yard` | aggressive_crawl_speed, air_drag, bounce_sound, contact_damage, falls_at_shake, gravity, health, hiding_reach_above, landing_speed_limit, shell_closed_updates, shot_launch_speed |
| `enemies/zeb.json` | `PipeBug` | contact_damage, emergence_height, flight_step, health, respawn_wait, trigger_top, trigger_width |
| `enemies/zebbo.json` | `PipeBug` | contact_damage, emergence_height, flight_step, health, respawn_wait, trigger_top, trigger_width |
| `enemies/zebetite.json` | `Zebetite` | contact_damage, fall_effect, health, pair_offset, shot_sound |
| `enemies/zeela.json` | `Crawler` | contact_damage, falls_at_shake, health, landing_speed, pursues |
| `enemies/zero.json` | `Crawler` | contact_damage, falls_at_shake, health, landing_speed, pursues |
| `enemies/zoa.json` | `Zoa` | contact_damage, health, reach, rise_speed |
| `enemies/zoomer.json` | `Crawler` | contact_damage, falls_at_shake, health, landing_speed, pursues |
| `hud/hud.json` | `Hud` | area_folder, select_sound |
| `objects/ceres_door_left.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/ceres_door_right.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/ceres_elevator_door.json` | `CeresElevatorDoor` | blocked, explosion, explosion_sound, smoke |
| `objects/ceres_ridley_room_door.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/ceres_steam_down.json` | `Hazard` | bite_damage, hit_damage, hold_updates, ongoing_hundredths, release_invincible_updates |
| `objects/ceres_steam_left.json` | `Hazard` | bite_damage, hit_damage, hold_updates, ongoing_hundredths, release_invincible_updates |
| `objects/ceres_steam_right.json` | `Hazard` | bite_damage, hit_damage, hold_updates, ongoing_hundredths, release_invincible_updates |
| `objects/ceres_steam_up.json` | `Hazard` | bite_damage, hit_damage, hold_updates, ongoing_hundredths, release_invincible_updates |
| `objects/dachora.json` | `Dachora` | activation_sound, ceiling_sound, charge_sound, echo_effect, launch_sound, speed_sound, speed_stop_sound |
| `objects/door_cap_blue_down.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/door_cap_blue_left.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/door_cap_blue_right.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/door_cap_blue_up.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/door_cap_eye_left.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/door_cap_eye_right.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/door_cap_green_down.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/door_cap_green_left.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/door_cap_green_right.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/door_cap_green_up.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/door_cap_grey_down.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/door_cap_grey_left.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/door_cap_grey_right.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/door_cap_grey_up.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/door_cap_red_left.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/door_cap_red_right.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/door_cap_yellow_down.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/door_cap_yellow_left.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/door_cap_yellow_right.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/door_cap_yellow_up.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/elevator.json` | `Carrier` |  |
| `objects/escape_dachora.json` | `EscapeDachora` |  |
| `objects/escape_etecoon.json` | `EscapeEtecoon` |  |
| `objects/etecoon.json` | `Etecoon` | jump_sound, wake_sound, wall_sound |
| `objects/gate.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/gate_switch_blue_left.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/gate_switch_blue_right.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/gate_switch_green_left.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/gate_switch_green_right.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/glass_tube.json` | `GlassTube` | break_sound, bubble, crack, cracking, dud_sound, intact, open_patch, shattered |
| `objects/gunship.json` | `Gunship` | dust_effect, hover_sound, pad_closed, pad_closing, pad_closing_sound, pad_opening, pad_opening_sound |
| `objects/item_shell.json` | `Barrier` | any_hit, close_sound, health, hurt_sound, open_sound, sealed_sound |
| `objects/morph_ball_eye.json` | `MorphBallEye` | activation_sound, beam_stop_sound, transition_updates |
| `rooms/tourian_statue_hall.json` | `TourianStatueHall` |  |
| `samus/power_bomb.json` | `PowerBomb` | first_reach, first_spread, fuse_updates, height_parts, last_reach, spread_gain, width_parts |
| `samus/samus.json` | `Samus` | boost_stage_updates, boosted_dash_cap, can_crawl, can_crouch, can_jump_while_crawling, can_wall_jump, charged_spin_updates, climb_per_pixel, crawl_bounce_fall_speed, crawl_bounce_speed, crawl_needs, crouch_needs, crouch_while_running, crystal_flash_ammunition, crystal_flash_drain_every, crystal_flash_energy_below, crystal_flash_energy_each, dash_acceleration, dash_action, dash_boost_needs, dash_cap, dying_updates, follow_ground_down, forms, gravity_damage_over, half_width, invincible_updates, jump_boost_needs, jump_cut_speed, jump_while_crawling_needs, knockback_updates, low_energy, low_energy_sound_every, mediums, reserve_refill_per_update, shinespark_acceleration, shinespark_crash_updates, shinespark_least_energy, shinespark_speed, shinespark_top_speed, shinespark_windup_updates, starting_energy, stored_shine_updates, suit_pickup_updates, turn_updates, varia_damage_over, wall_jump_across_speed, wall_jump_from_any_jump, wall_jump_lock_updates, wall_jump_needs, wall_jump_needs_steering_away, wall_jump_reach, weapons, xray_opening_updates, xray_reach |

## Room layouts

Each is a terrain layout. Those with patches listed must hold patches of those names.

| File | Patches |
| --- | --- |
| `cinematics/intro_discovery_room.json` |  |
| `cinematics/intro_mother_brain_room.json` |  |
| `rooms/brinstar/alpha_power_bomb.json` |  |
| `rooms/brinstar/below_spazer.json` |  |
| `rooms/brinstar/beta_power_bomb.json` |  |
| `rooms/brinstar/big_pink.json` |  |
| `rooms/brinstar/billy_mays.json` |  |
| `rooms/brinstar/blue_brinstar_boulders.json` |  |
| `rooms/brinstar/blue_brinstar_energy_tank.json` |  |
| `rooms/brinstar/brinstar_pre_map.json` |  |
| `rooms/brinstar/brinstar_reserve_tank.json` |  |
| `rooms/brinstar/caterpillar.json` |  |
| `rooms/brinstar/construction_zone.json` |  |
| `rooms/brinstar/dachora.json` |  |
| `rooms/brinstar/early_supers.json` |  |
| `rooms/brinstar/energy_refill_room_door_on_right.json` |  |
| `rooms/brinstar/etecoon_energy_tank.json` |  |
| `rooms/brinstar/etecoon_super.json` |  |
| `rooms/brinstar/first_missile.json` |  |
| `rooms/brinstar/green_brinstar_beetoms.json` |  |
| `rooms/brinstar/green_brinstar_firefleas.json` |  |
| `rooms/brinstar/green_brinstar_main_shaft.json` |  |
| `rooms/brinstar/green_brinstar_missile_refill.json` |  |
| `rooms/brinstar/green_hill_zone.json` |  |
| `rooms/brinstar/hellway.json` |  |
| `rooms/brinstar/hopper_energy_tank.json` |  |
| `rooms/brinstar/kraid.json` |  |
| `rooms/brinstar/kraid_eye_door.json` |  |
| `rooms/brinstar/kraid_refill.json` |  |
| `rooms/brinstar/map_station_room_door_on_right.json` |  |
| `rooms/brinstar/mini_kraid.json` |  |
| `rooms/brinstar/morph_ball.json` |  |
| `rooms/brinstar/noob_bridge.json` |  |
| `rooms/brinstar/pink_brinstar_hoppers.json` |  |
| `rooms/brinstar/pink_brinstar_power_bombs.json` |  |
| `rooms/brinstar/red_brinstar_firefleas.json` |  |
| `rooms/brinstar/red_tower.json` |  |
| `rooms/brinstar/save_room_door_on_left.json` |  |
| `rooms/brinstar/save_room_door_on_right.json` |  |
| `rooms/brinstar/skree_boost.json` |  |
| `rooms/brinstar/spazer.json` |  |
| `rooms/brinstar/spore_spawn.json` | ceiling_open |
| `rooms/brinstar/spore_spawn_farming.json` |  |
| `rooms/brinstar/spore_spawn_kihunters.json` |  |
| `rooms/brinstar/spore_spawn_super.json` |  |
| `rooms/brinstar/varia_suit.json` |  |
| `rooms/brinstar/warehouse_energy_tank.json` |  |
| `rooms/brinstar/warehouse_entrance.json` |  |
| `rooms/brinstar/warehouse_kihunter.json` |  |
| `rooms/brinstar/warehouse_zeela.json` |  |
| `rooms/brinstar/waterway_energy_tank.json` |  |
| `rooms/brinstar/xray_scope.json` |  |
| `rooms/ceres/ceres_elevator.json` |  |
| `rooms/ceres/ceres_ridley.json` |  |
| `rooms/ceres/ceres_ridley_hallway.json` |  |
| `rooms/ceres/dead_scientist.json` |  |
| `rooms/ceres/falling_tile.json` |  |
| `rooms/ceres/magnet_stairs.json` |  |
| `rooms/crateria/bomb_torizo.json` | wall_shot_blocks |
| `rooms/crateria/bowling_alley_path.json` |  |
| `rooms/crateria/climb.json` | escape_wall_destroyed |
| `rooms/crateria/crab_maze.json` |  |
| `rooms/crateria/crateria_kihunter.json` |  |
| `rooms/crateria/crateria_power_bombs.json` |  |
| `rooms/crateria/crateria_super.json` |  |
| `rooms/crateria/crateria_tube.json` |  |
| `rooms/crateria/east_ocean.json` |  |
| `rooms/crateria/elevator_to_blue_brinstar.json` |  |
| `rooms/crateria/final_missile.json` |  |
| `rooms/crateria/final_missile_bombway.json` |  |
| `rooms/crateria/flyway.json` |  |
| `rooms/crateria/forgotten_highway_elbow.json` |  |
| `rooms/crateria/forgotten_highway_elevator.json` |  |
| `rooms/crateria/forgotten_highway_kagos.json` |  |
| `rooms/crateria/gauntlet_energy_tank.json` |  |
| `rooms/crateria/gauntlet_entrance.json` |  |
| `rooms/crateria/green_brinstar_elevator.json` |  |
| `rooms/crateria/green_pirates_shaft.json` |  |
| `rooms/crateria/landing_site.json` |  |
| `rooms/crateria/lower_mushrooms.json` |  |
| `rooms/crateria/map_station_room_door_on_left.json` |  |
| `rooms/crateria/moat.json` |  |
| `rooms/crateria/parlor.json` | animals_passage_open |
| `rooms/crateria/pit.json` |  |
| `rooms/crateria/pre_bowling.json` |  |
| `rooms/crateria/pre_map_flyway.json` |  |
| `rooms/crateria/red_brinstar_elevator.json` |  |
| `rooms/crateria/save_room_door_on_right.json` |  |
| `rooms/crateria/statues.json` | floor_clear, floor_crumbling |
| `rooms/crateria/statues_hallway.json` |  |
| `rooms/crateria/terminator.json` |  |
| `rooms/crateria/west_ocean.json` |  |
| `rooms/maridia/aqueduct.json` |  |
| `rooms/maridia/below_botwoon_energy_tank.json` |  |
| `rooms/maridia/botwoon.json` | wall_down |
| `rooms/maridia/botwoon_energy_tank.json` |  |
| `rooms/maridia/botwoon_hallway.json` |  |
| `rooms/maridia/botwoon_quicksand.json` |  |
| `rooms/maridia/bug_sand_hole.json` |  |
| `rooms/maridia/butterfly.json` |  |
| `rooms/maridia/colosseum.json` |  |
| `rooms/maridia/crab_hole.json` |  |
| `rooms/maridia/crab_shaft.json` |  |
| `rooms/maridia/crab_tunnel.json` |  |
| `rooms/maridia/draygon.json` |  |
| `rooms/maridia/east_aqueduct_quicksand.json` |  |
| `rooms/maridia/east_cactus_alley.json` |  |
| `rooms/maridia/east_pants.json` |  |
| `rooms/maridia/east_sand_hall.json` |  |
| `rooms/maridia/east_sand_hole.json` |  |
| `rooms/maridia/east_tunnel.json` |  |
| `rooms/maridia/energy_refill_room_door_on_left.json` |  |
| `rooms/maridia/fish_tank.json` |  |
| `rooms/maridia/glass_tunnel_default.json` |  |
| `rooms/maridia/glass_tunnel_when_maridia_noob_tube_broken.json` |  |
| `rooms/maridia/halfie_climb.json` |  |
| `rooms/maridia/kassiuz.json` |  |
| `rooms/maridia/main_street.json` |  |
| `rooms/maridia/mama_turtle.json` |  |
| `rooms/maridia/map_station_room_door_on_left.json` |  |
| `rooms/maridia/maridia_elevator.json` |  |
| `rooms/maridia/maridia_missile_refill.json` |  |
| `rooms/maridia/mt_everest.json` |  |
| `rooms/maridia/northwest_maridia_bug.json` |  |
| `rooms/maridia/oasis.json` |  |
| `rooms/maridia/pants.json` |  |
| `rooms/maridia/plasma.json` |  |
| `rooms/maridia/plasma_beach_quicksand.json` |  |
| `rooms/maridia/plasma_spark.json` |  |
| `rooms/maridia/plasma_tutorial.json` |  |
| `rooms/maridia/pseudo_plasma_spark.json` |  |
| `rooms/maridia/red_fish.json` |  |
| `rooms/maridia/save_room_door_on_left.json` |  |
| `rooms/maridia/save_room_door_on_right.json` |  |
| `rooms/maridia/save_room_doors_on_both_sides.json` |  |
| `rooms/maridia/shaktool_default.json` |  |
| `rooms/maridia/shaktool_when_shaktool_cleared_path.json` |  |
| `rooms/maridia/space_jump.json` |  |
| `rooms/maridia/springball.json` |  |
| `rooms/maridia/the_precious.json` |  |
| `rooms/maridia/thread_the_needle.json` |  |
| `rooms/maridia/toilet.json` |  |
| `rooms/maridia/watering_hole.json` |  |
| `rooms/maridia/west_aqueduct_quicksand.json` |  |
| `rooms/maridia/west_cactus_alley.json` |  |
| `rooms/maridia/west_sand_hall.json` |  |
| `rooms/maridia/west_sand_hall_tunnel.json` |  |
| `rooms/maridia/west_sand_hole.json` |  |
| `rooms/maridia/west_tunnel.json` |  |
| `rooms/norfair/acid_snakes_tunnel.json` |  |
| `rooms/norfair/acid_statue.json` |  |
| `rooms/norfair/amphitheatre.json` |  |
| `rooms/norfair/bat_cave.json` |  |
| `rooms/norfair/bubble_mountain.json` |  |
| `rooms/norfair/business_center.json` |  |
| `rooms/norfair/cathedral.json` |  |
| `rooms/norfair/cathedral_entrance.json` |  |
| `rooms/norfair/crocomire.json` | bridge_collapsed, bridge_crumbles_1, bridge_crumbles_2, spike_wall_open |
| `rooms/norfair/crocomire_escape.json` |  |
| `rooms/norfair/crocomire_speedway.json` |  |
| `rooms/norfair/crumble_shaft.json` |  |
| `rooms/norfair/double_chamber.json` |  |
| `rooms/norfair/energy_refill_room_door_on_left.json` |  |
| `rooms/norfair/fast_pillars_setup.json` |  |
| `rooms/norfair/fast_ripper.json` |  |
| `rooms/norfair/frog_speedway.json` |  |
| `rooms/norfair/golden_torizo.json` |  |
| `rooms/norfair/grapple_beam.json` |  |
| `rooms/norfair/grapple_tutorial1.json` |  |
| `rooms/norfair/grapple_tutorial2.json` |  |
| `rooms/norfair/grapple_tutorial3.json` |  |
| `rooms/norfair/green_bubbles_missiles.json` |  |
| `rooms/norfair/hi_jump_boots.json` |  |
| `rooms/norfair/hi_jump_energy_tank.json` |  |
| `rooms/norfair/ice_beam.json` |  |
| `rooms/norfair/ice_beam_acid.json` |  |
| `rooms/norfair/ice_beam_gate.json` |  |
| `rooms/norfair/ice_beam_snake.json` |  |
| `rooms/norfair/ice_beam_tutorial.json` |  |
| `rooms/norfair/kronic_boost.json` |  |
| `rooms/norfair/lava_dive.json` |  |
| `rooms/norfair/lower_norfair_elevator.json` |  |
| `rooms/norfair/lower_norfair_escape_power_bombs.json` |  |
| `rooms/norfair/lower_norfair_farming.json` |  |
| `rooms/norfair/lower_norfair_fireflea.json` |  |
| `rooms/norfair/lower_norfair_spring_ball_maze.json` |  |
| `rooms/norfair/magdollite_tunnel.json` |  |
| `rooms/norfair/main_hall.json` |  |
| `rooms/norfair/map_station_room_door_on_right.json` |  |
| `rooms/norfair/metal_pirates.json` |  |
| `rooms/norfair/mickey_mouse.json` |  |
| `rooms/norfair/norfair_reserve_tank.json` |  |
| `rooms/norfair/pillar.json` |  |
| `rooms/norfair/plowerhouse.json` |  |
| `rooms/norfair/post_crocomire_farming.json` |  |
| `rooms/norfair/post_crocomire_jump.json` |  |
| `rooms/norfair/post_crocomire_missile.json` |  |
| `rooms/norfair/post_crocomire_power_bombs.json` |  |
| `rooms/norfair/post_crocomire_shaft.json` |  |
| `rooms/norfair/purple_farming.json` |  |
| `rooms/norfair/purple_shaft.json` |  |
| `rooms/norfair/red_kihunter_shaft.json` |  |
| `rooms/norfair/red_pirate_shaft.json` |  |
| `rooms/norfair/ridley.json` |  |
| `rooms/norfair/ridley_energy_tank.json` |  |
| `rooms/norfair/rising_tide.json` |  |
| `rooms/norfair/save_room_door_on_left.json` |  |
| `rooms/norfair/save_room_door_on_right.json` |  |
| `rooms/norfair/save_room_doors_on_both_sides.json` |  |
| `rooms/norfair/screw_attack.json` |  |
| `rooms/norfair/single_chamber.json` |  |
| `rooms/norfair/speed_booster.json` |  |
| `rooms/norfair/speed_booster_hall.json` |  |
| `rooms/norfair/spiky_acid_snakes_tunnel.json` |  |
| `rooms/norfair/spiky_platforms_tunnel.json` |  |
| `rooms/norfair/three_musketeers.json` |  |
| `rooms/norfair/upper_norfair_farming.json` |  |
| `rooms/norfair/upper_norfair_refill.json` |  |
| `rooms/norfair/volcano.json` |  |
| `rooms/norfair/wasteland.json` |  |
| `rooms/norfair/wave_beam.json` |  |
| `rooms/norfair/worst_room_in_the_game.json` |  |
| `rooms/tourian/big_boy.json` |  |
| `rooms/tourian/blue_hopper.json` |  |
| `rooms/tourian/dust_torizo.json` |  |
| `rooms/tourian/metroids1.json` |  |
| `rooms/tourian/metroids2.json` |  |
| `rooms/tourian/metroids3.json` |  |
| `rooms/tourian/metroids4.json` |  |
| `rooms/tourian/mother_brain.json` |  |
| `rooms/tourian/rinka_shaft.json` |  |
| `rooms/tourian/save_room_door_on_left.json` |  |
| `rooms/tourian/save_room_door_on_right.json` |  |
| `rooms/tourian/seaweed.json` |  |
| `rooms/tourian/tourian_escape1.json` |  |
| `rooms/tourian/tourian_escape2.json` |  |
| `rooms/tourian/tourian_escape3.json` |  |
| `rooms/tourian/tourian_escape4.json` |  |
| `rooms/tourian/tourian_eye_door.json` |  |
| `rooms/tourian/tourian_first.json` |  |
| `rooms/tourian/tourian_recharge.json` |  |
| `rooms/wrecked_ship/assembly_line.json` |  |
| `rooms/wrecked_ship/attic_default.json` |  |
| `rooms/wrecked_ship/attic_when_area_boss_dead.json` |  |
| `rooms/wrecked_ship/basement.json` |  |
| `rooms/wrecked_ship/bowling_alley.json` |  |
| `rooms/wrecked_ship/electric_death.json` |  |
| `rooms/wrecked_ship/gravity_suit.json` |  |
| `rooms/wrecked_ship/map_station_room_door_on_right.json` |  |
| `rooms/wrecked_ship/phantoon.json` |  |
| `rooms/wrecked_ship/save_room_door_on_left.json` |  |
| `rooms/wrecked_ship/spiky_death.json` |  |
| `rooms/wrecked_ship/sponge_bath.json` |  |
| `rooms/wrecked_ship/wrecked_ship_east_super_default.json` |  |
| `rooms/wrecked_ship/wrecked_ship_east_super_when_area_boss_dead.json` |  |
| `rooms/wrecked_ship/wrecked_ship_energy_tank.json` |  |
| `rooms/wrecked_ship/wrecked_ship_entrance.json` |  |
| `rooms/wrecked_ship/wrecked_ship_main_shaft_default.json` |  |
| `rooms/wrecked_ship/wrecked_ship_main_shaft_when_area_boss_dead.json` |  |
| `rooms/wrecked_ship/wrecked_ship_west_super.json` |  |

## Other files named

| File | Named in |
| --- | --- |
| `backdrops/brinstar_blue_grid_blocks.png` | `game/rooms/brinstar/alpha_power_bomb.tscn`, `game/rooms/brinstar/spazer.tscn` |
| `backdrops/brinstar_dark_pattern.png` | `game/rooms/brinstar/big_pink.tscn` |
| `backdrops/brinstar_horizontal_glow_patches.png` | `game/rooms/brinstar/etecoon_energy_tank.tscn`, `game/rooms/brinstar/etecoon_super.tscn`, `game/rooms/brinstar/green_brinstar_beetoms.tscn` and 2 more |
| `backdrops/brinstar_large_horizontal_pattern.png` | `game/rooms/brinstar/brinstar_reserve_tank.tscn` |
| `backdrops/brinstar_mechanical_room.png` | `game/rooms/brinstar/warehouse_entrance.tscn` |
| `backdrops/brinstar_mechanical_room_spike_floor.png` | `game/rooms/brinstar/xray_scope.tscn` |
| `backdrops/brinstar_medium_horizontal_pattern.png` | `game/rooms/brinstar/brinstar_pre_map.tscn` |
| `backdrops/brinstar_narrow_vertical_tower_brick.png` | `game/rooms/brinstar/beta_power_bomb.tscn`, `game/rooms/brinstar/skree_boost.tscn` |
| `backdrops/brinstar_narrow_vertical_tower_brick_vines.png` | `game/rooms/brinstar/mini_kraid.tscn` |
| `backdrops/brinstar_small_pattern.png` | `game/rooms/brinstar/construction_zone.tscn`, `game/rooms/brinstar/first_missile.tscn`, `game/rooms/brinstar/spore_spawn_super.tscn` |
| `backdrops/brinstar_spheres_pattern.png` | `game/rooms/brinstar/green_hill_zone.tscn` |
| `backdrops/brinstar_thin_horizontal_pattern.png` | `game/rooms/brinstar/early_supers.tscn`, `game/rooms/brinstar/pink_brinstar_hoppers.tscn`, `game/rooms/brinstar/pink_brinstar_power_bombs.tscn` and 1 more |
| `backdrops/brinstar_varied_small_pattern.png` | `game/rooms/brinstar/billy_mays.tscn`, `game/rooms/brinstar/blue_brinstar_boulders.tscn`, `game/rooms/brinstar/blue_brinstar_energy_tank.tscn` and 1 more |
| `backdrops/brinstar_vertical_brick.png` | `game/rooms/brinstar/below_spazer.tscn` |
| `backdrops/brinstar_vertical_glow_patches.png` | `game/rooms/brinstar/green_brinstar_main_shaft.tscn` |
| `backdrops/brinstar_vertical_tower.png` | `game/rooms/brinstar/hellway.tscn` |
| `backdrops/brinstar_wide_vertical_tower_brick.png` | `game/rooms/brinstar/red_tower.tscn` |
| `backdrops/ceres_capsule_hall.png` | `game/rooms/ceres/ceres_ridley_hallway.tscn`, `game/rooms/ceres/falling_tile.tscn`, `game/rooms/ceres/magnet_stairs.tscn` |
| `backdrops/ceres_elevator_shaft.png` | `game/rooms/ceres/ceres_elevator.tscn` |
| `backdrops/ceres_ridley_floor.png` | `game/rooms/ceres/ceres_ridley.tscn` |
| `backdrops/crateria_brick_room.png` | `game/rooms/crateria/final_missile_bombway.tscn`, `game/rooms/crateria/green_pirates_shaft.tscn` |
| `backdrops/crateria_brick_room_wall_art.png` | `game/rooms/crateria/pre_map_flyway.tscn` |
| `backdrops/crateria_brick_room_wall_art_dark.png` | `game/rooms/crateria/flyway.tscn` |
| `backdrops/crateria_elevator.png` | `game/rooms/crateria/forgotten_highway_elevator.tscn`, `game/rooms/crateria/red_brinstar_elevator.tscn` |
| `backdrops/crateria_horizontal_pattern_rocks.png` | `game/rooms/crateria/bowling_alley_path.tscn`, `game/rooms/crateria/gauntlet_energy_tank.tscn`, `game/rooms/crateria/gauntlet_entrance.tscn` |
| `backdrops/crateria_mechanical.png` | `game/rooms/crateria/bomb_torizo.tscn`, `game/rooms/crateria/climb.tscn`, `game/rooms/crateria/crateria_super.tscn` and 2 more |
| `backdrops/crateria_purple_rocks.png` | `game/rooms/crateria/lower_mushrooms.tscn`, `game/rooms/crateria/terminator.tscn` |
| `backdrops/crateria_rocks.png` | `game/rooms/crateria/crab_maze.tscn`, `game/rooms/crateria/crateria_kihunter.tscn`, `game/rooms/crateria/crateria_power_bombs.tscn` and 2 more |
| `backdrops/crateria_vertical_pattern_rocks.png` | `game/rooms/crateria/forgotten_highway_elbow.tscn`, `game/rooms/crateria/forgotten_highway_kagos.tscn` |
| `backdrops/maridia_brown_wall.png` | `game/rooms/maridia/crab_tunnel.tscn` |
| `backdrops/maridia_green_wall.png` | `game/rooms/maridia/east_sand_hall.tscn`, `game/rooms/maridia/west_sand_hall.tscn` |
| `backdrops/norfair_bubbles.png` | `game/rooms/norfair/bubble_mountain.tscn`, `game/rooms/norfair/green_bubbles_missiles.tscn` |
| `backdrops/norfair_cavern_horizontal_ruins.png` | `game/rooms/norfair/worst_room_in_the_game.tscn` |
| `backdrops/norfair_cavern_room_pillars.png` | `game/rooms/norfair/acid_statue.tscn`, `game/rooms/norfair/lower_norfair_escape_power_bombs.tscn`, `game/rooms/norfair/ridley.tscn` and 3 more |
| `backdrops/norfair_cavern_stalactites.png` | `game/rooms/norfair/acid_snakes_tunnel.tscn`, `game/rooms/norfair/grapple_tutorial1.tscn`, `game/rooms/norfair/lower_norfair_elevator.tscn` and 7 more |
| `backdrops/norfair_cavern_vertical.png` | `game/rooms/norfair/business_center.tscn`, `game/rooms/norfair/crumble_shaft.tscn`, `game/rooms/norfair/grapple_beam.tscn` and 3 more |
| `backdrops/norfair_cavern_vertical_ruins.png` | `game/rooms/norfair/fast_ripper.tscn`, `game/rooms/norfair/main_hall.tscn`, `game/rooms/norfair/pillar.tscn` |
| `backdrops/norfair_purple_patches.png` | `game/rooms/norfair/lower_norfair_spring_ball_maze.tscn`, `game/rooms/norfair/mickey_mouse.tscn`, `game/rooms/norfair/norfair_reserve_tank.tscn` and 3 more |
| `backdrops/norfair_room_statues.png` | `game/rooms/norfair/lower_norfair_farming.tscn`, `game/rooms/norfair/metal_pirates.tscn`, `game/rooms/norfair/plowerhouse.tscn` |
| `backdrops/norfair_small_pattern_brown_purple.png` | `game/rooms/norfair/amphitheatre.tscn`, `game/rooms/norfair/crocomire_speedway.tscn`, `game/rooms/norfair/grapple_tutorial3.tscn` and 6 more |
| `backdrops/norfair_small_pattern_brown_purple_bright.png` | `game/rooms/norfair/frog_speedway.tscn`, `game/rooms/norfair/hi_jump_boots.tscn`, `game/rooms/norfair/ice_beam_acid.tscn` and 2 more |
| `backdrops/norfair_vertical_pattern_brick.png` | `game/rooms/norfair/fast_pillars_setup.tscn` |
| `backdrops/scrolling_sky_east_ocean.png` | `game/rooms/crateria/east_ocean.tscn` |
| `backdrops/scrolling_sky_landing_site.png` | `game/rooms/crateria/landing_site.tscn` |
| `backdrops/scrolling_sky_pre_bowling.png` | `game/rooms/crateria/pre_bowling.tscn` |
| `backdrops/scrolling_sky_west_ocean.png` | `game/rooms/crateria/west_ocean.tscn` |
| `backdrops/statues_hallway_grey_tower_brick.png` | `game/rooms/crateria/statues_hallway.tscn` |
| `backdrops/tourian_mechanical_room.png` | `game/rooms/tourian/metroids2.tscn`, `game/rooms/tourian/metroids4.tscn`, `game/rooms/tourian/tourian_first.tscn` |
| `backdrops/tourian_mechanical_room_letterbox.png` | `game/rooms/tourian/metroids1.tscn`, `game/rooms/tourian/metroids3.tscn` |
| `backdrops/tourian_pipe_room.png` | `game/rooms/tourian/tourian_escape3.tscn`, `game/rooms/tourian/tourian_escape4.tscn` |
| `backdrops/tourian_pipe_room_dark.png` | `game/rooms/tourian/mother_brain.tscn` |
| `backdrops/tourian_pipe_room_letterbox.png` | `game/rooms/tourian/tourian_escape1.tscn` |
| `backdrops/tourian_pipe_room_vertical_separation.png` | `game/rooms/tourian/rinka_shaft.tscn`, `game/rooms/tourian/tourian_escape2.tscn` |
| `backdrops/tourian_statues.png` | `game/rooms/crateria/statues.tscn` |
| `backdrops/tourian_vertical_structures.png` | `game/rooms/tourian/big_boy.tscn`, `game/rooms/tourian/blue_hopper.tscn`, `game/rooms/tourian/dust_torizo.tscn` and 2 more |
| `backdrops/wrecked_ship_columns_tubes.png` | `game/rooms/wrecked_ship/basement.tscn`, `game/rooms/wrecked_ship/spiky_death.tscn` |
| `backdrops/wrecked_ship_entrance_hall.png` | `game/rooms/wrecked_ship/wrecked_ship_entrance.tscn` |
| `backdrops/wrecked_ship_experiment_room.png` | `game/rooms/wrecked_ship/attic.tscn` |
| `backdrops/wrecked_ship_panels_vents.png` | `game/rooms/wrecked_ship/gravity_suit.tscn`, `game/rooms/wrecked_ship/sponge_bath.tscn`, `game/rooms/wrecked_ship/wrecked_ship_east_super.tscn` and 1 more |
| `backdrops/wrecked_ship_panels_vents_horizontal.png` | `game/rooms/wrecked_ship/electric_death.tscn`, `game/rooms/wrecked_ship/wrecked_ship_east_super.tscn`, `game/rooms/wrecked_ship/wrecked_ship_main_shaft.tscn` |
| `backdrops/wrecked_ship_panels_vents_vertical.png` | `game/rooms/wrecked_ship/assembly_line.tscn` |
| `cinematics/ceres_destruction.json` | `game/cinematics/CeresDestruction.cs` |
| `cinematics/ending.json` | `game/cinematics/Ending.cs` |
| `cinematics/intro.json` | `game/cinematics/Intro.cs` |
| `effects/ceres_debris_dark.json` | `game/rooms/ceres/falling_tile.tscn` |
| `effects/ceres_debris_light.json` | `game/rooms/ceres/falling_tile.tscn` |
| `effects/ceres_haze.png` | `game/rooms/ceres/ceres_elevator.tscn`, `game/rooms/ceres/ceres_ridley.tscn`, `game/rooms/ceres/ceres_ridley_hallway.tscn` and 3 more |
| `effects/fog.png` | `game/rooms/crateria/climb.tscn`, `game/rooms/crateria/pit.tscn`, `game/rooms/crateria/statues_hallway.tscn` |
| `effects/rain.png` | `game/rooms/crateria/landing_site.tscn` |
| `effects/spores.png` | `game/rooms/brinstar/big_pink.tscn`, `game/rooms/brinstar/dachora.tscn`, `game/rooms/brinstar/spore_spawn_farming.tscn` and 1 more |
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
| `liquids/acid.json` | `game/rooms/crateria/bomb_torizo.tscn`, `game/rooms/crateria/climb.tscn`, `game/rooms/crateria/crateria_power_bombs.tscn` and 25 more |
| `liquids/lava.json` | `game/rooms/crateria/climb.tscn`, `game/rooms/norfair/acid_snakes_tunnel.tscn`, `game/rooms/norfair/bat_cave.tscn` and 18 more |
| `liquids/water.json` | `game/rooms/brinstar/below_spazer.tscn`, `game/rooms/brinstar/blue_brinstar_boulders.tscn`, `game/rooms/brinstar/skree_boost.tscn` and 12 more |
| `liquids/water_green.json` | `game/rooms/maridia/east_pants.tscn`, `game/rooms/maridia/east_sand_hall.tscn`, `game/rooms/maridia/oasis.tscn` and 5 more |
| `liquids/water_indigo.json` | `game/rooms/maridia/aqueduct_save.tscn`, `game/rooms/maridia/bug_sand_hole.tscn`, `game/rooms/maridia/butterfly.tscn` and 7 more |
| `liquids/water_purple.json` | `game/rooms/maridia/aqueduct.tscn`, `game/rooms/maridia/below_botwoon_energy_tank.tscn`, `game/rooms/maridia/botwoon.tscn` and 11 more |
| `liquids/water_teal.json` | `game/rooms/maridia/crab_hole.tscn`, `game/rooms/maridia/crab_shaft.tscn`, `game/rooms/maridia/crab_tunnel.tscn` and 9 more |
| `menus/game_over.png` | `game/menus/GameOverMode.cs` |
| `menus/title.png` | `game/menus/TitleMode.cs` |
| `objects/energy_station_rest.json` | `game/objects/station/energy_station.tscn` |
| `objects/energy_station_working.json` | `game/objects/station/energy_station.tscn` |
| `objects/map_station_rest.json` | `game/objects/station/map_station.tscn` |
| `objects/map_station_working.json` | `game/objects/station/map_station.tscn` |
| `objects/missile_station_rest.json` | `game/objects/station/missile_station.tscn` |
| `objects/missile_station_working.json` | `game/objects/station/missile_station.tscn` |
| `objects/save_station_rest.json` | `game/objects/station/save_station.tscn` |
| `objects/save_station_working.json` | `game/objects/station/save_station.tscn` |
| `text/emergency.png` | `game/rooms/ceres/ceres_ridley.tscn` |
