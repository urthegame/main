using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class LOQuery : MonoBehaviour {


	private Gui guiscript; //vieniigais/globalais GUI skripts

	void Awake () {
		guiscript =  GameObject.Find("GUI").GetComponent<Gui>();
	//	baseInit();
	}




	
	 public void PlaceOnGrid(int mode){
		
        //@refactor -- chekot uz kura levelobjekta atrodas lietojot levelskripta roomAtThisPosition metodi
        /*
		if(currentCollisions.Count > 0 ){

			guiscript.QueryMode = true;

			//sarakstaa parasti var buut tikai viens levelobjekts (kameer es kvarijuoju viena kubika ietvaros)
			foreach(KeyValuePair<GameObject, bool> x in currentCollisions){ //vienkaarshaa iteraacija caur Dictu darbojas, kameer man pietiek ar read-only
				guiscript.QueryTarget = x.Key.GetComponent<Levelobject>();
			}
		}*/
	}

	public  void PlacedInPlacer(){
		guiscript.QueryMode = true;
	}
	
	public  void RemovedFromPlacer(){
		guiscript.QueryMode = false;
		guiscript.QueryTarget = null;
	}

	public  void RemovedFromGrid(){
		guiscript.QueryMode = false;
		guiscript.QueryTarget = null;
	}


	
}
