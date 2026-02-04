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
        [SerializeField] private GameObject _speakingIndicator; // "Click to Continue" object

        [Header("Settings")]
        [SerializeField] private float _typingSpeed = 0.04f;
        [SerializeField] private AudioClip _typingSound;
        [SerializeField] private AudioSource _audioSource;

        private Coroutine _typingCoroutine;
        private Queue<string> _sentences = new Queue<string>();
        private bool _isTyping = false;
        private string _currentSentence = "";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null); // Detach from Canvas to be a valid Root for DontDestroyOnLoad
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            Hide();
        }

        private void Update()
        {
            if (_panel == null || !_panel.activeSelf) return;

            // Check for click or space
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                if (_isTyping)
                {
                    // Complete instantly
                    if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
                    _textMesh.text = _currentSentence;
                    _textMesh.ForceMeshUpdate(); // CRITICAL FIX
                    _isTyping = false;
                    
                    // Show Indicator
                    if (_speakingIndicator != null) _speakingIndicator.gameObject.SetActive(true);
                }
                else
                {
                    DisplayNextSentence();
                }
            }
        }

        public void ShowMessage(List<string> messages)
        {
            // Block Input
            var player = FindFirstObjectByType<Scream2D.Controllers.PlayerController>();
            if (player != null) player.SetInputBlocked(true);

            _sentences.Clear();
            foreach (string sentence in messages)
            {
                _sentences.Enqueue(sentence);
            }

            if (_panel != null) 
            {
                _panel.SetActive(true);
                // Reset text immediately
                if (_textMesh != null) 
                {
                    _textMesh.text = "";
                    _textMesh.SetAllDirty();
                }
                
                // Force Update
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(_panel.GetComponent<RectTransform>());
            }
            
            // Start the flow with a slight delay to allow UI to wake up
            StartCoroutine(StartDialogueSequence());
        }

        private IEnumerator StartDialogueSequence()
        {
            yield return null; // Wait for one frame for layout to settle
            DisplayNextSentence();
        }

        public void DisplayNextSentence()
        {
            if (_speakingIndicator != null) _speakingIndicator.gameObject.SetActive(false);

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
            // Unblock Input
            var player = FindFirstObjectByType<Scream2D.Controllers.PlayerController>();
            if (player != null) player.SetInputBlocked(false);

            if (_panel != null) _panel.SetActive(false);
            if (_textMesh != null) 
            {
                _textMesh.text = "";
                _textMesh.SetAllDirty(); 
            }
            _isTyping = false;
            
            if (_speakingIndicator != null) _speakingIndicator.gameObject.SetActive(false);

            // DETACH from input loop to prevent accidental re-trigger
            StopAllCoroutines(); 
        }

        private IEnumerator TypeText(string message)
        {
            yield return null; // Safety wait

            _isTyping = true;
            _textMesh.text = "";
            
            // Initial FORCE because usually the first character fails
            _textMesh.SetAllDirty();
            
            foreach (char letter in message.ToCharArray())
            {
                _textMesh.text += letter;
                // _textMesh.ForceMeshUpdate(); // Can be expensive every frame, SetAllDirty is usually safer for TMP
                
                if (_audioSource != null && _typingSound != null)
                {
                    _audioSource.pitch = Random.Range(0.95f, 1.05f);
                    _audioSource.PlayOneShot(_typingSound);
                }

                yield return new WaitForSeconds(_typingSpeed);
            }
            _isTyping = false;
            
            // Show Indicator
            if (_speakingIndicator != null) _speakingIndicator.gameObject.SetActive(true);
        }
    }
}
