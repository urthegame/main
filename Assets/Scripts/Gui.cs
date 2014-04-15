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
        mouse();
		keyboard();

      
   	}

    void mouse(){


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

        
        vert += height + vSpace;
        if(GUI.Button(new Rect(20,vert,80,height), new GUIContent("Base", "Triple Base unit"))) {
            levelscript.PutObjInPlacer("base-31");
        }

		vert += height + vSpace;
        if(GUI.Button(new Rect(left,vert,23,height), new GUIContent("A", "Air generator"))) {
			levelscript.PutObjInPlacer("air-gen-11");
		}       	

        if(GUI.Button(new Rect(left+28,vert,23,height), new GUIContent("E", "Electricity generator"))) {
            levelscript.PutObjInPlacer("electr-gen-22");
        }           

        if(GUI.Button(new Rect(left+58,vert,23,height), new GUIContent("W", "Water generator"))) {
            levelscript.PutObjInPlacer("water-gen-12");
        }           

        

        vert += height + vSpace;
        if(GUI.Button(new Rect(20,vert,35,height), new GUIContent("B1", "Single Bedroom"))) {
            levelscript.PutObjInPlacer("bedroom-11");
        }

        if(GUI.Button(new Rect(65,vert,35,height), new GUIContent("B2", "Double Bedroom"))) {
            levelscript.PutObjInPlacer("bedroom-21");
        }



        vert += height + vSpace;
        if(GUI.Button(new Rect(20,vert,80,height), new GUIContent("Canteen", "Canteen"))) {
            levelscript.PutObjInPlacer("canteen-21");
        }


        vert += height + vSpace;
        if(GUI.Button(new Rect(20,vert,35,height), new GUIContent("S1", "Single Storage"))) {
            levelscript.PutObjInPlacer("storage-11");
        }
        
        if(GUI.Button(new Rect(65,vert,35,height), new GUIContent("S2", "Double Storage"))) {
            levelscript.PutObjInPlacer("storage-21");
        }




		vert += height + vSpace;
        if(GUI.Button(new Rect(20,vert,80,height),  new GUIContent("Workshop", "Workshop"))) {
			levelscript.PutObjInPlacer("workshop-21");
		}

        vert += height + vSpace;
        if(GUI.Button(new Rect(left,vert,25,height), new GUIContent("C1", "Single Corridor"))) {
            levelscript.PutObjInPlacer("corridor-11");
        }           
        
        if(GUI.Button(new Rect(left+30,vert,26,height), new GUIContent("C2", "Double Corridor"))) {
            levelscript.PutObjInPlacer("corridor-21");
        }           
        
        if(GUI.Button(new Rect(left+60,vert,26,height), new GUIContent("C3", "Triple Corridor"))) {
            levelscript.PutObjInPlacer("corridor-31");
        }           




        vert += height + vSpace;
        if(GUI.Button(new Rect(left,vert,25,height), new GUIContent("S1", "Single storey modular Stairwell"))) {
            levelscript.PutObjInPlacer("stairwell-11");
        }           
        
        if(GUI.Button(new Rect(left+30,vert,26,height), new GUIContent("S2", "Two storey Stairwell"))) {
            levelscript.PutObjInPlacer("stairwell-12");
        }           
        
        if(GUI.Button(new Rect(left+60,vert,26,height), new GUIContent("S3", "Three storey Stairwell"))) {
            levelscript.PutObjInPlacer("stairwell-13");
        }           




        vert += height + vSpace;
        if(GUI.Button(new Rect(20,vert,35,height), new GUIContent("G1", "Ground cube"))) {
            levelscript.PutObjInPlacer("groundcube-1");
        }
        
        if(GUI.Button(new Rect(65,vert,35,height), new GUIContent("GF", "Flat Ground"))) {
            levelscript.PutObjInPlacer("groundcube-flat-1");
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