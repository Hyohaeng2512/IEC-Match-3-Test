using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BottomBoardController : MonoBehaviour
{
    public event Action OnMoveEvent = delegate { };

    public event Action IsBottomBoardFullEvent = delegate { };
    public bool IsBusy { get; private set; }

    private Board m_board;

    private GameManager m_gameManager;

    private Camera m_cam;

    private BottomBoardSettings m_bottomBoardSettings;

    private bool m_gameOver;


    public void Update()
    {
        if (m_gameOver) return;
        if (IsBusy) return;

        if (m_gameManager.CurrentMode != GameManager.eLevelMode.TIMER) return;

        if (Input.GetMouseButtonDown(0))
        {
            HandlePlayerClick();
        }
    }

    public void StartGame(GameManager gameManager, BottomBoardSettings bottomBoardSettings)
    {
        m_gameManager = gameManager;
        m_bottomBoardSettings = bottomBoardSettings;
        m_gameManager.StateChangedAction += OnGameStateChange;
        m_cam = Camera.main;
        m_board = new Board(this.transform, bottomBoardSettings);
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        IsBusy = state == GameManager.eStateGame.PAUSE;
        if (state == GameManager.eStateGame.GAME_OVER) m_gameOver = true;
    }

    internal void Clear() => m_board.Clear();

    public void ReceiveItem(Item item)
    {
        if (item == null) return;

        Cell empty = GetAllCells().FirstOrDefault(c => c.IsEmpty);

        empty.Assign(item);

        item.SetViewRoot(this.transform);

        SortItemsStableByType();

        item.AnimationMoveToPosition(0.3f);

        DOVirtual.DelayedCall(0.3f, () =>
        {
            OnBottomRowUpdate();
        });
    }

    private void OnBottomRowUpdate()
    {
        SortItemsStableByType();
        DOVirtual.DelayedCall(0.3f, () =>
        {
            ClearItemsAndResort();

            if (IsBottomBoardFull())
            {
                IsBottomBoardFullEvent();
            }
        });
    }

    private List<Cell> GetAllCells()
    {

        List<Cell> cells = new List<Cell>();

        for (int x = 0; x < m_bottomBoardSettings.BoardSizeX; x++)
        {
            for (int y = 0; y < m_bottomBoardSettings.BoardSizeY; y++)
            {
               cells.Add(m_board.GetCell(x, y));
            }
        }
        return cells;
    }

    private List<List<Item>> GroupItems()
    {
        var items = GetAllCells().Where(c => !c.IsEmpty).Select(c => c.Item).ToList();
        var groups = new List<List<Item>>();

        foreach (var item in items)
        {
            bool added = false;
            foreach (var group in groups)
            {
                if (group.Count > 0 && group[0].IsSameType(item))
                {
                    group.Add(item);
                    added = true;
                    break;
                }
            }
            if (!added)
            {
                groups.Add(new List<Item> { item });
            }
        }

        return groups;
    }

    private void SortItemsStableByType(float moveDuration = 0.25f)
    {
        var groups = GroupItems();
        if (groups.Sum(g => g.Count) <= 1) return;

        foreach (var c in GetAllCells()) c.Free();

        int idx = 0;
        foreach (var g in groups)
        {
            foreach (var item in g)
            {
                var cell = GetAllCells().ElementAt(idx++);
                cell.Assign(item);
                item.AnimationMoveToPosition(moveDuration);
            }
        }
    }

    private void ClearItemsAndResort(float moveDuration = 0.25f)
    {
        var groups = GroupItems();
        bool cleared = false;

        foreach (var g in groups)
        {
            if (g.Count >= 3)
            {
                cleared = true;
                foreach (var item in g)
                {
                    var cell = GetAllCells().FirstOrDefault(c => c.Item == item);
                    if (cell != null)
                    {
                        cell.ExplodeItem();
                        cell.Free();
                    }
                }
            }
        }

        if (cleared)
        {
            DOVirtual.DelayedCall(0.15f, () =>
            {
                SortItemsStableByType(moveDuration);
                OnMoveEvent?.Invoke();
                CompactItemsToLeft();
            });
        }     
    }

    private void CompactItemsToLeft(float moveDuration = 0.25f)
    {
        var items = GetAllCells()
            .Where(c => !c.IsEmpty)
            .Select(c => c.Item)
            .ToList();

        foreach (var cell in GetAllCells())
            cell.Free();

        int idx = 0;
        foreach (var item in items)
        {
            var cell = GetAllCells().ElementAt(idx++);
            cell.Assign(item);
            item.AnimationMoveToPosition(moveDuration);
        }
    }

    public bool IsBottomBoardFull()
    {
        return !GetAllCells().Any(c => c.IsEmpty);
    }

    private void HandlePlayerClick()
    {
        Vector3 worldPos = m_cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 pos = new Vector2(worldPos.x, worldPos.y);

        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);
        if (hit.collider != null)
        {
            Cell clickedCell = hit.collider.GetComponent<Cell>();
            if (clickedCell != null && !clickedCell.IsEmpty)
            {
                Item item = clickedCell.Item;
                clickedCell.Free();
                ReturnItemToBoard(item);
            }
        }
    }

    public void ReturnItemToBoard(Item item)
    {
        if (item == null) return;

        Cell emptyCell = m_gameManager.GetBoardController().GetEmptyCell();
        if (emptyCell != null)
        {
            item.SetCell(emptyCell);
            emptyCell.Assign(item);
            item.AnimationMoveToPosition();
        }
    }
}
