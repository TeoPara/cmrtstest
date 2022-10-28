using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class XenoSystem : MonoBehaviour
{
    public static List<Xeno> AllXenos = new List<Xeno>();
    
    public class Xeno
    {
        public bool CheckObstacle(Vector2 pos)
        {
            if (Tools.Walls.HasTile(Tools.Walls.WorldToCell(pos)))
                return true;
            return false;
        }

        public Xeno(GameObject GO)
        {
            GameObject = GO;
            SpriteRenderer = GO.GetComponent<SpriteRenderer>();
        }

        GameObject GameObject;
        SpriteRenderer SpriteRenderer;

        public Vector2 Position { get { return GameObject.transform.position; } set { GameObject.transform.position = value; } }
        string CurrentFacingDirection
        {
            get { return _currentFacingDirection; }
            set
            {
                _currentFacingDirection = value;

                switch (value)
                {
                    case "down":
                        SpriteRenderer.sprite = Resources.Load<Sprite>("xeno_S");
                        break;
                    case "up":
                        SpriteRenderer.sprite = Resources.Load<Sprite>("xeno_N");
                        break;
                    case "left":
                        SpriteRenderer.sprite = Resources.Load<Sprite>("xeno_W");
                        break;
                    case "right":
                        SpriteRenderer.sprite = Resources.Load<Sprite>("xeno_E");
                        break;
                }
            }
        }
        string _currentFacingDirection = "down";


        public Vector2 MovementTarget = new Vector2(float.MaxValue, float.MaxValue);

        public bool MoveTo(Vector2 direction)
        {
            if (MovementTarget != new Vector2(float.MaxValue, float.MaxValue))
                return false;
            if (CheckObstacle(Position + direction))
                return false;

            if (MarineSystem.AllMarines.Any(c => c.isAlive && Tools.Centralize(c.Position) == Position + direction))
                MarineSystem.AllMarines.First(c => c.isAlive && Tools.Centralize(c.Position) == Position + direction).Health -= 50;

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
            // not moving
            if (MovementTarget == new Vector2(float.MaxValue, float.MaxValue))
            {
                // move along path
                if (CurrentlyFollowingPath.Count > 0)
                {
                    if (MoveTo(CurrentlyFollowingPath[0] - Position))
                        CurrentlyFollowingPath.RemoveAt(0);
                }
            }
            // moving
            else
            {
                // not arrived
                Vector2 DirStep = (MovementTarget - Position).normalized * Time.deltaTime * 15;
                if ((Position - MovementTarget).magnitude > DirStep.magnitude)
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
        public void Flee(List<Vector2> VisibleMarines)
        {
            MovementTarget = Tools.Centralize(Position);
            CurrentlyFollowingPath = Tools.FindPathToFlee(Tools.Centralize(Position), VisibleMarines);
        }
        public void PathTo(Vector2 Destination)
        {
            MovementTarget = Tools.Centralize(Position);
            CurrentlyFollowingPath = Tools.FindPath(Tools.Centralize(Position), Destination);
        }
        public List<Vector2> CurrentlyFollowingPath = new List<Vector2>();

        // health
        public float Health
        {
            get
            {
                return _health;
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                    Kill();
                }
                _health = value;
            }
        } float _health = 100;

        public bool isAlive { get { return _isAlive; } set { _isAlive = value; if (value) SpriteRenderer.sortingOrder = 0; else SpriteRenderer.sortingOrder = -1; } } bool _isAlive = true;
        public bool isDead => !isAlive;
        public void Kill()
        {
            isAlive = false;
            GameObject.transform.eulerAngles = new Vector3(0, 0, 90);
            Controls.RemainingXenos--;
        }
    }

    void Start()
    {
        StartCoroutine(RandomMovexd());
        IEnumerator RandomMovexd()
        {
            while(true)
            {
                yield return new WaitForSeconds(0.5f);
                foreach (Xeno x in AllXenos)
                {
                    if (x.isDead)
                        continue;

                    MarineSystem.Marine Closest = null;
                    float ClosestDistance = float.MaxValue;
                    foreach(MarineSystem.Marine m in MarineSystem.AllMarines.Where(m => (Vector2.Distance(x.Position, m.Position) < 3 || Tools.HasSight(x.Position, Tools.Centralize(m.Position))) &&  m.isAlive && Tools.Centralize(m.Position) != Tools.Centralize(x.Position)))
                    {
                        if (Closest == null || Vector2.Distance(x.Position, m.Position) < ClosestDistance)
                        {
                            Closest = m;
                            ClosestDistance = Vector2.Distance(x.Position, m.Position);
                        }
                    }

                    if (Closest != null)
                    {
                        // attacking
                        if (x.Health > 25)
                        {
                            x.PathTo(Tools.Centralize(Closest.Position));
                        }
                        // running
                        else
                        {
                            List<Vector2> list = new List<Vector2>();
                            foreach (MarineSystem.Marine m in MarineSystem.AllMarines.Where(m => m.isAlive && Tools.HasSight(x.Position, Tools.Centralize(m.Position))))
                                list.Add(m.Position);
                            x.Flee(list);
                        }
                    }
                    if (Closest == null && x.MovementTarget == new Vector2(float.MaxValue, float.MaxValue))
                    {
                        Vector2 bruh = x.Position + new Vector2(UnityEngine.Random.Range(-10, 10), UnityEngine.Random.Range(-10, 10));
                        while (x.CheckObstacle(bruh))
                            bruh = x.Position + new Vector2(UnityEngine.Random.Range(-10, 10), UnityEngine.Random.Range(-10, 10));

                        x.PathTo(bruh);
                    }
                }
            }
        }
    }

    void Update()
    {
        foreach (Xeno x in AllXenos)
        {
            if (x.isDead) continue;

            x.MoveUpdate();
        }
    }
}
