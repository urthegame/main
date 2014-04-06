using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public enum Res { air, electricity, water };





/**
 * globalo resursu pieskatiitaajs, singltons
 * tikai glabaa veertiibas, vinjam taas pazinjo, vinsh pats nemeklee
 */ 
public class GlobalResources : MonoBehaviour {

	public int Money;

	public Dictionary<Res,float> Generation; // cik max var sarazhot
	public Dictionary<Res,float> Usage; // cik teeree


	void Awake () {

		Init ();
	}

	//inicializee [un reseto] 
	public void Init(){

		Money = 1000;


		Generation = new Dictionary<Res, float>();
		Usage = new Dictionary<Res, float>();
		
		
		foreach (Res r in Enum.GetValues(typeof(Res))){
			Generation[r] = 0;
			Usage[r] = 0;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
