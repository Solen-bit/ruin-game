using UnityEngine;
using MoreMountains.Feedbacks;

// Arena gate class, which handles the gate opening and closing
public class ArenaGate : MonoBehaviour
{
    [SerializeField] private BurstSpawnArea _burstSpawnArea; // Burst spawn area for the gate
    [SerializeField] private MMF_Player _gateFeedbacksOpen; // Feedback for the gate opening
    [SerializeField] private MMF_Player _gateFeedbacksClose; // Feedback for the gate closing

    private Animator _animator; // Animator for the gate

    // Awake is called before the first frame update
    private void Awake()
    {
        _animator = GetComponent<Animator>(); // Get the animator
    }

    // Start is called on the first frame update
    private void Start()
    {
        // Add a listener to the OnGate event in the BurstSpawnArea class
        // that calls the GateAction function when the event is triggered
        _burstSpawnArea.OnGate.AddListener(GateAction);
    }

    /// <summary>
    /// The GateAction function opens or closes a gate and plays feedback accordingly.
    /// </summary>
    /// <param name="gateOpen">A boolean variable that determines whether the gate should be opened or
    /// closed. If it is true, the gate will be opened, and if it is false, the gate will be
    /// closed.</param>
    private void GateAction(bool gateOpen)
    {
        if (gateOpen)
        {
            _animator.SetBool("Open", true);
            _gateFeedbacksOpen?.PlayFeedbacks();
        }
        else
        {
            _animator.SetBool("Open", false);
            _gateFeedbacksClose?.PlayFeedbacks();
        }
    }
}
