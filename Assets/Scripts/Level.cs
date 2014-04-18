using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class LevelLimits {
    //liimenja izmeers - kur var likt kluciishus
    public int XA = -15;
    public int XB = 15;
    public int YA = -15;
    public int YB = 0;
}

public class Level : MonoBehaviour {

    public bool DebugDrawNavgrid = true;
    public Dictionary<Vector4,float> Navgrid = new Dictionary<Vector4, float>(); //cik maksaa (un vai vispaar ir iespeejams) celjs no x,y uz citu x,y (shie 4 xyxy tiek iekodeeeti vector4, aspraatiigi, es zinu )
    public List<Room> ListOfRooms; //saraksts ar visiem liimenii esoshajiem leveloblokiem jeb telpam

    private GameObject roomHolder;
    private GameObject agentHolder;
    private GameObject gadgetHolder;
    private GameObject destroyHolder;
    private GameObject placer;
    private Gui guiscript; //vieniigais/globalais GUI skripts
    private GlobalResources gResScript; //globaalo resursu pieskatiitaaajs, arii singltons :P
    private WorkManager workManagerScript;//singltons 
    private Dictionary<Vector4, List<Vector2>> pathCache = new Dictionary<Vector4, List<Vector2>>(); //piekeshos katru atrasto celju, notiiriis paarziimeejot navgridu


    [HideInInspector]
    public Vector3 LastPosGrid;
    private Vector3 LastPosGridPrecise;
    [HideInInspector]
    public Vector3 LastPosDecGrid;
    private Vector3 LastPosDecGridPrecise;
    [HideInInspector]
    public bool objectInPlacer = false; //pleiseris ir konteineris, ko biida apkaart ar peli - priekshskatiijuma versija
    [HideInInspector]
    public bool gadgetEditMode; //ja TRUE, tad redigjee levelobjektos esoshos priekshmetus, ja FALSE tad redigjee pashus levelobjektus
    [HideInInspector]
    public Room lastRoomTargeted; //telpa kuraa tiek pirkti gadzheti - sho maina GUIskriptss

    private int numRooms = 0; //tikai aptuveni apjomi, lieto unikaaliem nosakumiem, neivs patiesai apjoma ntoeikshanai
    private int numAgents = 0;
    private int numGadgets = 0;
    private Dictionary<string,GameObject> prefabCache = new Dictionary<string,GameObject>(); // lai katru unikaalo prefabu ielaadeetu tikai vienreiz

    public LevelLimits limits; //liimenjrobezhas

    [HideInInspector]
    public float TimeScale = 1.0f;
    [HideInInspector]
    public float TimeScaleMin = .0000001f; //MIN nedriikst buut nulle - jo kameras kustiiba tiek kompenseeta atkariibaa no timescale un 0 gadiijumaa buus kosmoss
    [HideInInspector]
    public float TimeScaleMax = 5.0f;
    [HideInInspector]
    public float TimeScaleHistoric; //dazhreiz piespiedu kaartaa mainu aatrumu un te pieseivoju ieprieksheejo
    // lai kustinaatu objektus neatkariigi no speeles aatruma  delta time ir jaaizdala ar tiemscale:
    // (Time.deltaTime / levelscript.TimeScale) 


    public bool gridUnity; //vai ziimeet vieniibas gridu - levelobjektiem, ziimee plaknee, kas atrodas pretii kamerai
    public bool gridDecimal; //vai ziimeet decmaalgridu  - objektiem telpaa, ziimee uz telpas griidas
    public float gridDecimalY;  //kuraa staavaa, Y koordinaate, ziimet decimaalgridu | sho apdeito GUI skripts
    private float GridDecimalScale = 0.1f; //cik liels ir decimaalgrida ruutojums 

    
    void Start() {

        TimeScaleHistoric = TimeScale;

        roomHolder = GameObject.Find("LevelobjectHolder");
        agentHolder = GameObject.Find("AgentHolder");
        gadgetHolder = GameObject.Find("GadgetHolder");
        destroyHolder = GameObject.Find("DestroyHolder");
        placer = GameObject.Find("Placer");
        guiscript = GameObject.Find("GUI").GetComponent<Gui>();
        gResScript = GameObject.Find("Level").GetComponent<GlobalResources>(); 
        workManagerScript = GameObject.Find("Level").GetComponent<WorkManager>(); 

    }
    
    void Update() {
        drawGrid();
        mouse();

        if(DebugDrawNavgrid) {
            drawPaths();
        }


        Time.timeScale = TimeScale; //maina speeles aatrumu atkariibaa no GUI skriptinjaa esoshaa slaidera veertiibas

        
        //Debug.Log("There are " + FindObjectsOfType(typeof(GameObject)).Length + " gameObjects in your scene");
    }

   
    /**
     * ielaadee prefabu un paliek zem PLACER objekta, lai biidiidu apkaart kopaa ar PLEISERA objektu
     */ 
    public void PutObjInPlacer(string name) {

        emptyPlacer();
        GameObject prefab = loadLevelobjectPrefab(name);

        GameObject levelobject = Instantiate(
            prefab, 
            Vector3.one,
            Quaternion.identity) as GameObject;

    
        BaseLevelThing script = levelobject.GetComponent<BaseLevelThing>(); //objekts instanceeets un skriptaa varu apskatiities apreekjinaatos offsetus

        //jaapadod ir globaalaa poziicija, taapeec jaaliek tieshi zem PLEISERA, tad shis prefabs ieguus savu lokaalo poziiciju 0,0,0  => tieshi tur, kur vecaaks
        levelobject.transform.position = new Vector3(placer.transform.position.x + script.OffsetX,
                                                 placer.transform.position.y + script.OffsetY,
                                                0); 
        //offset_: poziicijaam x un y pieshauj 0,5, ja izmeers ir paara skaitlis(skat komentaaru faila apakshaa)

        levelobject.transform.parent = placer.transform; 
        levelobject.name = script.Prefabname + " " + (numRooms + 1);
        //levelobject.GetComponent<levelobject>().klucha_tips = "njigunjegu";  //@todo


        // [tikai] kameer objekts atrodas PLEISERII, tam ir rigidbody komponente (lai var koliizijas detekteet ar jau esoshajiem levelobjektiem)
        levelobject.AddComponent<Rigidbody>();
        levelobject.rigidbody.constraints = RigidbodyConstraints.FreezeAll; //lai nelido pa gaidu (true story)

        BoxCollider collider = levelobject.GetComponent<BoxCollider>();
        collider.size = new Vector3(collider.size.x * 0.99f, collider.size.y * 0.99f, collider.size.z * 0.99f); // ja kolaidera izmeers ir 1, tad tiek maldiigi reporteetas koliizijas ar blakus esoshu kolaideru | kolaideru izmeeri ir dazhaadi, taapeec reizinu ar .99 nevis uzsetoju .99

        levelobject.layer = 11; //pleisholdera levelobjektu leijeris
        objectInPlacer = true;
        script.PlacedInPlacer();//pazinjoju levelobjektam, ka tas tiek ielikts PLEISERII

        //print (" pos @create " + levelobject.transform.position + "  " + levelobject.transform.name);


    }

