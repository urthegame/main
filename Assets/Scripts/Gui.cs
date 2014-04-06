using UnityEngine;
using System.Collections;

public class Gui : MonoBehaviour {

	public bool QueryMode; //inforiiks sho uzsetos, lai GUIskripts raadiita vinja sagatavoto info
	public Levelobject QueryTarget; //inforiika apskataamais levelobjets

	private GUIStyle skin;
	private Level levelscript; //viens vieniigais Liimenja paarvaldniekskripts
	private GlobalResources gResScript; //globaalo resursu pieskatiitaaajs, arii singltons :P

	private int vert;

	public void Awake(){
		levelscript = GameObject.Find("Level").GetComponent<Level>(); //no-bullshit singleton
		gResScript  = GameObject.Find("Level").GetComponent<GlobalResources>(); //no-bullshit singleton

		Init ();
	}

	//lietas, ko vajag resetot/peistarteet ielaadeejot liimeni (ar singltoniem viss buus kaartiibaa)
	public void Init(){
		QueryMode = false;
	}

	public void Update(){
		keyboard();
	}

	
	void keyboard(){
		
		if (Input.GetKeyDown(KeyCode.Insert)){
			levelscript.FillWithGroundcubes();
		}
		
		
		
		if (Input.GetKeyDown(KeyCode.Slash)){
			levelscript.PutObjInPlacer("query-1");
		}
		if (Input.GetKeyDown(KeyCode.Delete)){
			levelscript.PutObjInPlacer("delete-1");
		}
		
		if (Input.GetKeyDown(KeyCode.Escape)){
			levelscript.emptyPlacer();
		}
		
		
		
	}
	


