using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnvironmentLanguageManager : MonoBehaviour
{
    // Reference to all chunk labels
    public GameObject[] labels;
    
    private readonly Dictionary<string, string> objectNameToGermanLabel = new Dictionary<string, string>
    {
        {"campus_0", "Campus 1"},
        {"campus_1", "Campus 2"},
        {"campus_2", "Campus 3"},
        {"canal_0", "Kanal\n1"},
        {"canal_1", "Kanal\n2"},
        {"canal_2", "Kanal\n3"},
        {"canal_3", "Kanal\n4"},
        {"cultural_0", "Kulturstätte\n1"},
        {"cultural_1", "Kulturstätte\n2"},
        {"haus_der_wissenschaft", "Liga der Wissenschaft"},
        {"alter_frachthafen", "Alter\nFrachthafen"},
        {"nature_0", "Naturpark 1"},
        {"nature_1", "Naturpark 2"},
        {"nature_2", "Naturpark 3"},
        {"industrial_0", "Fabrik 1"},
        {"industrial_1", "Fabrik 2"},
        {"industrial_2", "Fabrik 3"},
        {"residential_0", "Wohnviertel 1"},
        {"residential_1", "Wohnviertel 2"},
        {"residential_2", "Wohnviertel 3"},
        {"residential_3", "Wohnviertel 4"},
        {"spaceport_0", "Raumhafen 1"},
        {"spaceport_1", "Raumhafen 2"},
        {"ariane", "Ariane 11"},
        {"zwillingsgipfel", "Zwillingsgipfel"},
        {"stadthalle", "Rathaus"},
        {"skyscraper_0", "Wolkenkratzer 1"},
        {"skyscraper_1", "Wolkenkratzer 2"},
        {"skyscraper_2", "Wolkenkratzer 3"},
        {"skyscraper_3", "Wolkenkratzer 4"},
        
        {"HeaderLeft", "Links"},
        {"HeaderRight", "Rechts"}
    };
    
    private readonly Dictionary<string, string> objectNameToEnglishLabel = new Dictionary<string, string>
    {
        {"campus_0", "Campus 1"},
        {"campus_1", "Campus 2"},
        {"campus_2", "Campus 3"},
        {"canal_0", "Canal\n1"},
        {"canal_1", "Canal\n2"},
        {"canal_2", "Canal\n3"},
        {"canal_3", "Canal\n4"},
        {"cultural_0", "Cultural\nSite 1"},
        {"cultural_1", "Cultural\nSite 2"},
        {"haus_der_wissenschaft", "League of Science"},
        {"alter_frachthafen", "Old\nShipping Port"},
        {"nature_0", "Nature Park 1"},
        {"nature_1", "Nature Park 2"},
        {"nature_2", "Nature Park 3"},
        {"industrial_0", "Plant 1"},
        {"industrial_1", "Plant 2"},
        {"industrial_2", "Plant 3"},
        {"residential_0", "Residential Area 1"},
        {"residential_1", "Residential Area 2"},
        {"residential_2", "Residential Area 3"},
        {"residential_3", "Residential Area 4"},
        {"spaceport_0", "Spaceport 1"},
        {"spaceport_1", "Spaceport 2"},
        {"ariane", "Ariane 11"},
        {"zwillingsgipfel", "Twin Hills"},
        {"stadthalle", "City Hall"},
        {"skyscraper_0", "Skyscraper 1"},
        {"skyscraper_1", "Skyscraper 2"},
        {"skyscraper_2", "Skyscraper 3"},
        {"skyscraper_3", "Skyscraper 4"},
        
        {"HeaderLeft", "Left"},
        {"HeaderRight", "Right"}
    };
    
    
    private readonly Dictionary<string, string> objectNameToGermanLabelWithoutNewLines = new Dictionary<string, string>
    {
        {"campus_0", "Campus 1"},
        {"campus_1", "Campus 2"},
        {"campus_2", "Campus 3"},
        {"canal_0", "Kanal 1"},
        {"canal_1", "Kanal 2"},
        {"canal_2", "Kanal 3"},
        {"canal_3", "Kanal 4"},
        {"cultural_0", "Kulturstätte 1"},
        {"cultural_1", "Kulturstätte 2"},
        {"haus_der_wissenschaft", "Liga der Wissenschaft"},
        {"alter_frachthafen", "Alter Frachthafen"},
        {"nature_0", "Naturpark 1"},
        {"nature_1", "Naturpark 2"},
        {"nature_2", "Naturpark 3"},
        {"industrial_0", "Fabrik 1"},
        {"industrial_1", "Fabrik 2"},
        {"industrial_2", "Fabrik 3"},
        {"residential_0", "Wohnviertel 1"},
        {"residential_1", "Wohnviertel 2"},
        {"residential_2", "Wohnviertel 3"},
        {"residential_3", "Wohnviertel 4"},
        {"spaceport_0", "Raumhafen 1"},
        {"spaceport_1", "Raumhafen 2"},
        {"ariane", "Ariane 11"},
        {"zwillingsgipfel", "Zwillingsgipfel"},
        {"stadthalle", "Rathaus"},
        {"skyscraper_0", "Wolkenkratzer 1"},
        {"skyscraper_1", "Wolkenkratzer 2"},
        {"skyscraper_2", "Wolkenkratzer 3"},
        {"skyscraper_3", "Wolkenkratzer 4"},
        
        {"HeaderLeft", "Links"},
        {"HeaderRight", "Rechts"}
    };
    
    private readonly Dictionary<string, string> objectNameToEnglishLabelWithoutNewLines = new Dictionary<string, string>
    {
        {"campus_0", "Campus 1"},
        {"campus_1", "Campus 2"},
        {"campus_2", "Campus 3"},
        {"canal_0", "Canal 1"},
        {"canal_1", "Canal 2"},
        {"canal_2", "Canal 3"},
        {"canal_3", "Canal 4"},
        {"cultural_0", "Cultural Site 1"},
        {"cultural_1", "Cultural Site 2"},
        {"haus_der_wissenschaft", "League of Science"},
        {"alter_frachthafen", "Old Shipping Port"},
        {"nature_0", "Nature Park 1"},
        {"nature_1", "Nature Park 2"},
        {"nature_2", "Nature Park 3"},
        {"industrial_0", "Plant 1"},
        {"industrial_1", "Plant 2"},
        {"industrial_2", "Plant 3"},
        {"residential_0", "Residential Area 1"},
        {"residential_1", "Residential Area 2"},
        {"residential_2", "Residential Area 3"},
        {"residential_3", "Residential Area 4"},
        {"spaceport_0", "Spaceport 1"},
        {"spaceport_1", "Spaceport 2"},
        {"ariane", "Ariane 11"},
        {"zwillingsgipfel", "Twin Hills"},
        {"stadthalle", "City Hall"},
        {"skyscraper_0", "Skyscraper 1"},
        {"skyscraper_1", "Skyscraper 2"},
        {"skyscraper_2", "Skyscraper 3"},
        {"skyscraper_3", "Skyscraper 4"},
        
        {"HeaderLeft", "Left"},
        {"HeaderRight", "Right"}
    };

    public void SetLabelsToLanguage(bool isLanguageGerman)
    {
        string value;
        if (isLanguageGerman)
        {
            foreach (var go in labels)
            {
                if(objectNameToGermanLabel.TryGetValue(go.name, out value))
                {
                    go.GetComponent<TMP_Text>().SetText(value);
                }
                else
                {
                    Debug.LogWarning($"German label for GameObject {go.name} could not be found in dictionary. Name remains unchanged.");
                }
            }
        }
        else
        {
            foreach (var go in labels)
            {
                if(objectNameToEnglishLabel.TryGetValue(go.name, out value))
                {
                    go.GetComponent<TMP_Text>().SetText(value);
                }
                else
                {
                    Debug.LogWarning($"English label for GameObject {go.name} could not be found in dictionary. Name remains unchanged.");
                }
            }
        }
    }
    
    public void SetLabelsToLanguageWithoutNewLines(bool isLanguageGerman)
    {
        string value;
        if (isLanguageGerman)
        {
            foreach (var go in labels)
            {
                if(objectNameToGermanLabelWithoutNewLines.TryGetValue(go.name, out value))
                {
                    go.GetComponentInChildren<TMP_Text>().SetText(value);
                }
                else
                {
                    Debug.LogWarning($"German label for GameObject {go.name} could not be found in dictionary. Name remains unchanged.");
                }
            }
        }
        else
        {
            foreach (var go in labels)
            {
                if(objectNameToEnglishLabelWithoutNewLines.TryGetValue(go.name, out value))
                {
                    go.GetComponentInChildren<TMP_Text>().SetText(value);
                }
                else
                {
                    Debug.LogWarning($"English label for GameObject {go.name} could not be found in dictionary. Name remains unchanged.");
                }
            }
        }
    }
}
