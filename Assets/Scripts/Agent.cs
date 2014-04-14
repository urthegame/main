using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

[System.Serializable]
public class AgentNeeds {

    public enum Types { //visi agjentam svariigie resursi | jo tuvaak augshai enumaa, jo lielaaka prioritaate
        Water = 0,
        Sleep = 1
    }
    public static int numTypes = System.Enum.GetNames(typeof(Types)).Length; //cik ir resursu (ljoti sarezhgiitaa veidaa izskaitiits ENUM'a garums )

    public float[] Reserve = new float[numTypes]; //rezerves - cik daudz ir katra resursa 
    public float[] Max = new float[numTypes]; //maximums - cik daudz var uzrkaat katru resursu
    public bool[] Shortage = new bool[numTypes]; //vai shis resurss saak truukt un to ir jaaiet mekleet


    public AgentNeeds() {
        Reserve[(int)Types.Water] = 50;
        Reserve[(int)Types.Sleep] = 50;

        Max[(int)Types.Water] = 100;
        Max[(int)Types.Sleep] = 100;

        Shortage[(int)Types.Water] = false;
        Shortage[(int)Types.Sleep] = false;

    }

}

public class Agent : MonoBehaviour {

    private Animator avatarAnimator;

    static private Level levelscript;
    static private WorkManager workManagerScript;
//    private Transform thoughtbubble;
 //   private TextMesh tbText;
//    private bool tbOn;

    //paarvietoshanaas FSM mainiigie
    private List<Vector2> actualRoute = new List<Vector2>();//aktuaalaakais varonja celjsh 
    private Vector3 nextRouteNode; //celja punkts, uz kuru pashlaik varonis dodas
    private bool nextRouteNodeIsSet;
    private bool isWalking;
    private bool isScaling; //kaapj pa trepeem
    private bool isTurning;
    private Vector3 destinationNode; //is moving where
    private float nominalSpeed = 1.5f; //standarta aatrums ...
    private float speed; // patiesiesais aatrums - nominaalais aatrums * pashreizeejie apstaaklji
    private float nominalTurningSpeed = 10f;
    private float turningSpeed;
    private Color agentColor;
    private Color agentColorLight;

    //globaalaas FSM mainiigie
    public enum AgentStates {
        idling,
        choosingRandomDestination,
        choosingResourceDestination,
        choosingWorkDestination,
        traveling,
        arriving,
        consumingResource,
        working,
        offTheGrid
    }
    public AgentStates CurrentState = AgentStates.idling;
    public AgentNeeds Needs; //datu struktuura, kur glabaajaas cik agjentam ir katrs resurss

    // globaalaas FSM mainiigie nav droshi lietosahanaa nekur citur!!11
    private LOBlock currentRoom = null; // null vai telpa, kur atrodas cilveeks, to maina lielais FSM un tikai, galapunktos
    private int isCraving = -1; //kuru vajadziibu gaaja apmierinaat
    private float idlingFor; // cik ilgi slinkos
    private WorkUnit workUnit;  //ko darbinsh, ko dara (vai iet dariit) null, ja pashlaik neko nedara
    private float workingFor; //cik ilgi veel straadaas
    private float offTheGridFor; //cik ilgi muljljaajaas un netiek atpakalj telpaa

    void Awake() {
        avatarAnimator = transform.FindChild("cubeman").
           transform.FindChild("cubeman-size").
           transform.FindChild("cubeman-animation").GetComponent<Animator>();

   

        if(levelscript == null) {
            levelscript = GameObject.Find("Level").GetComponent<Level>();
            workManagerScript = GameObject.Find("Level").GetComponent<WorkManager>(); 
        }
        /*
        thoughtbubble = transform.FindChild("thoughtbubble");
        tbOn = true;
       // thoughtbubble.renderer.enabled = false;
        tbText = thoughtbubble.FindChild("text").GetComponent<TextMesh>();
        tbText.text = "+" + Random.Range(1,9999);
        */

        agentColor = new Color(Random.Range(0.1f, 0.5f), Random.Range(0.1f, 0.5f), Random.Range(0.1f, 0.5f), 1f);
      
       

        nominalSpeed *= Random.Range(0.7f, 1.3f); //atshkjiriigi standarta aatrumi katram agjentam     
        nominalTurningSpeed *= Random.Range(0.7f, 1.3f); 
        speed = nominalSpeed;
        turningSpeed = nominalTurningSpeed;


        Needs = new AgentNeeds();
    }

