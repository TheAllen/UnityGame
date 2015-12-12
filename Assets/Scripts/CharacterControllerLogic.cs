
using UnityEngine;
using System.Collections;


/// <summary>
/// #DESCRIPTION OF CLASS#
/// </summary>
public class CharacterControllerLogic : MonoBehaviour 
{
	#region Variables (private)

	[SerializeField]
	private Animator animator;
	//reference to character controller	
	[SerializeField]
	private float directionDampTime = 0.25f;
	//sets a delay from each action
	[SerializeField]
	private ThirdPersonCamera gamecam;
	[SerializeField]
	private float directionSpeed = 3.0f;
	[SerializeField]
	private float rotationDegreePerSecond = 120f;

	//private global variables
	private float speed = 0.0f;
	private float direction = 0.0f;
	private float horizontal = 0.0f;
	private float vertical = 0.0f;
	private AnimatorStateInfo stateInfo;

	//hashes
	private int m_LocomotionId = 0;

	#endregion

	#region Properties (public)

	#endregion

	#region Unity event

	void Start()
	{
		//checks if our animator exists grab it if so
		animator = GetComponent<Animator> ();

		//sets the animation controller
		if (animator.layerCount >= 2) 
		{
			animator.SetLayerWeight(1,1);
		}

		m_LocomotionId = Animator.StringToHash ("Base Layer.Locomotion");
		//sets the hashes

	}

	void Update()
	{
		//checks if animator controller exists then grab it if so
		if (animator) 
		{
			stateInfo = animator.GetCurrentAnimatorStateInfo(0);

			//pulls values from the controller input
			horizontal = Input.GetAxis ("Horizontal");
			vertical = Input.GetAxis ("Vertical");

			//gets the speed 
			//speed = h * h + v * v;
			//speed = new Vector2(horizontal, vertical).sqrMagnitude;
			//sets the direction where the character wants to go
			StickToWorldspace(this.transform, gamecam.transform, ref direction, ref speed);



			//animation
			animator.SetFloat("Speed", 2 * speed);
			//sets speed from the script and supplying to the CharacterController
			animator.SetFloat("Direction", direction, directionDampTime, Time.deltaTime);
			//sets direction from the script and supplying to the parameter in Unity

		}
	}

	//This function is called every fixed framerate frame
	void FixedUpdate()
	{
		if(IsInLocomotion() && ((direction >= 0 && horizontal >= 0) || direction < 0 && horizontal < 0))
		{
			Vector3 rotationAmount = Vector3.Lerp(Vector3.zero, new Vector3(0f, rotationDegreePerSecond * (horizontal < 0f ? -1f : 1f), 0f), Mathf.Abs(horizontal));
			Quaternion deltaRotation = Quaternion.Euler(rotationAmount * Time.deltaTime);
			this.transform.rotation = (this.transform.rotation * deltaRotation);
		}
	}

	#endregion

	#region Methods

	//translate our controls to worldspace
	public void StickToWorldspace(Transform root, Transform camera, ref float directionOut, ref float speedOut)
	{
		Vector3 rootDirection = root.forward;

		Vector3 stickDirection = new Vector3 (horizontal, 0.0f, vertical);

		//grabing the square magnitude of the vector from the x,y,z coordinate
		speedOut = stickDirection.sqrMagnitude;

		Vector3 CameraDirection = camera.forward;
		CameraDirection.y = 0.0f; // kill y
		//Creates a rotation which rotates from the fromDirection to the toDirection
		Quaternion referentialShift = Quaternion.FromToRotation (Vector3.forward, CameraDirection);

		Vector3 moveDirection = referentialShift * stickDirection;
		Vector3 axisSign = Vector3.Cross (moveDirection, rootDirection);

		Debug.DrawRay (new Vector3 (root.position.x, root.position.y + 2f, root.position.z), moveDirection, Color.green);
		Debug.DrawRay (new Vector3 (root.position.x, root.position.y + 2f, root.position.z), rootDirection, Color.magenta);
		Debug.DrawRay (new Vector3 (root.position.x, root.position.y + 2f, root.position.z), stickDirection, Color.blue);

		float angleRootToMove = Vector3.Angle (rootDirection, moveDirection) * (axisSign.y >= 0 ? -1f : 1f);

		angleRootToMove /= 180f;

		directionOut = angleRootToMove * directionSpeed;

	}

	public bool IsInLocomotion()
	{
		return stateInfo.nameHash == m_LocomotionId;
	}

	#endregion Methods
}