    /**
     * aizvaac visus PLEISERA beerninjus
     */ 
    public void emptyPlacer() {
        foreach(Transform childTransform in placer.transform) {
            //placer
            childTransform.GetComponent<BaseLevelThing>().RemovedFromPlacer(); //pazinjoju levelobjektam, ka tas tiek aizvaakts no PLEISERA
            Destroy(childTransform.gameObject);
        }
        objectInPlacer = false;
        gadgetEditMode = false; //vienmeer sho rezhiimu atcelj, iesleegs tikai, ja ievietos pleiserii gadzhetu
    }
    
    void mouse() {

        if(guiscript.IsMouseOverGui()) { //kursors atrodas uz HUDa,nevajag uzbaazties ar saviem kluchiem
            return;
        }
    
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

       
        //vienmeer cheko poziiciju pret vieniibas gridu 
        if(Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 8)) { // 1<<8 ir 8.slaanja bitmaska - neredzama plakne, kur lipinaat klaati limenobjektus
            int roundx = Mathf.RoundToInt(hit.point.x);
            int roundy = Mathf.RoundToInt(hit.point.y);
            LastPosGrid = new Vector3(roundx, roundy, 0); 
            LastPosGridPrecise = new Vector3(hit.point.x, hit.point.y, 0); 
            placer.transform.position = LastPosGrid;
        }

        //dazhreiz tikai cheko poziiciju pret decimaalgridu
        if(gadgetEditMode) {//gadzhetu redigjeeshanas redzhiiims
            if(Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 10)) { // 10. slaanis ir neredzama plakne FloorBasePlane | GUI skripts to noliek apskataamaa levelobjekta griidas augstumaa
                float roundx = Mathf.RoundToInt(hit.point.x / GridDecimalScale) * GridDecimalScale + GridDecimalScale / 2f;//x un z asiim pieskaita offsetu - pus ruutinju - lai atrastos grida ruutinjas viduu
                float roundy = Mathf.RoundToInt(hit.point.y / GridDecimalScale) * GridDecimalScale; //Y nedod offsetu, jo tam ir jaaatrodas mazliet virs zemes (telpas griidas biezums)
                float roundz = Mathf.RoundToInt(hit.point.z / GridDecimalScale) * GridDecimalScale + GridDecimalScale / 2f;

                /**
                 * te jaaapskataas vai gadzhets nelien aarpus robezhaam (njemot veeraa gadzheta izmeerus)
                 * gan uz blakus telpaam gan uz priekshu/aizmuguri
                 * 
                 * ja poziicija nav OK, tad neapdeitot LastPosDecGrid
                 */ 

                LastPosDecGrid = new Vector3(roundx, roundy, roundz); 
                LastPosDecGridPrecise = new Vector3(hit.point.x, hit.point.y,hit.point.z); 


            }



