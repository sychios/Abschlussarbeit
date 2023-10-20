using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SkyboxTransition : MonoBehaviour
{
    private PhotonView view;
    
    private readonly int cubemapTransition = Shader.PropertyToID("_CubemapTransition");

    private void Start()
    {
        view = GetComponent<PhotonView>();
        if(!view.IsMine)
            Destroy(this);
    }

    public void StartDayToNightTransition()
    {
        StartCoroutine(DayToNightTransition());
    }
    
    private IEnumerator DayToNightTransition()
    {
        var init = 0f;
        while (!(RenderSettings.skybox.GetFloat(cubemapTransition) >= 1f))
        {
            init += 0.01f;
            if (init > 1)
                init = 1;
            RenderSettings.skybox.SetFloat(cubemapTransition, init);
            yield return new WaitForSeconds(1f);
        }
    }
}
