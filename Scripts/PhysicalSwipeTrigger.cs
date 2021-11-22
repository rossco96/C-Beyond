using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class PhysicalSwipeTrigger : MonoBehaviour
{
	[SerializeField] private TMPro.TextMeshProUGUI test_debug_text_1;
	[SerializeField] private TMPro.TextMeshProUGUI test_debug_text_2;

	private enum TriggerSide
	{
		Right,
		Left,
		None
	}
	[SerializeField] private TriggerSide triggerSide;
	[SerializeField] private VisionController controller;

	private TriggerSide startingSide = TriggerSide.None;

	//private void Update()
	//{
	//	if (controller.HasStartedPhysicalSwipe && controller.SwipeTimedOut)
	//	{
	//		EndSwipe(false);
	//	}
	//}

	private void OnTriggerEnter(Collider col)
	{
		test_debug_text_2.text = $"STARTED SWIPE [{col.gameObject.name}/{triggerSide}]";

		if (controller.HasStartedPhysicalSwipe)
		{
			EndSwipe(true);
		}
		else
		{
			StartSwipe();
		}
	}

	private void StartSwipe()
	{
		controller.StartPhysicalSwipe();
		startingSide = triggerSide;
	}

	private void EndSwipe(bool success)
	{
		if (triggerSide == startingSide) return;
		controller.EndPhysicalSwipe();
		startingSide = TriggerSide.None;
	}
}
