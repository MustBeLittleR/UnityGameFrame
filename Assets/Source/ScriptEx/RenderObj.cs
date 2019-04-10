using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Events;
using LuaInterface;

namespace WCG
{
    public class RenderObj : MonoBehaviour
    {
        private string m_sName = "";
        private string m_sDummyNa = "";
        private Transform m_oCharTransform = null;
        private Animation m_oCharAnimation = null;
        private Vector3 m_nSelfScale = Vector3.one;
        private float m_nZoomRate = 0.0f;
        private LuaFunction m_OnAniEventCal = null;

        private Dictionary<string, Transform> m_tDummy = new Dictionary<string, Transform>();
        private Dictionary<int, EffectObj> m_tEffectObj = new Dictionary<int, EffectObj>();
        private List<int> m_gcList = new List<int>();

        //保存做跟随模型的特殊效果展示
        private Dictionary<int, SkinnedMeshRenderer> ms_dicEquipRenderer = new Dictionary<int, SkinnedMeshRenderer>();
		private Dictionary<int, Material> ms_dicEquipMaterial = new Dictionary<int, Material>();
		public Shader m_FrozenShader = GameMgr.Instance.m_FrozenShader;
		public Texture m_FrozenTexture = GameMgr.Instance.m_FrozenTexture;

		public static float m_iParticleSystemGrade = 1.0f; //0.2低，0.5中，1.0高

		private bool m_bUpdateChar = false;
		private Vector3 m_v3CurPosition = Vector3.zero;
		private Vector3 m_v3TarPosition = Vector3.zero;
		private float m_nMoveSpeed = 0;
		private float m_nRotaSpeed = 0;
		private Quaternion m_quCurRotation = Quaternion.identity;
		private Quaternion m_quTarRotation = Quaternion.identity;
		private MaterialAnimationAssistant m_oMatAniAssis = null;

        private bool m_bDitherAnim = false;         //动画控制标志        
        private bool m_bDitherChange;
        private int m_nDitherStartTime;
        private float m_fDitherTotalTime;             //动画播放总时间
        private float m_fDitherAxisX;                  //抖动在两轴振幅
        private float m_fDitherAxisY;
        private int m_nDitherCount;
        private int m_nDitherLimit;					//限定的次数

        //只有主角玩家自己才有
        private AudioSource m_AudioSource = null;
        AudioClip m_OneShotClip = null;

        public sealed class EffectObj
        {
            public int m_nType;
            public Transform m_oEffct;//牵线特效
            public Transform m_oSTrans;//自身Transform
            public Transform m_oSDummy;//自身dummy点
            public Transform m_oTDummy;//目标dummy点
            public Vector3 m_v3Offset;//位置偏移
            public float m_fLineLen;//特效长度
            public float m_fEffTime; //特效时长
            public float m_fCurTime; //特效时长

            public EffectObj(Transform oEffct, Transform oSTrans, Transform oSDummy, Vector3 v3Offset, float fLineLen)
            {
                m_nType = 1;
                m_oEffct = oEffct;
                m_oSTrans = oSTrans;
                m_oSDummy = oSDummy;
                m_v3Offset = v3Offset;
                m_fLineLen = fLineLen;
            }
            public EffectObj(Transform oEffct, Transform oSTrans, Transform oSDummy, Transform oTDummy, Vector3 v3Offset, float fEffTime)
            {
                m_nType = 2;
                m_oEffct = oEffct;
                m_oSTrans = oSTrans;
                m_oSDummy = oSDummy;
                m_oTDummy = oTDummy;
                m_v3Offset = v3Offset;
                m_fEffTime = fEffTime;
                m_fCurTime = 0;
            }

            public int GetEffInsId()
            {
                return m_oEffct.GetInstanceID();
            }

