using UnityEngine;
using UnityEngine.UI;

public class ColorsButtonController : MonoBehaviour {

    public Toggle[] toggles;
    public Color[] _colors;

    public Color[] TogglesIsOn()
    {
        int count = 0;
        for (int i = 0; i < toggles.Length; i++)
        {
            if (toggles[i].isOn)
            {
                count++;
            }
        }

        Color[] colors = new Color[count];
      
        for (int i = 0, k = 0; i < toggles.Length; i++)
        {
            if (toggles[i].isOn)
            {
                colors[k] = _colors[i];
                k++;
            }
        }
        return colors;
    }
}
