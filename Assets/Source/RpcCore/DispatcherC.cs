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
/// C++协议处理
/// </summary>
namespace WCG
{
	public class DispatcherC<T>
    {
		public Type[] HandlerRcvType  = null; 
		public Type[] HandlerSendType  = null;
		private UInt32 TYPE_SIZE;

		public DispatcherC() {
			TYPE_SIZE = (UInt32)Marshal.SizeOf(default(T).GetType());
			if (HandlerRcvType == null) {
				HandlerRcvType = new Type[] {

					typeof(CS2CGMoveBeign),
					typeof(CS2CGChangePath),
					typeof(CS2CGMoveStep),
					typeof(CS2CGMoveEnd),
					typeof(CS2CGStopMove),
					typeof(CS2CGStopMoveOnChangeScene),
					typeof(CS2CGSetSpeed),
					typeof(CS2CGSetPos),
					typeof(CS2CGGetControl),
					typeof(CS2CGReleaseControl),

					typeof(CS2CGDestroyFollower),
					typeof(CS2CGCreateStaticFollower),
					typeof(CS2CGCreateMoveableFollower),

					//skill
					typeof(CS2CGLaunchSkill),
					typeof(CS2CGSkillErr),
					typeof(CS2CGSyncSkillStep),
					typeof(CS2CGSyncSkillStep_UIPARAM),
					typeof(CS2CGSkillSingTickDelay),
					typeof(CS2CGCharge),

					//cd
					typeof(CS2CGRefreshCD),
					typeof(CS2CGAddCDTime),
                    typeof(CS2CGCurAddCDTime),
					typeof(CS2CGAddExtCDTime),
					typeof(CS2CGChangeCDRate),
					typeof(CS2CGAddGlobalCDTime),
					typeof(CS2CGChangeGlobalCDRate),

					typeof(CS2CGAddSingTime),
					typeof(CS2CGChangeSingRate),

					// 异常状态相关
					typeof(CS2CGCreateMixState),	// 创建
					typeof(CS2CGRemoveMixState),	// 删除
					typeof(CS2CGOverlingMixState),// 叠加

					//数据同步相关
					typeof(CS2CGSetName),
					typeof(CS2CGSetMaxHP),
					typeof(CS2CGSetMaxMP),
					typeof(CS2CGSetCurHP),
					typeof(CS2CGSetCurMP),
					typeof(CS2CGSetCurXP),
					typeof(CS2CGAddCurHP),
					typeof(CS2CGAddCurMP),
					typeof(CS2CGAddCurXP),
					typeof(CS2CGSetLevel),
					typeof(CS2CGSetMaxExp),
					typeof(CS2CGSetCurExp),
					typeof(CS2CGSetCharVol),

					//同步宠物数据
					typeof(CS2CGSetPetMaxExp),
					typeof(CS2CGSetPetCurExp),
					typeof(CS2CGSetPetTransState),
					typeof(CS2CGSetPetTempTrans),
					typeof(CS2CGSetPetSavvy),
					typeof(CS2CGSetPetRelShipLevel),
					typeof(CS2CGSetPetGrowType),
					typeof(CS2CGSetPetSkillNum),
					typeof(CS2CGSetPetType),

					//同步玩家数据
					typeof(CS2CGSynPlayerProperty),
					typeof(CS2CGSynPlayerState),//同步玩家状态
					typeof(CS2CGSynFightBasePro),//同步战斗基本数据，第一眼看到的

					typeof(CS2CGSyncPlayerBase),		// 同步玩家基本数据
					typeof(CS2CGSyncPet),				// 同步宠物数据
					typeof(CS2CGSyncMaster),			// 同步主人
					typeof(CS2CGSyncCreateMaster),		// 初始创建同步主人

					//同步战斗数据
					typeof(CS2CGSynFightShanbi),
					typeof(CS2CGSynFightNotMingzhong),
					typeof(CS2CGSynFightDamage),//伤害总值
					typeof(CS2CGSynFightGedang),
					typeof(CS2CGSynFightDikang),
					typeof(CS2CGSynFightDeath),//死亡通知
					typeof(CS2CGSynRelive),//复活
					typeof(CS2CGSynFightXishou),//护盾吸收了伤害

					//向客户端发送信息的消息
					typeof(CS2CGPrintMsg),

					//NPC用消息),
					typeof(CS2CGSynAction),	//同步动作.
					typeof(CS2CGSynNpcData),	//NPC同步
					typeof(CS2CGSynFunNpcData),//功能NPC同步

					//IAOBJ用消息
					typeof(CS2CGSynIAObjData),	// IAOBJ同步

					//场景互动OBJ
					//typeof(CS2CGSynCreateIAObject),	// 创建

					//show dps
					typeof(CS2CGSynDps),

					//减少吟唱时间
					typeof(CS2CGSynSkillSingTime),

					//真子弹同步
					typeof(CS2CGSynRealBTData)
				};
			}

			if (HandlerSendType == null) {
				HandlerSendType = new Type[] {
					typeof(CC2SGLuaModify),
					typeof(CC2SGMoveBeign),
					typeof(CC2SGChangePath),
					typeof(CC2SGMoveStep),
					typeof(CC2SGMoveEnd),
					typeof(CC2SGStopMove),
					typeof(CC2SGFollowerCreatePathFailed),
					typeof(CC2SGLaunchSkill),
                    typeof(CC2SGSyncCurSkill),
                    typeof(CC2SGCancelCurSkill),
				};
			}
		}


