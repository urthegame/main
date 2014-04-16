using UnityEngine;
using System.Collections;

public class Gui : MonoBehaviour {

	public bool QueryMode; //inforiiks sho uzsetos, lai GUIskripts raadiita vinja sagatavoto info
	public Levelobject QueryTarget; //inforiika apskataamais levelobjets

	private GUIStyle skin;
	private Level levelscript; //viens vieniigais Liimenja paarvaldniekskripts
	private GlobalResources gResScript; //globaalo resursu pieskatiitaaajs, arii singltons :P
    private Camctrl camerascript;

   
	private int vert;

   

	public void Awake(){
		levelscript = GameObject.Find("Level").GetComponent<Level>(); //no-bullshit singleton
		gResScript  = GameObject.Find("Level").GetComponent<GlobalResources>(); //no-bullshit singleton
        camerascript  = GameObject.Find("Camera").GetComponent<Camctrl>(); //no-bullshit singleton

		Init ();
	}

	//lietas, ko vajag resetot/peistarteet ielaadeejot liimeni (ar singltoniem viss buus kaartiibaa)
	public void Init(){
		QueryMode = false;
	}

	public void Update(){
        mouse();
		keyboard();

      
   	}

    void mouse(){


        if(IsMouseOverGui()) { 
            return;
        }


        
        if(Input.GetMouseButtonDown(1)) { // 0 => klik rait


            Levelobject room = levelscript.roomAtThisPosition(levelscript.lastPos.x,levelscript.lastPos.y);
            if(room != null){


                if(!QueryMode){
                    //pieseivo speeles aatrumu un kameras poziiciju, tikai ja neesam jau iekshaa zuumaa (saglabaaju peedeejos lietotaaja izveeleetos parametrus, pat vairaaku seciigu zuumu gadiijumaa)
                    levelscript.TimeScaleHistoric = levelscript.TimeScale;
                    camerascript.LastUserCamPos = camerascript.transform.position;
                } else {
                    stopQueryMode();
                }

                QueryMode = true;
                QueryTarget = room.GetComponent<Levelobject>();
                camerascript.ZoomToRoom(room.transform.position.x,room.transform.position.y);
                levelscript.TimeScale = 0.25f; // paleenina aatrumu

            } else {
                if(QueryMode) { //ja nav atrasta telpa un ieprieksh bija , tikai tad ir nepiecieshams noresetot (citaadi noreseto kameras poziiciju, kas vispaar nav uzsetota un ir slikti )
                    stopQueryMode();
                }
            }
        }

    }

    void stopQueryMode(){
        levelscript.emptyPlacer();
        QueryMode = false;
        QueryTarget = null;
        camerascript.UnZoomFromRoom();
        levelscript.TimeScale = levelscript.TimeScaleHistoric; //atjauno aatrumu
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
            stopQueryMode();
		}
		

        if (Input.GetKeyDown(KeyCode.KeypadPlus)){
            levelscript.AddAgent();
        }
        
