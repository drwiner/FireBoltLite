using UnityEngine;
using System.Collections;

public class TerrainTiler : MonoBehaviour {
    public Material tileMaterial;
    public Transform parentTransform;
    public float scale;
	// Use this for initialization
    void Start()
    {
        parentTransform = this.GetComponentInParent<Transform>();
        for (int x = -10; x < 10; x++)
        {
            for (int y = -10; y < 10; y++)
            {

                var tile = GameObject.CreatePrimitive(PrimitiveType.Plane);
                tile.transform.localScale = new Vector3(.5f, .5f, .5f);
                int xOffset = x % 2;
                tile.transform.position = new Vector3(x * scale, 0.1f, y * scale + xOffset * tile.transform.localScale.x);
                tile.GetComponent<MeshRenderer>().material = tileMaterial;
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
