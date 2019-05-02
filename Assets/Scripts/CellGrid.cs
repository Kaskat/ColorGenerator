using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CellGrid : MonoBehaviour {

    private Color[] _activeColor;
    private int minGroup, maxGroup;

    public GameObject _line;
    public Cell _cellPrefab;

    public int initHeight = 10;
    public int oldHeight;
    private int countFix = 0;

    private int _maxLineSize = 11;

    private int _mapHeight;

    int groupSize;
    int groupColorIndex;

    public int col = 0, row = 0;

    List<Cell> cellsInGroup;

    Cell[][] _cells;

    static List<GameObject> _lineView = new List<GameObject>();

    List<Cell> conflictCells = new List<Cell>();

    List<List<Cell>> listAvailable;

    public int MapHeight{
            get{ 
                return _mapHeight;
            }
            set {
   
                if(_mapHeight == value)
                {
                    return;
                }
                else if (value == 0)
                {
                    _mapHeight = initHeight;
                }
                else
                {
                    _mapHeight = value;
                }
                UpdateGrid();
            }
        }
    
    public Color[] ActiveColor
    {
        get
        {
            return _activeColor;
        }
        set
        {
            _activeColor = value;
        }
    }

    public int MaxGroup
    {
        get
        {
            return maxGroup;
        }
        set
        {
            maxGroup = value;
        }
    }

    public int MinGroup
    {
        get
        {
            return minGroup;
        }
        set
        {
            minGroup = value;
        }
    }

    void SetRandomColors()
    {
        for (int j = 0; j < MapHeight; j++)
        {
            for (int i = 0; i < _cells[j].Length; i++)
            {
                int randomColor = Random.Range(0, _activeColor.Length);
                _cells[j][i].Color = _activeColor[randomColor];
            }
        }
    }

    void UpdateGrid()
    {
        if (_cells == null) // Create start map
        {
            _cells = new Cell[_mapHeight][];

            for (int i = 0; i < _mapHeight; i++)
            {
                _cells[i] = CreateLine(i);
            }
            oldHeight = _mapHeight;
            NeighbourConnection();
        }
        else
        {
            if (oldHeight != MapHeight) // update if the height has changed
            {
                Cell[][] newCells = new Cell[MapHeight][];

                if (MapHeight - oldHeight > 0)
                {
                    for (int i = 0; i < _cells.Length; i++)
                    {
                        newCells[i] = _cells[i];
                    }

                    for (int i = 0; i < MapHeight - oldHeight; i++)
                    {
                        newCells[oldHeight + i] = CreateLine(oldHeight + i);
                    }
                }
                else //Delete extra lines
                {
                    for (int i = 0; i < MapHeight; i++)
                    {
                        newCells[i] = _cells[i];
                    }

                    for (int i = 0; i < oldHeight - MapHeight; i++)
                    {
                        DeleteLine(oldHeight - i);
                    }
                }

                oldHeight = MapHeight;
                _cells = newCells;
                NeighbourConnection();
            }
        }
    }

    private void NeighbourConnection()
    {
        for (int i = 0; i < MapHeight; i++)
        {
            for (int j = 0; j < _cells[i].Length; j++)
            {
                if (j > 0)
                {
                    _cells[i][j].SetNeighbor(CellDirection.W, _cells[i][j - 1]);
                }

                if (i > 0)
                {
                    if ((i & 1) == 1)
                    {
                        _cells[i][j].SetNeighbor(CellDirection.NW, _cells[i - 1][j]);
                        _cells[i][j].SetNeighbor(CellDirection.NE, _cells[i - 1][j + 1]);
                    }

                    if ((i & 1) == 0)
                    {
                        if (j > 0)
                        {
                            _cells[i][j].SetNeighbor(CellDirection.NW, _cells[i - 1][j - 1]);
                        }

                        if (j != _maxLineSize - 1)
                        {
                            _cells[i][j].SetNeighbor(CellDirection.NE, _cells[i - 1][j]);
                        }
                    }
                }
            }
        }
    }
    

    private void ColorizeMap()
    {
        for (row = 0; row < MapHeight; row++)
        {
            for (col = 0; col < _cells[row].Length; col++)
            {
                if (!_cells[row][col].isFormed)
                {
                    GroupBuilder(_cells[row][col]);
                }
            }

            if (countFix > 100) {
                Debug.LogError("LOOP SUPER RADICAL METHOD");
                RadicalSolve(true);
            } 

            if(conflictCells.Count > 0)
            {
                RadicalSolve();
            }
        }

        Debug.Log("Countfix : " + countFix);
        row = col = 0;
    }

    private void Update()
    {

        if (Input.GetMouseButtonDown(0))
        { 
            if (col == _cells[row].Length)
            {
                col = 0;

                if (row < MapHeight)
                {
                    row++;
                }
                else
                {
                    return;
                }
            }

            var cell = _cells[row][col++];

            if (cell.isFormed)
            {
                return;
            }
            GroupBuilder(cell);
        }
        }

    private void Refresh()
    {
        conflictCells = new List<Cell>();
        col = row = 0;
        countFix = 0;
        for (int i = 0; i < MapHeight; i++)
        {
            for (int j = 0; j < _cells[i].Length; j++)
            {
                _cells[i][j].colorIndex = -1;
                _cells[i][j].Color = new Color(0.6415094f, 0.6415094f, 0.6415094f);
                _cells[i][j].label.text = "";
            }
        }
    }
 
    private void RadicalSolve(bool rows = false)
    {
        if (rows)
        {
            if (row >= MaxGroup)
            {
                for (int i = row-MaxGroup; i <= row; i++)
                {
                    for (int j = 0; j < _cells[i].Length; j++)
                    {
                        var cell = _cells[i][j];
                        if (cell.isFormed)
                        {
                            cell.Destroy();
                        }
                    }
                }
            }
            else
            {
                Refresh();
                row = -1;
                return;
            }
        }

        row = -1;
        countFix++;
        for (int d = 0; d < conflictCells.Count; d++)
        {
            var neighbor = conflictCells[d].RealNeighbor();

            for (int i = 0; i < neighbor.Count; i++)
            {
                neighbor[i].Destroy();
            }
        }

        conflictCells = new List<Cell>();
    }
    
    void GroupBuilder(Cell cell)
    {
        groupColorIndex = -1;
        groupSize = 0;

        var availableColor = ColorCompability(cell);

        listAvailable = new List<List<Cell>>();
        cellsInGroup = new List<Cell>();

        if (availableColor.Length > 0)
        {
            int randomColor = availableColor.Length > 2 ? availableColor[Random.Range(0, availableColor.Length)] :
                              availableColor.Length != 1 ? (Random.value > 0.5f ? availableColor[1] : availableColor[0]) : availableColor[0];

            cell.colorIndex = randomColor;
            cell.Color = _activeColor[randomColor];
            groupColorIndex = randomColor;
            groupSize++;
            cell.label.text = groupSize.ToString();
            cellsInGroup.Add(cell);
        }
        else
        {
            cell.colorIndex = -2;
            cell.Color = Color.black;
            conflictCells.Add(cell);
            return;
        }

        if(groupSize == MaxGroup)
        {
            return;
        }

        GrowBody(cell);
    }  

    private void GrowBody(Cell cell)
    {
        var availableCells = cell.EvalutuionOfFreeSpace();
        
        if (availableCells.Count != 0)
        {
            var priorityCells = CreatePriority(cell, availableCells);

            if (priorityCells.Count == 0) {
                Debug.Log("No Prior way");
                return;
            }


            var newCell = ChooseWay(priorityCells);

            if (newCell == null) { Debug.Log("No Prior way"); return; }

            priorityCells.RemoveAt(0);
            listAvailable.Add(priorityCells);


            newCell.colorIndex = groupColorIndex;
            newCell.Color = _activeColor[groupColorIndex];
            cellsInGroup.Add(newCell);
            groupSize++;
            newCell.label.text = groupSize.ToString();

            if (groupSize < MaxGroup)
            {
                GrowBody(newCell);
            }
            else
            {
                return;
            }
        }
        else
        {
            if(groupSize < minGroup)
            {
                Debug.Log("minimum not reach");
            }
            else
            {
                Debug.Log("Available groupsize");
            }
            return;
        }
    }

    private List<Cell> CreatePriority(Cell cell, List<Cell> availableCells)
    {
        System.Random random = new System.Random();
        List<Cell> priorityCells = new List<Cell>();

        availableCells = NeighborColorCheckerCf2(availableCells);

        if(availableCells.Count == 0)
        {
            return new List<Cell>();
        }

        var conflict1 = NeighborColorCheckerCf1(availableCells);

        if(conflict1 != null)
        {
           priorityCells.Add(conflict1);
        }

        for (int i = availableCells.Count-1; i >= 1; i--)
        {
            int j = random.Next(i + 1);

            var temp = availableCells[j];
            availableCells[j] = availableCells[i];
            availableCells[i] = temp;
        }

        for (int i = 0; i < availableCells.Count; i++)
        {
            priorityCells.Add(availableCells[i]);
        }
        
        return priorityCells;
    }

    private Cell NeighborColorCheckerCf1(List<Cell> availableCells)
    {
        for (int i = 0; i < availableCells.Count; i++)
        {
            var availableColor = ColorCompability(availableCells[i]);

            if(availableColor.Length == 0)
            {
                Debug.LogWarning("Conflict 1");
                return availableCells[i];
            }
        }

        return null;
    }

    private List<Cell> NeighborColorCheckerCf2(List<Cell> availableCells)
    {
        var filtrWay = new List<Cell>();

        for (int i = 0; i < availableCells.Count; i++)
        {
            var availableColor = availableCells[i].ColorNeighbor(false);

            var neighbor = availableCells[i].RealNeighbor();

            for (int k = 0; k < neighbor.Count; k++)
            {
                for (int z = 0; z < cellsInGroup.Count; z++)
                {
                    if(neighbor[k] == cellsInGroup[z])
                    {
                        availableColor.Remove(groupColorIndex);
                    }
                }
            }

            for (int d = 0; d < availableColor.Count; d++)
            {
                if (availableColor[d] == groupColorIndex)
                {
                    Debug.LogWarning("Conflict 2");
                    filtrWay.Add(availableCells[i]);
                }
            }
        }

        for (int i = 0; i < filtrWay.Count; i++)
        {
            availableCells.Remove(filtrWay[i]);
        }

        return availableCells;
    }

    private Cell ChooseWay(List<Cell> prioritycells)
    {
        if (prioritycells.Count == 0) return null;

        var filtrWay = new List<Cell>();

        for (int i = 0; i < prioritycells.Count; i++)
        {
            var availableCells = prioritycells[i].EvalutuionOfFreeSpace();

            for (int d = 0; d < availableCells.Count; d++)
            {
                var availableColor = ColorCompability(availableCells[d], groupColorIndex);

                if (availableColor.Length == 0)
                {
                    availableCells[d].Color = Color.magenta;

                    if(groupSize+1 < maxGroup)
                    {
                        for (int z = 0; z < availableColor.Length; z++)
                        {
                            if(availableColor[z] == groupColorIndex)
                            {
                                prioritycells[i].Color = Color.yellow;
                                Debug.Log("FILT CELL " + prioritycells[i]);
                                if (filtrWay.Count == 0)
                                {
                                    filtrWay.Add(prioritycells[i]);
                                }
                                else
                                {
                                    for (int u = 0; u < filtrWay.Count; u++)
                                    {
                                        if (filtrWay[u] != prioritycells[i])
                                        {
                                            filtrWay[u] = prioritycells[i];
                                        }
                                    }
                                }
                                break;
                            }
                        }


                        availableCells[d].Color = Color.yellow;

                        for (int k = 0; k < filtrWay.Count; k++)
                        {
                            Debug.Log("Rm fltr " + k);
                            prioritycells.Remove(filtrWay[k]);
                        }

                        if (prioritycells.Count == 0) return null;

                        return prioritycells[0];
                    }
                    else
                    {
                        if(filtrWay.Count == 0) {
                            filtrWay.Add(prioritycells[i]);
                        }
                        else { 
                            for (int u = 0; u < filtrWay.Count; u++)
                            {
                                if(filtrWay[u] != prioritycells[i])
                                {
                                    filtrWay[u] = prioritycells[i];
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }

        for (int i = 0; i < filtrWay.Count; i++)
        {
            prioritycells.Remove(filtrWay[i]);
        }

        if (prioritycells.Count == 0) return null;

        if(filtrWay.Count == prioritycells.Count) {
            return null;
        }

        for (int i = 1; i < prioritycells.Count; i++)
        {
            var emptyCells = prioritycells[i].EvalutuionOfFreeSpace();

            emptyCells.Remove(prioritycells[0]);

            if(emptyCells.Count == 0)
            {
                Debug.LogWarning("Roque priority");

                var buff = prioritycells[i];
                prioritycells[i] = prioritycells[0];
                prioritycells[0] = buff;
            }
        }

        return prioritycells[0];
    }

    Cell ConflictAround(Cell cell)
    {
        var availableCell = cell.EvalutuionOfFreeSpace();

        if (availableCell.Count == 0) return null;

        for (int i = 0; i < availableCell.Count; i++)
        {
            var test = ColorCompability(availableCell[i], groupColorIndex).Length;
            if (test == 0)
            {
                Debug.LogWarning("Conflict Around");
                availableCell[i].Color = Color.cyan;
                return availableCell[i];
            }
        }

        return null;
    }

    private int[] ColorCompability(Cell cell)
    {
        var availableColor = new List<int>();

        var neighborColors = cell.ColorNeighbor();

        var activeColor = new int[ActiveColor.Length];

        for (int i = 0; i < ActiveColor.Length; i++)
        {
            activeColor[i] = i;
        }

        for (int i = 0; i < ActiveColor.Length; i++)
        {
            availableColor.Add(activeColor[i]);
            for (int j = 0; j < neighborColors.Count; j++)
            {
                if(activeColor[i] == neighborColors[j])
                {
                    availableColor.Remove(i);
                    break;
                }
            }
        }

        return availableColor.ToArray<int>();
    }

    private int[] ColorCompability(Cell cell, int index)
    {
        var availableColor = new List<int>();

        var neighborColors = cell.ColorNeighbor(cell);

        neighborColors.Add(index);


        var activeColor = new int[ActiveColor.Length];

        for (int i = 0; i < ActiveColor.Length; i++)
        {
            activeColor[i] = i;
        }

        for (int i = 0; i < ActiveColor.Length; i++)
        {
            availableColor.Add(activeColor[i]);
            for (int j = 0; j < neighborColors.Count; j++)
            {
                if (activeColor[i] == neighborColors[j])
                {
                    availableColor.Remove(i);
                    break;
                }
            }
        }

        return availableColor.ToArray<int>();
    }

    private Cell[] CreateLine(int index)
    {
        Cell[] cells = new Cell[ (index & 1) == 0 ? _maxLineSize : _maxLineSize-1];

        var lineView = Instantiate(_line, _line.transform.parent, false);

        lineView.name = "Line " + index;
        lineView.SetActive(true);

        _lineView.Add(lineView);

        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = Instantiate<Cell>(_cellPrefab, lineView.transform, false);
            cells[i].name = "Cell " + i;
        }

        return cells;
    }

    private void DeleteLine(int index)
    {
        Destroy(_lineView[index-1]);
        _lineView.RemoveAt(index-1);
    }
}