    public void Init(){
        transform.FindChild("cubeman").
        transform.FindChild("cubeman-size").
        transform.FindChild("cubeman-animation").
        transform.FindChild("body").renderer.material.color = agentColor;    

        //*
        //padaru kraasu gaishaaku (vienaadi palielinu katru komponenti)  (klampoju, lai nepaarsniedz max)
        //sho kraasu turpmaak lieto agjenta celja ziimeetaajs
        agentColorLight = new Color(Mathf.Clamp(agentColor.r + 0.2f, 0, 1),
                               Mathf.Clamp(agentColor.g + 0.2f, 0, 1),
                               Mathf.Clamp(agentColor.b + 0.2f, 0, 1),
                               1f);
        //*/

    }
    
    // Update is called once per frame
    void Update() {

        if(levelscript.DebugDrawNavgrid){
           drawPath();
        }

        manageResources(); //eed un gulj n stuff
        thinkAbout();//augstaaka liimenja stacked-finite-state-machine, kas liek varonim dariit lietas un pienjemt leemumu
        moveAbout(); //viszemaakaa liimenja overlaping-finite-state-machine, kas kustina varoni
       
        
    }

    /**
     * agjenta resursu teereetaajs/ieguveejs
     * uzsit karodzinjus, kad saak truukt kaads resurss
     * maina varonja aatrumu, kad beidzies kaads resurss
     */ 
    private void manageResources() {

        //parastie teereeshanas aatrumi katram resursam
        float[] consumption = new float[AgentNeeds.numTypes];
        consumption[(int)AgentNeeds.Types.Water] = 0.1f;
        consumption[(int)AgentNeeds.Types.Sleep] = 0.1f;

        if(CurrentState == AgentStates.traveling) { //ja agjents staigaa, tad teeree aatraak
            consumption[(int)AgentNeeds.Types.Water] = 0.2f;
            consumption[(int)AgentNeeds.Types.Sleep] = 0.2f;
        }


        //teerees visus resursus, kameer tie nebuus nulle
        for(int need = 0; need < AgentNeeds.numTypes; need++) {
            Needs.Reserve[need] -= consumption[need] * Time.deltaTime;

            //ja kaads no resursiem beidzies, iet un griezhas mazliet leenaak
            if(Needs.Reserve[need] < 0) {
                Needs.Reserve[need] = 0;
                speed = nominalSpeed * 0.75f; 
                turningSpeed = nominalTurningSpeed * 0.75f;
            }
    

            //ja resurss ir mazaak par 25%, tad atziimeeju, ka saacies taa truukums un agjents var iet mekleet taa papildinsaashanu
            if(Needs.Reserve[need] < Needs.Max[need] * 0.25) {
                Needs.Shortage[need] = true;
            } else {
                Needs.Shortage[need] = false;
            }

        }

        



        
    }


