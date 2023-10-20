using Photon.Pun;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    private readonly string _assistantPrefab = "PlayerPrefabs/ExposureAssistant";
    private readonly string participantPrefab = "PlayerPrefabs/ExposureParticipant";
    
    [SerializeField] private Transform spawnPointObject;
    private Vector3 _spawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        _spawnPoint = spawnPointObject.position;
        
        if (Application.platform == RuntimePlatform.WindowsEditor 
            || Application.platform == RuntimePlatform.WindowsPlayer 
            || Application.platform == RuntimePlatform.OSXEditor
            || Application.platform == RuntimePlatform.OSXPlayer
            || Application.platform == RuntimePlatform.LinuxEditor
            || Application.platform == RuntimePlatform.LinuxPlayer)
        {
            CreationPlayerManager.LocalPlayerInstance = PhotonNetwork.Instantiate(_assistantPrefab, _spawnPoint, Quaternion.identity);
        } 
        else if (Application.platform == RuntimePlatform.Android)
        {
            GameObject instance = PhotonNetwork.Instantiate(participantPrefab, _spawnPoint, Quaternion.identity);
            CreationPlayerManager.LocalPlayerInstance = instance;
            
            BorderCollider[] borders = GameObject.Find("Border").transform.GetComponentsInChildren<BorderCollider>();

            foreach (var border in borders)
            {
                border.skaterController = instance.GetComponent<SkaterController>();
            }
        }
    }
}
