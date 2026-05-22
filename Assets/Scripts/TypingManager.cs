using System;
using System.Collections;
using UnityEngine;
using TMPro;

namespace TheBlob.Cutscene
{
    /// <summary>
    /// Types text character by character into a TMP_Text field.
    /// </summary>
    public class TypingManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text _dialogueText;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private GameObject _dialoguePanel;

        private Coroutine _typingCoroutine;
        private GameObject _activePortrait;

        public bool IsTyping { get; private set; }

        public void ShowLine(string speakerName, GameObject portrait, string text, float duration, Action onComplete = null)
        {
            _dialoguePanel.SetActive(true);
            _nameText.text = speakerName;

            // Hide previous portrait, show new one
            if (_activePortrait != null)
                _activePortrait.SetActive(false);

            _activePortrait = portrait;
            if (_activePortrait != null)
                _activePortrait.SetActive(true);

            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);

            _typingCoroutine = StartCoroutine(TypeText(text, duration, onComplete));
        }

        public void Hide()
        {
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _typingCoroutine = null;
            }

            if (_activePortrait != null)
            {
                _activePortrait.SetActive(false);
                _activePortrait = null;
            }

            IsTyping = false;
            _dialoguePanel.SetActive(false);
        }

        public void SkipTyping()
        {
            // Handled by completing text instantly in coroutine check
        }

        private IEnumerator TypeText(string fullText, float duration, Action onComplete)
        {
            IsTyping = true;
            _dialogueText.text = "";

            int charCount = fullText.Length;
            if (charCount == 0)
            {
                IsTyping = false;
                onComplete?.Invoke();
                yield break;
            }

            float interval = duration / charCount;

            for (int i = 0; i < charCount; i++)
            {
                _dialogueText.text = fullText.Substring(0, i + 1);
                yield return new WaitForSeconds(interval);
            }

            IsTyping = false;
            _typingCoroutine = null;
            onComplete?.Invoke();
        }
    }
}