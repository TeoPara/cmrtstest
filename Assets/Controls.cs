using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MarineSystem;
using static UnityEditor.PlayerSettings;
using static XenoSystem;

public class Controls : MonoBehaviour
{
    bool HoldingLeft = false;
    bool HoldingRight = false;
    void Update()
    {
        if (OurLz == null)
            return;

        // spawning the mareens
        if (Input.GetKeyDown(KeyCode.I) && RemainingMarinesToSpawn>0)
        {
            if (OurLz.GetComponent<SpriteRenderer>().bounds.Contains((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition)))
            {
                RemainingMarinesToSpawn--;
                AllMarines.Add(new Marine(Instantiate(Resources.Load<GameObject>("Marine"), Tools.Centralize(Camera.main.ScreenToWorldPoint(Input.mousePosition)), Quaternion.identity)));
            }
        }

        // start holding right
        if (Input.GetMouseButtonDown(1))
        {
            HoldingRight = true;
            GameObject.Find("Rect").transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
            GameObject.Find("Rect").transform.Find("Graphic").gameObject.SetActive(true);
        }
        // released right
        if (Input.GetMouseButtonUp(1))
        {
            HoldingRight = false;
            GameObject.Find("Rect").transform.Find("Graphic").gameObject.SetActive(false);

            foreach (Marine m in AllMarines)
            {
                if (GameObject.Find("Rect").transform.Find("Graphic").GetComponent<SpriteRenderer>().bounds.Contains(m.Position))
                {
                    if (current == 1)
                        m.Highlight(true, new Color32(178, 132, 132, 155));
                    else if (current == 2)
                        m.Highlight(true, new Color32(121, 153, 181, 155));
                    else if (current == 3)
                        m.Highlight(true, new Color32(132, 178, 132, 155));
                }
            }
        }
        // holding right
        if (HoldingRight)
        {
            GameObject.Find("Rect").transform.localScale = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2)GameObject.Find("Rect").transform.position;
        }
       
