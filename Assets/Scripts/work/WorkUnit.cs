using UnityEngine;
using System.Collections;

/**
 * viena darbavieta
 */ 
[System.Serializable]
public class WorkUnit {
    public enum WorkUnitTypeNumbers {  ManualLabor = 1  };

    public GameObject levelobject; //te jaanoraada (prefabaa) objekts, kam pieder shis skripts
    //workType @todo -- celt/nojaukt, gatavot pusdienas, aarsteet kakjiishsus

    /**
     * vai darbinju var dariit, piem, celshana un nojaukshana kljuust pieejama tikai peec dazhaadiem notikumiem
     * kaa arii, ja telpaa nav elektriiba un uudens (prefabaa ir noraadiids, ka tas ir nepieciehsams), tad telpa izsleedz sho darbinju
     */ 
    public bool on;



    /**
     * prefabaa noraada vienu enuma veertiibu: 
     *   kuru no [WorkUnitType extendeejoshaam] klaseem lietot shim darbinjam kaa darbu aprakstosho skriptu
     * 
     */ 
    public WorkUnitTypeNumbers WorkUnitTypeNumber; 
    public WorkUnitType WorkDescriptionScript; //te ieliks sho darba tipa aprakstosho skriptu


    public WorkUnit() {

        switch(WorkUnitTypeNumber) {
        case WorkUnitTypeNumbers.ManualLabor:
            WorkDescriptionScript = new ManualLabor();
            break;


        }

    }

}
