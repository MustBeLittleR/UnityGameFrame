using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisposeCamRTAlpha : MonoBehaviour {

    public float mAlpha = 1.0f;
    public Material mMat = null;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (mAlpha < 0 || mAlpha > 1)
        {
            Debug.LogError("mAlpha < 0 || mAlpha > 1");
            return;
        }

        mMat.SetFloat("_Alpha", mAlpha);
        Graphics.Blit(source, destination, mMat);
    }
}
