using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



[System.Serializable]
public class Waypoints  {
    
    //iespeejamie paarvietoshanaas virzieni no 1 kubika uz naakamo
    //n = nowhere; top,left,bottom,right ..; lb = left&bottom ...
    public enum dirs {
        n = 0,
        t = 1,
        l = 1 << 1, //2
        b = 1 << 2, //4
        r = 1 << 3, //8
        tl = t | l,
        tb = t | b,
        tr = t | r,
        lb = l | b,
        lr = l | r,
        tlb = t | l | b,
        tlr = t | l | r,
        lbr = l | b | r,
        tbr = t | b | r,
        tlbr = t | l | b | r 
        
    };
    
    
    public dirs[] passableDirections;
    /**
     * prefabaa noraada "passableDirections" datu masiiva izmeeru - tik cik telpaa ir kubiku
     * katram telpas kubikam noraadaa iespeejamos ieshanas virzienus gan uz blakus telpaam, gan uz citiem kubikiem 1 telpas ietvaros
     * _____
     * 1 34 3  
     *   12 2
     *      1
     * 
     * pa labi un uz augshu: taatad +x un +y virzienaa
     * 
     * JA nav noraadiiti passabliDirections visiem telpas kubikiem, tad telpa netiek iekljauta navgridaa - taa ir nepieejams
     */ 
    
}

[System.Serializable]
public class ResourceInitInfo{
    // sekojoshie publiskie mainiigie ir jaauzsit prefabaa (tie startapaa tiks salikt DICTIONARY datu struktuuraas, taa ka - nekaadas mainishanas peec speeles palaishanas ):
    //ka ari shie ir manuali jatur lidzi aktualakajiem globalajiem resursiem :\
    public float GenerationAir; 
    public float GenerationElectricity;
    public float GenerationWater;
    public float UsageAir; 
    public float UsageElectricity;
    public float UsageWater;
    
    public float AgentNeedsWater; // agjentresursi - tie ies atsevishskjaa datu struktuuraa
    public float AgentNeedsSleep;
}

public enum FuncTypes {
    block, //noliekams bloks
    ground, //ambience n stuff
}

public class Room : BaseLevelThing {


    public float ConstrTime = 1; //cik sekundes ilgi buuvee
    public float DestrTime = 1; //cik sekundes ilgi jauc nostii
    public int Price = 0; //cik eiras maksaa uzbuuveesahana
    
    [HideInInspector]
    public bool Constructing; //vai celj
    [HideInInspector]
    public bool Destructing; //vai tomeer jauc nost
    //[HideInInspector]
    public float ConstrPercent;  //cik % uzcelts/nojaukts


    public Waypoints waypoints;  //prefabaa noraadaami weipointi   
    public WorkUnit[] workUnits; //prefabaa noraadaami telpaa daraamie darbinji
    public float[] AgentNeedsGeneration; //masiivs, kas nosaka, cik katru agjentresursu telpa rada

    

    public FuncTypes FuncType;  //funkcionaalais tips; jaaizveelas no enuma
    public bool DEBUG_ME = false;

    
    public ResourceInitInfo resourceInitInfo = new ResourceInitInfo();
    public Dictionary<Res,float> Generation; //cik resursvieniibas rada 1 sekundee 
    public Dictionary<Res,float> Usage; //cik resursvieniibas teeree 1 sekundee 
    public Dictionary<string,TextMesh> blockinfos;//blokinfo objektaa esoshie 3d teksta objekti | vajag public, jo dazhreiz galvenais liimenjskripts DEV_ONLY noluukos grib papkjiikjereet uz kaadu no shiem
    
    
    public bool Working; //darbojas = rada un teeree resursus; iesleedz tikai ,ja ir resursi, ko veelas teereet
    public bool WantWorking; //speeleetaajs veelaas sho telpu iesleegtu


