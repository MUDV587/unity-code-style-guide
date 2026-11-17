using UnityEngine;
using UnityEngine.UIElements;

namespace UnityStarter.SceneTemplates
{
    /// <summary>Demonstrates scene-local UI selection and modal focus without a game session.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class PreviewScreen : MonoBehaviour
    {
        private const string HiddenClass = "is-hidden";
        private VisualElement _root;
        private VisualElement _modal;
        private Button _openButton;
        private Button _closeButton;
        private Button _selectButton;
        private Label _status;
        private VisualElement _workspace;

        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _modal = _root.Q("modalOverlay");
            _workspace = _root.Q("workspace");
            _openButton = _root.Q<Button>("openModal");
            _closeButton = _root.Q<Button>("closeModal");
            _selectButton = _root.Q<Button>("selectItem");
            _status = _root.Q<Label>("status");
            _openButton.clicked += OpenModal;
            _closeButton.clicked += CloseModal;
            _selectButton.clicked += SelectItem;
            _root.RegisterCallback<KeyDownEvent>(HandleKeyDown, TrickleDown.TrickleDown);
            CloseModal();
        }

        private void OnDisable()
        {
            _openButton.clicked -= OpenModal;
            _closeButton.clicked -= CloseModal;
            _selectButton.clicked -= SelectItem;
            _root.UnregisterCallback<KeyDownEvent>(HandleKeyDown, TrickleDown.TrickleDown);
        }

        private void OpenModal()
        {
            _workspace.SetEnabled(false);
            _modal.RemoveFromClassList(HiddenClass);
            _closeButton.Focus();
        }

        private void CloseModal()
        {
            _modal.AddToClassList(HiddenClass);
            _workspace.SetEnabled(true);
            _openButton.Focus();
        }

        private void SelectItem()
        {
            _status.text = "Selected: Workshop / production building";
        }

        private void HandleKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Escape && !_modal.ClassListContains(HiddenClass))
            {
                CloseModal();
                evt.StopPropagation();
            }
        }
    }
}
