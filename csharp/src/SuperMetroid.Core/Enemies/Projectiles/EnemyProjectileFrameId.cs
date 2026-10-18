namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The frames an enemy projectile shows: the bank-$8D compositions its programs select, then every installed program
/// frame, named by its installed composition, in schema order.
/// </summary>
public enum EnemyProjectileFrameId
{
    /// <summary>The empty bank-$8D projectile spritemap, which draws nothing.</summary>
    Blank = 1,
    /// <summary><c>skree_debris</c>, a bank-$8D composition.</summary>
    SkreeDebris,
    /// <summary><c>metaree_debris</c>, a bank-$8D composition.</summary>
    MetareeDebris,
    /// <summary><c>draygon_frame_00</c>, an installed program frame.</summary>
    DraygonFrame00,
    /// <summary><c>draygon_frame_01</c>, an installed program frame.</summary>
    DraygonFrame01,
    /// <summary><c>draygon_frame_02</c>, an installed program frame.</summary>
    DraygonFrame02,
    /// <summary><c>draygon_frame_03</c>, an installed program frame.</summary>
    DraygonFrame03,
    /// <summary><c>draygon_frame_04</c>, an installed program frame.</summary>
    DraygonFrame04,
    /// <summary><c>draygon_frame_05</c>, an installed program frame.</summary>
    DraygonFrame05,
    /// <summary><c>draygon_frame_06</c>, an installed program frame.</summary>
    DraygonFrame06,
    /// <summary><c>draygon_frame_07</c>, an installed program frame.</summary>
    DraygonFrame07,
    /// <summary><c>draygon_frame_08</c>, an installed program frame.</summary>
    DraygonFrame08,
    /// <summary><c>draygon_frame_09</c>, an installed program frame.</summary>
    DraygonFrame09,
    /// <summary><c>draygon_frame_10</c>, an installed program frame.</summary>
    DraygonFrame10,
    /// <summary><c>draygon_frame_11</c>, an installed program frame.</summary>
    DraygonFrame11,
    /// <summary><c>draygon_frame_12</c>, an installed program frame.</summary>
    DraygonFrame12,
    /// <summary><c>draygon_frame_13</c>, an installed program frame.</summary>
    DraygonFrame13,
    /// <summary><c>draygon_frame_14</c>, an installed program frame.</summary>
    DraygonFrame14,
    /// <summary><c>draygon_frame_15</c>, an installed program frame.</summary>
    DraygonFrame15,
    /// <summary><c>draygon_frame_16</c>, an installed program frame.</summary>
    DraygonFrame16,
    /// <summary><c>draygon_frame_17</c>, an installed program frame.</summary>
    DraygonFrame17,
    /// <summary><c>draygon_frame_18</c>, an installed program frame.</summary>
    DraygonFrame18,
    /// <summary><c>draygon_frame_19</c>, an installed program frame.</summary>
    DraygonFrame19,
    /// <summary><c>draygon_frame_20</c>, an installed program frame.</summary>
    DraygonFrame20,
    /// <summary><c>draygon_frame_21</c>, an installed program frame.</summary>
    DraygonFrame21,
    /// <summary><c>draygon_frame_22</c>, an installed program frame.</summary>
    DraygonFrame22,
    /// <summary><c>draygon_frame_23</c>, an installed program frame.</summary>
    DraygonFrame23,
    /// <summary><c>draygon_frame_24</c>, an installed program frame.</summary>
    DraygonFrame24,
    /// <summary><c>draygon_frame_25</c>, an installed program frame.</summary>
    DraygonFrame25,
    /// <summary><c>draygon_frame_26</c>, an installed program frame.</summary>
    DraygonFrame26,
    /// <summary><c>crocomire_frame_00</c>, an installed program frame.</summary>
    CrocomireFrame00,
    /// <summary><c>crocomire_frame_01</c>, an installed program frame.</summary>
    CrocomireFrame01,
    /// <summary><c>crocomire_frame_02</c>, an installed program frame.</summary>
    CrocomireFrame02,
    /// <summary><c>crocomire_frame_03</c>, an installed program frame.</summary>
    CrocomireFrame03,
    /// <summary><c>crocomire_frame_04</c>, an installed program frame.</summary>
    CrocomireFrame04,
    /// <summary><c>crocomire_frame_05</c>, an installed program frame.</summary>
    CrocomireFrame05,
    /// <summary><c>crocomire_frame_06</c>, an installed program frame.</summary>
    CrocomireFrame06,
    /// <summary><c>crocomire_frame_07</c>, an installed program frame.</summary>
    CrocomireFrame07,
    /// <summary><c>crocomire_frame_08</c>, an installed program frame.</summary>
    CrocomireFrame08,
    /// <summary><c>crocomire_frame_09</c>, an installed program frame.</summary>
    CrocomireFrame09,
    /// <summary><c>crocomire_frame_10</c>, an installed program frame.</summary>
    CrocomireFrame10,
    /// <summary><c>crocomire_frame_11</c>, an installed program frame.</summary>
    CrocomireFrame11,
    /// <summary><c>crocomire_frame_12</c>, an installed program frame.</summary>
    CrocomireFrame12,
    /// <summary><c>ceres_ridley_frame_00</c>, an installed program frame.</summary>
    CeresRidleyFrame00,
    /// <summary><c>ceres_ridley_frame_01</c>, an installed program frame.</summary>
    CeresRidleyFrame01,
    /// <summary><c>ceres_ridley_frame_02</c>, an installed program frame.</summary>
    CeresRidleyFrame02,
    /// <summary><c>ceres_ridley_frame_03</c>, an installed program frame.</summary>
    CeresRidleyFrame03,
    /// <summary><c>ceres_ridley_frame_04</c>, an installed program frame.</summary>
    CeresRidleyFrame04,
    /// <summary><c>ceres_ridley_frame_05</c>, an installed program frame.</summary>
    CeresRidleyFrame05,
    /// <summary><c>ceres_ridley_frame_06</c>, an installed program frame.</summary>
    CeresRidleyFrame06,
    /// <summary><c>ceres_ridley_frame_07</c>, an installed program frame.</summary>
    CeresRidleyFrame07,
    /// <summary><c>ceres_ridley_frame_08</c>, an installed program frame.</summary>
    CeresRidleyFrame08,
    /// <summary><c>ceres_ridley_frame_09</c>, an installed program frame.</summary>
    CeresRidleyFrame09,
    /// <summary><c>ceres_ridley_frame_10</c>, an installed program frame.</summary>
    CeresRidleyFrame10,
    /// <summary><c>ceres_ridley_frame_11</c>, an installed program frame.</summary>
    CeresRidleyFrame11,
    /// <summary><c>ceres_ridley_frame_12</c>, an installed program frame.</summary>
    CeresRidleyFrame12,
    /// <summary><c>ceres_ridley_frame_13</c>, an installed program frame.</summary>
    CeresRidleyFrame13,
    /// <summary><c>ceres_ridley_frame_14</c>, an installed program frame.</summary>
    CeresRidleyFrame14,
    /// <summary><c>ceres_ridley_frame_15</c>, an installed program frame.</summary>
    CeresRidleyFrame15,
    /// <summary><c>ceres_ridley_frame_16</c>, an installed program frame.</summary>
    CeresRidleyFrame16,
    /// <summary><c>ceres_ridley_frame_17</c>, an installed program frame.</summary>
    CeresRidleyFrame17,
    /// <summary><c>ceres_ridley_frame_18</c>, an installed program frame.</summary>
    CeresRidleyFrame18,
    /// <summary><c>ceres_ridley_frame_19</c>, an installed program frame.</summary>
    CeresRidleyFrame19,
    /// <summary><c>ceres_ridley_frame_20</c>, an installed program frame.</summary>
    CeresRidleyFrame20,
    /// <summary><c>ceres_ridley_frame_21</c>, an installed program frame.</summary>
    CeresRidleyFrame21,
    /// <summary><c>ceres_ridley_frame_22</c>, an installed program frame.</summary>
    CeresRidleyFrame22,
    /// <summary><c>ceres_ridley_frame_23</c>, an installed program frame.</summary>
    CeresRidleyFrame23,
    /// <summary><c>ceres_ridley_frame_24</c>, an installed program frame.</summary>
    CeresRidleyFrame24,
    /// <summary><c>ceres_ridley_frame_25</c>, an installed program frame.</summary>
    CeresRidleyFrame25,
    /// <summary><c>ceres_falling_debris_frame_00</c>, an installed program frame.</summary>
    CeresFallingDebrisFrame00,
    /// <summary><c>ceres_falling_debris_frame_01</c>, an installed program frame.</summary>
    CeresFallingDebrisFrame01,
    /// <summary><c>phantoon_frame_00</c>, an installed program frame.</summary>
    PhantoonFrame00,
    /// <summary><c>phantoon_frame_01</c>, an installed program frame.</summary>
    PhantoonFrame01,
    /// <summary><c>phantoon_frame_02</c>, an installed program frame.</summary>
    PhantoonFrame02,
    /// <summary><c>phantoon_frame_03</c>, an installed program frame.</summary>
    PhantoonFrame03,
    /// <summary><c>phantoon_frame_04</c>, an installed program frame.</summary>
    PhantoonFrame04,
    /// <summary><c>phantoon_frame_05</c>, an installed program frame.</summary>
    PhantoonFrame05,
    /// <summary><c>phantoon_frame_06</c>, an installed program frame.</summary>
    PhantoonFrame06,
    /// <summary><c>phantoon_frame_07</c>, an installed program frame.</summary>
    PhantoonFrame07,
    /// <summary><c>phantoon_frame_08</c>, an installed program frame.</summary>
    PhantoonFrame08,
    /// <summary><c>phantoon_frame_09</c>, an installed program frame.</summary>
    PhantoonFrame09,
    /// <summary><c>phantoon_frame_10</c>, an installed program frame.</summary>
    PhantoonFrame10,
    /// <summary><c>phantoon_frame_11</c>, an installed program frame.</summary>
    PhantoonFrame11,
    /// <summary><c>phantoon_frame_12</c>, an installed program frame.</summary>
    PhantoonFrame12,
    /// <summary><c>phantoon_frame_13</c>, an installed program frame.</summary>
    PhantoonFrame13,
    /// <summary><c>phantoon_frame_14</c>, an installed program frame.</summary>
    PhantoonFrame14,
    /// <summary><c>phantoon_frame_15</c>, an installed program frame.</summary>
    PhantoonFrame15,
    /// <summary><c>phantoon_frame_16</c>, an installed program frame.</summary>
    PhantoonFrame16,
    /// <summary><c>phantoon_frame_17</c>, an installed program frame.</summary>
    PhantoonFrame17,
    /// <summary><c>phantoon_frame_18</c>, an installed program frame.</summary>
    PhantoonFrame18,
    /// <summary><c>phantoon_frame_19</c>, an installed program frame.</summary>
    PhantoonFrame19,
    /// <summary><c>phantoon_frame_20</c>, an installed program frame.</summary>
    PhantoonFrame20,
    /// <summary><c>phantoon_frame_21</c>, an installed program frame.</summary>
    PhantoonFrame21,
    /// <summary><c>phantoon_frame_22</c>, an installed program frame.</summary>
    PhantoonFrame22,
    /// <summary><c>phantoon_frame_23</c>, an installed program frame.</summary>
    PhantoonFrame23,
    /// <summary><c>phantoon_frame_24</c>, an installed program frame.</summary>
    PhantoonFrame24,
    /// <summary><c>phantoon_frame_25</c>, an installed program frame.</summary>
    PhantoonFrame25,
    /// <summary><c>phantoon_frame_26</c>, an installed program frame.</summary>
    PhantoonFrame26,
    /// <summary><c>phantoon_frame_27</c>, an installed program frame.</summary>
    PhantoonFrame27,
    /// <summary><c>phantoon_frame_28</c>, an installed program frame.</summary>
    PhantoonFrame28,
    /// <summary><c>phantoon_frame_29</c>, an installed program frame.</summary>
    PhantoonFrame29,
    /// <summary><c>phantoon_frame_30</c>, an installed program frame.</summary>
    PhantoonFrame30,
    /// <summary><c>kraid_rock_frame_00</c>, an installed program frame.</summary>
    KraidRockFrame00,
    /// <summary><c>kraid_rock_frame_01</c>, an installed program frame.</summary>
    KraidRockFrame01,
    /// <summary><c>kraid_rock_frame_02</c>, an installed program frame.</summary>
    KraidRockFrame02,
    /// <summary><c>kraid_rock_frame_03</c>, an installed program frame.</summary>
    KraidRockFrame03,
    /// <summary><c>kraid_rock_frame_04</c>, an installed program frame.</summary>
    KraidRockFrame04,
    /// <summary><c>kraid_rock_frame_05</c>, an installed program frame.</summary>
    KraidRockFrame05,
    /// <summary><c>kraid_rock_frame_06</c>, an installed program frame.</summary>
    KraidRockFrame06,
    /// <summary><c>fake_kraid_frame_00</c>, an installed program frame.</summary>
    FakeKraidFrame00,
    /// <summary><c>fake_kraid_frame_01</c>, an installed program frame.</summary>
    FakeKraidFrame01,
    /// <summary><c>fake_kraid_frame_02</c>, an installed program frame.</summary>
    FakeKraidFrame02,
    /// <summary><c>alcoon_fireball_frame_00</c>, an installed program frame.</summary>
    AlcoonFireballFrame00,
    /// <summary><c>alcoon_fireball_frame_01</c>, an installed program frame.</summary>
    AlcoonFireballFrame01,
    /// <summary><c>alcoon_fireball_frame_02</c>, an installed program frame.</summary>
    AlcoonFireballFrame02,
    /// <summary><c>alcoon_fireball_frame_03</c>, an installed program frame.</summary>
    AlcoonFireballFrame03,
    /// <summary><c>space_pirate_frame_00</c>, an installed program frame.</summary>
    SpacePirateFrame00,
    /// <summary><c>space_pirate_frame_01</c>, an installed program frame.</summary>
    SpacePirateFrame01,
    /// <summary><c>space_pirate_frame_02</c>, an installed program frame.</summary>
    SpacePirateFrame02,
    /// <summary><c>space_pirate_frame_03</c>, an installed program frame.</summary>
    SpacePirateFrame03,
    /// <summary><c>space_pirate_frame_04</c>, an installed program frame.</summary>
    SpacePirateFrame04,
    /// <summary><c>space_pirate_frame_05</c>, an installed program frame.</summary>
    SpacePirateFrame05,
    /// <summary><c>space_pirate_frame_06</c>, an installed program frame.</summary>
    SpacePirateFrame06,
    /// <summary><c>space_pirate_frame_07</c>, an installed program frame.</summary>
    SpacePirateFrame07,
    /// <summary><c>space_pirate_frame_08</c>, an installed program frame.</summary>
    SpacePirateFrame08,
    /// <summary><c>space_pirate_frame_09</c>, an installed program frame.</summary>
    SpacePirateFrame09,
    /// <summary><c>space_pirate_frame_10</c>, an installed program frame.</summary>
    SpacePirateFrame10,
    /// <summary><c>space_pirate_frame_11</c>, an installed program frame.</summary>
    SpacePirateFrame11,
    /// <summary><c>space_pirate_frame_12</c>, an installed program frame.</summary>
    SpacePirateFrame12,
    /// <summary><c>space_pirate_frame_13</c>, an installed program frame.</summary>
    SpacePirateFrame13,
    /// <summary><c>space_pirate_frame_14</c>, an installed program frame.</summary>
    SpacePirateFrame14,
    /// <summary><c>space_pirate_frame_15</c>, an installed program frame.</summary>
    SpacePirateFrame15,
    /// <summary><c>space_pirate_frame_16</c>, an installed program frame.</summary>
    SpacePirateFrame16,
    /// <summary><c>space_pirate_frame_17</c>, an installed program frame.</summary>
    SpacePirateFrame17,
    /// <summary><c>space_pirate_frame_18</c>, an installed program frame.</summary>
    SpacePirateFrame18,
    /// <summary><c>space_pirate_frame_19</c>, an installed program frame.</summary>
    SpacePirateFrame19,
    /// <summary><c>space_pirate_frame_20</c>, an installed program frame.</summary>
    SpacePirateFrame20,
    /// <summary><c>space_pirate_frame_21</c>, an installed program frame.</summary>
    SpacePirateFrame21,
    /// <summary><c>space_pirate_frame_22</c>, an installed program frame.</summary>
    SpacePirateFrame22,
    /// <summary><c>space_pirate_frame_23</c>, an installed program frame.</summary>
    SpacePirateFrame23,
    /// <summary><c>space_pirate_frame_24</c>, an installed program frame.</summary>
    SpacePirateFrame24,
    /// <summary><c>space_pirate_frame_25</c>, an installed program frame.</summary>
    SpacePirateFrame25,
    /// <summary><c>space_pirate_frame_26</c>, an installed program frame.</summary>
    SpacePirateFrame26,
    /// <summary><c>space_pirate_frame_27</c>, an installed program frame.</summary>
    SpacePirateFrame27,
    /// <summary><c>space_pirate_frame_28</c>, an installed program frame.</summary>
    SpacePirateFrame28,
    /// <summary><c>space_pirate_frame_29</c>, an installed program frame.</summary>
    SpacePirateFrame29,
    /// <summary><c>space_pirate_frame_30</c>, an installed program frame.</summary>
    SpacePirateFrame30,
    /// <summary><c>space_pirate_frame_31</c>, an installed program frame.</summary>
    SpacePirateFrame31,
    /// <summary><c>space_pirate_frame_32</c>, an installed program frame.</summary>
    SpacePirateFrame32,
    /// <summary><c>space_pirate_frame_33</c>, an installed program frame.</summary>
    SpacePirateFrame33,
    /// <summary><c>space_pirate_frame_34</c>, an installed program frame.</summary>
    SpacePirateFrame34,
    /// <summary><c>space_pirate_frame_35</c>, an installed program frame.</summary>
    SpacePirateFrame35,
    /// <summary><c>space_pirate_frame_36</c>, an installed program frame.</summary>
    SpacePirateFrame36,
    /// <summary><c>space_pirate_frame_37</c>, an installed program frame.</summary>
    SpacePirateFrame37,
    /// <summary><c>space_pirate_frame_38</c>, an installed program frame.</summary>
    SpacePirateFrame38,
    /// <summary><c>space_pirate_frame_39</c>, an installed program frame.</summary>
    SpacePirateFrame39,
    /// <summary><c>space_pirate_frame_40</c>, an installed program frame.</summary>
    SpacePirateFrame40,
    /// <summary><c>space_pirate_frame_41</c>, an installed program frame.</summary>
    SpacePirateFrame41,
    /// <summary><c>gunship_dust_frame_00</c>, an installed program frame.</summary>
    GunshipDustFrame00,
    /// <summary><c>gunship_dust_frame_01</c>, an installed program frame.</summary>
    GunshipDustFrame01,
    /// <summary><c>gunship_dust_frame_02</c>, an installed program frame.</summary>
    GunshipDustFrame02,
    /// <summary><c>gunship_dust_frame_03</c>, an installed program frame.</summary>
    GunshipDustFrame03,
    /// <summary><c>gunship_dust_frame_04</c>, an installed program frame.</summary>
    GunshipDustFrame04,
    /// <summary><c>gunship_dust_frame_05</c>, an installed program frame.</summary>
    GunshipDustFrame05,
    /// <summary><c>gunship_dust_frame_06</c>, an installed program frame.</summary>
    GunshipDustFrame06,
    /// <summary><c>gunship_dust_frame_07</c>, an installed program frame.</summary>
    GunshipDustFrame07,
    /// <summary><c>gunship_dust_frame_08</c>, an installed program frame.</summary>
    GunshipDustFrame08,
    /// <summary><c>gunship_dust_frame_09</c>, an installed program frame.</summary>
    GunshipDustFrame09,
    /// <summary><c>gunship_dust_frame_10</c>, an installed program frame.</summary>
    GunshipDustFrame10,
    /// <summary><c>gunship_dust_frame_11</c>, an installed program frame.</summary>
    GunshipDustFrame11,
    /// <summary><c>gunship_dust_frame_12</c>, an installed program frame.</summary>
    GunshipDustFrame12,
    /// <summary><c>gunship_dust_frame_13</c>, an installed program frame.</summary>
    GunshipDustFrame13,
    /// <summary><c>gunship_dust_frame_14</c>, an installed program frame.</summary>
    GunshipDustFrame14,
    /// <summary><c>gunship_dust_frame_15</c>, an installed program frame.</summary>
    GunshipDustFrame15,
    /// <summary><c>gunship_dust_frame_16</c>, an installed program frame.</summary>
    GunshipDustFrame16,
    /// <summary><c>gunship_dust_frame_17</c>, an installed program frame.</summary>
    GunshipDustFrame17,
    /// <summary><c>gunship_dust_frame_18</c>, an installed program frame.</summary>
    GunshipDustFrame18,
    /// <summary><c>gunship_dust_frame_19</c>, an installed program frame.</summary>
    GunshipDustFrame19,
    /// <summary><c>gunship_dust_frame_20</c>, an installed program frame.</summary>
    GunshipDustFrame20,
    /// <summary><c>gunship_dust_frame_21</c>, an installed program frame.</summary>
    GunshipDustFrame21,
    /// <summary><c>gunship_dust_frame_22</c>, an installed program frame.</summary>
    GunshipDustFrame22,
    /// <summary><c>gunship_dust_frame_23</c>, an installed program frame.</summary>
    GunshipDustFrame23,
    /// <summary><c>gunship_dust_frame_24</c>, an installed program frame.</summary>
    GunshipDustFrame24,
    /// <summary><c>gunship_dust_frame_25</c>, an installed program frame.</summary>
    GunshipDustFrame25,
    /// <summary><c>gunship_dust_frame_26</c>, an installed program frame.</summary>
    GunshipDustFrame26,
    /// <summary><c>gunship_dust_frame_27</c>, an installed program frame.</summary>
    GunshipDustFrame27,
    /// <summary><c>gunship_dust_frame_28</c>, an installed program frame.</summary>
    GunshipDustFrame28,
    /// <summary><c>gunship_dust_frame_29</c>, an installed program frame.</summary>
    GunshipDustFrame29,
    /// <summary><c>gunship_dust_frame_30</c>, an installed program frame.</summary>
    GunshipDustFrame30,
    /// <summary><c>gunship_dust_frame_31</c>, an installed program frame.</summary>
    GunshipDustFrame31,
    /// <summary><c>gunship_dust_frame_32</c>, an installed program frame.</summary>
    GunshipDustFrame32,
    /// <summary><c>gunship_dust_frame_33</c>, an installed program frame.</summary>
    GunshipDustFrame33,
    /// <summary><c>gunship_dust_frame_34</c>, an installed program frame.</summary>
    GunshipDustFrame34,
    /// <summary><c>gunship_dust_frame_35</c>, an installed program frame.</summary>
    GunshipDustFrame35,
    /// <summary><c>gunship_dust_frame_36</c>, an installed program frame.</summary>
    GunshipDustFrame36,
    /// <summary><c>gunship_dust_frame_37</c>, an installed program frame.</summary>
    GunshipDustFrame37,
    /// <summary><c>gunship_dust_frame_38</c>, an installed program frame.</summary>
    GunshipDustFrame38,
    /// <summary><c>gunship_dust_frame_39</c>, an installed program frame.</summary>
    GunshipDustFrame39,
    /// <summary><c>gunship_dust_frame_40</c>, an installed program frame.</summary>
    GunshipDustFrame40,
    /// <summary><c>gunship_dust_frame_41</c>, an installed program frame.</summary>
    GunshipDustFrame41,
    /// <summary><c>gunship_dust_frame_42</c>, an installed program frame.</summary>
    GunshipDustFrame42,
    /// <summary><c>gunship_dust_frame_43</c>, an installed program frame.</summary>
    GunshipDustFrame43,
    /// <summary><c>gunship_dust_frame_44</c>, an installed program frame.</summary>
    GunshipDustFrame44,
    /// <summary><c>gunship_dust_frame_45</c>, an installed program frame.</summary>
    GunshipDustFrame45,
    /// <summary><c>torizo_explosion_frame_00</c>, an installed program frame.</summary>
    TorizoExplosionFrame00,
    /// <summary><c>torizo_explosion_frame_01</c>, an installed program frame.</summary>
    TorizoExplosionFrame01,
    /// <summary><c>torizo_explosion_frame_02</c>, an installed program frame.</summary>
    TorizoExplosionFrame02,
    /// <summary><c>torizo_explosion_frame_03</c>, an installed program frame.</summary>
    TorizoExplosionFrame03,
    /// <summary><c>torizo_explosion_frame_04</c>, an installed program frame.</summary>
    TorizoExplosionFrame04,
    /// <summary><c>torizo_explosion_frame_05</c>, an installed program frame.</summary>
    TorizoExplosionFrame05,
    /// <summary><c>torizo_explosion_frame_06</c>, an installed program frame.</summary>
    TorizoExplosionFrame06,
    /// <summary><c>torizo_explosion_frame_07</c>, an installed program frame.</summary>
    TorizoExplosionFrame07,
    /// <summary><c>torizo_explosion_frame_08</c>, an installed program frame.</summary>
    TorizoExplosionFrame08,
    /// <summary><c>torizo_explosion_frame_09</c>, an installed program frame.</summary>
    TorizoExplosionFrame09,
    /// <summary><c>torizo_explosion_frame_10</c>, an installed program frame.</summary>
    TorizoExplosionFrame10,
    /// <summary><c>torizo_explosion_frame_11</c>, an installed program frame.</summary>
    TorizoExplosionFrame11,
    /// <summary><c>torizo_explosion_frame_12</c>, an installed program frame.</summary>
    TorizoExplosionFrame12,
    /// <summary><c>torizo_explosion_frame_13</c>, an installed program frame.</summary>
    TorizoExplosionFrame13,
    /// <summary><c>torizo_explosion_frame_14</c>, an installed program frame.</summary>
    TorizoExplosionFrame14,
    /// <summary><c>torizo_drool_frame_00</c>, an installed program frame.</summary>
    TorizoDroolFrame00,
    /// <summary><c>torizo_drool_frame_01</c>, an installed program frame.</summary>
    TorizoDroolFrame01,
    /// <summary><c>torizo_drool_frame_02</c>, an installed program frame.</summary>
    TorizoDroolFrame02,
    /// <summary><c>torizo_drool_frame_03</c>, an installed program frame.</summary>
    TorizoDroolFrame03,
    /// <summary><c>torizo_drool_frame_04</c>, an installed program frame.</summary>
    TorizoDroolFrame04,
    /// <summary><c>torizo_drool_frame_05</c>, an installed program frame.</summary>
    TorizoDroolFrame05,
    /// <summary><c>torizo_drool_frame_06</c>, an installed program frame.</summary>
    TorizoDroolFrame06,
    /// <summary><c>torizo_explosive_swipe_frame_00</c>, an installed program frame.</summary>
    TorizoExplosiveSwipeFrame00,
    /// <summary><c>torizo_explosive_swipe_frame_01</c>, an installed program frame.</summary>
    TorizoExplosiveSwipeFrame01,
    /// <summary><c>torizo_explosive_swipe_frame_02</c>, an installed program frame.</summary>
    TorizoExplosiveSwipeFrame02,
    /// <summary><c>torizo_explosive_swipe_frame_03</c>, an installed program frame.</summary>
    TorizoExplosiveSwipeFrame03,
    /// <summary><c>torizo_explosive_swipe_frame_04</c>, an installed program frame.</summary>
    TorizoExplosiveSwipeFrame04,
    /// <summary><c>bomb_torizo_statue_fragment_frame_00</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame00,
    /// <summary><c>bomb_torizo_statue_fragment_frame_01</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame01,
    /// <summary><c>bomb_torizo_statue_fragment_frame_02</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame02,
    /// <summary><c>bomb_torizo_statue_fragment_frame_03</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame03,
    /// <summary><c>bomb_torizo_statue_fragment_frame_04</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame04,
    /// <summary><c>bomb_torizo_statue_fragment_frame_05</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame05,
    /// <summary><c>bomb_torizo_statue_fragment_frame_06</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame06,
    /// <summary><c>bomb_torizo_statue_fragment_frame_07</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame07,
    /// <summary><c>bomb_torizo_statue_fragment_frame_08</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame08,
    /// <summary><c>bomb_torizo_statue_fragment_frame_09</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame09,
    /// <summary><c>bomb_torizo_statue_fragment_frame_10</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame10,
    /// <summary><c>bomb_torizo_statue_fragment_frame_11</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame11,
    /// <summary><c>bomb_torizo_statue_fragment_frame_12</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame12,
    /// <summary><c>bomb_torizo_statue_fragment_frame_13</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame13,
    /// <summary><c>bomb_torizo_statue_fragment_frame_14</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame14,
    /// <summary><c>bomb_torizo_statue_fragment_frame_15</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame15,
    /// <summary><c>bomb_torizo_statue_fragment_frame_16</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame16,
    /// <summary><c>bomb_torizo_statue_fragment_frame_17</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame17,
    /// <summary><c>bomb_torizo_statue_fragment_frame_18</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame18,
    /// <summary><c>bomb_torizo_statue_fragment_frame_19</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame19,
    /// <summary><c>bomb_torizo_statue_fragment_frame_20</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame20,
    /// <summary><c>bomb_torizo_statue_fragment_frame_21</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame21,
    /// <summary><c>bomb_torizo_statue_fragment_frame_22</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame22,
    /// <summary><c>bomb_torizo_statue_fragment_frame_23</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame23,
    /// <summary><c>bomb_torizo_statue_fragment_frame_24</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame24,
    /// <summary><c>bomb_torizo_statue_fragment_frame_25</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame25,
    /// <summary><c>bomb_torizo_statue_fragment_frame_26</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame26,
    /// <summary><c>bomb_torizo_statue_fragment_frame_27</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame27,
    /// <summary><c>bomb_torizo_statue_fragment_frame_28</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame28,
    /// <summary><c>bomb_torizo_statue_fragment_frame_29</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame29,
    /// <summary><c>bomb_torizo_statue_fragment_frame_30</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame30,
    /// <summary><c>bomb_torizo_statue_fragment_frame_31</c>, an installed program frame.</summary>
    BombTorizoStatueFragmentFrame31,
    /// <summary><c>torizo_chozo_orb_frame_00</c>, an installed program frame.</summary>
    TorizoChozoOrbFrame00,
    /// <summary><c>torizo_chozo_orb_frame_01</c>, an installed program frame.</summary>
    TorizoChozoOrbFrame01,
    /// <summary><c>torizo_chozo_orb_frame_02</c>, an installed program frame.</summary>
    TorizoChozoOrbFrame02,
    /// <summary><c>torizo_chozo_orb_frame_03</c>, an installed program frame.</summary>
    TorizoChozoOrbFrame03,
    /// <summary><c>torizo_chozo_orb_frame_04</c>, an installed program frame.</summary>
    TorizoChozoOrbFrame04,
    /// <summary><c>torizo_chozo_orb_frame_05</c>, an installed program frame.</summary>
    TorizoChozoOrbFrame05,
    /// <summary><c>torizo_chozo_orb_frame_06</c>, an installed program frame.</summary>
    TorizoChozoOrbFrame06,
    /// <summary><c>torizo_chozo_orb_frame_07</c>, an installed program frame.</summary>
    TorizoChozoOrbFrame07,
    /// <summary><c>torizo_chozo_orb_frame_08</c>, an installed program frame.</summary>
    TorizoChozoOrbFrame08,
    /// <summary><c>torizo_chozo_orb_frame_09</c>, an installed program frame.</summary>
    TorizoChozoOrbFrame09,
    /// <summary><c>torizo_chozo_orb_frame_10</c>, an installed program frame.</summary>
    TorizoChozoOrbFrame10,
    /// <summary><c>torizo_chozo_orb_frame_11</c>, an installed program frame.</summary>
    TorizoChozoOrbFrame11,
    /// <summary><c>torizo_chozo_orb_frame_12</c>, an installed program frame.</summary>
    TorizoChozoOrbFrame12,
    /// <summary><c>torizo_chozo_orb_frame_13</c>, an installed program frame.</summary>
    TorizoChozoOrbFrame13,
    /// <summary><c>torizo_chozo_orb_frame_14</c>, an installed program frame.</summary>
    TorizoChozoOrbFrame14,
    /// <summary><c>torizo_chozo_orb_frame_15</c>, an installed program frame.</summary>
    TorizoChozoOrbFrame15,
    /// <summary><c>torizo_chozo_orb_frame_16</c>, an installed program frame.</summary>
    TorizoChozoOrbFrame16,
    /// <summary><c>torizo_chozo_orb_frame_17</c>, an installed program frame.</summary>
    TorizoChozoOrbFrame17,
    /// <summary><c>torizo_sonic_boom_frame_00</c>, an installed program frame.</summary>
    TorizoSonicBoomFrame00,
    /// <summary><c>torizo_sonic_boom_frame_01</c>, an installed program frame.</summary>
    TorizoSonicBoomFrame01,
    /// <summary><c>torizo_sonic_boom_frame_02</c>, an installed program frame.</summary>
    TorizoSonicBoomFrame02,
    /// <summary><c>torizo_sonic_boom_frame_03</c>, an installed program frame.</summary>
    TorizoSonicBoomFrame03,
    /// <summary><c>torizo_sonic_boom_frame_04</c>, an installed program frame.</summary>
    TorizoSonicBoomFrame04,
    /// <summary><c>torizo_sonic_boom_frame_05</c>, an installed program frame.</summary>
    TorizoSonicBoomFrame05,
    /// <summary><c>torizo_sonic_boom_frame_06</c>, an installed program frame.</summary>
    TorizoSonicBoomFrame06,
    /// <summary><c>torizo_sonic_boom_frame_07</c>, an installed program frame.</summary>
    TorizoSonicBoomFrame07,
    /// <summary><c>torizo_sonic_boom_frame_08</c>, an installed program frame.</summary>
    TorizoSonicBoomFrame08,
    /// <summary><c>torizo_sonic_boom_frame_09</c>, an installed program frame.</summary>
    TorizoSonicBoomFrame09,
    /// <summary><c>torizo_sonic_boom_frame_10</c>, an installed program frame.</summary>
    TorizoSonicBoomFrame10,
    /// <summary><c>chozo_tourian_dust_frame_00</c>, an installed program frame.</summary>
    ChozoTourianDustFrame00,
    /// <summary><c>chozo_tourian_dust_frame_01</c>, an installed program frame.</summary>
    ChozoTourianDustFrame01,
    /// <summary><c>chozo_tourian_dust_frame_02</c>, an installed program frame.</summary>
    ChozoTourianDustFrame02,
    /// <summary><c>chozo_tourian_dust_frame_03</c>, an installed program frame.</summary>
    ChozoTourianDustFrame03,
    /// <summary><c>chozo_tourian_dust_frame_04</c>, an installed program frame.</summary>
    ChozoTourianDustFrame04,
    /// <summary><c>chozo_tourian_dust_frame_05</c>, an installed program frame.</summary>
    ChozoTourianDustFrame05,
    /// <summary><c>chozo_tourian_dust_frame_06</c>, an installed program frame.</summary>
    ChozoTourianDustFrame06,
    /// <summary><c>chozo_tourian_dust_frame_07</c>, an installed program frame.</summary>
    ChozoTourianDustFrame07,
    /// <summary><c>chozo_tourian_dust_frame_08</c>, an installed program frame.</summary>
    ChozoTourianDustFrame08,
    /// <summary><c>chozo_tourian_dust_frame_09</c>, an installed program frame.</summary>
    ChozoTourianDustFrame09,
    /// <summary><c>chozo_tourian_dust_frame_10</c>, an installed program frame.</summary>
    ChozoTourianDustFrame10,
    /// <summary><c>chozo_tourian_dust_frame_11</c>, an installed program frame.</summary>
    ChozoTourianDustFrame11,
    /// <summary><c>chozo_tourian_dust_frame_12</c>, an installed program frame.</summary>
    ChozoTourianDustFrame12,
    /// <summary><c>chozo_tourian_dust_frame_13</c>, an installed program frame.</summary>
    ChozoTourianDustFrame13,
    /// <summary><c>torizo_landing_dust_frame_00</c>, an installed program frame.</summary>
    TorizoLandingDustFrame00,
    /// <summary><c>torizo_landing_dust_frame_01</c>, an installed program frame.</summary>
    TorizoLandingDustFrame01,
    /// <summary><c>torizo_landing_dust_frame_02</c>, an installed program frame.</summary>
    TorizoLandingDustFrame02,
    /// <summary><c>torizo_landing_dust_frame_03</c>, an installed program frame.</summary>
    TorizoLandingDustFrame03,
    /// <summary><c>torizo_landing_dust_frame_04</c>, an installed program frame.</summary>
    TorizoLandingDustFrame04,
    /// <summary><c>torizo_landing_dust_frame_05</c>, an installed program frame.</summary>
    TorizoLandingDustFrame05,
    /// <summary><c>torizo_landing_dust_frame_06</c>, an installed program frame.</summary>
    TorizoLandingDustFrame06,
    /// <summary><c>torizo_landing_dust_frame_07</c>, an installed program frame.</summary>
    TorizoLandingDustFrame07,
    /// <summary><c>golden_torizo_egg_frame_00</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame00,
    /// <summary><c>golden_torizo_egg_frame_01</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame01,
    /// <summary><c>golden_torizo_egg_frame_02</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame02,
    /// <summary><c>golden_torizo_egg_frame_03</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame03,
    /// <summary><c>golden_torizo_egg_frame_04</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame04,
    /// <summary><c>golden_torizo_egg_frame_05</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame05,
    /// <summary><c>golden_torizo_egg_frame_06</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame06,
    /// <summary><c>golden_torizo_egg_frame_07</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame07,
    /// <summary><c>golden_torizo_egg_frame_08</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame08,
    /// <summary><c>golden_torizo_egg_frame_09</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame09,
    /// <summary><c>golden_torizo_egg_frame_10</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame10,
    /// <summary><c>golden_torizo_egg_frame_11</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame11,
    /// <summary><c>golden_torizo_egg_frame_12</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame12,
    /// <summary><c>golden_torizo_egg_frame_13</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame13,
    /// <summary><c>golden_torizo_egg_frame_14</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame14,
    /// <summary><c>golden_torizo_egg_frame_15</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame15,
    /// <summary><c>golden_torizo_egg_frame_16</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame16,
    /// <summary><c>golden_torizo_egg_frame_17</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame17,
    /// <summary><c>golden_torizo_egg_frame_18</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame18,
    /// <summary><c>golden_torizo_egg_frame_19</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame19,
    /// <summary><c>golden_torizo_egg_frame_20</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame20,
    /// <summary><c>golden_torizo_egg_frame_21</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame21,
    /// <summary><c>golden_torizo_egg_frame_22</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame22,
    /// <summary><c>golden_torizo_egg_frame_23</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame23,
    /// <summary><c>golden_torizo_egg_frame_24</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame24,
    /// <summary><c>golden_torizo_egg_frame_25</c>, an installed program frame.</summary>
    GoldenTorizoEggFrame25,
    /// <summary><c>golden_torizo_super_missile_frame_00</c>, an installed program frame.</summary>
    GoldenTorizoSuperMissileFrame00,
    /// <summary><c>golden_torizo_super_missile_frame_01</c>, an installed program frame.</summary>
    GoldenTorizoSuperMissileFrame01,
    /// <summary><c>golden_torizo_super_missile_frame_02</c>, an installed program frame.</summary>
    GoldenTorizoSuperMissileFrame02,
    /// <summary><c>golden_torizo_super_missile_frame_03</c>, an installed program frame.</summary>
    GoldenTorizoSuperMissileFrame03,
    /// <summary><c>golden_torizo_super_missile_frame_04</c>, an installed program frame.</summary>
    GoldenTorizoSuperMissileFrame04,
    /// <summary><c>golden_torizo_super_missile_frame_05</c>, an installed program frame.</summary>
    GoldenTorizoSuperMissileFrame05,
    /// <summary><c>golden_torizo_super_missile_frame_06</c>, an installed program frame.</summary>
    GoldenTorizoSuperMissileFrame06,
    /// <summary><c>golden_torizo_super_missile_frame_07</c>, an installed program frame.</summary>
    GoldenTorizoSuperMissileFrame07,
    /// <summary><c>golden_torizo_super_missile_frame_08</c>, an installed program frame.</summary>
    GoldenTorizoSuperMissileFrame08,
    /// <summary><c>golden_torizo_super_missile_frame_09</c>, an installed program frame.</summary>
    GoldenTorizoSuperMissileFrame09,
    /// <summary><c>golden_torizo_super_missile_frame_10</c>, an installed program frame.</summary>
    GoldenTorizoSuperMissileFrame10,
    /// <summary><c>golden_torizo_super_missile_frame_11</c>, an installed program frame.</summary>
    GoldenTorizoSuperMissileFrame11,
    /// <summary><c>golden_torizo_super_missile_frame_12</c>, an installed program frame.</summary>
    GoldenTorizoSuperMissileFrame12,
    /// <summary><c>golden_torizo_super_missile_frame_13</c>, an installed program frame.</summary>
    GoldenTorizoSuperMissileFrame13,
    /// <summary><c>golden_torizo_super_missile_frame_14</c>, an installed program frame.</summary>
    GoldenTorizoSuperMissileFrame14,
    /// <summary><c>golden_torizo_super_missile_frame_15</c>, an installed program frame.</summary>
    GoldenTorizoSuperMissileFrame15,
    /// <summary><c>golden_torizo_super_missile_frame_16</c>, an installed program frame.</summary>
    GoldenTorizoSuperMissileFrame16,
    /// <summary><c>golden_torizo_super_missile_frame_17</c>, an installed program frame.</summary>
    GoldenTorizoSuperMissileFrame17,
    /// <summary><c>golden_torizo_super_missile_frame_18</c>, an installed program frame.</summary>
    GoldenTorizoSuperMissileFrame18,
    /// <summary><c>golden_torizo_super_missile_frame_19</c>, an installed program frame.</summary>
    GoldenTorizoSuperMissileFrame19,
    /// <summary><c>golden_torizo_super_missile_frame_20</c>, an installed program frame.</summary>
    GoldenTorizoSuperMissileFrame20,
    /// <summary><c>golden_torizo_super_missile_frame_21</c>, an installed program frame.</summary>
    GoldenTorizoSuperMissileFrame21,
    /// <summary><c>golden_torizo_super_missile_frame_22</c>, an installed program frame.</summary>
    GoldenTorizoSuperMissileFrame22,
    /// <summary><c>golden_torizo_super_missile_frame_23</c>, an installed program frame.</summary>
    GoldenTorizoSuperMissileFrame23,
    /// <summary><c>golden_torizo_eye_beam_frame_00</c>, an installed program frame.</summary>
    GoldenTorizoEyeBeamFrame00,
    /// <summary><c>golden_torizo_eye_beam_frame_01</c>, an installed program frame.</summary>
    GoldenTorizoEyeBeamFrame01,
    /// <summary><c>golden_torizo_eye_beam_frame_02</c>, an installed program frame.</summary>
    GoldenTorizoEyeBeamFrame02,
    /// <summary><c>golden_torizo_eye_beam_frame_03</c>, an installed program frame.</summary>
    GoldenTorizoEyeBeamFrame03,
    /// <summary><c>golden_torizo_eye_beam_frame_04</c>, an installed program frame.</summary>
    GoldenTorizoEyeBeamFrame04,
    /// <summary><c>golden_torizo_eye_beam_frame_05</c>, an installed program frame.</summary>
    GoldenTorizoEyeBeamFrame05,
    /// <summary><c>golden_torizo_eye_beam_frame_06</c>, an installed program frame.</summary>
    GoldenTorizoEyeBeamFrame06,
    /// <summary><c>golden_torizo_eye_beam_frame_07</c>, an installed program frame.</summary>
    GoldenTorizoEyeBeamFrame07,
    /// <summary><c>golden_torizo_eye_beam_frame_08</c>, an installed program frame.</summary>
    GoldenTorizoEyeBeamFrame08,
    /// <summary><c>golden_torizo_eye_beam_frame_09</c>, an installed program frame.</summary>
    GoldenTorizoEyeBeamFrame09,
    /// <summary><c>golden_torizo_eye_beam_frame_10</c>, an installed program frame.</summary>
    GoldenTorizoEyeBeamFrame10,
    /// <summary><c>golden_torizo_eye_beam_frame_11</c>, an installed program frame.</summary>
    GoldenTorizoEyeBeamFrame11,
    /// <summary><c>golden_torizo_eye_beam_frame_12</c>, an installed program frame.</summary>
    GoldenTorizoEyeBeamFrame12,
    /// <summary><c>golden_torizo_eye_beam_frame_13</c>, an installed program frame.</summary>
    GoldenTorizoEyeBeamFrame13,
    /// <summary><c>golden_torizo_eye_beam_frame_14</c>, an installed program frame.</summary>
    GoldenTorizoEyeBeamFrame14,
    /// <summary><c>golden_torizo_eye_beam_frame_15</c>, an installed program frame.</summary>
    GoldenTorizoEyeBeamFrame15,
    /// <summary><c>golden_torizo_eye_beam_frame_16</c>, an installed program frame.</summary>
    GoldenTorizoEyeBeamFrame16,
    /// <summary><c>old_tourian_escape_wall_explosion_frame_00</c>, an installed program frame.</summary>
    OldTourianEscapeWallExplosionFrame00,
    /// <summary><c>old_tourian_escape_wall_explosion_frame_01</c>, an installed program frame.</summary>
    OldTourianEscapeWallExplosionFrame01,
    /// <summary><c>old_tourian_escape_wall_explosion_frame_02</c>, an installed program frame.</summary>
    OldTourianEscapeWallExplosionFrame02,
    /// <summary><c>old_tourian_escape_wall_explosion_frame_03</c>, an installed program frame.</summary>
    OldTourianEscapeWallExplosionFrame03,
    /// <summary><c>old_tourian_escape_wall_explosion_frame_04</c>, an installed program frame.</summary>
    OldTourianEscapeWallExplosionFrame04,
    /// <summary><c>old_tourian_escape_wall_explosion_frame_05</c>, an installed program frame.</summary>
    OldTourianEscapeWallExplosionFrame05,
    /// <summary><c>dragon_fireball_frame_00</c>, an installed program frame.</summary>
    DragonFireballFrame00,
    /// <summary><c>dragon_fireball_frame_01</c>, an installed program frame.</summary>
    DragonFireballFrame01,
    /// <summary><c>dragon_fireball_frame_02</c>, an installed program frame.</summary>
    DragonFireballFrame02,
    /// <summary><c>dragon_fireball_frame_03</c>, an installed program frame.</summary>
    DragonFireballFrame03,
    /// <summary><c>dragon_fireball_frame_04</c>, an installed program frame.</summary>
    DragonFireballFrame04,
    /// <summary><c>dragon_fireball_frame_05</c>, an installed program frame.</summary>
    DragonFireballFrame05,
    /// <summary><c>dragon_fireball_frame_06</c>, an installed program frame.</summary>
    DragonFireballFrame06,
    /// <summary><c>dragon_fireball_frame_07</c>, an installed program frame.</summary>
    DragonFireballFrame07,
    /// <summary><c>eye_door_frame_00</c>, an installed program frame.</summary>
    EyeDoorFrame00,
    /// <summary><c>eye_door_frame_01</c>, an installed program frame.</summary>
    EyeDoorFrame01,
    /// <summary><c>eye_door_frame_02</c>, an installed program frame.</summary>
    EyeDoorFrame02,
    /// <summary><c>eye_door_frame_03</c>, an installed program frame.</summary>
    EyeDoorFrame03,
    /// <summary><c>eye_door_frame_04</c>, an installed program frame.</summary>
    EyeDoorFrame04,
    /// <summary><c>eye_door_frame_05</c>, an installed program frame.</summary>
    EyeDoorFrame05,
    /// <summary><c>eye_door_frame_06</c>, an installed program frame.</summary>
    EyeDoorFrame06,
    /// <summary><c>eye_door_frame_07</c>, an installed program frame.</summary>
    EyeDoorFrame07,
    /// <summary><c>eye_door_frame_08</c>, an installed program frame.</summary>
    EyeDoorFrame08,
    /// <summary><c>eye_door_frame_09</c>, an installed program frame.</summary>
    EyeDoorFrame09,
    /// <summary><c>eye_door_frame_10</c>, an installed program frame.</summary>
    EyeDoorFrame10,
    /// <summary><c>eye_door_sweat_frame_00</c>, an installed program frame.</summary>
    EyeDoorSweatFrame00,
    /// <summary><c>eye_door_sweat_frame_01</c>, an installed program frame.</summary>
    EyeDoorSweatFrame01,
    /// <summary><c>eye_door_sweat_frame_02</c>, an installed program frame.</summary>
    EyeDoorSweatFrame02,
    /// <summary><c>eye_door_sweat_frame_03</c>, an installed program frame.</summary>
    EyeDoorSweatFrame03,
    /// <summary><c>tourian_statue_frame_00</c>, an installed program frame.</summary>
    TourianStatueFrame00,
    /// <summary><c>tourian_statue_frame_01</c>, an installed program frame.</summary>
    TourianStatueFrame01,
    /// <summary><c>tourian_statue_frame_02</c>, an installed program frame.</summary>
    TourianStatueFrame02,
    /// <summary><c>tourian_statue_frame_03</c>, an installed program frame.</summary>
    TourianStatueFrame03,
    /// <summary><c>tourian_statue_frame_04</c>, an installed program frame.</summary>
    TourianStatueFrame04,
    /// <summary><c>tourian_statue_frame_05</c>, an installed program frame.</summary>
    TourianStatueFrame05,
    /// <summary><c>tourian_statue_frame_06</c>, an installed program frame.</summary>
    TourianStatueFrame06,
    /// <summary><c>tourian_statue_frame_07</c>, an installed program frame.</summary>
    TourianStatueFrame07,
    /// <summary><c>tourian_statue_frame_08</c>, an installed program frame.</summary>
    TourianStatueFrame08,
    /// <summary><c>tourian_statue_frame_09</c>, an installed program frame.</summary>
    TourianStatueFrame09,
    /// <summary><c>tourian_statue_frame_10</c>, an installed program frame.</summary>
    TourianStatueFrame10,
    /// <summary><c>tourian_statue_frame_11</c>, an installed program frame.</summary>
    TourianStatueFrame11,
    /// <summary><c>tourian_statue_frame_12</c>, an installed program frame.</summary>
    TourianStatueFrame12,
    /// <summary><c>tourian_statue_frame_13</c>, an installed program frame.</summary>
    TourianStatueFrame13,
    /// <summary><c>tourian_statue_frame_14</c>, an installed program frame.</summary>
    TourianStatueFrame14,
    /// <summary><c>tourian_statue_frame_15</c>, an installed program frame.</summary>
    TourianStatueFrame15,
    /// <summary><c>tourian_statue_frame_16</c>, an installed program frame.</summary>
    TourianStatueFrame16,
    /// <summary><c>tourian_statue_frame_17</c>, an installed program frame.</summary>
    TourianStatueFrame17,
    /// <summary><c>tourian_statue_frame_18</c>, an installed program frame.</summary>
    TourianStatueFrame18,
    /// <summary><c>tourian_statue_frame_19</c>, an installed program frame.</summary>
    TourianStatueFrame19,
    /// <summary><c>tourian_statue_frame_20</c>, an installed program frame.</summary>
    TourianStatueFrame20,
    /// <summary><c>tourian_statue_frame_21</c>, an installed program frame.</summary>
    TourianStatueFrame21,
    /// <summary><c>tourian_statue_frame_22</c>, an installed program frame.</summary>
    TourianStatueFrame22,
    /// <summary><c>tourian_statue_frame_23</c>, an installed program frame.</summary>
    TourianStatueFrame23,
    /// <summary><c>tourian_statue_frame_24</c>, an installed program frame.</summary>
    TourianStatueFrame24,
    /// <summary><c>tourian_statue_frame_25</c>, an installed program frame.</summary>
    TourianStatueFrame25,
    /// <summary><c>tourian_statue_frame_26</c>, an installed program frame.</summary>
    TourianStatueFrame26,
    /// <summary><c>tourian_statue_frame_27</c>, an installed program frame.</summary>
    TourianStatueFrame27,
    /// <summary><c>nuclear_waffle_frame_00</c>, an installed program frame.</summary>
    NuclearWaffleFrame00,
    /// <summary><c>nuclear_waffle_frame_01</c>, an installed program frame.</summary>
    NuclearWaffleFrame01,
    /// <summary><c>nuclear_waffle_frame_02</c>, an installed program frame.</summary>
    NuclearWaffleFrame02,
    /// <summary><c>nuclear_waffle_frame_03</c>, an installed program frame.</summary>
    NuclearWaffleFrame03,
    /// <summary><c>nuclear_waffle_frame_04</c>, an installed program frame.</summary>
    NuclearWaffleFrame04,
    /// <summary><c>nuclear_waffle_frame_05</c>, an installed program frame.</summary>
    NuclearWaffleFrame05,
    /// <summary><c>nuclear_waffle_frame_06</c>, an installed program frame.</summary>
    NuclearWaffleFrame06,
    /// <summary><c>nuclear_waffle_frame_07</c>, an installed program frame.</summary>
    NuclearWaffleFrame07,
    /// <summary><c>nuclear_waffle_frame_08</c>, an installed program frame.</summary>
    NuclearWaffleFrame08,
    /// <summary><c>nuclear_waffle_frame_09</c>, an installed program frame.</summary>
    NuclearWaffleFrame09,
    /// <summary><c>nuclear_waffle_frame_10</c>, an installed program frame.</summary>
    NuclearWaffleFrame10,
    /// <summary><c>nuclear_waffle_frame_11</c>, an installed program frame.</summary>
    NuclearWaffleFrame11,
    /// <summary><c>polyp_rock_00</c>, an installed program frame.</summary>
    PolypRock00,
    /// <summary><c>shaktool_frame_00</c>, an installed program frame.</summary>
    ShaktoolFrame00,
    /// <summary><c>shaktool_frame_01</c>, an installed program frame.</summary>
    ShaktoolFrame01,
    /// <summary><c>shaktool_frame_02</c>, an installed program frame.</summary>
    ShaktoolFrame02,
    /// <summary><c>shaktool_frame_03</c>, an installed program frame.</summary>
    ShaktoolFrame03,
    /// <summary><c>shaktool_frame_04</c>, an installed program frame.</summary>
    ShaktoolFrame04,
    /// <summary><c>shaktool_frame_05</c>, an installed program frame.</summary>
    ShaktoolFrame05,
    /// <summary><c>shaktool_frame_06</c>, an installed program frame.</summary>
    ShaktoolFrame06,
    /// <summary><c>shaktool_frame_07</c>, an installed program frame.</summary>
    ShaktoolFrame07,
    /// <summary><c>mother_brain_turret_frame_00</c>, an installed program frame.</summary>
    MotherBrainTurretFrame00,
    /// <summary><c>mother_brain_turret_frame_01</c>, an installed program frame.</summary>
    MotherBrainTurretFrame01,
    /// <summary><c>mother_brain_turret_frame_02</c>, an installed program frame.</summary>
    MotherBrainTurretFrame02,
    /// <summary><c>mother_brain_turret_frame_03</c>, an installed program frame.</summary>
    MotherBrainTurretFrame03,
    /// <summary><c>mother_brain_turret_frame_04</c>, an installed program frame.</summary>
    MotherBrainTurretFrame04,
    /// <summary><c>mother_brain_turret_frame_05</c>, an installed program frame.</summary>
    MotherBrainTurretFrame05,
    /// <summary><c>mother_brain_turret_frame_06</c>, an installed program frame.</summary>
    MotherBrainTurretFrame06,
    /// <summary><c>mother_brain_turret_frame_07</c>, an installed program frame.</summary>
    MotherBrainTurretFrame07,
    /// <summary><c>mother_brain_turret_frame_08</c>, an installed program frame.</summary>
    MotherBrainTurretFrame08,
    /// <summary><c>mother_brain_turret_frame_09</c>, an installed program frame.</summary>
    MotherBrainTurretFrame09,
    /// <summary><c>mother_brain_turret_frame_10</c>, an installed program frame.</summary>
    MotherBrainTurretFrame10,
    /// <summary><c>mother_brain_turret_frame_11</c>, an installed program frame.</summary>
    MotherBrainTurretFrame11,
    /// <summary><c>mother_brain_turret_frame_12</c>, an installed program frame.</summary>
    MotherBrainTurretFrame12,
    /// <summary><c>mother_brain_turret_frame_13</c>, an installed program frame.</summary>
    MotherBrainTurretFrame13,
    /// <summary><c>mother_brain_turret_frame_14</c>, an installed program frame.</summary>
    MotherBrainTurretFrame14,
    /// <summary><c>mother_brain_turret_frame_15</c>, an installed program frame.</summary>
    MotherBrainTurretFrame15,
    /// <summary><c>mother_brain_turret_frame_16</c>, an installed program frame.</summary>
    MotherBrainTurretFrame16,
    /// <summary><c>mother_brain_turret_frame_17</c>, an installed program frame.</summary>
    MotherBrainTurretFrame17,
    /// <summary><c>mother_brain_turret_frame_18</c>, an installed program frame.</summary>
    MotherBrainTurretFrame18,
    /// <summary><c>mother_brain_turret_frame_19</c>, an installed program frame.</summary>
    MotherBrainTurretFrame19,
    /// <summary><c>mother_brain_turret_frame_20</c>, an installed program frame.</summary>
    MotherBrainTurretFrame20,
    /// <summary><c>mother_brain_blue_ring_00</c>, an installed program frame.</summary>
    MotherBrainBlueRing00,
    /// <summary><c>mother_brain_blue_ring_01</c>, an installed program frame.</summary>
    MotherBrainBlueRing01,
    /// <summary><c>mother_brain_blue_ring_02</c>, an installed program frame.</summary>
    MotherBrainBlueRing02,
    /// <summary><c>mother_brain_blue_ring_03</c>, an installed program frame.</summary>
    MotherBrainBlueRing03,
    /// <summary><c>mother_brain_blue_ring_04</c>, an installed program frame.</summary>
    MotherBrainBlueRing04,
    /// <summary><c>mother_brain_blue_ring_05</c>, an installed program frame.</summary>
    MotherBrainBlueRing05,
    /// <summary><c>mother_brain_blue_ring_impact_00</c>, an installed program frame.</summary>
    MotherBrainBlueRingImpact00,
    /// <summary><c>mother_brain_blue_ring_impact_01</c>, an installed program frame.</summary>
    MotherBrainBlueRingImpact01,
    /// <summary><c>mother_brain_blue_ring_impact_02</c>, an installed program frame.</summary>
    MotherBrainBlueRingImpact02,
    /// <summary><c>mother_brain_blue_ring_impact_03</c>, an installed program frame.</summary>
    MotherBrainBlueRingImpact03,
    /// <summary><c>mother_brain_blue_ring_impact_04</c>, an installed program frame.</summary>
    MotherBrainBlueRingImpact04,
    /// <summary><c>mother_brain_blue_ring_impact_05</c>, an installed program frame.</summary>
    MotherBrainBlueRingImpact05,
    /// <summary><c>projectile_86_c76e_frame_00</c>, an installed program frame.</summary>
    Projectile86C76eFrame00,
    /// <summary><c>projectile_86_c76e_frame_01</c>, an installed program frame.</summary>
    Projectile86C76eFrame01,
    /// <summary><c>projectile_86_c76e_frame_02</c>, an installed program frame.</summary>
    Projectile86C76eFrame02,
    /// <summary><c>projectile_86_c76e_frame_03</c>, an installed program frame.</summary>
    Projectile86C76eFrame03,
    /// <summary><c>projectile_86_c76e_frame_04</c>, an installed program frame.</summary>
    Projectile86C76eFrame04,
    /// <summary><c>projectile_86_c76e_frame_05</c>, an installed program frame.</summary>
    Projectile86C76eFrame05,
    /// <summary><c>projectile_86_c76e_frame_06</c>, an installed program frame.</summary>
    Projectile86C76eFrame06,
    /// <summary><c>projectile_86_c76e_frame_07</c>, an installed program frame.</summary>
    Projectile86C76eFrame07,
    /// <summary><c>projectile_86_c76e_frame_08</c>, an installed program frame.</summary>
    Projectile86C76eFrame08,
    /// <summary><c>mother_brain_hand_beam_frame_00</c>, an installed program frame.</summary>
    MotherBrainHandBeamFrame00,
    /// <summary><c>mother_brain_hand_beam_frame_01</c>, an installed program frame.</summary>
    MotherBrainHandBeamFrame01,
    /// <summary><c>mother_brain_hand_beam_frame_02</c>, an installed program frame.</summary>
    MotherBrainHandBeamFrame02,
    /// <summary><c>mother_brain_hand_beam_frame_03</c>, an installed program frame.</summary>
    MotherBrainHandBeamFrame03,
    /// <summary><c>mother_brain_hand_beam_frame_04</c>, an installed program frame.</summary>
    MotherBrainHandBeamFrame04,
    /// <summary><c>mother_brain_hand_beam_frame_05</c>, an installed program frame.</summary>
    MotherBrainHandBeamFrame05,
    /// <summary><c>mother_brain_hand_beam_frame_06</c>, an installed program frame.</summary>
    MotherBrainHandBeamFrame06,
    /// <summary><c>mother_brain_hand_beam_frame_07</c>, an installed program frame.</summary>
    MotherBrainHandBeamFrame07,
    /// <summary><c>mother_brain_hand_beam_frame_08</c>, an installed program frame.</summary>
    MotherBrainHandBeamFrame08,
    /// <summary><c>mother_brain_hand_beam_frame_09</c>, an installed program frame.</summary>
    MotherBrainHandBeamFrame09,
    /// <summary><c>mother_brain_hand_beam_frame_10</c>, an installed program frame.</summary>
    MotherBrainHandBeamFrame10,
    /// <summary><c>mother_brain_hand_beam_frame_11</c>, an installed program frame.</summary>
    MotherBrainHandBeamFrame11,
    /// <summary><c>mother_brain_hand_beam_frame_12</c>, an installed program frame.</summary>
    MotherBrainHandBeamFrame12,
    /// <summary><c>mother_brain_hand_beam_frame_13</c>, an installed program frame.</summary>
    MotherBrainHandBeamFrame13,
    /// <summary><c>mother_brain_hand_beam_frame_14</c>, an installed program frame.</summary>
    MotherBrainHandBeamFrame14,
    /// <summary><c>mother_brain_hand_beam_frame_15</c>, an installed program frame.</summary>
    MotherBrainHandBeamFrame15,
    /// <summary><c>mother_brain_hand_beam_frame_16</c>, an installed program frame.</summary>
    MotherBrainHandBeamFrame16,
    /// <summary><c>mother_brain_hand_beam_frame_17</c>, an installed program frame.</summary>
    MotherBrainHandBeamFrame17,
    /// <summary><c>mother_brain_hand_beam_frame_18</c>, an installed program frame.</summary>
    MotherBrainHandBeamFrame18,
    /// <summary><c>mother_brain_hand_beam_frame_19</c>, an installed program frame.</summary>
    MotherBrainHandBeamFrame19,
    /// <summary><c>mother_brain_hand_beam_frame_20</c>, an installed program frame.</summary>
    MotherBrainHandBeamFrame20,
    /// <summary><c>projectile_86_c829_frame_00</c>, an installed program frame.</summary>
    Projectile86C829Frame00,
    /// <summary><c>projectile_86_c829_frame_01</c>, an installed program frame.</summary>
    Projectile86C829Frame01,
    /// <summary><c>projectile_86_c829_frame_02</c>, an installed program frame.</summary>
    Projectile86C829Frame02,
    /// <summary><c>projectile_86_c829_frame_03</c>, an installed program frame.</summary>
    Projectile86C829Frame03,
    /// <summary><c>projectile_86_c829_frame_04</c>, an installed program frame.</summary>
    Projectile86C829Frame04,
    /// <summary><c>projectile_86_c829_frame_05</c>, an installed program frame.</summary>
    Projectile86C829Frame05,
    /// <summary><c>mother_brain_drool_attached_00</c>, an installed program frame.</summary>
    MotherBrainDroolAttached00,
    /// <summary><c>mother_brain_drool_attached_01</c>, an installed program frame.</summary>
    MotherBrainDroolAttached01,
    /// <summary><c>mother_brain_drool_attached_02</c>, an installed program frame.</summary>
    MotherBrainDroolAttached02,
    /// <summary><c>mother_brain_drool_attached_03</c>, an installed program frame.</summary>
    MotherBrainDroolAttached03,
    /// <summary><c>mother_brain_drool_attached_04</c>, an installed program frame.</summary>
    MotherBrainDroolAttached04,
    /// <summary><c>mother_brain_drool_released</c>, an installed program frame.</summary>
    MotherBrainDroolReleased,
    /// <summary><c>projectile_86_c8e1_frame_00</c>, an installed program frame.</summary>
    Projectile86C8e1Frame00,
    /// <summary><c>projectile_86_c8e1_frame_01</c>, an installed program frame.</summary>
    Projectile86C8e1Frame01,
    /// <summary><c>projectile_86_c8e1_frame_02</c>, an installed program frame.</summary>
    Projectile86C8e1Frame02,
    /// <summary><c>projectile_86_c8e1_frame_03</c>, an installed program frame.</summary>
    Projectile86C8e1Frame03,
    /// <summary><c>projectile_86_ca22_frame_00</c>, an installed program frame.</summary>
    Projectile86Ca22Frame00,
    /// <summary><c>projectile_86_ca22_frame_01</c>, an installed program frame.</summary>
    Projectile86Ca22Frame01,
    /// <summary><c>projectile_86_ca22_frame_02</c>, an installed program frame.</summary>
    Projectile86Ca22Frame02,
    /// <summary><c>projectile_86_ca22_frame_03</c>, an installed program frame.</summary>
    Projectile86Ca22Frame03,
    /// <summary><c>projectile_86_ca22_frame_04</c>, an installed program frame.</summary>
    Projectile86Ca22Frame04,
    /// <summary><c>projectile_86_ca22_frame_05</c>, an installed program frame.</summary>
    Projectile86Ca22Frame05,
    /// <summary><c>projectile_86_ca22_frame_06</c>, an installed program frame.</summary>
    Projectile86Ca22Frame06,
    /// <summary><c>projectile_86_ca22_frame_07</c>, an installed program frame.</summary>
    Projectile86Ca22Frame07,
    /// <summary><c>projectile_86_caa4_frame_00</c>, an installed program frame.</summary>
    Projectile86Caa4Frame00,
    /// <summary><c>projectile_86_caa4_frame_01</c>, an installed program frame.</summary>
    Projectile86Caa4Frame01,
    /// <summary><c>projectile_86_caa4_frame_02</c>, an installed program frame.</summary>
    Projectile86Caa4Frame02,
    /// <summary><c>projectile_86_caa4_frame_03</c>, an installed program frame.</summary>
    Projectile86Caa4Frame03,
    /// <summary><c>projectile_86_caa4_frame_04</c>, an installed program frame.</summary>
    Projectile86Caa4Frame04,
    /// <summary><c>projectile_86_caa4_frame_05</c>, an installed program frame.</summary>
    Projectile86Caa4Frame05,
    /// <summary><c>projectile_86_caa4_frame_06</c>, an installed program frame.</summary>
    Projectile86Caa4Frame06,
    /// <summary><c>projectile_86_caa4_frame_07</c>, an installed program frame.</summary>
    Projectile86Caa4Frame07,
    /// <summary><c>projectile_86_cac8_frame_00</c>, an installed program frame.</summary>
    Projectile86Cac8Frame00,
    /// <summary><c>projectile_86_cac8_frame_01</c>, an installed program frame.</summary>
    Projectile86Cac8Frame01,
    /// <summary><c>projectile_86_cac8_frame_02</c>, an installed program frame.</summary>
    Projectile86Cac8Frame02,
    /// <summary><c>projectile_86_cac8_frame_03</c>, an installed program frame.</summary>
    Projectile86Cac8Frame03,
    /// <summary><c>projectile_86_cac8_frame_04</c>, an installed program frame.</summary>
    Projectile86Cac8Frame04,
    /// <summary><c>projectile_86_cac8_frame_05</c>, an installed program frame.</summary>
    Projectile86Cac8Frame05,
    /// <summary><c>projectile_86_cac8_frame_06</c>, an installed program frame.</summary>
    Projectile86Cac8Frame06,
    /// <summary><c>projectile_86_cac8_frame_07</c>, an installed program frame.</summary>
    Projectile86Cac8Frame07,
    /// <summary><c>projectile_86_cb0d_frame_00</c>, an installed program frame.</summary>
    Projectile86Cb0dFrame00,
    /// <summary><c>mother_brain_top_tube_frame_00</c>, an installed program frame.</summary>
    MotherBrainTopTubeFrame00,
    /// <summary><c>mother_brain_top_tube_frame_01</c>, an installed program frame.</summary>
    MotherBrainTopTubeFrame01,
    /// <summary><c>mother_brain_top_tube_frame_02</c>, an installed program frame.</summary>
    MotherBrainTopTubeFrame02,
    /// <summary><c>mother_brain_top_tube_frame_03</c>, an installed program frame.</summary>
    MotherBrainTopTubeFrame03,
    /// <summary><c>mother_brain_glass_frame_00</c>, an installed program frame.</summary>
    MotherBrainGlassFrame00,
    /// <summary><c>mother_brain_glass_frame_01</c>, an installed program frame.</summary>
    MotherBrainGlassFrame01,
    /// <summary><c>mother_brain_glass_frame_02</c>, an installed program frame.</summary>
    MotherBrainGlassFrame02,
    /// <summary><c>mother_brain_glass_frame_03</c>, an installed program frame.</summary>
    MotherBrainGlassFrame03,
    /// <summary><c>mother_brain_glass_frame_04</c>, an installed program frame.</summary>
    MotherBrainGlassFrame04,
    /// <summary><c>mother_brain_glass_frame_05</c>, an installed program frame.</summary>
    MotherBrainGlassFrame05,
    /// <summary><c>mother_brain_glass_frame_06</c>, an installed program frame.</summary>
    MotherBrainGlassFrame06,
    /// <summary><c>mother_brain_glass_frame_07</c>, an installed program frame.</summary>
    MotherBrainGlassFrame07,
    /// <summary><c>mother_brain_glass_frame_08</c>, an installed program frame.</summary>
    MotherBrainGlassFrame08,
    /// <summary><c>mother_brain_glass_frame_09</c>, an installed program frame.</summary>
    MotherBrainGlassFrame09,
    /// <summary><c>mother_brain_glass_frame_10</c>, an installed program frame.</summary>
    MotherBrainGlassFrame10,
    /// <summary><c>mother_brain_glass_frame_11</c>, an installed program frame.</summary>
    MotherBrainGlassFrame11,
    /// <summary><c>mother_brain_glass_frame_12</c>, an installed program frame.</summary>
    MotherBrainGlassFrame12,
    /// <summary><c>mother_brain_glass_frame_13</c>, an installed program frame.</summary>
    MotherBrainGlassFrame13,
    /// <summary><c>mother_brain_glass_frame_14</c>, an installed program frame.</summary>
    MotherBrainGlassFrame14,
    /// <summary><c>mother_brain_glass_frame_15</c>, an installed program frame.</summary>
    MotherBrainGlassFrame15,
    /// <summary><c>mother_brain_glass_frame_16</c>, an installed program frame.</summary>
    MotherBrainGlassFrame16,
    /// <summary><c>mother_brain_glass_frame_17</c>, an installed program frame.</summary>
    MotherBrainGlassFrame17,
    /// <summary><c>mother_brain_glass_frame_18</c>, an installed program frame.</summary>
    MotherBrainGlassFrame18,
    /// <summary><c>mother_brain_glass_frame_19</c>, an installed program frame.</summary>
    MotherBrainGlassFrame19,
    /// <summary><c>mother_brain_glass_frame_20</c>, an installed program frame.</summary>
    MotherBrainGlassFrame20,
    /// <summary><c>mother_brain_glass_frame_21</c>, an installed program frame.</summary>
    MotherBrainGlassFrame21,
    /// <summary><c>mother_brain_glass_frame_22</c>, an installed program frame.</summary>
    MotherBrainGlassFrame22,
    /// <summary><c>mother_brain_glass_frame_23</c>, an installed program frame.</summary>
    MotherBrainGlassFrame23,
    /// <summary><c>mother_brain_glass_frame_24</c>, an installed program frame.</summary>
    MotherBrainGlassFrame24,
    /// <summary><c>mother_brain_glass_frame_25</c>, an installed program frame.</summary>
    MotherBrainGlassFrame25,
    /// <summary><c>mother_brain_glass_frame_26</c>, an installed program frame.</summary>
    MotherBrainGlassFrame26,
    /// <summary><c>mother_brain_glass_frame_27</c>, an installed program frame.</summary>
    MotherBrainGlassFrame27,
    /// <summary><c>mother_brain_glass_frame_28</c>, an installed program frame.</summary>
    MotherBrainGlassFrame28,
    /// <summary><c>mother_brain_glass_frame_29</c>, an installed program frame.</summary>
    MotherBrainGlassFrame29,
    /// <summary><c>mother_brain_glass_frame_30</c>, an installed program frame.</summary>
    MotherBrainGlassFrame30,
    /// <summary><c>mother_brain_glass_frame_31</c>, an installed program frame.</summary>
    MotherBrainGlassFrame31,
    /// <summary><c>mother_brain_glass_frame_32</c>, an installed program frame.</summary>
    MotherBrainGlassFrame32,
    /// <summary><c>mother_brain_glass_frame_33</c>, an installed program frame.</summary>
    MotherBrainGlassFrame33,
    /// <summary><c>mother_brain_glass_frame_34</c>, an installed program frame.</summary>
    MotherBrainGlassFrame34,
    /// <summary><c>mother_brain_glass_frame_35</c>, an installed program frame.</summary>
    MotherBrainGlassFrame35,
    /// <summary><c>mother_brain_glass_frame_36</c>, an installed program frame.</summary>
    MotherBrainGlassFrame36,
    /// <summary><c>mother_brain_glass_frame_37</c>, an installed program frame.</summary>
    MotherBrainGlassFrame37,
    /// <summary><c>mother_brain_glass_frame_38</c>, an installed program frame.</summary>
    MotherBrainGlassFrame38,
    /// <summary><c>mother_brain_glass_frame_39</c>, an installed program frame.</summary>
    MotherBrainGlassFrame39,
    /// <summary><c>mother_brain_glass_frame_40</c>, an installed program frame.</summary>
    MotherBrainGlassFrame40,
    /// <summary><c>mother_brain_glass_frame_41</c>, an installed program frame.</summary>
    MotherBrainGlassFrame41,
    /// <summary><c>mother_brain_glass_frame_42</c>, an installed program frame.</summary>
    MotherBrainGlassFrame42,
    /// <summary><c>mother_brain_glass_frame_43</c>, an installed program frame.</summary>
    MotherBrainGlassFrame43,
    /// <summary><c>mother_brain_glass_frame_44</c>, an installed program frame.</summary>
    MotherBrainGlassFrame44,
    /// <summary><c>mother_brain_glass_frame_45</c>, an installed program frame.</summary>
    MotherBrainGlassFrame45,
    /// <summary><c>mother_brain_glass_frame_46</c>, an installed program frame.</summary>
    MotherBrainGlassFrame46,
    /// <summary><c>mother_brain_glass_frame_47</c>, an installed program frame.</summary>
    MotherBrainGlassFrame47,
    /// <summary><c>mother_brain_glass_frame_48</c>, an installed program frame.</summary>
    MotherBrainGlassFrame48,
    /// <summary><c>mother_brain_glass_frame_49</c>, an installed program frame.</summary>
    MotherBrainGlassFrame49,
    /// <summary><c>mother_brain_glass_frame_50</c>, an installed program frame.</summary>
    MotherBrainGlassFrame50,
    /// <summary><c>mother_brain_glass_frame_51</c>, an installed program frame.</summary>
    MotherBrainGlassFrame51,
    /// <summary><c>mother_brain_glass_frame_52</c>, an installed program frame.</summary>
    MotherBrainGlassFrame52,
    /// <summary><c>mother_brain_glass_frame_53</c>, an installed program frame.</summary>
    MotherBrainGlassFrame53,
    /// <summary><c>mother_brain_glass_frame_54</c>, an installed program frame.</summary>
    MotherBrainGlassFrame54,
    /// <summary><c>mother_brain_glass_frame_55</c>, an installed program frame.</summary>
    MotherBrainGlassFrame55,
    /// <summary><c>mother_brain_glass_frame_56</c>, an installed program frame.</summary>
    MotherBrainGlassFrame56,
    /// <summary><c>mother_brain_glass_frame_57</c>, an installed program frame.</summary>
    MotherBrainGlassFrame57,
    /// <summary><c>mother_brain_glass_frame_58</c>, an installed program frame.</summary>
    MotherBrainGlassFrame58,
    /// <summary><c>mother_brain_glass_frame_59</c>, an installed program frame.</summary>
    MotherBrainGlassFrame59,
    /// <summary><c>mother_brain_glass_frame_60</c>, an installed program frame.</summary>
    MotherBrainGlassFrame60,
    /// <summary><c>mother_brain_glass_frame_61</c>, an installed program frame.</summary>
    MotherBrainGlassFrame61,
    /// <summary><c>mother_brain_glass_frame_62</c>, an installed program frame.</summary>
    MotherBrainGlassFrame62,
    /// <summary><c>mother_brain_glass_frame_63</c>, an installed program frame.</summary>
    MotherBrainGlassFrame63,
    /// <summary><c>mother_brain_glass_frame_64</c>, an installed program frame.</summary>
    MotherBrainGlassFrame64,
    /// <summary><c>mother_brain_glass_frame_65</c>, an installed program frame.</summary>
    MotherBrainGlassFrame65,
    /// <summary><c>mother_brain_glass_frame_66</c>, an installed program frame.</summary>
    MotherBrainGlassFrame66,
    /// <summary><c>mother_brain_glass_frame_67</c>, an installed program frame.</summary>
    MotherBrainGlassFrame67,
    /// <summary><c>ki_hunter_acid_spit_frame_00</c>, an installed program frame.</summary>
    KiHunterAcidSpitFrame00,
    /// <summary><c>ki_hunter_acid_spit_frame_01</c>, an installed program frame.</summary>
    KiHunterAcidSpitFrame01,
    /// <summary><c>ki_hunter_acid_spit_frame_02</c>, an installed program frame.</summary>
    KiHunterAcidSpitFrame02,
    /// <summary><c>ki_hunter_acid_spit_frame_03</c>, an installed program frame.</summary>
    KiHunterAcidSpitFrame03,
    /// <summary><c>ki_hunter_acid_spit_frame_04</c>, an installed program frame.</summary>
    KiHunterAcidSpitFrame04,
    /// <summary><c>ki_hunter_acid_spit_frame_05</c>, an installed program frame.</summary>
    KiHunterAcidSpitFrame05,
    /// <summary><c>ki_hunter_acid_spit_frame_06</c>, an installed program frame.</summary>
    KiHunterAcidSpitFrame06,
    /// <summary><c>ki_hunter_acid_spit_frame_07</c>, an installed program frame.</summary>
    KiHunterAcidSpitFrame07,
    /// <summary><c>ki_hunter_acid_spit_frame_08</c>, an installed program frame.</summary>
    KiHunterAcidSpitFrame08,
    /// <summary><c>ki_hunter_acid_spit_frame_09</c>, an installed program frame.</summary>
    KiHunterAcidSpitFrame09,
    /// <summary><c>ki_hunter_acid_spit_frame_10</c>, an installed program frame.</summary>
    KiHunterAcidSpitFrame10,
    /// <summary><c>ki_hunter_acid_spit_frame_11</c>, an installed program frame.</summary>
    KiHunterAcidSpitFrame11,
    /// <summary><c>ki_hunter_acid_spit_frame_12</c>, an installed program frame.</summary>
    KiHunterAcidSpitFrame12,
    /// <summary><c>ki_hunter_acid_spit_frame_13</c>, an installed program frame.</summary>
    KiHunterAcidSpitFrame13,
    /// <summary><c>ki_hunter_acid_spit_frame_14</c>, an installed program frame.</summary>
    KiHunterAcidSpitFrame14,
    /// <summary><c>ki_hunter_acid_spit_frame_15</c>, an installed program frame.</summary>
    KiHunterAcidSpitFrame15,
    /// <summary><c>ki_hunter_acid_spit_frame_16</c>, an installed program frame.</summary>
    KiHunterAcidSpitFrame16,
    /// <summary><c>ki_hunter_acid_spit_frame_17</c>, an installed program frame.</summary>
    KiHunterAcidSpitFrame17,
    /// <summary><c>ki_hunter_acid_spit_frame_18</c>, an installed program frame.</summary>
    KiHunterAcidSpitFrame18,
    /// <summary><c>kago_bug_frame_00</c>, an installed program frame.</summary>
    KagoBugFrame00,
    /// <summary><c>kago_bug_frame_01</c>, an installed program frame.</summary>
    KagoBugFrame01,
    /// <summary><c>kago_bug_frame_02</c>, an installed program frame.</summary>
    KagoBugFrame02,
    /// <summary><c>kago_bug_frame_03</c>, an installed program frame.</summary>
    KagoBugFrame03,
    /// <summary><c>kago_bug_frame_04</c>, an installed program frame.</summary>
    KagoBugFrame04,
    /// <summary><c>kago_bug_frame_05</c>, an installed program frame.</summary>
    KagoBugFrame05,
    /// <summary><c>kago_bug_frame_06</c>, an installed program frame.</summary>
    KagoBugFrame06,
    /// <summary><c>kago_bug_frame_07</c>, an installed program frame.</summary>
    KagoBugFrame07,
    /// <summary><c>kago_bug_frame_08</c>, an installed program frame.</summary>
    KagoBugFrame08,
    /// <summary><c>kago_bug_frame_09</c>, an installed program frame.</summary>
    KagoBugFrame09,
    /// <summary><c>kago_bug_frame_10</c>, an installed program frame.</summary>
    KagoBugFrame10,
    /// <summary><c>powamp_spike_frame_00</c>, an installed program frame.</summary>
    PowampSpikeFrame00,
    /// <summary><c>powamp_spike_frame_01</c>, an installed program frame.</summary>
    PowampSpikeFrame01,
    /// <summary><c>powamp_spike_frame_02</c>, an installed program frame.</summary>
    PowampSpikeFrame02,
    /// <summary><c>work_robot_laser_frame_00</c>, an installed program frame.</summary>
    WorkRobotLaserFrame00,
    /// <summary><c>work_robot_laser_frame_01</c>, an installed program frame.</summary>
    WorkRobotLaserFrame01,
    /// <summary><c>work_robot_laser_frame_02</c>, an installed program frame.</summary>
    WorkRobotLaserFrame02,
    /// <summary><c>work_robot_laser_frame_03</c>, an installed program frame.</summary>
    WorkRobotLaserFrame03,
    /// <summary><c>work_robot_laser_frame_04</c>, an installed program frame.</summary>
    WorkRobotLaserFrame04,
    /// <summary><c>work_robot_laser_frame_05</c>, an installed program frame.</summary>
    WorkRobotLaserFrame05,
    /// <summary><c>work_robot_laser_frame_06</c>, an installed program frame.</summary>
    WorkRobotLaserFrame06,
    /// <summary><c>noob_tube_frame_00</c>, an installed program frame.</summary>
    NoobTubeFrame00,
    /// <summary><c>noob_tube_frame_01</c>, an installed program frame.</summary>
    NoobTubeFrame01,
    /// <summary><c>noob_tube_frame_02</c>, an installed program frame.</summary>
    NoobTubeFrame02,
    /// <summary><c>noob_tube_frame_03</c>, an installed program frame.</summary>
    NoobTubeFrame03,
    /// <summary><c>noob_tube_frame_04</c>, an installed program frame.</summary>
    NoobTubeFrame04,
    /// <summary><c>noob_tube_frame_05</c>, an installed program frame.</summary>
    NoobTubeFrame05,
    /// <summary><c>noob_tube_frame_06</c>, an installed program frame.</summary>
    NoobTubeFrame06,
    /// <summary><c>noob_tube_frame_07</c>, an installed program frame.</summary>
    NoobTubeFrame07,
    /// <summary><c>noob_tube_frame_08</c>, an installed program frame.</summary>
    NoobTubeFrame08,
    /// <summary><c>noob_tube_frame_09</c>, an installed program frame.</summary>
    NoobTubeFrame09,
    /// <summary><c>noob_tube_frame_10</c>, an installed program frame.</summary>
    NoobTubeFrame10,
    /// <summary><c>noob_tube_frame_11</c>, an installed program frame.</summary>
    NoobTubeFrame11,
    /// <summary><c>noob_tube_frame_12</c>, an installed program frame.</summary>
    NoobTubeFrame12,
    /// <summary><c>noob_tube_frame_13</c>, an installed program frame.</summary>
    NoobTubeFrame13,
    /// <summary><c>noob_tube_frame_14</c>, an installed program frame.</summary>
    NoobTubeFrame14,
    /// <summary><c>noob_tube_frame_15</c>, an installed program frame.</summary>
    NoobTubeFrame15,
    /// <summary><c>noob_tube_frame_16</c>, an installed program frame.</summary>
    NoobTubeFrame16,
    /// <summary><c>noob_tube_frame_17</c>, an installed program frame.</summary>
    NoobTubeFrame17,
    /// <summary><c>noob_tube_frame_18</c>, an installed program frame.</summary>
    NoobTubeFrame18,
    /// <summary><c>noob_tube_frame_19</c>, an installed program frame.</summary>
    NoobTubeFrame19,
    /// <summary><c>noob_tube_frame_20</c>, an installed program frame.</summary>
    NoobTubeFrame20,
    /// <summary><c>noob_tube_frame_21</c>, an installed program frame.</summary>
    NoobTubeFrame21,
    /// <summary><c>noob_tube_frame_22</c>, an installed program frame.</summary>
    NoobTubeFrame22,
    /// <summary><c>noob_tube_frame_23</c>, an installed program frame.</summary>
    NoobTubeFrame23,
    /// <summary><c>noob_tube_frame_24</c>, an installed program frame.</summary>
    NoobTubeFrame24,
    /// <summary><c>noob_tube_frame_25</c>, an installed program frame.</summary>
    NoobTubeFrame25,
    /// <summary><c>noob_tube_frame_26</c>, an installed program frame.</summary>
    NoobTubeFrame26,
    /// <summary><c>noob_tube_frame_27</c>, an installed program frame.</summary>
    NoobTubeFrame27,
    /// <summary><c>noob_tube_frame_28</c>, an installed program frame.</summary>
    NoobTubeFrame28,
    /// <summary><c>noob_tube_frame_29</c>, an installed program frame.</summary>
    NoobTubeFrame29,
    /// <summary><c>noob_tube_frame_30</c>, an installed program frame.</summary>
    NoobTubeFrame30,
    /// <summary><c>noob_tube_frame_31</c>, an installed program frame.</summary>
    NoobTubeFrame31,
    /// <summary><c>noob_tube_frame_32</c>, an installed program frame.</summary>
    NoobTubeFrame32,
    /// <summary><c>noob_tube_frame_33</c>, an installed program frame.</summary>
    NoobTubeFrame33,
    /// <summary><c>noob_tube_frame_34</c>, an installed program frame.</summary>
    NoobTubeFrame34,
    /// <summary><c>noob_tube_frame_35</c>, an installed program frame.</summary>
    NoobTubeFrame35,
    /// <summary><c>noob_tube_frame_36</c>, an installed program frame.</summary>
    NoobTubeFrame36,
    /// <summary><c>noob_tube_frame_37</c>, an installed program frame.</summary>
    NoobTubeFrame37,
    /// <summary><c>noob_tube_frame_38</c>, an installed program frame.</summary>
    NoobTubeFrame38,
    /// <summary><c>noob_tube_frame_39</c>, an installed program frame.</summary>
    NoobTubeFrame39,
    /// <summary><c>noob_tube_frame_40</c>, an installed program frame.</summary>
    NoobTubeFrame40,
    /// <summary><c>noob_tube_frame_41</c>, an installed program frame.</summary>
    NoobTubeFrame41,
    /// <summary><c>noob_tube_frame_42</c>, an installed program frame.</summary>
    NoobTubeFrame42,
    /// <summary><c>noob_tube_frame_43</c>, an installed program frame.</summary>
    NoobTubeFrame43,
    /// <summary><c>noob_tube_frame_44</c>, an installed program frame.</summary>
    NoobTubeFrame44,
    /// <summary><c>noob_tube_frame_45</c>, an installed program frame.</summary>
    NoobTubeFrame45,
    /// <summary><c>noob_tube_frame_46</c>, an installed program frame.</summary>
    NoobTubeFrame46,
    /// <summary><c>noob_tube_frame_47</c>, an installed program frame.</summary>
    NoobTubeFrame47,
    /// <summary><c>noob_tube_frame_48</c>, an installed program frame.</summary>
    NoobTubeFrame48,
    /// <summary><c>noob_tube_frame_49</c>, an installed program frame.</summary>
    NoobTubeFrame49,
    /// <summary><c>noob_tube_frame_50</c>, an installed program frame.</summary>
    NoobTubeFrame50,
    /// <summary><c>noob_tube_frame_51</c>, an installed program frame.</summary>
    NoobTubeFrame51,
    /// <summary><c>noob_tube_frame_52</c>, an installed program frame.</summary>
    NoobTubeFrame52,
    /// <summary><c>noob_tube_frame_53</c>, an installed program frame.</summary>
    NoobTubeFrame53,
    /// <summary><c>noob_tube_frame_54</c>, an installed program frame.</summary>
    NoobTubeFrame54,
    /// <summary><c>noob_tube_frame_55</c>, an installed program frame.</summary>
    NoobTubeFrame55,
    /// <summary><c>noob_tube_frame_56</c>, an installed program frame.</summary>
    NoobTubeFrame56,
    /// <summary><c>noob_tube_frame_57</c>, an installed program frame.</summary>
    NoobTubeFrame57,
    /// <summary><c>noob_tube_frame_58</c>, an installed program frame.</summary>
    NoobTubeFrame58,
    /// <summary><c>noob_tube_frame_59</c>, an installed program frame.</summary>
    NoobTubeFrame59,
    /// <summary><c>noob_tube_frame_60</c>, an installed program frame.</summary>
    NoobTubeFrame60,
    /// <summary><c>noob_tube_frame_61</c>, an installed program frame.</summary>
    NoobTubeFrame61,
    /// <summary><c>noob_tube_frame_62</c>, an installed program frame.</summary>
    NoobTubeFrame62,
    /// <summary><c>noob_tube_frame_63</c>, an installed program frame.</summary>
    NoobTubeFrame63,
    /// <summary><c>noob_tube_frame_64</c>, an installed program frame.</summary>
    NoobTubeFrame64,
    /// <summary><c>noob_tube_frame_65</c>, an installed program frame.</summary>
    NoobTubeFrame65,
    /// <summary><c>noob_tube_frame_66</c>, an installed program frame.</summary>
    NoobTubeFrame66,
    /// <summary><c>noob_tube_frame_67</c>, an installed program frame.</summary>
    NoobTubeFrame67,
    /// <summary><c>noob_tube_frame_68</c>, an installed program frame.</summary>
    NoobTubeFrame68,
    /// <summary><c>noob_tube_frame_69</c>, an installed program frame.</summary>
    NoobTubeFrame69,
    /// <summary><c>noob_tube_frame_70</c>, an installed program frame.</summary>
    NoobTubeFrame70,
    /// <summary><c>noob_tube_frame_71</c>, an installed program frame.</summary>
    NoobTubeFrame71,
    /// <summary><c>noob_tube_frame_72</c>, an installed program frame.</summary>
    NoobTubeFrame72,
    /// <summary><c>noob_tube_frame_73</c>, an installed program frame.</summary>
    NoobTubeFrame73,
    /// <summary><c>noob_tube_frame_74</c>, an installed program frame.</summary>
    NoobTubeFrame74,
    /// <summary><c>noob_tube_frame_75</c>, an installed program frame.</summary>
    NoobTubeFrame75,
    /// <summary><c>noob_tube_frame_76</c>, an installed program frame.</summary>
    NoobTubeFrame76,
    /// <summary><c>noob_tube_frame_77</c>, an installed program frame.</summary>
    NoobTubeFrame77,
    /// <summary><c>noob_tube_frame_78</c>, an installed program frame.</summary>
    NoobTubeFrame78,
    /// <summary><c>noob_tube_frame_79</c>, an installed program frame.</summary>
    NoobTubeFrame79,
    /// <summary><c>noob_tube_frame_80</c>, an installed program frame.</summary>
    NoobTubeFrame80,
    /// <summary><c>noob_tube_frame_81</c>, an installed program frame.</summary>
    NoobTubeFrame81,
    /// <summary><c>noob_tube_frame_82</c>, an installed program frame.</summary>
    NoobTubeFrame82,
    /// <summary><c>noob_tube_frame_83</c>, an installed program frame.</summary>
    NoobTubeFrame83,
    /// <summary><c>noob_tube_frame_84</c>, an installed program frame.</summary>
    NoobTubeFrame84,
    /// <summary><c>noob_tube_frame_85</c>, an installed program frame.</summary>
    NoobTubeFrame85,
    /// <summary><c>noob_tube_frame_86</c>, an installed program frame.</summary>
    NoobTubeFrame86,
    /// <summary><c>noob_tube_frame_87</c>, an installed program frame.</summary>
    NoobTubeFrame87,
    /// <summary><c>noob_tube_frame_88</c>, an installed program frame.</summary>
    NoobTubeFrame88,
    /// <summary><c>noob_tube_frame_89</c>, an installed program frame.</summary>
    NoobTubeFrame89,
    /// <summary><c>cacatac_spike_frame_00</c>, an installed program frame.</summary>
    CacatacSpikeFrame00,
    /// <summary><c>cacatac_spike_frame_01</c>, an installed program frame.</summary>
    CacatacSpikeFrame01,
    /// <summary><c>cacatac_spike_frame_02</c>, an installed program frame.</summary>
    CacatacSpikeFrame02,
    /// <summary><c>cacatac_spike_frame_03</c>, an installed program frame.</summary>
    CacatacSpikeFrame03,
    /// <summary><c>cacatac_spike_frame_04</c>, an installed program frame.</summary>
    CacatacSpikeFrame04,
    /// <summary><c>cacatac_spike_frame_05</c>, an installed program frame.</summary>
    CacatacSpikeFrame05,
    /// <summary><c>cacatac_spike_frame_06</c>, an installed program frame.</summary>
    CacatacSpikeFrame06,
    /// <summary><c>cacatac_spike_frame_07</c>, an installed program frame.</summary>
    CacatacSpikeFrame07,
    /// <summary><c>cacatac_spike_frame_08</c>, an installed program frame.</summary>
    CacatacSpikeFrame08,
    /// <summary><c>cacatac_spike_frame_09</c>, an installed program frame.</summary>
    CacatacSpikeFrame09,
    /// <summary><c>stoke_frame_00</c>, an installed program frame.</summary>
    StokeFrame00,
    /// <summary><c>stoke_frame_01</c>, an installed program frame.</summary>
    StokeFrame01,
    /// <summary><c>spore_spawn_frame_00</c>, an installed program frame.</summary>
    SporeSpawnFrame00,
    /// <summary><c>spore_spawn_frame_01</c>, an installed program frame.</summary>
    SporeSpawnFrame01,
    /// <summary><c>spore_spawn_frame_02</c>, an installed program frame.</summary>
    SporeSpawnFrame02,
    /// <summary><c>spore_spawn_frame_03</c>, an installed program frame.</summary>
    SporeSpawnFrame03,
    /// <summary><c>spore_spawn_frame_04</c>, an installed program frame.</summary>
    SporeSpawnFrame04,
    /// <summary><c>spore_spawn_frame_05</c>, an installed program frame.</summary>
    SporeSpawnFrame05,
    /// <summary><c>spore_spawn_frame_06</c>, an installed program frame.</summary>
    SporeSpawnFrame06,
    /// <summary><c>spore_spawn_frame_07</c>, an installed program frame.</summary>
    SporeSpawnFrame07,
    /// <summary><c>spore_spawn_frame_08</c>, an installed program frame.</summary>
    SporeSpawnFrame08,
    /// <summary><c>spore_spawn_frame_09</c>, an installed program frame.</summary>
    SporeSpawnFrame09,
    /// <summary><c>spore_spawn_frame_10</c>, an installed program frame.</summary>
    SporeSpawnFrame10,
    /// <summary><c>spore_spawn_frame_11</c>, an installed program frame.</summary>
    SporeSpawnFrame11,
    /// <summary><c>spore_spawn_frame_12</c>, an installed program frame.</summary>
    SporeSpawnFrame12,
    /// <summary><c>spore_spawn_frame_13</c>, an installed program frame.</summary>
    SporeSpawnFrame13,
    /// <summary><c>spore_spawn_frame_14</c>, an installed program frame.</summary>
    SporeSpawnFrame14,
    /// <summary><c>spore_spawn_frame_15</c>, an installed program frame.</summary>
    SporeSpawnFrame15,
    /// <summary><c>spore_spawn_frame_16</c>, an installed program frame.</summary>
    SporeSpawnFrame16,
    /// <summary><c>fune_namihe_fireball_frame_00</c>, an installed program frame.</summary>
    FuneNamiheFireballFrame00,
    /// <summary><c>fune_namihe_fireball_frame_01</c>, an installed program frame.</summary>
    FuneNamiheFireballFrame01,
    /// <summary><c>fune_namihe_fireball_frame_02</c>, an installed program frame.</summary>
    FuneNamiheFireballFrame02,
    /// <summary><c>fune_namihe_fireball_frame_03</c>, an installed program frame.</summary>
    FuneNamiheFireballFrame03,
    /// <summary><c>fune_namihe_fireball_frame_04</c>, an installed program frame.</summary>
    FuneNamiheFireballFrame04,
    /// <summary><c>fune_namihe_fireball_frame_05</c>, an installed program frame.</summary>
    FuneNamiheFireballFrame05,
    /// <summary><c>magdollite_lava_frame_00</c>, an installed program frame.</summary>
    MagdolliteLavaFrame00,
    /// <summary><c>magdollite_lava_frame_01</c>, an installed program frame.</summary>
    MagdolliteLavaFrame01,
    /// <summary><c>projectile_86_e0ee_frame_00</c>, an installed program frame.</summary>
    Projectile86E0eeFrame00,
    /// <summary><c>projectile_86_e0ee_frame_01</c>, an installed program frame.</summary>
    Projectile86E0eeFrame01,
    /// <summary><c>projectile_86_e0ee_frame_02</c>, an installed program frame.</summary>
    Projectile86E0eeFrame02,
    /// <summary><c>projectile_86_e0ee_frame_03</c>, an installed program frame.</summary>
    Projectile86E0eeFrame03,
    /// <summary><c>projectile_86_e100_frame_00</c>, an installed program frame.</summary>
    Projectile86E100Frame00,
    /// <summary><c>projectile_86_e100_frame_01</c>, an installed program frame.</summary>
    Projectile86E100Frame01,
    /// <summary><c>projectile_86_e100_frame_02</c>, an installed program frame.</summary>
    Projectile86E100Frame02,
    /// <summary><c>projectile_86_e100_frame_03</c>, an installed program frame.</summary>
    Projectile86E100Frame03,
    /// <summary><c>projectile_86_e100_frame_04</c>, an installed program frame.</summary>
    Projectile86E100Frame04,
    /// <summary><c>projectile_86_e100_frame_05</c>, an installed program frame.</summary>
    Projectile86E100Frame05,
    /// <summary><c>projectile_86_e11a_frame_00</c>, an installed program frame.</summary>
    Projectile86E11aFrame00,
    /// <summary><c>projectile_86_e11a_frame_01</c>, an installed program frame.</summary>
    Projectile86E11aFrame01,
    /// <summary><c>projectile_86_e11a_frame_02</c>, an installed program frame.</summary>
    Projectile86E11aFrame02,
    /// <summary><c>projectile_86_e11a_frame_03</c>, an installed program frame.</summary>
    Projectile86E11aFrame03,
    /// <summary><c>projectile_86_e11a_frame_04</c>, an installed program frame.</summary>
    Projectile86E11aFrame04,
    /// <summary><c>projectile_86_e11a_frame_05</c>, an installed program frame.</summary>
    Projectile86E11aFrame05,
    /// <summary><c>projectile_86_e11a_frame_06</c>, an installed program frame.</summary>
    Projectile86E11aFrame06,
    /// <summary><c>projectile_86_e138_frame_00</c>, an installed program frame.</summary>
    Projectile86E138Frame00,
    /// <summary><c>projectile_86_e138_frame_01</c>, an installed program frame.</summary>
    Projectile86E138Frame01,
    /// <summary><c>projectile_86_e138_frame_02</c>, an installed program frame.</summary>
    Projectile86E138Frame02,
    /// <summary><c>projectile_86_e138_frame_03</c>, an installed program frame.</summary>
    Projectile86E138Frame03,
    /// <summary><c>projectile_86_e138_frame_04</c>, an installed program frame.</summary>
    Projectile86E138Frame04,
    /// <summary><c>projectile_86_e138_frame_05</c>, an installed program frame.</summary>
    Projectile86E138Frame05,
    /// <summary><c>projectile_86_e152_frame_00</c>, an installed program frame.</summary>
    Projectile86E152Frame00,
    /// <summary><c>projectile_86_e152_frame_01</c>, an installed program frame.</summary>
    Projectile86E152Frame01,
    /// <summary><c>projectile_86_e152_frame_02</c>, an installed program frame.</summary>
    Projectile86E152Frame02,
    /// <summary><c>projectile_86_e152_frame_03</c>, an installed program frame.</summary>
    Projectile86E152Frame03,
    /// <summary><c>projectile_86_e152_frame_04</c>, an installed program frame.</summary>
    Projectile86E152Frame04,
    /// <summary><c>projectile_86_e168_frame_00</c>, an installed program frame.</summary>
    Projectile86E168Frame00,
    /// <summary><c>projectile_86_e168_frame_01</c>, an installed program frame.</summary>
    Projectile86E168Frame01,
    /// <summary><c>projectile_86_e168_frame_02</c>, an installed program frame.</summary>
    Projectile86E168Frame02,
    /// <summary><c>projectile_86_e168_frame_03</c>, an installed program frame.</summary>
    Projectile86E168Frame03,
    /// <summary><c>projectile_86_e168_frame_04</c>, an installed program frame.</summary>
    Projectile86E168Frame04,
    /// <summary><c>projectile_86_e17e_frame_00</c>, an installed program frame.</summary>
    Projectile86E17eFrame00,
    /// <summary><c>projectile_86_e17e_frame_01</c>, an installed program frame.</summary>
    Projectile86E17eFrame01,
    /// <summary><c>projectile_86_e17e_frame_02</c>, an installed program frame.</summary>
    Projectile86E17eFrame02,
    /// <summary><c>projectile_86_e17e_frame_03</c>, an installed program frame.</summary>
    Projectile86E17eFrame03,
    /// <summary><c>projectile_86_e17e_frame_04</c>, an installed program frame.</summary>
    Projectile86E17eFrame04,
    /// <summary><c>projectile_86_e17e_frame_05</c>, an installed program frame.</summary>
    Projectile86E17eFrame05,
    /// <summary><c>projectile_86_e198_frame_00</c>, an installed program frame.</summary>
    Projectile86E198Frame00,
    /// <summary><c>projectile_86_e198_frame_01</c>, an installed program frame.</summary>
    Projectile86E198Frame01,
    /// <summary><c>projectile_86_e198_frame_02</c>, an installed program frame.</summary>
    Projectile86E198Frame02,
    /// <summary><c>projectile_86_e1a6_frame_00</c>, an installed program frame.</summary>
    Projectile86E1a6Frame00,
    /// <summary><c>projectile_86_e1a6_frame_01</c>, an installed program frame.</summary>
    Projectile86E1a6Frame01,
    /// <summary><c>projectile_86_e1b0_frame_00</c>, an installed program frame.</summary>
    Projectile86E1b0Frame00,
    /// <summary><c>projectile_86_e1b0_frame_01</c>, an installed program frame.</summary>
    Projectile86E1b0Frame01,
    /// <summary><c>projectile_86_e1b0_frame_02</c>, an installed program frame.</summary>
    Projectile86E1b0Frame02,
    /// <summary><c>projectile_86_e1b0_frame_03</c>, an installed program frame.</summary>
    Projectile86E1b0Frame03,
    /// <summary><c>projectile_86_e1b0_frame_04</c>, an installed program frame.</summary>
    Projectile86E1b0Frame04,
    /// <summary><c>projectile_86_e1c6_frame_00</c>, an installed program frame.</summary>
    Projectile86E1c6Frame00,
    /// <summary><c>projectile_86_e1c6_frame_01</c>, an installed program frame.</summary>
    Projectile86E1c6Frame01,
    /// <summary><c>projectile_86_e1c6_frame_02</c>, an installed program frame.</summary>
    Projectile86E1c6Frame02,
    /// <summary><c>projectile_86_e1c6_frame_03</c>, an installed program frame.</summary>
    Projectile86E1c6Frame03,
    /// <summary><c>projectile_86_e1d8_frame_00</c>, an installed program frame.</summary>
    Projectile86E1d8Frame00,
    /// <summary><c>projectile_86_e1d8_frame_01</c>, an installed program frame.</summary>
    Projectile86E1d8Frame01,
    /// <summary><c>projectile_86_e1d8_frame_02</c>, an installed program frame.</summary>
    Projectile86E1d8Frame02,
    /// <summary><c>projectile_86_e1d8_frame_03</c>, an installed program frame.</summary>
    Projectile86E1d8Frame03,
    /// <summary><c>projectile_86_e1ea_frame_00</c>, an installed program frame.</summary>
    Projectile86E1eaFrame00,
    /// <summary><c>projectile_86_e1ea_frame_01</c>, an installed program frame.</summary>
    Projectile86E1eaFrame01,
    /// <summary><c>projectile_86_e1ea_frame_02</c>, an installed program frame.</summary>
    Projectile86E1eaFrame02,
    /// <summary><c>projectile_86_e1ea_frame_03</c>, an installed program frame.</summary>
    Projectile86E1eaFrame03,
    /// <summary><c>projectile_86_e1fc_frame_00</c>, an installed program frame.</summary>
    Projectile86E1fcFrame00,
    /// <summary><c>projectile_86_e1fc_frame_01</c>, an installed program frame.</summary>
    Projectile86E1fcFrame01,
    /// <summary><c>projectile_86_e208_frame_00</c>, an installed program frame.</summary>
    Projectile86E208Frame00,
    /// <summary><c>projectile_86_e208_frame_01</c>, an installed program frame.</summary>
    Projectile86E208Frame01,
    /// <summary><c>projectile_86_e208_frame_02</c>, an installed program frame.</summary>
    Projectile86E208Frame02,
    /// <summary><c>projectile_86_e208_frame_03</c>, an installed program frame.</summary>
    Projectile86E208Frame03,
    /// <summary><c>projectile_86_e208_frame_04</c>, an installed program frame.</summary>
    Projectile86E208Frame04,
    /// <summary><c>projectile_86_e208_frame_05</c>, an installed program frame.</summary>
    Projectile86E208Frame05,
    /// <summary><c>projectile_86_e222_frame_00</c>, an installed program frame.</summary>
    Projectile86E222Frame00,
    /// <summary><c>projectile_86_e222_frame_01</c>, an installed program frame.</summary>
    Projectile86E222Frame01,
    /// <summary><c>projectile_86_e222_frame_02</c>, an installed program frame.</summary>
    Projectile86E222Frame02,
    /// <summary><c>projectile_86_e222_frame_03</c>, an installed program frame.</summary>
    Projectile86E222Frame03,
    /// <summary><c>projectile_86_e234_frame_00</c>, an installed program frame.</summary>
    Projectile86E234Frame00,
    /// <summary><c>projectile_86_e234_frame_01</c>, an installed program frame.</summary>
    Projectile86E234Frame01,
    /// <summary><c>projectile_86_e234_frame_02</c>, an installed program frame.</summary>
    Projectile86E234Frame02,
    /// <summary><c>projectile_86_e234_frame_03</c>, an installed program frame.</summary>
    Projectile86E234Frame03,
    /// <summary><c>projectile_86_e246_frame_00</c>, an installed program frame.</summary>
    Projectile86E246Frame00,
    /// <summary><c>projectile_86_e246_frame_01</c>, an installed program frame.</summary>
    Projectile86E246Frame01,
    /// <summary><c>projectile_86_e246_frame_02</c>, an installed program frame.</summary>
    Projectile86E246Frame02,
    /// <summary><c>projectile_86_e246_frame_03</c>, an installed program frame.</summary>
    Projectile86E246Frame03,
    /// <summary><c>projectile_86_e258_frame_00</c>, an installed program frame.</summary>
    Projectile86E258Frame00,
    /// <summary><c>projectile_86_e258_frame_01</c>, an installed program frame.</summary>
    Projectile86E258Frame01,
    /// <summary><c>projectile_86_e258_frame_02</c>, an installed program frame.</summary>
    Projectile86E258Frame02,
    /// <summary><c>projectile_86_e266_frame_00</c>, an installed program frame.</summary>
    Projectile86E266Frame00,
    /// <summary><c>projectile_86_e266_frame_01</c>, an installed program frame.</summary>
    Projectile86E266Frame01,
    /// <summary><c>projectile_86_e266_frame_02</c>, an installed program frame.</summary>
    Projectile86E266Frame02,
    /// <summary><c>projectile_86_e266_frame_03</c>, an installed program frame.</summary>
    Projectile86E266Frame03,
    /// <summary><c>projectile_86_e266_frame_04</c>, an installed program frame.</summary>
    Projectile86E266Frame04,
    /// <summary><c>projectile_86_e266_frame_05</c>, an installed program frame.</summary>
    Projectile86E266Frame05,
    /// <summary><c>projectile_86_e266_frame_06</c>, an installed program frame.</summary>
    Projectile86E266Frame06,
    /// <summary><c>projectile_86_e266_frame_07</c>, an installed program frame.</summary>
    Projectile86E266Frame07,
    /// <summary><c>projectile_86_e266_frame_08</c>, an installed program frame.</summary>
    Projectile86E266Frame08,
    /// <summary><c>projectile_86_e266_frame_09</c>, an installed program frame.</summary>
    Projectile86E266Frame09,
    /// <summary><c>projectile_86_e266_frame_10</c>, an installed program frame.</summary>
    Projectile86E266Frame10,
    /// <summary><c>projectile_86_e266_frame_11</c>, an installed program frame.</summary>
    Projectile86E266Frame11,
    /// <summary><c>projectile_86_e266_frame_12</c>, an installed program frame.</summary>
    Projectile86E266Frame12,
    /// <summary><c>projectile_86_e266_frame_13</c>, an installed program frame.</summary>
    Projectile86E266Frame13,
    /// <summary><c>projectile_86_e266_frame_14</c>, an installed program frame.</summary>
    Projectile86E266Frame14,
    /// <summary><c>projectile_86_e266_frame_15</c>, an installed program frame.</summary>
    Projectile86E266Frame15,
    /// <summary><c>projectile_86_e2a8_frame_00</c>, an installed program frame.</summary>
    Projectile86E2a8Frame00,
    /// <summary><c>projectile_86_e2a8_frame_01</c>, an installed program frame.</summary>
    Projectile86E2a8Frame01,
    /// <summary><c>projectile_86_e2a8_frame_02</c>, an installed program frame.</summary>
    Projectile86E2a8Frame02,
    /// <summary><c>projectile_86_e2a8_frame_03</c>, an installed program frame.</summary>
    Projectile86E2a8Frame03,
    /// <summary><c>projectile_86_e2ba_frame_00</c>, an installed program frame.</summary>
    Projectile86E2baFrame00,
    /// <summary><c>projectile_86_e2ba_frame_01</c>, an installed program frame.</summary>
    Projectile86E2baFrame01,
    /// <summary><c>projectile_86_e2ba_frame_02</c>, an installed program frame.</summary>
    Projectile86E2baFrame02,
    /// <summary><c>projectile_86_e2ba_frame_03</c>, an installed program frame.</summary>
    Projectile86E2baFrame03,
    /// <summary><c>projectile_86_e2ba_frame_04</c>, an installed program frame.</summary>
    Projectile86E2baFrame04,
    /// <summary><c>projectile_86_e2ba_frame_05</c>, an installed program frame.</summary>
    Projectile86E2baFrame05,
    /// <summary><c>projectile_86_e2d4_frame_00</c>, an installed program frame.</summary>
    Projectile86E2d4Frame00,
    /// <summary><c>projectile_86_e2d4_frame_01</c>, an installed program frame.</summary>
    Projectile86E2d4Frame01,
    /// <summary><c>projectile_86_e2d4_frame_02</c>, an installed program frame.</summary>
    Projectile86E2d4Frame02,
    /// <summary><c>projectile_86_e2d4_frame_03</c>, an installed program frame.</summary>
    Projectile86E2d4Frame03,
    /// <summary><c>projectile_86_e2d4_frame_04</c>, an installed program frame.</summary>
    Projectile86E2d4Frame04,
    /// <summary><c>projectile_86_e2d4_frame_05</c>, an installed program frame.</summary>
    Projectile86E2d4Frame05,
    /// <summary><c>projectile_86_e2d4_frame_06</c>, an installed program frame.</summary>
    Projectile86E2d4Frame06,
    /// <summary><c>projectile_86_e2f2_frame_00</c>, an installed program frame.</summary>
    Projectile86E2f2Frame00,
    /// <summary><c>projectile_86_e2f2_frame_01</c>, an installed program frame.</summary>
    Projectile86E2f2Frame01,
    /// <summary><c>projectile_86_e2f2_frame_02</c>, an installed program frame.</summary>
    Projectile86E2f2Frame02,
    /// <summary><c>projectile_86_e2f2_frame_03</c>, an installed program frame.</summary>
    Projectile86E2f2Frame03,
    /// <summary><c>projectile_86_e2f2_frame_04</c>, an installed program frame.</summary>
    Projectile86E2f2Frame04,
    /// <summary><c>projectile_86_e2f2_frame_05</c>, an installed program frame.</summary>
    Projectile86E2f2Frame05,
    /// <summary><c>projectile_86_e2f2_frame_06</c>, an installed program frame.</summary>
    Projectile86E2f2Frame06,
    /// <summary><c>projectile_86_e2f2_frame_07</c>, an installed program frame.</summary>
    Projectile86E2f2Frame07,
    /// <summary><c>projectile_86_e314_frame_00</c>, an installed program frame.</summary>
    Projectile86E314Frame00,
    /// <summary><c>projectile_86_e314_frame_01</c>, an installed program frame.</summary>
    Projectile86E314Frame01,
    /// <summary><c>projectile_86_e314_frame_02</c>, an installed program frame.</summary>
    Projectile86E314Frame02,
    /// <summary><c>projectile_86_e314_frame_03</c>, an installed program frame.</summary>
    Projectile86E314Frame03,
    /// <summary><c>projectile_86_e314_frame_04</c>, an installed program frame.</summary>
    Projectile86E314Frame04,
    /// <summary><c>projectile_86_e314_frame_05</c>, an installed program frame.</summary>
    Projectile86E314Frame05,
    /// <summary><c>projectile_86_e314_frame_06</c>, an installed program frame.</summary>
    Projectile86E314Frame06,
    /// <summary><c>projectile_86_e314_frame_07</c>, an installed program frame.</summary>
    Projectile86E314Frame07,
    /// <summary><c>projectile_86_e314_frame_08</c>, an installed program frame.</summary>
    Projectile86E314Frame08,
    /// <summary><c>projectile_86_e314_frame_09</c>, an installed program frame.</summary>
    Projectile86E314Frame09,
    /// <summary><c>projectile_86_e314_frame_10</c>, an installed program frame.</summary>
    Projectile86E314Frame10,
    /// <summary><c>projectile_86_e314_frame_11</c>, an installed program frame.</summary>
    Projectile86E314Frame11,
    /// <summary><c>projectile_86_e314_frame_12</c>, an installed program frame.</summary>
    Projectile86E314Frame12,
    /// <summary><c>projectile_86_e314_frame_13</c>, an installed program frame.</summary>
    Projectile86E314Frame13,
    /// <summary><c>projectile_86_e314_frame_14</c>, an installed program frame.</summary>
    Projectile86E314Frame14,
    /// <summary><c>projectile_86_e314_frame_15</c>, an installed program frame.</summary>
    Projectile86E314Frame15,
    /// <summary><c>projectile_86_e314_frame_16</c>, an installed program frame.</summary>
    Projectile86E314Frame16,
    /// <summary><c>projectile_86_e314_frame_17</c>, an installed program frame.</summary>
    Projectile86E314Frame17,
    /// <summary><c>projectile_86_e314_frame_18</c>, an installed program frame.</summary>
    Projectile86E314Frame18,
    /// <summary><c>projectile_86_e314_frame_19</c>, an installed program frame.</summary>
    Projectile86E314Frame19,
    /// <summary><c>projectile_86_e314_frame_20</c>, an installed program frame.</summary>
    Projectile86E314Frame20,
    /// <summary><c>projectile_86_e314_frame_21</c>, an installed program frame.</summary>
    Projectile86E314Frame21,
    /// <summary><c>projectile_86_e314_frame_22</c>, an installed program frame.</summary>
    Projectile86E314Frame22,
    /// <summary><c>projectile_86_e314_frame_23</c>, an installed program frame.</summary>
    Projectile86E314Frame23,
    /// <summary><c>projectile_86_e314_frame_24</c>, an installed program frame.</summary>
    Projectile86E314Frame24,
    /// <summary><c>projectile_86_e314_frame_25</c>, an installed program frame.</summary>
    Projectile86E314Frame25,
    /// <summary><c>projectile_86_e314_frame_26</c>, an installed program frame.</summary>
    Projectile86E314Frame26,
    /// <summary><c>projectile_86_e314_frame_27</c>, an installed program frame.</summary>
    Projectile86E314Frame27,
    /// <summary><c>projectile_86_e314_frame_28</c>, an installed program frame.</summary>
    Projectile86E314Frame28,
    /// <summary><c>projectile_86_e314_frame_29</c>, an installed program frame.</summary>
    Projectile86E314Frame29,
    /// <summary><c>projectile_86_e314_frame_30</c>, an installed program frame.</summary>
    Projectile86E314Frame30,
    /// <summary><c>projectile_86_e392_frame_00</c>, an installed program frame.</summary>
    Projectile86E392Frame00,
    /// <summary><c>projectile_86_e392_frame_01</c>, an installed program frame.</summary>
    Projectile86E392Frame01,
    /// <summary><c>projectile_86_e392_frame_02</c>, an installed program frame.</summary>
    Projectile86E392Frame02,
    /// <summary><c>projectile_86_e3a0_frame_00</c>, an installed program frame.</summary>
    Projectile86E3a0Frame00,
    /// <summary><c>projectile_86_e3a0_frame_01</c>, an installed program frame.</summary>
    Projectile86E3a0Frame01,
    /// <summary><c>projectile_86_e3a0_frame_02</c>, an installed program frame.</summary>
    Projectile86E3a0Frame02,
    /// <summary><c>projectile_86_e3a0_frame_03</c>, an installed program frame.</summary>
    Projectile86E3a0Frame03,
    /// <summary><c>projectile_86_e3a0_frame_04</c>, an installed program frame.</summary>
    Projectile86E3a0Frame04,
    /// <summary><c>projectile_86_e3a0_frame_05</c>, an installed program frame.</summary>
    Projectile86E3a0Frame05,
    /// <summary><c>projectile_86_e3a0_frame_06</c>, an installed program frame.</summary>
    Projectile86E3a0Frame06,
    /// <summary><c>projectile_86_e3a0_frame_07</c>, an installed program frame.</summary>
    Projectile86E3a0Frame07,
    /// <summary><c>projectile_86_e3a0_frame_08</c>, an installed program frame.</summary>
    Projectile86E3a0Frame08,
    /// <summary><c>projectile_86_e3c6_frame_00</c>, an installed program frame.</summary>
    Projectile86E3c6Frame00,
    /// <summary><c>projectile_86_e3c6_frame_01</c>, an installed program frame.</summary>
    Projectile86E3c6Frame01,
    /// <summary><c>projectile_86_e3c6_frame_02</c>, an installed program frame.</summary>
    Projectile86E3c6Frame02,
    /// <summary><c>projectile_86_e3c6_frame_03</c>, an installed program frame.</summary>
    Projectile86E3c6Frame03,
    /// <summary><c>projectile_86_e3c6_frame_04</c>, an installed program frame.</summary>
    Projectile86E3c6Frame04,
    /// <summary><c>projectile_86_e3c6_frame_05</c>, an installed program frame.</summary>
    Projectile86E3c6Frame05,
    /// <summary><c>projectile_86_e3c6_frame_06</c>, an installed program frame.</summary>
    Projectile86E3c6Frame06,
    /// <summary><c>projectile_86_e3c6_frame_07</c>, an installed program frame.</summary>
    Projectile86E3c6Frame07,
    /// <summary><c>projectile_86_e3e8_frame_00</c>, an installed program frame.</summary>
    Projectile86E3e8Frame00,
    /// <summary><c>projectile_86_e3e8_frame_01</c>, an installed program frame.</summary>
    Projectile86E3e8Frame01,
    /// <summary><c>projectile_86_e3e8_frame_02</c>, an installed program frame.</summary>
    Projectile86E3e8Frame02,
    /// <summary><c>projectile_86_e3e8_frame_03</c>, an installed program frame.</summary>
    Projectile86E3e8Frame03,
    /// <summary><c>projectile_86_e3e8_frame_04</c>, an installed program frame.</summary>
    Projectile86E3e8Frame04,
    /// <summary><c>projectile_86_e3e8_frame_05</c>, an installed program frame.</summary>
    Projectile86E3e8Frame05,
    /// <summary><c>projectile_86_e3e8_frame_06</c>, an installed program frame.</summary>
    Projectile86E3e8Frame06,
    /// <summary><c>projectile_86_e3e8_frame_07</c>, an installed program frame.</summary>
    Projectile86E3e8Frame07,
    /// <summary><c>projectile_86_e40a_frame_00</c>, an installed program frame.</summary>
    Projectile86E40aFrame00,
    /// <summary><c>projectile_86_e40a_frame_01</c>, an installed program frame.</summary>
    Projectile86E40aFrame01,
    /// <summary><c>projectile_86_e40a_frame_02</c>, an installed program frame.</summary>
    Projectile86E40aFrame02,
    /// <summary><c>projectile_86_e40a_frame_03</c>, an installed program frame.</summary>
    Projectile86E40aFrame03,
    /// <summary><c>projectile_86_e40a_frame_04</c>, an installed program frame.</summary>
    Projectile86E40aFrame04,
    /// <summary><c>projectile_86_e40a_frame_05</c>, an installed program frame.</summary>
    Projectile86E40aFrame05,
    /// <summary><c>projectile_86_e40a_frame_06</c>, an installed program frame.</summary>
    Projectile86E40aFrame06,
    /// <summary><c>projectile_86_e40a_frame_07</c>, an installed program frame.</summary>
    Projectile86E40aFrame07,
    /// <summary><c>downward_gate_frame_00</c>, an installed program frame.</summary>
    DownwardGateFrame00,
    /// <summary><c>downward_gate_frame_01</c>, an installed program frame.</summary>
    DownwardGateFrame01,
    /// <summary><c>downward_gate_frame_02</c>, an installed program frame.</summary>
    DownwardGateFrame02,
    /// <summary><c>downward_gate_frame_03</c>, an installed program frame.</summary>
    DownwardGateFrame03,
    /// <summary><c>downward_gate_frame_04</c>, an installed program frame.</summary>
    DownwardGateFrame04,
    /// <summary><c>downward_gate_frame_05</c>, an installed program frame.</summary>
    DownwardGateFrame05,
    /// <summary><c>downward_gate_frame_06</c>, an installed program frame.</summary>
    DownwardGateFrame06,
    /// <summary><c>downward_gate_frame_07</c>, an installed program frame.</summary>
    DownwardGateFrame07,
    /// <summary><c>downward_gate_frame_08</c>, an installed program frame.</summary>
    DownwardGateFrame08,
    /// <summary><c>save_station_electricity_frame_00</c>, an installed program frame.</summary>
    SaveStationElectricityFrame00,
    /// <summary><c>save_station_electricity_frame_01</c>, an installed program frame.</summary>
    SaveStationElectricityFrame01,
    /// <summary><c>save_station_electricity_frame_02</c>, an installed program frame.</summary>
    SaveStationElectricityFrame02,
    /// <summary><c>save_station_electricity_frame_03</c>, an installed program frame.</summary>
    SaveStationElectricityFrame03,
    /// <summary><c>save_station_electricity_frame_04</c>, an installed program frame.</summary>
    SaveStationElectricityFrame04,
    /// <summary><c>save_station_electricity_frame_05</c>, an installed program frame.</summary>
    SaveStationElectricityFrame05,
    /// <summary><c>save_station_electricity_frame_06</c>, an installed program frame.</summary>
    SaveStationElectricityFrame06,
    /// <summary><c>save_station_electricity_frame_07</c>, an installed program frame.</summary>
    SaveStationElectricityFrame07,
    /// <summary><c>botwoon_frame_00</c>, an installed program frame.</summary>
    BotwoonFrame00,
    /// <summary><c>botwoon_frame_01</c>, an installed program frame.</summary>
    BotwoonFrame01,
    /// <summary><c>botwoon_frame_02</c>, an installed program frame.</summary>
    BotwoonFrame02,
    /// <summary><c>botwoon_frame_03</c>, an installed program frame.</summary>
    BotwoonFrame03,
    /// <summary><c>botwoon_frame_04</c>, an installed program frame.</summary>
    BotwoonFrame04,
    /// <summary><c>botwoon_frame_05</c>, an installed program frame.</summary>
    BotwoonFrame05,
    /// <summary><c>botwoon_frame_06</c>, an installed program frame.</summary>
    BotwoonFrame06,
    /// <summary><c>botwoon_frame_07</c>, an installed program frame.</summary>
    BotwoonFrame07,
    /// <summary><c>botwoon_frame_08</c>, an installed program frame.</summary>
    BotwoonFrame08,
    /// <summary><c>botwoon_frame_09</c>, an installed program frame.</summary>
    BotwoonFrame09,
    /// <summary><c>botwoon_frame_10</c>, an installed program frame.</summary>
    BotwoonFrame10,
    /// <summary><c>botwoon_frame_11</c>, an installed program frame.</summary>
    BotwoonFrame11,
    /// <summary><c>botwoon_frame_12</c>, an installed program frame.</summary>
    BotwoonFrame12,
    /// <summary><c>botwoon_frame_13</c>, an installed program frame.</summary>
    BotwoonFrame13,
    /// <summary><c>botwoon_frame_14</c>, an installed program frame.</summary>
    BotwoonFrame14,
    /// <summary><c>botwoon_frame_15</c>, an installed program frame.</summary>
    BotwoonFrame15,
    /// <summary><c>botwoon_frame_16</c>, an installed program frame.</summary>
    BotwoonFrame16,
    /// <summary><c>botwoon_frame_17</c>, an installed program frame.</summary>
    BotwoonFrame17,
    /// <summary><c>botwoon_frame_18</c>, an installed program frame.</summary>
    BotwoonFrame18,
    /// <summary><c>botwoon_frame_19</c>, an installed program frame.</summary>
    BotwoonFrame19,
    /// <summary><c>botwoon_frame_20</c>, an installed program frame.</summary>
    BotwoonFrame20,
    /// <summary><c>botwoon_frame_21</c>, an installed program frame.</summary>
    BotwoonFrame21,
    /// <summary><c>botwoon_frame_22</c>, an installed program frame.</summary>
    BotwoonFrame22,
    /// <summary><c>botwoon_frame_23</c>, an installed program frame.</summary>
    BotwoonFrame23,
    /// <summary><c>botwoon_frame_24</c>, an installed program frame.</summary>
    BotwoonFrame24,
    /// <summary><c>botwoon_frame_25</c>, an installed program frame.</summary>
    BotwoonFrame25,
    /// <summary><c>botwoon_frame_26</c>, an installed program frame.</summary>
    BotwoonFrame26,
    /// <summary><c>botwoon_frame_27</c>, an installed program frame.</summary>
    BotwoonFrame27,
    /// <summary><c>botwoon_frame_28</c>, an installed program frame.</summary>
    BotwoonFrame28,
    /// <summary><c>botwoon_frame_29</c>, an installed program frame.</summary>
    BotwoonFrame29,
    /// <summary><c>botwoon_frame_30</c>, an installed program frame.</summary>
    BotwoonFrame30,
    /// <summary><c>botwoon_frame_31</c>, an installed program frame.</summary>
    BotwoonFrame31,
    /// <summary><c>botwoon_frame_32</c>, an installed program frame.</summary>
    BotwoonFrame32,
    /// <summary><c>botwoon_frame_33</c>, an installed program frame.</summary>
    BotwoonFrame33,
    /// <summary><c>botwoon_frame_34</c>, an installed program frame.</summary>
    BotwoonFrame34,
    /// <summary><c>botwoon_frame_35</c>, an installed program frame.</summary>
    BotwoonFrame35,
    /// <summary><c>botwoon_frame_36</c>, an installed program frame.</summary>
    BotwoonFrame36,
    /// <summary><c>botwoon_frame_37</c>, an installed program frame.</summary>
    BotwoonFrame37,
    /// <summary><c>botwoon_frame_38</c>, an installed program frame.</summary>
    BotwoonFrame38,
    /// <summary><c>botwoon_frame_39</c>, an installed program frame.</summary>
    BotwoonFrame39,
    /// <summary><c>botwoon_frame_40</c>, an installed program frame.</summary>
    BotwoonFrame40,
    /// <summary><c>botwoon_frame_41</c>, an installed program frame.</summary>
    BotwoonFrame41,
    /// <summary><c>botwoon_frame_42</c>, an installed program frame.</summary>
    BotwoonFrame42,
    /// <summary><c>botwoon_frame_43</c>, an installed program frame.</summary>
    BotwoonFrame43,
    /// <summary><c>botwoon_frame_44</c>, an installed program frame.</summary>
    BotwoonFrame44,
    /// <summary><c>botwoon_frame_45</c>, an installed program frame.</summary>
    BotwoonFrame45,
    /// <summary><c>yapping_maw_frame_00</c>, an installed program frame.</summary>
    YappingMawFrame00,
    /// <summary><c>yapping_maw_frame_01</c>, an installed program frame.</summary>
    YappingMawFrame01,
    /// <summary><c>enemy_death_frame_00</c>, an installed program frame.</summary>
    EnemyDeathFrame00,
    /// <summary><c>enemy_death_frame_01</c>, an installed program frame.</summary>
    EnemyDeathFrame01,
    /// <summary><c>enemy_death_frame_02</c>, an installed program frame.</summary>
    EnemyDeathFrame02,
    /// <summary><c>enemy_death_frame_03</c>, an installed program frame.</summary>
    EnemyDeathFrame03,
    /// <summary><c>enemy_death_frame_04</c>, an installed program frame.</summary>
    EnemyDeathFrame04,
    /// <summary><c>enemy_death_frame_05</c>, an installed program frame.</summary>
    EnemyDeathFrame05,
    /// <summary><c>enemy_death_frame_06</c>, an installed program frame.</summary>
    EnemyDeathFrame06,
    /// <summary><c>enemy_death_frame_07</c>, an installed program frame.</summary>
    EnemyDeathFrame07,
    /// <summary><c>enemy_death_frame_08</c>, an installed program frame.</summary>
    EnemyDeathFrame08,
    /// <summary><c>enemy_death_frame_09</c>, an installed program frame.</summary>
    EnemyDeathFrame09,
    /// <summary><c>enemy_death_frame_10</c>, an installed program frame.</summary>
    EnemyDeathFrame10,
    /// <summary><c>enemy_death_frame_11</c>, an installed program frame.</summary>
    EnemyDeathFrame11,
    /// <summary><c>enemy_death_frame_12</c>, an installed program frame.</summary>
    EnemyDeathFrame12,
    /// <summary><c>enemy_death_frame_13</c>, an installed program frame.</summary>
    EnemyDeathFrame13,
    /// <summary><c>enemy_death_frame_14</c>, an installed program frame.</summary>
    EnemyDeathFrame14,
    /// <summary><c>enemy_pickup_frame_00</c>, an installed program frame.</summary>
    EnemyPickupFrame00,
    /// <summary><c>enemy_pickup_frame_01</c>, an installed program frame.</summary>
    EnemyPickupFrame01,
    /// <summary><c>enemy_pickup_frame_02</c>, an installed program frame.</summary>
    EnemyPickupFrame02,
    /// <summary><c>enemy_pickup_frame_03</c>, an installed program frame.</summary>
    EnemyPickupFrame03,
    /// <summary><c>enemy_pickup_frame_04</c>, an installed program frame.</summary>
    EnemyPickupFrame04,
    /// <summary><c>enemy_pickup_frame_05</c>, an installed program frame.</summary>
    EnemyPickupFrame05,
    /// <summary><c>enemy_pickup_frame_06</c>, an installed program frame.</summary>
    EnemyPickupFrame06,
    /// <summary><c>enemy_pickup_frame_07</c>, an installed program frame.</summary>
    EnemyPickupFrame07,
    /// <summary><c>enemy_pickup_frame_08</c>, an installed program frame.</summary>
    EnemyPickupFrame08,
    /// <summary><c>enemy_pickup_frame_09</c>, an installed program frame.</summary>
    EnemyPickupFrame09,
    /// <summary><c>enemy_pickup_frame_10</c>, an installed program frame.</summary>
    EnemyPickupFrame10,
    /// <summary><c>enemy_pickup_frame_11</c>, an installed program frame.</summary>
    EnemyPickupFrame11,
    /// <summary><c>enemy_pickup_frame_12</c>, an installed program frame.</summary>
    EnemyPickupFrame12,
    /// <summary><c>enemy_pickup_frame_13</c>, an installed program frame.</summary>
    EnemyPickupFrame13,
    /// <summary><c>enemy_pickup_frame_14</c>, an installed program frame.</summary>
    EnemyPickupFrame14,
    /// <summary><c>enemy_pickup_frame_15</c>, an installed program frame.</summary>
    EnemyPickupFrame15,
    /// <summary><c>enemy_death_frame_15</c>, an installed program frame.</summary>
    EnemyDeathFrame15,
    /// <summary><c>enemy_death_frame_16</c>, an installed program frame.</summary>
    EnemyDeathFrame16,
    /// <summary><c>enemy_death_frame_17</c>, an installed program frame.</summary>
    EnemyDeathFrame17,
    /// <summary><c>enemy_death_frame_18</c>, an installed program frame.</summary>
    EnemyDeathFrame18,
    /// <summary><c>enemy_death_frame_19</c>, an installed program frame.</summary>
    EnemyDeathFrame19,
    /// <summary><c>enemy_death_frame_20</c>, an installed program frame.</summary>
    EnemyDeathFrame20,
    /// <summary><c>enemy_death_frame_21</c>, an installed program frame.</summary>
    EnemyDeathFrame21,
    /// <summary><c>enemy_death_frame_22</c>, an installed program frame.</summary>
    EnemyDeathFrame22,
    /// <summary><c>enemy_death_frame_23</c>, an installed program frame.</summary>
    EnemyDeathFrame23,
    /// <summary><c>enemy_death_frame_24</c>, an installed program frame.</summary>
    EnemyDeathFrame24,
    /// <summary><c>enemy_death_frame_25</c>, an installed program frame.</summary>
    EnemyDeathFrame25,
    /// <summary><c>enemy_death_frame_26</c>, an installed program frame.</summary>
    EnemyDeathFrame26,
    /// <summary><c>enemy_death_frame_27</c>, an installed program frame.</summary>
    EnemyDeathFrame27,
    /// <summary><c>enemy_death_frame_28</c>, an installed program frame.</summary>
    EnemyDeathFrame28,
    /// <summary><c>enemy_death_frame_29</c>, an installed program frame.</summary>
    EnemyDeathFrame29,
    /// <summary><c>enemy_death_frame_30</c>, an installed program frame.</summary>
    EnemyDeathFrame30,
    /// <summary><c>falling_spark_frame_00</c>, an installed program frame.</summary>
    FallingSparkFrame00,
    /// <summary><c>falling_spark_frame_01</c>, an installed program frame.</summary>
    FallingSparkFrame01,
    /// <summary><c>falling_spark_frame_02</c>, an installed program frame.</summary>
    FallingSparkFrame02,
    /// <summary><c>falling_spark_frame_03</c>, an installed program frame.</summary>
    FallingSparkFrame03,
    /// <summary><c>falling_spark_frame_04</c>, an installed program frame.</summary>
    FallingSparkFrame04,
    /// <summary><c>falling_spark_frame_05</c>, an installed program frame.</summary>
    FallingSparkFrame05,
    /// <summary><c>falling_spark_frame_06</c>, an installed program frame.</summary>
    FallingSparkFrame06,
    /// <summary><c>falling_spark_frame_07</c>, an installed program frame.</summary>
    FallingSparkFrame07,
    /// <summary><c>falling_spark_frame_08</c>, an installed program frame.</summary>
    FallingSparkFrame08,
    /// <summary><c>falling_spark_frame_09</c>, an installed program frame.</summary>
    FallingSparkFrame09,
    /// <summary><c>falling_spark_frame_10</c>, an installed program frame.</summary>
    FallingSparkFrame10,
    /// <summary><c>falling_spark_frame_11</c>, an installed program frame.</summary>
    FallingSparkFrame11,
    /// <summary><c>falling_spark_frame_12</c>, an installed program frame.</summary>
    FallingSparkFrame12,
    /// <summary><c>falling_spark_frame_13</c>, an installed program frame.</summary>
    FallingSparkFrame13,
}
