using UnityEngine;
using System.Collections;

public class Camctrl : MonoBehaviour {
    
    

    private float speed = 10f; //varbuut aaatrumam jaabuut atkariigam no zuuuma
    private float zoomSpeedFOV = 1.5f;
    private float zoomSpeedDistance = 13f;
    private Vector3 basePos = new Vector3(2.5f, -3.3f, -10);
    private Camera cam;
    private Level levelscript;
    private Vector3 autoMovePosition;
    private float autoMoveTime;
    private bool autoExcluseiveMoveEnabled; //neljauj lietotaajam kustinaat kameru
   
    [HideInInspector]
    public Vector3 LastUserCamPos;


    void Start() {
        levelscript = GameObject.Find("Level").GetComponent<Level>();
        transform.position = basePos;
        cam = transform.FindChild("Main Camera").GetComponent<Camera>();
        cam.orthographic = false;
        //cam.orthographicSize = 5;
        cam.fieldOfView = 60;
    }
    

    void Update() {

        autoCameraMove();
        if(!autoExcluseiveMoveEnabled) {
            manualCameraMove();        
        }
       
    }

    private void manualCameraMove() {
    
        float x = transform.position.x;
        float y = transform.position.y;
        float z = transform.position.z;
        float speedMod = 1f;
        float scrollSpeed = speed; // varbuut padariit atkariibu no zuuma
    
    
        //muuvit
        if(Input.GetKey(KeyCode.S)) {
            y -= scrollSpeed * (Time.deltaTime / levelscript.TimeScale);
        }
        if(Input.GetKey(KeyCode.W)) {
            y += scrollSpeed * (Time.deltaTime / levelscript.TimeScale);
        }
        if(Input.GetKey(KeyCode.A)) {
            x -= scrollSpeed * (Time.deltaTime / levelscript.TimeScale);
        }
        if(Input.GetKey(KeyCode.D)) {
            x += scrollSpeed * (Time.deltaTime / levelscript.TimeScale);
        }
        if(Input.GetKey(KeyCode.LeftShift)) {
            speedMod = 2f;
        }
    
    
        /*//zuumit orto
    if (Input.GetKey(KeyCode.E)){
        cam.orthographicSize -= zoomSpeed * cam.orthographicSize * (Time.deltaTime / levelscript.TimeScale);
        if(cam.orthographicSize < 1f){
            cam.orthographicSize = 1f;
        }
    }
    if (Input.GetKey(KeyCode.Q)){
        cam.orthographicSize += zoomSpeed * cam.orthographicSize * (Time.deltaTime / levelscript.TimeScale);
        if(cam.orthographicSize > 10f){
            cam.orthographicSize = 10f;
        }
    }
    //*/
    
    
        //* //zuumo fov kopaa ar kameras attaalumu
        if(Input.GetKey(KeyCode.E)) {
           // cam.fieldOfView -= zoomSpeed * Time.deltaTime * 5;
            z += zoomSpeedDistance * (Time.deltaTime / levelscript.TimeScale);
           

        
        }
        if(Input.GetKey(KeyCode.Q)) {
            //cam.fieldOfView += zoomSpeed * Time.deltaTime * 5;
            z -= zoomSpeedDistance * (Time.deltaTime / levelscript.TimeScale);

        }
        z = Mathf.Clamp(z,-15f,-2f); //ierobezho kameras attaalumu no liimenja plaknes
        cam.fieldOfView = calculateFOV(z);
        //*/

    
    
        //reset
        if(Input.GetKey(KeyCode.Tab)) {
            transform.position = basePos;
            cam.orthographicSize = 5;
            cam.fieldOfView = 60;
        }
    
        transform.position = new Vector3(
        (x * speedMod),
        (y * speedMod),
         z);


    }

    private void autoCameraMove() {
        if(autoMoveTime > 0) {
            autoMoveTime -= (Time.deltaTime / levelscript.TimeScale);
            float distance = Vector3.Distance(autoMovePosition, transform.position);

            if(distance < 0.1f) {
                autoMoveTime = 0; //atnaacaam
            } else {
                float autoSpeed = Mathf.Clamp(distance, 3f, 9999f);//aatrums ir proporcionaals attaalumam | neljauju aatrumam buut mazaakam par 1 (citaadi epedeejo streekjiiti ljoooooti leeni iet
                transform.position = transform.position + (autoMovePosition - transform.position) * autoSpeed * (Time.deltaTime / levelscript.TimeScale); 
                cam.fieldOfView = calculateFOV(transform.position.z);
            }

        }
    }

    public void ZoomToRoom(float x, float y) {

        autoExcluseiveMoveEnabled = true;
        autoMoveTime = 1.4f; //automaatiskaas kustiibas ilgums (neietekmee aatrumu, palielinot tikai paildzinaas automaatiskaas kustiibas ciinju ar manuaalo kameras kustiibu)
        autoMovePosition = new Vector3(x, y, -3);
        //print("zoom " + autoMovePosition + " diff: " + (autoMovePosition - transform.position));
    }

    public void UnZoomFromRoom() {
        autoExcluseiveMoveEnabled = false;
        autoMoveTime = 0.8f;
        autoMovePosition = LastUserCamPos;

    }


    /**
     * kamera kustaas daudz, FOV mainaas maz; ja sho maksimu maina, tad var sanaakt chau persiki
     * 
     * apreekjina FieldOFView atkariibaa no kameras attaaluma liidz zemei
     * 60 graadi pie attaaluma -10
     * piezuumojot palielina par zoomSpeed, par katru attaalum vieniibu vieniibu
     */
    private float calculateFOV(float distance){

        return Mathf.Clamp ((
                    (distance + 10) * zoomSpeedFOV) + 60    
                    ,45,90);  //min 45, max 90


        /**
         * @todo -- nevis klampot, bet gliitu funkchu, kas paleenaam samazina FOX
         * jo zuumoojot bez FOV mainjas, izskataas diivaini un skjietami kustaas ar citu aatrumu
         */ 
    }

}

