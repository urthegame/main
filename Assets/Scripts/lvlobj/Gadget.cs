using UnityEngine;
using System.Collections;

public class Gadget : BaseLevelThing {

    //

    public bool[] suitableForRooms = new bool[RoomRoles.Names.Length];//noraada true/false katrai telpas lomai - vai shis gadzhets var atrasties attieciigajaa telpaa
    //svarigi ir nomainiit katraa gadzheta prefabaa, katru reizi, kad nomainaas telpu lomu skaits, nosaukumi




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
