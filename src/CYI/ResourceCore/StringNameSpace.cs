using System.Collections.Generic;

public static class StringAdrScene
{
    public const string StartScene = "Scene_Start";
    public const string LobbyScene = "Scene_Lobby";
    public const string GameScene = "Scene_Game";
    public const string EndingScene = "Scene_Ending";
}

public static class StringPrefs
{
    public static readonly Dictionary<SoundType, string> SoundTypeDict = new()
    {
        { SoundType.Bgm, "BgmVolume" },
        { SoundType.Sfx, "SfxVolume" }
    };
}

/// <summary>
/// 어드레서블 라벨 Front
/// 해당 씬 종류
/// </summary>
public static class StringAdrLabelFront
{
    public const string Atlas = "Label_Atlas";
    
    public const string Default = "Label_Default";
    public const string EntryScene = "Label_EntryScene";   
    public const string StartScene = "Label_StartScene";
    public const string LobbyScene = "Label_LobbyScene";
    public const string GameScene = "Label_GameScene";
    public const string EndingScene = "Label_EndingScene";
}

/// <summary>
/// 어드레서블 라벨 Back
/// 로드하려는 타입
/// </summary>
public static class StringAdrLabelBack
{
    public const string Object = "_Obj";
    public const string Sprite = "_Sprite";
    public const string AudioClip = "_AudioClip";
}

#region Addressable Sprite Address

// 네이밍 규칙을 정해서 
// 코드로 마지막 string을 검사해서 
// 확장성 고려하면 이렇게 하면 됨
// 씬에서 필요한 애들만 가져와서  (SceneName)
// 

public static class StringAdrBg
{
    public static readonly List<string> ChapterList = new ()
    {
        "BG_Chapter1", // Default
        "BG_Chapter1",
        "BG_Chapter2",
        "BG_Chapter3",
    };
    
    public static readonly List<string> ChapterBossList = new ()
    {
        "BG_Chapter1Boss", // Default
        "BG_Chapter1Boss",
        "BG_Chapter2Boss",
        "BG_Chapter3Boss"
    };
    
    public static readonly List<string> ChapterBlurList = new ()
    {
        "BG_Chapter1_Blur", // Default
        "BG_Chapter1_Blur",
        "BG_Chapter2_Blur",
        "BG_Chapter3_Blur",
    };
    public static readonly List<string> ChapterBossBlurList = new ()
    {
        "BG_Chapter1Boss_Blur", // Default
        "BG_Chapter1Boss_Blur",
        "BG_Chapter2Boss_Blur",
        "BG_Chapter3Boss_Blur"
    };
    
    public static readonly List<string> ChapterSelectList = new ()
    {
        "BG_Chapter1_Select", // Default
        "BG_Chapter1_Select",
        "BG_Chapter2_Select",
        "BG_Chapter3_Select"
    };
    
    public const string Loading = "BG_Loading";
    public const string LoadingBlur = "BG_Loading_Blur";
    public const string Lobby = "BG_Lobby";
    public const string LobbyBlur = "BG_Lobby_Blur";
    
    public const string Gacha = "BG_Gacha";
    public const string GachaBlur = "BG_Gacha_Blur";
    public const string Blacksmith = "BG_Blacksmith";
    
    public const string StageReady = "BG_StageReady";
}

public static class StringAdrEntity
{
    public static readonly Dictionary<string, string> EntityDict = new()
    {
        { "UNIT_1001", "Sprite_Unit_1001" },
        { "UNIT_1002", "Sprite_Unit_1002" },
        { "UNIT_1003", "Sprite_Unit_1003" },
        { "MONS_0001", "Sprite_Monster_0001" },
        { "MONS_0002", "Sprite_Monster_0002" },
        { "MONS_0003", "Sprite_Monster_0003" },
        { "MONS_0004", "Sprite_Monster_0004" },
        { "MONS_0005", "Sprite_Monster_0005" },
        { "MONS_0006", "Sprite_Monster_0006" },
        { "MONS_0007", "Sprite_Monster_0007" },
        { "MONS_0008", "Sprite_Monster_0008" },
        { "MONS_0009", "Sprite_Monster_0009" },
        { "MONS_0010", "Sprite_Monster_0010" },
        { "MONS_0011", "Sprite_Monster_0011" },
    };

