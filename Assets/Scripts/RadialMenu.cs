using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuSector {

    public int id;
    public float filled; //graadi - skatiit DrawSector funkcijas dokumentaaciju
    public float rotation; //    -- " ---- " --
       
    private GameObject sectorBase;
    static private RadialMenu radialMenuScript; //viens vieniigais radialmenu skripts (shis fails)
    private Color colorBase;
    private Color colorHover;

    public MenuSector(int id, float filled, float rotation, Color colorBase) {

        if(radialMenuScript == null) {
            radialMenuScript = GameObject.Find("RadialMenu").GetComponent<RadialMenu>();
        }

        //hoverkraasa buus mazliec tidaada kaa normaalaa
        colorHover = new Color(Mathf.Clamp(colorBase.r - 0.2f, 0, 1),
                                      Mathf.Clamp(colorBase.g - 0.2f, 0, 1),
                                      Mathf.Clamp(colorBase.b - 0.2f, 0, 1),
                                                   1f);

        this.id = id;
        this.filled = filled;
        this.rotation = rotation;
        this.colorBase = colorBase;

        //izveidos 2 segmentus, atshkjiriigaas kraasaaas, vienu pasleeps
        sectorBase = radialMenuScript.DrawSector(filled, rotation, colorBase);
    }

    public void hover(bool on) {
        if(on){
            sectorBase.renderer.material.color = colorHover;
        } else {
            sectorBase.renderer.material.color = colorBase;
        }
    }
   
}

public class RadialMenu : MonoBehaviour {

    /**
     * sho skriptu pieskata GUI skripts (lai viss lietotaajinputs buutu vienuviet)
     */

    public bool RadialMenuWasOn; //vai ieprieksheejaa kadraa bija iesleegts
    public bool RadialMenuOn;  //vai ir iesleegts


    private Level levelscript;
    private Vector3 defaultPosition = new Vector3(0, 0, 9.5f); //lokaalaas koordinaates - attieciibaa pret vecaaku: kameru
    private float menuScale; //RadialMenu tiek skeilots pa xy asiim - to man vajag te zinaat, lai gloaalaas koordinaates paarveerstu lokaalajaas
    private GameObject baseplane;
    private Vector2 lastMousePos; //xy relatiivi pret centru

    private int numSectors;
    private List<MenuSector>  menuSectors = new List<MenuSector>();
    private int lastSectorHovered;
    
    void Awake() {

        levelscript = GameObject.Find("Level").GetComponent<Level>();

        /**
         * zem radialmenu geimbojekta atrodas base gameobjekts - menjucha pamats,
         * to dereetu izveito tik lielu, lai gliiti iederas jebkuraa ekraana izmeeraa
         */ 

        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();


        menuScale = transform.localScale.x; //x un y ir vienaadas, ja nav, tad kaut kas nav labi
        baseplane = GameObject.Find("Baseplane");
   

        // ----- 1. plaaksne ----- 

        menuSectors.Add(new MenuSector(1, 180, 0, new Color(0.5f, 0.5f, 0.5f, 0.8f)));
        menuSectors.Add(new MenuSector(2, 25, 180, new Color(0.15f, 0.25f, 0.7f, 0.9f))); //buus cilveekinfo
        menuSectors.Add(new MenuSector(3, 25, 205, new Color(0.1f, 0.2f, 0.8f, 0.9f))); //ir blokinfo riiks
        menuSectors.Add(new MenuSector(4, 35, 230, new  Color(0.8f, 0.125f, 0.17f, 0.9f)));
        menuSectors.Add(new MenuSector(5, 95, 265, new Color(0.2f, 0.8f, 0.3f, 0.9f)));




        //  drawSector(25, 180, new Color(0.15f,0.25f,0.7f,0.9f)  );
        //  drawSector(25, 205, new Color(0.1f,0.2f,0.8f,0.9f) );
        //  drawSector(35, 230, new Color(0.8f,0.125f,0.17f,0.9f) );
        //  drawSector(95, 265, new Color(0.8f,0.8f,0.7f,0.9f) );



        // drawSector(44, 0);
        //drawSector(44, 45);
        // drawSector(44, 90);

        stopwatch.Stop();
        print("RadialMenu time: " + stopwatch.Elapsed);
        

    }
    
