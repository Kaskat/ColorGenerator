using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;


public class Cell : MonoBehaviour {

    private Image image;

    private int inGroup;

    public Text label;

    [SerializeField]
    private int _colorIndex = -1;

    public int colorIndex
    {
        get
        {
            return _colorIndex;
        }

        set
        {
            _colorIndex = value;
        }
    }

    public bool isFormed {
        get
        {
            return colorIndex >= 0;
        }
    }

    [SerializeField]
    private Cell[] _neighbors;

    public Cell[] Neighbors{
        get{
            return _neighbors;
        }
    }

    public Color Color
    {
        get
        {
            return image.color;
        }

        set
        {
            image.color = value;
        }
    }

    private void OnEnable()
    {
        image = GetComponent<Image>();
    }

    public Cell GetNeighbor(CellDirection direction){
        return _neighbors[(int)direction];
    }

    public void SetNeighbor (CellDirection direction, Cell cell)
    {
        _neighbors[(int)direction] = cell;
        cell._neighbors[(int)direction.Opposite()] = this;
    }


    public List<int> ColorNeighbor(Cell parentcell)
    {
        var neighborColors = new List<int>();
        for (int i = 0; i < Neighbors.Length; i++)
        {
            if (Neighbors[i] != null)
                neighborColors.Add(Neighbors[i].colorIndex);
        }

        neighborColors.Remove(parentcell.colorIndex);
        var distinctColor = neighborColors.Distinct().ToList();
        distinctColor.Remove(-1);

        return distinctColor;
    }

    public List<int> ColorNeighbor(bool distinct = true)
    {
        var neighborColors = new List<int>();
        for (int i = 0; i < Neighbors.Length; i++)
        {
            if (Neighbors[i] != null)
                neighborColors.Add(Neighbors[i].colorIndex);
        }

        var allColor = distinct ? neighborColors.Distinct().ToList() : neighborColors;
        allColor.Remove(-1);

        return allColor;
    }

    public List<Cell> EvalutuionOfFreeSpace()
    {
        var emptyNeighbors = new List<Cell>();

        for (int i = 0; i < Neighbors.Length; i++)
        {
            if (Neighbors[i] != null) 
            {
                if (!Neighbors[i].isFormed)
                {
                    emptyNeighbors.Add(Neighbors[i]);
                }
            }
        }
        return emptyNeighbors;
    }

    public List<Cell> RealNeighbor()
    {
        var realNeighbor = new List<Cell>();
        for (int i = 0; i < Neighbors.Count(); i++)
        {
            if(Neighbors[i] != null)
            {
                if (Neighbors[i].isFormed)
                {
                    realNeighbor.Add(Neighbors[i]);
                }
            }
        }
        return realNeighbor;
    }

    public bool NeigbhborAround(List<Cell> cells)
    {
        var realNeighbor = Neighbors.ToList();
        for (int i = 0; i < cells.Count(); i++)
        {
            realNeighbor.Remove(cells[i]);
        }

        for (int i = 0; i < realNeighbor.Count(); i++)
        {
            if (realNeighbor[i] != null)
            {
                if (realNeighbor[i].isFormed)
                {
                    return true;
                }
            }
        }
        return false;
    }


    public void Destroy()
    {
        var neighbors = RealNeighbor();
        int buffColor = colorIndex;
        colorIndex = -1;
        Color = new Color(0.6415094f, 0.6415094f, 0.6415094f);
        label.text = "";

        for (int i = 0; i < neighbors.Count; i++)
        {
            if(neighbors[i].colorIndex == buffColor)
            {
                neighbors[i].Destroy();
                break;
            }
        }
    }
}
