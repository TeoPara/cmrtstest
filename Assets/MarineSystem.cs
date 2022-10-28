using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class MarineSystem : MonoBehaviour
{
    public static List<Marine> AllMarines = new List<Marine>();
    public static MarineSystem Ref;
    
    public class Marine
    {
        public Marine(GameObject GO)
        {
            GameObject = GO;
            SpriteRenderer = GO.GetComponent<SpriteRenderer>();
        }
        GameObject GameObject;

        public void Highlight(bool x, Color32 color)
        {
            color.a = 255;
            GameObject.transform.Find("Highlight").gameObject.SetActive(x);
            GameObject.transform.Find("Highlight").GetComponent<SpriteRenderer>().color = color;
        }
        public bool Highlighted => GameObject.transform.Find("Highlight").gameObject.activeInHierarchy;

        // sprite
        SpriteRenderer SpriteRenderer;
        string CurrentFacingDirection
        {
            get { return _currentFacingDirection; }
            set
            {
                _currentFacingDirection = value;

                switch (value)
                {
                    case "down":
                        SpriteRenderer.sprite = Resources.Load<Sprite>("marine_S");
                        break;
                    case "up":
                        SpriteRenderer.sprite = Resources.Load<Sprite>("marine_N");
                        break;
                    case "left":
                        SpriteRenderer.sprite = Resources.Load<Sprite>("marine_W");
                        break;
                    case "right":
                        SpriteRenderer.sprite = Resources.Load<Sprite>("marine_E");
                        break;
                }
                GameObject.transform.Find("Highlight").GetComponent<SpriteRenderer>().sprite = SpriteRenderer.sprite;
            }
        } string _currentFacingDirection = "down";
    
        // movemnet
        bool MoveTo(Vector2 direction)
        {
            if (MovementTarget != new Vector2(float.MaxValue, float.MaxValue))
                return false;
            if (CheckObstacle(Position + direction))
                return false;

            if (direction == new Vector2(1, 0))
                CurrentFacingDirection = "right";
            else if (direction == new Vector2(-1, 0))
                CurrentFacingDirection = "left";
            else if (direction == new Vector2(0, 1))
                CurrentFacingDirection = "up";
            else if (direction == new Vector2(0, -1))
                CurrentFacingDirection = "down";

            MovementTarget = Position + direction;

            return true;
        }
        public void MoveUpdate()
        {
            if (CurrentlyFollowingPath.Count > 0)
                Debug.DrawLine(Position, CurrentlyFollowingPath[0], Color.red);

            // not moving
            if (MovementTarget == new Vector2(float.MaxValue, float.MaxValue))
            {
                // hasve path, move along path
                if (CurrentlyFollowingPath.Count > 0)
                {
                    if (MoveTo(CurrentlyFollowingPath[0] - Position))
                        CurrentlyFollowingPath.RemoveAt(0);
                }
                // havese no path, check if can get path from destinationsqueue
                else
                {
                    if (DestinationsQueue.Count>0)
                    {
                        // find closest
                        Vector2 closest = new Vector2(float.MaxValue, float.MaxValue);
                        foreach(Vector2 v in DestinationsQueue)
                        {
                            if (closest == new Vector2(float.MaxValue, float.MaxValue) || (Position - v).magnitude < (Position - closest).magnitude)
                                closest = v;
                        }
                        // remove all before closest
                        if (closest != new Vector2(float.MaxValue, float.MaxValue))
                        {
                            while (DestinationsQueue.IndexOf(closest) > 0)
                                DestinationsQueue.RemoveAt(0);
                        }


                        PathTo(Tools.Centralize(DestinationsQueue[0]));
                        DestinationsQueue.RemoveAt(0);
                    }
                }
            }
            // moving
            else
            {
                // not arrived
                Vector2 DirStep = (MovementTarget - Position).normalized * Time.deltaTime * 5;
                if ((Position - MovementTarget).magnitude > DirStep.magnitude && Position != MovementTarget)
                {
                    Position += DirStep;
                }
                // arrived
                else
                {
                    Position = MovementTarget;
                    MovementTarget = new Vector2(float.MaxValue, float.MaxValue);
                }
            }
        }
        public void PathTo(Vector2 Destination)
        {
            //MovementTarget = Tools.Centralize(Position);
            CurrentlyFollowingPath = Tools.FindPath(Tools.Centralize(Position), Destination);
            if (CurrentlyFollowingPath.Count == 0)
                Debug.Log("PATH FAILED");
        }
        public bool CheckObstacle(Vector2 pos)
        {
            if (AllMarines.Any(c => c.isAlive && (Tools.Centralize(c.Position) == pos || c.MovementTarget == pos)
            && !(c.CurrentlyFollowingPath.Count>0 && CurrentlyFollowingPath.Count>0 && (c.CurrentlyFollowingPath[0] == Position && CurrentlyFollowingPath[0] == c.Position))))
                return true;
            if (Tools.Walls.HasTile(Tools.Walls.WorldToCell(pos)))
                return true;
            return false;
        }
        public Vector2 Position { get { return GameObject.transform.position; } set { GameObject.transform.position = value; } }
        public Vector2 MovementTarget = new Vector2(float.MaxValue, float.MaxValue);
        public List<Vector2> CurrentlyFollowingPath = new List<Vector2>();
        public List<Vector2> DestinationsQueue = new List<Vector2>();

        // health
        public float Health
        {
            get
            {
                return _health;
            }
            set
            {
                if (value <= 0)
                {
                    value = 0;
                    Kill();
                }
                _health = value;
                GameObject.transform.Find("Canvas").Find("HP").GetComponent<RectTransform>().sizeDelta = new Vector2(value / 100f, 0.06f);
            }
        }
        float _health = 100;
        public bool isAlive
        {
            get { return _isAlive; }
            set
            {
                // start or stop perma death timer
                if (_isAlive && !value && StartedPermaDeathTimer == null)
                    StartedPermaDeathTimer = Ref.StartCoroutine(PermaDeathTimer());
                else if (!_isAlive && value && StartedPermaDeathTimer != null)
                    Ref.StopCoroutine(StartedPermaDeathTimer);

                // adjust sorting order so that bodies appear below
                if (value)
                    SpriteRenderer.sortingOrder = 0;
                else
                    SpriteRenderer.sortingOrder = -1;

                // disable or enable the light
                GameObject.transform.Find("Light").gameObject.SetActive(value);
                //
                _isAlive = value;
            }
        } bool _isAlive = true;
        public bool isDead => !isAlive;
        public bool PermaDead = false;
        public void Kill()
        {
            isAlive = false;
            GameObject.transform.eulerAngles = new Vector3(0, 0, 90);
        }

        Coroutine StartedPermaDeathTimer;
        IEnumerator PermaDeathTimer()
        {
            yield return new WaitForSeconds(30);
            PermaDead = true;
            Debug.Log("Marine now permanently dead");
        }

        // combat
        float lastShot = 0;
        public void ShootUpdate()
        {
            if (Time.time - lastShot > 0.2f)
            {
                Vector2 Closest = new Vector2(float.MaxValue, float.MaxValue);
                foreach(XenoSystem.Xeno x in XenoSystem.AllXenos)
                {
                    if (Vector2.Distance(x.Position, Position) > 25) continue;
                    if (Tools.HasSight(Position, x.Position) == false) continue;
                    if (x.isDead) continue;
                    if (Closest == new Vector2(float.MaxValue, float.MaxValue) || Vector2.Distance(Position, x.Position) < Vector2.Distance(Position, Closest))
                    {
                        Closest = x.Position;
                    }
                }

                if (Closest != new Vector2(float.MaxValue, float.MaxValue))
                {
                    Vector3 PosA = Position + (Closest - Position).normalized;
                    Vector3 PosB = Position + (Closest - Position).normalized * 2;

                    if (!AllMarines.Any(c => Tools.Centralize(c.Position) == Tools.Centralize(PosA) || Tools.Centralize(c.Position) == Tools.Centralize(PosB)))
                    {
                        ProjectileSystem.Shoot(Position, (Closest + new Vector2(UnityEngine.Random.Range(-1,1), UnityEngine.Random.Range(-1, 1)) - Position).normalized);
                        lastShot = Time.time;
                    }
                }
            }
        }
    }

    void Start()
    {
        Ref = this;
    }

    public Vector2 ObjectiveDestination = new Vector2();

    void Update()
    {
        foreach (Marine m in AllMarines)
        {
            if (m.isDead) continue;
            m.MoveUpdate();
            m.ShootUpdate();
        }
    }
}
