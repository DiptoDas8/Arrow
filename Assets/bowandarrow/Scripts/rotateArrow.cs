using UnityEngine;
using System.Collections;

// this class steers the arrow and its behaviour


public class rotateArrow : MonoBehaviour {

	// register collision
	bool collisionOccurred;

	// References to GameObjects gset in the inspector
	public GameObject arrowHead;
	public GameObject risingText;
	public GameObject bow;

	// Reference to audioclip when target is hit
	public AudioClip targetHit;

	// the vars realize the fading out of the arrow when target is hit
	float alpha;
	float   life_loss;
	public Color color = Color.white;

	// Use this for initialization
	void Start () {
		// set the initialization values for fading out
		float duration = 2f;
		life_loss = 1f / duration;
		alpha = 1f;
	}



	// Update is called once per frame
	void Update () {
		//this part of update is only executed, if a rigidbody is present
		// the rigidbody is added when the arrow is shot (released from the bowstring)
		if (transform.GetComponent<Rigidbody>() != null) {
			// do we fly actually?
			if (GetComponent<Rigidbody>().velocity != Vector3.zero) {
				// get the actual velocity
				Vector3 vel = GetComponent<Rigidbody>().velocity;
				// calc the rotation from x and y velocity via a simple atan2
				float angleZ = Mathf.Atan2(vel.y,vel.x)*Mathf.Rad2Deg;
				float angleY = Mathf.Atan2(vel.z,vel.x)*Mathf.Rad2Deg;
				// rotate the arrow according to the trajectory
				transform.eulerAngles = new Vector3(0,-angleY,angleZ);
			}
		}

		// if the arrow hit something...
		if (collisionOccurred) {
			// fade the arrow out
			alpha -= Time.deltaTime * life_loss;
			GetComponent<Renderer>().material.color = new Color(color.r,color.g,color.b,alpha);
			
			// if completely faded out, die:
			if (alpha <= 0f) {
				// create new arrow
				bow.GetComponent<bowAndArrow>().createArrow(true);
				// and destroy the current one
				Destroy(gameObject);
			}
		}
	}


	//
	// void OnCollisionEnter(Collision other)
	//
	// other: the other object the arrow collided with
	//


	void OnCollisionEnter(Collision other) {
		// we must determine where the other object has been hit
		float y;
		// we have to determine a score
		int actScore = 0;

		//so, did a collision occur already?
		if (collisionOccurred) {
			// fix the arrow and let it not move anymore
			transform.position = new Vector3(other.transform.position.x,transform.position.y,transform.position.z);
			// the rest of the method hasn't to be calculated
			return;
		}

		// I installed cubes as border collider outside the screen
		// If the arrow hits these objects, the player lost an arrow
		if (other.transform.name == "Cube") {
			bow.GetComponent<bowAndArrow>().createArrow(false);
			Destroy(gameObject);
		}

		// Ok - 
		// we hit the target
		if (other.transform.name == "target") {
			// play the audio file ("trrrrr")
			GetComponent<AudioSource>().PlayOneShot(targetHit);
			// set velocity to zero
			GetComponent<Rigidbody>().velocity = Vector3.zero;
			// disable the rigidbody
			GetComponent<Rigidbody>().isKinematic = true;
			transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
			// and a collision occurred
			collisionOccurred = true;
			// disable the arrow head to create optical illusion
			// that arrow hit the target
			arrowHead.SetActive(false);
			// though there may be more than one contact point, we take 
			// the first one in order
			y = other.contacts[0].point.y;
			// y is the absolute coordinate on the screen, not on the collider, 
			// so we subtract the collider's position
			y = y - other.transform.position.y;

			// we hit at least white...
			if (y < 1.48557f && y > -1.48691f)
				actScore = 10;
			// ... it could be black, too ...
			if (y < 1.36906f && y > -1.45483f)
				actScore = 20;
			// ... even blue is possible ...
			if (y < 0.9470826f && y > -1.021649f)
				actScore = 30;
			// ... or red ...
			if (y < 0.6095f && y > -0.760f)
				actScore = 40;
			// ... or gold !!!
			if (y < 0.34f && y > -0.53f)
				actScore = 50;

			// create a rising text for score display
			GameObject rt = (GameObject)Instantiate(risingText, new Vector3(0,0,0),Quaternion.identity);
			rt.transform.position = other.transform.position + new Vector3(-1,1,0);
			rt.transform.name = "rt";
			rt.GetComponent<TextMesh>().text= "+"+actScore;
			// inform the master script about the score
			bow.GetComponent<bowAndArrow>().setPoints(actScore);
		}
	}


	//
	// public void setBow
	//
	// set a reference to the main game object 

	public void setBow(GameObject _bow) {
		bow = _bow;
	}
}