    /*
     * globaalaas uzvediibas FSM (finite-state-machine)
     * @todo -- stack baased ? ---paartaisiishu, ja atradiishu vismaz 12 iemeslus
     * 
     */ 
    private void thinkAbout() {

        LOBlock room = null;
        

        switch(CurrentState) {
        //---------------------------------------------------------------
        case AgentStates.idling:
            isCraving = -1; //nemeklee nekaadu resursu
            workUnit = null;

            int numResourceShortages = Needs.Shortage.Count(c => c); // ezoteeriskaa veidaa izskaita cik ir TRUE shajaa masiivaa - tik mums ir resursu, kas iet uz galu un jaaiet tos papildinaat
            if(numResourceShortages > 0){ //meklees sev resursus
                CurrentState = AgentStates.choosingResourceDestination;

            } else if(idlingFor > 0){ //pagaidiis bezdarbiibaa

                idlingFor -= Time.deltaTime;
                //bodyAnimator.SetBool("waving", true);
            
            } else if(workManagerScript.IsThereWorkAvailable()) { //ir kaads darbinsh pieejams
            //    print("ir kaads darbinsh");
                CurrentState = AgentStates.choosingWorkDestination;


            } else { //ja nav jaameklee resursus, tad izveeleesies nejaushu galapunktu un tur pagaidiis
                CurrentState = AgentStates.choosingRandomDestination;
                idlingFor = Random.Range(1, 3);
                //bodyAnimator.SetBool("waving", false);

               // print("STATE:idling + choseRND");
            }
            //print("STATE:idling");
            break;
            //---------------------------------------------------------------
        case AgentStates.choosingRandomDestination:


            room = levelscript.roomWhereIcanDoThis(-1); //dabuus nejaushu telpa


            if(room != null) {
                Vector2 rc = levelscript.randomCubeInThisRoom(room); //randomcube - nejaushss kubiks atrastajaa telpaa
                if(GoThere(rc.x, rc.y) == -1){ // kljuuda - agjents neatrodas uz grida
                    CurrentState = AgentStates.offTheGrid;
                    break;
                }

             //   print("rooom "  + Mathf.RoundToInt(room.transform.position.x) + ", " + Mathf.RoundToInt(room.transform.position.y) + "randCube "  + Mathf.RoundToInt(rc.x) + ", " + Mathf.RoundToInt(rc.y));
              // print("RANDrooom "  + (room.transform.position.x) + ", " + (room.transform.position.y) + "   RANDcube "  + (rc.x) + ", " + (rc.y));
                
                CurrentState = AgentStates.traveling;
            } else { 
                CurrentState = AgentStates.idling; //nav pat 1 nejaushas telpas, nu tad neko - turpina gaidiit
            }

            //print("STATE:choosingRandomDestination");
            break;
            //---------------------------------------------------------------
        case AgentStates.choosingResourceDestination:
            //jaaatrod TUVAAKAA telpa, kas apmierina vajadziibu peec resursa

           
            for(int need = 0; need < AgentNeeds.numTypes; need++) { //iet cauri visaam agjentvajadziibaam
                if(Needs.Shortage[need]){ //shis resurss truukst, to ir jaaiet mekleet | pirmaas vajadziibas ir svariigaakas (jo taas apskata sekvencionaali, varbuut vajag randomizeet seciibu ikreizi ?)

                    room = levelscript.roomWhereIcanDoThis(need);
                    if(room != null){ //ir atrasta sho vajadziibu apmierinosha telpa
                        isCraving = need; //pieseivos kuru vajadziibu ies apmierinaat - lai zinaatu, ka jaait prom, tikliidz TAA ir apmierinaata
                        break; //neapskata paareejaas vajadziibas
                    }
                }
            }

            
            if(room != null){  //jaaiet uz atrasto telpu
                Vector2 rc = levelscript.randomCubeInThisRoom(room); //randomcube - nejaushss kubiks atrastajaa telpaa
                if(GoThere(rc.x, rc.y) == -1){ // kljuuda - agjents neatrodas uz grida
                    CurrentState = AgentStates.offTheGrid;
                    break;
                }
                CurrentState = AgentStates.traveling;
               // print("atrada  " + rc.x + ","  + rc.y);
            } else {
                CurrentState = AgentStates.idling; //neko neatrada - jaaiet neko nedariit
              //  print("neatrada");
            }




           // print("STATE:choosingResourceDestination craving=" + isCraving);
            break;
            //---------------------------------------------------------------
        case AgentStates.choosingWorkDestination:
            //print("STATE:choosingWorkDestination ");

            List<WorkUnit> potentialJobs = new List<WorkUnit>();

            int currX = Mathf.FloorToInt(transform.position.x);
            int currY = Mathf.FloorToInt(transform.position.y);

            bool workingRemotely = false; //shutka :|  spec gadiijums, kad nevar veikt darbinju darba vietaa (taa neatrodas uz navgrida), bet darbinshs tik svariigs, ka to nevar nedariit - taapeec remoteWork gadiijumaa straadaas no noraadiitaas pociizijas
            Vector2 remoteWork = Vector2.zero; //poziicija no kuras straadaas, ja nebuus pieejam iistaa darbavieta

            //dabuus visus aktuaalos darbinjus, kur agjents var AIZIET | nejaushaa seciibaa
            foreach(WorkUnit w in workManagerScript.worklist.OrderBy(a => System.Guid.NewGuid())) {
                if(w.IsOn() && w.agentWorkingOn == null) { //tikai iesleegtie, briivie darbi
                    int destX = Mathf.FloorToInt(w.parentGameobject.transform.position.x);
                    int destY = Mathf.FloorToInt(w.parentGameobject.transform.position.y);

                    if(levelscript.FindPath(currX,currY,destX,destY).Count > 0){ //ja ir celjsh no agjenta liidz darba telpai
                        potentialJobs.Add(w);
                    } else { //nav celja liidz darbavietais

                        if((int)w.WorkUnitTypeNumber < 10){ //BET buuvdarbos taa biezhi vien notiek

                            //paarbaudiis telpas kaiminjus, ja tie ir pieejami, tad darbinsh tomeer ir pieejams
                            //@todo -- vertikaalaam telpaam ir vairaak saanu kaiminju ?
                            //@todo -- viena puse tiek apskatiita 1. vai tas neradiis neproporcionaalu agjentu veelmi iet straadaat no 1 puses pat, ja celjs tur ir taals ?
                            if(levelscript.FindPath(currX,currY,destX+w.parentLevelobject.SizeX,destY).Count > 0){  //labaa puse - x+ telpas platums
                                potentialJobs.Add(w);
                                workingRemotely = true;
                                remoteWork = new Vector2(destX+w.parentLevelobject.SizeX,destY); //neis uz nepiejamo kluciiti, bet straadaas no shiis poziiicijas
                            } else if(levelscript.FindPath(currX,currY,destX-1,destY).Count > 0){  //kreisaa puse - x-1
                                potentialJobs.Add(w);
                                workingRemotely = true;
                                remoteWork = new Vector2(destX-1,destY);
                            }


                        }

                    }

                }
            }


            workUnit = null;
            if(potentialJobs.Count > 0){
                workUnit = potentialJobs.ElementAt(0); //pirmais darbs (no nejaushi sakaartota saraksta)
            } else { //nav neviena briiva,pieejama darbinja
                CurrentState = AgentStates.idling;
                break;
            }

            room = workUnit.parentLevelobject; //darbinja parametrs - kurai telpai vinsh pieder
            if(room != null){  //jaaiet uz atrasto telpu

                Vector2 workLocation;

                if(workingRemotely){
                    workLocation = remoteWork; //atrastaa poziicija vistuvaak darbam, jo nevar atrasties pashaa darbavietaa
                } else {
                    workLocation = levelscript.randomCubeInThisRoom(room); //randomcube - nejaushss kubiks atrastajaa telpaa
                }

                    
                if(GoThere(workLocation.x, workLocation.y) == -1){ // kljuuda - agjents neatrodas uz grida
                    CurrentState = AgentStates.offTheGrid;
                    break;
                }



                CurrentState = AgentStates.traveling;
                workUnit.ReserveWork(true,this);
              //  print("atrada darbinju");
            } else {
                CurrentState = AgentStates.idling; //neko neatrada - jaaiet neko nedariit
               // print("NEatrada darbinju");
            }


            break;
            //---------------------------------------------------------------
        case AgentStates.traveling:
            // print("STATE:traveling");
            //kad agjents buus nonaacis galaa, tad vinja staavokli nomainiis moveAbout() metodee nevis sheit; nomainiis uz "arriving"
          

            break;
           //---------------------------------------------------------------
        case AgentStates.offTheGrid:
            //neatrodas uz navgrida, jaaatrod tuvaakais navgridaa esoshais punts un jaaevakueejas taa virzienaa

            if(offTheGridFor > 0){ //pagaidiis kaadu laicinju, lai neceptu procesoru
                offTheGridFor -= Time.deltaTime;
                break;
            }

            if(levelscript.Navgrid.Count > 0){ //ja ir navgridaa punkti, kur mukt

                Vector2 closestPoint = new Vector2(0,0);
                float closestDistance = 0;
                float x = transform.position.x;
                float y = transform.position.y;

                foreach(KeyValuePair<Vector4, float> p in levelscript.Navgrid) {
                    if(p.Key.y == y){ //meklee tuvaakos punkts tikai shajaa staavaa                    
                        float d = levelscript.MapDistanceBetweenPoints(p.Key.x,p.Key.y,x,y);//cik taalu shis navgrida punkts no agjenta
                        if(closestDistance == 0 || d < closestDistance){
                            closestPoint = new Vector2(p.Key.x,p.Key.y); //shis ir tuvaakais navgrida punkts
                            closestDistance = d;
                        }
                    }
                }

                if(closestDistance > 0){
                //    print("atradaam kaadu punktu, kur glaabties");
                    destinationNode = closestPoint;
                    actualRoute.Add(closestPoint); //manuaali izveidoju celju (stur tikai 1 punktu - kur skrienu glaabties)
                    CurrentState = AgentStates.traveling; //peec ieshanas automaatiski staavoklis buus IDLING
                    //print(x + "," + y + " -> " + closestPoint);
                    break;
                } else {
                  //  print("NEatradaam nevienu punktu, kur glaabties");
                    offTheGridFor = 3; //3 sekundes pagaidiis, liidz meegjinaas atkal, varbuut tad buuus kaut kas uzbuuveets
                }

            }
            break;
           //-----------------------------------------------------------------
        case AgentStates.arriving:


          

            if(workUnit != null){ //te ieradies darba dariishanaas
             //   print("ieradies straaadaaat");
                workingFor = Random.Range(2,5); //dazhas sekundes pastraadaas
                avatarAnimator.SetBool("working", true); 
                CurrentState = AgentStates.working;

                break; //break case

            } else if(isCraving >= 0){//sheit ieradies apmierinaat kaadu vajadziibu

                room = levelscript.roomAtThisPosition(transform.position.x,transform.position.y);  //null vai telpa


                if(room == null){ //atnaacis, bet neatrodas telpaa, hmmm
                    CurrentState = AgentStates.idling;
                 //   print("iatnaacis, bet neatrodas telpaa, hmmm");
                } else { //viss kaartiibaa ir telpaa

                    bool roomIsProviding = false;
                    for(int need = 0; need < AgentNeeds.numTypes; need++) { //iet cauri visaam agjentvajadziibaam
                        if(Needs.Shortage[need]){ //shis resurss truukst
                            if(room.AgentNeedsGeneration[need] > 0){ //telpa dod sho resursu
                                roomIsProviding = true;
                                break;
                            }
                        }
                    }

                    if(roomIsProviding){ //telpa deriiga, saakam eest
                      //  print("telpa deriiga, saakam eest");
                        currentRoom = room; //piekesho telpu, kur atnaacis                   
                        CurrentState = AgentStates.consumingResource;
                    } else { //telpa neko nedod
                        //print("telpa neko nedod");
                        CurrentState = AgentStates.idling;
                    }


                }

            }  else if(isCraving == -1){ //ir bezmeerkjiigi ieradies
                CurrentState = AgentStates.idling;
                break; //break case
            }
            

            //print("STATE:arriving");
            break;
            //---------------------------------------------------------------
        case AgentStates.consumingResource:
            // currentRoom -- tikai sheit droshi varu lietot sho mainiigo, jo tas ir uzsists ieprieksheejaa kadraa vieniigajaa IFaa, kas ved uz shejieni


            for(int need = 0; need < AgentNeeds.numTypes; need++) { //iet cauri visaam agjentvajadziibaam

                avatarAnimator.SetBool("eating", true); //pagaidaam ir tikai 1 teereeshanas animaacija - eeshana, visus resursus pateeree eedot - arii miedzinju XD

                Needs.Reserve[need] += currentRoom.AgentNeedsGeneration[need] * Time.deltaTime; //eed visus resursus (kas ir 0 vai pozitiivs skaitlis)

                if(Needs.Reserve[need] > Needs.Max[need]){ //neljauj paareesties, ierobesho maximumu
                    Needs.Reserve[need] = Needs.Max[need];
                }

                if(Needs.Reserve[isCraving] == Needs.Max[isCraving]){ // vajadziiba, kuras deelj naaca uz sho telpu, ir apmierinaata
                    CurrentState = AgentStates.idling; 
                    avatarAnimator.SetBool("eating", false);
                }

            }







            //print("STATE:consumingResource");
            break;
            //---------------------------------------------------------------
        case AgentStates.working:



          



            if(!workUnit.IsOn()){ //darbinsh tiek izsleegts
                CurrentState = AgentStates.idling;
                avatarAnimator.SetBool("working", false); 
                workUnit.ReserveWork(false); //padara darbinju pieejamu citiem
                break;
            }


            workingFor -= Time.deltaTime;
            workUnit.DoSomeActualMotherFlippingJob();


            if(workingFor < 0){ //buus gana straadaats
                CurrentState = AgentStates.idling;
                avatarAnimator.SetBool("working", false); 
                workUnit.ReserveWork(false);//padara darbinju pieejamu citiem
                break;
            }

            //visa magjija un progress notiek workunit skriptaa


            break;

        }




    }

