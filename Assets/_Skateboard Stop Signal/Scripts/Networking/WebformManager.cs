using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

public class WebformManager : MonoBehaviour
{
    
    private string url = "http://127.0.0.1:5000";

    private static readonly string hashString = "ynoha2QFNgHIqZsTFcxEOB4xK26v0VWi5oONAGLaf75M3p6q5gssi3H5c4K5su5c0wzrjqIUrhTP8VwNMtCSOlgQzcCGLotvp6mHibwoANa3LfjkUSeKwXwH0NVTSGel";

    // Start is called before the first frame update
    void Start()
    {
        StopAllCoroutines();
        StartCoroutine(Upload("http://127.0.0.1:5000/save", ToCsv()));

        //StartCoroutine(Send(_url));
    }

    IEnumerator Upload(string url, string csvString)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", Encoding.UTF8.GetBytes(csvString));
        form.AddField("name", "fileName");
        form.AddField("id", Random.Range(0,30));
        form.AddField("u_hash", Sha256(hashString));
        
        UnityWebRequest uwr = UnityWebRequest.Post(url, form);
        yield return uwr.SendWebRequest();
        
        if (uwr.isNetworkError)
        {
            Debug.Log("Error while handling " + uwr.error);
            StartCoroutine(Upload(this.url, csvString));
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
        }
    }
    
    static string Sha256(string str)
    {
        SHA256Managed crypt = new SHA256Managed();
        StringBuilder hash = new StringBuilder();

        // offset: 0
        byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(str), 0, Encoding.UTF8.GetByteCount(str));

        foreach (byte b in crypto)
        {
            // ToString: hexadecimal
            hash.Append(b.ToString("x2"));
        }

        return hash.ToString().ToLower();
    }
    
    

    string ToCsv()
    {

        
        string[] h = {"id", "condition", "phase" , "event", "time"};

        string[] entry1 = {"0", "A", "intro", "PUT", "23.04.2021#15.32.22"};
        string[] entry2 = {"1", "A", "creation", "DEL", "23.04.2021#15.41.23"};
        string[] entry3 = {"2", "B", "ssttrial", "DEL", "23.04.2021#15.44.22"};
        string[] entry4 = {"3", "B", "sst", "ROT", "23.04.2021#15.32.22"};

        

        string header = string.Join(";",h);
        string values = "";
        values += string.Join(";",entry1) + "\n";
        values += string.Join(";",entry2) + "\n";
        values += string.Join(";",entry3) + "\n";
        values += string.Join(";",entry4) + "\n";
        values += string.Join(";",entry4) + "\n";
        
        string output = string.Join("\n",header,values);
        return output;
    }
}
