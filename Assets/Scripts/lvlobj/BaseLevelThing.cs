using UnityEngine;
using System.Collections;

public enum GridStickyness {
    uniGrid,
    deciGrid,
    EEEAAGLE
};

public abstract class BaseLevelThing : MonoBehaviour {


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
        if(collider != null) { //agjentiem nav shii kolaidera un nav vajadziigi arii shie parametri
            SizeX = collider.size.x;
            SizeY = collider.size.y;
            SizeZ = collider.size.z;
        }

    }


    //deseralizeesahana - LOADGAME
    public abstract void InitFromString(string str);
    //seralizeesahana - SAVEGAME
    public abstract string InitToString();



    public abstract  void PlacedInPlacer();
    public abstract void RemovedFromPlacer();
        

    public abstract void PlaceOnGrid(int mode);
    public abstract void RemovedFromGrid();


}
