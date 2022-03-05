using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;
using KModkit;
using System.Text.RegularExpressions;

public class MovelessMazeScript : MonoBehaviour
{

    public KMAudio Audio;
    public GameObject StatusLight;
    public GameObject RefMazeTile;
    public List<KMSelectable> Buttons;
    public KMBombModule Module;

    private List<GemTile> _buttonTiles = new List<GemTile>();
    private GemTile _statusLight;
    private MazeGrid _maze;

    private Queue<SubmitEntry> _submitEntries;
    private SubmitEntry _currentEntry;

    private bool[] _isHighlighted = new bool[2];

    private bool _solved;
    private bool _animating;

    private static int _sfx;

    static int _moduleIdCounter = 1;
    int _moduleID = 0;

    void Awake()
    {
        _moduleID = _moduleIdCounter++;
        _sfx = 1;
        for (int i = 0; i < Buttons.Count; i++)
        {
            int x = i;
            Buttons[i].OnInteract += delegate { PressButton(x); return false; };
            Buttons[i].OnHighlight += delegate { HighlightButton(x, true); return; };
            Buttons[i].OnHighlightEnded += delegate { HighlightButton(x, false); return; };
        }
    }

    private void PressButton(int pos)
    {
        if (_solved || _animating)
            return;

        Buttons[pos].AddInteractionPunch();
        switch (pos)
        {
            case 0:
                _maze.Toggle();
                foreach (var gem in _buttonTiles)
                    gem.Toggle();

                PlayPress();
                StartCoroutine(Animate());
                break;
            case 1:
                bool good = _buttonTiles[1].State() == _currentEntry.Get();
                if (good)
                {
                    Log("You submitted {0}. Expected was {1}. This is correct.", _buttonTiles[1].State(), _currentEntry.Get());
                }
                else
                {
                    Log("You submitted {0}. Expected was {1}. This is incorrect.", _buttonTiles[1].State(), _currentEntry.Get());
                    Module.HandleStrike();
                }
                if (PullEntry())
                {
                    PlayPress();

                    StartCoroutine(Continue(good ? 1 : 0));
                }
                else
                {
                    Audio.PlaySoundAtTransform("solve", Module.transform);

                    StartCoroutine(Solve());
                }
                break;
        }
    }

    private void HighlightButton(int pos, bool active)
    {
        if (!_solved)
            _isHighlighted[pos] = active;
    }

    private IEnumerator HighlightAnim()
    {
        float[] transitionState = new float[2];
        MeshRenderer[] meshes = new MeshRenderer[2];
        for (int i = 0; i < Buttons.Count; i++)
            meshes[i] = Buttons[i].GetComponent<MeshRenderer>();

        while (!_solved || transitionState.Sum() != 0)
        {
            for (int i = 0; i < Buttons.Count; i++)
            {
                int mult = _isHighlighted[i] ? 1 : -1;
                transitionState[i] = Mathf.Clamp(transitionState[i] + Time.deltaTime * mult, 0f, 1f);
                float value = transitionState[i] * 0.75f + 0.125f;
                meshes[i].material.color = new Color(value, value, value);
            }
            yield return null;
        }
    }

    private IEnumerator Animate()
    {
        _animating = true;

        for (float t = 0; t < 1; t += Time.deltaTime)
        {
            _maze.Update(t);
            foreach (GemTile gem in _buttonTiles)
                gem.Update(t);
            yield return null;
        }

        _maze.Update(1);
        foreach (GemTile gem in _buttonTiles)
            gem.Update(1);

        _animating = false;
    }

    private IEnumerator Solve()
    {
        _animating = true;

        _maze.GetColorSet().Empty();
        _maze.Empty();
        _buttonTiles[0].Set(0, 0);
        if (_buttonTiles[1].State() == 1)
            _buttonTiles[1].Toggle();
        _statusLight.Set(0, 2);

        _statusLight.Toggle();
        for (float t = 0; t < 1; t += Time.deltaTime)
        {
            _statusLight.Update(t);
            _maze.Update(t);
            foreach (GemTile gem in _buttonTiles)
                gem.Update(t);
            yield return null;
        }

        _statusLight.Update(1);
        _maze.Update(1);
        foreach (GemTile gem in _buttonTiles)
            gem.Update(1);

        _animating = false;

        Log("No more inputs to request. The module is solved.", _buttonTiles[1].State(), _currentEntry.Get());
        Module.HandlePass();
        _solved = true;
        _isHighlighted = new bool[2];
    }