            public bool UpdateObj()
            {
                if (!m_oEffct || !m_oSTrans)
                    return false;

                if (m_nType == 1)
                {
                    if (m_fLineLen > 0)
                    {
                        RaycastHit hit;
                        float nLineLen = 0;
                        Vector3 v3StartPos = m_oSTrans.position + (m_oSTrans.up.normalized * 0.2f);
                        //Debug.DrawLine(v3StartPos, v3StartPos + transform.forward.normalized* nLineLen, Color.red);
                        if (Physics.Raycast(v3StartPos, m_oSTrans.forward, out hit, nLineLen))
                            if (hit.collider != null)
                                nLineLen = Vector3.Distance(m_oSTrans.position, hit.point);

                        if (nLineLen != m_fLineLen)
                        {
                            m_fLineLen = nLineLen;
                            UIMgr.Instance.SetAllLineRenderer(m_oEffct.gameObject, nLineLen / 10);
                            m_oEffct.position = m_oSTrans.position + m_oSTrans.forward.normalized * nLineLen + m_v3Offset;
                            m_oEffct.LookAt(m_oSDummy);
                        }
                    }
                }
                else if (m_nType == 2)
                {
                    if (!m_oTDummy)
                        return false;

                    if (m_fEffTime > 0)
                    {
                        m_fCurTime += Time.deltaTime;
                        float nRate = Mathf.Min(1, m_fCurTime / m_fEffTime);
                        if (nRate >= 1)
                        {
                            float fLineLen = Vector3.Distance(m_oSDummy.position, m_oTDummy.position);
                            UIMgr.Instance.SetAllLineRenderer(m_oEffct.gameObject, fLineLen / 10);
                            m_oEffct.position = m_oTDummy.position;
                            m_oEffct.LookAt(m_oSDummy);
                        }
                        else
                        {
                            float fLineLen = Vector3.Distance(m_oSDummy.position, m_oTDummy.position) * nRate;
                            UIMgr.Instance.SetAllLineRenderer(m_oEffct.gameObject, fLineLen / 10);
                            m_oEffct.position = m_oSTrans.position + m_oSTrans.forward.normalized * fLineLen + m_v3Offset;
                            m_oEffct.LookAt(m_oSDummy);
                        }
                    }
                }
                return true;
            }
        }

        void Awake()
        {
            m_oCharTransform = gameObject.transform;
            m_sName = gameObject.name;
            m_sDummyNa = Path.GetFileNameWithoutExtension(m_sName).ToLower();
            m_oCharAnimation = gameObject.GetComponent<Animation>();
            m_nSelfScale = transform.localScale;
			GetMatAniAssistant();
		}

        private MaterialAnimationAssistant GetMatAniAssistant()
		{
			if (!m_oMatAniAssis)
			{
				m_oMatAniAssis = gameObject.GetComponent<MaterialAnimationAssistant>();
				if (!m_oMatAniAssis)
				{
					m_oMatAniAssis = gameObject.AddComponent<MaterialAnimationAssistant>();
				}
			}
			return m_oMatAniAssis;
		}

		Transform GetDummy(Transform oTransform, string sDummy)
        {
            Transform oDummy = oTransform.Find(sDummy);
            if (oDummy)
                return oDummy;
            for (int i = 0; i < oTransform.childCount; i++)
            {
                oDummy = GetDummy(oTransform.GetChild(i), sDummy);
                if (oDummy)
                    return oDummy;
            }
            return oDummy;
        }

		private void DebugMsg(string msg)
		{
			Debug.LogWarning("RenderObj->\t" + msg);
		}

		Quaternion GenGameQuaRot(Vector3 tarPosition, Vector3 curPosition)
		{
			Vector3 v3Offset = tarPosition - curPosition;
			v3Offset.z = .0f;
			float nAngle = Vector3.Angle(Vector3.up, v3Offset);
			if (v3Offset.x < .0f) nAngle = 360 - nAngle;
			return GetGraphRotation(nAngle);
		}

        void Update()
        {
            UpdateDither(Time.deltaTime);
            UpdateEffect();
        }

        private void UpdateDither(float fDeltaTime)
        {
            if (m_bDitherAnim && m_fDitherTotalTime > 0)
            {
                //经过的时间                
                int nElapse = Time.renderedFrameCount - m_nDitherStartTime;
                if (fDeltaTime <= m_fDitherTotalTime)
                {
                    if (nElapse >= 4)
                    {
                        //std::cout<< "抖一次" << nElapse << std::endl;
                        float fX = m_oCharTransform.localPosition.x;
                        float fY = m_oCharTransform.localPosition.y;
                        float fZ = m_oCharTransform.localPosition.z;
                        if (m_bDitherChange)
                        {
                            m_oCharTransform.localPosition = new Vector3(fX + m_fDitherAxisX, fY + m_fDitherAxisY, fZ);
                            m_bDitherChange = false;
                        }
                        else
                        {
                            m_oCharTransform.localPosition = new Vector3(fX - m_fDitherAxisX, fY - m_fDitherAxisY, fZ);
                            m_bDitherChange = true;
                        }
                        m_nDitherStartTime = Time.renderedFrameCount;
                        ++m_nDitherCount;
                    }
                    m_fDitherTotalTime = m_fDitherTotalTime - fDeltaTime;
                }
                else
                {
                    m_bDitherAnim = false;
                    m_fDitherTotalTime = -1;

                }

                //来回振幅
                if (m_nDitherLimit == m_nDitherCount)
                {
                    m_bDitherAnim = false;
                    m_nDitherCount = 0;
                }
            }
        }


