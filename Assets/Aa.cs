using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public class Aa : MonoBehaviour {

    public KMAudio Audio;
    public KMSelectable[] btn;
    public KMBombInfo Info;
    public TextMesh screen;
    public KMNeedyModule needy;
    public int codeOverride;
    
    private bool _isAwake = false, _isQuery = false, TwitchPlaysActive;
    private int response, current;

    private static int _moduleIdCounter = 1;
    private int _moduleId;

    void Start()
    {
        _moduleId = _moduleIdCounter++;
    }

    private void Awake()
    {
        needy.OnNeedyActivation += OnNeedyActivation;
        needy.OnNeedyDeactivation += OnNeedyDeactivation;
        needy.OnTimerExpired += OnTimerExpired;

        for (int i = 0; i < btn.Length; i++)
        {
            int j = i;
            btn[i].OnInteract += delegate () { HandlePress(j); return false; };
        }
    }

    protected void OnNeedyActivation()
    {
        int code = Random.Range(1, 142);
        if (codeOverride > -1)
            code = codeOverride;
        var text = "";
        var value = (code - 5) / 26;
        if (code < 5)
            text = new[] { "K", "M", "B", "T" }[code - 1];
        else
            text = Format("{0}{1}", 'a' + value, 'a' + code - value * 26 - 5);
        screen.text = text;
        response = code * 3;

        _isAwake = true;
        Log("Selected code = {0}, Expected response = {1}", screen.text, response);
        if (TwitchPlaysActive)
            needy.CountdownTime = 60f;
    }

    protected void OnNeedyDeactivation()
    {
        screen.text = "Waiting...";
        Exitfunc();
    }

    protected void OnTimerExpired()
    {
        GetComponent<KMNeedyModule>().OnStrike();
        Exitfunc();
    }

    void HandlePress(int j)
    {
        btn[j].AddInteractionPunch();
        if (!_isAwake) return;

        current = int.Parse(current + j.ToString());
        if (current == response)
        {
            needy.HandlePass();
            Log("Answer correct");
            Exitfunc();
        }
        else if (current.ToString().Length == response.ToString().Length)
        {
            needy.HandleStrike();
            Log("Pressed {0}, expected {1}", current, response);
        }
        else
            return;
        current = 0;
    }

    void Exitfunc()
    {
        screen.text = "";
        _isAwake = false;
        Log("Module deactivated.");
    }

    string Format(string text, params int[] chars)
    {
        object[] objs = chars.Select(x => (object)(char)x).ToArray();
        return string.Format(text, objs);
    }

    void Log(string log, params object[] obj)
    {
        log = string.Format(log, obj);
        Debug.LogFormat("[aa #{0}] {1}", _moduleId, log);
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Submit a response code with “!{0} submit (code)”. For example, “!{0} submit 100” for code 100.";
#pragma warning restore 414

    KMSelectable[] ProcessTwitchCommand(string command)
    {
        Match match = Regex.Match(command, @"^submit (\d{1,3})", RegexOptions.IgnoreCase);

        if (match.Success)
        {
            command = match.Groups[1].Value;
            KMSelectable[] selectable = new KMSelectable[0];
            foreach (char c in command)
            {
                selectable = selectable.Concat(new[] { btn[int.Parse(c.ToString())] }).ToArray();
            }
            return selectable;
        }

        return null;
    }
}
