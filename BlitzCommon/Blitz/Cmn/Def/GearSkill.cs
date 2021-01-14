namespace BlitzCommon.Blitz.Cmn.Def
{
    public enum GearSkill : int
    {
        Locked = -1,
        MainInk_Save = 0,
        SubInk_Save,
        InkRecovery_Up,
        HumanMove_Up,
        SquidMove_Up,
        SpecialIncrease_Up,
        RespawnSpecialGauge_Save,
        SpecialTime_Up,
        RespawnTime_Save,
        JumpTime_Save,
        BombDistance_Up,
        OpInkEffect_Reduction,
        BombDamage_Reduction, // 4.3.0+: Bomb Defense Up DX
        MarkingTime_Reduction, // 4.3.0+: Main Power Up

        // Cmn::Def::SpecialSkill
        StartAllUp = 100,
        EndAllUp,
        MinorityUp,
        ComeBack,
        SquidMoveSpatter_Reduction,
        DeathMarking,
        ThermalInk,
        Exorcist,
        ExSkillDouble,
        SuperJumpSign_Hide,
        ObjectEffect_Up,
        SomersaultLanding,
        RespawnRadar
    }
}
