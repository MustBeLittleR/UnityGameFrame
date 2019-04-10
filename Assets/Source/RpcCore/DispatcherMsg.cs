using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;

/// <summary>
/// add by zhongcy
/// 协议来源于C++头文件
/// </summary>
namespace WCG
{
	public struct TBasePrtlMsg
	{
		public Byte m_uId;
	}

	/// <summary>
	/// 类型头uint8 : ER2CGameProtocol Rcp2CltProtocol.h  /////////////////////
	/// </summary>
	enum ER2CGameProtocol
	{
		eR2C_RcpReturn = 0,
		eR2C_ProtocolCount
	};

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CR2CRcpReturn
	{
		public Int32 uRcpId;
		public Int32 uRcpCode;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public Char[] szIp;

		public Int32 uRcpTime;
	}

	/// <summary>
	/// 类型头uint8 : EClt2CnsProtocol ////////////////////////////////////////////////////////////////
	/// </summary>
	// 先读取最开始uint8数据来判断类型后 再进行解码
	enum EClt2CnsProtocol
	{
		eC2CS_Ping = 0,
		eC2CS_GameLogicCPtr,
		eC2CS_GameLogicLuaPtr,
		eC2CS_GameLoginLuaPtr,
		eC2CS_GameUserLogin,
		eC2CS_NpCheck,
		eC2CS_ProtocolCount
	};

