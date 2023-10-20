using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class BorderCollider : MonoBehaviour
{
    public SkaterController skaterController;

    public OVRScreenFade ovrScreenFade;
    
    private const float FadeTime = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        GameObject otherGameObject = other.gameObject;

        StartCoroutine(FadeInAndTeleport(otherGameObject));
    }

    private IEnumerator FadeInAndTeleport(GameObject player)
    {
        ovrScreenFade = player.GetComponentInChildren<OVRScreenFade>();
        
        ovrScreenFade.FadeOut(FadeTime);
        
        yield return new WaitForSeconds(FadeTime);
        yield return new WaitForEndOfFrame();
        
        skaterController.ResetPlayerPosition();
        
        ovrScreenFade.FadeIn(2.5f);
    }
}
