using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class computeMarching : MonoBehaviour
{
    public ComputeShader MarchingShader;
    private RenderTexture _target;

    [SerializeField]
    GameObject RenderedObject;

    Camera camera;


    struct triangle
    {
        public Vector3 v1, v2, v3;
        public Vector3 pos;
        public Vector3 nor;
        public Vector3 bounds;
    }

    triangle[] tris;

    void Awake()
    {
        camera = Camera.main;
        Mesh mesh = RenderedObject.GetComponent<MeshFilter>().mesh;
        tris = new triangle[mesh.triangles.Length / 3];

        int triIndex = 0;
        for (int i = 0; i < tris.Length; i++)
        {
            tris[i].v1 = mesh.vertices[mesh.triangles[triIndex]];
            tris[i].v2 = mesh.vertices[mesh.triangles[triIndex + 1]];
            tris[i].v3 = mesh.vertices[mesh.triangles[triIndex + 2]];
            tris[i].pos = (tris[i].v1 + tris[i].v2 + tris[i].v3) / 3.0f;
            tris[i].nor = Vector3.Cross(tris[i].v1 - tris[i].v2, tris[i].v2 - tris[i].v3);
            tris[i].bounds = new Vector3(
                Mathf.Max(new float[] {tris[i].v1.x, tris[i].v2.x,tris[i].v3.x})-
                Mathf.Min(new float[] {tris[i].v1.x, tris[i].v2.x,tris[i].v3.x}),

                Mathf.Max(new float[] {tris[i].v1.y, tris[i].v2.y,tris[i].v3.y})-
                Mathf.Min(new float[] {tris[i].v1.y, tris[i].v2.y,tris[i].v3.y}),

                Mathf.Max(new float[] {tris[i].v1.z, tris[i].v2.z,tris[i].v3.z})-
                Mathf.Min(new float[] {tris[i].v1.z, tris[i].v2.z,tris[i].v3.z})
            );

            triIndex += 3;
        }
        

        ComputeBuffer buffer = new ComputeBuffer(tris.Length, sizeof(float)*18);
        buffer.SetData(tris);
        MarchingShader.SetBuffer(0, "tris", buffer);
        MarchingShader.SetInt("trisLength", tris.Length);       
    }

    void setShaderParameters()
    {
        MarchingShader.SetMatrix("_CamToWorld", camera.cameraToWorldMatrix);
        MarchingShader.SetMatrix("_CamInverseProj", camera.projectionMatrix.inverse);
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        setShaderParameters();
        Render(destination);
    }
    private void Render(RenderTexture destination)
    {
        // Make sure we have a current render target
        InitRenderTexture();
        // Set the target and dispatch the compute shader
        MarchingShader.SetTexture(0, "Result", _target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        MarchingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        // Blit the result texture to the screen
        Graphics.Blit(_target, destination);
    }
    private void InitRenderTexture()
    {
        if (_target == null || _target.width != Screen.width || _target.height != Screen.height)
        {
            // Release render texture if we already have one
            if (_target != null)
                _target.Release();
            // Get a render target for Ray Tracing
            _target = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.Create();
        }
    }
}
