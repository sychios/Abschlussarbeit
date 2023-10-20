using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LikertScale : MonoBehaviour
{
    private ToggleGroup _toggleGroup;
    [SerializeField] private Toggle[] toggles = new Toggle[7]; // 7 or 21-item likert-scale

    [SerializeField] private TMP_Text[] likertLabels = new TMP_Text[7]; // have to be assigned from left to right
    private void Awake()
    {
        _toggleGroup = GetComponentInChildren<ToggleGroup>();
        Reset();
    }

    public void SetAllLabels(string[] labels) //index out of bounds
    {
        if (likertLabels.Length == 2)
        {
            likertLabels[0].SetText(labels[0]);
            likertLabels[1].SetText(labels[labels.Length-1]);
            return;
        }
        
        if (labels.Length != likertLabels.Length)
            return; //TODO: 21-likert fails here? need 19 empty texts

        for (var i = 0; i < likertLabels.Length; i++)
        {
            likertLabels[i].SetText(labels[i]);
        }

        return;
    }

    public void Reset()
    {
        _toggleGroup.SetAllTogglesOff();
    }

    public bool IsAnswered()
    {
        return _toggleGroup.AnyTogglesOn();
    }

    public void SetAnswer(string toggle)
    {
        for (int i = 0; i < toggles.Length; i++)
        {
            if (toggles[i].gameObject.name == toggle)
            {
                toggles[i].isOn = true;

                break;
            }
        }
    }

    public string GetAnswer()
    {
        if (IsAnswered())
        {
            foreach (var toggle in _toggleGroup.ActiveToggles())
            {
                return toggle.gameObject.name;
            }
        }

        return "None";
    }

    public int GetAnswerAsInt()
    {
        if (IsAnswered())
        {
            for (int i = 0; i < toggles.Length; i++)
            {
                if (toggles[i].isOn)
                    return i;
            }
        }

        return -1;
    }
}
