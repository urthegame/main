using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/**
 * singltons
 * pieder Level geimobjektam
 */ 
public class WorkManager : MonoBehaviour {
    public List<WorkUnit> worklist = new List<WorkUnit>(); //visu aktuvaalo pieejamo darbinju saraksts
    public bool workAvailable;


    /**
     * pieregjistreee levelobjeta padoto darbinju kopeejaa listee
     */ 
    public void AddWork(WorkUnit workunit){
        workunit.Init();
        worklist.Add(workunit);


        checkForAvailableWork();

    }


    /**
     * iesleedz/izsleedz visus darbinjus, kas pieder padotajai telpai
     */ 
    public void SetStatusOnThisBlocksJobs(bool status, LOBlock block){

        foreach(WorkUnit w in worklist){
            if(w.parentLevelobject == block){ 
                w.on = status;
            }
        }

        checkForAvailableWork();

    }


    public WorkUnit GetWork(){

            /**
             * @todo -- atrast piemeerotaako darbu (nav veel prasmes un darba prasiibas)
             * @todo -- atrast tuvaako (dabuut listi ar deriigajiem darbiem un sakaartot peec attaaluma)
             * @todo -- tikai darbus, kur ir briivas poziicijas
             */ 
        foreach(WorkUnit w in worklist.OrderBy(a => System.Guid.NewGuid())){
                if(w.on){ 
                    return w;
                }
            }

        return null;

    }

    private void checkForAvailableWork(){
        /**
         * iziet cauri visai listei un noskaidro vai ir kaads darbinsh pieejams
         * @todo -- tikai aktiivos (on) darbinjus
         * @todo -- tikai, ja ir kaads slots briivs
         */ 
        workAvailable = false;
        int i = 0;
        foreach(WorkUnit w in worklist){

            if(w.parentGameobject == null){ //telpa ar darbinju izdzeesta
                worklist.RemoveAt(i);
                checkForAvailableWork(); //izmainiiju listi, taapeec nevaru turpinaat ciklu, atlukushos jaaskata jaunaa ciklaa
                return;
            }

            if(w.on){ 
                workAvailable = true;
                break;
            }
            i++;
        }

    }

}



