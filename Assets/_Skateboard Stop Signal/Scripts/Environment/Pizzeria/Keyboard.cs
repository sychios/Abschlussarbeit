using UnityEngine;

public class Keyboard : MonoBehaviour
{
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material highlightMaterial;

    private bool creationFinished = false;
    private bool _townFinished = false;
    public bool TownFinished
    {
        get => _townFinished;
        set => _townFinished = value;
    }

    private new MeshRenderer renderer;

    private void Start()
    {
        renderer = GetComponent<MeshRenderer>();
    }

    public void FinishCreation()
    {
        if (!creationFinished && TownFinished)
        {
            GameObject.Find("CreationCanvas").GetComponent<CreationCanvas>().FinishCreation();
            creationFinished = true;
            Untouch();
        }
    }

    public void Touch()
    {
        if (!TownFinished) return;
        renderer.material = highlightMaterial;
    }

    public void Untouch()
    {
        if (!TownFinished) return;
        renderer.material = defaultMaterial;
    }
}
