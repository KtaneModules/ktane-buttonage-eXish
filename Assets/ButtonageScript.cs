using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using System;

public class ButtonageScript : MonoBehaviour {

    public KMAudio audio;
    public KMBombInfo bomb;

    public KMSelectable[] buttons;
    public MeshRenderer[] btnRends;
    public MeshRenderer[] borderRends;
    public TextMesh[] btnTexts;
    public Material[] colorMats;

    private List<int> chineseOrderNums = new List<int>();
    private int[] chosenBtns = new int[64];
    private int[] chosenBorders = new int[64];
    private int specialCt;
    private int pCt;
    private int correctBtn;
    private bool chineseOrder;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        moduleSolved = false;
        foreach (KMSelectable obj in buttons)
        {
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
        }
    }

    void Start ()
    {
        Debug.LogFormat("[Buttonage #{0}] Buttons:", moduleId);
        string[] colorLog = { "K", "W", "B", "G", "O", "I", "R", "Y", "A" };
        int[] borderSet = { 1, 5, 6, 8 };
        string logger = "";
        for (int i = 0; i < 64; i++)
        {
            int textChance = UnityEngine.Random.Range(0, 10);
            switch (textChance)
            {
                case 0:
                    btnTexts[i].text = "M";
                    btnTexts[i].color = new Color(1, 0, 0);
                    btnRends[i].material = colorMats[1];
                    chosenBtns[i] = 1;
                    borderRends[i].material = colorMats[6];
                    chosenBorders[i] = 6;
                    specialCt++;
                    break;
                case 1:
                case 2:
                case 3:
                    btnTexts[i].text = "P";
                    int[] colorSet = { 2, 3, 6, 7 };
                    int choice = UnityEngine.Random.Range(0, colorSet.Length);
                    chosenBtns[i] = colorSet[choice];
                    if (choice == 3)
                        btnTexts[i].color = new Color(1, 0.557f, 0);
                    else
                        btnTexts[i].color = new Color(1, 1, 1);
                    btnRends[i].material = colorMats[colorSet[choice]];
                    int choice2 = UnityEngine.Random.Range(0, borderSet.Length);
                    chosenBorders[i] = borderSet[choice2];
                    borderRends[i].material = colorMats[borderSet[choice2]];
                    pCt++;
                    break;
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    btnTexts[i].text = "";
                    int choice3 = UnityEngine.Random.Range(0, colorMats.Length - 1);
                    chosenBtns[i] = choice3;
                    btnRends[i].material = colorMats[choice3];
                    int choice4 = UnityEngine.Random.Range(0, borderSet.Length);
                    chosenBorders[i] = borderSet[choice4];
                    borderRends[i].material = colorMats[borderSet[choice4]];
                    break;
            }
            logger += colorLog[chosenBtns[i]] + colorLog[chosenBorders[i]] + btnTexts[i].text + " ";
            if (i % 8 == 7)
            {
                Debug.LogFormat("[Buttonage #{0}] {1}", moduleId, logger.Trim());
                logger = "";
            }
        }
        Debug.LogFormat("[Buttonage #{0}] (Button Color, Border Color, Text)", moduleId);
        int curNum = 0;
        for (int i = 0; i < 64; i++)
        {
            if (chosenBtns[i] != 0 && chosenBtns[i] != 1)
                curNum += chosenBtns[i] - 1;
        }
        Debug.LogFormat("[Buttonage #{0}] Sum each button's color value (excluding k&w): {1}", moduleId, curNum);
        for (int i = 0; i < 64; i++)
        {
            if (chosenBtns[i] == 0 || chosenBtns[i] == 1)
                curNum -= chosenBtns[i] + 1;
        }
        Debug.LogFormat("[Buttonage #{0}] Subtract each k&w button's color value: {1}", moduleId, curNum);
        curNum *= specialCt;
        Debug.LogFormat("[Buttonage #{0}] Multiply by number of special buttons: {1}", moduleId, curNum);
        curNum += pCt;
        Debug.LogFormat("[Buttonage #{0}] Add number of buttons with 'P' text: {1}", moduleId, curNum);
        for (int i = 0; i < 64; i++)
        {
            if (chosenBorders[i] == 8)
                curNum--;
            else if (chosenBorders[i] == 5)
                curNum++;
            else if (chosenBorders[i] == 1)
                curNum += 2;
        }
        Debug.LogFormat("[Buttonage #{0}] Apply border color offsets for each button: {1}", moduleId, curNum);
        correctBtn = Mod(curNum, 64);
        if (bomb.GetSerialNumberNumbers().Last() % 2 == 1)
        {
            chineseOrder = true;
            Debug.LogFormat("[Buttonage #{0}] Last digit of serial number is odd", moduleId);
        }
        else
            Debug.LogFormat("[Buttonage #{0}] Last digit of serial number is even", moduleId);
        Debug.LogFormat("[Buttonage #{0}] Correct button is {1} in {2}reading order", moduleId, correctBtn, chineseOrder ? "Chinese " : "");
        if (chineseOrder)
        {
            for (int i = 7; i >= 0; i--)
            {
                for (int j = 0; j < 8; j++)
                    chineseOrderNums.Add(j * 8 + i);
            }
            correctBtn = chineseOrderNums[correctBtn];
        }
    }

    void PressButton(KMSelectable pressed)
    {
        if (moduleSolved != true)
        {
            pressed.AddInteractionPunch();
            audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
            if (chineseOrder)
                Debug.LogFormat("[Buttonage #{0}] Pressed button {1} in Chinese reading order", moduleId, chineseOrderNums.IndexOf(Array.IndexOf(buttons, pressed)));
            else
                Debug.LogFormat("[Buttonage #{0}] Pressed button {1} in reading order", moduleId, Array.IndexOf(buttons, pressed));
            if (Array.IndexOf(buttons, pressed) == correctBtn)
            {
                moduleSolved = true;
                GetComponent<KMBombModule>().HandlePass();
                Debug.LogFormat("[Buttonage #{0}] That button is correct, module solved", moduleId);
                for (int i = 0; i < 64; i++)
                {
                    btnTexts[i].text = "";
                    btnRends[i].material = colorMats[3];
                    borderRends[i].material = colorMats[1];
                }
            }
            else
            {
                GetComponent<KMBombModule>().HandleStrike();
                Debug.LogFormat("[Buttonage #{0}] That button is incorrect, strike", moduleId);
            }
        }
    }

    int Mod(int x, int m)
    {
        int r = x % m;
        return r < 0 ? r + m : r;
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press <1-64> [Presses the specified button in reading order]";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length > 2)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 2)
            {
                int temp = -1;
                if (!int.TryParse(parameters[1], out temp))
                {
                    yield return "sendtochaterror!f The specified button '" + parameters[1] + "' is invalid!";
                    yield break;
                }
                if (temp < 1 || temp > 64)
                {
                    yield return "sendtochaterror The specified button '" + parameters[1] + "' is out of range 1-64!";
                    yield break;
                }
                buttons[temp - 1].OnInteract();
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify a button to press!";
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        buttons[correctBtn].OnInteract();
        yield return new WaitForSeconds(.1f);
    }
}