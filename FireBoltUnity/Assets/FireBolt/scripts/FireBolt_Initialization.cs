﻿using UnityEngine;
using System.Collections;

public class FireBolt_Initialization : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
    {
        ElPresidente elPresidente = this.GetComponent<ElPresidente>();
        elPresidente.Init();

        //elPresidente.createdGameObjects.Add(cowboy,
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