    private Dictionary<Room,bool> neighbors; //visi unikaalie, kardinaalie kaiminji (NO diagonal chicks!)
    private int everyOtherFrameCounter;
    
   
    void Awake () {
        
       
        baseInit();

        //masu inicializacija
        Generation = new Dictionary<Res, float>();
        Usage = new Dictionary<Res, float>(); 
        blockinfos = new Dictionary<string, TextMesh>();
        neighbors = new Dictionary<Room,bool>(); 
        
        foreach (Res r in Enum.GetValues(typeof(Res))){
            Generation[r] = 0;
            Usage[r] = 0;
        }
        
        
        // prefabaa dfineetos mainiigos saliek Dictaa, pisnis, bet iebuuveetais inspektors neraada sarezshgiitas datu struktuuras [dictionary]
        Generation[Res.air] = resourceInitInfo.GenerationAir;
        Generation[Res.electricity] = resourceInitInfo.GenerationElectricity;
        Generation[Res.water] = resourceInitInfo.GenerationWater;
        Usage[Res.air] = resourceInitInfo.UsageAir;
        Usage[Res.electricity] = resourceInitInfo.UsageElectricity;
        Usage[Res.water] = resourceInitInfo.UsageWater;
        
        
        AgentNeedsGeneration = new float[AgentNeeds.numTypes];
        AgentNeedsGeneration[(int)AgentNeeds.Types.Water] = resourceInitInfo.AgentNeedsWater;
        AgentNeedsGeneration[(int)AgentNeeds.Types.Sleep] = resourceInitInfo.AgentNeedsSleep;
        
        WantWorking = true;


      
        
        if(ConstrTime > 0){ 
            ConstrTime = 60 / ConstrTime; //lai buuveeshanas ilgums buutu sekundees (lai var reizinaat ar delta_time)
        }
        if(DestrTime > 0){ 
            DestrTime = 60 / DestrTime; 
        }
        
    }



    
    void Update () {
        
        if(placedOnGrid){
            
            if(Constructing){
                
                //ConstrPercent += ConstrTime * Time.deltaTime;
                //konstrukcijas procentus inkrementee workUnit skriptaa
                
                if(ConstrPercent >= 100){
                    ConstrPercent = 100;
                    Constructing = false;
                    setWorkingStatus(WantWorking,true); //ieszleedz iekshaa (ja vien speeleetaajs nav paguvis buuveesahnas laikaa izsleegt aaraa)
                    blockinfos["percent"].renderer.enabled = false;
                    workManagerScript.RemoveAllConstructionJobsForThisBlock(this); //buuveeshana pabeigta, jaaizniicina darbinsh
                }
            }
            
            if(Destructing){
                //ConstrPercent -= DestrTime * Time.deltaTime;
                //konstrukcijas procentus inkrementee workUnit skriptaa
                
                if(ConstrPercent <= 0){
                    workManagerScript.RemoveAllConstructionJobsForThisBlock(this); //nojaukshana pabeigta, jaaizniicina darbinsh
                    Destroy(transform.gameObject); //aizvaac sho kluciiti no liimenja
                    //Destructing = false;
                    levelscript.CalculateNavgrid(); //lieku liimenim paarreekjinaat visus ejamos celjus, jo ir izmainjas
                }
                
            }
            
            
            if(everyOtherFrameCounter++ > 5){ //ik peec n kadreim (varbuut ik peec m milisekuneed ?) (un veel - varbuut vajag kaut kaadu globaalu noslodzes indikaatoru, darbu rindu - lai shis retais darbinsh tiek izliidzinaats ar citiem retiem darbinjiem)
                updateBlockInfo();
                everyOtherFrameCounter = 0;
            }
        }



        if(DEBUG_ME) {


            float x = transform.position.x - (SizeX/2f);
            float y = transform.position.y - (SizeY/2f);
            Vector3 pos = new Vector3(x,y,0);
        //    print(pos);

            Color color = new Color(0.09F, 0.991F, 0.099F, 1F);
           // Debug.DrawLine (new Vector3(x+0.15f,y+0.15f,0), new Vector3(x-0.15f,y-0.15f,0),color);
          //  Debug.DrawLine (new Vector3(x+0.15f,y-0.10f,0), new Vector3(x-0.15f,y+0.10f,0),color);


            color = new Color(0.99F, 0.991F, 0.09F, 1F);

            for(int b=0; b<SizeY; b++) {
                for(int a=0; a<SizeX; a++) {
                    Debug.DrawLine (new Vector3(x+a+0.1f,y+b+0.1f,0), new Vector3(x+a-0.1f,y+b-0.1f,0),color);
                    Debug.DrawLine (new Vector3(x+a+0.1f,y+b-0.1f,0), new Vector3(x+a-0.1f,y+b+0.1f,0),color);
                }
            } 


            color = new Color(0.1F, 0.9F, 0.3F, 1F);
            foreach(Vector2 p in levelscript.AllAccessableCubesInThisRoom(this,true )){
                Debug.DrawLine (new Vector3(p.x+0.1f,p.y+0.1f,0), new Vector3(p.x,p.y,0),color);
                Debug.DrawLine (new Vector3(p.x+0.11f,p.y+0.11f,0), new Vector3(p.x+0.01f,p.y-0.01f,0),color);
                Debug.DrawLine (new Vector3(p.x+0.12f,p.y+0.12f,0), new Vector3(p.x+0.02f,p.y-0.02f,0),color);
                Debug.DrawLine (new Vector3(p.x+0.13f,p.y+0.13f,0), new Vector3(p.x+0.03f,p.y-0.03f,0),color);
            }
            //*/

        }
        
    }
    
    
    override public void PlaceOnGrid(int mode){
        
        GameObject level = GameObject.Find("LevelobjectHolder");
        
        
        if(mode == 0){ //manuaali peivienotajiem levelobjektiem paarbaudiis vai nav novietots aarpus liimenja robezhaam
            if(transform.position.x-(SizeX * 0.5f) + 1 < levelscript.limits.XA){
                //print ("par taalu uz kreiso pusi");
                return;
            }
            if(transform.position.x+(SizeX * 0.5f) > levelscript.limits.XB){
                //print ("par taalu uz Labo pusi");
                return;
            }
            if(transform.position.y < levelscript.limits.YA){
                //print ("par taalu uz leju");
                return;
            }
            if(transform.position.y+(SizeY * 0.5f) > levelscript.limits.YB){
                //print ("par taalu uz augshu");
                return;
            }
        }
        

        
        if(!levelscript.IsThisSpotFree(this) ){
            print ("She nedriikst likt bloka !");
        } else {
            //print ("Bloks tiek novietots uz laukuma (tochna) ");
            
            if(mode == 0){ //nulltais rezhiims - speeleetaajs manuaali uzliek levelobjektu
                if(gResScript.Money < Price ){
                    return; //nav naudas, necelj
                }
                gResScript.Money -= Price;
            }
            
            //nonuljljo Z poziiciju un rotaaciju - jo ir VIENS objekts, kam shie parametri NEZKAPEEC uzsetojas uz NIECIIGAAM veeriibaam (piem.: -9.313226E-10)
            transform.position = new Vector3(transform.position.x,transform.position.y,0);
            transform.rotation = Quaternion.Euler(0,0,0);
            
            findAndInformNeighbors();
            
            transform.parent = level.transform; //novieto liimenjobjektu konteinerii, kur dziivo liimenim piederoshie objekti       
            transform.gameObject.layer = 9; //levelobjektu leijeris
            Destroy(transform.gameObject.GetComponent<Rigidbody>()); // kameer levelobjekts ir PLEISHOLDERII, tam pieder rigidbodijs, lai var koliizijas kolideet, nu tas vairs nav nepiecieshams
            
            //bloks sev panjem blockinfo objketu, kas atteelos vinja resursus
            
            GameObject blockinfoPrefab = Resources.Load("blockinfo") as GameObject; 
            
            GameObject blockinfo = Instantiate(
                blockinfoPrefab, 
                new Vector3 (transform.position.x,transform.position.y,-0.5f), //bloka info-objektu novieto -0.5 pa Z asi - priekshaa levelobjektam
                Quaternion.identity ) as GameObject;
            blockinfo.transform.parent = transform; //novieto pareizi hierarhijaa
            
            //skriptaa piekesheos teksta objektus, kas atteelo shii geimobjekta resursus
            blockinfos["air"] = blockinfo.transform.FindChild("air").GetComponent<TextMesh>();
            blockinfos["electricity"] = blockinfo.transform.FindChild("electricity").GetComponent<TextMesh>();
            blockinfos["water"] = blockinfo.transform.FindChild("water").GetComponent<TextMesh>();
            blockinfos["off"] = blockinfo.transform.FindChild("off").GetComponent<TextMesh>();
            blockinfos["on"] = blockinfo.transform.FindChild("on").GetComponent<TextMesh>();
            blockinfos["wantoff"] = blockinfo.transform.FindChild("wantoff").GetComponent<TextMesh>();
            blockinfos["wanton"] = blockinfo.transform.FindChild("wanton").GetComponent<TextMesh>();
            blockinfos["percent"] = blockinfo.transform.FindChild("percent").GetComponent<TextMesh>();
            
            
            
            if(mode == 0){ //nulltais rezhiims - speeleetaajs manuaali uzliek levelobjektu
                Constructing = true; //saak buuveeshanu 
                
            } else { //levelobjekts tiek likts ielaadeejot seivgemu
                
                if(!Constructing && !Destructing){
                    blockinfos["percent"].renderer.enabled = false; //peec nokluseejuma shis ir iesleegts
                }
                
            }
            
            
            /**
             * ielaadeets puspabeigts vai tikko nopirkts
             * izveido celtnieciibas darbinju
             * to buus jaaizvaac, kad pabeigs
             */ 
            
            
            if(Constructing){ 
                if(ConstrTime == 0){ //momentaa buuveejamaas telpas
                    //darbinju neveido
                    ConstrPercent = 100; //100 procenti, taatad naakamajaa UPDATE funkchaa konstrukcija tiks finalizeeta
                } else {
                    workManagerScript.CreateAndAddConstructionJob(this,WorkUnit.WorkUnitTypes._Construction );
                }
            }
            
            //tas pats ar nost jaukshanu
            if(Destructing){ 
                
                if(DestrTime == 0){
                    DestrTime = 0;
                } else {
                    workManagerScript.CreateAndAddConstructionJob(this,WorkUnit.WorkUnitTypes._Destruction );
                }
                
            }
            
            //prefabaa defineetos darbinjus ieliek kopeejaa sarakstaa
            foreach(WorkUnit w in workUnits){
                workManagerScript.AddWork(w);
            }
            
            
            updateBlockInfo();
            if(FuncType == FuncTypes.ground){//zemiites blokiem aizvaac GUI, tiem viss ir 0 un tikai lieki aizpilda ekraanu
                foreach(KeyValuePair<string, TextMesh> bInfo in blockinfos) {
                    if(bInfo.Key != "percent") { //procentinjus man vajag, paareejie - tiek pasleepti
                        bInfo.Value.transform.position = new Vector3(0,0,-99999); //nevis izsleedzu bet aizstumju taalu prom - jo dazhaas vietaas tie tiek iesleegti/izsleegti
                    }
                }
                
            }
            
            placedOnGrid = true;
            
            
        }
        
    }
    
