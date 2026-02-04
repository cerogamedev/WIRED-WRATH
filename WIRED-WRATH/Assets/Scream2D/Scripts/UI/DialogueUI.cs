using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Scream2D.UI
{
    public class DialogueUI : MonoBehaviour
    {
        public static DialogueUI Instance { get; private set; }

        [Header("UI Elements")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _textMesh;
        [SerializeField] private Image _speakingIndicator; // Optional: Icon for who is speaking

        [Header("Settings")]
        [SerializeField] private float _typingSpeed = 0.04f;
        [SerializeField] private AudioClip _typingSound;
        [SerializeField] private AudioSource _audioSource;

        private Queue<string> _sentences = new Queue<string>();
        private bool _isTyping = false;
        private string _currentSentence = "";

        private void Update()
        {
            if (_panel == null || !_panel.activeSelf) return;

            // Check for click or space
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                if (_isTyping)
                {
                    // Complete instantly
                    StopAllCoroutines();
                    _textMesh.text = _currentSentence;
                    _isTyping = false;
                }
                else
                {
                    DisplayNextSentence();
                }
            }
        }

        public void ShowMessage(List<string> messages)
        {
            _sentences.Clear();
            foreach (string sentence in messages)
            {
                _sentences.Enqueue(sentence);
            }

            if (_panel != null) _panel.SetActive(true);
            DisplayNextSentence();
        }

        public void DisplayNextSentence()
        {
            if (_sentences.Count == 0)
            {
                Hide();
                return;
            }

            _currentSentence = _sentences.Dequeue();
            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
            _typingCoroutine = StartCoroutine(TypeText(_currentSentence));
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
            if (_textMesh != null) _textMesh.text = "";
            _isTyping = false;
        }

        private IEnumerator TypeText(string message)
        {
            _isTyping = true;
            _textMesh.text = "";
            
            foreach (char letter in message.ToCharArray())
            {
                _textMesh.text += letter;
                
                if (_audioSource != null && _typingSound != null)
                {
                    _audioSource.pitch = Random.Range(0.95f, 1.05f);
                    _audioSource.PlayOneShot(_typingSound);
                }

                yield return new WaitForSeconds(_typingSpeed);
            }
            _isTyping = false;
        }
    }
}