    public static readonly Dictionary<string, string> EntityShadeDict = new()
    {
        // {"UNIT_0001", "Sprite_UnitShade_0001"},
        // {"UNIT_0002", "Sprite_UnitShade_0002"},
        // {"UNIT_0003", "Sprite_UnitShade_0003"},
        { "MONS_0001", "Sprite_MonsterShade_0001" },
        { "MONS_0002", "Sprite_MonsterShade_0002" },
        { "MONS_0003", "Sprite_MonsterShade_0003" },
        { "MONS_0004", "Sprite_MonsterShade_0004" },
        { "MONS_0005", "Sprite_MonsterShade_0005" },
        { "MONS_0006", "Sprite_MonsterShade_0006" },
        { "MONS_0007", "Sprite_MonsterShade_0007" },
        { "MONS_0008", "Sprite_MonsterShade_0008" },
        { "MONS_0009", "Sprite_MonsterShade_0009" },
        { "MONS_0010", "Sprite_MonsterShade_0010" },
        { "MONS_0011", "Sprite_MonsterShade_0011" },
    };
}

public static class StringAdrUI
{
    public const string Catsite = "UI_Catsite";
    public const string CatsiteShade = "UI_CatsiteShade";
    public const string CatsiteSelected = "UI_CatsiteSelected";
    
    public const string UnitSlot = "UI_UnitSlot";
    public const string UnitSlotSelected = "UI_UnitSlot_Selected";
    
    public const string AutoToggleOn = "UI_AutoToggleOn";
    public const string AutoToggleOff = "UI_AutoToggleOff";
    
    public const string Victory = "UI_Victory";
    public const string Defeat = "UI_Defeat";
    
    public const string TurnUser = "UI_TurnUser";
    public const string TurnMonster = "UI_TurnMonster";

    public static readonly List<string> BossHpGauge = new()
    {
        "UI_BossHpGauge01", // 100
        "UI_BossHpGauge02", // 70
        "UI_BossHpGauge03", // 50
        "UI_BossHpGauge04"  // 30
    };
}

public static class StringAdrItemBg
{
    public static readonly Dictionary<ItemRarity, string> ItemFrameDict = new()
    {
        { ItemRarity.None, "UI_Frame_ITEM_Common" },
        { ItemRarity.Common, "UI_Frame_ITEM_Common" },
        { ItemRarity.Uncommon, "UI_Frame_ITEM_Uncommon" },
        { ItemRarity.Rare, "UI_Frame_ITEM_Rare" },
        { ItemRarity.SuperRare, "UI_Frame_ITEM_SuperRare" },
        { ItemRarity.Legendary, "UI_Frame_ITEM_Legendary" }
    };
}

public static class StringAdrCardBg
{
    public static readonly Dictionary<ItemRarity, string> GoldCardBgDict = new()
    {
        { ItemRarity.Common, "BG_GoldCard_Common" },
        { ItemRarity.Uncommon, "BG_GoldCard_Uncommon" },
        { ItemRarity.Rare, "BG_GoldCard_Rare" },
        { ItemRarity.SuperRare, "BG_GoldCard_SuperRare" },
        { ItemRarity.Legendary, "BG_GoldCard_Legendary" }
    };

    public static readonly Dictionary<ItemRarity, string> DiaCardBgDict = new()
    {
        { ItemRarity.Common, "BG_DiaCard_Common" },
        { ItemRarity.Uncommon, "BG_DiaCard_Uncommon" },
        { ItemRarity.Rare, "BG_DiaCard_Rare" },
        { ItemRarity.SuperRare, "BG_DiaCard_SuperRare" },
        { ItemRarity.Legendary, "BG_DiaCard_Legendary" }
    };

    public const string GoldFront = "BG_GoldCard_Front";
    public const string DiaFront = "BG_DiaCard_Front";
}

public static class StringAdrIcon
{
    public static readonly Dictionary<string, string> UnitIconDict = new()
    {
        { "UNIT_1001", "UI_Icon_UNIT_1001" },
        { "UNIT_1002", "UI_Icon_UNIT_1002" },
        { "UNIT_1003", "UI_Icon_UNIT_1003" }
    };
    
    public static readonly Dictionary<string, string> UnitSkillShade = new()
    {
        { "UNIT_1001", "Icon_Shade_SKIL_0001" },
        { "UNIT_1002", "Icon_Shade_SKIL_0002" },
        { "UNIT_1003", "Icon_Shade_SKIL_0003" }
    };
    
    public static readonly Dictionary<ResourceType, string> ResourceDict = new()
    {
        { ResourceType.Gold, "Icon_Gold" },
        { ResourceType.Diamond, "Icon_Dia" },
        { ResourceType.Piece, "Icon_Piece" }
    };

