using Photon.Pun;
using TMPro;
using UnityEngine;

public class RoomPropertiesCanvas : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    
    // Start is called before the first frame update
    void Start()
    {
        text.SetText("No Properties refreshed.");
    }

    public void OnClick()
    {
        object condition;
        object id;
        PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(RoomProperty.Condition, out condition);
        PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(RoomProperty.ParticipantId, out id);
        
        text.SetText("Condition: " + condition + "\nID: " + id);
    }
}
