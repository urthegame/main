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

	// tiek dzeests aaraa
	public override void RemovedFromGrid(){
		Destructing = true; //saakam jaukt nost

        //jaaizveido worknodes, lai no blakusesoshaam telpaam vareetu piekljuut un veikt darbu
        /*
        GameObject prefab = Resources.Load("worknode") as GameObject; 
        GameObject node = Instantiate(
            prefab, 
            new Vector3(transform.position.x+0.75f, transform.position.y, 0),
            Quaternion.identity) as GameObject;
        node.name = "worknode - right";
        node.transform.parent = transform;

        node = Instantiate(
            prefab, 
            new Vector3(transform.position.x-0.75f, transform.position.y, 0),
            Quaternion.identity) as GameObject;
        node.name = "worknode - left";
        node.transform.parent = transform;

*/
       

	}


	public override void PlacedInPlacer(){}
	public override void RemovedFromPlacer(){}
//	public override void RemovedFromGrid(){}
	public override void TouchedAnother(){}
	public override void StopTouchedAnother(){}
	public override void InitFromString(string str){}
	public override string InitToString(){return "";}

}
