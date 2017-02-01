using UnityEngine;
using System.Collections;

public class camMovement : MonoBehaviour {

	public GameObject arrow;

	// Use this for initialization
	void Start () {
		arrow = null;
	}

	public void setArrow(GameObject _arrow) {
		arrow = _arrow;
	}

	public void resetCamera() {
		transform.position = new Vector3 (0, 0, -9.32f);
	}

	// Update is called once per frame
	void Update () {
		if (arrow != null) {
			Vector3 position = transform.position;
			float z = position.z;
			position = Vector3.Lerp (transform.position, arrow.transform.position, Time.deltaTime);
			position.z = z;
			transform.position = position;
		}
	}
}
