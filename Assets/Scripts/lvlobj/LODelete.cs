using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class LODelete : Levelobject {
	

	public void Awake(){
		baseInit();
	}

	public override void PlaceOnGrid(int mode){

		if(currentCollisions.Count > 0 ){
		//	print ("Kaut kas tiks izdzeests (atvechaju) ");

            //@refactor -- chekot uz kura levelobjekta atrodas lietojot levelskripta roomAtThisPosition metodi



			//ies caur visiem levelobjektiem, ar ko pashlaik saskaraas un tos izniicijaas
			List<GameObject> collidingGameObjects = new List<GameObject>(currentCollisions.Keys); //jaadabuu atsleedzinjas un tad jaaitere (nevis pa taisno, kaa taadam barbaram)
			foreach (GameObject collidingGameObject in collidingGameObjects)
			{
				collidingGameObject.gameObject.GetComponent<Levelobject>().RemovedFromGrid();
				currentCollisions.Remove(collidingGameObject); //izmet no saraksta, jo vairaak nesaskaras
				//Destroy(collidingGameObject); //izniicija arii levelobjektu ar ko saskaaraas
			}




		} else {
			//print ("Hmm, nav nekaa ko dzeest, :\\  ");
		}

	}

	public override void PlacedInPlacer(){}
	public override void RemovedFromPlacer(){}
	//public override void PlaceOnGrid(int mode){}
	public override void RemovedFromGrid(){}
	public override void TouchedAnother(){}
	public override void StopTouchedAnother(){}
	public override void InitFromString(string str){}
	public override string InitToString(){return "";}

	
}
