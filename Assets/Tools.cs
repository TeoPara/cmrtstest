using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tools : MonoBehaviour
{
    public static Tilemap Walls => GameObject.Find("Map").transform.Find("Walls").GetComponent<Tilemap>();
    public static Vector2 Centralize(Vector2 c) => new Vector2(Mathf.FloorToInt(c.x) + 0.5f, Mathf.FloorToInt(c.y) + 0.5f);

    public static List<Vector2> FindPathToFlee(Vector2 Start, List<Vector2> FleeFrom)
    {
        Vector2 Avg = Vector2.zero;
        foreach (Vector2 v in FleeFrom)
            Avg += v;
        Avg /= FleeFrom.Count;

        Vector2 Goal = (Start - Avg).normalized * 15;
        Goal = Centralize(Goal);
        while (Walls.HasTile(Walls.WorldToCell(Goal)))
            Goal += new Vector2(UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(-5, 5));

        return FindPath(Start, Goal);
    }
    public static List<Vector2> FindPath(Vector2 Start, Vector2 End)
    {
        List<Vector2> Visited = new List<Vector2>();
        List<Vector2> EndResult = new List<Vector2>();

        if (!Walls.HasTile(Walls.WorldToCell(End)))
            Next(new List<Vector2>() { Start });
        
        void Next(List<Vector2> soFar, int count = 0)
        {
            if (count == 300)
            {
                Debug.Log("MAX");
                return;
            }

            Vector2 CurrentPosition = soFar.Last();

            Visited.Add(CurrentPosition);

            if (CurrentPosition == End)
            {
                EndResult = soFar;
                return;
            }
            else
            {
                List<Vector2> dirs = new List<Vector2>();

                // get dirs based on direction to end
                {
                    float difX = Mathf.Abs(End.x - CurrentPosition.x);
                    float difY = Mathf.Abs(End.y - CurrentPosition.y);

                    if (End.x < CurrentPosition.x && End.y == CurrentPosition.y)
                    {
                        dirs = new List<Vector2>()
                        {
                            new Vector2(-1,0),
                            new Vector2(0,1),
                            new Vector2(0,-1),
                            new Vector2(1,0),
                        };
                    }

                    else if (End.x > CurrentPosition.x && End.y == CurrentPosition.y)
                    {
                        dirs = new List<Vector2>()
                        {
                            new Vector2(1,0),
                            new Vector2(0,-1),
                            new Vector2(0,1),
                            new Vector2(-1,0),
                        };
                    }
                    else if (End.x == CurrentPosition.x && End.y < CurrentPosition.y)
                    {
                        dirs = new List<Vector2>()
                        {
                            new Vector2(0,-1),
                            new Vector2(-1,0),
                            new Vector2(1,0),
                            new Vector2(0,1),
                        };
                    }
                    else if (End.x == CurrentPosition.x && End.y > CurrentPosition.y)
                    {
                        dirs = new List<Vector2>()
                        {
                            new Vector2(0,1),
                            new Vector2(1,0),
                            new Vector2(-1,0),
                            new Vector2(0,-1),
                        };
                    }
                    else if (End.x < CurrentPosition.x && End.y < CurrentPosition.y)
                    {
                        if (difX >= difY)
                        {
                            dirs = new List<Vector2>()
                            {
                                new Vector2(-1,0),
                                new Vector2(0,1),
                                new Vector2(0,-1),
                                new Vector2(1,0),
                            };
                        }
                        else
                        {
                            dirs = new List<Vector2>()
                            {
                                new Vector2(0,-1),
                                new Vector2(-1,0),
                                new Vector2(1,0),
                                new Vector2(0,1),
                            };
                        }
                    }
                    else if (End.x < CurrentPosition.x && End.y > CurrentPosition.y)
                    {
                        if (difX >= difY)
                        {
                            dirs = new List<Vector2>()
                            {
                                new Vector2(-1,0),
                                new Vector2(0,1),
                                new Vector2(0,-1),
                                new Vector2(1,0),
                            };
                        }
                        else
                        {
                            dirs = new List<Vector2>()
                            {
                                new Vector2(0,1),
                                new Vector2(1,0),
                                new Vector2(-1,0),
                                new Vector2(0,-1),
                            };
                        }
                    }
                    else if (End.x > CurrentPosition.x && End.y < CurrentPosition.y)
                    {
                        if (difX >= difY)
                        {
                            dirs = new List<Vector2>()
                            {
                                new Vector2(1,0),
                                new Vector2(0,-1),
                                new Vector2(0,1),
                                new Vector2(-1,0),
                            };
                        }
                        else
                        {
                            dirs = new List<Vector2>()
                            {
                                new Vector2(0,-1),
                                new Vector2(-1,0),
                                new Vector2(1,0),
                                new Vector2(0,1),
                            };
                        }
                    }
                    else if (End.x > CurrentPosition.x && End.y > CurrentPosition.y)
                    {
                        if (difX >= difY)
                        {
                            dirs = new List<Vector2>()
                            {
                                new Vector2(1,0),
                                new Vector2(0,1),
                                new Vector2(0,-1),
                                new Vector2(-1,0),
                            };
                        }
                        else
                        {
                            dirs = new List<Vector2>()
                            {
                                new Vector2(0,1),
                                new Vector2(1,0),
                                new Vector2(-1,0),
                                new Vector2(0,-1),
                            };
                        }
                    }
                }


                bool foundsome = false;
                foreach (Vector2 v in dirs)
                {
                    if (!Visited.Contains(CurrentPosition + v) && !Walls.HasTile(Walls.WorldToCell(CurrentPosition + v)))
                    {
                        soFar.Add(CurrentPosition + v);

                        Next(soFar, count + 1);

                        foundsome = true;
                        break;
                    }
                }
                if (foundsome == false)
                {
                    soFar.RemoveAt(soFar.Count() - 1);
                    Next(soFar, count + 1);
                }
            }
        }

        // cut uneccessary small U-shapes
        if (EndResult.Count > 3)
        {
            while (true)
            {
                List<Vector2> result = TraceBack(EndResult);
                if (result == null)
                    break;
                else
                    EndResult = result;
            }
        }

        // remove the start, not needed
        if (EndResult.Count > 0)
            EndResult.RemoveAt(0);

        return EndResult;

        List<Vector2> TraceBack(List<Vector2> Path)
        {
            List<int> toRemove = new List<int>();
            for (int i = Path.Count - 1; i > 0; i--)
            {
                Vector2 v = Path[i];

                if (Path.Where(c => i - Path.IndexOf(c) > 1).Any(c => c == v + new Vector2(-1, 0)))
                {
                    Vector2 found = Path.Where(c => i - Path.IndexOf(c) > 1).FirstOrDefault(c => c == v + new Vector2(-1, 0));
                    int count = i - Path.IndexOf(found);
                    for (int fff = Path.IndexOf(found) + 1; fff < (Path.IndexOf(found) + count); fff++)
                    {
                        toRemove.Add(fff);
                    }
                    break;
                }
                if (Path.Where(c => i - Path.IndexOf(c) > 1).Any(c => c == v + new Vector2(1, 0)))
                {
                    Vector2 found = Path.Where(c => i - Path.IndexOf(c) > 1).FirstOrDefault(c => c == v + new Vector2(1, 0));
                    int count = i - Path.IndexOf(found);
                    for (int fff = Path.IndexOf(found) + 1; fff < (Path.IndexOf(found) + count); fff++)
                    {
                        toRemove.Add(fff);
                    }
                    break;
                }
                if (Path.Where(c => i - Path.IndexOf(c) > 1).Any(c => c == v + new Vector2(0, 1)))
                {
                    Vector2 found = Path.Where(c => i - Path.IndexOf(c) > 1).FirstOrDefault(c => c == v + new Vector2(0, 1));
                    int count = i - Path.IndexOf(found);
                    for (int fff = Path.IndexOf(found) + 1; fff < (Path.IndexOf(found) + count); fff++)
                    {
                        toRemove.Add(fff);
                    }
                    break;
                }
                if (Path.Where(c => i - Path.IndexOf(c) > 1).Any(c => c == v + new Vector2(0, -1)))
                {
                    Vector2 found = Path.Where(c => i - Path.IndexOf(c) > 1).FirstOrDefault(c => c == v + new Vector2(0, -1));
                    int count = i - Path.IndexOf(found);
                    for (int fff = Path.IndexOf(found) + 1; fff < (Path.IndexOf(found) + count); fff++)
                    {
                        toRemove.Add(fff);
                    }
                    break;
                }
            }

            if (toRemove.Count == 0)
                return null;

            List<Vector2> toRemove2 = new List<Vector2>();
            foreach (int bbb in toRemove)
            {
                toRemove2.Add(Path[bbb]);
            }
            foreach (Vector2 v in toRemove2)
                Path.Remove(v);

            return Path;
        }
    }

    public static bool HasSight(Vector2 Start, Vector2 End)
    {
        return Bresenham((int)Start.x, (int)Start.y, (int)End.x, (int)End.y);
        bool Bresenham(int x0, int y0, int x1, int y1)
        {
            // Get the "slope"
            int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy, e2;

            // Infinite loop
            for (; ; )
            {
                // check if reached end
                if (x0 == x1 && y0 == y1)
                    break;

                // Hit a wall
                if (Walls.HasTile(Walls.WorldToCell(new Vector3(x0+0.5f,y0+0.5f))))
                    return false;
                
                // Go a tile further towards the destination using the slope
                e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
            return true;
        }
    }
}