		public Transform GetDummyFromCache(string sDummy)
		{
			Transform oDummy = null;
			if (m_tDummy.ContainsKey(sDummy))
				oDummy = m_tDummy[sDummy];
			else
			{
				if (m_sDummyNa.Equals(sDummy.ToLower()))
                {
                    oDummy = m_oCharTransform;                
                    m_tDummy[sDummy] = m_oCharTransform;
                }
                else
                {
                    oDummy = GetDummy(m_oCharTransform, sDummy);
                    if (oDummy)
                    {
                        oDummy.localScale = Vector3.one;
                        m_tDummy[sDummy] = oDummy;
                    }
                }
			}
			return oDummy;
		}

        public void SetScale(float nScale)
        {
            m_oCharTransform.localScale = new Vector3(nScale, nScale, nScale) ;
        }

        public void SetModelZoomRate(float nZoomRate)
        {
            m_nZoomRate = nZoomRate;
            m_oCharTransform.localScale = m_nSelfScale * m_nZoomRate;    
        }

		public void SetVisible(bool bVisible)
		{
			gameObject.SetActive(bVisible);
		}

        private Quaternion GetGraphRotation(float fLogicDir)
        {
            Quaternion qua = Quaternion.AngleAxis(fLogicDir, SceneMgr.Instance.GetRootRotAxis());
            return qua * SceneMgr.Instance.GetRootNodeRot();
        }

        public static bool DealFixedRotation(GameObject oEffect)
        {
            FixedRotationObject oRotationObj = oEffect.GetComponent<FixedRotationObject>();
            if (oRotationObj)
            {
                oRotationObj.ResetTransformState();
                return true;
            }
            return false;
        }

        #region 音效接口
        public void Set3DSoundVol(float fvolume)
        {
            if (m_AudioSource)
                m_AudioSource.volume = fvolume;
        }

        public void Stop3DSound()
        {
            if (m_AudioSource && m_AudioSource.isPlaying)
                m_AudioSource.Stop();
        }

        AudioSource GetAudioSource()
        {
            if (null == m_AudioSource)
            {
                m_AudioSource = gameObject.AddComponent<AudioSource>();
                m_AudioSource.rolloffMode = AudioRolloffMode.Linear;
                m_AudioSource.spatialBlend = 1;
                m_AudioSource.minDistance = 3;
                m_AudioSource.maxDistance = 12;
            }
            return m_AudioSource;
        }