    override public void RemovedFromGrid(){
        
        
        workManagerScript.RemoveAllConstructionJobsForThisBlock(this); //jaapaartrauc visi celtnieciibas darbi, if-any
        
        if(DestrTime == 0){
            ConstrPercent = 0; // ja konstrukcijas laiks ir nulle, tad uzsit 0 procentuis (naakamais UPDATE finalizees un aizvaaks sho kluciiti)
        } else {
            //ja ir konstrukcijas laiks, tad jaaveido darbinsh
            workManagerScript.CreateAndAddConstructionJob(this,WorkUnit.WorkUnitTypes._Destruction );
        }
        
        setWorkingStatus(false,true);//jaaizsleedz pirms aizvaakshanas, lai var atskaitiit savus resursus no globaalaa kopuma
        Destructing = true; //saakam jaukt nost
        Constructing = false; //paarstaaj celt, ja veel nebija pabeidzis
        blockinfos["percent"].renderer.enabled = true; //iesleedzu procentraadiitaaju
        
    }
    
    
   
    
    
    /**
     * 
     */ 
    private void findAndInformNeighbors(){
        //print ("meklee kaiminjus");
        
        neighbors = new Dictionary<Room, bool>(); //aizpilda visu sarakstu par jaunu (vienkaarshaak nekaa skipot jau zinaamos)
        int levelobjectLayer = 1 << 9;
        
        //pa labi
        float radius = (SizeY - 0.1f)/2; //mazliet mazaaks par augstumu (raadiuss, taapeec puse)
        float distance = SizeX; 
        Vector3 p1 = new Vector3(transform.position.x-distance/2f,transform.position.y,transform.position.z);
        foreach( RaycastHit hit in Physics.SphereCastAll(p1,radius, Vector3.right,distance,levelobjectLayer) ){
            //  print ("got right " + hit.transform.name);
            
            //draudzeejaas tikai ar citiem levelobjektiem, ja objektam nav shii skripta, tas ir nesvariigs klucis un nav pelniijis tikt pieskaitiits pie kaiminjiem :P
            Room otherRoom = hit.transform.gameObject.GetComponent<Room>(); 
            if(otherRoom){
                if (!neighbors.ContainsKey(otherRoom)){ //paarbauda vai shis kaiminsh jau nav sarakstaa (diivains bugs ielaadeejot saglabaatu liimeni)
                    neighbors.Add(otherRoom,true);
                }
            }
            
        }
        //pa kreisi
        p1 = new Vector3(transform.position.x+distance/2f,transform.position.y,transform.position.z);
        foreach( RaycastHit hit in Physics.SphereCastAll(p1,radius, Vector3.left,distance,levelobjectLayer) ){
            //print ("got left " + hit.transform.name);
            Room otherRoom = hit.transform.gameObject.GetComponent<Room>(); 
            if(otherRoom){
                if (!neighbors.ContainsKey(otherRoom)){
                    neighbors.Add(otherRoom,true);
                }
            }
        }
        //augsha
        radius = (SizeX - 0.1f)/2; 
        distance = SizeY; 
        p1 = new Vector3(transform.position.x,transform.position.y-distance/2f,transform.position.z);
        foreach( RaycastHit hit in Physics.SphereCastAll(p1,radius, Vector3.up,distance,levelobjectLayer) ){
            //print ("got top " + hit.transform.name);
            Room otherRoom = hit.transform.gameObject.GetComponent<Room>(); 
            if(otherRoom){
                if (!neighbors.ContainsKey(otherRoom)){
                    neighbors.Add(otherRoom,true);
                }
            }
        }
        //leja
        radius = (SizeX - 0.1f)/2; 
        distance = SizeY; 
        p1 = new Vector3(transform.position.x,transform.position.y+distance/2f,transform.position.z);
        foreach( RaycastHit hit in Physics.SphereCastAll(p1,radius, Vector3.down,distance,levelobjectLayer) ){
            //  print ("got bottom " + hit.transform.name);
            Room otherRoom = hit.transform.gameObject.GetComponent<Room>(); 
            if(otherRoom){
                if (!neighbors.ContainsKey(otherRoom)){
                    neighbors.Add(otherRoom,true);
                }
            }
        }
        
        //print (transform.name + " has found " + neighbors.Count + " neighbors" );
        
        //visiem jaunapzinaatajiem kaiminjiem pazinjoshu par sevi, lai tie ieliek savaas kaiminjlistees
        foreach (Room neighbor in neighbors.Keys){
            neighbor.addNeighbor(this);
        }
    }
    
