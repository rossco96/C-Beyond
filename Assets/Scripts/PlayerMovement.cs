#define DEBUG_CONTROLS

using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	[SerializeField] private float moveSpeed;
	[SerializeField] private float rotSpeed;

	private Transform t;
	private float axisVertical;
	private float axisHorizontal;

	private void Awake()
	{
		t = gameObject.transform;
	}

	private void FixedUpdate()
    {
#if DEBUG_CONTROLS
		CheckForKeyboardInput();
#else
		CheckForOculusInput();
#endif
	}

	private void CheckForOculusInput()
	{

	}

	private void CheckForKeyboardInput()
	{
		axisVertical = Input.GetAxis("Vertical");
		axisHorizontal = Input.GetAxis("Horizontal");

		if (axisVertical != 0)
		{
			t.localPosition += t.forward * axisVertical * moveSpeed * Time.deltaTime;
		}
		if (axisHorizontal != 0)
		{
			t.Rotate(Vector3.up * axisHorizontal * rotSpeed * Time.deltaTime);
		}
	}
}