            placer.transform.position = LastPosDecGrid;
        }
        


        if(Input.GetMouseButtonDown(0)) { // 0 => klik left 
            //  print (lastPos);

            if(objectInPlacer) { //PLEISERII ir ielaadeeds prefabs
                Transform levelobject = placer.transform.GetChild(0); //pienjemu, ka tikai viens objekts tiek likts vienlaiciigi
                BaseLevelThing script = levelobject.GetComponent<BaseLevelThing>(); //objekts instanceeets un skriptaa varu apskatiities apreekjinaatos offsetus

                if(script.GetType().ToString() == "Gadget"){
                    Gadget gadgetscript = (Gadget)script;
                    gadgetscript.parentRoom = lastRoomTargeted; //ja tiek likts gadzhets (nevis telpa) tad gadhetam pasaka kurai telpai tas pieder 
                }

                script.PlaceOnGrid(0); //katrs bloks pats izlems, ko dariit, kad tas tiek klikskshkinaats uz grida (ja bloks tiek novietots, tad tas tiks aizvaakts no PLEISERA)
                CalculateNavgrid();//kaut kas iespeejams ir mainiijies, jaaparreekjina

                if(placer.transform.childCount == 0) { 

                    objectInPlacer = false; //bloks tika novietots (vai aizvaakts or smnt) un nu PLEISERIS ir tukshss
                    numRooms++; //droshs paliek droshs - palielinaashu skaitiitaaju (pat ja kluciitis netika novietots liimenii)
                }


            }

        }

        if(Input.GetMouseButtonDown(1)) { //rait
            emptyPlacer();
        }


        if(Input.GetMouseButtonDown(2)) { // 2 => klik mid 
            print(LastPosGrid + ":" + LastPosGridPrecise + " deci: " + LastPosDecGrid + ":" + LastPosDecGridPrecise );
        }



    }
            
    void drawGrid() {
                
        if(gridUnity){ //vieniibas grids

            float go = 0.5f; //grid offset (objekti ir tiek pozicioneeti centraa, taapeec rezhgjis jaanobiida, yaddayaddayadda ... )
            Color color = new Color(0.8F, 0.8F, 0.8F, 0.9F);

            for(int x = limits.XA; x< limits.XB; x++) {
                for(int y = limits.YA; y< limits.YB; y++) {
                    Debug.DrawLine(new Vector3(x + go, y + go, -0.5f), new Vector3(x + go, y - 1 + go, -0.5f), color);
                    Debug.DrawLine(new Vector3(x + go, y + go, -0.5f), new Vector3(x - 1 + go, y + go, -0.5f), color);

                }
            }
        }

        if(gridDecimal){ //decimaalgrids

            
            float go = 0; //scale / 2f; //grid offset 
            Color color = new Color(0.8F, 0.8F, 0.8F, 0.9F);
            float Y = gridDecimalY;


            for(float x = limits.XA; x< limits.XB; x+=GridDecimalScale) {
                for(float z = -0.5f; z< 0.5f; z+=GridDecimalScale) {
                    Debug.DrawLine(new Vector3(x + go, Y, z+go), new Vector3(x + go, Y, z+go+GridDecimalScale), color);
                    Debug.DrawLine(new Vector3(x + go, Y, z+go), new Vector3(x + go-GridDecimalScale, Y, z+go), color);
                    
                }
            }
        }



    }

    public void SaveLevel(string levelname) {

        //webpleijerii nav failu rakstiisahanas iespeeja :\
        #if UNITY_STANDALONE_WIN 
       
      

        if(levelname == "") {
            levelname = "stuff-01";
        }

        int i = 0;
        string output = "";
        output += gResScript.Money + "\n"; //pirmaa rinda globaalaas lietass
        foreach(Transform lvlobj in roomHolder.transform) {

            BaseLevelThing script = lvlobj.GetComponent<BaseLevelThing>();
            string prefabname = script.Prefabname;
            string extraParams = script.InitToString(); //papildparametru strings, ja levelobjekta papildskipts ir pacenties taadus dabuut, ja ne tad tukssh strings

            //tips x y z \n
            output += prefabname + " " + lvlobj.position.x + " " + lvlobj.position.y + " " + lvlobj.position.z + extraParams + "\n";
            i++;
        }
        int numBlocks = i;

        i = 0;
        foreach(Transform lvlobj in agentHolder.transform) {
            BaseLevelThing script = lvlobj.GetComponent<BaseLevelThing>();
            string prefabname = script.Prefabname;
            string extraParams = script.InitToString(); 
            output += prefabname + " " + lvlobj.position.x + " " + lvlobj.position.y + " " + lvlobj.position.z + extraParams + "\n";
            i++;
        }
        int numAgents = i;

        i = 0;
        foreach(Transform lvlobj in gadgetHolder.transform) {
            BaseLevelThing script = lvlobj.GetComponent<BaseLevelThing>();
            string prefabname = script.Prefabname;
            string extraParams = script.InitToString(); 
            output += prefabname + " " + lvlobj.position.x + " " + lvlobj.position.y + " " + lvlobj.position.z + extraParams + "\n";
            i++;
        }
        int numGadgets = i;



        print("Saglabaati visas " + numBlocks + " telpas ar visiem " + numGadgets + " un " + numAgents + "agjentiem" );
        System.IO.File.WriteAllText("Levels/" + levelname + ".lvl", output); //nevar seivot límeni, ja ir webpleijera versija

        #endif
    }

    public void LoadLevel(string levelname) {
        #if UNITY_STANDALONE_WIN 

        if(levelname == "") {
            levelname = "stuff-01";
        }
    

        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();


        //dzeesh levelobjektus
        while(roomHolder.transform.childCount > 0){
            Transform childTransform = roomHolder.transform.GetChild(0);
            childTransform.transform.name = childTransform.transform.name+"+DELETE";
            childTransform.transform.parent = destroyHolder.transform; //jaaieliek pagaidu konteinerii, jo Destroy notiks tikai naakamajaa kadraa, bet man vajag iisto konteineri tukshu ljoti driiz - jo navgrida paarreekjinaataajs skatiisies tur esoshos objektus
            childTransform.transform.position = new Vector3(-1000,9999,9999);
            Destroy(childTransform.gameObject);
        }
        //dzeesh gadzhetus taadaa pashaa veidaa kaa telpas
        while(gadgetHolder.transform.childCount > 0){
            Transform childTransform = gadgetHolder.transform.GetChild(0);
            childTransform.transform.name = childTransform.transform.name+"+DELETE";
            childTransform.transform.parent = destroyHolder.transform; 
            childTransform.transform.position = new Vector3(-1000,9999,9999);
            Destroy(childTransform.gameObject);
        }

        //dzeesh agjentus
        foreach(Transform agent in agentHolder.transform) {
            Destroy(agent.gameObject);
        }

        numRooms = 0;
        gResScript.Init();
        guiscript.Init();
        workManagerScript.Init();


        string[] lvlLines = System.IO.File.ReadAllLines("Levels/" + levelname + ".lvl");
        if(lvlLines.Length > 0) {
            //pirmaa rinda ir globaalaas lietas
            string[] c = lvlLines[0].Split(' '); 
            gResScript.Money = int.Parse(c[0]);

        }

      
        //atlikushaas rindas ir levelobjekti | skip(1) ... burtiski noskipo pirmo elementu
        foreach(string l in lvlLines.Skip(1)) {
          
            string[] c = l.Split(' '); //sadala pa komponenteem:  nosaukums,x,y,z, + tonna ar objektiem specifiskiem parametriem
                        
            //print (l);
            string prefabName = c[0];
            float x = float.Parse(c[1]);
            float y = float.Parse(c[2]);
            float z = float.Parse(c[3]);
            //print (type + "=  " + x + ":" + y + ":" + z );


            GameObject prefab = loadLevelobjectPrefab(prefabName); 
            GameObject levelobject = Instantiate(
                prefab, 
                new Vector3(x, y, z),
                Quaternion.identity) as GameObject;
                        
            BaseLevelThing script = levelobject.GetComponent<BaseLevelThing>();





            if(script.GetType().ToString() == "Room"){ //peec skripta nosaku kaada veida prefabs vinhs ir un kur ir jaanoliek hierarhijaa
                levelobject.transform.parent = roomHolder.transform; 
                levelobject.name = script.Prefabname + " " + (numRooms + 1);
                levelobject.gameObject.layer = 9; //telpu leijeris -- paareejiem prefabiem nav nepiecieshams
                numRooms++;    

            } else if(script.GetType().ToString() == "Agent"){
                levelobject.transform.parent = agentHolder.transform; 
                levelobject.name = script.Prefabname + " " + (numAgents + 1);
                numAgents++;

            } else if(script.GetType().ToString() == "Gadget"){
                levelobject.transform.parent = gadgetHolder.transform; 
                levelobject.name = script.Prefabname + " " + (numGadgets + 1);
                numGadgets++;

            } else {
                print("Unrecognized prefab type: " + script.GetType().ToString() + "  actual line: " + l); //neatpaziistu skripta klasiiti
            }



            script.InitFromString(l);
            script.PlaceOnGrid(1); //zinjoju prefaba skriptam, ka prefabs novietots speeles laukumaa


            
        }


        CheckWorkingStatusEveryBlock(1); //kad vissi levelobjekti novietoti, varu sleegt tos iekshaa; ir/vai jaasleedz iekshaa, shis statuss jau ir uzsetots levelobjektaa
        CheckWorkingStatusEveryBlock(1); //divreiz, jo paarbauda ljoti nesistemaatiski un, ja gjenerators ir peedeejais objekts, tad var gadiities, ka neiesleedz nevienu levelobjektu, jo nav elektriibas >:)
        CheckWorkingStatusEveryBlock(1); //triisreiz!11
        CalculateNavgrid();


        stopwatch.Stop();
        print("ir ielaadeets liimenis " + levelname + " ir  " + (numRooms) + " telpas ar " + numGadgets + " lietaam un " + numAgents + " agjentiem  " + stopwatch.Elapsed);


        #endif
    }


    //@note -- ir aizdomas, ka shii keshoshana ir bezjeedziiga, junitijs pats piekesho atveertus prefabus
    public GameObject loadLevelobjectPrefab(string prefabName) {
        if(!prefabCache.ContainsKey(prefabName)) {

            //ljoti haxxxoriigs veids kaa, nezinot subdirektoriju, ielaadeeju prefabus
            GameObject prefab = Resources.Load("Rooms/" + prefabName) as GameObject;

            if(prefab == null){ //nebija levelobjektu diraaa, skatiis pie gadzhetiem
                prefab = Resources.Load("Gadgets/" + prefabName) as GameObject;
            }

            if(prefab == null){ //nebija levelobjektu diraaa, skatiis pie gadzhetiem
                prefab = Resources.Load("Agents/" + prefabName) as GameObject;
            }

            //PS, ja te nav tad buus ERRORS, lai taa paliek!
           
                
            prefabCache[prefabName] = prefab;



        }
        return prefabCache[prefabName];
    }


    /**
     * aizpilda visu limeni ar zemes kubikiem
     * tikai tukshajos laucinjos
     * 
     * @note -- ljoti leens, ja meegjina atkaarotit izpildiit( jo tiek atrastas koliizija ar jau esoshiem kluciishiem)
     */ 
    public void FillWithGroundcubes() {
        int gameobjectLayer = 1 << 9; //9. slaanis ir visi levelobjekti
        GameObject prefab;//
        GameObject groundCube = loadLevelobjectPrefab("groundcube-1");    
        GameObject groundFlat = loadLevelobjectPrefab("groundcube-flat-1");    


        for(int i = limits.XA; i<limits.XB; i++) {
            for(int j = limits.YA; j<limits.YB+1; j++) { //viss liiimeni kaa arii 1 rinda virs liimenjia (+1 pa Y asi) 

                Vector3 positionInGrid = new Vector3(i, j, 0);

                if(Physics.CheckSphere(positionInGrid, 0.25f, gameobjectLayer)) { //vai laucinsh nav jau aiznjemts ar kaadu geimobjektu
                    //  print ("aiznjemts " +positionInGrid );
                    continue;
                }

                if(j == limits.YB){ //viena rinda virs liimenja - tur ies cita veida zemes kluciitis
                    prefab = groundFlat;
                } else {
                    prefab = groundCube;
                }


                //@NON_DRY sekojoshais koda gabals tiek vairaakkaartiigi :D dubleets shajaa failaa

                GameObject levelobject = Instantiate(
                    prefab, 
                    positionInGrid,
                    Quaternion.identity) as GameObject;
                
                Room script = levelobject.GetComponent<Room>(); //objekts instanceeets un skriptaa varu apskatiities apreekjinaatos offsetus                      
                levelobject.transform.parent = roomHolder.transform; 
                levelobject.name = script.Prefabname + " " + (numRooms + 1);
                numRooms++;
                levelobject.gameObject.layer = 9; //levelobjektu leijeris
                
                script.PlaceOnGrid(1); //zinjoju blokam, ka tas novietots speeles laukumaa
                script.ConstrPercent = 100; //haks - pasaku, ka gatavs

            }
        }


        CalculateNavgrid();

    }

    /**
     * ikvienam liimenii esoshajam blokam paarbaudiis vai tas var tikt ielseegts 
     * (ja speeleetaajs ir veeleejiet to iesleegt)
     * un iesleegs, ja var
     * kaa arii izleegs kaadu nejaushu bloku, ja resursu visiem nepietiek (sleegs aaraa tik daudz nejaushu bloku, kameer pietiks)
     * 
     * te dereetu sakaartot levelobjektu sarakstu:
     *  iesleegshanai vajag svariigaakos priekshaa
     *  izsleegshanai vajag nesvariigaakos priekshaa
     * tapec ir neiespejami sakartot, lai izpilditos abi nosacijumi, atliek randomizet un peistadit to kaa fiichu :D
     * 
     * @todo - levelobjekta svariigums, saakumaa gjeneratori svariigi, paareejie mazsvariigi, peeec tam ljaut speeleetaajam to modificeet
     * 
     * ordermode:
     *          0 - hronologjiskaa izveidoshanas seciibaa
     *          1 - randomizeet
     *          2 - sakaartot peec svariiguma VEEL NAV
     */     
    public void CheckWorkingStatusEveryBlock(int orderMode) {
        //print("paraustiis klokjus");


        Dictionary<string,Room> sorted = new Dictionary<string, Room>();

        if(orderMode == 0) { //hronologjiski

            for(int i = 0; i< roomHolder.transform.childCount; i++) { //ikviens levelobjekts liimenii
                Room lo = roomHolder.transform.GetChild(i).GetComponent<Room>();
                sorted.Add(i.ToString("D10"), lo); // D10 ir zeropadings, 10 cipari kopaa | taatad strings sorteesies tieshi taadaa pashaa seciibaa kaa saakotneejais INT
            }
        } else if(orderMode == 1) {  //random

            /**
             * @bullshit -- shii randomizaacija ir meeesls - jaalieto orderby..newguid   piem.:   foreach(Levelobject r in levelscript.ListOfRooms.OrderBy(a => System.Guid.NewGuid())){
             */ 
            
            for(int i = 0; i< roomHolder.transform.childCount; i++) { //ikviens levelobjekts liimenii
                Room lo = roomHolder.transform.GetChild(i).GetComponent<Room>();
                sorted.Add(Random.Range(1000, 9999) + "-" + i, lo); //atsleega ir nejaushs, tachu garanteeti unikaals strings
            }
        }

        foreach(KeyValuePair<string,Room> x in sorted.OrderBy(key => key.Key)) {

            Room lo = x.Value.GetComponent<Room>();
                
            if(lo.WantWorking) {//provees iesleegt tikai tad, ja tam ir jaabuut iesleegtam
                lo.setWorkingStatus(lo.WantWorking, false);
                }
        }


    }

    public void CalculateNavgrid() {

        pathCache = new Dictionary<Vector4, List<Vector2>>(); //noreseto atrsto celju keshu
        Navgrid = new Dictionary<Vector4, float>(); //noreseto
        ListOfRooms = new List<Room>();//pie viena ieguus aktuaalaako levelobjektu sarakstu 

        Dictionary<Vector2,int> directionsInCubes = new Dictionary<Vector2, int>(); //ikvienam blokam (x,y ) liimeenii, kam vinja prefabaa ir noraaditi iespeejamie virzieni (Waypoints.dirs), tiek ielikti te


        /**
         * A) vispirms noskaidroju kuri kubiki iejami un tiks izmantoti paathfaindingaa, vinju virzieni tiek salikti datu struktuuraa
         */
        for(int i = 0; i< roomHolder.transform.childCount; i++) { //ikviens levelobjekts liimenii
            Room room = roomHolder.transform.GetChild(i).GetComponent<Room>();
              
            if( room.ConstrPercent < 1 &&  room.Destructing) { 
                    continue; // skipo geimobjektus, kas tiek jaukti nost (kad gandriiz jau nojaukts)
                }
                /*
                if( room.ConstrPercent < 50 && room.Constructing) { 
                    continue; 
                }*/
                //te nevar likt nekaadus smalkaakus ierobezheojumus - pie cik % const un cik % destr uzskatiit, ka telpa ir ejama -- jo neviens peec tam neizsauks nevgrida paarreekjinaataaju!
                ListOfRooms.Add(room);
                float x = room.transform.position.x - (room.SizeX * 0.5f) + 0.5f; //atnjemu istabas izmeerus - t.i. xy moraada uz istabas apaksheejo kreiso kubicinju 
                float y = room.transform.position.y - (room.SizeY * 0.5f) + 0.5f;

                int numCubes = room.waypoints.passableDirections.Length; // cik kubikiem istabaa ir noraadiiti ieshanas virzieni (vajag tieshi tik, cik ir kubiju istabaa)
                if(numCubes != room.SizeX * room.SizeY) {
                    continue; //prefaba nav noraadiiti iespeejamie virzieni visiem kubikiem, skipojam shaadu
                }

                int currentCube = 0;
                for(int b=0; b<room.SizeY; b++) {//ikviens kubiks istabaa | apskata visus kubikus telpaa no kreisaas uz labo, no apakshas uz augshu
                    for(int a=0; a<room.SizeX; a++) { 

                        int directionsInThisCube = (int)room.waypoints.passableDirections[currentCube++]; //taipkaastoju enumu uz intu | kaa aii uzzinu kaados virzienos shajaa kubikaa var paarvietoties 
                        Vector2 pos = new Vector2(Mathf.RoundToInt(x + a), Mathf.RoundToInt(y + b));
                        if(!directionsInCubes.ContainsKey(pos)) { 
                            directionsInCubes.Add(pos, directionsInThisCube);
                        }

                    }   
                }
            
        }

        /**
         * A+) visi levelobjekti ir apskatiiti, tagad skatiishos aarpusee - ejami ir arii tie kubiki, kas ir tukshi un zem kuriem atrodas levelobjekts - taatad ejama ir arii tukshas alaas
         */
        for(int x = limits.XA; x< limits.XB; x++) {
            for(int y = limits.YA; y< limits.YB; y++) {
           
                if(roomAtThisPosition(x,y) == null && roomAtThisPosition(x,y-1) != null){ //shis kubiks tukshs, apakshaa nav tukshs

                    Vector2 pos = new Vector2(x,y); //taatad shis kubiks ir ejams
                    if(!directionsInCubes.ContainsKey(pos)) { 
                        directionsInCubes.Add(pos, (int)Waypoints.dirs.lr); // L<->R kustiiba
                    }

                }

            }
        }


      
        /**
         * B) zinot, kuri kubiki ir atveerti satiksmei, varu salikt patiesaas celja maksas (galvenais vai vispaar no a uz b var aiziet)
         */
        foreach(KeyValuePair<Vector2,int> cube in directionsInCubes) { 
            float x = cube.Key.x;
            float y = cube.Key.y;
            int directionsInThisCube = cube.Value; //   (int) Waypoints.dirs
            if(directionsInThisCube == 0){ //shis kubiks nezkapee ir ierakstiits, lai gan tajaa nav atljauta kustiiba
                continue;
            }

         
        
            bool cubeIsPassable = false;

            //int directionsInThisCube = (int)room.waypoints.passableDirections[currentCube++]; //taipkaastoju enumu uz intu | kaa aii uzzinu kaados virzienos shajaa kubikaa var paarvietoties 
            //Vector3 o = new Vector3(x+a,y+b,-1);
        
            if((directionsInThisCube & (int)Waypoints.dirs.t) == (int)Waypoints.dirs.t) { // shis klucis ljauj iet augshup ... 
                int neighborDirs;
                if(directionsInCubes.TryGetValue(new Vector2(Mathf.RoundToInt(x), Mathf.RoundToInt(y + 1)), out neighborDirs)) { // ... bet vai augshaa ir klucis
                    if((neighborDirs & (int)Waypoints.dirs.b) == (int)Waypoints.dirs.b) { // ... ,kursh ljauj iet lejup
                        //Debug.DrawLine(o,new Vector3(x+a+0.03f,y+b+0.5f,-1),Color.green); 
                        Vector4 path = new Vector4(Mathf.RoundToInt(x), Mathf.RoundToInt(y), Mathf.RoundToInt(x), Mathf.RoundToInt(y + 1));//vienaa vector4 tiek ielikti 1 punkta xy un otraa punkta xy
                        if(!Navgrid.ContainsKey(path)) {
                            Navgrid.Add(path, 1f); 
                        }

                    }
                    //Debug.DrawLine(o,new Vector3(x+a+0.02f,y+b+0.4f,-1),Color.yellow); //
                }
                //Debug.DrawLine(o,new Vector3(x+a+0.01f,y+b+0.3f,-1),Color.red);
                cubeIsPassable = true;
            }
            if((directionsInThisCube & (int)Waypoints.dirs.l) == (int)Waypoints.dirs.l) {  //shis klucis ljauj iet pa kreisi ... 
                int neighborDirs;
                if(directionsInCubes.TryGetValue(new Vector2(Mathf.RoundToInt(x - 1), Mathf.RoundToInt(y)), out neighborDirs)) { // ... bet vai pa kreisi ir klucis
                    if((neighborDirs & (int)Waypoints.dirs.r) == (int)Waypoints.dirs.r) { // ... ,kursh ljauj iet pa labi
                        //Debug.DrawLine(o,new Vector3(x+a-0.5f,y+b+0.03f,-1),Color.green); 
                        Vector4 path = new Vector4(Mathf.RoundToInt(x), Mathf.RoundToInt(y), Mathf.RoundToInt(x - 1), Mathf.RoundToInt(y));
                        if(!Navgrid.ContainsKey(path)) {
                            Navgrid.Add(path, 1f); 
                        }
                    }
                    //Debug.DrawLine(o,new Vector3(x+a-0.4f,y+b+0.02f,-1),Color.yellow); //
                }
                //Debug.DrawLine(o,new Vector3(x+a-0.3f,y+b+0.01f,-1),Color.red);
                cubeIsPassable = true;
            }
            if((directionsInThisCube & (int)Waypoints.dirs.b) == (int)Waypoints.dirs.b) { //shis kluis ljauj iet lejup
                int neighborDirs;
                if(directionsInCubes.TryGetValue(new Vector2(Mathf.RoundToInt(x), Mathf.RoundToInt(y - 1)), out neighborDirs)) { // ... bet vai lejaa ir klucis
                    if((neighborDirs & (int)Waypoints.dirs.t) == (int)Waypoints.dirs.t) { // ... ,kursh ljauj iet augshup
                        //Debug.DrawLine(o,new Vector3(x+a+0.03f,y+b-0.5f,-1),Color.green); 
                        Vector4 path = new Vector4(Mathf.RoundToInt(x), Mathf.RoundToInt(y), Mathf.RoundToInt(x), Mathf.RoundToInt(y - 1));
                        if(!Navgrid.ContainsKey(path)) {
                            Navgrid.Add(path, 1f); 
                        }

                    }
                    //Debug.DrawLine(o,new Vector3(x+a+0.02f,y+b-0.4f,-1),Color.yellow); //
                }
                //Debug.DrawLine(o,new Vector3(x+a+0.01f,y+b-0.3f,-1),Color.red);
                cubeIsPassable = true;
            }
            if((directionsInThisCube & (int)Waypoints.dirs.r) == (int)Waypoints.dirs.r) { // klucis ljuaj iet pa labi
                int neighborDirs;
                if(directionsInCubes.TryGetValue(new Vector2(Mathf.RoundToInt(x + 1), Mathf.RoundToInt(y )), out neighborDirs)) { 
                    if((neighborDirs & (int)Waypoints.dirs.l) == (int)Waypoints.dirs.l) { 
                        //Debug.DrawLine(o,new Vector3(x+a+0.5f,y+b+0.03f,-1),Color.green); 
                        Vector4 path = new Vector4(Mathf.RoundToInt(x), Mathf.RoundToInt(y ), Mathf.RoundToInt(x + 1), Mathf.RoundToInt(y));
                        if(!Navgrid.ContainsKey(path)) {
                            Navgrid.Add(path, 1f); 
                        }
                    }
                    //Debug.DrawLine(o,new Vector3(x+a+0.4f,y+b+0.02f,-1),Color.yellow); 
                }
                //Debug.DrawLine(o,new Vector3(x+a+0.3f,y+b+0.01f,-1),Color.red);
                cubeIsPassable = true;
            }
        



            if(cubeIsPassable){
                //navgridaa, joka peec, ieliks celju no klucha uz sevi (tad varees zhigli uzzinaat, vai punkts eksistee navgridaaa)
                Vector4 loop = new Vector4(Mathf.RoundToInt(x), Mathf.RoundToInt(y), Mathf.RoundToInt(x), Mathf.RoundToInt(y));
                if(!Navgrid.ContainsKey(loop)) {
                    Navgrid.Add(loop, 0f); 
                }
            }


               // }   
           // }
        
        
        
        }
    



//        print("navgrid updated; " + Navgrid.Count + " nodes");
//        print("telpas paarskaititas, ir " + ListOfRooms.Count); 


    }

    void drawPaths() {
        foreach(KeyValuePair<Vector4, float> p in Navgrid) { 
            //print(p.Key.x + ","+p.Key.y +  " -> " + p.Key.z + ","+p.Key.w);
            Debug.DrawLine(new Vector3(p.Key.x + 0.2f, p.Key.y + 0.2f, -1),
                           new Vector3(p.Key.z + 0.2f, p.Key.w + 0.2f, -1),
                           Color.green); 
        }
    }


    /**
     * te buutu jaapiekesho visi atrastie celji un jaanotiira keshss ikreiz paarbuuveejot navgridu
     */ 
    public List<Vector2> FindPath(int sx, int sy, int fx, int fy) {
            
      
        //grafa virsotnes ir Vector4 - virsotnes X poziicija, Y poziicija, ATTAALUMS lidz meerkjim (no siis pozziicijas) + attalums no shiis poz. liidz startam, ATTAALUMS liidz startam (t.s. G-score)
        //lietoju parastu listi un sorteeju [kad vajag] peec Vector4 z komponentes

        Vector4 start = new Vector4(sx, sy, MapDistanceBetweenPoints(sx, sy, fx, fy), 0f);
        Vector4 finish = new Vector4(fx, fy, 0f, 0f);


        Vector4 loop = new Vector4(Mathf.RoundToInt(sx),Mathf.RoundToInt(sy),Mathf.RoundToInt(sx),Mathf.RoundToInt(sy)); // punkts atpakalj uz sevi -- shaadi paarbaudu, vai tagadeejais punkts vispaar atrodas navgridaaa
        if(!Navgrid.ContainsKey(loop)) { //saakumpunkts neatrodas uz grida, nevaru piemekleet celju
            throw new System.Exception("not-on-a-grid");
            //print("nav uz grida!   " + loop);
        }
   
        Vector4 pathID = new Vector4(sx, sy, fx, fy); // unikaals celja identifikators 
        if(pathCache.ContainsKey(pathID)){
            return pathCache[pathID]; //bija keshaa
        }

        int iter = 0;    


        
        List<Vector4> open = new List<Vector4>(); //veel jaapskata
        List<Vector4> closed = new List<Vector4>(); 
        Dictionary<Vector2,Vector2> route = new Dictionary<Vector2,Vector2>();  //key-val  celjsh no -> uz  //te saliks daudzus celjus un iisto rekontrusees, kad buus sasniegts galapunkts
        
        open.Add(start); //saku, ka jaaapskata ir saakumvirsotne
        
        
        
        while(open.Count > 0) { //kameer ir veel virsotnes, kas jaaapskata
            
            open.Sort((a, b) => b.z.CompareTo(a.z)); 
            
            
            Vector4 current = open[open.Count - 1];  //panjemu DERIIGAAKO virsotni (minimals attalums no saakuma un beigaam)
            //print("Current " + current);
            open.RemoveAt(open.Count - 1); //aizvacu sho elemenu no listes (POP 2 soljos :D )
            
            //  print (Random.value + "A* AAApskatu " + current.x + "," + current.y);
            
            if(current.x == finish.x && current.y == finish.y) { //es ceru, ka INT=>FLOAT=>INT nemaina vertibu :\
                //ir atrasts celjsh, peec tam izdomaashu kaa to dabuut aaraa
                //   print ("A*  Celjsh atrasts !");
                
                List<Vector2> realRoute = new List<Vector2>(); // A* iedeva key=>value tonnu ar celjiem, man ir jaatrod iistais
                
                //  print ("A* atrasti celji no " + route.Count +  " punktiem" );
                /*for (int index = 0; index < route.Count; index++) {
                    var item = route.ElementAt (index);
                    var itemKey = item.Key;
                    var itemValue = item.Value;
                    
                    print (Random.value + "A* Route " + itemKey.x + "," + itemKey.y + " => " + itemValue.x + "," + itemValue.y);
                } //*/
                
                realRoute.Add(new Vector2(fx, fy)); //finisha punkts
                
                
                int aa = 0;
                while(route.ContainsKey(realRoute[realRoute.Count - 1])) {//savienoju celju, saakot no finisha; pa vienam lieku klaat realRoute listee un taas peedeejo pievienoto elementu lietoju kaa atsleegu routes datu struktuuraa
                    // Latviski: ja eksistee Ruute, kam atsleega ir peedeejaa RealRuute, tad pievienoju RealRuuteem sho ruuti, kam atsleega ir peedeejaa RealRuute
                    realRoute.Add(route[realRoute[realRoute.Count - 1]]);
                    if(aa++ > 1000) {
                        print("nu ir suudi !");
                        break;
                    }
                }
                
                //  print ("A* iistais celjs sastaav no " + realRoute.Count +  " punktiem" );
                
                //for (int i = 0; i < realRoute.Count; i++) { print ("A* RealRoute " + realRoute [i].x + "," + realRoute [i].y);}

                pathCache.Add(pathID,realRoute); //piekesho
                return realRoute;
                
            }
            
            closed.Add(current);
            List<Vector4> neighbors = new List<Vector4>(); //jasadabuu visi virsotnes kaiminji, kur var aiziet
            
            Vector4 maybeNeighbor;

           
            
            maybeNeighbor = new Vector4(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y), Mathf.RoundToInt(current.x - 1), Mathf.RoundToInt(current.y)); //vector4: no_x,no_y,uz_x,uz_y
            // print("varbuutderiigs kaiminsh: " + maybeNeighbor + "  #" + maybeNeighbor.GetHashCode() );
            if(Navgrid.ContainsKey(maybeNeighbor)) { //liiimenja navgridaa ir atrodams celjshs starp shiem kaiminjiem
                neighbors.Add(new Vector4(current.x - 1, current.y));
                //   print("deriigs kaiminsh: " + maybeNeighbor+ "  #" + maybeNeighbor.GetHashCode() );
            }
            
            maybeNeighbor = new Vector4(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y), Mathf.RoundToInt(current.x + 1), Mathf.RoundToInt(current.y)); 
            //  print("varbuutderiigs kaiminsh: " + maybeNeighbor+ "  #" + maybeNeighbor.GetHashCode() );
            if(Navgrid.ContainsKey(maybeNeighbor)) { 
                neighbors.Add(new Vector4(current.x + 1, current.y));
                // print("deriigs kaiminsh: " + maybeNeighbor+ "  #" + maybeNeighbor.GetHashCode() );
            }
            
            maybeNeighbor = new Vector4(current.x, current.y, current.x, current.y - 1);
            if(Navgrid.ContainsKey(maybeNeighbor)) { 
                neighbors.Add(new Vector4(current.x, current.y - 1));
                //  print("deriigs kaiminsh: " + maybeNeighbor);
            }
            maybeNeighbor = new Vector4(current.x, current.y, current.x, current.y + 1);
            if(Navgrid.ContainsKey(maybeNeighbor)) { 
                neighbors.Add(new Vector4(current.x, current.y + 1));
                // print("deriigs kaiminsh: " + maybeNeighbor);
            }
            //print("maybeNeighbor " + maybeNeighbor);
            
            
            for(int i = 0; i < neighbors.Count; i++) {
                //    print (Random.value + "A* Apskatu Kaiminju " + neighbors [i].x + "," + neighbors [i].y);
                if(isInThisList(neighbors[i], closed)) { //skipojam jau apskatiitus kaiminjus
                    //    print (Random.value + "A*  KKKaiminsh atrodas CLOSED listee " + neighbors [i].x + "," + neighbors [i].y);
                    continue;
                } else {
                    //   print (Random.value + "A*  KKKaiminsh ok " + neighbors [i].x + "," + neighbors [i].y);
                }
                
                float tentativeGScore = current.w + MapDistanceBetweenPoints(current.x, current.y, neighbors[i].x, neighbors[i].y); //tagadeejaa punkta attaalums no saakuma + attalums no tagadeejaa punkta liidz kaiminjam
                float gScoreNeighbor = MapDistanceBetweenPoints(neighbors[i].x, neighbors[i].y, sx, sy);
                //     print (Random.value + "A*  tentativeGScore = " + tentativeGScore);
                
                if(tentativeGScore > 9999f) { //NEPIEEJAMAS virsotnes, neapskatu un lieku aizveerto virsotnju sarakstaa
                    closed.Add(neighbors[i]);
                    continue;
                }
                
                if(!isInThisList(neighbors[i], open) || tentativeGScore < gScoreNeighbor) {
                    
                    if(!route.ContainsKey(new Vector2(neighbors[i].x, neighbors[i].y))) {
                        route.Add(new Vector2(neighbors[i].x, neighbors[i].y), new Vector2(current.x, current.y)); //celjs no->uz (vai otradi, po) 
                        //   print (Random.value + "A*  Celja gabals pievienots: " + + current.x + "," + current.y);
                    }
                    
                    if(!isInThisList(neighbors[i], open)) {
                        //aaaaaaa, vektori ir read-only :\ jaaveido jauns 
                        neighbors[i] = new Vector4(neighbors[i].x, neighbors[i].y,
                                                   tentativeGScore + MapDistanceBetweenPoints(neighbors[i].x, neighbors[i].y, fx, fy), //f_score = attalums uz startu + uz finishu
                                                   tentativeGScore //g score:  attalums no saakuma lidz apskatamajam + attalums lidz shim kaiminjam -- varbuut vajag taisno stripu no shii kaiminja uz saakumu? NEEEE!
                        );
                        
                        open.Add(neighbors[i]);
                        //  print (Random.value + "A*  Kaiminjsh ievietots OPEN listee " + + neighbors [i].x + "," + neighbors [i].y);
                    }
                }
                
                if(iter++ > 5000) {
                    print("A*  nu jau buus gana");
                    pathCache.Add(pathID,new List<Vector2>()); //piekesho arii sho neatrasto celju
                    return new List<Vector2>();
                }
                
            }
            
        }
        // print ("A*  Hmm, neatrada");
        
        pathCache.Add(pathID,new List<Vector2>()); //piekesho arii sho neatrasto celju
        return new List<Vector2>();
        
    }
    
    
    
    /**
     * Vai shis vektors ir shajaa listee 
     * @note -- vektoram skataas tikai x un y komponentes
     */
    bool isInThisList(Vector4 el, List<Vector4> thislist) {
        for(int i = 0; i < thislist.Count; i++) {
            if(el.x == thislist[i].x && el.y == thislist[i].y) {
                return true;
            }
        }
        return false;
        
    }
    
    /**
     * aliass priesh tiem floutinjiem
     */ 
    public float MapDistanceBetweenPoints(float x1, float y1, float x2, float y2) {
        return MapDistanceBetweenPoints(Mathf.FloorToInt(x1), Mathf.FloorToInt(y1), Mathf.FloorToInt(x2), Mathf.FloorToInt(y2));
    }
    
    /**
     * attalums starp punktiem uz kartes (tieshaa liinijaa)
     */
    public float MapDistanceBetweenPoints(int x1, int y1, int x2, int y2) {
        float dx = Mathf.Abs(x1 - x2);
        float dy = Mathf.Abs(y1 - y2);
        float d = (dx + dy); //manhetenas distance | manhetena ir taisnaakas striipas nekaa diagonaaldistancei, kaa arii shajaa speelee nav diagonaalpaarvietosahnaas
        // print( x1 +"," +y1 +" -> " +x2 +"," +y2 + " = " + d );
        return d;
        
    }

    /**
     * atgriezh telpu (Levelobject) vai null, kuraa ietilpst padotaas XY koordinaates
     * 
     * 
     */
    public Room roomAtThisPosition(float x, float y) {
                
        int numLevelblocks = ListOfRooms.Count;


        for(int i = 0; i < numLevelblocks; i++) {

            Room r = ListOfRooms[i];
            float halfWidth = r.SizeX / 2f;
            float halfHeight = r.SizeY / 2f;

            if(r.transform.position.x + halfWidth > x && r.transform.position.x - halfWidth <= x && 
                r.transform.position.y + halfHeight > y && r.transform.position.y - halfHeight <= y) { // vienkaarsha AABB kooliiziju detekcija, telpas koordinaate ir taas centraa, taapeec +/- pusgarums/platums
                return r;
            }
        }

       
        return null;
    }

    /**
     * atgriezh istabu vai null - kur var apmierinaat padoto vajadziibu
     *  ja vajadziiba ir -1, tad dod nejaushu telpu
     */ 
    public Room roomWhereIcanDoThis(int need) {

        Room room = null;

        int numLevelblocks = ListOfRooms.Count;
        if(numLevelblocks == 0) {
            return null; //pavei, telpu nav vispaar
        }

        if(need == -1) { //nav noraadiita vajadziiba - dos nejaushu telpu
            room = ListOfRooms[Random.Range(0, numLevelblocks)];
        } else { //meklees istabu, kas apmierina pieprasiito vajadziibu           
            foreach(Room r in ListOfRooms.OrderBy(a => System.Guid.NewGuid())) {
                if(r.AgentNeedsGeneration[need] > 0) { //telpa rada apskataamo agjentresursu
                    room = r;
                    break;
                }
            }
        }


        return room;
               
    }


    /**
     * atgriezh abosluutaas XY koordinaates 1 nejausha bloka koordinaates, kursh ietilpst padotajaa telpaa
     * nedod kubikus, kas nav navgridaa
     */ 
    public Vector2 randomCubeInThisRoom(Room room) {

                    
        List<Vector2> accessableCubes = AllAccessableCubesInThisRoom(room); 

        if(accessableCubes.Count > 0) {
            return accessableCubes[Random.Range(0, accessableCubes.Count)]; //telpas saakumkoordinaateem pieskaita nejausha telpas bloka koordinaates 

        }

        return new Vector2(room.transform.position.x, room.transform.position.y); //telpas saakumpunkts;

    }


    /**
     * 
     */
    public List<Vector2> AllAccessableCubesInThisRoom(Room room, bool includeNeighbors = false){

        List<Vector2> allCubes = new List<Vector2> ();
        Vector2 coords = new Vector2(room.transform.position.x - room.SizeX/2f, 
                                     room.transform.position.y - room.SizeY/2f); //punkts telpas labajaa apakseheejaa stuuriiti
       
        for(int b=0; b<room.SizeY; b++) {
            for(int a=0; a<room.SizeX; a++) {
                
                int localX = Mathf.FloorToInt(a + coords.x + 0.5f); //apskataamaa kubika koordinaates
                int localY = Mathf.FloorToInt(b + coords.y + 0.5f); //pieskaitu 0.5 pirms FLOORoju, citaadi rodas negliita nobiide

                if(Navgrid.ContainsKey(new Vector4(localX,localY,localX,localY))) { //vai navgridaa ir shis punkts x,y,x,y )
                    allCubes.Add(new Vector2(localX,localY));
                }

            }
        }


        //+ visi robezhkubicinji (no diagonal chicks)
        if(includeNeighbors){

            for(int b=0; b<room.SizeY; b++) { //eju no augshas uz leju

                //kubiks pa kreisi
                int localX = Mathf.FloorToInt(-1 + coords.x+ 0.5f);
                int localY = Mathf.FloorToInt(b + coords.y+ 0.5f);                
                if(Navgrid.ContainsKey(new Vector4(localX,localY,localX,localY))) {
                    allCubes.Add(new Vector2(localX,localY));
                }

                //kubiks pa labi
                localX = Mathf.FloorToInt(room.SizeX + coords.x+ 0.5f);
                localY = Mathf.FloorToInt(b + coords.y+ 0.5f);                
                if(Navgrid.ContainsKey(new Vector4(localX,localY,localX,localY))) {
                    allCubes.Add(new Vector2(localX,localY));
                }
            }


            for(int a=0; a<room.SizeX; a++) { //horizontaali
                
                //kubiks uz augshu
                int localX = Mathf.FloorToInt(a + coords.x+ 0.5f);
                int localY = Mathf.FloorToInt(-1 + coords.y+ 0.5f);                
                if(Navgrid.ContainsKey(new Vector4(localX,localY,localX,localY))) {
                    allCubes.Add(new Vector2(localX,localY));
                }
                

                //kubiks uz leju
                localX = Mathf.FloorToInt(a + coords.x+ 0.5f);
                localY = Mathf.FloorToInt(room.SizeY + coords.y+ 0.5f);                
                if(Navgrid.ContainsKey(new Vector4(localX,localY,localX,localY))) {
                   allCubes.Add(new Vector2(localX,localY));
                }
                
            }


        }




        return allCubes;
    }

    /**
     * vai shii levelobjekta nevins kubiks neatrodas uz kaada cita levelobjekta [jebkura kluciisha]
     */ 
    public bool IsThisSpotFree(Room newRoom){

        float x = newRoom.transform.position.x - (newRoom.SizeX/2f);
        float y = newRoom.transform.position.y - (newRoom.SizeY/2f);

        for(int b=0; b<newRoom.SizeY; b++) {
            for(int a=0; a<newRoom.SizeX; a++) {

                if(roomAtThisPosition(x+a,y+b) != null){
                    //shajaa poziicijaa ir atrasts levelobjekts
                    return false;
                }
            }
        }

        return true;

    }


    /**
     * izveido agjentu un novieto liimenii peles poziicijaa
     */ 
    public void AddAgent(){
        
        GameObject prefab = loadLevelobjectPrefab("avatar-1");
        GameObject agent = Instantiate(
            prefab, 
            //new Vector3(0, -4, 0),
            LastPosGrid,
            Quaternion.identity) as GameObject;
        agent.transform.parent = agentHolder.transform;
        Agent script = agent.GetComponent<Agent>();
        agent.name = script.Prefabname + " " + (numAgents + 1);
        numAgents++;
        script.Init();

    }

    /**
     * aizvaac 1. agjentu
     */ 
    public void RemAgent(){

        if(agentHolder.transform.childCount > 0){
            Destroy(agentHolder.transform.GetChild(0).gameObject );
        }

    }
        
}