    /**
     * zemaakaa FMS  - ruupeeejaas par ieshanu, roteeshanu un kaapshanu uz naakamo punktu atrastajaa celjaa
     */
    private void moveAbout() {


        if(!nextRouteNodeIsSet) { //nav dabuut naakamais solis
         //   print ("njem naakamo virsotni  lastRoute.Count = "  + lastRoute.Count);
            if(actualRoute != null && actualRoute.Count > 0) { //bet ir veel celjsh priekhsaa
                nextRouteNodeIsSet = true;
                nextRouteNode = actualRoute[actualRoute.Count - 1]; //POP 2 soljos :/
                actualRoute.RemoveAt(actualRoute.Count - 1);
                //print ("panjeema naakamo virsotni");
                //lieku iet un griezties uz naakamo punktu

                if(Mathf.Round(nextRouteNode.y) != Mathf.Round(destinationNode.y)) { //buus jaakaapj pa kaapneem uz citu staavu 
                    isScaling = true; 
                } else { // naakamais punkts atrodas tajaa pashaa staavaa
                    isWalking = true;
                }

                isTurning = true;
                destinationNode = new Vector3(nextRouteNode.x, nextRouteNode.y, nextRouteNode.z); 
                avatarAnimator.SetBool("fast", true);
                //aaaaaaaanimeeeeeet!

            } else {
                if(CurrentState == AgentStates.traveling) { //nomaina staavokli lielajaa FSM - ka  esam sasniegushi galapunktu
                    CurrentState = AgentStates.arriving;
                    //print ("ieshana beigusies ? esam klaat ? ");
                }

            }
        }


        //griezhas pret meerkji (kameer nav pagriezies) | griezhas tikai pa Y asi (personaazhs staav uz x z plaknes )
        if(isTurning) {
            float y = transform.position.y;
            float z = transform.position.z; //joka peec nofiksees arii z parametru
            Vector3 a = new Vector3(destinationNode.x, y, z);
            Vector3 b = new Vector3(transform.position.x, y, z);

            if(Vector3.Distance(a, b) > 0.01) { // zhigli apaskatiishos vai a un b punkti nav viens un tas pats (ja ir, tad nav vajadziibas griezties un kvaternionu reekjinaataajs ir verni dusmiigs, ja prasu lenjkji starp 2 identiskiem vektoriem)

                Quaternion targetRotation = Quaternion.LookRotation(a - b, Vector3.up);
                if(Quaternion.Angle(targetRotation, transform.rotation) < 10f) { //gandriiz jau ir pagriezies
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 100);  //atlikushos graadus norotee uzreiz 
                    //print ("vairs negriezhas");
                    isTurning = false;

                } else {
                    //print ("griezhas");
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turningSpeed);    
                }

            } else {
                isTurning = false;
            }
        }



        if(isWalking && !isTurning) { //ies tikai, ja buus  pagriezies pareizajaa virzienaa (citaadi rodas bezgaliiga groziishanaas)


            //uzskatu, ka ir atnaacis mirklii, kad saak iet iet prom: punkts uz kuru jaapaariet sahajaa kadraa ir taalaak no meerkja, nekaa pashreizeejaa lokaacija  |  shii metode ir visizturigaakaa pret FPS piikjiem
            Vector3 newpos = transform.position + transform.TransformDirection(Vector3.forward) * speed * Time.deltaTime; //punkts uz kuru shajaa kadraa paarvietoties
            if(Vector3.Distance(transform.position, destinationNode) < Vector3.Distance(newpos, destinationNode)) {// grib attaalinaaties
                isWalking = false;
                nextRouteNodeIsSet = false;
                avatarAnimator.SetBool("fast", false);
                transform.position = destinationNode; //ir atnaacis
            } else {
                transform.position = newpos; //iet
            }

            
        }

        if(isScaling) {



            if(Vector3.Distance(transform.position, destinationNode) < 0.05f) { //vecaa metode atnaakshanas noteikshanai, ja buus probleemas, vajadzees uzlabot uz skatiishanos vai neiet projaam, nevis tuvumu meerkjim
                isScaling = false;
                nextRouteNodeIsSet = false;
                transform.position = destinationNode; //ir atnaacis
                avatarAnimator.SetBool("fast", false);
            } else {
                Vector3 direction; //taa kaa varonis negriezhas un augshu/leju, tad man manuaali jaapadod virzien, kuraa gribu, lai vinsh kaapj
                if(destinationNode.y > transform.position.y) {
                    direction = Vector3.up;
                } else {
                    direction = Vector3.down;
                }

                transform.position = transform.position + transform.TransformDirection(direction) * speed * Time.deltaTime;        
            }
            
        }

    }

    /**
     * atrod celju uz padoto punktu
     * 
     * atgriezh 0, ja viss labi, -1, ja agjents neatrodas uz grida
     * 
     */ 
    public int GoThere(float x, float y) {   

        int currX = Mathf.FloorToInt(transform.position.x);
        int currY = Mathf.FloorToInt(transform.position.y);

        int destX = Mathf.FloorToInt(x);
        int destY = Mathf.FloorToInt(y);

        actualRoute = new List<Vector2>();

        try {
            actualRoute = levelscript.FindPath(currX, currY, destX, destY);   
            return 0;
        } catch(System.Exception e) {
            if(e.Message == "not-on-a-grid"){
                print("not on a gridddd");
                return -1;
            }
        

        }

     
        return 118;//te nevinam nav jaanonaak

    }

    private void drawPath() {
    

        if(actualRoute.Count > 0) {
            for(int i = 0; i < actualRoute.Count-1; i++) {
                

                ///Debug.DrawLine (pos, npos);

                for(float thickness = 0.01f; thickness < 0.05f; thickness+= 0.01f) {
                    Vector3 pos = new Vector3(actualRoute[i].x + thickness, actualRoute[i].y + thickness, 0);
                    Vector3 npos = new Vector3(actualRoute[i + 1].x + thickness, actualRoute[i + 1].y + thickness, 0);
                    Debug.DrawLine(pos, npos, agentColorLight);
                }

                //  print ("A* Ziimee " + lastRoute [i].x + "," + lastRoute [i].y + " " + lastRoute [i+1].x + "," + lastRoute [i+1].y);         
                
            }
        }

    }

    //serializeeshanas funkcha - atgriezh visus parametrus CSV stringaa
    public string InitToString(){

        return string.Format(" {0} {1} {2} {3} {4} {5} {6}", 
                             agentColor.r,
                             agentColor.g,
                             agentColor.b,
                             nominalSpeed,
                             nominalTurningSpeed,
                             Needs.Reserve[(int)AgentNeeds.Types.Water],
                             Needs.Reserve[(int)AgentNeeds.Types.Sleep]                          
                             );
        /**
         * pagaidaam save/load ignoree jebkaadu meerkji un motivaaciju utt. seivo tikai izskatu un atrashanaas vietu - ar to pietiek, lai izskatiitos, ka viss straadaa
         */ 

    }

    //deserializeeshana
    public void InitFromString(string str){
        string[] c = str.Split(' '); //sadala pa komponenteem:  nosaukums,x,y,z un tad shim levelobjektam svariigaas lietas
        //taatad skipojam pirmos 4 (un mees skaitam no nulles)


        agentColor = new Color(float.Parse(c[4]),float.Parse(c[5]),float.Parse(c[6]),1f);
        nominalSpeed = float.Parse(c[7]);
        nominalTurningSpeed = float.Parse(c[8]);
        Needs.Reserve[(int)AgentNeeds.Types.Water] = float.Parse(c[9]);
        Needs.Reserve[(int)AgentNeeds.Types.Sleep] = float.Parse(c[10]);

        Init(); //lai agjents njem veeraa tikko nomainiitaas veertiibas

    }



}