    public static readonly Dictionary<RewardType, string> RewardDict = new()
    {
        { RewardType.Gold, "Icon_Gold" },
        { RewardType.Diamond, "Icon_Dia" },
        { RewardType.Exp, "Icon_Exp" },
        { RewardType.Piece, "Icon_Piece" }
    };

    public static readonly List<string> UserIcon = new()
    {
        "Icon_User01",
        "Icon_User02",
        "Icon_User03",
        "Icon_User04",
        "Icon_User05",
        "Icon_User06",
        "Icon_User07",
        "Icon_User08",
        "Icon_User09"
    };
}

public static class StringAdrAudioBgm
{
    public const string EntryScene = "BGM_EntryScene";
    public const string LobbyScene = "BGM_LobbyScene";
    public const string BattleScene = "BGM_BattleScene";
    public const string BattleBoss = "BGM_BattleBoss";
    public const string BattleReady = "BGM_BattleReady";
    public const string BlackSmith = "BGM_Blacksmith";
    public const string Gacha = "BGM_Gacha";
    public const string EndingScene = "BGM_EndingScene";
}

public static class StringAdrAudioSfx
{
    public const string ButtonClick = "SFX_ButtonClick";
    public const string BattleWin = "SFX_BattleWin";
    public const string BattleLose = "SFX_BattleLose";
    public const string AttackMelee = "SFX_AttackMelee";
    public const string AttackRange = "SFX_AttackRange";
    public const string AttackMonster = "SFX_AttackMonster";
    public const string UsherSkill = "SFX_UsherSkill";
    public const string MomoSkill = "SFX_MomoSkill";
    public const string RuruSkill = "SFX_RuruSkill";
    public const string GetReward = "SFX_GetReward";
    public const string Equipment = "SFX_Equipment";
    public const string Open = "SFX_Open";
    public const string LimitBreak = "SFX_Bs_LimitBreak";
    public const string EnhancementTry = "SFX_Bs_EnhancementTry";
    public const string EnhancementSuccess = "SFX_Bs_EnhancementSuccess";
    public const string EnhancementFail = "SFX_Bs_EnhancementFail";
    public const string Dismantle = "SFX_Bs_Dismantle";
    public const string UsherMeow = "SFX_UsherMeow";
    public const string MomoMeow = "SFX_MomoMeow";
    public const string RuruMeow = "SFX_RuruMeow";
    public const string SkillMons0003 = "SFX_Mons_Skill0003";
    public const string SkillMons0004 = "SFX_Mons_Skill0004";
    public const string SkillMons0005 = "SFX_Mons_Skill0005";
    public const string SkillMons0006 = "SFX_Mons_Skill0006";
    public const string SkillMons0007 = "SFX_Mons_Skill0007";
    public const string SkillMons0008 = "SFX_Mons_Skill0008";
    public const string SkillMons0009 = "SFX_Mons_Skill0009";
    public const string SkillMons0010 = "SFX_Mons_Skill0010";
    public const string SkillMons0011 = "SFX_Mons_Skill0011";
    public const string AttackMons0004 = "SFX_Mons_Attack0004";
    public const string AttackMons0007 = "SFX_Mons_Attack0007";
    public const string AttackMons0011 = "SFX_Mons_Attack0011";
    public const string MonsterHit = "SFX_Mons_GetHit";
    
    public static readonly string[] RandomUnitAttackSfx = 
    {
        "SFX_CatAttack01",
        "SFX_CatAttack02",
        "SFX_CatAttack03",
        "SFX_CatAttack04",
        "SFX_CatAttack05",
    };
    public static readonly string[] RandomUnitHitSfx = 
    {
        "SFX_CatHit01", "SFX_CatHit02", "SFX_CatHit03", "SFX_CatHit04",
        "SFX_CatHit05", "SFX_CatHit06", "SFX_CatHit07", "SFX_CatHit08"
    };
}

public static class StringAdrEffect
{
    public const string Touch = "Effect_Touch";
    public const string Drag = "Effect_Drag";
    
    public const string EffectAttack = "Effect_Attack";
    public const string EffectSkill = "Effect_Skill";
    public const string EffectStatus = "Effect_Status";
}