        public float Play3DSound(string sSoundName, bool bLoop)
        {
            float nSoundLength = 0.0f;
            if (MediaMgr.Instance.bSoundMute)
                return nSoundLength;

            if (bLoop)
            {
                string pathName = "media/sound/" + sSoundName;
                if (null != m_AudioSource && null != m_AudioSource.clip)
                {
                    if (pathName.Equals(m_AudioSource.clip.name))
                    {
                        if (!m_AudioSource.isPlaying)
                        {
                            m_AudioSource.volume = MediaMgr.Instance.GetSoundVolume();
                            m_AudioSource.Play();
                        }
                        return m_AudioSource.clip.length;
                    }
                }
                UnityAction<UnityEngine.Object> ac = (obj) =>
                {
                    if (null != obj)
                    {
                        AudioClip clip = (AudioClip)obj;
                        if (null != clip && transform)
                        {
                            AudioSource audio = GetAudioSource();
                            if (audio.clip)
                            {
                                ResourceMgr.UnLoadAsset(audio.clip);
                                audio.clip = null;
                                audio.clip = clip;
                            }
                            clip.name = pathName;
                            audio.clip = clip;
                            audio.loop = bLoop;
                            audio.volume = MediaMgr.Instance.GetSoundVolume();
                            audio.Play();
                        }
                    }
                };

                RunMode eRunMode = GameMgr.Instance.m_eRunMode;
                if (eRunMode == RunMode.eDeveloper)
                    ac(MediaMgr.Instance.GetClip(pathName));
                else if (eRunMode == RunMode.eDebug || eRunMode == RunMode.eRelease)
                    MediaMgr.Instance.GetClipAsync(pathName, ac);
            }
            else
            {
                string pathName = "media/sound/" + sSoundName;
                if (null != m_OneShotClip)
                {
                    if (pathName.Equals(m_OneShotClip.name))
                    {
                        AudioSource audio = GetAudioSource();
                        audio.PlayOneShot(m_OneShotClip);
                        return m_OneShotClip.length;
                    }
                }
                UnityAction<UnityEngine.Object> ac = (obj) =>
                {
                    if (null != obj)
                    {
                        AudioClip clip = (AudioClip)obj;
                        if (null != clip && transform)
                        {
                            AudioSource audio = GetAudioSource();
                            if (m_OneShotClip)
                            {
                                ResourceMgr.UnLoadAsset(m_OneShotClip);
                                m_OneShotClip = null;
                                m_OneShotClip = clip;
                            }
                            clip.name = pathName;
                            audio.PlayOneShot(clip, MediaMgr.Instance.GetSoundVolume());
                            nSoundLength = clip.length;
                        }
                    }
                };

                RunMode eRunMode = GameMgr.Instance.m_eRunMode;
                if (eRunMode == RunMode.eDeveloper)
                    ac(MediaMgr.Instance.GetClip(pathName));
                else if (eRunMode == RunMode.eDebug || eRunMode == RunMode.eRelease)
                    MediaMgr.Instance.GetClipAsync(pathName, ac);
            }

            return nSoundLength;
        }
        #endregion 音效接口

        #region 效果接口
        public void MixHide(bool bHide)
		{
			CharacterTransparentEffect oTranEff = gameObject.GetComponent<CharacterTransparentEffect>();
			if (!oTranEff)
				oTranEff = gameObject.AddComponent<CharacterTransparentEffect>();

			if (bHide)
			{
				float nAlpha = 0.3f;
				oTranEff.setTintColor(new Color(1, 1, 1, nAlpha), nAlpha);
			}
			else
			{
				oTranEff.RevertEffect();
			}
		}

		public void ResetShader(bool bCloseOutLight)
		{
			MaterialAnimationAssistant oMatAniAssis = GetMatAniAssistant();
			if (oMatAniAssis)
				oMatAniAssis.resetShader(bCloseOutLight);
		}

		public void SetPetrify(bool bEnable)
		{
			MaterialAnimationAssistant oMatAniAssis = GetMatAniAssistant();
			if (oMatAniAssis)
			{
				if (bEnable)
					oMatAniAssis.playFossilisedEffect();
				else
					oMatAniAssis.endFossilisedEffect();
			}
		}

		public void SetFrozen(bool bEnable)
		{
			MaterialAnimationAssistant oMatAniAssis = GetMatAniAssistant();
			if (oMatAniAssis)
			{
				if (bEnable)
					oMatAniAssis.playFrozenEffect();
				else
					oMatAniAssis.endFrozenEffect();
			}
		}

		public void SetEquipFrozen(bool bEnable)
		{
			List<int> listEquip = new List<int>(ms_dicEquipRenderer.Keys);
			for (int i = 0; i < listEquip.Count; i++)
			{
				int insId = listEquip[i];
				SkinnedMeshRenderer meshRendere = ms_dicEquipRenderer[insId];
				if (meshRendere)
				{
					if (bEnable)
					{
						if (meshRendere.sharedMaterial && !ms_dicEquipMaterial.ContainsKey(insId))
						{
							ms_dicEquipMaterial.Add(insId, meshRendere.sharedMaterial);
						}
						for (int j = 0; j < meshRendere.materials.Length; j++)
						{
							meshRendere.materials[j].shader = m_FrozenShader;
							meshRendere.materials[j].SetTexture("_Reftex", m_FrozenTexture);
						}
					}
					else
					{
						Material oldMaterial = null;
						ms_dicEquipMaterial.TryGetValue(insId, out oldMaterial);
						if (oldMaterial)
						{
							meshRendere.sharedMaterial = oldMaterial;
							ms_dicEquipMaterial.Remove(insId);
						}
					}
				}
			}
		}

