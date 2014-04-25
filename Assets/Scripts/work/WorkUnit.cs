using UnityEngine;
using System.Collections;

/**
 * viena darbavieta
 * kalpo tikai kaa datu struktuura - funkcionalitaate atrodas WorkManager skriptaa
 * 
 */ 
[System.Serializable]
public class WorkUnit {

    public enum WorkUnitTypes {  
        _ConstructionRoom = 1,  //sho izveidos automaatiski katrai telpai, nav nepiecieshams prefabaa veidot shii tipa darbinjus
        _DestructionRoom = 2,   // --"---"--- 
        _ConstructionGadget = 3,  //automaatiski izveodojams darbinsh gadzheta veidoshanai (gadzhets pieder telpai)
        _DestructionGadget = 4,   //

    // pirmie 10 tiek uzskatiiti par speciaalajiem darbiem, kas nav atkariigi no telpas resursies un iesl./izsl.
        ManualLabor = 10,  
    };

    public GameObject parentGameobject; //te jaanoraada (prefabaa) objekts, kam pieder shis skripts
    [HideInInspector]
    public Room parentRoom; //telpa, kam pieder shis darbs; automaatiski ieguushu no (ieguushu no "parentGameobject" )
    public Gadget parentGadget; //gadzhets, kam pieder shis darbs, (gadzhets pieder parentRoom)  ja shis ir null,tad darbs pieder pa tiesho telpai nevis gadzhetam

    /**
     * vai darbinju var dariit
     * ja telpaa nav elektriiba vai uudens utt. (prefabaa ir noraadiids, ka telpa teeree), tad telpa izsleegs sho darbinju
     */ 
    private bool on;
    [HideInInspector]
    public Agent agentWorkingOn; //kursh agjents pashlaik straada sho darbinju
    [HideInInspector]
    public Vector2 BestPositionToStandWhileWorking = new Vector2(); //pozciicija liimeni, kur jaaatrodas straadaajot, tiks uzsista, atrodot darbinju agjentam


    /**
     * prefabaa noraada vienu enuma veertiibu: 
     * 
     */ 
    public WorkUnitTypes WorkUnitTypeNumber; 



    /**
     * shai klasei nestraadaa konstruktors, jo taa tiek izveidota prefabaa nevis c# kodaa
     */ 
    public void Init() {


        if(parentGameobject != null) {
            parentRoom = parentGameobject.GetComponent<Room>();
        }


        /**
         * peec izveeleetaa darba veida, uzsitiis parametrus
         */ 
        switch(WorkUnitTypeNumber) {
        case WorkUnitTypes.ManualLabor:
            //stuff, njigu njegu

            break;

        }

    }

    /**
     * Veiks darbu
     * zinot straadnieku, levelobjektu un darba veidu
     */ 
    public void DoSomeActualMotherFlippingJob() {
        if(!on || agentWorkingOn == null) { 
            return;
        }

        
        switch(WorkUnitTypeNumber) {

        case WorkUnitTypes._ConstructionRoom:
            parentRoom.ConstrPercent += parentRoom.ConstrTime * Time.deltaTime * 2.5f;

            break;
        case WorkUnitTypes._DestructionRoom:
            parentRoom.ConstrPercent -= parentRoom.DestrTime * Time.deltaTime * 25f;
            
            break;

        case WorkUnitTypes._ConstructionGadget:
            parentGadget.ConstrPercent += parentGadget.ConstrTime * Time.deltaTime * 2.5f;
            
            break;
        case WorkUnitTypes._DestructionGadget:
            parentGadget.ConstrPercent -= parentGadget.ConstrTime * Time.deltaTime * 2.5f;
            
            break;
        case WorkUnitTypes.ManualLabor:


            break;
        }


    }

    /**
     * sleedz iekshaa aaraa darbinju
     * celtnieciibas darbinji klausa tikai, ja padod otro parametru TRUE
     */ 
    public void setOn(bool status, bool forceEvenTheMostStubbornOfStatuses = false) {
        on = status;

        if(!forceEvenTheMostStubbornOfStatuses) {
            //celtnieciibas darbu nevar izsleegt (levelobjekts izniicinaas to, kad tas vairs nebuus aktuaals)
            if((int)WorkUnitTypeNumber < 10) {//automaatiskie darbi, pirmaas 10 enum veertiibas
                on = true;
            }
        }

    }

    public bool IsOn() {
        return on;
    }

    
    /**
     * pieraksta shim darbinjam, ka shis agjents dara sho darbu (un darbs ir nepieejams citiem) 
     * start - TRUE saak darbu; FALSE beidz darbu (tas kljuuust atkal pieejams citiem)
     */
    public void ReserveWork(bool start, Agent agent = null) {
        if(start) {
            agentWorkingOn = agent;
        } else {
            agentWorkingOn = null;

        }
    }

}
