using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChange : MonoBehaviour
{

    [SerializeField] private string sceneName; //make sure target scene is in build settings

    [SerializeField] private Button btn;

    private void Start()
    {
        btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(NewGame);
    }

    private void NewGame()
    {
        if(PermanentUI.perm != null)
            PermanentUI.perm.Reset();
        SceneManager.LoadScene(sceneName);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            // Instead of loading scene directly, show the level complete panel
            if (MenuManager.Instance != null)
            {
                MenuManager.Instance.ShowLevelComplete(sceneName);
            }
            else
            {
                // Fallback if MenuManager is missing
                SceneManager.LoadScene(sceneName); 
            }
        }
    }


    public void ChangeScenes()
    {
        SceneManager.LoadScene(sceneName);
    }
}
