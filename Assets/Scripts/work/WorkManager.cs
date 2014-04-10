using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class WorkManager : MonoBehaviour {
    public List<WorkUnit> worklist = new List<WorkUnit>(); //visu aktuvaalo pieejamo darbinju saraksts


    public void AddWork(){

    }


    public WorkUnit GetWork(){

        try{ //worklistee tikai pievieno; izdzeestu telpu darbinji turpina te buut, dzeestie darbinji dos EXCEPTION!111

        } catch (System.Exception e){
            //jaaizdzeesh darbinsh no listes, kurs radiija exceptionu
            GetWork(); //meegjinaas veeelreiz, liidz atradiis darbu (vaiatgrieziis null)

        }

        return null;

    }

}



