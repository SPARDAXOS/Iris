using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Initialization {
    public class Initializer {

        //TODO: Load GameInstance using addressables

        [RuntimeInitializeOnLoadMethod]
        private static void InitializeGame() {
            var resource = Resources.Load<GameObject>("GameInstance");
            GameObject game = Object.Instantiate(resource);
            Object.DontDestroyOnLoad(game);

            GameInstance gameInstance = game.GetComponent<GameInstance>();
            gameInstance.Initialize();
        }



    }
}
