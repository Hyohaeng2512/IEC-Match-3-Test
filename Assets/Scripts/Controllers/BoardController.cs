using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public event Action OnMoveEvent = delegate { };
    public event Action IsBoardClearEvent = delegate { };
    public bool IsBusy { get; private set; }

    private Board m_board;

    private GameManager m_gameManager;

    private BottomBoardController m_bottomBoardController;

    private bool m_isDragging;

    private Camera m_cam;

    private Collider2D m_hitCollider;

    private GameSettings m_gameSettings;

    private List<Cell> m_potentialMatch;

    private float m_timeAfterFill;

    private bool m_hintIsShown;

    private bool m_gameOver;

    public void StartGame(GameManager gameManager, GameSettings gameSettings)
    {
        m_gameManager = gameManager;

        m_gameSettings = gameSettings;

        m_bottomBoardController = gameManager.GetBottomBoardController();

        m_gameManager.StateChangedAction += OnGameStateChange;

        m_cam = Camera.main;

        m_board = new Board(this.transform, gameSettings);

        Fill();
    }

    private void Fill()
    {
        m_board.Fill();
        //FindMatchesAndCollapse();
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.GAME_STARTED:
                IsBusy = false;
                break;
            case GameManager.eStateGame.PAUSE:
                IsBusy = true;
                break;
            case GameManager.eStateGame.GAME_OVER:
                m_gameOver = true;
                break;
        }
    }


    public void Update()
    {
        if (m_gameOver) return;
        if (IsBusy) return;


        if (Input.GetMouseButtonDown(0))
        {
            HandlePlayerClicked();
            CheckingBoardItem();
        }

    }

    private void CheckingBoardItem()
    {
        if (IsEmpty())
        {
            IsBoardClearEvent?.Invoke();
        }
    }

    public bool IsEmpty()
    {
        foreach (var cell in GetAllCells())
        {
            if (cell.Item != null)
                return false;
        }
        return true;
    }

    private void HandlePlayerClicked()
    {
        Vector3 worldPos = m_cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 pos = new Vector2(worldPos.x, worldPos.y);

        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);
        if (hit.collider != null)
        {
            Cell cell = hit.collider.GetComponent<Cell>();
            if (cell != null && cell.Item != null)
            {
                Item item = cell.Item;

                cell.Free();

                m_bottomBoardController.ReceiveItem(item);
            }
        }
    }

    public List<Cell> GetAllCells()
    {

        List<Cell> cells = new List<Cell>();

        for (int x = 0; x < m_gameSettings.BoardSizeX; x++)
        {
            for (int y = 0; y < m_gameSettings.BoardSizeY; y++)
            {
                cells.Add(m_board.GetCell(x, y));
            }
        }
        return cells;
    }

    internal void Clear()
    {
        m_board.Clear();
    }

}