        if (Input.GetKeyDown(KeyCode.KeypadMinus)){
            levelscript.RemAgent();
        }
        

		
	}
	


	void OnGUI () {


        int left = 20;
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

		vert += 53;
        GUI.Box(new Rect(15,vert,90,22), string.Format("Speed: {0:0.00}x", levelscript.TimeScale));

        vert += 25;
        levelscript.TimeScale = GUI.HorizontalSlider (new Rect (20,vert,80,height), levelscript.TimeScale, levelscript.TimeScaleMin, levelscript.TimeScaleMax); 
        

        
        
        vert += 25;
        if(GUI.Button(new Rect(20,vert,35,height),new GUIContent("S", "Save") )) {
			levelscript.SaveLevel("");
		}
        if(GUI.Button(new Rect(65,vert,35,height), new GUIContent("L", "Load"))) {
			levelscript.LoadLevel("");
		}

		vert += height + vSpace;
        if(GUI.Button(new Rect(20,vert,35,height), new GUIContent("X", "Destruction tool"))) {
			levelscript.PutObjInPlacer("delete-1");
		}
        if(GUI.Button(new Rect(65,vert,35,height), new GUIContent("?", "Query tool"))) {
			levelscript.PutObjInPlacer("query-1");
		}

        vert += height + vSpace;
        float w4 = 20;
        float w4Dist = 2;
        int w4Num = 0;
        if(GUI.Button(new Rect(17+ (w4Dist+w4)*w4Num++,vert,w4,height), new GUIContent("1", "1x1"))) {
            levelscript.PutObjInPlacer("corridor-11");
        }        
        if(GUI.Button(new Rect(17+(w4Dist+w4)*w4Num++,vert,w4,height), new GUIContent("2", "2x1"))) {
            levelscript.PutObjInPlacer("corridor-21");
        }
        if(GUI.Button(new Rect(17+(w4Dist+w4)*w4Num++,vert,w4,height), new GUIContent("3", "3x1"))) {
            levelscript.PutObjInPlacer("corridor-31");
        }
        if(GUI.Button(new Rect(17+(w4Dist+w4)*w4Num++,vert,w4,height), new GUIContent("4", "4x1"))) {
            levelscript.PutObjInPlacer("corridor-41");
        }

        vert += height + vSpace;
        w4Num = 0;
        if(GUI.Button(new Rect(17+ (w4Dist+w4)*w4Num++,vert,w4,height), new GUIContent("1", "1x2"))) {
            levelscript.PutObjInPlacer("corridor-12");
        }        
        if(GUI.Button(new Rect(17+(w4Dist+w4)*w4Num++,vert,w4,height), new GUIContent("2", "2x2"))) {
            levelscript.PutObjInPlacer("corridor-22");
        }
        if(GUI.Button(new Rect(17+(w4Dist+w4)*w4Num++,vert,w4,height), new GUIContent("3", "3x2"))) {
            levelscript.PutObjInPlacer("corridor-32");
        }
        if(GUI.Button(new Rect(17+(w4Dist+w4)*w4Num++,vert,w4,height), new GUIContent("4", "4x2"))) {
            levelscript.PutObjInPlacer("corridor-42");
        }

        vert += height + vSpace;
        w4Num = 0;
        if(GUI.Button(new Rect(17+ (w4Dist+w4)*w4Num++,vert,w4,height), new GUIContent("1", "1x3"))) {
            levelscript.PutObjInPlacer("corridor-13");
        }        
        if(GUI.Button(new Rect(17+(w4Dist+w4)*w4Num++,vert,w4,height), new GUIContent("2", "2x3"))) {
           // levelscript.PutObjInPlacer("corridor-23");
        }
        if(GUI.Button(new Rect(17+(w4Dist+w4)*w4Num++,vert,w4,height), new GUIContent("3", "3x3"))) {
            //levelscript.PutObjInPlacer("corridor-33");
        }
        if(GUI.Button(new Rect(17+(w4Dist+w4)*w4Num++,vert,w4,height), new GUIContent("4", "4x3"))) {
           // levelscript.PutObjInPlacer("corridor-43");
        }

        vert += height + vSpace;
        w4Num = 0;
        if(GUI.Button(new Rect(17+ (w4Dist+w4)*w4Num++,vert,w4,height), new GUIContent("1", "1x4"))) {
            levelscript.PutObjInPlacer("corridor-14");
        }        
        if(GUI.Button(new Rect(17+(w4Dist+w4)*w4Num++,vert,w4,height), new GUIContent("2", "2x4"))) {
           // levelscript.PutObjInPlacer("corridor-24");
        }
        if(GUI.Button(new Rect(17+(w4Dist+w4)*w4Num++,vert,w4,height), new GUIContent("3", "3x4"))) {
           // levelscript.PutObjInPlacer("corridor-34");
        }
        if(GUI.Button(new Rect(17+(w4Dist+w4)*w4Num++,vert,w4,height), new GUIContent("4", "4x4"))) {
           // levelscript.PutObjInPlacer("corridor-44");
        }





		vert += height;


        //visaam pogaam ir shis tuultip (katrai savs teksts)
        GUI.Label (new Rect (20,vert+10,130,20), GUI.tooltip);

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

                    string offText = "OFF";
                    string onText = "on";
                if( QueryTarget.Working ){
                         offText = "off";
                         onText = "ON";
                    }

                    if(GUI.Button(new Rect(Screen.width - 10 - 130,65,35,20), offText)) {
                        QueryTarget.setWorkingStatus(false,true);
                    }
                    if(GUI.Button(new Rect(Screen.width - 10 - 130 + 45 ,65,35,20),onText)) {
                        QueryTarget.setWorkingStatus(true,true);
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