    private IEnumerator Continue(int isCorrect)
    {
        _animating = true;

        _statusLight.Set(0, isCorrect);
        _statusLight.Toggle();
        for (float t = 0; t < 1; t += Time.deltaTime * 2)
        {
            _buttonTiles[0].Update(t / 2);
            _statusLight.Update(t);
            yield return null;
        }
        _statusLight.Update(1);
        _statusLight.Toggle();
        for (float t = 0; t < 1; t += Time.deltaTime * 2)
        {
            _buttonTiles[0].Update((t + 1) / 2);
            _statusLight.Update(t);
            yield return null;
        }
        _buttonTiles[0].Update(1);
        _statusLight.Update(1);


        _animating = false;
    }

    private bool PullEntry()
    {
        if (_submitEntries.Count != 0)
        {
            _currentEntry = _submitEntries.Dequeue();
            _currentEntry.SetTile(_buttonTiles[0]);

            Log("Requesting input for {0}-{1}. Expecting {2}.", _currentEntry.Get(0) + 1, _currentEntry.Get(1) + 1, _currentEntry.Get());
            return true;
        }
        return false;
    }

    void Start()
    {
        _solved = false;

        BuildGrid();

        Log("The chosen colour sets are: {0}.", _maze.GetColorSet().ToString());

        Log("The grids are: {0}.", _maze);

        Log("The calculated matrix is: {0}.", _maze.GetMatrix());

        _submitEntries = new Queue<SubmitEntry>(_maze.GetMatrix().GetEntries().Shuffle());

        _buttonTiles.Add(new GemTile(Buttons[0].GetComponentsInChildren<MeshRenderer>()[1], _maze.GetColorSet()));
        PullEntry();

        _buttonTiles.Add(new GemTile(Buttons[1].GetComponentsInChildren<MeshRenderer>()[1], new ColorManager(ColorManager.Style.Binary)));
        _buttonTiles[1].Set(0, 0);


        _statusLight = new GemTile(StatusLight.GetComponentsInChildren<MeshRenderer>()[1], new ColorManager(ColorManager.Style.StatusLight));
        _statusLight.Set(0, 0);

        StartCoroutine(HighlightAnim());
        StartCoroutine(Animate());
    }

    private void BuildGrid()
    {
        int size = 6;
        float distance = 0.014f;

        _maze = new MazeGrid(size, 4, 4);

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float deltaX = distance * (x - y);
                float deltaZ = distance * (size - 1 - x - y);
                GameObject newTile = Instantiate(RefMazeTile, Module.transform);
                newTile.transform.localPosition += new Vector3(deltaX, 0, deltaZ);
                _maze.SetTile(x, y, newTile.GetComponentsInChildren<MeshRenderer>()[1]);
            }

        RefMazeTile.transform.localScale = new Vector3(0, 0, 0);

        _maze.Initialise();
    }

    private void Log(string message, params object[] args)
    {
        string moduleName = "Matrix Mapping";
        Debug.LogFormat("[" + moduleName + " #" + _moduleID + "] " + message, args);
    }

    private void PlayPress()
    {
        Audio.PlaySoundAtTransform("press_" + _sfx, Module.transform);
        _sfx++;
        if (_sfx > 4)
            _sfx -= 4;
    }

#pragma warning disable 414
    private string TwitchHelpMessage = "'!{0} toggle' to toggle between the two states, '!{0} submit' to submit the current state.";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        yield return null;
        command = command.ToLowerInvariant();
        if (command == "toggle")
            Buttons[0].OnInteract();
        else if (command == "submit")
            Buttons[1].OnInteract();
        else
        {
            yield return "sendtochaterror Invalid command.";
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return true;
        while (!_solved)
            if (_animating)
                yield return true;
            else
                Buttons[_buttonTiles[1].State() == _currentEntry.Get() ? 1 : 0].OnInteract();
    }
}