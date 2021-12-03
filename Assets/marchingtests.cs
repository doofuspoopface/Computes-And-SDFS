using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class marchingtests : MonoBehaviour
{
    Mesh mesh;

    public Transform emptyp;

    Transform debugSphere;

    struct triangle
    {
        public Vector3 v1, v2, v3;
        public Vector3 pos;
        public Vector3 nor;
        public Vector3 bounds;
    }

    triangle[] tris;
    void Start()
    {
        debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        debugSphere.localScale *= 0.5f;

        mesh = GetComponent<MeshFilter>().mesh;
        tris = new triangle[mesh.triangles.Length / 3];


        int triIndex = 0;
        for (int i = 0; i < tris.Length; i++)
        {
            tris[i].v1 = mesh.vertices[mesh.triangles[triIndex]];
            tris[i].v2 = mesh.vertices[mesh.triangles[triIndex + 1]];
            tris[i].v3 = mesh.vertices[mesh.triangles[triIndex + 2]];
            tris[i].pos = (tris[i].v1 + tris[i].v2 + tris[i].v3) / 3.0f;
            tris[i].nor = Vector3.Cross(tris[i].v1 - tris[i].v2, tris[i].v2 - tris[i].v3);

            triIndex += 3;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 Point = emptyp.position;
        //Debug.DrawLine(Point, tris[map(Point).ID].pos);
        Result res = rayCast(emptyp.position, emptyp.forward);
        Vector3 hitPoint = emptyp.position + 
        (emptyp.forward * res.Hit);

        Result res2 = O_rayCast(emptyp.position, emptyp.forward);

        Vector3 hitPoint2 = emptyp.position + 
        (emptyp.forward * res2.Hit);

        Debug.Log(res.ID + " " + res2.ID + " " + (res.ID == res2.ID));

        Debug.DrawLine(emptyp.position, hitPoint, Color.blue);
        debugSphere.position = hitPoint2;

    }

    float dot2(Vector3 a)
    {
        return Vector3.Dot(a, a);
    }
    float triangleSDF(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 ba = b - a; Vector3 pa = p - a;
        Vector3 cb = c - b; Vector3 pb = p - b;
        Vector3 ac = a - c; Vector3 pc = p - c;
        Vector3 nor = Vector3.Cross(ba, ac);

        return Mathf.Sqrt(
          (Mathf.Sign(Vector3.Dot(Vector3.Cross(ba, nor), pa)) +
           Mathf.Sign(Vector3.Dot(Vector3.Cross(cb, nor), pb)) +
           Mathf.Sign(Vector3.Dot(Vector3.Cross(ac, nor), pc)) < 2.0)
           ?
           Mathf.Min(Mathf.Min(
           dot2(ba * Mathf.Clamp(Vector3.Dot(ba, pa) / dot2(ba), 0.0f, 1.0f) - pa),
           dot2(cb * Mathf.Clamp(Vector3.Dot(cb, pb) / dot2(cb), 0.0f, 1.0f) - pb)),
           dot2(ac * Mathf.Clamp(Vector3.Dot(ac, pc) / dot2(ac), 0.0f, 1.0f) - pc))
           :
           Vector3.Dot(nor, pa) * Vector3.Dot(nor, pa) / dot2(nor));
    }

    struct Point
    {
        public float distance;
        public int ID;
    }

    struct Result
    {
        public float Hit;
        public int ID;
    }
    Point map(Vector3 point)
    {
        Point closestPoint = new Point();
        closestPoint.distance = 1000000;
        for (int i = 0; i < tris.Length; i++)
        {
            float dist = triangleSDF(point, tris[i].v1, tris[i].v2, tris[i].v3);
            if (dist < closestPoint.distance)
            {
                closestPoint.distance = dist;
                closestPoint.ID = i;
            }
        }
        return closestPoint;
    }

    int closestTri(Vector3 p, Vector3 dir)
    {
        float maxDelta = 0.0f;
        int closestID = -1;
        for(int i = 0; i < tris.Length; i++)
        {
            float d1 = triangleSDF(p, tris[i].v1, tris[i].v2, tris[i].v3);
            float d2 = triangleSDF(p + dir, tris[i].v1, tris[i].v2, tris[i].v3);

            if(d1-d2 > maxDelta)
            {
                maxDelta = d1-d2;
                closestID = i;
            }
        }
        return closestID;
    }
    Result O_rayCast(Vector3 Origin, Vector3 Direction)
    {
        float maxDist = 100;
        int i = 0;
        float dist=0.1f;

        int c = closestTri(Origin, Direction);

        Result res = new Result();

        res.ID = c;

        for(float t = dist; t< maxDist;)
        {
            float h = triangleSDF(Origin + Direction * t, tris[c].v1, tris[c].v2, tris[c].v3);
            if(h<0.1f)
            {

                res.Hit = t;
                return res;
            }
            t += h;
        }
        res.ID = -2;
        res.Hit = maxDist;
        return res;
    }

    Result rayCast(Vector3 Origin, Vector3 Direction)
    {
        float maxDist = 100;
        float dist=0.1f;
        Result res = new Result();
        for(float t = dist; t< maxDist;)
        {
            Point h = map(Origin + Direction * t);
            if(h.distance<0.01f)
            {
                res.Hit = t;
                res.ID = h.ID;
                return res;
            }
            t += h.distance;
        }

        res.Hit = maxDist;
        res.ID = -1;
       
        return res;
    }
}
