using UnityEngine;
using System.Collections;

public class Gadget : BaseLevelThing {

    [HideInInspector]
    public Room parentRoom; //kurai telpai shis gadzhets pieder | atradiis rantaimaa

    private float lastConstrPercent = -1f;
    private GameObject percentageShowingFiller; //puscaurspiidiigs kubs, kas aizpilda visu objekta tilpuu - taa puscaurpiidiiba tiks mainiita atkariibaa no pabeigtiibas procentiem

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
                    transform.parent = levelscript.destroyHolder.transform; //ievieto sepciaalaa konteinerii, kur sagadiis savu izniicinaashanu (naakamaja kadraa) jo man vajag, lai jau shajaa kadraa pareizais konteineris satureetu tikai deriigos objektus 
                    Destroy(transform.gameObject); //aizvaac sho kluciiti no liimenja
                    levelscript.CalculateNavgrid(); //lieku liimenim paarreekjinaat visus ejamos celjus, jo ir izmainjas
                }
                
            }
            
            if(ConstrPercent != lastConstrPercent){
                lastConstrPercent = ConstrPercent;

                percentageShowingFiller.renderer.material.color = new Color(0,0,0, (100-ConstrPercent)/200f + 0.25f); // puscaurspiidiiba  0.25 - 0.75 
                if(ConstrPercent >= 100){
                    percentageShowingFiller.renderer.material.color = new Color(0,0,0,0); //pilniigi caurspiidiigs
                }
            }
        }

    }

    override public void InitFromString(string str){
        string[] c = str.Split(' '); 
      
        Destructing = bool.Parse(c[4]);
        Constructing = bool.Parse(c[5]);
        ConstrPercent = float.Parse(c[6]);
              
    }

    override public string InitToString(){
        
        return string.Format(" {0} {1} {2}",
                             Destructing,
                             Constructing,                                           
                             Mathf.RoundToInt(ConstrPercent)
                             
                             );
        
    }
    
    public override void PlacedInPlacer() {
    }

    public override void RemovedFromPlacer() {
    }
    
    public override void PlaceOnGrid(int mode) {

        float x = transform.position.x;
        float y = transform.position.y;
        parentRoom = levelscript.roomAtThisPosition(x,y); //gadzhets vienmeer pieder 1 telpai

        if(mode == 0){ //manuaali peivienotajiem gadzhetiem paarbaudiis vai driikst te atrasties
            if(parentRoom == null ){
                print ("paga, paga, gazhets netiek nolikts telpaa!");
                return;            
            }

            //paarbauda ai viss gadzhets (taa robezhas nosaka kolaideris) ietilpst shajaa telpaa (t.i. gadzhets neatrodas ar vienu kaaju aaraa)
            float halfWidth = parentRoom.SizeX / 2f;
            float halfHeight = parentRoom.SizeY / 2f;
            float w = SizeX / 2f;
            float h = SizeY / 2f;
            if(parentRoom.transform.position.x + halfWidth - w > x && parentRoom.transform.position.x - halfWidth + w < x /* &&  ---neesmu paarliecinaats par Y pareiziibu, bet tagad pietiek ar X
               parentRoom.transform.position.y + halfHeight -h > y && parentRoom.transform.position.y - halfHeight + h < y */ ) { 
               
            } else {
                print("gadzhets neietilpst VISS vienaa telpaa");
                return; 
            }


            if(!levelscript.IsThisSpotFree(this)){
                print ("vietinja aiznjemta!");
                return;
            }

        }


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
        transform.gameObject.layer = 10; //gadzhetu leijeris
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



        GameObject prefab = levelscript.loadLevelobjectPrefab("filler-1");
        
        percentageShowingFiller = Instantiate(
            prefab, 
            Vector3.zero,
            Quaternion.identity) as GameObject;
        percentageShowingFiller.transform.parent = transform;
        percentageShowingFiller.transform.localPosition = new Vector3(0,0,0);
        percentageShowingFiller.transform.localScale = new Vector3(SizeX,SizeY,SizeZ);
        percentageShowingFiller.renderer.material.color = new Color(0,0,0, (100-ConstrPercent)/200f + 0.25f);  

        
        placedOnGrid = true;
        
    }

    public override void RemovedFromGrid() {
        
        
        workManagerScript.RemoveAllConstructionJobsForThisGadget(this); //jaapaartrauc visi celtnieciibas darbi, if-any
        
        if(DestrTime == 0){
            ConstrPercent = 0; // ja konstrukcijas laiks ir nulle, tad uzsit 0 procentuis (naakamais UPDATE finalizees un aizvaaks sho kluciiti)
        } else {
            //ja ir konstrukcijas laiks, tad jaaveido darbinsh
            workManagerScript.CreateAndAddConstructionJob(parentRoom,this,WorkUnit.WorkUnitTypes._DestructionGadget );
        }
        
        //setWorkingStatus(false,true);//jaaizsleedz pirms aizvaakshanas, lai var atskaitiit savus resursus no globaalaa kopuma
        Destructing = true; //saakam jaukt nost
        Constructing = false; //paarstaaj celt, ja veel nebija pabeidzis

        percentageShowingFiller.renderer.material.color = new Color(0,0,0,0.25f);

    }
    
    
}
