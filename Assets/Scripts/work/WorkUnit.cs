using UnityEngine;
using System.Collections;

/**
 * viena darbavieta
 */ 
[System.Serializable]
public class WorkUnit {
    public enum WorkUnitTypes {  ManualLabor = 1  };

    public GameObject levelobject; //te jaanoraada (prefabaa) objekts, kam pieder shis skripts
    //workType @todo -- celt/nojaukt, gatavot pusdienas, aarsteet kakjiishsus

    /**
     * vai darbinju var dariit, piem, celshana un nojaukshana kljuust pieejama tikai peec dazhaadiem notikumiem
     * kaa arii, ja telpaa nav elektriiba un uudens (prefabaa ir noraadiids, ka tas ir nepieciehsams), tad telpa izsleedz sho darbinju
     */ 
    public bool on;



    /**
     * prefabaa noraada vienu enuma veertiibu: 
     * 
     */ 
    public WorkUnitTypes WorkUnitTypeNumber; 



    public WorkUnit() {


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
