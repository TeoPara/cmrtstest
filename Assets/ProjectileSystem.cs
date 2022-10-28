using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSystem : MonoBehaviour
{
    public static List<Projectile> AllProjectiles = new List<Projectile>();
    public class Projectile
    {
        public Vector2 Direction;
        public GameObject GameObject;
        public float TimeAdded;
    }
    public static void Shoot(Vector2 pos, Vector2 dir)
    {
        AllProjectiles.Add(new Projectile() { Direction = dir, GameObject = Instantiate(Resources.Load<GameObject>("Bullet"), pos, Quaternion.identity), TimeAdded = Time.time });
    }

    private void Update()
    {
        for (int i = 0; i < AllProjectiles.Count; i++)
        {
            Projectile p = AllProjectiles[i];

            // move
            p.GameObject.transform.position += (Vector3)p.Direction * Time.deltaTime * 45f;

            // time out
            if (Time.time - p.TimeAdded > 5)
            {
                Destroy(p.GameObject);
                AllProjectiles.Remove(p);
                i--;
                continue;
            }

            // hit wall
            if (Tools.Walls.HasTile(Tools.Walls.WorldToCell(p.GameObject.transform.position)))
            {
                Destroy(p.GameObject);
                AllProjectiles.Remove(p);
                i--;
                continue;
            }

            // hit xeno
            foreach (XenoSystem.Xeno x in XenoSystem.AllXenos)
            {
                if (x.isDead)
                    continue;

                if (Tools.Centralize(p.GameObject.transform.position) == Tools.Centralize(x.Position))
                {
                    x.Health -= 4.5f;

                    Destroy(p.GameObject);
                    AllProjectiles.Remove(p);
                    i--;
                    break;
                }
            }
        }
    }
}
