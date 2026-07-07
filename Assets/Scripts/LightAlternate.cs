using UnityEngine;
using System.Collections.Generic;

public class LightAlternate : MonoBehaviour
{
    private List<Light> lights = new List<Light>();

    private int currentLight = 0;
    private int lastBeat = -1;

    void Start()
    {
        // Prende tutte le luci figlie
        foreach (Transform child in transform)
        {
            Light light = child.GetComponent<Light>();

            if (light != null)
            {
                lights.Add(light);
                light.enabled = false;
            }
        }

        // Accende la prima
        if (lights.Count > 0)
            lights[0].enabled = true;
    }

    void Update()
    {
        if (IsNewBeat())
        {
            AlternateLights();
        }
    }

    private void AlternateLights()
    {
        if (lights.Count == 0)
            return;

        // Spegne la luce corrente
        lights[currentLight].enabled = false;

        // Passa alla successiva
        currentLight = (currentLight + 1) % lights.Count;

        // Accende la nuova
        lights[currentLight].enabled = true;
    }

    private bool IsNewBeat()
    {
        if (RhythmManager.Instance == null)
            return false;

        int currentBeat = RhythmManager.Instance.GetCurrentBeat();

        if (currentBeat != lastBeat)
        {
            lastBeat = currentBeat;
            return true;
        }

        return false;
    }
}