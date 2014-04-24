using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum GridStickyness {
    uniGrid,
    deciGrid,
    EEEAAGLE
};



[System.Serializable]
public class Waypoints  {
    
    //iespeejamie paarvietoshanaas virzieni no 1 kubika uz naakamo
    //n = nowhere; top,left,bottom,right ..; lb = left&bottom ...
    public enum dirs {
        n = 0,
        t = 1,
        l = 1 << 1, //2
        b = 1 << 2, //4
        r = 1 << 3, //8
        tl = t | l,
        tb = t | b,
        tr = t | r,
        lb = l | b,
        lr = l | r,
        tlb = t | l | b,
        tlr = t | l | r,
        lbr = l | b | r,
        tbr = t | b | r,
        tlbr = t | l | b | r 
        
    };
    
    
    public dirs[] passableDirections;
    /**
     * prefabaa noraada "passableDirections" datu masiiva izmeeru - tik cik telpaa ir kubiku
     * katram telpas kubikam noraadaa iespeejamos ieshanas virzienus gan uz blakus telpaam, gan uz citiem kubikiem 1 telpas ietvaros
     * _____
     * 1 34 3  
     *   12 2
     *      1
     * 
     * pa labi un uz augshu: taatad +x un +y virzienaa
     * 
     * JA nav noraadiiti passabliDirections visiem telpas kubikiem, tad telpa netiek iekljauta navgridaa - taa ir nepieejams
     */ 
    
}

[System.Serializable]
public class ResourceInitInfo{
    // sekojoshie publiskie mainiigie ir jaauzsit prefabaa (tie startapaa tiks salikt DICTIONARY datu struktuuraas, taa ka - nekaadas mainishanas peec speeles palaishanas ):
    //ka ari shie ir manuali jatur lidzi aktualakajiem globalajiem resursiem :\
    public float GenerationAir; 
    public float GenerationElectricity;
    public float GenerationWater;
    public float UsageAir; 
    public float UsageElectricity;
    public float UsageWater;
    
    public float AgentNeedsWater; // agjentresursi - tie ies atsevishskjaa datu struktuuraa
    public float AgentNeedsSleep;
}


public abstract class BaseLevelThing : MonoBehaviour {


    public Waypoints waypoints;  //prefabaa noraadaami weipointi | telpaam svariigi, gadzhetiem var buut, agjentiem nav  || no shii tiks veidots navgrids
    public WorkUnit[] workUnits; //prefabaa noraadaami telpaa daraamie darbinji | telpaam var buut, gadzhetiem arii var buut (tos deligjees telpaam), agjentiem n/a (varbuut buus autogjenereetie piem.: "glaabt sho draudzinju" :P )
    public float[] AgentNeedsGeneration; //masiivs, kas nosaka, cik katru agjentresursu objekts rada | telpaam nevajag, gadzhetiem vajag, agjentiem n/a

    public ResourceInitInfo resourceInitInfo = new ResourceInitInfo();
    public Dictionary<Res,float> Generation; //cik resursvieniibas rada 1 sekundee | telpaam nevajag, gadzhetiem vajag (tiks deligjeeti telpaam), agjentiem n/a
    public Dictionary<Res,float> Usage; //cik resursvieniibas teeree 1 sekundee | telpaam nevajag, gadzhetiem vajag (tiks deligjeeti telpaam), agjentiem n/a
    
    public float ConstrTime = 1; //cik sekundes ilgi buuvee
    public float DestrTime = 1; //cik sekundes ilgi jauc nostii
    public int Price = 0; //cik eiras maksaa uzbuuveesahana
    
    [HideInInspector]
    public bool Constructing; //vai celj
    [HideInInspector]
    public bool Destructing; //vai tomeer jauc nost
    //[HideInInspector]
    public float ConstrPercent;  //cik % uzcelts/nojaukts