	public struct C2CS_Ping
	{
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct C2CS_GameLogicLuaPtr
	{
		public Int16 uDataLength;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct C2CS_GameLogicCPtr
	{
		public Byte uDataLength;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct C2CS_GameLoginLuaPtr
	{
		public Int16 uDataLength;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct C2CS_GameUserLogin
	{
		public Int32 uRcpId;

		public Int32 nRcpCode;
		public Int32 uRcpTime;
		public Int32 nUserId;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public Byte[] szPassword;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public Byte[] szIp;

		public Int32 uMachineId;
		public Int32 uMachineIdHash;

		public Byte nVersionLength;
		public Byte uPassportLength;

		[MarshalAs(UnmanagedType.I1)]
		public Boolean bCLLogin;
        public Int32 uChannelId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public Byte[] sVersion;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public Byte[] sPassport;
    }

	/// <summary>
	/// 类型头uint8 : TCns2GasProtocol ////////////////////////////////////////////////////////////////
	/// </summary>
	enum ECns2CltProtocol
	{
		eCS2C_Ping = 0,
		eCS2C_GameLogicCPtr,
		eCS2C_GameLogicLuaPtr,

		eCS2C_CnsLoginLuaPtr,
		eCs2C_NpCheck,
		eCs2C_NpCheckFail,

		eCS2C_ProtocolCount
	};


	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CPing
	{
		public int GetTotalLen() { return 0; }
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGameLogicCPtr
	{
		public int uDataLength;
		public int GetTotalLen() { return uDataLength; }
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CCS2CGameLogicLuaPtr
	{
		public int uDataLength;
		public int GetTotalLen() { return uDataLength; }
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CCS2CCnsLoginLuaPtr
	{
		public int uDataLength;
		public int GetTotalLen() { return uDataLength; }
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CCS2CNpCheck
	{
		public Int32 dwIndex;
		public Int32 dwValue1;
		public Int32 dwValue2;
		public Int32 dwValue3;
		public int GetTotalLen() { return 0; }
	}

	public struct CCS2CNpCheckFail
	{
		public int GetTotalLen() { return 0; }
	}


	/// <summary>
	/// 类型头uint8 : EC2SGameProtocol C2SGameProtocol.h  /////////////////////
	/// </summary>
	enum EC2SGameProtocol
	{
		eC2SG_LuaModify = 0,
		eC2SG_MoveBeign,
		eC2SG_ChangePath,
		eC2SG_MoveStep,
		eC2SG_MoveEnd,
		eC2SG_StopMove,

		eC2SG_FollowerCreatePathFailed,

		eC2SG_LaunchSkill,
        eC2SG_SyncCurSkill,
        eC2SG_CancelCurSkill,

        eC2SG_ProtocolCount
	};

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CC2SGLuaModify
	{
		public Byte type;
	}


	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CC2SGMoveBeign
	{
		public Byte type;
		public UInt32 uPixelBeginX;
		public UInt32 uPixelBeginY;
		public UInt32 uPixelEndX;
		public UInt32 uPixelEndY;
		//bool bIgnoreBlock;				// 玩家不可能忽略阻挡
		//bool bIgnoreFlyBlock;

		[MarshalAs(UnmanagedType.I1)]
		public Boolean bForceBresenham;
		public Byte uGap;
		public Byte uPathFindType;
		//uint16 uSpeed;						// 暂时不需要
		public Double dPathLen;                 // 路径长度	

		public int GetTotalLen() { return 0; }
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CC2SGMoveEnd
	{
		public Byte type;
		public UInt32 uPixelEndX;
		public UInt32 uPixelEndY;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CC2SGMoveStep
	{
		public Byte type;
		public Int16 nOfsetPixelPosX;
		public Int16 nOfsetPixelPosY;
		public Double dPathLen;
		public UInt16 uSpeed;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CC2SGStopMove
	{
		public Byte type;
		public UInt32 uPixelEndX;
		public UInt32 uPixelEndY;
		public Double dStopRate;
	}


	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CC2SGChangePath
	{
		public Byte type;
		public UInt32 uPixelBeginX;
		public UInt32 uPixelBeginY;
		public UInt32 uPixelEndX;
		public UInt32 uPixelEndY;
		public Int16 nPixelOffsetX;
		public Int16 nPixelOffsetY;
		//bool bIgnoreFlyBlock;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bForceBresenham;
		public Byte uGap;
		public Byte uPathFindType;
		//uint16 uSpeed;				// 暂时不要
		public Double dPathLen;                 // 路径长度
		public Double dChangePathRate;      // 在上一条路径切换时候的进度

		public int GetTotalLen() { return 0; }
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CC2SGFollowerCreatePathFailed
	{
		public Byte type;
		public UInt32 uGlobalId;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CC2SGLaunchSkill
	{
		public Byte type;
		public UInt32 nTargetGlobalId;
		public Int32 nSkillId;
		public UInt32 nxPos;
		public UInt32 nzPos;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bCloseSwitchSkill;
		public float fDisTS;
		public Int32 nSID;
		public Int32 nTID;
		public Int32 nSX;
		public Int32 nSY;
		public Int32 nTX;
		public Int32 nTY;
	}

    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CC2SGSyncCurSkill
    {
        public Byte type;
        public Int32 nSkillId;
        public UInt32 nxPos;
        public UInt32 nzPos;
        [MarshalAs(UnmanagedType.I1)]
        public Boolean bEnable;
    }

    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CC2SGCancelCurSkill
	{
		public Byte type;
	}


    /// <summary>
    /// 类型头uint8 : ES2CGameProtocol PlayerProperty  S2CGameProtocol.h  /////////////////////
    /// </summary>
    enum ES2CGameProtocol
	{
		eS2CG_MoveBeign = 0,
		eS2CG_ChangePath,
		eS2CG_MoveStep,
		eS2CG_MoveEnd,
		eS2CG_StopMove,
		eS2CG_StopMoveOnChageScene,
		eS2CG_SetSpeed,
		eS2CG_SetPos,
		eS2CG_GetControl,
		eS2CG_ReleaseControl,

		eS2CG_DestroyFollower,
		eS2CG_CreateStaticFollower,
		eS2CG_CreateMoveableFollower,

		//skill
		eS2C_LaunchSkill,
		eS2C_SkillErr,
		eS2C_SyncSkillStep,
		eS2C_SyncSkillStep1_UIPARAM,
		eS2C_SkillSingTickDelay,
		eS2C_Charge,

		//cd
		eS2C_RefreshCD,
		eS2C_AddCDTime,
        eS2C_AddCurCDTime,
        eS2C_AddExtCDTime,
		eS2C_ChangeCDRate,
		eS2C_AddGlobalCDTime,
		eS2C_ChangeGlobalCDRate,

		eS2C_AddSingTime,
		eS2C_ChangeSingRate,

		// 异常状态相关
		eS2CG_CreateMixState,   // 创建
		eS2CG_RemoveMixState,   // 删除
		eS2CG_OverlyingMixState,    // 叠加

		//数据同步相关
		eS2CG_SetMaxName,
		eS2CG_SetMaxHP,
		eS2CG_SetMaxMP,
		eS2CG_SetCurHP,
		eS2CG_SetCurMP,
		eS2CG_SetCurXP,
		eS2CG_AddCurHP,
		eS2CG_AddCurMP,
		eS2CG_AddCurXP,
		eS2CG_SetLevel,
		eS2CG_SetMaxExp,
		eS2CG_SetCurExp,
		eS2CG_SetCharVol,
		//同步宠物数据
		eS2CG_SetPetMaxExp,
		eS2CG_SetPetCurExp,
		eS2CG_SetPetTransState,
		eS2CG_SetPetTempTrans,
		eS2CG_SetPetSavvy,
		eS2CG_SetPetRelShipLevel,
		eS2CG_SetPetGrowType,
		eS2CG_SetPetSkillNum,
		eS2CG_SetPetType,

		//同步玩家数据
		eS2CG_SynPlayerProperty,
		eS2CG_SynPlayerState,//同步玩家状态
		eS2CG_SynFightBasePro,//同步战斗基本数据，第一眼看到的

		eS2CG_SyncPlayerBase,       // 同步玩家基本数据
		eS2CG_SyncPet,              // 同步宠物数据
		eS2CG_SyncMaster,           // 同步主人
		eS2CG_SyncCreateMaster,     // 初始创建同步主人

		//同步战斗数据
		eS2CG_SynFightShanbi,
		eS2CG_SynFightNotMingzhong,
		eS2CG_SynFightDamage,//伤害总值
		eS2CG_SynFightGedang,
		eS2CG_SynFightDikang,
		eS2CG_SynFightDeath,//死亡通知
		eS2CG_SynRelive,//复活
		eS2CG_SynFightXishou,//护盾吸收了伤害

		//向客户端发送信息的消息
		eS2CG_PrintMsg,

		//NPC用消息,
		eS2CG_SynAction,    //同步动作.
		eS2CG_SynNpcData,   //NPC同步
		eS2CG_SynFunNpcData,//功能NPC同步

		//IAOBJ用消息
		eS2CG_SyncIAObjData,    // IAOBJ同步


		//场景互动OBJ
		//eS2CG_SynCreateIAObject,	// 创建

		//show dps
		eS2CG_SynDps,

		//减少吟唱时间
		eS2C_SynSkillSingTime,

		//真子弹同步
		eS2CG_SynRealBTData,

		// 这里协议记得一定放到最后
		eS2CG_ProtocolCount
	};

	//二级协议更新玩家的数据，不频繁同步的属性
	enum PlayerProperty
	{
		ePro_Liliang = 0,
		ePro_Neili,
		ePro_Tizhi,
		ePro_Huigen,
		ePro_Lingxing,

		ePro_WuliGJ,
		ePro_WuliFY,
		ePro_FashuGJ,
		ePro_FashuFY,
		ePro_Fuzhu,
		ePro_MingzhongZ,
		ePro_ShanbiZ,
		ePro_GedangZ,
		ePro_HuixinZ,
		ePro_HuixinFYZ,

		ePro_YSGJ_Du,
		ePro_YSGJ_Bing,
		ePro_YSGJ_Huo,
		ePro_YSKX_Du,
		ePro_YSKX_Bing,
		ePro_YSKX_Huo,

		ePro_MingzhongL,
		ePro_ShanbiL,
		ePro_GedangL,
		ePro_HuixinL,
		ePro_HuixinFYL,//25

		ePro_PetMaxHP,//26
	};

	//////////////////////////////////////////////////////////////////////////
	// 下面，协议开始
	////////////////////////////////////////////////////////////////////////// 
	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGDestroyFollower
	{
		public UInt32 uObjId;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGCreateStaticFollower
	{
		public UInt32 uObjId;
		public UInt64 uSceneId;
		public UInt32 uPixelPosX;
		public UInt32 uPixelPosY;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGCreateMoveableFollower
	{
		public UInt32 uObjId;
		public UInt64 uSceneId;
		public UInt32 uPixelPosX;
		public UInt32 uPixelPosY;
		public UInt32 uMoveBeginPixelX;
		public UInt32 uMoveBeginPixelY;
		public UInt32 uMoveEndPixelX;
		public UInt32 uMoveEndPixelY;
		public UInt16 uSpeed;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bIgnoreBlock;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bIgnoreFlyBlock;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bForceBresenham;
		public Byte uGap;
		public Byte uPathFindType;
		public Double dMoveRate;
		public Double dPathLen;
	}

	/// 对象系统协议
	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGMoveBeign
	{
		public UInt32 uGlobalId;
		public UInt32 uMoveBeginPixelX;
		public UInt32 uMoveBeginPixelY;
		public UInt32 uMoveEndPixelX;
		public UInt32 uMoveEndPixelY;
		public UInt16 uSpeed;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bIgnoreBlock;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bIgnoreFlyBlock;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bForceBresenham;
		public Byte uGap;
		public Byte uPathFindType;
		public Double dPathLen;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGChangePath
	{
		public UInt32 uGlobalId;
		public UInt32 uPixelBeginX;
		public UInt32 uPixelBeginY;
		public UInt32 uPixelEndX;
		public UInt32 uPixelEndY;
		public UInt16 uSpeed;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bIgnoreBlock;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bIgnoreFlyBlock;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bForceBresenham;
		public Byte uGap;
		public Byte uPathFindType;
		public Double dPathLen;
		public Double dChangePathRate;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGMoveStep
	{
		public UInt32 uGlobalId;
		public Double fPathLen;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGMoveEnd
	{
		public UInt32 uGlobalId;
	}


	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGStopMove
	{
		public UInt32 uGlobalId;
		public UInt32 uPixelEndX;
		public UInt32 uPixelEndY;
		public Double dStopMoveRate;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGStopMoveOnChangeScene
	{
		public UInt32 uGlobalId;
		public UInt32 uPixelEndX;
		public UInt32 uPixelEndY;
	}


	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSetSpeed
	{
		public UInt32 uGlobalId;
		public UInt16 uSpeed;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSetPos
	{
		public UInt32 uGlobalId;
		public UInt32 uPixelPosX;
		public UInt32 uPixelPosY;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGGetControl
	{
		public UInt32 uGlobalId;
		public UInt32 uPixelPosX;
		public UInt32 uPixelPosY;
	}


	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGReleaseControl
	{
		public UInt32 uGlobalId;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGLaunchSkill
	{
		public UInt32 uSId;
		public UInt32 uTId;
		public UInt32 uSkillId;
		public UInt16 uCurGenID;
		public Byte uLaunchCharacterType;
		public Int32 nSingTime;
	}


	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSkillErr
	{
		public UInt32 uSId;
		public UInt32 uTId;
		public UInt32 uSkillId;
		public UInt16 uCurGenID;
		public Int32 err;
		public Byte nStepState;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bBreak;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bServerSkill;
		public Byte nErrInfoIndex;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSyncSkillStep
	{
		public UInt32 uGlobalId;
		public UInt32 uTargetId;
		public UInt32 uSkillId;
		public UInt16 uCurGenID;
		public Byte nStepState;
		public Int32 nSingTimes;    //吟唱时间为间隔时间的倍数，只对吟唱阶段有效果，对该值修改，第一个阶段不能为吟唱
		public Int32 nBulletTime;   //子弹飞行时间，服务器算的
		public UInt16 uPosX;
		public UInt16 uPoxY;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bCloseSwitchSkill;   //关闭开关技能
        public UInt16 uAtkSpeed;    //攻击速度
    }


	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSyncSkillStep_UIPARAM
	{
		public UInt32 uGlobalId;
		public UInt32 uTargetId;
		public UInt32 uSkillId;
		public UInt16 uCurGenID;
		public Byte nStepState;
		public Int32 nSingTimes;    //吟唱时间为间隔时间的倍数，只对吟唱阶段有效果，对该值修改，第一个阶段不能为吟唱
		public Int32 nBulletTime;   //子弹飞行时间，服务器算的
		public UInt16 uPosX;
		public UInt16 uPoxY;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bCloseSwitchSkill;   //关闭开关技能
        public UInt16 uAtkSpeed;    //攻击速度

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		public Byte[] sUIParam;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSkillSingTickDelay
	{
		public UInt32 uGlobalId;
		public UInt32 uSkillId;
		public UInt16 uCurGenID;
		public Int32 nCurTickTimes;
		public Int32 nTotalTickTimes;
	}


	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGCharge
	{
		public UInt32 uGlobalId;
		public UInt32 uGridX;
		public UInt32 uGridY;
		public Int32 uChargeTime;
	}

	//////////////////////// cd
	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGRefreshCD
	{
		public UInt32 uGlobalId;
		public UInt32 uCDId;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGAddExtCDTime
	{
		public UInt32 uGlobalId;
		public Int32 uExtCDTime;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGAddCDTime
	{
		public UInt32 uGlobalId;
		public UInt32 uCDID;
		public Int32 nAddCDTime;
	}

    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CS2CGCurAddCDTime
    {
        public UInt32 uGlobalId;
        public UInt32 uCDID;
        public Int32 nAddCDTime;
    }

    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGChangeCDRate
	{
		public UInt32 uGlobalId;
		public UInt32 uCDID;
		public float fChangeCDRate;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGAddGlobalCDTime
	{
		public UInt32 uGlobalId;
		public Int32 nAddGCDTime;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGChangeGlobalCDRate
	{
		public UInt32 uGlobalId;
		public float fChangeGCDRate;
	}


	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGAddSingTime
	{
		public UInt32 uGlobalId;
		public Int32 nAddSingTime;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGChangeSingRate
	{
		public UInt32 uGlobalId;
		public float fChangeSingRate;
	}

	// 异常状态相关协议数据结构
	// 创建
	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGCreateMixState
	{
		public UInt32 uGlobalId;
		public UInt32 uMSID;
		public UInt32 uMSDataID;    // 在数据表中的索引
		public Int32 nDuration; // 持续时间
		public Int32 nRemaining;    // 剩余时间
		public Byte nOverly;
		public UInt32 uSrcGlobalId;
	}

	// 删除
	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGRemoveMixState
	{
		public UInt32 uGlobalId;
		public UInt32 uMSID;
	}


	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGOverlingMixState
	{
		public UInt32 uGlobalId;
		public UInt32 uMSID;
		public Int32 nDuration; // 持续时间
		public Int32 nRemaining;    // 剩余时间
		public UInt32 nOvery;
	}

	// 打印消息
	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGPrintMsg
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public Byte[] strPrintMsg;
	}



	//数据同步相关
	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSetName
	{
		public UInt32 uGlobalId;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		public Byte[] sName;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSetMaxHP
	{
		public UInt32 uGlobalId;
		public UInt32 uValue;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSetMaxMP
	{
		public UInt32 uGlobalId;
		public UInt32 uValue;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSetCurHP
	{
		public UInt32 uGlobalId;
		public UInt32 uValue;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSetCurMP
	{
		public UInt32 uGlobalId;
		public UInt32 uValue;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSetCurXP
	{
		public UInt32 uGlobalId;
		public UInt32 uValue;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGAddCurHP
	{
		public UInt32 uGlobalId;
		public UInt32 uGlobalIdSrc;
		public UInt32 uAssGlobalId;
		public Int32 uValue;
		public Byte uDisplayType;
		public Int32 iSkillId;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGAddCurMP
	{
		public UInt32 uGlobalId;
		public Int32 uValue;

		[MarshalAs(UnmanagedType.I1)]
		public Boolean bDisplay;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGAddCurXP
	{
		public UInt32 uGlobalId;
		public Int32 uValue;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSetLevel
	{
		public UInt32 uGlobalId;
		public UInt16 uLevel;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSetMaxExp
	{
		public UInt32 uValue;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSetCurExp
	{
		public UInt32 uValue;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSetCharVol
	{
		public UInt32 uGlobalId;
		public Double dCharVol;
	}

	//同步宠物数据
	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSetPetMaxExp
	{
		public UInt32 uGlobalId;
		public UInt32 uValue;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSetPetCurExp
	{
		public UInt32 uGlobalId;
		public Byte uCoteId;
		public UInt32 uValue;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSetPetTransState
	{
		public UInt32 uGlobalId;
		public Byte uTrans;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSetPetType
	{
		public UInt32 uGlobalId;
		public Byte uPetType;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSetPetGrowType
	{
		public UInt32 uGlobalId;
		public Byte uGrowType;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSetPetSkillNum
	{
		public UInt32 uGlobalId;
		public Byte uSkillNum;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSetPetSavvy
	{
		public UInt32 uGlobalId;
		public byte uSavvy;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSetPetRelShipLevel
	{
		public UInt32 uGlobalId;
		public Byte uRelShipLevel;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSetPetTempTrans
	{
		public UInt32 uGlobalId;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bTempTrans;
	}


	//同步宠物基本数据，第一眼看到的
	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSyncPet
	{
		public UInt32 uGlobalId;
		public UInt32 uMasterId;
		public UInt16 uPetId;
		public Byte uCampType;
		public Byte uNameColorType;
		public Byte uTrans;     //永久变身
		public Byte uSavvy;      //悟性等级
		public Byte uSkillNum;   //技能数
		public Byte uGrowType;  //成长率品质
		public Byte uRelShipLevel;//亲密度等级
		public Byte uPetType;    //宠物类型

		[MarshalAs(UnmanagedType.I1)]
		public Boolean bTempTrans;  //临时变身
									//double dCharVol;	//CRoffTODO ZYH:用uint8

		/*
		static CS2CGSyncPet()
		{
			uGlobalId = 0;
			uMasterId = 0;
			uPetId = 0;
			uCampType = 0;
			uNameColorType = 0;
			uTrans = 0;
			uSavvy = 0;
			uSkillNum = 0;
			uGrowType = 0;
			uRelShipLevel = 0;
			bTempTrans = false;
			uPetType = 0;
			//dCharVol = 1.0;
		}*/
	}

	//同步玩家数据
	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSynPlayerProperty
	{
		public Byte uCharType;
		public Byte uPetCoteId;
        public UInt32 uGlobalId;
        public Byte uType;
		public Double dValue;
	}

	//同步玩家状态
	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSynPlayerState
	{
		public UInt32 uGlobalId;
		public UInt16 uType;

		[MarshalAs(UnmanagedType.I1)]
		public Boolean bState;
	}


	//同步基本战斗数据，第一眼看到的
	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSynFightBasePro
	{
		public UInt32 uGlobalId;
		public UInt32 uMaxHP;
		public UInt32 uMaxMP;
		public UInt32 uCurHP;
		public UInt32 uCurMP;
		public UInt32 uCurXP;
		public Double dCharVol;
		public UInt16 uLevel;
		public Byte uPartPai;
		public Byte uSex;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bDead;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bShayiKeyin;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bFightState;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bYinshen;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bXiaoshi;
		public Byte uDataLength;

		/*
		static CS2CGSynFightBasePro()
		{
			uGlobalId = 0;
			uMaxHP = 0;
			uMaxMP = 0;
			uCurHP = 0;
			uCurMP = 0;
			uCurXP = 0;
			dCharVol = 1.0;
			uLevel = 0;
			uPartPai = 0;
			uSex = 0;
			bDead = false;
			bShayiKeyin = false;
			bFightState = false;
			bYinshen = false;
			bXiaoshi = false;
			uDataLength = 0;
		}*/
		public UInt32 GetPayloadSizeDerived()
		{
			return uDataLength;
		}
		/*如有需要再添加*/
	}


	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct EquipItem
	{
		public Byte uBigId;                 // 装备大类ID
		public Byte uLevel;                 // 强化登记
		public UInt32 uSmallId;             // 小类ID

		public void EquipItemInit()
		{
			uBigId = 0;
			uLevel = 0;
			uSmallId = 0;
		}
	};

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSyncPlayerBase
	{
		public UInt32 uGlobalId;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
		public Byte[] szCharId;

		public Byte uCamp;                                      // 弘毅。天道，新手阵营
		public Byte uCampNobility;                              //阵营爵位
		public Byte uTongPos;                                       //帮会职务
		public Byte uRide;
		public Byte uPkMode;
		public Byte uNameColorType;
		public EquipItem eiWeapon;
		public EquipItem eiHead;
		public EquipItem eiShouder;
		public EquipItem eiCloth;
		public EquipItem eiCuff;
		public EquipItem eiShoe;
		public EquipItem eiFashionHead;
		public EquipItem eiFashionCloth;
		public EquipItem eiFashionBack;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bEnableFashion;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bHideHeadItem;
		public Byte uHair;
		public Byte uHairCol;
		public Byte uFace;
		public UInt32 uTitleId;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bVIP;
		public Byte uVIPLevel;
		public Byte uDataLength;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bTattooed;
		public Byte uCarryId;
		public Byte uPlayerType;
		public Byte uTattooLength;
        public UInt32 uChapterId;
        public UInt32 uSectionId;
        public UInt32 uStepId;
		/*
		static CS2CGSyncPlayerBase()
		{
			uCamp = 0;
			uCampNobility = 0;
			uTongPos = 0;
			uRide = 0;
			uPkMode = 0;
			uNameColorType = 0;
			uHair = 0;
			uHairCol = 0;
			uFace = 0;
			uTitleId = 0;
			bVIP = false;
			uVIPLevel = 1;
			uDataLength = 0;
			bTattooed = false;
			uCarryId = 0;
			uTattooLength = 0;
		}*/

		public UInt32 GetPayloadSizeDerived()
		{
			return (UInt32)(uDataLength + uTattooLength);
		}
	}


	//同步主人
	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSyncMaster
	{
		public UInt32 uGlobalId;
		public UInt32 uMasterId;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSyncCreateMaster
	{
		public UInt32 uGlobalId;
		public UInt32 uMasterId;
	}

	//同步战斗数据
	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSynFightShanbi
	{
		public UInt32 uGlobalIdSrc;
		public UInt32 uGlobalIdDes;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSynFightNotMingzhong
	{
		public UInt32 uGlobalIdSrc;
		public UInt32 uGlobalIdDes;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSynFightDamage
	{
		public UInt32 uGlobalIdSrc;
		public UInt32 uGlobalIdDes;
		public UInt32 uDamage;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bHuixin;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSynFightGedang
	{
		public UInt32 uGlobalIdSrc;
		public UInt32 uGlobalIdDes;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSynFightDikang
	{
		public UInt32 uGlobalIdSrc;
		public UInt32 uGlobalIdDes;
	}

	//战斗对象死亡
	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSynFightDeath
	{
		public UInt32 uGlobalId;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		public Byte[] sName;

		public Byte uType;
	}


	//复活
	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSynRelive
	{
		public UInt32 uGlobalId;
		public UInt32 uGlobalIdSrc;//救人者
		public UInt32 uReliveType;
	}


	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSynFightXishou
	{
		public UInt32 uGlobalIdSrc;
		public UInt32 uGlobalIdDes;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSynAction
	{
		public UInt32 uGlobalId;
		public Int32 nActionId;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSynNpcData
	{
		public UInt32 uGlobalId;
		public UInt32 uId;
		public UInt32 uValidTeamID;
		public Byte uDir;
		public Byte uPower;
		public Byte uGroupType;
		public Byte uDropLvType;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bFightWithMe;
		public Byte uValidPlayerCharIDLength;
		public Byte uAddNameLength;
        [MarshalAs(UnmanagedType.I1)]
        public Boolean bNewCreate;

		public Byte uRandomAiLength;

		/*
		static CS2CGSynNpcData()
		{
			uGlobalId = 0;
			uId = 0;	
			uValidTeamID = 0;
			uDir = 0;
			uPower = 0;
			bFightWithMe = false;
			uValidPlayerCharIDLength = 0;
			uAddNameLength = 0;		
		}*/

		public UInt32 GetPayloadSizeDerived()
		{
			return (UInt32)(uValidPlayerCharIDLength + uAddNameLength + uRandomAiLength);
		}
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSynFunNpcData
	{
		public UInt32 uGlobalId;
		public UInt32 uId;
		public Byte uDir;
		/*
		static CS2CGSynFunNpcData()
		{
			uGlobalId = 0;
			uId = 0;
			uDir = 0;
		}*/
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSynIAObjData
	{
		public UInt32 uGlobalId;
		public UInt32 uDataIdx;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bEnable;
		public UInt32 uSceneID;
		public UInt32 uGridX;
		public UInt32 uGridY;
		[MarshalAs(UnmanagedType.I1)]
		public Boolean bBornState;
		public Byte uDir;
		public Byte uCamp;
		public Byte uNameColor;
		public UInt32 uSpecialType;
		public UInt32 uSceneConfigId;
		/*
		static CS2CGSynIAObjData()
		{
			uGlobalId = 0;
			uDataIdx = 0;
			bEnable = false;
			uSceneID = 0;
			uGridX = 0;
			uGridY = 0;
			bBornState = true;
			uDir = 0;
			uCamp = 0;
			uNameColor = 0;
		}*/
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSynDps
	{
		public Int32 m_nDpsStartTime;
		public Int32 m_nTotalHurtVal;
		public Int32 m_nSkillNum;
		public Int32 m_nTotalSkillHurtVal;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSynSkillSingTime
	{
		public Int32 iSkillId;
		public Byte uEffectType;
		public Double dValue;
	}

	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct CS2CGSynRealBTData
	{
		public Int32 uGlobalId;
		public Byte uAcType;
		public Int32 uBulletId;
		public Int32 uMasterId;
		public Int32 uTargetId;
		public Int64 uSceneId;
		public Int32 uSpeed;
		public Int32 uBegPosX;
		public Int32 uBegPosY;
		public Int32 uCurPosX;
		public Int32 uCurPosY;
		public Int32 uEndPosX;
		public Int32 uEndPosY;
		public Byte uPopTimes;
		public Int32 uUserVal;
	}
}