        if (HoldingLeft && current == 3)
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (DestinationsQueueBeingDefined.Count == 0)
            {
                foreach (GameObject o in SpawnedMarkersOfDestinationsQueueBeingDefined)
                    Destroy(o);

                DestinationsQueueBeingDefined.Add(pos);
                SpawnedMarkersOfDestinationsQueueBeingDefined.Add(Instantiate(Resources.Load<GameObject>("marker"), pos, Quaternion.identity));
            }
            else if ((DestinationsQueueBeingDefined.Last() - pos).magnitude > 5)
            {
                GameObject added = Instantiate(Resources.Load<GameObject>("marker"), pos, Quaternion.identity);
                added.GetComponent<LineRenderer>().SetPosition(0, added.transform.position);
                added.GetComponent<LineRenderer>().SetPosition(1, DestinationsQueueBeingDefined.Last());
                SpawnedMarkersOfDestinationsQueueBeingDefined.Add(added);
                DestinationsQueueBeingDefined.Add(pos);
            }
        }
        if (HoldingLeft && Input.GetMouseButtonUp(0))
        {
            foreach (Marine m in HighlightedMarines)
                m.DestinationsQueue = DestinationsQueueBeingDefined.ToList();
            DestinationsQueueBeingDefined = new List<Vector2>();
            foreach (Marine m in AllMarines)
                m.Highlight(false, new Color32());
        }


        if (Input.GetMouseButtonDown(0))
            HoldingLeft = true;
        if (Input.GetMouseButtonUp(0))
            HoldingLeft = false;


        // execute order
        if (current == 2 && Input.GetMouseButtonDown(0))
        {
            if (current == 2)
            {
                Vector2 pos = Tools.Centralize(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                foreach (Marine m in HighlightedMarines)
                    m.PathTo(pos);
            }
            foreach (Marine m in AllMarines)
                m.Highlight(false, new Color32());
        }
    }

    List<GameObject> SpawnedMarkersOfDestinationsQueueBeingDefined = new List<GameObject>();

    IEnumerable<Marine> HighlightedMarines => AllMarines.Where(m => m.Highlighted);

    List<Vector2> DestinationsQueueBeingDefined = new List<Vector2>();

    int current = 0;
    public void FocusClicked()
    {
        GameObject.Find("HUD").transform.Find("Orders").Find("Focus").GetComponent<UnityEngine.UI.Image>().color = new Color32(200, 200, 200, 255);
        GameObject.Find("HUD").transform.Find("Orders").Find("Hold").GetComponent<UnityEngine.UI.Image>().color = new Color32(255, 255, 255, 255);
        GameObject.Find("HUD").transform.Find("Orders").Find("Move").GetComponent<UnityEngine.UI.Image>().color = new Color32(255, 255, 255, 255);
        GameObject.Find("Rect").transform.Find("Graphic").GetComponent<SpriteRenderer>().color = new Color32(178, 132, 132, 155);
        current = 1;
    }
    public void HoldClicked()
    {
        GameObject.Find("HUD").transform.Find("Orders").Find("Focus").GetComponent<UnityEngine.UI.Image>().color = new Color32(255, 255, 255, 255);
        GameObject.Find("HUD").transform.Find("Orders").Find("Hold").GetComponent<UnityEngine.UI.Image>().color = new Color32(200, 200, 200, 255);
        GameObject.Find("HUD").transform.Find("Orders").Find("Move").GetComponent<UnityEngine.UI.Image>().color = new Color32(255, 255, 255, 255);
        GameObject.Find("Rect").transform.Find("Graphic").GetComponent<SpriteRenderer>().color = new Color32(121, 153, 181, 155);
        current = 2;
    }
    public void MoveClicked()
    {
        GameObject.Find("HUD").transform.Find("Orders").Find("Focus").GetComponent<UnityEngine.UI.Image>().color = new Color32(255, 255, 255, 255);
        GameObject.Find("HUD").transform.Find("Orders").Find("Hold").GetComponent<UnityEngine.UI.Image>().color = new Color32(255, 255, 255, 255);
        GameObject.Find("HUD").transform.Find("Orders").Find("Move").GetComponent<UnityEngine.UI.Image>().color = new Color32(200, 200, 200, 255);
        GameObject.Find("Rect").transform.Find("Graphic").GetComponent<SpriteRenderer>().color = new Color32(132, 178, 132, 155);
        current = 3;
    }




    GameObject OurLz;
    float XenosLeftToSpawn = 20;
    int RemainingMarinesToSpawn
    {
        get { return _remainingMarinesToSpawn; }
        set
        {
            GameObject.Find("HUD").transform.Find("YouHave---RemainingMarines").GetComponent<TMPro.TMP_Text>().text = "You have " + value + " remaining marines to spawn";
            _remainingMarinesToSpawn = value;
        }
    }int _remainingMarinesToSpawn = 25;

    public static int RemainingXenos
    {
        get { return _remainingXenos; }
        set
        {
            GameObject.Find("HUD").transform.Find("RemainingXenosToKill").GetComponent<TMPro.TMP_Text>().text = "Objective: Kill " + value + " Bugzz";
            _remainingXenos = value;
        }
    }
    static int _remainingXenos = 20;


    private void Start()
    {
        Camera.main.transform.position = new Vector3(-69.2760f, 10.90234f, -10f);
        Camera.main.orthographicSize = 42;

        StartCoroutine(XenoSpawning());
        IEnumerator XenoSpawning()
        {
            while(true)
            {
                yield return new WaitForSeconds(5f);

                if (OurLz == null)
                    continue;
                if (AllXenos.Where(x => x.isAlive).Count() > 20)
                    continue;
                if (XenosLeftToSpawn == 0)
                    continue;

                AllXenos.Add(new Xeno(Instantiate(Resources.Load<GameObject>("Xeno"), GameObject.Find("SpawnpointsXenos").transform.GetChild(0).position, Quaternion.identity)));
                AllXenos.Add(new Xeno(Instantiate(Resources.Load<GameObject>("Xeno"), GameObject.Find("SpawnpointsXenos").transform.GetChild(1).position, Quaternion.identity)));
                XenosLeftToSpawn -= 2f;
            }
        }
    }
    public void StartGameLZOneCLicked()
    {
        GameObject.Find("HUD").transform.Find("ChooseLZ").gameObject.SetActive(false);

        Camera.main.transform.position = new Vector3(-1.92f, 3.5f, -10f);
        Camera.main.orthographicSize = 13.26f;

        OurLz = GameObject.Find("LZOneArea");
        OurLz.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f, 0.15f);
    }
    public void StartGameLZTwoCLicked()
    {
        GameObject.Find("HUD").transform.Find("ChooseLZ").gameObject.SetActive(false);

        Camera.main.transform.position = new Vector3(-119.2f, 9.7f, -10f);
        Camera.main.orthographicSize = 15.7f;

        OurLz = GameObject.Find("LZTwoArea");
        OurLz.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f, 0.15f);
    }
}