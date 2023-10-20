using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class Skateboard : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject exitCanvas;

    private void Start()
    {
        continueButton.interactable = false;
        exitCanvas.SetActive(false);
    }

    public void LoadExposure()
    {
        StartCoroutine(StartExposure());
    }

    public IEnumerator StartExposure()
    {
        CreationPlayerManager.LocalPlayerInstance.GetComponent<AgoraLauncher>().Call_LeaveChannelRPC();
        
        yield return new WaitUntil(
            () => CreationPlayerManager.LocalPlayerInstance.GetComponent<AgoraLauncher>().ChannelLeft);

        PhotonNetwork.LoadLevel("SampleScene");
    }

    public void EnableCanvas()
    {
        continueButton.interactable = true;
        exitCanvas.SetActive(true);
    }
}
