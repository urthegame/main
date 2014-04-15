﻿using UnityEngine;
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

    private Level levelscript;



    public void Awake(){
        levelscript = GameObject.Find("Level").GetComponent<Level>();
    }

    public void Init(){
        worklist = new List<WorkUnit>();
    }


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
    public void SetStatusOnThisBlocksJobs(bool status, Levelobject block) {

        foreach(WorkUnit w in worklist) {
            if(w.parentLevelobject == block) { //darbs pieder shim levelobjektam
                w.setOn(status);
            }
        }

        IsThereWorkAvailable();

    }


    /**
     * atradiis nejaushu darbinju, kas der padotajam agjentam (pagaodaam tikai peec taa vai agjent var vai envar piekljuut) 
     * atgrieziis darbinju un vietu, kur dariit sho darbinju 
     */ 
    public WorkUnit GetWork(Agent agent) {


        List<WorkUnit> potentialJobs = new List<WorkUnit>();

        //dabuu listi ar iesleegtiem/neaiznjemtiem darbiem
        foreach(WorkUnit w in worklist.OrderBy(a => System.Guid.NewGuid())) {
            if(w.IsOn() && w.agentWorkingOn == null) { 
                potentialJobs.Add(w);
            }
        }

        int agX = Mathf.FloorToInt(agent.transform.position.x);
        int agY = Mathf.FloorToInt(agent.transform.position.y);


        while(potentialJobs.Count > 0) {
            WorkUnit potentialJob = potentialJobs[0];  //njem pa vienam potenciaalam darbam 
            potentialJobs.RemoveAt(0); // POP 2 soljos :\


            bool includingNeightborCubes = false;
            if( (int)potentialJob.WorkUnitTypeNumber < 10){ //buuvdarbus var dariit arii no kaiminjkubikiem
                includingNeightborCubes = true;
            }

            List<Vector2> allCubes = levelscript.AllAccessableCubesInThisRoom(potentialJob.parentLevelobject,includingNeightborCubes); //visi kubiki telpaa, kurai pieder darbs (+ visi kaiminjkubiki, ja iipashi paluudz)


            foreach(Vector2 cube in allCubes.OrderBy(a => System.Guid.NewGuid())) {

                if(levelscript.FindPath(agX,agY,Mathf.RoundToInt(cube.x),Mathf.RoundToInt(cube.y)).Count > 0 ){
                    potentialJob.BestPositionToStandWhileWorking = cube; //pirmais nejaushais, agjentam pieejamais kubiks arii buus vieta, kur dariit sho darbinju
                    return potentialJob;
                }

            }
               




        }



        return null; //neatradaas neviens deriigs darbinsh ar pieejamu poziiciju, kur straadaat

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
    public void CreateAndAddConstructionJob(Levelobject block, WorkUnit.WorkUnitTypes workType){

        WorkUnit constructionJob = new WorkUnit();
        constructionJob.parentGameobject = block.gameObject;
        constructionJob.WorkUnitTypeNumber = workType;
        constructionJob.setOn(true);
        AddWork(constructionJob);

    }

    /**
     * izniicina visus buuvdarbus shim levelobjektam
     */ 
    public void RemoveAllConstructionJobsForThisBlock(Levelobject block){
            
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