		public void HighLight()
		{
			MaterialAnimationAssistant oMatAniAssis = GetMatAniAssistant();
			if (oMatAniAssis)
				oMatAniAssis.playHighlight();
		}

        public void SetDither(float nAxisX, float nAxisY, int nDitherLimit, float time/*=-1*/ )
        {
            if (!m_bDitherAnim)
                m_bDitherAnim = true;

            m_fDitherAxisX = nAxisX;
            m_fDitherAxisY = nAxisY;
            m_fDitherTotalTime = time;

            if (m_bDitherAnim)
                m_nDitherStartTime = Time.renderedFrameCount;
            else
                m_fDitherTotalTime = -1;

            m_nDitherLimit = nDitherLimit * 2;
        }

        public void SetHighLightColor(float r, float g, float b, float a)
		{
			MaterialAnimationAssistant oMatAniAssis = GetMatAniAssistant();
			if (oMatAniAssis)
				oMatAniAssis.HighLightColor = new Color(r, g, b, a);
		}

        public void PlayOutLineEffect(float r, float g, float b, float a, float width)
        {
            MaterialAnimationAssistant oMatAniAssis = GetMatAniAssistant();
            if (oMatAniAssis)
            {
                oMatAniAssis.OutlineColor = new Color(r, g, b, a);
                oMatAniAssis.OutlineWidth = width;
                oMatAniAssis.PlayOutLineEffect();
            }
        }
        public void PlayOutLight(float r, float g, float b, float a)
        {
            MaterialAnimationAssistant oMatAniAssis = GetMatAniAssistant();
            if (oMatAniAssis)
            {               
                oMatAniAssis.MainBlendColor = new Color(r, g, b, 100 / 255f);
                oMatAniAssis.PlayOutLineEffect();
            }
        }

        public void FadeOut()
		{
			MaterialAnimationAssistant oMatAniAssis = GetMatAniAssistant();
			if (oMatAniAssis)
				oMatAniAssis.playFadeout();
		}
        #endregion  效果接口

        #region 动作接口
        public void StopAction()
        {
            if (m_oCharAnimation)
                m_oCharAnimation.Stop();
        }

        public void PlayActionOnce(string sAction, float fSpeed, float fTime, string sActionEx = null)
        {
			PlayQueuedAction(sAction, fSpeed, false, fTime, sActionEx);
        }

        public void PlayActionLoop(string sAction, float fSpeed, float fTime, string sActionEx = null)
        {
			PlayQueuedAction(sAction, fSpeed, true, fTime, sActionEx);
        }

		public bool HasAniAciton(string sAction)
		{
			if (m_oCharAnimation)
				return null != m_oCharAnimation.GetClip(sAction);
			return false;
		}