	void OnGUI () {

		int height = 20;
		int vSpace = 10;

		vert = 35;
		GUI.Box(new Rect(15,vert,90,22), string.Format("€: {0}",gResScript.Money));

		vert = 60;
		GUI.Box(new Rect(15,vert,90,50), string.Format("A: {0}/{1}\nE: {2}/{3}\nW: {4}/{5}",
		                                               gResScript.Generation[Res.air],gResScript.Usage[Res.air],
		                                               gResScript.Generation[Res.electricity],gResScript.Usage[Res.electricity],
		                                               gResScript.Generation[Res.water],gResScript.Usage[Res.water]
		                                               ) );
		vert += 60;


		
		if(GUI.Button(new Rect(20,vert,35,height), "S")) {
			levelscript.SaveLevel("");
		}
		if(GUI.Button(new Rect(65,vert,35,height), "L")) {
			levelscript.LoadLevel("");
		}
		vert += height + vSpace;
		if(GUI.Button(new Rect(20,vert,35,height), "X")) {
			levelscript.PutObjInPlacer("delete-1");
		}
		if(GUI.Button(new Rect(65,vert,35,height), "?")) {
			levelscript.PutObjInPlacer("query-1");
		}
		vert += height + vSpace;
		if(GUI.Button(new Rect(20,vert,80,height), "Air gen.")) {
			levelscript.PutObjInPlacer("air-gen-11");
		}

		vert += height + vSpace;
		if(GUI.Button(new Rect(20,vert,80,height), "Electr. gen.")) {
			levelscript.PutObjInPlacer("electr-gen-22");
		}
		vert += height + vSpace;
		if(GUI.Button(new Rect(20,vert,80,height), "Water gen.")) {
			levelscript.PutObjInPlacer("water-gen-12");
		}
        vert += height + vSpace;
        if(GUI.Button(new Rect(20,vert,80,height), "Bedroom 1")) {
            levelscript.PutObjInPlacer("bedroom-11");
        }
        vert += height + vSpace;
        if(GUI.Button(new Rect(20,vert,80,height), "Bedroom 2")) {
            levelscript.PutObjInPlacer("bedroom-21");
        }
        vert += height + vSpace;
        if(GUI.Button(new Rect(20,vert,80,height), "Canteen")) {
            levelscript.PutObjInPlacer("canteen-21");
        }
		vert += height + vSpace;
		if(GUI.Button(new Rect(20,vert,80,height), "Storage 1")) {
			levelscript.PutObjInPlacer("storage-11");
		}
		vert += height + vSpace;
		if(GUI.Button(new Rect(20,vert,80,height), "Storage 2")) {
			levelscript.PutObjInPlacer("storage-21");
		}	
		vert += height + vSpace;
		if(GUI.Button(new Rect(20,vert,80,height), "workshop")) {
			levelscript.PutObjInPlacer("workshop-21");
		}
		vert += height + vSpace;
		if(GUI.Button(new Rect(20,vert,80,height), "corridor 1")) {
			levelscript.PutObjInPlacer("corridor-11");
		}
		vert += height + vSpace;
		if(GUI.Button(new Rect(20,vert,80,height), "corridor 2")) {
			levelscript.PutObjInPlacer("corridor-21");
		}
		vert += height + vSpace;
		if(GUI.Button(new Rect(20,vert,80,height), "Stairwell 2")) {
			levelscript.PutObjInPlacer("stairwell-12");
		}
		vert += height + vSpace;
		if(GUI.Button(new Rect(20,vert,80,height), "Stairwell 3")) {
			levelscript.PutObjInPlacer("stairwell-13");
		}
		vert += height + vSpace;
		if(GUI.Button(new Rect(20,vert,80,height), "*ground*")) {
			levelscript.PutObjInPlacer("groundcube-1");
		}

		vert += height;

		//backgoundbox
		GUI.Box(new Rect(10,10,100,vert), "Ur");


		//-----------
		//infobloka (query) autputs, ja te paliks par sarezhgjiitu, tad vajadzees paartaisiit, lai katrs kluciitis taisa savu query GUI

		if(QueryMode){ //veel nekas netiek apskatiits
			GUI.Box(new Rect(Screen.width - 10 - 130,10,130,20), "?");

		}

		try{
			if(QueryMode){ //tagad tiek apskatiits

				GUI.Box(new Rect(Screen.width - 10 - 130,10,130,50), "?\n" + QueryTarget.name);

				if(QueryTarget is LOBlock){ //levelbloks (kam ir iesl./izsl.funkcionalitaate)

					LOBlock itIsActuallyLOBlock = (LOBlock)QueryTarget;

					if( itIsActuallyLOBlock.Working ){
						if(GUI.Button(new Rect(Screen.width - 10 - 130,65,80,20), "Turn off")) {
							itIsActuallyLOBlock.setWorkingStatus(false,true);
						}
					} else {
						if(GUI.Button(new Rect(Screen.width - 10 - 130,65,80,20), "Turn on")) {
							itIsActuallyLOBlock.setWorkingStatus(true,true);
						}
					}

				}
				



			}
		} catch {

			QueryMode = false; //apskataamais levelobjets tika izniicinaats inforiiku neinformeejot :(
		}


	}

	//vai peljuks atrodas virs kaada no shajaa skriptaa radiitajiem elementiem
	public bool IsMouseOverGui() {

		//kreisaa puse
		if(Input.mousePosition.x < 110 && Input.mousePosition.y > Screen.height - (vert+20) ){ //pagalam vienkaarshi, veelaak vajadzees praatiigaaku DRY risinaajumu
			return true;
		}

		//labaa puse
		if(QueryMode){
			if(Input.mousePosition.x >Screen.width - 140 && Input.mousePosition.y > Screen.height - 90 ){ //pagalam vienkaarshi, veelaak vajadzees praatiigaaku DRY risinaajumu
				return true;
			}
		}


		return false;
	}


}



/**
 * @todo -- praatiigs GUI veidotaajs nevis pikseljus defineet
 * @todo -- konsolideet GUIa pogas ar levelskripta hotkijiem 
 * @todo -- pareizs IsMouseOverGui()
 * 
 */ 