    //mans kaiminsh, kursh ir mani atradis, iepaziistina ar sevi; pievienoju vinju savam kaiminjsarakstam
    public void addNeighbor(Room neighbor){
        
        if (!neighbors.ContainsKey(neighbor)) { 
            neighbors.Add(neighbor,true);
        }
    }
    
    
    /**
     * ielseedz/izsleedz resursteereeshanu/resursgjenereeshanu
     * 
     */ 
    public void setWorkingStatus(bool newWorkingStatus,bool checkOthers){
        
        /*
         *    ir    work | not work
         *          -----+---------
         *  grib    work | not work
         * 
         */
        
        WantWorking = newWorkingStatus; //speeleetaajs *veelaas*, lai straadaaatu, bet vai patieshaam straadaas, redzeesim ...
        
        if(Constructing || Destructing){//nav veel uzcelts, nevar straadaat
            return; 
        }
        
        
        if(WantWorking){ //grib, lai straadaa
            
            //var iesleegt tikai, ja no globaalajiem, pieejamajiem resursiem ir iespeejams dabuut visus nepiecieshamos
            bool resourcesAvailable = true;
            foreach (Res r in Enum.GetValues(typeof(Res))){
                if(Usage[r] > 0){ //skataas tikai vajadziigos resursus (citaadinj var iekulties aritmeetiska rakstura bugos :P )                
                    if(gResScript.Generation[r] - gResScript.Usage[r] < Usage[r]){ //globaalais paarpalikums nav pietiekams 
                        resourcesAvailable = false;
                        break;
                    }
                }
            }
            
            if(Working){ //jau pashlaik straadaa, bet vai pietiek resursu, lai turpinaatu straadaashanu
                
                bool deficit = false;
                foreach (Res r in Enum.GetValues(typeof(Res))){
                    if(Usage[r] > 0){ //skatos tikai levelobjekta pateereetos resursus
                        if(gResScript.Generation[r] < gResScript.Usage[r]){ //ir globaals resursa deficiits
                            deficit = true;
                        }
                    }
                }
                
                if(deficit){ //sleedzu sho levelobjektu aaraa
                    //print ("naacaas izsleegt " + name + "  resursi-- ");
                    foreach (Res r in Enum.GetValues(typeof(Res))){ 
                        gResScript.Generation[r] -= Generation[r];
                        gResScript.Usage[r] -= Usage[r];
                    }
                    Working = false;
                }
                
                
            } else { //pashlaik nestraadaa
                
                if(resourcesAvailable){ //ir pieejami resursi, varam iesleegt
                    //print ("izdevaas iesleegt " + name + "  resursi++ ");
                    foreach (Res r in Enum.GetValues(typeof(Res))){
                        gResScript.Generation[r] += Generation[r];//globaalo resursu kopumam pieskaita savu devumu
                        gResScript.Usage[r] += Usage[r];
                    }
                    Working = true;
                }
                
            }
            
        } else { //grib, lai nestraadaa
            
            if(Working){ //pashlaik straadaa, jaasleedz aaraa, jaaatdod savaaktie resursi
                //print ("izdevaas izsleegt " + name);
                foreach (Res r in Enum.GetValues(typeof(Res))){ 
                    gResScript.Generation[r] -= Generation[r];
                    gResScript.Usage[r] -= Usage[r];
                }
                Working = false;
            } else { //pashlaik nestraadaa, viss viss super, ejam maajaas
                
            }
            
        }
        
        
        
        
        if(checkOthers){ //lai izvairiitos no bezgaliigas rekursijas, jo visu bloku paarbaudiitaajfunkcha izsauks, sho, kas izsauks visu bloku paarbaudiitaajfunkchu ....
            levelscript.CheckWorkingStatusEveryBlock(1);  //paramtrs 1 - nejaushaa seciibaa, lai izskataas reaalistiskaak
            levelscript.CheckWorkingStatusEveryBlock(1);  //vajag divreiz, tas nogludina dazhus neliidzenumus (piem. elektriibas atsleegshana dazhaam telpaam peec 1 gjeneratora izleegshanas)
        }
        
        
        //paarsleedz ikoninjas
        blockinfos["off"].renderer.enabled = !Working;
        blockinfos["on"].renderer.enabled = Working;
        
        blockinfos["wantoff"].renderer.enabled = !WantWorking;
        blockinfos["wanton"].renderer.enabled = WantWorking;
        
        workManagerScript.SetStatusOnThisBlocksJobs(Working,this); //iesleedz/izsleedz darbinjus shajaa telpaa
        
    }
    
