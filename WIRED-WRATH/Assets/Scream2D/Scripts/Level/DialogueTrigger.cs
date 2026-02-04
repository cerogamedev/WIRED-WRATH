using UnityEngine;
using Scream2D.UI;

namespace Scream2D.Level
{
    public class DialogueTrigger : MonoBehaviour
    {
        [TextArea(3, 10)]
        public System.Collections.Generic.List<string> Messages = new System.Collections.Generic.List<string>() { "Hello stranger..." };

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                DialogueUI.Instance?.ShowMessage(Messages);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                DialogueUI.Instance?.Hide();
            }
        }
    }
}
