using UnityEngine.SceneManagement;
using UnityEngine;

namespace Assets.SceneManagement
{
    public class RestartLevel : MonoBehaviour
    {
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.R))
            {
                ReloadScene();
            }
        }

        public void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
