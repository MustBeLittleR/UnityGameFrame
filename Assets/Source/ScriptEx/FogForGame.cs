using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace WCG
{
	public class FogForGame : MonoBehaviour
	{
        //底图RenderTexture大小
        public float mRenderWidth = 2048;
        public float mRenderHeight = 2048;

        //地图截屏长宽
        public float mMapWidth = 1200;
        public float mMapHeigh = 720;

        //迷雾半径参数
        public float mRadius = 20;
        public float mGradient = 20;

        //处理遮罩的材质
        public Material mFogMaterial = null;

        //相机渲染的遮罩目标
        public GameObject mQuadObj = null;
        private Material mQuadMaterial = null;

        //最终渲染地图效果图
        public RawImage mFullMap = null;
        private Material mBlendMaterial = null;

        //处理迷雾摄像机
        private Camera mCamera = null;
        private RenderTexture mFogRender;

        //缩放值
        private float mScale = 1.0f;
        private Texture mDefault;

        //获取迷雾状态辅助参数
        private Texture2D mTex2D = null;

        //调试变量
        bool bDoDraw = false;
        bool bInitDebug = false;

        string mMaskPath;

        // Use this for initialization
        void Awake()
		{
            mMaskPath = Application.persistentDataPath + "/fogMask.png";

            mQuadMaterial = mQuadObj.GetComponent<MeshRenderer>().material;
            mBlendMaterial = mFullMap.material;

            mCamera = GetComponent<Camera>();
            //mCamera.enabled = false;
            mDefault = mQuadMaterial.GetTexture("_MainTex");

            mFogRender = mCamera.targetTexture;
            mTex2D = new Texture2D(1, 1);
		}

		public void InitFogSize(float nWidth, float nHeight)
		{
            //截屏的长宽
            mMapWidth = nWidth;
            mMapHeigh = nHeight;
            mScale = 1.0f;

            //如果截屏比我们设置的renderTexture大，需要进行缩放
            if (mMapWidth > mRenderWidth || mMapHeigh > mRenderHeight)
            {
                float scaleWidth = mRenderWidth / mMapWidth;
                float scaleHeight = mRenderHeight / mMapHeigh;
                mScale = (scaleWidth < scaleHeight) ? scaleWidth : scaleHeight;
            }

            //混合的时候的缩放值
            mBlendMaterial.SetFloat("_MaskWidthScale", (mMapWidth * mScale) / mRenderWidth);
            mBlendMaterial.SetFloat("_MaskHeightScale", (mMapHeigh * mScale) / mRenderHeight);

            //传值RenderTexture长宽
            mFogMaterial.SetFloat("_RenderWidth", mRenderWidth);
            mFogMaterial.SetFloat("_RenderHeight", mRenderHeight);
        }

        //重置Mask值
        public void ReSetMaskTexture()
        {
            mQuadMaterial.SetTexture("_MainTex", mDefault);
        }

        //使用老的
        public void UseOldTexture()
        {
            if (!File.Exists(mMaskPath))
                ReSetMaskTexture();
            else
            {
                byte[] data = File.ReadAllBytes(mMaskPath);
                Texture2D texture = new Texture2D(1,1);
                bool doLoad = texture.LoadImage(data);

                if (texture == null || !doLoad)
                    ReSetMaskTexture();
                else
                    mQuadMaterial.SetTexture("_MainTex", texture);
            }
        }

        //保存当前的Mask
        public void SaveRenderTexture()
        {
            if (mFogRender == null)
                return;

            Texture2D tex2d = new Texture2D(mFogRender.width, mFogRender.height, TextureFormat.ARGB32, false);
            RenderTexture.active = (RenderTexture)mQuadMaterial.GetTexture("_MainTex");//mFogRender;
            tex2d.ReadPixels(new Rect(0, 0, mFogRender.width, mFogRender.height), 0, 0);
            tex2d.Apply();
            RenderTexture.active = null;

            mQuadMaterial.GetTexture("_MainTex");

            byte[] data = tex2d.EncodeToPNG();

            if (File.Exists(mMaskPath))
                File.Delete(mMaskPath);

            File.WriteAllBytes(mMaskPath, data);
        }

		public void SyncFogInfo(float fPosX, float fPosY, float fRadius, float fGradient)
		{
            if (fPosX >= 1 || fPosX <= 0 || fPosY >= 1 || fPosY <= 0)
                return;

            mRadius = (int)(fRadius / 8);
            mGradient = (int)(fGradient / 8);
            float fMapPosX = fPosX * mMapWidth;
            float fMapPosY = fPosY * mMapHeigh;

            //如果截屏比我们设置的RenderTexture大，需要进行缩放
            if (mMapWidth > mRenderWidth || mMapHeigh > mRenderHeight)
            {
                //对当前位置坐标值进行缩放
                fMapPosX = fMapPosX * mScale;
                fMapPosY = fMapPosY * mScale;
            }

            //计算在renderTexture上面的UV坐标
            fPosX = fMapPosX / mRenderWidth;
            fPosY = fMapPosY / mRenderHeight;

            //传UV坐标和半径以及渐变参数
            mFogMaterial.SetColor("_FogOfWar", new Color(fPosX, fPosY, mRadius, mGradient));
            //mCamera.Render();
            bDoDraw = true;
        }

		public float GetFogState(float fPosX, float fPosY)
		{
			if (fPosX >= 1 || fPosX <= 0 || fPosY >= 1 || fPosY <= 0)
				return 0;

            float fFog = 0.0f;
            float fMapPosX = fPosX * mMapWidth;
            float fMapPosY = fPosY * mMapHeigh;

            //如果截屏比我们设置的RenderTexture大，需要进行缩放
            if (mMapWidth > mRenderWidth || mMapHeigh > mRenderHeight)
            {
                //对当前位置坐标值进行缩放
                fMapPosX = fMapPosX * mScale;
                fMapPosY = fMapPosY * mScale;
            }

			RenderTexture.active = mFogRender;
			mTex2D.ReadPixels(new Rect(fMapPosX, fMapPosY, 1, 1), 0, 0);
			mTex2D.Apply();
			RenderTexture.active = null;
			fFog = mTex2D.GetPixel(0, 0).a;

			return fFog;
		}

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (bDoDraw && (Time.frameCount % 10 == 0))
            {
                //mBlendMaterial.SetTexture("_Mask", destination);
                mQuadMaterial.SetTexture("_MainTex", destination);
                Graphics.Blit(source, destination, mFogMaterial);
                bDoDraw = false;
            }
            else
                Graphics.Blit(source, destination);
        }

        /*void Update()
        {
            if (Input.GetMouseButtonDown(0))
                bDoDraw = true;
            else if (Input.GetMouseButtonUp(0))
                bDoDraw = false;

            if (bDoDraw)
            {
                if (!bInitDebug)
                {
                    bInitDebug = true;
                    InitFogSize(mMapWidth, mMapHeigh);
                }

                Vector3 pos = Input.mousePosition;
                float fRateX = pos.x / Screen.width;
                float fRateY = pos.y / Screen.height;
                SyncFogInfo(fRateX, fRateY, mRadius, mGradient);
            }
        }*/
    }
}