        private void PlayQueuedAction(string sAction, float nActionStateSpeed, bool bLoop, float fTime, string sActionEx)
        {
            if (string.IsNullOrEmpty(sActionEx))
            {
                if (HasAniAciton(sAction))
                {
                    QueueMode queueMode = QueueMode.PlayNow;
                    AnimationState anistate = m_oCharAnimation.CrossFadeQueued(sAction, 0.2f, queueMode, PlayMode.StopAll);
                    anistate.speed = nActionStateSpeed;
                    /*anistate.AddMixingTransform(GetDummyFromCache("Bip001 Spine").parent.parent.parent, false);
                    anistate.AddMixingTransform(GetDummyFromCache("Bip001 Spine").parent.parent, false);
                    anistate.AddMixingTransform(GetDummyFromCache("Bip001 Spine").parent, false);
                    anistate.AddMixingTransform(GetDummyFromCache("Bip001 Spine"));
                    anistate.AddMixingTransform(GetDummyFromCache("Bip001 L Thigh").parent.parent.parent.parent, false);
                    anistate.AddMixingTransform(GetDummyFromCache("Bip001 L Thigh").parent.parent.parent, false);
                    anistate.AddMixingTransform(GetDummyFromCache("Bip001 L Thigh").parent.parent, false);
                    anistate.AddMixingTransform(GetDummyFromCache("Bip001 L Thigh").parent, false);
                    anistate.AddMixingTransform(GetDummyFromCache("Bip001 L Thigh"));
                    anistate.AddMixingTransform(GetDummyFromCache("Bip001 R Thigh"));
                    anistate.AddMixingTransform(GetDummyFromCache("Bone_B01"));
                    anistate.AddMixingTransform(GetDummyFromCache("Bone_F01"));
                    anistate.AddMixingTransform(GetDummyFromCache("Bone_L01"));
                    anistate.AddMixingTransform(GetDummyFromCache("Bone_R01"));*/
                    anistate.wrapMode = bLoop ? WrapMode.Loop : WrapMode.Once;
                    anistate.time = fTime;
                }
                //else
                //	Debug.LogWarningFormat("PlayQueuedAction Action Not Found:{0}, {1}", gameObject.name, sAction);
            }
            else
            {
                if (HasAniAciton(sAction) && HasAniAciton(sActionEx))
                {
                    AnimationState anistateUp = m_oCharAnimation[sAction];
                    anistateUp.layer = 0;
                    anistateUp.weight = 1;
                    anistateUp.blendMode = AnimationBlendMode.Blend;
                    anistateUp.AddMixingTransform(GetDummyFromCache("Bip001 Spine").parent.parent.parent, false);
                    anistateUp.AddMixingTransform(GetDummyFromCache("Bip001 Spine").parent.parent, false);
                    anistateUp.AddMixingTransform(GetDummyFromCache("Bip001 Spine").parent, false);
                    anistateUp.AddMixingTransform(GetDummyFromCache("Bip001 Spine"));
                    anistateUp.wrapMode = bLoop ? WrapMode.Loop : WrapMode.Once;

                    AnimationState anistateDown = m_oCharAnimation[sActionEx];
                    anistateDown.layer = 1;
                    anistateDown.weight = 1;
                    anistateDown.blendMode = AnimationBlendMode.Blend;
                    anistateDown.AddMixingTransform(GetDummyFromCache("Bip001 L Thigh").parent.parent.parent.parent, false);
                    anistateDown.AddMixingTransform(GetDummyFromCache("Bip001 L Thigh").parent.parent.parent, false);
                    anistateDown.AddMixingTransform(GetDummyFromCache("Bip001 L Thigh").parent.parent, false);
                    anistateDown.AddMixingTransform(GetDummyFromCache("Bip001 L Thigh").parent, false);
                    anistateDown.AddMixingTransform(GetDummyFromCache("Bip001 L Thigh"));
                    anistateDown.AddMixingTransform(GetDummyFromCache("Bip001 R Thigh"));
                    anistateDown.AddMixingTransform(GetDummyFromCache("Bone_B01"));
                    anistateDown.AddMixingTransform(GetDummyFromCache("Bone_F01"));
                    anistateDown.AddMixingTransform(GetDummyFromCache("Bone_L01"));
                    anistateDown.AddMixingTransform(GetDummyFromCache("Bone_R01"));
                    anistateDown.wrapMode = WrapMode.Loop;

                    m_oCharAnimation.Play(sAction, PlayMode.StopAll);
                    m_oCharAnimation.Play(sActionEx);
                    anistateUp.time = fTime;
                }
                //else
                //	Debug.LogWarningFormat("PlayQueuedAction Action Not Found:{0}, {1}, {2}", gameObject.name, sAction, sActionEx);
            }

        }

        public void SetAniEvent(LuaFunction fun)
        {
            m_OnAniEventCal = fun;
        }
        public void OnAniEvent()
        {
            if (null != m_OnAniEventCal)
                m_OnAniEventCal.Call();
        }
        #endregion  动作接口

