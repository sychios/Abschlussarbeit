using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

// Synchronizes the players main text and likert values
public class CanvasSynchronize : MonoBehaviour, IInRoomCallbacks
{
    public TMP_Text text;
    public TMP_Text likertValue;
    
    private ExitGames.Client.Photon.Hashtable _properties = new ExitGames.Client.Photon.Hashtable
    {
        {"CurrentText", " -- "},
        {"likertValue", "0"}
    };

    private Dictionary<int, String> _values = new Dictionary<int, string>
    {
        {0, "Value for entry 1" },
        {1, "Value for entry 2" },
        {2, "Value for entry 3" },
        {3, "Value for entry 4" }
    };

    private int _instructionCounter;

    private void Awake()
    {
        var settingCustomProperties = PhotonNetwork.CurrentRoom.SetCustomProperties(_properties);

        if (!settingCustomProperties)
        {
            Debug.LogError("Setting Custom properties was not successful.");
            return;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("CurrentText"))
        {
            text.text = (string) PhotonNetwork.CurrentRoom.CustomProperties["CurrentText"];
        }
        else
        {
            Debug.LogError("Custom property \"CurrentText\" not found.");
        }
        
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("likertValue"))
        {
            likertValue.text = (string) PhotonNetwork.CurrentRoom.CustomProperties["likertValue"];
        }
        else
        {
            Debug.LogError("Custom property \"likertValue\" not found.");
        }
    }

    public void OnContinueButtonClicked()
    {
        _instructionCounter = _instructionCounter == 3 ? 3 : _instructionCounter++;
        SetInstruction(_instructionCounter);
    }

    public void OnReturnButtonClicked()
    {
        _instructionCounter = _instructionCounter == 0 ? 0 : _instructionCounter--;
        SetInstruction(_instructionCounter);
    }

    private void SetInstruction(int counter)
    {
        PhotonNetwork.CurrentRoom.CustomProperties["CurrentText"] = _values[counter];
        PhotonNetwork.CurrentRoom.CustomProperties["likertValue"] = Random.Range(0,8).ToString();
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        throw new NotImplementedException();
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        throw new NotImplementedException();
    }

    public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if(propertiesThatChanged.ContainsKey("CurrentText"))
            text.SetText((string) propertiesThatChanged["CurrentText"]);
        if(propertiesThatChanged.ContainsKey("likertValue"))
            likertValue.SetText((string) propertiesThatChanged["likertValue"]);
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        throw new NotImplementedException();
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        throw new NotImplementedException();
    }
}