    void Update() {


        if(!RadialMenuWasOn && RadialMenuOn) {
            transform.localPosition = defaultPosition; //noliek kameras priekshaa
            lastSectorHovered = 0;
            /**
             * @todo -- vajag nopozicioneet menjuci max tuvu pelei, bet lai saglabaatu nelielu atstarpiiti no ekraanmalas
             * @todo -- vai gadienaa nevajag izmeeru pielaagot ekraana izmeeram ?
             * @todo -- 
             */
            //print("on");
        } 
        if(RadialMenuWasOn && !RadialMenuOn) {
            transform.position = new Vector3(0, 0, -20f); //pasleepjs, noliekot noliek taalu aizmuguree
            //print("off");
            doMenuAction();
        }

        //print("RadialMenuWasOn " + RadialMenuWasOn + "   RadialMenuOn " + RadialMenuOn);

        //rejkaasts pret "baseplane" objektu
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int layer = 1 << 10;
        if(Physics.Raycast(ray, out hit, 100, layer)) { 
            lastMousePos = new Vector2((hit.point.x - baseplane.transform.position.x) / menuScale, (hit.point.y - baseplane.transform.position.y) / menuScale);
            //ieguustu relatiivo poz pret bespleina centru
            float directionDegrees = Mathf.Atan2(lastMousePos.x, lastMousePos.y) * Mathf.Rad2Deg;
            //print("pele ir " + lastMousePos );


            if(directionDegrees < 0) { //ieprieksh -180 .. 180 graadi | paartaisu uz 0 - 360 (jo negatiivie graadi patiesiibaa ir virs 180)
                directionDegrees += 360; 
            }
           // print(directionDegrees);

            foreach(MenuSector sector in menuSectors) {
                if(directionDegrees > sector.rotation && directionDegrees < sector.rotation + sector.filled) {
                    sector.hover(true);
                    lastSectorHovered = sector.id;
                } else {
                    sector.hover(false);
                }
            }


        }


       

    }


    /**
     * izveidojot sektorus, katram jaaiedod unikaalu ID, 
     * sheit peec shiem IDiem tiek viekta kaada darbiiba
     */ 
    void doMenuAction() {

        switch(lastSectorHovered) {
        case 1:
            print("do-nuttn");
            break;
        case 2:
            print("@todo -- agjentu inforiiks");
            break;
        case 3:
            levelscript.PutObjInPlacer("query-1");
           // print("inforiiks");
            break;
        case 4:
            levelscript.PutObjInPlacer("delete-1");
           // print("del");
            break;
        default:
            break;
        }

    }

    /**
     * partOfTheCircleFilled 0-360   cik daudz aplja aizpildiits (saak no 12:00, iet pulkstenjraadiitaaja kustiibas virzienam)
     * rotation - 0-360 graadi   +graadi ir pulkstenjraadiitaaja kustiibas virzienaa
     *
     */ 
    public GameObject DrawSector(float partOfTheCircleFilled, float rotation, Color color, float size = 1) {

        //float partOfTheCircleFilled = 1f / 4f; // cik daudz aplja ziimeet
        // float rotation = 45f;

        partOfTheCircleFilled = partOfTheCircleFilled / 360; //ieksheeji lieto 0-1 nevis graaadus 0-360
        partOfTheCircleFilled = (1 - partOfTheCircleFilled); //turmpaak lieto cik-daudz-no-aplja-neziimeet

        //iekshkjiigi lieto kaut kaadu kreisu rotaaciju
        rotation -= 90;
        rotation *= -1;


        //izviedo jaunu geimobjektu ar nepiecieshamajaam komponenteem
        GameObject sector = new GameObject();
        sector.transform.parent = baseplane.transform;
        Vector3 p = sector.transform.parent.transform.position;
        sector.transform.position = new Vector3(p.x, p.y, p.z - 0.5f); //par 0.5 tuvaak kamerai nekaa vecaaks
        sector.transform.name = "Sector " + numSectors++;
        sector.transform.localScale = new Vector3(size, size, size);
        sector.transform.rotation = Quaternion.Euler(0, 0, rotation);
        sector.AddComponent("MeshRenderer");
        sector.AddComponent("MeshFilter");
        Mesh mesh = sector.transform.GetComponent<MeshFilter>().mesh;
        sector.renderer.material = baseplane.renderer.material; //lietos lielaa aplja materiaalu (veidot/ielaaadeet jaunu ir saraezhgiiti un to ir jaatcertas manuaali izniicinaat)
        sector.renderer.material.color = color;//new Color(Random.Range(0.1f, 0.5f), Random.Range(0.1f, 0.5f), Random.Range(0.1f, 0.5f), 0.7f);



        float x, y;
        float angle = (2f * Mathf.PI) * partOfTheCircleFilled; 
        float angle_stepsize = 0.1f;


        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        int vertexCount = 0;
        
        
        Vector3 lastPos = new Vector3(0, 0, 0); 
        Vector3 center = new Vector3(0, 0, 0);



        while(angle <= 2f * Mathf.PI) {
            x = Mathf.Cos(angle);
            y = Mathf.Sin(angle);

            Vector3 pos = new Vector3(x, y);


            vertices.Add(lastPos);
            vertices.Add(center);
            vertices.Add(pos);
            triangles.Add(vertexCount++);
            triangles.Add(vertexCount++);
            triangles.Add(vertexCount++);
            lastPos = pos;            
     
            angle += angle_stepsize;
        }


        //+ jaapieshauj peedeejais punkts
        vertices.Add(lastPos);
        vertices.Add(center);
        vertices.Add(new Vector3(1, 0, 0));
        triangles.Add(vertexCount++);
        triangles.Add(vertexCount++);
        triangles.Add(vertexCount++);
        //*/

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();


        return sector;


    }


}
