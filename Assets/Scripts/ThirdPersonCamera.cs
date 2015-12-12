using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BarsEffect))]
public class ThirdPersonCamera : MonoBehaviour {

	[SerializeField]
	private float distanceAway;
	//fixed distance away from the back of the character
	[SerializeField]
	private float distanceUp;
	//fixed distance up from the character 
	[SerializeField]
	private float smooth;
	//the time it takes for the camera to adjust to where it should face the character
	[SerializeField]
	private Transform followXForm;
	//a child of the beta. Allows use to follow this instead of the beta rigid body
	[SerializeField]
	private float wideScreen = 0.2f;
	[SerializeField]
 	private float targetingTime = 0.5f;
	//time it takes for the bars to tween down
	[SerializeField]
	private Vector3 offset = new Vector3(0f, 1.5f, 0f);
	[SerializeField]
	private float camSmoothDampTime = 0.1f;


	//private global variables
	private Vector3 lookDir;
	private Vector3 targetPosition;
	//where we try to target the position of the camera
	private BarsEffect barEffect;
	private CamStates camState = CamStates.Behind;
	//default camera state
	

	
	public enum CamStates
	{
		Behind,
		FirstPerson,
		Target,
		free
		
	}

	//smoothing and damping
	private Vector3 velocityCamSmooth = Vector3.zero;

	// Use this for initialization
	void Start () {
	
		followXForm = GameObject.FindWithTag ("Player").transform;
		//changes in respect of the player's transform using the tag
		lookDir = followXForm.forward;
		
		barEffect = GetComponent<BarsEffect>();
		if(barEffect == null)
		{
			Debug.LogError("Attach a widescreen BarsEffect script to the camera.", this);
			//attaches a message if error
		}
		
		
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	void OnDrawGizmos()
	{

	}

	void LateUpdate()
	{
		//A distance away from the character
		Vector3 characterOffset = followXForm.position + new Vector3(0f, distanceUp, 0f);

		//determine camera state
		if(Input.GetAxis("Target") > 0.01f)
		{
			barEffect.coverage = Mathf.SmoothStep(barEffect.coverage, wideScreen, targetingTime);
			
			camState = CamStates.Target;
		}
		else
		{
			barEffect.coverage = Mathf.SmoothStep(barEffect.coverage, 0f, targetingTime);
			
			camState = CamStates.Behind;
		}
		
		switch(camState)
		{
			case CamStates.Behind:
				//calculate direction of the camera in relation to the player
				lookDir = characterOffset - this.transform.position;
				//vector from the head of the character to the camera
				lookDir.y = 0;
				lookDir.Normalize();
				//kill the y value and normalize and this gives us the direction
				Debug.DrawRay (this.transform.position, lookDir, Color.green);
		
				//setting the position of the camera be doing vector addition
				targetPosition = characterOffset + followXForm.up * distanceUp - lookDir * distanceAway;
				Debug.DrawRay (followXForm.position, Vector3.up * distanceUp, Color.red);
				Debug.DrawRay (followXForm.position, -1f * followXForm.forward * distanceAway, Color.blue);
				Debug.DrawLine (followXForm.position, targetPosition, Color.magenta);
				//targetPosition = characterOffset + followXForm.up * distanceUp - lookDir * distanceAway;
			break;	
			
			case CamStates.Target:
				lookDir = followXForm.forward;
				
			break;	
		}
		targetPosition = characterOffset + followXForm.up * distanceUp - lookDir * distanceAway;
		CompensateForWalls (characterOffset, ref targetPosition);

		smoothPosition(this.transform.position, targetPosition);
		//targetPosition = characterOffset + followXForm.up * distanceUp - lookDir * distanceAway;

		//camera faces the character
		transform.LookAt (followXForm);


	}


	private void smoothPosition(Vector3 fromPos, Vector3 toPos)
	{
		this.transform.position = Vector3.SmoothDamp (fromPos, toPos, ref velocityCamSmooth, camSmoothDampTime);
	}

	//method that pushes the camera against the wall instead of going over
	private void CompensateForWalls(Vector3 fromObject, ref Vector3 toTarget)
	{
		Debug.DrawLine (fromObject, toTarget, Color.cyan);

		RaycastHit wallHit = new RaycastHit ();

		if (Physics.Linecast (fromObject, toTarget, out wallHit)) 
		{
			Debug.DrawRay(wallHit.point, Vector3.left, Color.red);
			toTarget = new Vector3(wallHit.point.x, toTarget.y, wallHit.point.z);
		}
	}
}
