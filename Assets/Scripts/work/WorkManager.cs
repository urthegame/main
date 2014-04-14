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
            if(w.parentLevelobject == block) { //darbs pieder shim levelobjektam
                w.setOn(status);
            }
        }

        IsThereWorkAvailable();

    }

    //@deprecated -- paaraak cieshi integreets ar agjentu un ceja mekleeshanu, meklees darbinju agjenta FSMaa
    public WorkUnit GetWork() {


        /**
         * @todo -- atrast piemeerotaako darbu (nav veel prasmes un darba prasiibas)
         * @todo -- atrast tuvaako (dabuut listi ar deriigajiem darbiem un sakaartot peec attaaluma)
         * tikai darbus, kur ir briivas poziicijas
         */ 
        foreach(WorkUnit w in worklist.OrderBy(a => System.Guid.NewGuid())) {
            if(w.IsOn() && w.agentWorkingOn == null) { 
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

            if(w.IsOn() && w.agentWorkingOn == null) { 
                workAvailable = true;
                break;
            }
            i++;
        }

       return workAvailable;
    }


    /**
     * izveido buuvdarbu (sho netaisa prefabam aarpus speeles, jo shis ir iislaiciigi pieejams darbs)
     */ 
    public void CreateAndAddConstructionJob(LOBlock block, WorkUnit.WorkUnitTypes workType){

        WorkUnit constructionJob = new WorkUnit();
        constructionJob.parentGameobject = block.gameObject;
        constructionJob.WorkUnitTypeNumber = workType;
        constructionJob.setOn(true);
        AddWork(constructionJob);

    }

    /**
     * izniicina visus buuvdarbus shim levelobjektam
     */ 
    public void RemoveAllConstructionJobsForThisBlock(LOBlock block){
            
        int i = 0;
        foreach(WorkUnit w in worklist) {
            
            if(w.parentLevelobject == block && (int)w.WorkUnitTypeNumber < 10) { //shai telpai piederoshs Buuvdarbs darbinsh
                w.setOn(false,true); //svariigi izsleegt, citaadi nabaga agjents straadaas liidz darbalaika beigaam
                worklist.RemoveAt(i);
                
                RemoveAllConstructionJobsForThisBlock(block);   //izmainiiju listi, taapeec nevaru turpinaat ciklu, atlikushos jaaskata jaunaa ciklaa
                return;
            }
            

            i++;
        }

    }



}



