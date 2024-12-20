using UnityEngine;
using UnityEngine.UI;
namespace Tactility.Task
{
    public class LoadingIndicator : MonoBehaviour
    {
        [SerializeField] private Image loadingBarFill;
        [SerializeField] private Image loadingBarBackground;
        [SerializeField] private Canvas loadingCanvas;

        private float targetProgress = 0;

        private void Start()
        {
            HideLoadingIndicator();
        }

        // This method can be called to update the loading progress
        public void UpdateProgress(float progress)
        {
            targetProgress = Mathf.Clamp01(progress); // Ensures the value is between 0 and 1
            UpdateUI();
        }

        private void UpdateUI()
        {
            loadingBarFill.fillAmount = targetProgress;
        }


        public void ShowLoadingIndicator(Vector3 targetPos)
        {
            loadingCanvas.gameObject.SetActive(true);
            loadingBarBackground.transform.position = targetPos;
        }

        public void HideLoadingIndicator()
        {
            loadingCanvas.gameObject.SetActive(false);
        }


    }
}