    [HideInInspector]
    public float SizeX = 1; //objekta izmeers abosluutajaas vieniibaas - sho apreekjinaas peec objektam piederoshaa boxCollider izmeeriem -- taapeec vajag kolaiderim noraadiit pareizas veertiibas
    [HideInInspector]
    public float SizeY = 1;
    [HideInInspector]
    public float SizeZ = 1;
    public string Prefabname = "kaads aizmirsis mani paarsaukt, ibio!"; //kaa sauc prefabu, kam shis pieder, atstarpes aizliegtas


    protected static GlobalResources gResScript; //globaalo resursu pieskatiitaaajs - singltons 
    protected static WorkManager workManagerScript;//darbinju pieskatiitaajs - arii singltons
    protected static Level levelscript; //viens vieniigais Liimenja paarvaldniekskripts

    protected static GameObject roomHolder;
    protected static GameObject agentHolder;
    protected static GameObject gadgetHolder;

    
    //shie parametri tiks apreekjinaati
    [HideInInspector]
    public float OffsetX; // vai vinjam vajag pabiidiities par puslaucinju (vajag, ja vinjam ir paara skaitlis izmeeraa)
    [HideInInspector]
    public float OffsetY;

    protected bool placedOnGrid; //vai novietots liimenii (pirms tam tiek vazaats apkaart PLEISERII)

    public GridStickyness Stickyness; //pie kura grida lips klaat - vieninieka vai desmitdaljnieka
    



    protected void baseInit(){


        OffsetX =  0.5f* ((SizeX+1f)%2f);
        OffsetY =  -0.5f* ((SizeY+1f)%2f);
        
        if(gResScript == null){ // piestartee statiskaas references uz singltoniem
            gResScript  = GameObject.Find("Level").GetComponent<GlobalResources>(); 
            workManagerScript = GameObject.Find("Level").GetComponent<WorkManager>(); 
            levelscript = GameObject.Find("Level").GetComponent<Level>();

            roomHolder = GameObject.Find("LevelobjectHolder");
            agentHolder = GameObject.Find("AgentHolder");
            gadgetHolder = GameObject.Find("GadgetHolder");
        }

        //peec kolaidera izmeera izreekjinaas objekta kanonisko izmeeru
        BoxCollider collider = transform.GetComponent<BoxCollider>(); 
        SizeX = collider.size.x;
        SizeY = collider.size.y;
        SizeZ = collider.size.z;

        Generation = new Dictionary<Res, float>();
        Usage = new Dictionary<Res, float>(); 
        
        foreach (Res r in Enum.GetValues(typeof(Res))){
            Generation[r] = 0;
            Usage[r] = 0;
        }
        
        
        // prefabaa dfineetos mainiigos saliek Dictaa, pisnis, bet iebuuveetais inspektors neraada sarezshgiitas datu struktuuras [dictionary]
        Generation[Res.air] = resourceInitInfo.GenerationAir;
        Generation[Res.electricity] = resourceInitInfo.GenerationElectricity;
        Generation[Res.water] = resourceInitInfo.GenerationWater;
        Usage[Res.air] = resourceInitInfo.UsageAir;
        Usage[Res.electricity] = resourceInitInfo.UsageElectricity;
        Usage[Res.water] = resourceInitInfo.UsageWater;
        
        
        AgentNeedsGeneration = new float[AgentNeeds.numTypes];
        AgentNeedsGeneration[(int)AgentNeeds.Types.Water] = resourceInitInfo.AgentNeedsWater;
        AgentNeedsGeneration[(int)AgentNeeds.Types.Sleep] = resourceInitInfo.AgentNeedsSleep;


    }


    //deseralizeesahana - LOADGAME
    public abstract void InitFromString(string str);
    //seralizeesahana - SAVEGAME
    public abstract string InitToString();



    public abstract  void PlacedInPlacer();
    public abstract void RemovedFromPlacer();
        

    /**
     * mode=0 manuaali
     * mode=1 automaatiski peec loadgame
     */ 
    public abstract void PlaceOnGrid(int mode);
    public abstract void RemovedFromGrid();


}
