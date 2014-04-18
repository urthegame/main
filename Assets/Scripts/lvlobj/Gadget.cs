using UnityEngine;
using System.Collections;

public class Gadget : BaseLevelThing {

    public Room parentRoom; //kurai telpai shis gadzhets pieder

    public bool[] suitableForRooms = new bool[RoomRoles.Names.Length];//noraada true/false katrai telpas lomai - vai shis gadzhets var atrasties attieciigajaa telpaa
    //svarigi ir nomainiit katraa gadzheta prefabaa, katru reizi, kad nomainaas telpu lomu skaits, nosaukumi




    void Awake() {
        baseInit();
    }

    void Update() {
    
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

        /**
         * nepaarbauda vai atrodas atljautaa vietaa, to dara levelskriptaa pirms shiis funkchas izsaukshanas
         */ 


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
