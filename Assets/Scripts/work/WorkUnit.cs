using UnityEngine;
using System.Collections;

/**
 * viena darbavieta
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


    public int positions = 1; //cik cilveeki var dariit sho darbu vienlaiciigi



    /**
     * prefabaa noraada vienu enuma veertiibu: 
     * 
     */ 
    public WorkUnitTypes WorkUnitTypeNumber; 



    /**
     * shai klasei nestraadaa konstruktors, jo taa tiek izveidota prefabaa nevis c# kodaa
     */ 
    public void Init() {

        on = true;//joka peec iesleedz uzreiz

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

}
