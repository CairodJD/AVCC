using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CotcSdk;
using UnityEditor;

public class GameManager : MonoBehaviour {

    public static GameManager GM {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<GameManager>();
                DontDestroyOnLoad(_instance);
            }
            return _instance;
        }
    }
    private static GameManager _instance;

    private Button replaybutton;

    private void OnEnable() {
        SceneManager.sceneLoaded += OnsceneLoaded;
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= OnsceneLoaded;
    }

    private bool reset = false;


    //SETTING LAST GAMER WITH ID : 601b3d69983902137b692bf2 and secret : b059c9a992c6145627d18e6ca6eab2d2556f1411


    private void Awake() {
        if (reset == true) {
            PlayerPrefs.DeleteAll();
        } else {
            Debug.Log("last gamer " + PlayerPrefs.GetString(LAST_GAMERID_KEY));
        }
        if (!_instance) {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }


    private void OnsceneLoaded(Scene scene, LoadSceneMode mode) {
        switch (scene.buildIndex) {
            case 0 :
                
                InitCloud();
                break;
            case 1 :
                //InitCloud();
                first_time = Time.time;
                FindObjectOfType<Boite2nuit>().replay.onClick.AddListener(replay);
                enemyHolder = GameObject.Find("Enemies").transform;
                generator = GameObject.FindGameObjectWithTag("Grid").GetComponent<GridGenerator>();
                controller = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
                source = GetComponent<AudioSource>();
                startGame();
                StartCoroutine(spawnEnemy(enemySpawnInterval));

                break;
        }
    }


    public IEnumerator PlayAsync(int scene) {
        AsyncOperation ope = SceneManager.LoadSceneAsync(scene);
        loadingSlider.SetActive(true);
        while (!ope.isDone) {
            loadingSlider.GetComponent<Slider>().value = Mathf.Clamp01(ope.progress / .9f);
            yield return null;
        }
    }


    public void Play(int scene) {
        StartCoroutine(PlayAsync(scene));
    }

    public void QUIT() {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif

    }

    #region player

    public PlayerController controller;

#endregion



#region gameplay
    public Transform enemyHolder;
    public GridGenerator generator;
    public GameObject[] enemyPrefabs;
    public float enemySpawnInterval = 10;
    [HideInInspector]
    public float first_time = 0f;
    public Vector3 enemySpawnPos {
        get {
            return generator.enemiesSpawnPos[Random.Range(0, generator.enemiesSpawnPos.Count)];
        }
    }


    public void startGame() {
        Cursor.visible = true;
        generator.SetGame(generator.mapSize);
    }

    public IEnumerator spawnEnemy(float timer) {
        while (true) {

            while (Time.timeScale == 0) {
                yield return null;
            }

            Instantiate(enemyPrefabs[Random.Range(0,enemyPrefabs.Length)], enemySpawnPos + Vector3.up / 2, Quaternion.Euler(Vector3.up * 180), enemyHolder);
            yield return new WaitForSecondsRealtime(timer);
        }
    }

    public List<Enemy> GetEnemies() {
        List<Enemy> enemies = new List<Enemy>();
        foreach (Transform transform in enemyHolder) {
            enemies.Add(transform.gameObject.GetComponent<Enemy>());
        }
        return enemies;
    }

    public void replay() {
        StopAllCoroutines();
        SceneManager.LoadScene(1);
        Time.timeScale = 1;
    }



#endregion

    

#region music

    public AudioClip defeatsong;
    public AudioClip enemydeath;
    public AudioClip rb;
    public AudioClip enemytp;
    public AudioClip cardplaced;

    private AudioSource source;

    public void defeat() {
        source.clip = defeatsong;
        source.Play();
    }
    public void enemyDeath() {
        source.clip = enemydeath;
        source.Play();
    }
    public void placedCard() {
        source.clip = cardplaced;
        source.Play();
    }
    public void RollBackSE() {
        source.clip = rb;
        source.Play();
    }
    public void enemyTP() {
        source.clip = enemytp;
        source.Play();
    }


#endregion


#region Cloud
    // The cloud allows to make generic operations (non user related)
    private Cloud Cloud;
    // The gamer is the base to perform most operations. A gamer object is obtained after successfully signing in.
    private Gamer Gamer = null;
    // The friend, when fetched, can be used to send messages and such
    //private string FriendId;
    // When a gamer is logged in, the loop is launched for domain private. Only one is run at once.
    private DomainEventLoop Loop;

    public InputField EmailInput;
    public InputField passInput;

    public Button playAsLast;



    private bool loading = false;

    private const string LAST_GAMERID_KEY = "LAST_GAMERID";
    private const string LAST_GAMERSECRET_KEY = "LAST_GAMERSECRET";
    private const string BASE_LEADERBOARD_KEY = "AVCC";
    private string lastGamerID = null;
    private string lastGamerSECRET = null;




    private void InitCloud() {
        //loading animation 
        bHoleLoading.SetActive(false);
       
        // Link with the CotC Game Object
        var cb = FindObjectOfType<CotcGameObject>();
        if (cb == null) {
            Debug.LogError("Please put a Clan of the Cloud prefab in your scene!");
            return;
        }
        // Log unhandled exceptions (.Done block without .Catch -- not called if there is any .Then)
        Promise.UnhandledException += (object sender, ExceptionEventArgs e) => {
            Debug.LogError("Unhandled exception: " + e.Exception.ToString());
        };
        // Initiate getting the main Cloud object
        cb.GetCloud().Done(cloud => {
            Cloud = cloud;
            // Retry failed HTTP requests once
            Cloud.HttpRequestFailedHandler = (HttpRequestFailedEventArgs e) => {
                if (e.UserData == null) {
                    e.UserData = new object();
                    e.RetryIn(1000);
                } else
                    e.Abort();
            };
            Debug.Log("Setup done");

            StartCoroutine(SetupScene());

        });
    }

    //set up the menu based on last loggeg user
    private IEnumerator SetupScene() {
        Debug.Log("Setting up scene");
        bHoleLoading.SetActive(true);
        playAsLast.gameObject.SetActive(false);
        Menus.SetActive(false);
        if (CheckIFLastUserExist) {
            Debug.Log("retrieving last gamer ");
            GetLast();
            yield return new WaitUntil(() => Gamer != null);
            yield return new WaitForSecondsRealtime(3); // fake loading process
            //Debug.Log(Gamer.Profile);

            playAsLast.transform.GetChild(0).GetComponent<Text>().text = "Play as " + Gamer["profile"]["displayName"];
            playAsLast.gameObject.SetActive(true);
        }
        bHoleLoading.SetActive(false);
        Menus.SetActive(true);
        yield return null;
    }


    private void GetLast() {
        Cloud.ResumeSession(
                gamerId: lastGamerID,
                gamerSecret: lastGamerSECRET)
            .Done(gamer => {
                Gamer = gamer;
                Debug.Log("Signed in succeeded (ID = " + gamer.GamerId + ")");
                Debug.Log("Login data: " + gamer);
                Debug.Log("Server time: " + gamer["servertime"]);
            }, ex => {
                // The exception should always be CotcException
                CotcException error = (CotcException)ex;
                Debug.LogError("Failed to login: " + error.ErrorCode + " (" + error.HttpStatusCode + ")");
            });
    }
  

   
    private bool CheckIFLastUserExist {
        get {
            lastGamerID = PlayerPrefs.GetString(LAST_GAMERID_KEY);
            lastGamerSECRET = PlayerPrefs.GetString(LAST_GAMERSECRET_KEY);

            if (lastGamerID != string.Empty && lastGamerSECRET != string.Empty) {
                //Debug.Log(" id " + lastGamerID + "  \n secret : " + lastGamerSECRET);
                return true;
            }
            return false;
        }
    }
    
   
        

    

    // Log in by e-mail
    public void DoLoginEmail() {
        // You may also not provide a .Catch handler and use .Done instead of .Then. In that
        // case the Promise.UnhandledException handler will be called instead of the .Done
        // block if the call fails.

        Debug.Log(" mail :" + EmailInput.text + "  \n  pass :" + passInput.text);

        StartCoroutine(LoadingAnim());
        Cloud.Login(
            network: LoginNetwork.Email.Describe(),
            networkId: EmailInput.text,
            networkSecret: passInput.text
            ).Catch(ex => {
                // The exception should always be CotcException
                CotcException error = (CotcException)ex;
                Debug.LogError("Failed to login: " + error.ErrorCode + " (" + error.HttpStatusCode + ")");
            })
            .Done(this.DidLoginInjection);
    }

    // Signs in with an anonymous account
    public void DoLogin() {
        // Call the API method which returns an Promise<Gamer> (promising a Gamer result).
        // It may fail, in which case the .Then or .Done handlers are not called, so you
        // should provide a .Catch handler.
        StartCoroutine(LoadingAnim());
        Cloud.LoginAnonymously()
            .Then(gamer => DidLoginInjection(gamer))
            .Catch(ex => {
                // The exception should always be CotcException
                CotcException error = (CotcException)ex;
                Debug.LogError("Failed to login: " + error.ErrorCode + " (" + error.HttpStatusCode + ")");
            });
    }

    private void DidLoginInjection(Gamer newGamer) {
        StartCoroutine(DidLogin(newGamer, 3f));
    }

    private IEnumerator DidLogin(Gamer newGamer , float faketime = 3) {

        if (Gamer != null) {
            Debug.LogWarning("Current gamer " + Gamer.GamerId + " has been dismissed");
            //Loop.Stop();
        }
        Gamer = newGamer;
        Loop = Gamer.StartEventLoop();
        Loop.ReceivedEvent += Loop_ReceivedEvent;
        SetLastGamer(Gamer);
        //Fake time so we can see the animation playing
        yield return new WaitForSecondsRealtime(faketime);

        loading = false;
        Debug.Log("Signed in successfully (ID = " + Gamer.GamerId + ")");

        Play(1);
    }
    private void SetLastGamer(Gamer lastgamer) {
        Debug.Log("SETTING LAST GAMER WITH ID : " + lastgamer["gamer_id"] + "  and secret : " + lastgamer["gamer_secret"]);
        PlayerPrefs.SetString(LAST_GAMERID_KEY, lastgamer["gamer_id"]);
        PlayerPrefs.SetString(LAST_GAMERSECRET_KEY, lastgamer["gamer_secret"]);
    }


    public void SendScore(float score, GameObject listOfScore) {

        Gamer.Scores.Domain("private").Post(
                (long)score,
                BASE_LEADERBOARD_KEY,
                ScoreOrder.HighToLow
            ).Done(postedScore => {
                Debug.Log("SCORE POSTED  : " + postedScore.HasBeenSaved);
                SetLeaderboard(listOfScore);
            }, ex => {
                // The exception should always be CotcException
                CotcException error = (CotcException)ex;
                Debug.LogError("Could not post score: " + error.ErrorCode + " (" + error.ErrorInformation + ")");
            });


    }

    private void insertRowsInLeaderboard(int rowI , Score score, GameObject listOfScore) {
        Transform row = listOfScore.transform.GetChild(rowI);
        row.GetChild(0).GetComponent<Text>().text = score.GamerInfo["profile"]["displayName"];
        row.GetChild(1).GetComponent<Text>().text = score.Value.ToString();
        row.gameObject.SetActive(true);
    }

    private void SetLeaderboard(GameObject listOfScore) {
        //clear last leaderboard
        foreach (Transform child in listOfScore.transform) {
            SetGameObjectInactive(child.gameObject);
        }
        //fetch infos
        Gamer.Scores.Domain("private").BestHighScores(BASE_LEADERBOARD_KEY,10,1)
        .Done(bestHighScoresRes => {
            Debug.Log("COUNT SCORE :" + bestHighScoresRes.Count);
            for (int i = 0; i < bestHighScoresRes.Count; i++) {
                Score score = bestHighScoresRes[i];
                //Debug.Log(score.Rank + ". " + score.GamerInfo["profile"]["displayName"] + ": " + score.Value);
                insertRowsInLeaderboard(i,score, listOfScore);
            }              
        }, ex => {
            // The exception should always be CotcException
            CotcException error = (CotcException)ex;
            Debug.LogError("Could not get best high scores: " + error.ErrorCode + " (" + error.ErrorInformation + ")");
        });
    }


    private void Loop_ReceivedEvent(DomainEventLoop sender, EventLoopArgs e) {
        Debug.Log("Received event of type " + e.Message.Type + ": " + e.Message.ToJson());
    }


#endregion



#region MISC
    public GameObject bHoleLoading;
    public GameObject loadingSlider;
    public GameObject Menus;
    private IEnumerator LoadingAnim() {
        loading = true;
        Menus.SetActive(false);
        bHoleLoading.SetActive(true);
        while (loading) {

            yield return null;
        }
        bHoleLoading.SetActive(false);
        Menus.SetActive(true);
    }

    public void SetGameObjectActive(GameObject state) {
        state.SetActive(true);
    }
    public void SetGameObjectInactive(GameObject state) {
        state.SetActive(false);
    }
#endregion

}
