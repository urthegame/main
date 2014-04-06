using UnityEngine;
using System.Collections;

public class Stanleyctrl : MonoBehaviour {

	private float speed = 2f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		//reset
		if (Input.GetKeyDown(KeyCode.Home)){
			transform.position = new Vector3(
				0,
				1.89f,
				0);
		}


		//staavu +/- paarsleedzeejs
		if (Input.GetKeyDown(KeyCode.PageDown)){
			transform.position = new Vector3(
				transform.position.x,
				transform.position.y -1,
				transform.position.z);
		}
		if (Input.GetKeyDown(KeyCode.PageUp)){
			transform.position = new Vector3(
				transform.position.x,
				transform.position.y +1,
				transform.position.z);
		}

		
		float y = 0;
		float z = 0;
		float x = 0;
	


		//muuvit
		if (Input.GetKey(KeyCode.UpArrow)){
			z += speed * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.DownArrow)){
			z -= speed * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.LeftArrow)){
			x -= speed * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.RightArrow)){
			x += speed * Time.deltaTime;
		}

	

		transform.position = new Vector3(
			transform.position.x + x,
			transform.position.y + y,
			transform.position.z + z);


		//max pret kameru un prom no kaeras
		if(transform.position.z > 0.67f){
			transform.position = new Vector3(
				transform.position.x,
				transform.position.y,
				0.67f);
		}
		if(transform.position.z < -0.166f){
			transform.position = new Vector3(
				transform.position.x,
				transform.position.y,
				-0.166f);
		}


	}
}
