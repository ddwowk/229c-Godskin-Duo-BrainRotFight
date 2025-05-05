using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button crediteBT;

    private void Awake()
    {
        crediteBT?.onClick.AddListener(() => { Application.OpenURL("https://ddwowk.github.io/GI204Credits/GI204.html?fbclid=IwZXh0bgNhZW0CMTEAAR1NYWmsxmb1yDsurbsytsBrr3tz9axXZthghmkVlperPxNkzP-bdxftYuA_aem_ZT0TPiYcVtURXZN20fzOLw"); });
    }
    public void OnClickLoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