public static class StringAdrCutin
{
    public static readonly Dictionary<string, List<string>> UnitSkillDict = new()
    {
        { "UNIT_1001", new(){"Cutin_UNIT_1001_BG","Cutin_UNIT_1001_Entity","Cutin_UNIT_1001_Shine","Cutin_UNIT_1001_Slash_Bottom","Cutin_UNIT_1001_Slash_Top"}},
        { "UNIT_1002", new(){"Cutin_UNIT_1002_BG","Cutin_UNIT_1002_Entity","Cutin_UNIT_1002_Shine","Cutin_UNIT_1002_Slash_Bottom","Cutin_UNIT_1002_Slash_Top"}},
        { "UNIT_1003", new(){"Cutin_UNIT_1003_BG","Cutin_UNIT_1003_Entity","Cutin_UNIT_1003_Shine","Cutin_UNIT_1003_Slash_Bottom","Cutin_UNIT_1003_Slash_Top"}}
    };
}

#endregion

#region FireStore

public static class FirestoreCollection
{
    // 1. DATA
    public const string Data = "DATA";
    
    // DATA-ROWDATA내의 컬렉션 목록
    public const string Item = "ITEM";
    public const string Monster = "MONSTER";
    public const string Stage = "STAGE";
    public const string Unit = "UNIT";
    public const string Skill = "SKILL";
    public const string LevelUpExp  = "LEVELUPEXP";
    public const string Enhancement =  "ENHANCEMENT";
    public const string LimitBreak = "LIMITBREAK";
    public const string Collection = "COLLECTION";
    public const string ItemDismantle =  "ITEMDISMANTLE";
    public const string Guide = "GUIDE";

    // 2. TABLE
    public const string Table = "TABLE";
    
    // 3. USER
    public const string User = "USER";
    
    // USER-UID내의 컬렉션 목록
    // 3-1. ACCOUNT
    public const string Account = "ACCOUNT";
    
    // 3-2. SAVE
    public const string Save = "SAVE";
    
    public const string Items =  "ITEMS";
    public const string Units =  "UNITS";
    public const string Collects =  "COLLECTS";
    public const string Progresses =  "PROGRESSES";
}

public static class FirestoreDocument
{
    public const string RowData = "ROWDATA";
    
    public const string Gacha = "GACHA";
    public const string UnLock = "UNLOCK";
    
    // 3-1 ACCOUNT 내의 문서
    public const string Profile = "PROFILE";
    
    // 3-2 SAVE 내의 문서
    public const string Inventory = "INVENTORY";
    public const string Info = "INFO";
    public const string Stage = "STAGE";
    public const string Collected = "COLLECTED";
}

public static class FirebaseCondition
{
    public const string stageTitle = "stageTitle";
}

public static class AnalyticsMainScreen
{
    public const string Main = "Main";
    public const string Collection = "Collection";
    public const string Inventory = "Inventory";
    public const string UnitInventory = "UnitInvnetory";
}

public static class AnalyticsGachaScreen
{
    public const string GachaMain = "GachaMain";
    public const string GachaChooseGold = "GachaChooseGold";
    public const string GachaChooseDia = "GachaChooseDia";
    public const string GachaCeilingPopup = "GachaCeilingPopup";
}

public static class AnalyticsBattleScreen
{
    public const string SelectStage = "SelectStage";
    public const string BattleReady = "BattleReady";
    public const string Win = "Win";
    public const string Lose = "Lose";
}

public static class AnalyticsBSScreen
{
    public const string BSMain = "BSMain";
}

public static class AnalyticsEvent
{
    public const string GachaStart = "gacha_start";
    public const string GachaResult = "gacha_result";
    public const string GachaCelling = "gacha_celling";
}

public static class PlayerUnitCode
{
    public const string Usher = "UNIT_1001";
    public const string Momo = "UNIT_1002";
    public const string Ruru = "UNIT_1003";
    
}

public static class MonsterCode
{
    public const string Cocoon = "MONS_0001";
    public const string WingedCocoon = "MONS_0002";
    public const string PurpleMushroom = "MONS_0004";
    public const string GreenMushroom = "MONS_0005";
    public const string HappySeedling = "MONS_0006";
    public const string GuardianOfSilence = "MONS_0008";
    public const string DontCryingDeer = "MONS_0009";
    public const string SilentHummingbird = "MONS_0010";
}
public static class BossMonsterCode
{
    public const string Boss1 = "MONS_0003";
    public const string Boss2 = "MONS_0007";
    public const string Boss3 = "MONS_0011";
}

public static class SignUpData
{
    public const string DefaultUnitCode = "UNIT_1001";
    public const string DefaultWeaponCode = "ITEM_0001";
    public const string DefaultUArmorCode = "ITEM_0006";
    public const int DefaultGold = 30000;
    public const int DefaultDiamond = 300;
}

#endregion
