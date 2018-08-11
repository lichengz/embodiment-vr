using UnityEngine;
using System.Collections;

public class InputForcusSystemOverlaySample : MonoBehaviour
{

	public GameObject[] hands;
	public GameObject nearObjectOwner;

	private void OnInputFocusAcquired()
	{
		Debug.Log("Input Focus Acquired, the application is the foreground application and should receive input");
		foreach (GameObject hand in hands)
		{
			hand.SetActive(true);
		}
	}

	private void OnInputFocusLost()
	{
		Debug.Log("Input Focus Lost,  the application is in the background, should hide any input representations such as hands");

		foreach (GameObject hand in hands)
		{
			hand.SetActive(false);
		}
	}

	private void OnSystemOverlayPresented()
	{
		Debug.Log("System Overlay Presented, such as a dashboard. In this case the application  should pause while still drawing, avoid drawing near-field graphics so they do not visually fight with the system overlay, and consume fewer CPU and GPU resources.");

		float hideObjectsInRange = 3.0f;
		Transform parentTransformObj = nearObjectOwner.transform;

		for (int i = 0; i < parentTransformObj.childCount; i++)
		{
			Transform child = parentTransformObj.GetChild(i);

			if ((child.position - Camera.current.transform.position).magnitude < hideObjectsInRange)
				child.gameObject.GetComponent<Renderer>().enabled = false;
		}

		// Pause animations
		Time.timeScale = 0.0f;

		// Pause the sound/music
		GetComponent<AudioSource>().enabled = false;

	}

	private void OnSystemOverlayHide()
	{
		Debug.Log("System Overlay is invisible, resume the game");
		Transform parentTransformObj = nearObjectOwner.transform;

		for (int i = 0; i < parentTransformObj.childCount; i++)
		{
			Transform child = parentTransformObj.GetChild(i);
			child.gameObject.GetComponent<Renderer>().enabled = true;
		}

		// Resume animations
		Time.timeScale = 1.0f;

		// Resume the sound/music
		GetComponent<AudioSource>().enabled = true;
	}

	// Use this for initialization
	void Start()
	{
		OVRManager.InputFocusAcquired += OnInputFocusAcquired;
		OVRManager.InputFocusLost += OnInputFocusLost;
		OVRManager.SystemOverlayPresented += OnSystemOverlayPresented;
		OVRManager.SystemOverlayHide += OnSystemOverlayHide;
	}

	// Update is called once per frame
	void Update()
	{

	}
}
