using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using POpusCodec.Enums;
using UnityEngine;

public class ScreenShot : MonoBehaviour
{
    private PhotonView view;

    private const int ScreenShotWidth = 1280;
    private const int ScreenShotHeight = 720;

    private List<Camera> cameras = new List<Camera>();
    private void Start()
    {
        view = GetComponent<PhotonView>();
        var cameras = GameObject.FindGameObjectsWithTag("ScreenshotCamera");
        
        if (!view.IsMine)
        {
            foreach (var cam in cameras)
            {
                Destroy(cam);
            }
            Destroy(this);
            return;
        }
        
        cameras.ToList().ForEach(c => this.cameras.Add(c.GetComponent<Camera>()));
    }

    public void TakeScreenShotsAndSaveToPath(string path)
    {
        try
        {
            if(cameras.Count == 0)
                throw new Exception("Screenshot camera list is empty.");

            var props = PhotonNetwork.CurrentRoom.CustomProperties;
            props.TryGetValue(RoomProperty.Condition, out var condition);
            props.TryGetValue(RoomProperty.ParticipantId, out var pId);
            var userSubString = "";
            if (condition as string != "")
            {
                userSubString += condition +  "_" ;
            }

            if (pId as string != "")
            {
                userSubString += pId + "_";
            }
            
            foreach (var cam in cameras)
            {
                cam.enabled = true;
                var pngByteArray = TakeScreenShot(cam);
                var camType = cam.orthographic ? "orth" : "persp";
                var filename = userSubString + cam.gameObject.name + "_" + camType + ".png";
                QuestionnairePersistence.WritePicture(path, filename, pngByteArray);
                cam.enabled = false;
            }

        }
        catch (Exception e)
        {
            Debug.LogError("Taking screenshot failed: " + e.Message);
            throw;
        }
    }

    private byte[] TakeScreenShot(Camera cam)
    {
        RenderTexture rt = new RenderTexture(ScreenShotWidth, ScreenShotHeight, 24);
        cam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(ScreenShotWidth, ScreenShotHeight, TextureFormat.RGB24, false);
        cam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, ScreenShotWidth, ScreenShotHeight), 0, 0);
        cam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        return screenShot.EncodeToPNG();
    }
}
