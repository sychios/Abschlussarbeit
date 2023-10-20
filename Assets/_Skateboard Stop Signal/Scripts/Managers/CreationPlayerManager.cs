using Photon.Pun;
using UnityEngine;

// Keeps track of player instances in creation scene
public class CreationPlayerManager : MonoBehaviour
{
    private Transform creationSpawnPoint;

    private const string CreationAssistantPrefab = "PlayerPrefabs/CreationAssistant";
    private const string CreationParticipantPrefab = "PlayerPrefabs/CreationOVRPlayerController";

    public static GameObject LocalPlayerInstance;
    
    // Start is called before the first frame update
    private void Start()
    {
        creationSpawnPoint = GameObject.FindWithTag("Respawn").transform;

        if (Application.platform == RuntimePlatform.Android)
        {
            LocalPlayerInstance = PhotonNetwork.Instantiate(CreationParticipantPrefab, creationSpawnPoint.position,
                creationSpawnPoint.rotation);
            PhotonNetwork.SetMasterClient(LocalPlayerInstance.GetPhotonView().Owner);
        }
        else if(Application.platform == RuntimePlatform.WindowsEditor 
                || Application.platform == RuntimePlatform.WindowsPlayer 
                || Application.platform == RuntimePlatform.OSXEditor
                || Application.platform == RuntimePlatform.OSXPlayer
                || Application.platform == RuntimePlatform.LinuxEditor
                || Application.platform == RuntimePlatform.LinuxPlayer)
        {
            LocalPlayerInstance = PhotonNetwork.Instantiate(CreationAssistantPrefab, creationSpawnPoint.position, creationSpawnPoint.rotation);
        }
    }

    //So we stop loading scenes if we quit app
    private void OnApplicationQuit()
    {
        StopAllCoroutines();
    }
}
