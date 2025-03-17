using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UB.UI
{
    public class SceneUI : UIView
    {
        public GameObject Panal;
        public GameObject buttonObject;
        public GameObject GridObject;
        // Start is called before the first frame update
        private void Start()
        {
            int a = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < a; i++)
            {
                var go = Instantiate(buttonObject, GridObject.transform);
                go.GetComponent<TextMeshProUGUI>().text = "Scene " + i;
                int index = i;
                go.GetComponentInChildren<Button>().onClick.AddListener(() => ClickButton(index));
                go.SetActive(true);
            }
            GridObject.GetComponent<RectTransform>().sizeDelta = new Vector2(GridObject.GetComponent<RectTransform>().sizeDelta.x, 80 * a);
        }
        public void ClickButton(int SceneNum)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNum);
            TimeManager.Instance.SetForceTimeScale(1);
            TimeManager.Instance.AbleSetTimeScale = true;
        }

        public override void EnableAction()
        {
            
        }
    }

}