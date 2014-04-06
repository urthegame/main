using UnityEngine;
using System.Collections;

/**
 * neizraktas zemiites pildiijums
 */ 
public class LOGround : Levelobject {



	public void Awake(){
		baseInit();

	}


	public void Update(){

		if(Destructing){ //jauc nostiim
			ConstrPercent -= DestrTime * Time.deltaTime;
			if(ConstrPercent <= 0){
				Destroy(transform.gameObject); //aizvaac sho kluciiti no liimenja
			}
		}

	}


	public override void PlaceOnGrid(int mode){
		GameObject level = GameObject.Find("LevelobjectHolder");
		
		
		
		if(currentCollisions.Count > 0 ){
			print ("She nedriikst likt bloka !");
		} else {
			//print ("Bloks tiek novietots uz laukuma (tochna) ");
			transform.parent = level.transform; //novieto liimenjobjektu konteinerii, kur dziivo liimenim piederoshie objekti		
			transform.gameObject.layer = 9; //levelobjektu leijeris
			Destroy(transform.gameObject.GetComponent<Rigidbody>()); // kameer levelobjekts ir PLEISHOLDERII, tam pieder rigidbodijs, lai var koliizijas kolideet, nu tas vairs nav nepiecieshams
			ConstrPercent = 100; //100% = pilniibaa pebaigts zemes kluciitiis	
		}
		
	}

	// tiek dzeests aaraa - vajag leenaam izdzeesties
	public override void RemovedFromGrid(){
		Destructing = true; //saakam jaukt nost
	}


	public override void PlacedInPlacer(){}
	public override void RemovedFromPlacer(){}
//	public override void RemovedFromGrid(){}
	public override void TouchedAnother(){}
	public override void StopTouchedAnother(){}
	public override void InitFromString(string str){}
	public override string InitToString(){return "";}

}