/**
 * par vieniibas (unity) gridu liimenjobjektiem
 * grids tiek ziimeets +0.5 pa x un y asi
 * objektu poziicijas tiek glabaatas peec to centraalajaam koordinaateem
 * objekti, kam ir paara skaits vieniibu pa x vai y, attieciigajaa virzienaa tiek offsetoti pa 0.5 - lai visi objekti pieliptu pie rezhgja
 * shaadi visi objekti neatkariigi no lieluma ieguust gliitas, apaljas koordinaates 
 * 
 * taatad grida liiniju krustpunktos ir ..0.5, bet grida ruutinjas viduu ir ir apaljas koordinaates
 * 
 * 
 * par liimenjobjektu atrashanaas vietu
 * paarveershot peles koordinaates uz liimenjkoordinaateem, stars kolidee ar LevelBasePlane (8. slaanis), kas atrodas +1 pa Z asi, bet visi levelobjekti dziivo uz Z=0 - lai tie nekolideeto ar sho plakni
 * 
 * veel par liiminjobjektiem:
 * to atrashanaas punkts ir liimenjobjekta CENTRAA 
 *      - taapeec pirms lietot liimenjobjekta vai taa kubika koordinaates saistiibaa ar navgridu (ja liimenjobjekts ir vairaakus kubikus liels) taa koordinaates ir jaanoapaljo uz INT (floor)
 * tie tiek nobiidiiti pa x un y asiim 0.5 vai 0 vieniibas - atkariibaa no izmeera attieciigajaa virzienaa: 
 *      1,3,5... izmeera kluciishi netiek nobiidiiti tajaa asii, kur ir shis nepaara skaita izmeers
 *      1,4,6... izmeeru kluciishi tiek nobiidiiti par +0.5 tajaa asii, kur ir shis izmeers
 * 
 */ 
