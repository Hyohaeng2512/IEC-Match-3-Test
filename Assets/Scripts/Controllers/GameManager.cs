using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Analytics;

public class GameManager : MonoBehaviour
{
    public event Action<eStateGame> StateChangedAction = delegate { };

    public enum eLevelMode
    {
        TIMER,
        MOVES
    }

    public enum eStateGame
    {
        SETUP,
        MAIN_MENU,
        GAME_STARTED,
        PAUSE,
        GAME_OVER,
        LEVEL_WIN
    }

    private eStateGame m_state;
    public eStateGame State
    {
        get { return m_state; }
        private set
        {
            m_state = value;

            StateChangedAction(m_state);
        }
    }


    private GameSettings m_gameSettings;

    private BottomBoardSettings m_bottomBoardSettings;

    private BoardController m_boardController;

    public BoardController GetBoardController() => m_boardController;

    private BottomBoardController m_bottomBoardController;
    public BottomBoardController GetBottomBoardController() => m_bottomBoardController;

    private UIMainManager m_uiMenu;

    private LevelCondition m_levelCondition;

    private eLevelMode m_currentMode;
    public eLevelMode CurrentMode => m_currentMode;

    private void Awake()
    {
        State = eStateGame.SETUP;

        m_gameSettings = Resources.Load<GameSettings>(Constants.GAME_SETTINGS_PATH);
        m_bottomBoardSettings = Resources.Load<BottomBoardSettings>(Constants.BOTTOM_BOARD_SETTING_PATH);


        m_uiMenu = FindObjectOfType<UIMainManager>();
        m_uiMenu.Setup(this);
    }

    void Start()
    {
        State = eStateGame.MAIN_MENU;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_boardController != null) m_boardController.Update();
        if (m_bottomBoardController != null) m_bottomBoardController.Update();
    }


    internal void SetState(eStateGame state)
    {
        State = state;

        if(State == eStateGame.PAUSE)
        {
            DOTween.PauseAll();
        }
        else
        {
            DOTween.PlayAll();
        }
    }

    public void LoadLevel(eLevelMode mode)
    {
        m_boardController = new GameObject("BoardController").AddComponent<BoardController>();
        m_bottomBoardController = new GameObject("BottomBoardController").AddComponent<BottomBoardController>();

        m_boardController.StartGame(this, m_gameSettings);
        m_bottomBoardController.StartGame(this, m_bottomBoardSettings);

        m_currentMode = mode;

        if (mode != eLevelMode.TIMER) 
        {
            m_bottomBoardController.IsBottomBoardFullEvent += GameOver;
        }
        
        m_boardController.IsBoardClearEvent += LevelWin;

        if (mode == eLevelMode.MOVES)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelMoves>();
            m_levelCondition.Setup(m_gameSettings.LevelMoves, m_uiMenu.GetLevelConditionView(), m_boardController);
        }
        else if (mode == eLevelMode.TIMER)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelTime>();
            m_levelCondition.Setup(m_gameSettings.LevelTime, m_uiMenu.GetLevelConditionView(), this);
        }

        m_levelCondition.ConditionCompleteEvent += GameOver;

        

        State = eStateGame.GAME_STARTED;
    }

    public void GameOver()
    {
        StartCoroutine(WaitBoardController(false));
    }

    public void LevelWin()
    {
        StartCoroutine(WaitBoardController(true));
    }

    internal void ClearLevel()
    {
        if (m_boardController)
        {
            m_boardController.Clear();
            Destroy(m_boardController.gameObject);
            m_boardController = null;
        }

        if (m_bottomBoardController)
        {
            m_bottomBoardController.Clear();
            m_bottomBoardController.IsBottomBoardFullEvent -= GameOver;

            Destroy(m_bottomBoardController.gameObject);
            m_bottomBoardController = null;
        }
    }

    private IEnumerator WaitBoardController(bool isWin = false)
    {
        while (m_boardController.IsBusy)
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(0.5f);
        if (!isWin)
        {
            State = eStateGame.GAME_OVER;
        }
        else
        {
            State = eStateGame.LEVEL_WIN;
        }
        if (m_levelCondition != null)
        {
            m_levelCondition.ConditionCompleteEvent -= GameOver;
            m_boardController.IsBoardClearEvent -= LevelWin;
            Destroy(m_levelCondition);
            m_levelCondition = null;
        }
    }

    public void StartAutoplay()
    {
        StartCoroutine(AutoPlayRoutine());
    }

    private IEnumerator AutoPlayRoutine(float duration = 0.5f)
    {
        if (m_boardController == null || m_bottomBoardController == null)
            yield break;

        while (m_boardController != null && !m_boardController.IsEmpty() && State == eStateGame.GAME_STARTED)
        {
            List<Cell> cells = m_boardController.GetAllCells();
            List<Cell> availableCells = cells.FindAll(c => c.Item != null);

            if (availableCells.Count == 0)
                yield break;

            Cell firstCell = availableCells[UnityEngine.Random.Range(0, availableCells.Count)];
            Item targetItem = firstCell.Item;

            firstCell.Free();
            m_bottomBoardController.ReceiveItem(targetItem);

            yield return new WaitForSeconds(duration);

            List<Cell> sameItems = availableCells.
                FindAll(c => c.Item != null && c.Item.IsSameType(targetItem) && c != firstCell);

            int picks = Mathf.Min(2, sameItems.Count);
            for (int i = 0; i < picks; i++)
            {
                Cell cell = sameItems[i];
                Item item = cell.Item;
                cell.Free();
                m_bottomBoardController.ReceiveItem(item);

                yield return new WaitForSeconds(duration);
            }
        }

        if (m_boardController.IsEmpty())
        {
            LevelWin();
        }

    }
    public void StartAutoLose()
    {
        StartCoroutine(AutoLoseRoutine());
    }

    private IEnumerator AutoLoseRoutine(float duration = 0.5f)
    {
        if (m_boardController == null || m_bottomBoardController == null)
            yield break;

        List<Item> pickedItems = new List<Item>();

        while (m_boardController != null && !m_boardController.IsEmpty() && State == GameManager.eStateGame.GAME_STARTED)
        {
            List<Cell> cells = m_boardController.GetAllCells();

            List<Cell> availableCells = cells.
                FindAll(c => c.Item != null && !pickedItems.Exists(i => i.IsSameType(c.Item)));

            if (availableCells.Count == 0)
                yield break;

            int index = UnityEngine.Random.Range(0, availableCells.Count);
            Cell cellToPick = availableCells[index];
            Item targetItem = cellToPick.Item;

            cellToPick.Free();
            m_bottomBoardController.ReceiveItem(targetItem);
            pickedItems.Add(targetItem);

            yield return new WaitForSeconds(duration);
        }

        if (m_bottomBoardController.IsBottomBoardFull())
        {
            GameOver();
        }

    }
}
