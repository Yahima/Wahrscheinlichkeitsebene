using System;
using UnityEngine;

public class History
{
    string currentState;
    Tuple<Vector2Int, string> step;

    public History(string currentState, Tuple<Vector2Int, string> step)
    {
        CurrentState = currentState;
        Step = step;
    }

    public string CurrentState { get => currentState; set => currentState = value; }
    public Tuple<Vector2Int, string> Step { get => step; set => step = value; }
}
