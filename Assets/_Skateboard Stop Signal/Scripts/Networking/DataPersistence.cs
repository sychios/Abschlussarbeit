using Photon.Pun;
using UnityEngine;

public class DataPersistence : MonoBehaviour
{
    //private string _directoryPath = "C:\\Users\\Marius\\Documents\\Studie\\";
    private string directoryPath;

    private PhotonView view;

    private void Start()
    {
        directoryPath = Application.persistentDataPath + "\\Studie\\";
        view = GetComponent<PhotonView>();
    }

    public void Call_SaveFile(string path, string fileName, string fileData)
    {
        if(view.IsMine)
            view.RPC("SaveFile", RpcTarget.Others, path, fileName, fileData);
    }
    
    [PunRPC]
    public void SaveFile(string path, string filename, string fileData)
    {
        QuestionnairePersistence.WriteFile(directoryPath + path, filename, fileData);
    }
    
    
}
