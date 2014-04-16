using UnityEngine;
using System.Collections;

public class Gadget : BaseLevelThing {

	void Awake () {
        baseInit();
	}
	

	void Update () {
	
	}


    public override void InitFromString(string str){}
    public override string InitToString(){ return "";}
    
    
    
    public override void PlacedInPlacer(){}
    public override void RemovedFromPlacer(){}
    
    
    public override void PlaceOnGrid(int mode){}
    public override void RemovedFromGrid(){}


}
