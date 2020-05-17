using UnityEngine;

namespace Game
{
    public class GateOpener : MonoBehaviour
    {
        private static readonly int Trigger = Animator.StringToHash("Trigger");

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                GetComponent<Animator>().SetTrigger(Trigger);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                GetComponent<Animator>().SetTrigger(Trigger);
            }
        }
    }
}
