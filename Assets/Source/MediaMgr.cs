using System;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System.Collections;
using LuaInterface;

namespace WCG
{
	public class MediaMgr
	{
		public static readonly MediaMgr Instance = CreateInstance();
		static MediaMgr CreateInstance()
		{
			GameObject go = GameMgr.Instance.gameObject;
			MediaMgr mgr = new MediaMgr();
			mgr.m_MusicSource = go.AddComponent<AudioSource>();
			mgr.m_SoundSource = go.AddComponent<AudioSource>();
			return mgr;
		}

        AudioClip m_OneShotClip;

        AudioSource m_MusicSource;
		AudioSource m_SoundSource;

        public MediaMgr ()
		{}

		public bool bMusicMute
        {
			get { return m_MusicSource.mute; }
			set { m_MusicSource.mute = value; }
		}
		public bool bSoundMute
        {
			get { return m_SoundSource.mute; }
			set { m_SoundSource.mute = value; }
		}
		public bool bMusicIsPlaying
		{
			get { return m_MusicSource.isPlaying; }
		}
		public bool bSoundIsPlaying
		{
			get { return m_SoundSource.isPlaying; }
        }


		#region 音乐
		//播放2D剪辑音乐 sFileName:(Data/media/music/)下的相对文件完整路径
        public void PlayMusic(string sMusicName, bool bLoop)
		{
			string pathName = "media/music/" + sMusicName;
			if (null != m_MusicSource.clip)
				if (pathName.Equals(m_MusicSource.clip.name))
                {
                    if (!m_MusicSource.isPlaying)
                        m_MusicSource.Play();
                    return;
                }

			UnityAction<UnityEngine.Object> ac = (obj) =>
			{
				if (null != obj)
				{
					AudioClip clip = (AudioClip)obj;
					if (null != clip)
					{
						ResourceMgr.UnLoadAsset(m_MusicSource.clip);
						clip.name = pathName;
						m_MusicSource.clip = clip;
						m_MusicSource.loop = bLoop;
						m_MusicSource.Play();
					}
				}
			};

			RunMode eRunMode = GameMgr.Instance.m_eRunMode;
			if (eRunMode == RunMode.eDeveloper)
				ac(GetClip(pathName));
			else if (eRunMode == RunMode.eDebug || eRunMode == RunMode.eRelease)
				GetClipAsync(pathName, ac);
		}
		public void SetMusicVolume(float fvolume)
		{
			m_MusicSource.volume = fvolume;
		}

		public float GetMusicVolume()
		{
			return m_MusicSource.volume;
		}

		public void SetMusicPause(bool bPause)
		{
            if (bPause)
			    m_MusicSource.Pause();
            else
                m_MusicSource.UnPause();
        }

        public void StopMusic()
		{
			m_MusicSource.Stop();
        }

        public void ReplayMusic()
        {
            m_MusicSource.Play();
        }
        #endregion

        #region 音效
        //播放2D剪辑音效(只能单播一次剪辑音效) sFileName:(Data/media/sound/)下的相对文件完整路径
        public float PlaySound(string sSoundName, bool bLoop)
        {
            float nSoundLength = 0.0f;
            if (m_SoundSource.mute) return nSoundLength;

			string pathName = "media/sound/" + sSoundName;
            if (null != m_OneShotClip)
                if (pathName.Equals(m_OneShotClip.name))
                {
                    m_SoundSource.PlayOneShot(m_OneShotClip, m_SoundSource.volume);
                    return m_OneShotClip.length;
                }

            UnityAction<UnityEngine.Object> ac = (obj) =>
            {
                if (null != obj)
                {
                    AudioClip clip = (AudioClip)obj;
                    if (null != clip)
                    {
                        if (m_OneShotClip)
                        {
                            ResourceMgr.UnLoadAsset(m_OneShotClip);
                            m_OneShotClip = null;
                            m_OneShotClip = clip;
                        }
                        clip.name = pathName;
                        m_SoundSource.PlayOneShot(clip, m_SoundSource.volume);
                        nSoundLength = clip.length;
                    }
                }
            };

            RunMode eRunMode = GameMgr.Instance.m_eRunMode;
			if(eRunMode == RunMode.eDeveloper)
				ac(GetClip(pathName));
			else if (eRunMode == RunMode.eDebug || eRunMode == RunMode.eRelease)
				GetClipAsync(pathName, ac);

			return nSoundLength;
		}

