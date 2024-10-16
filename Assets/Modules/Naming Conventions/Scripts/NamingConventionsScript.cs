﻿using KeepCoding;
using NamingConventions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

/// <summary>
/// On the Subject of Naming Conventions - A modded "Keep Talking and Nobody Explodes" module created by Emik.
/// </summary>
public class NamingConventionsScript : ModuleScript
{
    public KMSelectable[] Buttons;
    public Renderer[] Texts;
    public Texture[] Textures;

    /// <summary>
    /// Contains the solution for each DataType DataType. Returns default rules if Rule Seed is 1, otherwise returns RuleSeededSolutions property.
    /// </summary>
    internal Dictionary<DataType, bool[]> Solutions
    {
        get
        {
            bool rnd = RuleSeed.Seed != 1;
            return new Dictionary<DataType, bool[]>
            {
                {
                    DataType.Class,
                    rnd ? RuleSeededSolutions[(int)DataType.Class]
                        : new[] { true, false, true, false, true, false }
                },
                {
                    DataType.Constructor,
                    rnd ? RuleSeededSolutions[(int)DataType.Constructor]
                        : new[] { true, false, true, false, true, false }
                },
                {
                    DataType.Method,
                    rnd ? RuleSeededSolutions[(int)DataType.Method]
                        : new[] { true, true,  false, false, true, false }
                },
                {
                    DataType.Argument,
                    rnd ? RuleSeededSolutions[(int)DataType.Argument]
                        : new[] { false, true, false, true, true, false }
                },
                {
                    DataType.Local,
                    rnd ? RuleSeededSolutions[(int)DataType.Local]
                        : new[] { false, true, false, true, true, false }
                },
                {
                    DataType.Constant,
                    rnd ? RuleSeededSolutions[(int)DataType.Constant]
                        : new[] { true, false, false, false, true, false }
                },
                {
                    DataType.Field,
                    rnd ? RuleSeededSolutions[(int)DataType.Field]
                        : new[] { false, true, false, true, true, true }
                },
                {
                    DataType.Property,
                    rnd ? RuleSeededSolutions[(int)DataType.Property]
                        : new[] { true, true, false, true, true, false }
                },
                {
                    DataType.Delegate,
                    rnd ? RuleSeededSolutions[(int)DataType.Delegate]
                        : new[] { true, false, true, true, false, false }
                },
                {
                    DataType.Enum,
                    rnd ? RuleSeededSolutions[(int)DataType.Enum]
                        : new[] { true, true, false, false, false, false }
                },
            };
        }
    }

    /// <summary>
    /// Stores the current state of the buttons.
    /// </summary>
    internal bool[] textStates;

    /// <summary>
    /// The amount of buttons there are.
    /// </summary>
    private const int Length = 7;

    /// <summary>
    /// Whether the module is currently selected or not.
    /// </summary>
    private bool _isSelected;

    /// <summary>
    /// Stores the index of each character of each button, based on the Textures array.
    /// </summary>
    private int[][] _textIndexes = new int[Length][];

    /// <summary>
    /// An index meant for the Constructor DataType, since it's 11 characters long, but the module cannot go higher than 10.
    /// </summary>
    private int Index { get { return _index; } set { if (_index == default(int)) _index = value; } }
    private int _index, _textureOffset;

    /// <summary>
    /// Contains the current DataType.
    /// </summary>
    internal DataType DataType { get { return _dataType; } set { if (_dataType == default(DataType)) _dataType = value; } }
    private DataType _dataType;

    private bool[][] RuleSeededSolutions
    {
        get
        {
            var rule = new MonoRandom(RuleSeed.Seed);
            return Enumerable.Range(0, 10).Select(i => Enumerable.Range(0, 6).Select(j => rule.Next(2) == 0).ToArray()).ToArray();
        }
    }

    public override void OnAwake()
    {
        // Initializes the arrays.
        textStates = Length.RandomBooleans();
        _textIndexes = new int[Length][];
    }

