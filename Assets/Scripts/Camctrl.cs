using UnityEngine;
using System.Collections;

public class Camctrl : MonoBehaviour {
	
	

	private float speed = 10f; // aaatrumam jaabuut atkariigam no zuuuma
	private float zoomSpeed = 1.8f;
	private Vector3 basePos = new Vector3(2.5f,-3.3f,-10);
	private Camera cam;
    private Level levelscript;


	void Start () {
        levelscript = GameObject.Find("Level").GetComponent<Level>();
		transform.position = basePos;
		cam = transform.FindChild("Main Camera").GetComponent<Camera>();
		cam.orthographic = false;
		//cam.orthographicSize = 5;
		cam.fieldOfView = 60;
	}
	
	// Update is called once per frame
	void Update () {
		
		float y = 0;
		float z = 0;
		float x = 0;
		float speedMod = 1f;
		float scrollSpeed = speed;// * cam.cam.fieldOfView;
		
		
		//muuvit
		if (Input.GetKey(KeyCode.S)){
            y -= scrollSpeed * (Time.deltaTime / levelscript.TimeScale);
		}
		if (Input.GetKey(KeyCode.W)){
			y += scrollSpeed * (Time.deltaTime / levelscript.TimeScale);
		}
		if (Input.GetKey(KeyCode.A)){
			x -= scrollSpeed * (Time.deltaTime / levelscript.TimeScale);
		}
		if (Input.GetKey(KeyCode.D)){
			x += scrollSpeed * (Time.deltaTime / levelscript.TimeScale);
		}
		if (Input.GetKey(KeyCode.LeftShift)){
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
		if (Input.GetKey(KeyCode.E)){
			cam.fieldOfView -= zoomSpeed * Time.deltaTime * 5;
			z += scrollSpeed * (Time.deltaTime / levelscript.TimeScale);
			if(cam.fieldOfView < 53f){ //peec FV nosaka, kad vairs nedriikst zuumot tuvaak
				cam.fieldOfView = 53f;
				z = 0;
			}

		}
		if (Input.GetKey(KeyCode.Q)){
			cam.fieldOfView += zoomSpeed * Time.deltaTime * 5;
			z -= scrollSpeed * (Time.deltaTime / levelscript.TimeScale);
			if(cam.fieldOfView > 100){
				cam.fieldOfView = 100f;
				z = 0;
			}

		}
		//*/


		
		//reset
		if (Input.GetKey(KeyCode.Tab)){
			transform.position = basePos;
			cam.orthographicSize = 5;
			cam.fieldOfView = 60;
		}
		
		transform.position = new Vector3(
			transform.position.x + (x*speedMod),
			transform.position.y + (y*speedMod),
			transform.position.z + z);


		
	}
}

