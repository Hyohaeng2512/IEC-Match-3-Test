using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BottomBoardSettings", menuName = "Game/BottomBoardSettings")]
public class BottomBoardSettings : ScriptableObject
{
    public int BoardSizeX = 5;

    public int BoardSizeY = 1;

    public int MatchesMin = 3;

    public Vector3 offsets = new Vector3();
}