		// 然后再处理某个链接的数据的时候就能支持处理了某个协议后
		// 停止继续解析协议
		public int LoopDispatch(ref byte[] pData, uint uSize)
		{
			uint uTotalSize = uSize;
			uint uProcessedOnce=0;
			uint uProcessedSize = 0;

			for(;;)
			{
				bool bRes = OnceDispatch(ref pData, uTotalSize, uProcessedSize, ref uProcessedOnce);
				if(bRes)
				{
					uTotalSize -= uProcessedOnce;
					uProcessedSize += uProcessedOnce;
					uProcessedOnce = 0;
					if (uProcessedSize >= uSize) {
						break;
					}
				}
				else
				{
					break;
				}
			}
			return (int)uProcessedSize;
		}


		public bool OnceDispatch(ref byte[] pData, uint uSize, uint uProcessedSize, ref uint uProcessedOnce)
		{
			int type = 0;
			int msgSize = 0;
            Type callBackType = null;
            try
			{
				if (uSize < TYPE_SIZE)
					return false;

				if (TYPE_SIZE == sizeof(byte))
				{
					type = pData[uProcessedSize];
				}
				else
				{
					byte[] pPos = new byte[TYPE_SIZE];
					Array.Copy(pData, uProcessedSize, pPos, 0, TYPE_SIZE);
					type = BitConverter.ToInt32(pPos, 0);
				}

				callBackType = HandlerRcvType[type];
				msgSize = Marshal.SizeOf(callBackType);

				if (uSize < msgSize)
					return false;

				uProcessedOnce += TYPE_SIZE;
			}
			catch (System.Exception ex)
			{
				Debug.LogErrorFormat("OnceDispatch_Error uSize:{0},uProcessedSize:{1},uProcessedOnce:{2},type:{3},msgSize:{4},TYPE_SIZE:{5},RpcLen:{6},ex:{7}", uSize,uProcessedSize,uProcessedOnce,type,msgSize,TYPE_SIZE, HandlerRcvType.Length, ex.ToString());
			}

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            string sMark = string.Format("CallLuaCsRpc {0}", callBackType != null ? callBackType.Name : type.ToString());
            Profile.BenchmarkBegin(sMark);
#endif
            bool bOk = CsRpcWrap.Call_Bytes2Handler_To_Lua(type, ref pData, (uint)(uProcessedSize + uProcessedOnce), ref uProcessedOnce);

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            Debug.AssertFormat(bOk, "CsRpcWrap.Call_Bytes2Handler_To_Lua Error type:{0}:", type);
            Profile.BenchmarkEnd(sMark);
#endif

            return true;
		}
			
		public void LuaCallSendApi(int iFuncId, object[] args)
		{
			try {
				if (iFuncId > HandlerSendType.Length) {
					Debug.LogError("LuaCallSendApi iFuncId err:"+iFuncId + " " + HandlerSendType.Length);
					return;
				}

				Type callBackType = HandlerSendType[iFuncId];

				if (callBackType.GetFields().Length != args.Length) {
					Debug.LogError("LuaCallSendApi Length err:"+iFuncId +","+callBackType.GetFields().Length + "," + args.Length);
					return;
				}

				//WQ_TODO
				CsRpcWrap.To_Bytes_Handler pByHandler = CsRpcWrap.to_bytes_pHander[iFuncId];
				Debug.Assert(null != pByHandler, "CsRpcWrap.To_Bytes_Handler Err:" + iFuncId);
				Byte[] data = pByHandler(ref args, 0);

				RpcSendHander.SendLogicCPrt(ref data, data.Length);
			} catch (Exception e) {
				Debug.LogError("LuaCallSendApi Exception:iFuncId" + iFuncId+ ",args:" + args.ToString() + ",\n"+ e.ToString());
			}
		}
	}
}

