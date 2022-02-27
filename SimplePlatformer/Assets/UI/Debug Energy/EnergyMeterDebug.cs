using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character;
using UnityEngine.UI;

public class EnergyMeterDebug : MonoBehaviour
{
    IPlatformer character;
    Image ren;
    public Color[] colors = { Color.green, Color.yellow, Color.red };

    private void Awake()
    {
        ren = GetComponent<Image>();
        character = GetComponentInParent<IPlatformer>();
    }
    private void Update()
    {
        if (character.Energy > 0.5f)
            ren.color = colors[0];
        else if (character.Energy > 0.1f)
            ren.color = colors[1];
        else
            ren.color = colors[2];

        ren.enabled = character.Energy < 1f;
        
        if (character.Energy > 0)
            ren.fillAmount = character.Energy;
        else
            ren.fillAmount = 1f;
    }
}