    private void Start()
    {
        // Assigns the KMSelectables.
        Buttons.Assign(onInteract: HandlePresses);
        Get<KMSelectable>().Assign(onInteract: () => _isSelected = true, onDefocus: () => _isSelected = false);

        // Gets random DataType.
        Index = Rnd.Range(0, 10);
        DataType = default(DataType).GetValues().PickRandom();

        // Start rendering.
        UpdateIndexes();
        StartCoroutine(JiggleText());

        Log("The solution for {0} in rule seed {1} is {2}.",
            DataType,
            RuleSeed.Seed,
            Enumerable.Range(0, 6).Select(i => SetTextIndexes(i + 1, Solutions[DataType][i]).Trim()).Join(", "));
    }

    public override void OnTimerTick()
    {
        if (IsSolved)
            return;

        for (int i = 0; i < textStates.Length; i++)
            textStates[i] = !textStates[i];

        if (_isSelected)
            PlaySound(SFX.Nc.Tick);

        UpdateIndexes();
    }

    private IEnumerator JiggleText()
    {
        while (true)
        {
            // Cycles between the 3 fonts, there are 22 letters within this alphabet.
            for (_textureOffset = 0; _textureOffset < 66; _textureOffset += 22)
            {
                UpdateTexts();
                yield return new WaitForSecondsRealtime(0.15f);
            }
        }
    }

    private void HandlePresses(int i)
    {
        ButtonEffect(Buttons[i], 0, SFX.Nc.Touch, KMSoundOverride.SoundEffect.ButtonPress);

        if (IsSolved)
            return;

        // The first button is the submit button.
        if (i == 0)
            Submit();

        // Toggles the state of the button.
        textStates[i] = !textStates[i];

        // Renders the text.
        UpdateIndexes();
    }

    private void Submit()
    {
        if (IsCorrect())
        {
            PlaySound(SFX.Nc.Solve);
            Solve("The submission was correct, solved!");
        }
        else
        {
            PlaySound(SFX.Nc.Strike);
            Strike("The incorrect option was submitted for button(s) {0}, that's 1 strike please!"
                .Form(Enumerable.Range(2, 6).Where(i => textStates[i - 1] != Solutions[_dataType][i - 2]).Join(", ")));
        }
    }

    private string SetTextIndexes(int i, bool b)
    {
        const string True = "True", False = "False";
        string output;

        // Assigns based on button index and state.
        switch (i)
        {
            case 0:
                output = DataType == DataType.Constructor
                ? DataType.ToString().Remove(Index, 1)
                : DataType.ToString(); break;

            case 1: output = IsSolved ? "Is" : b ? "PascalCase" : "camelCase "; break;
            case 2: output = IsSolved ? "Convention" : b ? True : False; break;
            case 3: output = IsSolved ? "" : b ? True : False; break;
            case 4: output = IsSolved ? "Module" : b ? True : False; break;
            case 5: output = IsSolved ? "Is" : b ? "Alphameric" : "Numeric   "; break;
            case 6: output = IsSolved ? "Solved" : b ? True : False; break;

            default: throw new NotImplementedException("i: " + i);
        }

        // It has to render 10 letters.
        return output.PadRight(10);
    }

    private bool IsCorrect()
    {
        return textStates
            .Skip(1)
            .Select((b, i) => b == Solutions[DataType][i])
            .All(b => b);
    }

    private void UpdateTexts()
    {
        for (int i = 0; i < Texts.Length; i++)
            Texts[i].material.mainTexture = _textIndexes[i / 10][i % 10] == -1
                ? Textures.Last()
                : Textures[_textureOffset + _textIndexes[i / 10][i % 10]];
    }

    private void UpdateIndexes()
    {
        for (int i = 0; i < Length; i++)
            _textIndexes[i] = SetTextIndexes(i, textStates[i])
                .Select(c => new[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'l', 'm', 'n', 'o', 'p', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y' }.IndexOf(c.ToLower()))
                .ToArray();

        UpdateTexts();
    }
}
