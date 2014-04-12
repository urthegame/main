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
    public void AddWork(WorkUnit workunit) {
        workunit.Init();
        worklist.Add(workunit);


        IsThereWorkAvailable();

    }


    /**
     * iesleedz/izsleedz visus darbinjus, kas pieder padotajai telpai
     */ 
    public void SetStatusOnThisBlocksJobs(bool status, LOBlock block) {

        foreach(WorkUnit w in worklist) {
            if(w.parentLevelobject == block) { 
                w.on = status;
            }
        }

        IsThereWorkAvailable();

    }

    public WorkUnit GetWork() {

        /**
         * @todo -- atrast piemeerotaako darbu (nav veel prasmes un darba prasiibas)
         * @todo -- atrast tuvaako (dabuut listi ar deriigajiem darbiem un sakaartot peec attaaluma)
         * @todo -- tikai darbus, kur ir briivas poziicijas
         */ 
        foreach(WorkUnit w in worklist.OrderBy(a => System.Guid.NewGuid())) {
            if(w.on && w.agentWorkingOn == null) { 
                return w;
            }
        }

        return null;

    }

    public bool IsThereWorkAvailable() {
        /**
         * iziet cauri visai listei un noskaidro vai ir kaads darbinsh pieejams
         * -- tikai aktiivos (on) darbinjus
         * -- tikai, ja kaads agjents jau tur nestraadaa
         */ 
        workAvailable = false; //reizee arii globaalais mainiigais
        int i = 0;
        foreach(WorkUnit w in worklist) {

            if(w.parentGameobject == null) { //telpa ar darbinju izdzeesta
                worklist.RemoveAt(i);
               
                return IsThereWorkAvailable();   //izmainiiju listi, taapeec nevaru turpinaat ciklu, atlikushos jaaskata jaunaa ciklaa
            }

            if(w.on && w.agentWorkingOn == null) { 
                workAvailable = true;
                break;
            }
            i++;
        }

       return workAvailable;
    }


}



