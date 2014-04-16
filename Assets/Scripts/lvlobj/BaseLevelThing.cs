using UnityEngine;
using System.Collections;

public abstract class BaseLevelThing : MonoBehaviour {

    //skripts pieder prefabam un shos parametrus nomaina manuaali caur inspektoru - izveidojot prefabu un pieshkjirot tam sho skriptu
    public int SizeX = 1;
    public int SizeY = 1;
    public int SizeZ = 1;
    public string Prefabname = "kaads aizmirsis mani paarsaukt, ibio!"; //kaa sauc prefabu, kam shis pieder, atstarpes aizliegtas


    protected static GlobalResources gResScript; //globaalo resursu pieskatiitaaajs - singltons 
    protected static WorkManager workManagerScript;//darbinju pieskatiitaajs - arii singltons
    protected static Level levelscript; //viens vieniigais Liimenja paarvaldniekskripts


    
    //shie parametri tiks apreekjinaati
    [HideInInspector]
    public float OffsetX; // vai vinjam vajag pabiidiities par puslaucinju (vajag, ja vinjam ir paara skaitlis izmeeraa)
    [HideInInspector]
    public float OffsetY;

    protected bool placedOnGrid; //vai novietots liimenii (pirms tam tiek vazaats apkaart PLEISERII)
    
    



    protected void baseInit(){


        OffsetX =  0.5f* ((SizeX+1f)%2f);
        OffsetY =  -0.5f* ((SizeY+1f)%2f);
        
        if(gResScript == null){ // piestartee statiskaas references uz singltoniem
            gResScript  = GameObject.Find("Level").GetComponent<GlobalResources>(); 
            workManagerScript = GameObject.Find("Level").GetComponent<WorkManager>(); 
            levelscript = GameObject.Find("Level").GetComponent<Level>();
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
