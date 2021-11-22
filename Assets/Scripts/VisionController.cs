using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionController : MonoBehaviour
{
	[SerializeField] private TMPro.TextMeshProUGUI test_debug_text_1;
	[SerializeField] private TMPro.TextMeshProUGUI test_debug_text_2;

	[SerializeField] private Camera testCamera;

	public enum VisionState
	{
		Normal,
		View1,
		View2,
		View3
	}

	// have public event for OnCurrentVisionStateChange?
	private VisionState currentVisionState = VisionState.Normal;
	private bool visionStateChangeRequired = false;

	private enum SwipeDirection
	{
		Right,
		Left,
		Up,
		Down,
		NONE
	}

	private enum ControllerSwiped
	{
		Primary,
		Secondary,
		NONE
	}
	private ControllerSwiped controllerSwiped = ControllerSwiped.NONE;

	private const float STATE_CHANGE_TIMEOUT = 1.0f;			// can't change state again until 1.0s after changing
	private const float PHYSICAL_SWIPE_TIME_LENGTH = 1.0f;		// physical swipe must take less than 1.0s to complete

	//private bool isTouchController = true;
	private bool isStateChangeTimeoutReset = true;
	private bool isStickMagnitudeReset = true;      // also used by hands for "virtual" stick magnitude
	
	private float primaryStickAngle;
	private float secondaryStickAngle;

    void Update()
    {
		//isTouchController = (OVRInput.GetConnectedControllers() & OVRInput.Controller.Touch) == OVRInput.Controller.Touch;

		if (CheckCanChangeState() == false) return;

		//if (isTouchController) ControllersUpdate();
		//else HandsUpdate();
		ControllersUpdate();

		if (visionStateChangeRequired) ChangeVision();
    }

	private bool CheckCanChangeState()
	{
		if (isStickMagnitudeReset == false)
		{
			//if (isTouchController)
			//{
				if (controllerSwiped == ControllerSwiped.Primary)
				{
					if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).magnitude < 0.2f)
					{
						isStickMagnitudeReset = true;
					}
				}
				else
				{
					if (OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).magnitude < 0.2f)
					{
						isStickMagnitudeReset = true;
					}
				}
			//}
			//else 
			//{
			//	// if player no longer pressing virtual swipe button (or whatever the mechanic is)
			//	//isStickMagnitudeReset = true;
			//}
		}

		//test_debug_text_1.text = $"[{isStateChangeTimeoutReset} / {isStickMagnitudeReset}]";

		return isStateChangeTimeoutReset && isStickMagnitudeReset;
	}

	private void ControllersUpdate()
	{
		// get and store each stick magnitude here
		// rename vars from Left/Right to Primary/Secondary

		if (OVRInput.Get(OVRInput.Button.One) || OVRInput.Get(OVRInput.Button.Three))
		{
			// these look a bit clunky?
			if (CheckControllerSwipe(SwipeDirection.Right) == SwipeDirection.Right)
			{
				currentVisionState = (VisionState)(((int)currentVisionState + 1) % 4);
				visionStateChangeRequired = true;
			}
			else if (CheckControllerSwipe(SwipeDirection.Left) == SwipeDirection.Left)
			{
				currentVisionState = (VisionState)(((int)currentVisionState - 1) % 4);
				visionStateChangeRequired = true;
			}
		}
	}

	private void HandsUpdate()
	{

	}

	private SwipeDirection CheckControllerSwipe(SwipeDirection direction)
	{
		Vector2 angleDirection = Vector2.zero;
		if (direction == SwipeDirection.Right)
		{
			angleDirection = Vector2.right;
		}
		else if (direction == SwipeDirection.Left)
		{
			angleDirection = Vector2.left;
		}

		primaryStickAngle = Vector2.Angle(OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick), angleDirection);
		secondaryStickAngle = Vector2.Angle(OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick), angleDirection);
		bool hasPrimaryStickMoved = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).magnitude > 0.7f;
		bool hasSecondaryStickMoved = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).magnitude > 0.7f;

		if (hasPrimaryStickMoved && primaryStickAngle > -40.0f && primaryStickAngle < 40.0f)
		{
			controllerSwiped = ControllerSwiped.Primary;
			return direction;
		}

		if (hasSecondaryStickMoved && secondaryStickAngle > -40.0f && secondaryStickAngle < 40.0f)
		{
			controllerSwiped = ControllerSwiped.Secondary;
			return direction;
		}

		return SwipeDirection.NONE;
	}

	private void ChangeVision()
	{
		StartCoroutine(ResetChangeTimeout());

		test_debug_text_2.text = $"SETTING VISION TO {currentVisionState}";

		// set everything to currentVisionState (tags, layers, camera, etc)
		switch (currentVisionState)
		{
			case VisionState.Normal:
				testCamera.backgroundColor = Color.grey;
				break;
			case VisionState.View1:
				testCamera.backgroundColor = Color.red;
				break;
			case VisionState.View2:
				testCamera.backgroundColor = Color.green;
				break;
			case VisionState.View3:
				testCamera.backgroundColor = Color.blue;
				break;
			default:
				break;
		}
	}

	private IEnumerator ResetChangeTimeout()
	{
		isStickMagnitudeReset = false;
		visionStateChangeRequired = false;
		controllerSwiped = ControllerSwiped.NONE;
		
		isStateChangeTimeoutReset = false;
		yield return new WaitForSeconds(STATE_CHANGE_TIMEOUT);
		isStateChangeTimeoutReset = true;
	}

	// ########################################################
	// ############# public vars/functions ####################
	// ########################################################

	public bool HasStartedPhysicalSwipe { get; private set; } = false;
	public bool SwipeTimedOut = false;
	private DateTime physicalSwipeStartTime;

	public void StartPhysicalSwipe()
	{
		HasStartedPhysicalSwipe = true;
		physicalSwipeStartTime = DateTime.Now;
		StartCoroutine(StartSwipeTimeout());
	}

	public void EndPhysicalSwipe()
	{
		StopCoroutine(StartSwipeTimeout());
		if ((DateTime.Now - physicalSwipeStartTime).TotalSeconds <= PHYSICAL_SWIPE_TIME_LENGTH)
		{
			visionStateChangeRequired = true;
		}
		// reset all physical swipe related vars
		HasStartedPhysicalSwipe = false;
		SwipeTimedOut = false;

		test_debug_text_2.text = "EndSwipe (succ)";
	}

	private IEnumerator StartSwipeTimeout()
	{
		yield return new WaitForSeconds(PHYSICAL_SWIPE_TIME_LENGTH);
		SwipeTimedOut = true;

		test_debug_text_2.text = "Swipe timed out";
	}
}
