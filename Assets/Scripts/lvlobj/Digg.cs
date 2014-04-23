using UnityEngine;
using System.Collections;

/**
 * iipashss, nenoliekams levelobjekts - 
 *  sho lietos, lai raktu zemi (funkcionaaali: dzeestu telpas, bet tikai zemes telpas, telpas, kam FuncType ir noraadits "zemiite")
 */ 
public class Digg : BaseLevelThing {

    public void Awake(){
        baseInit();
    }


    
    
    public override void InitFromString(string str){}
    public override string InitToString( ){   return "";}
        
    
    public override  void PlacedInPlacer(){}
    public override void RemovedFromPlacer(){}        
    public override void PlaceOnGrid(int mode){

        Room target = levelscript.roomAtThisPosition(levelscript.LastPosGrid.x,levelscript.LastPosGrid.y);

        if(target != null) {
            print("raksim " + levelscript.LastPosGrid + " " + target.name);

            target.RemovedFromGrid();//pazinjoju dzeeshamajam objektam, ka tas tiek aizvaakts no laukuma (vinjhs pats organizees aizvaakshanos)
        } else {
            print("te nav ko rakt!");
        }


    }
    public override void RemovedFromGrid(){}

}
