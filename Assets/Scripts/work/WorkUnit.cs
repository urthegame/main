using UnityEngine;
using System.Collections;

/**
 * viena darbavieta
 * kalpo tikai kaa datu struktuura - funkcionalitaate atrodas WorkManager skriptaa
 * 
 */ 
[System.Serializable]
public class WorkUnit {
    public enum WorkUnitTypes {  ManualLabor = 1  };

    public GameObject parentGameobject; //te jaanoraada (prefabaa) objekts, kam pieder shis skripts
    [HideInInspector]
    public LOBlock parentLevelobject; //shii geimobjekta LOBlock komponente (ieguushu no "parentGameobject" )

    /**
     * vai darbinju var dariit, piem, celshana un nojaukshana kljuust pieejama tikai peec dazhaadiem notikumiem
     * kaa arii, ja telpaa nav elektriiba un uudens (prefabaa ir noraadiids, ka tas ir nepieciehsams), tad telpa izsleedz sho darbinju
     */ 
    public bool on;

    [HideInInspector]
    public Agent agentWorkingOn; //kursh agjents pashlaik straada sho darbinju



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
            parentLevelobject = parentGameobject.GetComponent<LOBlock>();
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
     * pieraksta shim darbinjam, ka shis agjents dara sho darbu (un darbs ir nepieejams citiem) 
     * start - TRUE saak darbu; FALSE beidz darbu (tas kljuuust atkal pieejams citiem)
     */
    public void ReserveWork(bool start, Agent agent = null){
        if(start) {
            agentWorkingOn = agent;
        } else {
            agentWorkingOn = null;

        }
    }

}
