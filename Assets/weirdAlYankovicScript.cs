using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class weirdAlYankovicScript : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable[] Buttons;
    public ContainedLyric[] buttonLyrics;
    public Material[] vinylMats;
    public AudioClip[] songs;

    private int songIndex = 0;
    public String[] songOptions;
    public String songName = "";
    public String[] artistOptions;
    public String artistName;

    public String[] potentialLyrics;
    public String[] chosenLyrics;

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
        for(int i = 0; i <= 2; i++)
        {
            Buttons[i].GetComponent<Renderer>().material = vinylMats[0];
            buttonLyrics[i].pressed = false;
        }
        PickSong();
        PickLyrics();
    }

    void PickSong()
    {
        songIndex = UnityEngine.Random.Range(0,20);
        songName = songOptions[songIndex];
        artistName = artistOptions[songIndex];
        Debug.LogFormat("[Weird Al Yankovic #{0}] Your chosen song is {1} by Weird Al Yankovic.", moduleId, songName);
    }

    void PickLyrics()
    {
        for(int i = 0; i <= 2; i++)
        {
            int index = UnityEngine.Random.Range(0,6);
            while(chosenIndices.Contains(index))
            {
                index = UnityEngine.Random.Range(0,6);
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
        if(moduleSolved || pressedButton.GetComponent<ContainedLyric>().pressed)
        {
            return;
        }
        pressedButton.AddInteractionPunch();
        Audio.PlaySoundAtTransform("scratch", transform);
        pressedButton.GetComponent<ContainedLyric>().pressed = true;
        pressedButton.GetComponent<Renderer>().material = vinylMats[1];
        if(pressedButton.GetComponent<ContainedLyric>().containedLyric == chosenLyrics[stage])
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
        if(stage == 3)
        {
            if(incorrect)
            {
                GetComponent<KMBombModule>().HandleStrike();
                Debug.LogFormat("[Weird Al Yankovic #{0}] Strike! The order was not correct.", moduleId);
                stage = 0;
                incorrect = false;
                Start();
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
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Press 3 buttons using !{0} press <buttons>. (1 = top; 2 = middle; 3 = bottom.)";
    #pragma warning restore 414
    public KMSelectable[] ProcessTwitchCommand(string command)
    {
        if (command.Equals("press 123", StringComparison.InvariantCultureIgnoreCase))
        {
            return new KMSelectable[] { Buttons[0], Buttons[1], Buttons[2] };
        }
        else if (command.Equals("press 132", StringComparison.InvariantCultureIgnoreCase))
        {
            return new KMSelectable[] { Buttons[0], Buttons[2], Buttons[1] };
        }
        else if (command.Equals("press 213", StringComparison.InvariantCultureIgnoreCase))
        {
            return new KMSelectable[] { Buttons[1], Buttons[0], Buttons[2] };
        }
        else if (command.Equals("press 231", StringComparison.InvariantCultureIgnoreCase))
        {
            return new KMSelectable[] { Buttons[1], Buttons[2], Buttons[0] };
        }
        else if (command.Equals("press 312", StringComparison.InvariantCultureIgnoreCase))
        {
            return new KMSelectable[] { Buttons[2], Buttons[0], Buttons[1] };
        }
        else if (command.Equals("press 321", StringComparison.InvariantCultureIgnoreCase))
        {
            return new KMSelectable[] { Buttons[2], Buttons[1], Buttons[0] };
        }
        return null;
    }
}
