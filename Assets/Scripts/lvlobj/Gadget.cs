using UnityEngine;
using System.Collections;

public class Gadget : BaseLevelThing {

    [HideInInspector]
    public Room parentRoom; //kurai telpai shis gadzhets pieder | atradiis rantaimaa

    void Awake() {
        baseInit();
       

    }

    void Update() {
    
        
        if(placedOnGrid){
            
            if(Constructing){
                
                //ConstrPercent += ConstrTime * Time.deltaTime;
                //konstrukcijas procentus inkrementee workUnit skriptaa
                
                if(ConstrPercent >= 100){
                    ConstrPercent = 100;
                    Constructing = false;
                   
                    workManagerScript.RemoveAllConstructionJobsForThisGadget(this); //buuveeshana pabeigta, jaaizniicina darbinsh
                }
            }
            
            if(Destructing){
                //ConstrPercent -= DestrTime * Time.deltaTime;
                //konstrukcijas procentus inkrementee workUnit skriptaa
                
                if(ConstrPercent <= 0){
                    workManagerScript.RemoveAllConstructionJobsForThisGadget(this); //nojaukshana pabeigta, jaaizniicina darbinsh
                    Destroy(transform.gameObject); //aizvaac sho kluciiti no liimenja
                    //Destructing = false;
                    levelscript.CalculateNavgrid(); //lieku liimenim paarreekjinaat visus ejamos celjus, jo ir izmainjas
                }
                
            }
            

        }

    }

    public override void InitFromString(string str) {
    }

    public override string InitToString() {
        return "";
    }
    
    public override void PlacedInPlacer() {
    }

    public override void RemovedFromPlacer() {
    }
    
    public override void PlaceOnGrid(int mode) {

        float x = transform.position.x;
        float y = transform.position.y;
        if(mode == 0){ //manuaali peivienotajiem gadzhetiem paarbaudiis vai driikst te atrasties
            if(levelscript.roomAtThisPosition(x,y) == null ){
                print ("paga, paga, gazhets netiek nolikts telpaa!");
                return;
                /**
                 * @todo -- paarbaudiit vai viss gadzhets ietilpst istabaa (ne tikai viduspunkts) 
                 */ 
            }

            /**
             * @todo -- paarbaudiit, vai gadzhets tiek novietots uz griidas vairaakstaaviigaas telpaas ()
             */ 

            if(!levelscript.IsThisSpotFree(this)){
                print ("vietinja aiznjemta!");
                return;
            }

        }

        parentRoom = levelscript.roomAtThisPosition(x,y);
        if(parentRoom == null) {
            print("watwatwat!? objekts nav novietots telpaa");
            return;
        }



        if(mode == 0) { //nulltais rezhiims - speeleetaajs manuaali uzliek levelobjektu
            if(gResScript.Money < Price) {
                return; //nav naudas, necelj
            }
            gResScript.Money -= Price;
        }


        transform.parent = gadgetHolder.transform; //novieto gadzhetu konteinerii
        Destroy(transform.gameObject.GetComponent<Rigidbody>()); // kameer levelobjekts ir PLEISHOLDERII, tam pieder rigidbodijs, lai var koliizijas kolideet, nu tas vairs nav nepiecieshams



        if(mode == 0) { //nulltais rezhiims - speeleetaajs manuaali uzliek levelobjektu
            Constructing = true; //saak buuveeshanu 
            
        } else { //levelobjekts tiek likts ielaadeejot seivgemu
                        
        }



        /**
         * ielaadeets puspabeigts vai tikko nopirkts
         * izveido celtnieciibas darbinju
         * to buus jaaizvaac, kad pabeigs
         */ 
        
        
        if(Constructing) { 
            if(ConstrTime == 0) { //momentaa buuveejamaas telpas
                //darbinju neveido
                ConstrPercent = 100; //100 procenti, taatad naakamajaa UPDATE funkchaa konstrukcija tiks finalizeeta
            } else {
                workManagerScript.CreateAndAddConstructionJob(parentRoom,this, WorkUnit.WorkUnitTypes._ConstructionGadget);
            }
        }
        
        //tas pats ar nost jaukshanu
        if(Destructing) { 
            
            if(DestrTime == 0) {
                DestrTime = 0;
            } else {
                workManagerScript.CreateAndAddConstructionJob(parentRoom,this, WorkUnit.WorkUnitTypes._DestructionGadget);
            }
            
        }

        /**
         * sho buus jaapaardomaa- 
         * gadzhetam prefabaa noraadiis, ka vinshs ir lietojams kaa darbavieta
         * un tad shiis darbavietas atdos telpai or smtn
         * / 
        //prefabaa defineetos darbinjus ieliek kopeejaa sarakstaa
        foreach(WorkUnit w in workUnits) {
            workManagerScript.AddWork(w);
        } //*/

        
        
        placedOnGrid = true;
        
        
        
    }

    public override void RemovedFromGrid() {
    }
    
    
}
