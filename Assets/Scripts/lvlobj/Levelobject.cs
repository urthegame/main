using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public abstract class Levelobject : MonoBehaviour {


	protected static GlobalResources gResScript; //globaalo resursu pieskatiitaaajs - singltons 
    protected static WorkManager workManagerScript;//darbinju pieskatiitaajs - arii singltons
	protected static Level levelscript; //viens vieniigais Liimenja paarvaldniekskripts

	public float ConstrTime = 1; //cik sekundes ilgi buuvee
	public float DestrTime = 1; //cik sekundes ilgi jauc nostii
	public int Price = 0; //cik eiras maksaa uzbuuveesahana

    [HideInInspector]
    public bool Constructing; //vai celj (nevis jauc nost)
    [HideInInspector]
    public bool Destructing; //vai tomeer jauc nost
    //[HideInInspector]
	public float ConstrPercent;  //cik % uzcelts


	//shii kluciisha praktiskaa funkcija:
	public enum FuncTypes {
		block, //noliekams bloks
		util, //nenoliekams bloks, info prasiitaajs, aaraa dzeeseejs vai kas cits
        ground, //ambience
	}


	//skripts pieder prefabam un shos parametrus nomaina manuaali caur inspektoru - izveidojot prefabu un pieshkjirot tam sho skriptu
	//**svariigi**
	public int SizeX = 1;
	public int SizeY = 1;
	public int SizeZ = 1;
	public string Prefabname = "kaads aizmirsis mani paarsaukt, ibio!";
	public FuncTypes FuncType;  //funkcionaalais tips; jaaizveelas no enuma



	//shie parametri tiks apreekjinaati
	public float OffsetX; // vai vinjam vajag pabiidiities par puslaucinju (vajag, ja vinjam ir paara skaitlis izmeeraa)
	public float OffsetY;

	

	protected Dictionary<GameObject,bool> currentCollisions = new Dictionary<GameObject,bool>(); //retardeets dikcionaars, kur glabaa visus levelobjektus ar ko shis levelobjekts kolidee (svariigs tikai KEY, ignoreeju VAL)



	/**
	 * svariigi, ka beerninji izsauc sho
	 */ 
	protected void baseInit(){ 
		OffsetX =  0.5f* ((SizeX+1f)%2f);
		OffsetY =  -0.5f* ((SizeY+1f)%2f);

		if(gResScript == null){
			//print("you know, this one time ...");
			gResScript  = GameObject.Find("Level").GetComponent<GlobalResources>(); 
            workManagerScript = GameObject.Find("Level").GetComponent<WorkManager>(); 
			levelscript = GameObject.Find("Level").GetComponent<Level>();
		}


		if(ConstrTime > 0){ 
            ConstrTime = 60 / ConstrTime; //lai buuveeshanas ilgums buutu sekundees (lai var reizinaat ar delta_time)
		}
		
		
		
		if(DestrTime > 0){ 
            DestrTime = 60 / DestrTime;	
		}
		


		

	}

	
	public abstract void PlacedInPlacer();
	public abstract void RemovedFromPlacer();

	public abstract void PlaceOnGrid(int mode); //mode:   0 = lietotaajs uzliek; 1 = seivgeima ielaades funkcija uzliek
	public abstract void RemovedFromGrid();
	
	//kolidee ar citu levelobjektu
	public abstract void TouchedAnother();
	
	//paarstaaj kolideet ar citu levelobjektu
	public abstract void StopTouchedAnother();
	
	//ielaadeejot liimeni te pados stringu, ko objekts atshifrees un uzsetots savus ieksheejos mainiigos  (skat implementaaciju LOBlock.cs failaa)
	public abstract void InitFromString(string str);
	
	//atgreziis svariigos ieksheejos mainiigos (skat implementaaciju LOBlock.cs failaa)
	public abstract string InitToString();






	void OnCollisionEnter(Collision collision) {
		if( collision.gameObject.layer != 9){ //apskata tikai koliizjas ar 9. slaanja objektiem (gatavie levelobjekti)
			return;
		}
		currentCollisions[collision.gameObject] = true;
		TouchedAnother();
	}
	void OnCollisionExit(Collision collision) {
		if( collision.gameObject.layer != 9){
			return;
		}
		currentCollisions.Remove(collision.gameObject);
		StopTouchedAnother();
	}


	





}