        #region 装备接口
        public GameObject BindEquip(string sDummy, string sEquip, string sResetDummy)
		{
			Transform oDummy = GetDummyFromCache(sDummy);
			if (!oDummy)
			{
				DebugMsg("BindEquip wrong sDummy:" + m_sName + "," + sDummy);
				return null;
			}
			GameObject goEquipObj = ResourceMgr.LoadAssetEx(sEquip, false);
			if (!goEquipObj)
			{
				DebugMsg("BindEquip wrong sEffect:" + sEquip);
				return null;
			}

			//做表现相关
			SkinnedMeshRenderer[] arrRenderers = goEquipObj.GetComponentsInChildren<SkinnedMeshRenderer>();
			for (int i = 0; i < arrRenderers.Length; i++)
			{
				SkinnedMeshRenderer renderer = arrRenderers[i];
				if (renderer && renderer.gameObject.GetComponent<Renderer>() && !renderer.gameObject.name.Contains("shadow"))
				{
					int insId = goEquipObj.GetInstanceID();
					if (!ms_dicEquipRenderer.ContainsKey(insId))
						ms_dicEquipRenderer.Add(insId, renderer);
					break;
				}
			}

			goEquipObj.SetActive(false);
			Transform oEquipTrans = goEquipObj.transform;
			oEquipTrans.SetParent(oDummy);
			oEquipTrans.localPosition = Vector3.zero;
			oEquipTrans.localRotation = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
			oEquipTrans.localScale = Vector3.one;
			Transform oBoneTrans = goEquipObj.transform.Find(sResetDummy);
			if (oBoneTrans)
			{
				oBoneTrans.localPosition = Vector3.zero;
				oBoneTrans.localRotation = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
				oBoneTrans.localScale = Vector3.one;
			}
			goEquipObj.SetActive(true);
			return goEquipObj;
		}

		public void UnBindEquip(GameObject goEquipObj)
		{
			if (!goEquipObj) return;

			int insId = goEquipObj.GetInstanceID();
			if (ms_dicEquipRenderer.ContainsKey(insId))
			{
				ms_dicEquipRenderer.Remove(insId);
			}
		}
        #endregion 装备接口

        #region 特效接口
        void UpdateEffect()
        {
            if(m_tEffectObj.Count > 0)
            {
                Dictionary<int, EffectObj>.Enumerator iter = m_tEffectObj.GetEnumerator();
                while (iter.MoveNext())
                {
                    int nkey = iter.Current.Value.GetEffInsId();
                    if (!iter.Current.Value.UpdateObj())
                    {
                        m_gcList.Add(nkey);
                    }
                }
                iter.Dispose();
            }
    
            if(m_gcList.Count > 0)
            {
                List<int>.Enumerator gciter = m_gcList.GetEnumerator();
                while (gciter.MoveNext())
                {
                    RemoveEffect(gciter.Current);
                }
                gciter.Dispose();
                m_gcList.Clear();
            }
        }

        public void RemoveEffect(int nInsId)
        {
            if (m_tEffectObj.ContainsKey(nInsId))
                m_tEffectObj.Remove(nInsId);
        }

        public GameObject BindEffect(string sDummy, string sEffect, float nZoomRate, float fDecTime)
        {
            Transform oDummy = GetDummyFromCache(sDummy);
            if (!oDummy)
            {
				DebugMsg("BindEffect wrong sDummy:" + m_sName + "," + sDummy);
                return null;
            }

            GameObject oEffectObj = ResourceMgr.LoadAssetEx(sEffect, false);
            if (!oEffectObj)
            {
				DebugMsg("BindEffect wrong sEffect:" + sEffect);
                return null;
            }
            oEffectObj.SetActive(false);
            Transform oEffectTrans = oEffectObj.transform;
            oEffectTrans.SetParent(oDummy);
            oEffectTrans.localPosition = Vector3.zero;
            oEffectTrans.localRotation = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
            oEffectTrans.localScale = Vector3.one;

	        UIMgr.ApplyParentLayer(oEffectObj);
			DealFixedRotation(oEffectObj);
			oEffectObj.SetActive(true);
            return oEffectObj;
        }

        public GameObject BindEffectByPos(string sEffect, float nPosX, float nPosY, float nPosZ, float fLogicDir, float fDecTime)
        {
            GameObject oEffectObj = ResourceMgr.LoadAssetEx(sEffect, true);
			if (!oEffectObj)
			{
				DebugMsg("BindEffectByPos wrong sEffect:" + sEffect);
				return null;
			}
			oEffectObj.SetActive(false);
			float fGraphPosZ = SceneMgr.Instance.GetGraphicsPosZ(nPosY);
			oEffectObj.transform.position = new Vector3(nPosX, nPosY, fGraphPosZ);
			oEffectObj.transform.rotation = GetGraphRotation(fLogicDir);
			
			UIMgr.SetLayer(oEffectObj, gameObject.layer);
			DealFixedRotation(oEffectObj);
			oEffectObj.SetActive(true);
			return oEffectObj;
        }

