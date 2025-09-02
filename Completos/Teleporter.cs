using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
public class LevelLoader : MonoBehaviour
{
    public GameObject player;
    private bool dentroDaPorta = false; // flag para saber se est� dentro do collider
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (player != null)
        {
            DontDestroyOnLoad(player);
        }
    }
    private void Update()
    {
        // S� interage se estiver dentro da porta e apertar o bot�o
        if (dentroDaPorta && Input.GetButtonDown("Interact"))
        {
            StartCoroutine(LoadYourAsyncScene(1));
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Porta"))
        {
            dentroDaPorta = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Porta"))
        {
            dentroDaPorta = false;
        }
    }
    private IEnumerator LoadYourAsyncScene(int sceneIndex)
    {
        Scene cenaAtual = SceneManager.GetActiveScene();
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        Scene novaCena = SceneManager.GetSceneByBuildIndex(sceneIndex);
        SceneManager.MoveGameObjectToScene(player, novaCena);
        SceneManager.UnloadSceneAsync(cenaAtual);
    }
}