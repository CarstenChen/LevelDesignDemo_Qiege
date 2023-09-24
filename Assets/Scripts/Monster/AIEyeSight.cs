using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIEyeSight : MonoBehaviour
{
    [Header("Eye Sight Settings")]
    public MeshFilter mesh;
    [Range(5f, 1000f)]
    public float accuracy = 50;
    [Range(1f, 180f)]
    public float angle = 60f;
    [Range(1f, 180f)]
    public float radius = 5f;
    public float range = 6f;
    public bool showLOS = true;
    public string detectionTags;

    protected AIMonsterController monster;
    protected bool isTriggerRange = false;

    protected List<Vector3> itemList = new List<Vector3>();

    public SphereCollider holder;
    protected Light lightHolder;

    // Start is called before the first frame update
    void Start()
    {
        InitializeComponent();
    }

    // Update is called once per frame
    void Update()
    {
        if (LinesManager.isPlayingLines && !LinesManager.isReadingStartPlot)
        {
            holder.radius = range / 3;
        }
        else
        {
            holder.radius = range;
        }

        if (!isTriggerRange)
        {
            monster.playerInSphereTrigger = false;
            monster.playerInSight = false;
            return;
        }
        else
        {
            monster.playerInSphereTrigger = true;
        }




        List<Vector3> newVertices = new List<Vector3>();
        newVertices.Add(Vector3.zero);
        GetCone();

        foreach(Vector3 item in itemList)
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.position, item);

            if (Physics.Raycast(ray, out hit, range))
            {
                if (hit.collider.gameObject.tag == detectionTags)
                {
                    CallBack(hit.collider.gameObject);
                    return;
                }
                newVertices.Add(hit.point - transform.position);
            }
            else
            {
                newVertices.Add(ray.GetPoint(range) - transform.position);
            }

            monster.playerInSight = false;
        }



        if (showLOS)
        {
            DrawLos(newVertices);
        }
        else
        {
            mesh.mesh.Clear();
        }
    }

    void InitializeComponent()
    {
        monster = GetComponent<AIMonsterController>();

        holder.radius = range;

        lightHolder = GetComponent<Light>();
        if (lightHolder)
        {
            if (lightHolder.type == LightType.Spot)
            {
                angle = lightHolder.spotAngle / 2;
                radius = lightHolder.spotAngle;
                range = lightHolder.range;
            }
        }
    }

    void CallBack(GameObject obj)
    {
     //   Debug.Log("See:" + obj.name);
        monster.playerInSight = true;
    }

    public List<Vector3> GetCone()
    {
        itemList.Clear();
        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float angleIncrement = Mathf.PI * 2 * goldenRatio;

        for (int i = 0; i < accuracy; i++)
        {
            float t = (float)i / accuracy;
            float inclination = Mathf.Acos(1 - 2 * t);
            float azimuth = angleIncrement * i;

            float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
            float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
            float z = Mathf.Cos(inclination);
            Vector3 rotation = new Vector3(x, y, z) * radius;
            rotation += transform.forward * range;
            itemList.Add(rotation);
        }
        return itemList;
    }

    void DrawLos(List<Vector3> newVertices)
    {
        mesh.mesh.Clear();
        List<Vector2> newUV = new List<Vector2>();
        List<int> newTriangles = new List<int>();
        for (int i = 1; i < newVertices.Count - 1; i++)
        {
            newTriangles.Add(0);
            newTriangles.Add(i);
            newTriangles.Add(i + 1);
        }
        for (int i = 0; i < newVertices.Count; i++)
        {
            newUV.Add(new Vector2(newVertices[i].x, newVertices[i].z));
        }
        mesh.mesh.vertices = newVertices.ToArray();
        mesh.mesh.triangles = newTriangles.ToArray();
        mesh.mesh.uv = newUV.ToArray();
        mesh.transform.rotation = Quaternion.identity;
        mesh.mesh.RecalculateNormals();
    }

    void OnTriggerStay(Collider other)
    {

        if (other.gameObject.tag == "Player")
        {
            isTriggerRange = true;
            monster.playerFirstFound = true;

            if (Random.Range(0, 300) == 0 && !monster.playerHeard)
            {
                monster.playerHeard = true;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            isTriggerRange = false;
            monster.playerHeard = false;
        }
    }
}