        public GameObject BindLineEffect(string sEffect, GameObject oTarget, string sSelfDummy, string sTarDummy, float nEffTime)
        {
			RenderObj oTarRenObj = oTarget.GetComponent<RenderObj>();
            if(!oTarRenObj)
            {
				DebugMsg("Target is NOT a Character:" + oTarget.name);
                return null;
            }

            Transform oTarDummy = oTarRenObj.GetDummyFromCache(sTarDummy);
            if (!oTarDummy)
            {
				DebugMsg("BindLineEffect wrong sDummyTarget:" + m_sName + "," + sTarDummy);
                return null;
            }

            Transform oSelfDummy = GetDummyFromCache(sSelfDummy);
            if (!oSelfDummy)
            {
				DebugMsg("BindLineEffect wrong sDummySelf:" + m_sName + "," + sSelfDummy);
                return null;
            }

            GameObject oLineObj = ResourceMgr.LoadAssetEx(sEffect, true);
            if (!oLineObj)
            {
				DebugMsg("BindLineEffect wrong sEffect:" + sEffect);
                return null;
            }

            oLineObj.SetActive(false);
            Transform trTempDy = transform.Find("Temp_Dummy");
            if (!trTempDy)
            {
                GameObject goTempDy = new GameObject("Temp_Dummy");
                trTempDy = goTempDy.transform;
                trTempDy.SetParent(transform);
            }
            trTempDy.transform.position = oSelfDummy.position;

            Transform oLineTrans = oLineObj.transform;
            oLineTrans.position = oTarDummy.position;
            float fOffsetY = transform.InverseTransformPoint(oSelfDummy.position).y;
            Vector3 v3Offset = fOffsetY * transform.up.normalized;
            UIMgr.Instance.SetAllLineRenderer(oLineObj, 1/10);
            oLineTrans.LookAt(trTempDy.transform.position);
            oLineTrans.SetParent(trTempDy.transform);
            oLineObj.SetActive(true);

            EffectObj oEffObj = new EffectObj(oLineTrans, transform, trTempDy, oTarDummy, v3Offset, nEffTime/1000);
            m_tEffectObj.Add(oLineObj.GetInstanceID(), oEffObj);
            return oLineObj;
        }

        public GameObject BindLineEffectEx(string sEffect, string sDummy, float nPosX, float nPosY, float nPosZ, float fDecTime)
        {
            Transform oSelfDummy = GetDummyFromCache(sDummy);
            if (!oSelfDummy)
            {
                DebugMsg("BindLineEffectEx wrong sDummySelf:" + m_sName + "," + sDummy);
                return null;
            }

            GameObject oLineObj = ResourceMgr.LoadAssetEx(sEffect, true);
            if (!oLineObj)
            {
                DebugMsg("BindLineEffectEx wrong sEffect:" + sEffect);
                return null;
            }

            oLineObj.SetActive(false);
            Transform trTempDy = transform.Find("Temp_Dummy");
            if (!trTempDy)
            {
                GameObject goTempDy = new GameObject("Temp_Dummy");
                trTempDy = goTempDy.transform;
                trTempDy.SetParent(transform);
            }
            trTempDy.transform.position = oSelfDummy.position;

            float fGraphPosZ = SceneMgr.Instance.GetGraphicsPosZ(nPosY);
            Vector3 v3TarPos = new Vector3(nPosX, nPosY, fGraphPosZ);
            Transform oLineTrans = oLineObj.transform;
            float fOffsetY = transform.InverseTransformPoint(oSelfDummy.position).y;
            Vector3 v3Offset = fOffsetY*transform.up.normalized;
            oLineTrans.position = v3TarPos + v3Offset;
            oLineTrans.LookAt(trTempDy.transform.position);
            oLineTrans.SetParent(trTempDy.transform);

            float nLineLen = Vector3.Distance(oSelfDummy.position, v3TarPos);
            UIMgr.Instance.SetAllLineRenderer(oLineObj, nLineLen / 10);
            oLineObj.SetActive(true);

            EffectObj oEffObj = new EffectObj(oLineTrans, transform, trTempDy, v3Offset, nLineLen);
            m_tEffectObj.Add(oLineObj.GetInstanceID(), oEffObj);

            return oLineObj;
        }
        #endregion 特效接口

        void OnDestroy()
        {
		}
    }
}