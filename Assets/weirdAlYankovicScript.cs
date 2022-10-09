using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;

public class weirdAlYankovicScript : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;
    public KMSelectable[] Buttons;
    public ContainedLyric[] buttonLyrics;
    public Material[] vinylMats;
    public AudioClip[] songs;

    private int songIndex = 0;
    public string[] songOptions;
    public string songName = "";
    public string[] artistOptions;
    public string artistName;

    public string[] potentialLyrics;
    public string[] chosenLyrics;

    public TextMesh[] lyricsText;
    public int[] lyricIndices = new int[3];
    private List<int> chosenIndices = new List<int>();

    private int stage = 0;
    private bool incorrect;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach (KMSelectable button in Buttons)
        {
            KMSelectable pressedButton = button;
            button.OnInteract += delegate () { ButtonPress(pressedButton); return false; };
        }
    }

    void Start()
    {
        Generate();
    }

    void Generate()
    {
        
        for (int i = 0; i <= 2; i++)
        {
            Buttons[i].GetComponent<Renderer>().material = vinylMats[0];
            buttonLyrics[i].pressed = false;
        }
        PickSong();
        PickLyrics();
    }

    void PickSong()
    {
        songIndex = UnityEngine.Random.Range(0, 20);
        songName = songOptions[songIndex];
        artistName = artistOptions[songIndex];
        Debug.LogFormat("[Weird Al Yankovic #{0}] Your chosen song is {1} by Weird Al Yankovic.", moduleId, songName);
    }

    void PickLyrics()
    {
        for (int i = 0; i <= 2; i++)
        {
            int index = UnityEngine.Random.Range(0, 6);
            while (chosenIndices.Contains(index))
            {
                index = UnityEngine.Random.Range(0, 6);
            }
            chosenIndices.Add(index);

            chosenLyrics[i] = potentialLyrics[index + (songIndex * 6)];
            lyricsText[i].text = chosenLyrics[i];
            lyricIndices[i] = index;
            buttonLyrics[i].containedLyric = chosenLyrics[i];
        }
        chosenIndices.Clear();
        Debug.LogFormat("[Weird Al Yankovic #{0}] Your three chosen lyrics are {1}, {2}, & {3}.", moduleId, chosenLyrics[0], chosenLyrics[1], chosenLyrics[2]);
        Array.Sort(lyricIndices, chosenLyrics);
        Debug.LogFormat("[Weird Al Yankovic #{0}] Press {1}, then {2}, then {3}.", moduleId, chosenLyrics[0], chosenLyrics[1], chosenLyrics[2]);
    }

    void ButtonPress(KMSelectable pressedButton)
    {
        if (moduleSolved || pressedButton.GetComponent<ContainedLyric>().pressed)
        {
            return;
        }
        pressedButton.AddInteractionPunch();
        pressedButton.GetComponent<ContainedLyric>().pressed = true;
        pressedButton.GetComponent<Renderer>().material = vinylMats[1];
        if (pressedButton.GetComponent<ContainedLyric>().containedLyric == chosenLyrics[stage])
        {
            stage++;
            Debug.LogFormat("[Weird Al Yankovic #{0}] You pressed {1}. That is correct.", moduleId, pressedButton.GetComponent<ContainedLyric>().containedLyric);
        }
        else
        {
            Debug.LogFormat("[Weird Al Yankovic #{0}] You pressed {1}. That is incorrect.", moduleId, pressedButton.GetComponent<ContainedLyric>().containedLyric);
            stage++;
            incorrect = true;
        }
        if (stage == 3)
        {
            if (incorrect)
            {
                GetComponent<KMBombModule>().HandleStrike();
                Debug.LogFormat("[Weird Al Yankovic #{0}] Strike! The order was not correct.", moduleId);
                stage = 0;
                incorrect = false;
                Generate();
            }
            else
            {
                moduleSolved = true;
                GetComponent<KMBombModule>().HandlePass();
                Debug.LogFormat("[Weird Al Yankovic #{0}] Module disarmed. You are white and nerdy enough.", moduleId);
                Audio.PlaySoundAtTransform("winScratch", transform);
                Audio.PlaySoundAtTransform("oldRecord", transform);
                Audio.PlaySoundAtTransform(songs[songIndex].name, transform);
            }
        }
        else
        {
            Audio.PlaySoundAtTransform("scratch", transform);
        }
    }

    //twitch plays
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} press 1 2 3 [Press buttons 1, 2, 3 (from top to bottom)]. | 'press' is optional.";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        var m = Regex.Match(command, @"^\s*(?:press\s+|submit\s+)?([123 ]+)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!m.Success)
            yield break;
        var btns = m.Groups[1].Value.Where(ch => "123".Contains(ch)).Select(ch => Buttons["123".IndexOf(ch)]).ToArray();
        if (btns.Length != 3 || btns.Distinct().Count() != 3)
            yield break;
        yield return null;
        yield return btns;
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        if (incorrect)
        {
            moduleSolved = true;
            GetComponent<KMBombModule>().HandlePass();
            // due to 2019 blan code, exish wins this time -Quinn Wuest
        }
        var solution = Buttons.Select(i => Array.IndexOf(chosenLyrics, i.GetComponent<ContainedLyric>().containedLyric)).ToArray();
        for (int i = 0; i < 3; i++)
        {
            Buttons[solution[i]].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        yield break;
    }
}