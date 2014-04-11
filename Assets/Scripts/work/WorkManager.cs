using UnityEngine;
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

        /**
         * iziet cauri visai listei un noskaidro vai ir kaads darbinsh pieejams
         */ 
        workAvailable = false;
        foreach(WorkUnit w in worklist){
            if(w.on){ 
                workAvailable = true;
                break;
            }
        }

    }


    public WorkUnit GetWork(){

        try{ //worklistee tikai pievieno; izdzeestu telpu darbinji turpina te buut, dzeestie darbinji dos EXCEPTION!111


            /**
             * @todo -- atrast piemeerotaako darbu (nav veel prasmes un darba prasiibas)
             * @todo -- atrast tuvaako (dabuut listi ar deriigajiem darbiem un sakaartot peec attaaluma)
             * @todo -- tikai darbus, kur ir briivas poziicijas
             */ 
            foreach(WorkUnit w in worklist){
                if(w.on){ 
                    return w;
                } else {
                    print(w + " ir izsleegts darbinsh" );
                }
            }



        } catch (System.Exception e){
            //jaaizdzeesh darbinsh no listes, kurs radiija exceptionu
           // GetWork(); //meegjinaas veeelreiz, liidz atradiis darbu (vaiatgrieziis null)
            print("WIIUUUWIIIUUUWIIIUUUWIIUU");

        }

        return null;

    }

}