    override public void InitFromString(string str){
        string[] c = str.Split(' '); 
        /**sadala pa komponenteem:  nosaukums,x,y,z un tad shim levelobjektam svariigaas lietas
        * taatad skipojam pirmos 4 (un mees skaitam no nulles)
        * bool savefileWantWorking = bool.Parse(c[4]); 
        * shos statusu nedriikst vienkaarshi mainiit, bloks ir korekti jaaiesleedz:
        * setWorkingStatus(savefileWantWorking,false);
        */ 
        
        Working = false; //visi saak izsleegti, taapeec shis statuss nav jaaseivo 
        WantWorking = bool.Parse(c[4]); //meegjinaas iesleegt veelaak, ja levelobjektam IR jaabuut iesleegtam
        Destructing = bool.Parse(c[5]);
        Constructing = bool.Parse(c[6]);
        ConstrPercent = float.Parse(c[7]);
        
        
        
        
    }
    
    /**
     * @note -- tikai svariigie parametri no shii skripta
     *          pirmos 4 visu kluchu kopiigos parametrus seivotaajs pats pieliks
     * @note -- strings saakas ar atstarpi
     */ 
    override public string InitToString(){
        
        return string.Format(" {0} {1} {2} {3}", WantWorking,                                            
                             Destructing,
                             Constructing,                                           
                             Mathf.RoundToInt(ConstrPercent)
                             
                             );
     
        
        
    }
    
    private void updateBlockInfo(){
       
        if(Constructing || Destructing){
            blockinfos["percent"].text = string.Format("{0}%", Mathf.RoundToInt(ConstrPercent));
            
        }
    }
    
    /**
     * levelskripts izsauc sho
     */ 
    override public void PlacedInPlacer(){
        
    }

    /**
     * levelskripts izsauc sho
     */ 
    override public void RemovedFromPlacer(){
        
    }
    


}