        public void SetSoundVolume(float fvolume)
        {
            m_SoundSource.volume = fvolume;
        }

        public float GetSoundVolume()
        {
            return m_SoundSource.volume;
        }

        public void SetSoundPause(bool bPause)
        {
            if (bPause)
                m_SoundSource.Pause();
            else
                m_SoundSource.UnPause();
        }

        public void StopSound()
        {
            m_SoundSource.Stop();
        }

        public void ReplaySound()
        {
            m_SoundSource.Play();
        }
        #endregion

        #region LoadClip
        public AudioClip GetClip(string pathName)
		{
			if (string.IsNullOrEmpty(Path.GetExtension(pathName)))
			if (!pathName.EndsWith(".ogg"))
				pathName = pathName + ".ogg";

			string assetName = pathName.Substring(pathName.LastIndexOf("/") + 1);
			assetName = assetName.Substring(0, assetName.LastIndexOf("."));
			UnityEngine.Object clip = ResourceMgr.LoadAsset(pathName, assetName, typeof(AudioClip));
			return null != clip ? clip as AudioClip : null;
		}

		public void GetClipAsync(string pathName, UnityAction<UnityEngine.Object> cal)
		{
			if (string.IsNullOrEmpty(Path.GetExtension(pathName)))
			if (!pathName.EndsWith(".ogg"))
				pathName = pathName + ".ogg";
			
			string assetName = pathName.Substring(pathName.LastIndexOf("/") + 1);
			assetName = assetName.Substring(0, assetName.LastIndexOf("."));
			ResourceMgr.LoadAssetAsync(pathName, assetName, typeof(AudioClip), cal);
		}
        #endregion

        #region 视屏
        // fShowLoginWindow, 第一次播放完视频后，显示登录窗口，并且调用下面函数注册播放视频tick。
        // fPlayVideoTick, 如果不是第一次播放视频，直接注册播放视频tick。
        public bool PlayVideo(string sVideoName, bool bIsFirstLaunch, LuaFunction fCallBack)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (fCallBack != null)
            {
                fCallBack.Call();
            }
            return true;
#else
            GameMgr.Instance.StartCoroutine(PlayVideoOnHandheldDevices(sVideoName, bIsFirstLaunch, fCallBack));
            return false;
#endif
        }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
#else
        private bool m_bIsFirstPlay = true;
        IEnumerator PlayVideoOnHandheldDevices(string sVideoName, bool bIsFirstLaunch, LuaFunction fCallBack)
        {
            yield return new WaitForEndOfFrame();
            // 把视频文件放到Assets/StreamingAssets里面

            if (bIsFirstLaunch)
            {// 如果是第一次启动游戏，强制看完宣传视频。。。
                Handheld.PlayFullScreenMovie(sVideoName, Color.black, FullScreenMovieControlMode.Hidden);
            }
            else
            {
                Handheld.PlayFullScreenMovie(sVideoName, Color.black, FullScreenMovieControlMode.CancelOnInput);
            }

            // 以下参考http://answers.unity3d.com/questions/305407/how-to-check-if-handheldplayfullscreenmovie-has-st.html
            yield return new WaitForEndOfFrame();

            // 如果是第一次播放视频，要手动显示登录窗口，之前试过播放完后再创建登录窗口，然而会有延迟，中间会蓝屏。
            // 也试过播放视频时先创建登录窗口，播放完后(在第二个WaitForEndOfFrame之后)显示，也会有蓝屏，因为播放完后，主相机的背景。
            // 现在的做法是，在第二个WaitForEndOfFrame之前，直接显示窗口，经测试，这里调用显示并不会被渲染出来，
            // 等视频结束后，登录窗口才被渲染出来，正是我们想要的。
            if (fCallBack != null)
                fCallBack.Call();

            yield return new WaitForEndOfFrame();
        }
#endif
	}
    #endregion
}

