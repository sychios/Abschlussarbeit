using System;
using UnityEngine;
using UnityEngine.UI;

public class AssistantLikertScale : MonoBehaviour
{
    private ToggleGroup _toggleGroup;
    [SerializeField] private Toggle[] toggles = new Toggle[7]; // left to right, low to high
    
    private void Awake()
    {
        _toggleGroup = GetComponentInChildren<ToggleGroup>();
    }

    private void OnEnable()
    {
        Reset();
    }

    public void Reset()
    {
        foreach (var toggle in toggles)
        {
            toggle.isOn = false;
        }
    }
    
    public void SetAnswer(int toggleIndex)
    {
        if (toggleIndex > 7) // ceil 21-item likert to 7-item
        {
            toggleIndex = Mathf.CeilToInt(toggleIndex / 3f);
        }
        Reset();
        if (toggleIndex >= 0)
            try
            {
                toggles[toggleIndex].isOn = true;
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine(e);
                throw;
            }
    }
}
