using UnityEngine;
using UnityEngine.UI;

public class UISettingsController : MonoBehaviour {

	private int colorCount;

    public InputField inputColorCount;

    [Space]
    public InputField inputMaxGroup;
    public Slider sliderMaxGroup;

    [Space]
    public InputField inputMinGroup;
    public Slider sliderMinGroup;

    [Space]
    public CellGrid _cellGrid;
    public ColorsButtonController _colorsBtnController;


    private void Awake()
    {
        UpdateActiveColors();
        _cellGrid.MapHeight = 6;
        _cellGrid.MaxGroup = 3;
        _cellGrid.MinGroup = 2;
    }

    public void SetMapHeight(Text num)
    {
        _cellGrid.MapHeight = int.Parse(num.text);
    }

    public void SetMinGroup(Text num)
    {
       _cellGrid.MinGroup = int.Parse(num.text);
        sliderMinGroup.value = int.Parse(num.text);
    }

    public void SetMaxGroup(Text num)
    {
        _cellGrid.MaxGroup = int.Parse(num.text);
        sliderMaxGroup.value = int.Parse(num.text);
    }

    public void SetMinGroup(float num)
    {
        _cellGrid.MinGroup = (int)num;
        inputMinGroup.text = num.ToString();
    }

    public void SetMaxGroup(float num)
    {
        _cellGrid.MaxGroup = (int)num;
        inputMaxGroup.text = num.ToString();
    }

    public void UpdateActiveColors()
    {
        var activeColor = _colorsBtnController.TogglesIsOn();
        _cellGrid.ActiveColor = activeColor;
        inputColorCount.text = activeColor.Length.ToString();
    }
}
