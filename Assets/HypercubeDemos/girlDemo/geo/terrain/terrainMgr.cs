using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[RequireComponent (typeof(randomTable))]
[ExecuteInEditMode]
public class terrainMgr : MonoBehaviour {

    public GameObject[] terrains;
    public float terrainSize;

    public float displayDistance;

    public Transform salientPos;

    public int minThings;
    public int maxThings;
    public placeableThing[] spawnables;

    randomTable rand;
	// Use this for initialization
	void Start () 
    {
        rand = GetComponent<randomTable>();
        if (salientPos == null)
            salientPos = transform;

	}
	
	// Update is called once per frame
	void Update () 
    {
        if (transform.hasChanged)
        {
            updatePosition();
            transform.hasChanged = false;
        }
	}

    void updatePosition()
    {

        Vector3 topL = new Vector3();
        topL.x = terrainSize * Mathf.Floor(salientPos.position.x / terrainSize);
        topL.z = terrainSize * Mathf.Ceil(salientPos.position.z / terrainSize);

        Vector3 topR = new Vector3();
        topR.x = terrainSize * Mathf.Ceil(salientPos.position.x / terrainSize);
        topR.z = terrainSize * Mathf.Ceil(salientPos.position.z / terrainSize);

        Vector3 dwnL = new Vector3();
        dwnL.x = terrainSize * Mathf.Floor(salientPos.position.x / terrainSize);
        dwnL.z = terrainSize * Mathf.Floor(salientPos.position.z / terrainSize);

        Vector3 dwnR = new Vector3();
        dwnR.x = terrainSize * Mathf.Ceil(salientPos.position.x / terrainSize);
        dwnR.z = terrainSize * Mathf.Floor(salientPos.position.z / terrainSize);

        if (!utils.isWithinGridRange2D(salientPos.position, topL, displayDistance))
        {
            terrains[0].SetActive(false);
        }
        else
        {
            rand.seed = (uint)(topL.x + topL.z);
            terrains[0].transform.rotation = getRandomYAxis();
            if (terrains[0].transform.position != topL)
                addTrees(terrains[0]);           
            terrains[0].transform.position = topL;
            terrains[0].SetActive(true);
        }

        if (utils.isWithinGridRange2D(salientPos.position, topR, displayDistance) && salientPos.position.x != 0)
        {
            rand.seed = (uint)(topR.x + topR.z);
            terrains[1].transform.rotation = getRandomYAxis();
            if (terrains[1].transform.position != topR)
                addTrees(terrains[1]);            
            terrains[1].transform.position = topR;           
            terrains[1].SetActive(true);
        }
        else
        {
            terrains[1].SetActive(false);
        }

        if (utils.isWithinGridRange2D(salientPos.position, dwnL, displayDistance) && salientPos.position.z != 0)
        {
            rand.seed = (uint)(dwnL.x + dwnL.z);
            terrains[2].transform.rotation = getRandomYAxis();
            if (terrains[2].transform.position != dwnL)
                addTrees(terrains[2]);
            terrains[2].transform.position = dwnL;
            terrains[2].SetActive(true);                   
        }
        else
        {
            terrains[2].SetActive(false);
        }

        if (utils.isWithinGridRange2D(salientPos.position, dwnR, displayDistance) && salientPos.position.x != 0 && salientPos.position.z != 0)
        {
            rand.seed = (uint)(dwnR.x + dwnR.z);
            terrains[3].transform.rotation = getRandomYAxis();  
            if (terrains[3].transform.position != dwnR)
                addTrees(terrains[3]);
            terrains[3].transform.position = dwnR;
            terrains[3].SetActive(true);                   
        }
        else
        {
            terrains[3].SetActive(false);
        }
    }

    void addTrees(GameObject terrain)
    {
        //destroy existing children
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in terrain.transform) children.Add(child.gameObject);
        children.ForEach(child => DestroyImmediate(child));

        //make things

        int thingCount = rand.getInt(minThings, maxThings);
        for (int i = 0; i < thingCount; i++)
        {
            GameObject newThing = chooseThingToPlace();
            if (!newThing)
                continue;

            newThing.transform.parent = terrain.transform;

            float halfTerrainSize = terrainSize/2;
            float raycastHeight = 20f;
            Vector3 pos = new Vector3(rand.getFloat(terrain.transform.position.x - halfTerrainSize, terrain.transform.position.x + halfTerrainSize), raycastHeight, rand.getFloat(terrain.transform.position.z - halfTerrainSize, terrain.transform.position.z + halfTerrainSize));

            RaycastHit hit;
            if (Physics.Raycast(pos, Vector3.down, out hit, 50))
            {
                pos = hit.point;
            }
            else
                pos.y = 0; //just jam it into 0, who knows why it didn't collide

            newThing.transform.rotation = Quaternion.Euler(0f, rand.getFloat(0f, 360f), 0f);

            newThing.transform.position = pos;
        }
    }

    GameObject chooseThingToPlace()
    {
        int total = 0;
        foreach (placeableThing p in spawnables)
        {
            total += p.chance;
        }

        int choice = rand.getInt(1, total);

        total = 0;
        foreach (placeableThing p in spawnables)
        {
            total += p.chance;
            if (choice <= total) // we found our choice
            {
                return Instantiate(p.gameObject);
            }
        }
        Debug.LogWarning("There is a bug. nothing was chosen to place.");
        return null;
    }

 
    //face either N S E W
    Quaternion getRandomYAxis()
    {
        int r = rand.getInt(0, 3);
        if (r == 0)
            return Quaternion.identity;
        if (r == 1)
            return Quaternion.Euler(0, 90 ,0);
        if (r == 2)
            return Quaternion.Euler(0, 180, 0);
      //  if (r == 3)
            return Quaternion.Euler(0, 270, 0);
    }






}
