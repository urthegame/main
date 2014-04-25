using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Gui : MonoBehaviour {

    [HideInInspector]
    public bool QueryMode; 
    [HideInInspector]
    public BaseLevelThing QueryTarget; //inforiika apskataamais levelobjets

    //private GUIStyle skin;
    private Level levelscript; //viens vieniigais Liimenja paarvaldniekskripts
    private GlobalResources gResScript; //globaalo resursu pieskatiitaaajs, arii singltons :P
    private Camctrl camerascript;
    private WorkManager workManagerScript;
    private int vert;
    private int lastLevelobjectLookedAt;

    private float rightPlaqueHeight;
    private float leftPlaqueHeight;


//    private Dictionary<string,bool[]> gadgetPrefabs = new Dictionary<string, bool[]>(); //kaa sauc gadgeta prefabu | kaadaas telpaas tas var atrasties



    public void Awake() {
        levelscript = GameObject.Find("Level").GetComponent<Level>(); //no-bullshit singleton
        gResScript = GameObject.Find("Level").GetComponent<GlobalResources>(); //no-bullshit singleton
        workManagerScript = GameObject.Find("Level").GetComponent<WorkManager>(); 
        camerascript = GameObject.Find("Camera").GetComponent<Camctrl>(); //no-bullshit singleton


        Init();
    }

    //lietas, ko vajag resetot/peistarteet ielaadeejot liimeni (ar singltoniem viss buus kaartiibaa)
    public void Init() {
        QueryMode = false;
    }

    public void Update() {
        mouse();
        keyboard();

      
    }

    void mouse() {


        if(IsMouseOverGui()) { 
            return;
        }

        if(Input.GetMouseButtonDown(1)) { // 0 => klik rait



            BaseLevelThing levelobject = levelscript.LastHoverObject; //LastHoverObject uzsit levelskriptaa katraa kadraa

            if(levelobject != null && levelobject.GetType().ToString() == "Room" ){ //hoverotais objekts ir telpa
                Room mightBeGroundCube = (Room)levelobject;
                if(mightBeGroundCube.FuncType == FuncTypes.ground){ //shii telpa ir zemes kluciitis, shos ignoreejam - neljaujam hoverot
                    levelobject = null; 
                }
            }


            if(levelobject != null) {

                if(levelobject.GetHashCode() == lastLevelobjectLookedAt){ //atkaaroti spiezh labo peli uz vienas telpas, taatad grib izsleegt kverijreezhiimu
                    stopQueryMode();
                    lastLevelobjectLookedAt = -1;
                    return;
                }
                lastLevelobjectLookedAt = levelobject.GetHashCode();

                if(!QueryMode) {//ieprieksh nebija kverijrezhiims
                    //pieseivo speeles aatrumu un kameras poziiciju, tikai ja neesam jau iekshaa zuumaa (saglabaaju peedeejos lietotaaja izveeleetos parametrus, pat vairaaku seciigu zuumu gadiijumaa)
                    levelscript.TimeScaleHistoric = levelscript.TimeScale;
                    camerascript.LastUserCamPos = camerascript.transform.position;
                } else {
                    stopQueryMode();
                }

                QueryMode = true;
                QueryTarget = levelobject;       
             

                camerascript.ZoomToRoom(levelobject.transform.position.x, levelobject.transform.position.y - (levelobject.SizeY / 2f) + 1.5f); // centree uz punktu nvieniibas virs griidas 
                levelscript.TimeScale = 0.25f; // paleenina aatrumu                

            } else {
                if(QueryMode) { //ja nav nekas apskataams un ieprieksh bija , tikai tad ir nepiecieshams noresetot (citaadi noreseto kameras poziiciju, kas vispaar nav uzsetota un ir slikti )
                    stopQueryMode();
                }
            }
        }

    }

    void stopQueryMode() {
        levelscript.emptyPlacer();
        QueryMode = false;
        QueryTarget = null;
        camerascript.UnZoomFromRoom();
        levelscript.TimeScale = levelscript.TimeScaleHistoric; //atjauno aatrumu
    }
    
    void keyboard() {
        
        if(Input.GetKeyDown(KeyCode.Insert)) {
            levelscript.FillWithGroundcubes();
        }
        
        

        if(Input.GetKeyDown(KeyCode.Delete)) {
            levelscript.PutObjInPlacer("digg-1");
        }
        
        if(Input.GetKeyDown(KeyCode.Escape)) {          
            stopQueryMode();
        }
        

        if(Input.GetKeyDown(KeyCode.KeypadPlus)) {
            levelscript.AddAgent();
        }
        
        if(Input.GetKeyDown(KeyCode.KeypadMinus)) {
            levelscript.RemAgent();
        }
        

        
    }

    void OnGUI() {


        /**********************************menjucis**kreisajaa**puseee******************************/
         
        int left = 20;
        int height = 20;
        int vSpace = 10;
      


        vert = 35;
        GUI.Box(new Rect(15, vert, 90, 22), string.Format("$: {0}", gResScript.Money));

        vert = 60;
        GUI.Box(new Rect(15, vert, 90, 50), string.Format("A: {0}/{1}\nE: {2}/{3}\nW: {4}/{5}",
                                                       gResScript.Generation[Res.air], gResScript.Usage[Res.air],
                                                       gResScript.Generation[Res.electricity], gResScript.Usage[Res.electricity],
                                                       gResScript.Generation[Res.water], gResScript.Usage[Res.water]
        ));

        vert += 53;
        GUI.Box(new Rect(15, vert, 90, 22), string.Format("Speed: {0:0.00}x", levelscript.TimeScale));

        vert += 25;
        levelscript.TimeScale = GUI.HorizontalSlider(new Rect(20, vert, 80, height), levelscript.TimeScale, levelscript.TimeScaleMin, levelscript.TimeScaleMax); 
        

        
        
        vert += 25;
        if(GUI.Button(new Rect(20, vert, 35, height), new GUIContent("S", "Save"))) {
            levelscript.SaveLevel("");
        }
        if(GUI.Button(new Rect(65, vert, 35, height), new GUIContent("L", "Load"))) {
            levelscript.LoadLevel("");
        }

       
        vert += height + vSpace;
        if(GUI.Button(new Rect(20, vert, 35, height), new GUIContent("G1", "Toggle unity grid"))) {
            levelscript.gridUnity = !levelscript.gridUnity;
        }
        if(GUI.Button(new Rect(65, vert, 35, height), new GUIContent("G10", "Toggle decimal grid"))) {
            levelscript.gridDecimal = !levelscript.gridDecimal;
        }

        vert += height + vSpace;
        if(GUI.Button(new Rect(20, vert, 35, height), new GUIContent("Grond", "Ground"))) {
            levelscript.PutObjInPlacer("groundcube-1");
        }
        if(GUI.Button(new Rect(65, vert, 35, height), new GUIContent("Digg", "Delete ground cube (or any other stucture)"))) {
            levelscript.PutObjInPlacer("digg-1");
        }
       



        vert += height + vSpace;
        float w4 = 20;
        float w4Dist = 2;
        int w4Num = 0;
        if(GUI.Button(new Rect(17 + (w4Dist + w4) * w4Num++, vert, w4, height), new GUIContent("1", "1x1"))) {
            levelscript.PutObjInPlacer("corridor-11");
        }        
        if(GUI.Button(new Rect(17 + (w4Dist + w4) * w4Num++, vert, w4, height), new GUIContent("2", "2x1"))) {
            levelscript.PutObjInPlacer("corridor-21");
        }
        if(GUI.Button(new Rect(17 + (w4Dist + w4) * w4Num++, vert, w4, height), new GUIContent("3", "3x1"))) {
            levelscript.PutObjInPlacer("corridor-31");
        }
        if(GUI.Button(new Rect(17 + (w4Dist + w4) * w4Num++, vert, w4, height), new GUIContent("4", "4x1"))) {
            levelscript.PutObjInPlacer("corridor-41");
        }

        vert += height + vSpace - 7;
        w4Num = 0;
        if(GUI.Button(new Rect(17 + (w4Dist + w4) * w4Num++, vert, w4, height), new GUIContent("1", "1x2"))) {
            levelscript.PutObjInPlacer("corridor-12");
        }        
        if(GUI.Button(new Rect(17 + (w4Dist + w4) * w4Num++, vert, w4, height), new GUIContent("2", "2x2"))) {
            levelscript.PutObjInPlacer("corridor-22");
        }
        if(GUI.Button(new Rect(17 + (w4Dist + w4) * w4Num++, vert, w4, height), new GUIContent("3", "3x2"))) {
            levelscript.PutObjInPlacer("corridor-32");
        }
        if(GUI.Button(new Rect(17 + (w4Dist + w4) * w4Num++, vert, w4, height), new GUIContent("4", "4x2"))) {
            levelscript.PutObjInPlacer("corridor-42");
        }

        vert += height + vSpace - 7;
        w4Num = 0;
        if(GUI.Button(new Rect(17 + (w4Dist + w4) * w4Num++, vert, w4, height), new GUIContent("1", "1x3"))) {
            levelscript.PutObjInPlacer("corridor-13");
        }        
        if(GUI.Button(new Rect(17 + (w4Dist + w4) * w4Num++, vert, w4, height), new GUIContent("2", "2x3"))) {
            // levelscript.PutObjInPlacer("corridor-23");
        }
        if(GUI.Button(new Rect(17 + (w4Dist + w4) * w4Num++, vert, w4, height), new GUIContent("3", "3x3"))) {
            //levelscript.PutObjInPlacer("corridor-33");
        }
        if(GUI.Button(new Rect(17 + (w4Dist + w4) * w4Num++, vert, w4, height), new GUIContent("4", "4x3"))) {
            // levelscript.PutObjInPlacer("corridor-43");
        }

        vert += height + vSpace - 7;
        w4Num = 0;
        if(GUI.Button(new Rect(17 + (w4Dist + w4) * w4Num++, vert, w4, height), new GUIContent("1", "1x4"))) {
            levelscript.PutObjInPlacer("corridor-14");
        }        
        if(GUI.Button(new Rect(17 + (w4Dist + w4) * w4Num++, vert, w4, height), new GUIContent("2", "2x4"))) {
            // levelscript.PutObjInPlacer("corridor-24");
        }
        if(GUI.Button(new Rect(17 + (w4Dist + w4) * w4Num++, vert, w4, height), new GUIContent("3", "3x4"))) {
            // levelscript.PutObjInPlacer("corridor-34");
        }
        if(GUI.Button(new Rect(17 + (w4Dist + w4) * w4Num++, vert, w4, height), new GUIContent("4", "4x4"))) {
            // levelscript.PutObjInPlacer("corridor-44");
        }



        vert += height + vSpace;
        if(GUI.Button(new Rect(20, vert, 35, height), new GUIContent("B1", "Single Bed"))) {
            levelscript.PutObjInPlacer("bed-single");
        }
        if(GUI.Button(new Rect(65, vert, 35, height), new GUIContent("B2", "Two storey Bed"))) {
            levelscript.PutObjInPlacer("bed-stacked");
        }
        vert += height + vSpace;
        if(GUI.Button(new Rect(20, vert, 35, height), new GUIContent("St1", "Fancy Stairwell"))) {
            levelscript.PutObjInPlacer("stairwell-fancy");
        }
        if(GUI.Button(new Rect(65, vert, 35, height), new GUIContent("St2", "Simple Stairwell"))) {
            levelscript.PutObjInPlacer("stairwell-simple");
        }



        vert += height;


      

        //backgoundbox
        GUI.Box(new Rect(10, 10, 100, vert), "Ur");


    

        /**********************************menjucis**labajaaa**puseee******************************/
        try {
            if(QueryMode) { //tagad tiek apskatiits

                if(QueryTarget.GetType().ToString() == "Room"){
                    QueryRoomGUI();
                }
                if(QueryTarget.GetType().ToString() == "Gadget"){
                    QueryGadgetGUI();
                }
                if(QueryTarget.GetType().ToString() == "Agent"){
                    QueryAgentGUI();
                }


            }
            
        } catch {

            QueryMode = false; //apskataamais levelobjets tika izniicinaats inforiiku neinformeejot :(
        }




        /**********************************hoverojamaa**objekta**menjucis**viduu********************************/

        if(levelscript.LastHoverObject != null){
            string type = levelscript.LastHoverObject.GetType().ToString();

            left = Mathf.RoundToInt((Screen.width /2f) - 65);
            vert = 10;
            height = 35;
            GUI.Box(new Rect(left, vert, 130, height), "hovered "+ type +":\n" + levelscript.LastHoverObject.name);

        }
        




        
        //visaam pogaam ir shis tuultips (katrai savs teksts)
        GUI.Label(new Rect(Screen.width / 2f  , 15, 130, 60), GUI.tooltip);




    }


    public void QueryRoomGUI(){

        Room room = (Room)QueryTarget; //zinu, ka querytarget ir room klase

        int left = Screen.width - 10 - 130;
        vert = 10;
        rightPlaqueHeight = 125;
        GUI.Box(new Rect(left, vert, 130, rightPlaqueHeight), "?room?\n" + room.name);
        
        
        vert += 40;
        GUI.Box(new Rect(left+5, vert, 130-10, 55), string.Format("A: {0}/{1}\nE: {2}/{3}\nW: {4}/{5}",
                                                                  room.Generation[Res.air], room.Usage[Res.air],
                                                                  room.Generation[Res.electricity], room.Usage[Res.electricity],
                                                                  room.Generation[Res.water], room.Usage[Res.water]
                                                                  ));
        vert += 60;
        
        string offText = "OFF";
        string offTooltipText = "Turn Room off; currently not working";
        string onText = "on";
        string onTooltipText = "Turn Room on; currently not working";
        if(room.Working) {
            offText = "off";
            offTooltipText = "Turn Room off; currently working";
            onText = "ON";
            onTooltipText = "Turn Room on; currently working";
        }
        
        
        
        if(GUI.Button(new Rect(left+5, vert, 35, 20), new GUIContent(offText, offTooltipText))) {
            room.setWorkingStatus(false, true);
        }
        if(GUI.Button(new Rect(left + 45+5, vert, 35, 20), new GUIContent(onText, onTooltipText))) {
            room.setWorkingStatus(true, true);
        }
        
        
        if(GUI.Button(new Rect(left + 5 + 90, vert, 30, 20), new GUIContent("X", "Remove room"))) {
            room.RemovedFromGrid();
        }  
        

        
        vert += 30;


        string worklist = "";
        int numlines = 0;
        foreach(WorkUnit w in workManagerScript.worklist) {
            if(w.parentRoom != room) { //skataas tikai darbus, kas pieder shai telpai
                continue;
            }

            if(w.on) {  //vai iesleegts
                worklist += "+ ";
            } else {
                worklist += "- ";
            }

            worklist += "@" + w.parentRoom.name; //kuraa telpa atrodas
           
            if(w.parentGadget != null) { //opcinaaali - gadzhets, kam pieder darbs
                worklist += "\n  " + w.parentGadget.name;
                numlines++;
            }

            worklist += "\n  " + w.WorkUnitTypeNumber.ToString();

            if(w.agentWorkingOn != null) { //agjents kursh straadaa
                worklist += "\n  " + w.agentWorkingOn.name;
                numlines++;
            }



            worklist += "\n\n";
            numlines += 2;
        }
        
        GUI.Box(new Rect(left+5, vert, 130-10, 25+numlines*15), string.Format("Worklist: \n"+worklist));
        

    }
    public void QueryGadgetGUI(){
        Gadget gadget = (Gadget)QueryTarget; //zinu, ka querytarget ir Gadget klase

        int left = Screen.width - 10 - 130;
        vert = 10;
        rightPlaqueHeight = 125;
        GUI.Box(new Rect(left, vert, 130, rightPlaqueHeight), "?gadget?\n" + gadget.name);
        
        vert += 40;
        GUI.Box(new Rect(left+5, vert, 130-10, 55), string.Format("A: {0}/{1}\nE: {2}/{3}\nW: {4}/{5}",
                                                                  gadget.Generation[Res.air], gadget.Usage[Res.air],
                                                                  gadget.Generation[Res.electricity], gadget.Usage[Res.electricity],
                                                                  gadget.Generation[Res.water], gadget.Usage[Res.water]
                                                                  ));
        vert += 60;
       
        
        if(GUI.Button(new Rect(left + 5 + 90, vert, 30, 20), new GUIContent("X", "Remove gadget"))) {
            gadget.RemovedFromGrid();
        }  
        
        
        vert += 30;
        
        
        
        string worklist = "";
        int numlines = 0;
        foreach(WorkUnit w in workManagerScript.worklist) {
            if(w.parentGadget != gadget) { //skataas tikai darbus, kas pieder shim gadzhetams
                continue;
            }
            
            if(w.on) {  //vai iesleegts
                worklist += "+ ";
            } else {
                worklist += "- ";
            }
            
            worklist += "@" + w.parentRoom.name; //kuraa telpa atrodas
            

            worklist += "\n  " + w.parentGadget.name;
            numlines++;

            
            worklist += "\n  " + w.WorkUnitTypeNumber.ToString();
            
            if(w.agentWorkingOn != null) { //agjents kursh straadaa
                worklist += "\n  " + w.agentWorkingOn.name;
                numlines++;
            }
            
            
            
            worklist += "\n\n";
            numlines += 2;
        }
        
        GUI.Box(new Rect(left+5, vert, 130-10, 25+numlines*15), string.Format("Worklist: \n"+worklist));


    }

    public void QueryAgentGUI(){
        Agent agent = (Agent)QueryTarget; //zinu, ka querytarget ir Agent klase
        

        int left = Screen.width - 10 - 130;
        vert = 10;
        rightPlaqueHeight = 65;
        GUI.Box(new Rect(left, vert, 130, rightPlaqueHeight), "?agent?\n" + agent.name);
        
        
        vert += 40;
      

        
        
        
        if(GUI.Button(new Rect(left + 5 + 90, vert, 30, 20), new GUIContent("X", "Remove agent"))) {
            agent.RemovedFromGrid();
        }  
        
        
        
        vert += 30;
        
        
        
        string worklist = "";
        int numlines = 0;
        foreach(WorkUnit w in workManagerScript.worklist) {
            if(w.agentWorkingOn != agent) { //skataas tikai darbus, ko dara shis agjents
                continue;
            }
            
            if(w.on) {  //vai iesleegts
                worklist += "+ ";
            } else {
                worklist += "- ";
            }
            
            worklist += "@" + w.parentRoom.name; //kuraa telpa atrodas
            
            if(w.parentGadget != null) { //opcinaaali - gadzhets, kam pieder darbs
                worklist += "\n  " + w.parentGadget.name;
                numlines++;
            }
            
            worklist += "\n  " + w.WorkUnitTypeNumber.ToString();
           

            worklist += "\n\n";
            numlines += 2;
        }
        
        GUI.Box(new Rect(left+5, vert, 130-10, 25+numlines*15), string.Format("Worklist: \n"+worklist));

    }
    
    //vai peljuks atrodas virs kaada no shajaa skriptaa radiitajiem elementiem
    public bool IsMouseOverGui() {

        //kreisaa puse
        if(Input.mousePosition.x < 110 && Input.mousePosition.y > Screen.height - (vert + 20)) { //pagalam vienkaarshi, veelaak vajadzees praatiigaaku DRY risinaajumu
            return true;
        }

        //labaa puse
        if(QueryMode) {
            if(Input.mousePosition.x > Screen.width - 140 && Input.mousePosition.y > Screen.height - rightPlaqueHeight) { //pagalam vienkaarshi, veelaak vajadzees praatiigaaku DRY risinaajumu
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