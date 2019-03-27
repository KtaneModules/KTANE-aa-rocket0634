using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Aa : MonoBehaviour {

    public KMAudio Audio;
    public KMSelectable[] btn;
    public KMBombInfo Info;
    public TextMesh screen;

    private readonly string[] texts = {"K","M","B","T","aa","ab","ac","ad","ae","af",
                             "ag","ah","ai","aj","ak","al","am","an","ao","ap",
                             "aq","ar","as","at","au","av","aw","ax","ay","az","ba","bb",
                             "bc","bd","be","bf","bg","bh","bi","bj","bk","bl","bm","bn", "bo","bp","bq","br","bs","bt","bu","bv","bw","bx",
                             "by","bz","ca","cb","cc","cd","ce","cf","cg","ch",
                             "ci","cj","ck","cl","cm","cn","co","cp","cq","cr","cs","ct",
                             "cu","cv","cw","cx","cy","cz","da","db","dc","dd","de","df"};
    private readonly int[] respcode =  {003, 006, 009, 012, 015, 018, 021, 024, 027, 030, 033, 036, 039, 042, 045, 048, 051, 054, 057, 060,
                             063, 066, 069, 072, 075, 078, 081, 084, 087, 090, 093, 096, 099, 102, 105, 108, 111, 114, 117, 120, 123, 126, 129, 132, 135, 138, 141, 144, 147, 150, 153, 156, 159, 162, 165, 168, 171, 174, 177, 180, 183, 186, 189, 192,
                             195, 198, 201, 204, 207, 210, 213, 216, 219, 222, 225, 228, 231, 234, 237, 240, 243, 246, 249, 252, 255, 258, 261, 264, };
    private bool _isAwake = false, _isQuery = false;
    private int response, adder=0;
    private string current = null;

    private static int _moduleIdCounter = 1;
    private int _moduleId;

    void Start()
    {
        _moduleId = _moduleIdCounter++;
    }

    private void Awake()
    {
        GetComponent<KMNeedyModule>().OnNeedyActivation += OnNeedyActivation;
        GetComponent<KMNeedyModule>().OnNeedyDeactivation += OnNeedyDeactivation;
        GetComponent<KMNeedyModule>().OnTimerExpired += OnTimerExpired;

        btn[0].OnInteract += delegate ()
        {
            HandlePress(0);
            return false;
        };
        btn[1].OnInteract += delegate ()
        {
            HandlePress(1);
            return false;
        };
        btn[2].OnInteract += delegate ()
        {
            HandlePress(2);
            return false;
        };
        btn[3].OnInteract += delegate ()
        {
            HandlePress(3);
            return false;
        };
        btn[4].OnInteract += delegate ()
        {
            HandlePress(4);
            return false;
        };
        btn[5].OnInteract += delegate ()
        {
            HandlePress(5);
            return false;
        };
        btn[6].OnInteract += delegate ()
        {
            HandlePress(6);
            return false;
        };
        btn[7].OnInteract += delegate ()
        {
            HandlePress(7);
            return false;
        };
        btn[8].OnInteract += delegate ()
        {
            HandlePress(8);
            return false;
        };
        btn[9].OnInteract += delegate ()
        {
            HandlePress(9);
            return false;
        };
    }

    protected void OnNeedyActivation()
    {
        if(!_isQuery)
        {
            int numbatt = 0, sum = 0;
            string serialno = null;
            foreach (string batteryInfo in Info.QueryWidgets(KMBombInfo.QUERYKEY_GET_BATTERIES, null))
                numbatt += JsonConvert.DeserializeObject<Dictionary<string, int>>(batteryInfo)["numbatteries"];
            foreach (string serialNumberInfo in Info.QueryWidgets(KMBombInfo.QUERYKEY_GET_SERIAL_NUMBER, null))
                serialno = JsonConvert.DeserializeObject<Dictionary<string, string>>(serialNumberInfo)["serial"];
            for(int i=0;i<6;i++)
            {
                if (serialno[i] >= 48 && serialno[i] <= 57) sum += serialno[i] - 48;
            }
            adder = numbatt * sum;
            _isQuery = true;
        }
        int code = Random.Range(0, 88);
        screen.text = texts[code];
        response = respcode[code];
        if (code >= 89)
        {
            response += adder;
        }

        _isAwake = true;
        Debug.LogFormat("[NeedyHTTP #{0}] Selected code = {1}, Expected response = {2}", _moduleId, screen.text, response);
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

        current += j.ToString();
        if(current.Length == 3)
        {
            Debug.LogFormat("[NeedyHTTP #{0}] Entered = {1}, Expected = {2}", _moduleId, current, response.ToString("D3"));
            if (current == response.ToString("D3"))
            {
                GetComponent<KMNeedyModule>().HandlePass();
                Debug.LogFormat("[NeedyHTTP #{0}] Answer correct! Module passed!", _moduleId);
                Exitfunc();
            }

            else
            {
                GetComponent<KMNeedyModule>().HandleStrike();
                Debug.LogFormat("[NeedyHTTP #{0}] Answer incorrect! Strike! If you belive this strike was in error, immediatly contact @ryaninator81 on Discord.", _moduleId);
            }
            current = null;
        }
    }

    void Exitfunc()
    {
        screen.text = "";
        _isAwake = false;
        Debug.LogFormat("[NeedyHTTP #{0}] Module deactivated.",_moduleId);
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Submit a response code with “!{0} resp (code)”. For example, “!{0} resp 100” for code 100.";
#pragma warning restore 414

    KMSelectable[] ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();

        if (Regex.IsMatch(command, @"^submit +\d\d\d$"))
        {
            command = command.Substring(5, 3);
            return new[] { btn[int.Parse(command[0].ToString())], btn[int.Parse(command[1].ToString())], btn[int.Parse(command[2].ToString())] };
        }

        return null;
    }
}
