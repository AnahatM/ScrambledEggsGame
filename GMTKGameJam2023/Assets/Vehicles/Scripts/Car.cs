using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;
using System.Linq;

public abstract class Car : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject scorePopUp;
    [SerializeField] private string comboSymbol = "x";
    [Tooltip("Synbol used before combo count, e.g. {x}4")]
    [SerializeField] private TextMeshProUGUI comboText;

    public enum CarType
    {
        Light,
        Heavy
    }

    [Header("Car Info")]
    [SerializeField] public CarType carType;  // Enum for car type
    [SerializeField] public float carHealth = 100;     // Float for health
    [SerializeField] public int carPrice = 2;
    [SerializeField] public bool ignoreTokens = false;
    [SerializeField] private bool isSlicingCar = false;
    [SerializeField] private bool canIBeBombed = true;
    [SerializeField] public bool canSpinOut = false;
    [SerializeField] public bool isSpinning = false;
    [SerializeField] private bool carInHitStop = false;
    private float degreesPerSecond = 540f;
    public bool carInAction = true; //is car in play (not being launched)?
    public bool carTeleporting = false;


    [Header("Buy Info")]
    [SerializeField] public int carShopPrice;

    // [Header("Ultimate Info")]
    // [SerializeField] public bool isUltimate = false;
    // [SerializeField] public float ultimateResetTime;

    [Header("Tags")]
    [SerializeField] private string deathboxTag = "Death Box";

    [Header("Placement Criteria")]
    [SerializeField] public List<string> placeableLaneTags;
    [SerializeField] public bool placeableAnywhere = false;

    [Header("Speed")]
    [SerializeField] public float carSpeed = 5f;
    private float currentSpeed;

    [Header("Damage")]
    [SerializeField] private int damage = 120;
    [SerializeField] private float comboMultiplier = 0.2f;
    private float defaultComboMultiplier = 1f;
    [SerializeField] private GameObject DeathExplosion;

    [Header("Particles")]
    [SerializeField] private ParticleSystem tokenCollectParticles;
    [SerializeField] private ParticleSystem cashCollectParticles;
    [SerializeField] private float particleDestroyDelay = 2f;
    [SerializeField] private GameObject exhaustParticleParent;

    //[Header("References to Children")]
    public GameObject carSpriteObject;

    [Header("PopUp Values")]
    [SerializeField] private string scorePopUpMsg = "";
    [Tooltip("Text after getting points, e.g. 100 {Poitns}")]
    [SerializeField] private string tokenPopUpMsg = "Token";
    [Tooltip("Text after getting tokens, e.g. 1 {Token}")]
    [SerializeField] private string cashPopUpMsg = "Cash";
    [Tooltip("Text after getting tokens, e.g. 1 {Cash}")]
    [SerializeField] private float popupDestroyDelay = 0.7f;
    [SerializeField] private float carHitStopEffectMultiplier = 1.0f;

    [Header("Camera Shake Values")]
    [SerializeField] private float camShakeDuration = 0.15f;
    [SerializeField] private float camShakeMagnitude = 0.25f;

    [Header("Sound")]
    [SerializeField] private SoundConfig[] spawnSound;

    public int carKillCount = 0;
    public int totalPoints = 0;

    protected GameManager gameManager;
    protected TutorialManager tutorialManager;
    private Rigidbody2D rb;

    private Coroutine freezeCoroutine;

    [HideInInspector] public CameraShaker cameraShaker;
    [HideInInspector] public SoundManager soundManager;

    private void Awake()
    {
        cameraShaker = FindObjectOfType<CameraShaker>();
        gameManager = FindObjectOfType<GameManager>();
        tutorialManager = FindObjectOfType<TutorialManager>();
        soundManager = FindObjectOfType<SoundManager>();
        rb = GetComponent<Rigidbody2D>();
    }

    public virtual void Start()
    {
        carKillCount = 0;

        carSpriteObject = GetComponentInChildren<SpriteRenderer>().gameObject;

        soundManager?.RandomPlaySound(spawnSound);

        // Shake Camera
        if (cameraShaker.isActiveAndEnabled)
            CameraShaker.instance.Shake(camShakeDuration, camShakeMagnitude);
    }

    private void Update()
    {
        if (isSpinning)
        {
            if (carType == CarType.Light)
            {
                carSpriteObject.transform.Rotate(new Vector3(0, 0, degreesPerSecond) * Time.deltaTime);
            }
            else
            {
                carSpriteObject.transform.Rotate(new Vector3(0, 0, degreesPerSecond / 2) * Time.deltaTime);
            }

        }
        UpdateComboText();
    }

    public void UpdateComboText()
    {
        if (comboText != null)
        {
            comboText.text = $"{comboSymbol}{carKillCount}";
        }
    }

    public virtual void SetCarSpeed(float speed)
    {
        if (rb != null)
            rb.velocity = transform.up * speed;
        currentSpeed = speed;
    }

    private void SlowCarSpeed()
    {
        // Debug.Log("Car is Slowed");
        float slowSpeed = carSpeed / 2;
        if (rb != null)
            rb.velocity = transform.up * slowSpeed;
        currentSpeed = slowSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Debug.Log("Triggering Hit with: "+ collision.gameObject.name);

        if (gameManager != null && gameManager.isGameOver)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (tutorialManager != null && tutorialManager.isGameOver)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // Check if Hit Chicken
        ChickenHealth chickenHealth = collision.gameObject.GetComponent<ChickenHealth>();
        if (chickenHealth == null && collision.transform.parent != null)
            chickenHealth = collision.transform.parent.GetComponent<ChickenHealth>();

        // Check if Hit Token
        TokenController token = collision.gameObject.GetComponent<TokenController>();

        // Check if Hit Wall
        WallController wall = collision.gameObject.GetComponent<WallController>();

        if (chickenHealth != null)
            HandleChickenCollision(chickenHealth);

        if (token != null & !ignoreTokens)
            HandleTokenCollision(token);

        if (collision.gameObject.name.Contains("SlowSubstance") && !(gameObject.name.Contains("Bulldozer")))
            HandleSlowSubstanceCollision(collision.gameObject);

        if (wall != null)
            HandleWallCollision(wall);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.name.Contains("SlowSubstance"))
            SetCarSpeed(carSpeed);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // Check if Hit Car
        Car otherCar = other.gameObject.GetComponent<Car>();

        if (otherCar != null)
            HandleVehicleCollision(otherCar);

        // Check if Hit Sheep
        Sheep sheep = other.gameObject.GetComponent<Sheep>();

        if (sheep != null)
            HandleSheepCollision(sheep);

    }

    private void HandleVehicleCollision(Car otherCar)
    {
        // Perform the check on only one of the cars (the one with a lower y-coordinate)
        if (this.transform.position.y < otherCar.transform.position.y)
        {

            CameraShaker.instance.Shake(camShakeDuration, camShakeMagnitude);
            StartCoroutine(CarHitStop(0.1f));
            otherCar.StartCoroutine(CarHitStop(0.1f));

            // If Heavy > Light, Destroy Light
            if (this.carType == CarType.Heavy && otherCar.carType == CarType.Light)
            {
                otherCar.LaunchCar();
                this.carHealth -= otherCar.carHealth;
            }
            else if (otherCar.carType == CarType.Heavy && this.carType == CarType.Light)
            {
                LaunchCar();
                otherCar.carHealth -= this.carHealth;
            }
            // Else if car 1 health > car 2 health, destroy car 2 and subtract car 2 health from car 1
            else if (this.carHealth > otherCar.carHealth)
            {
                otherCar.LaunchCar();
                this.carHealth -= otherCar.carHealth;

            }
            else if (otherCar.carHealth > this.carHealth)
            {
                LaunchCar();
                otherCar.carHealth -= this.carHealth;
            }
            // TODO - Remove Sheep Later
            else if (this.carHealth == otherCar.carHealth && !(this.GetType() == typeof(Sheep)))
            {
                otherCar.LaunchCar();
                this.carHealth -= otherCar.carHealth;
            }


        }
    }

    protected virtual void HandleWallCollision(WallController wall)
    {
        //Destroy Heavy destroys wall
        if (carType == CarType.Heavy)
        {
            carHealth -= wall.damage;
            wall.WallHit();
            if (carHealth <= 0)
                LaunchCar();

        }

        //Destroy Light
        if (carType == CarType.Light)
        {
            LaunchCar();
            wall.WallHit();
        }

        CameraShaker.instance.Shake(camShakeDuration, camShakeMagnitude);
        StartCoroutine(CarHitStop(0.1f));
    }

    protected virtual void HandleSlowSubstanceCollision(GameObject slowSubstance)
    {
        SlowCarSpeed();
    }

    protected void HandleTokenCollision(TokenController token)
    {
        if (token.cashBag)
        {
            HandleCashTokenCollision(token);
        }
        else
        {
            HandleEnergyTokenCollision(token);
        }

    }

    public void HandleEnergyTokenCollision(TokenController token)
    {
        // Token Particles
        if (tokenCollectParticles != null)
        {
            GameObject newTokenParticles = Instantiate(
                            tokenCollectParticles.gameObject,
                            token.transform.position,
                            Quaternion.identity
                        );
            Destroy(newTokenParticles, particleDestroyDelay);
        }

        // +1 Token Popup
        ShowPopup(
            token.transform.position,
            $"{token.tokenValue} {tokenPopUpMsg}"
        );

        // Collect Tokens
        token.TokenCollected();

        if (gameManager != null)
            gameManager.UpdateTokens(token.tokenValue);
        if (tutorialManager != null)
            tutorialManager.UpdateTokens(token.tokenValue);
    }

    public void HandleCashTokenCollision(TokenController token)
    {
        // Token Particles
        if (cashCollectParticles != null)
        {
            GameObject newTokenParticles = Instantiate(
                            cashCollectParticles.gameObject,
                            token.transform.position,
                            Quaternion.identity
                        );
            Destroy(newTokenParticles, particleDestroyDelay);
        }

        // +1 Token Popup
        ShowPopup(
            token.transform.position,
            $"{token.cashValue} {cashPopUpMsg}"
        );

        // Collect Tokens
        token.TokenCollected();

        PlayerValues.playerCash += 5;
    }

    public virtual void HandleChickenCollision(ChickenHealth chickenHealth)
    {
        // Check if Chicken Will Die
        if (chickenHealth.health - damage <= 0)
        {
            KillChicken(chickenHealth);
        }

        //PsychicHen hit
        if (chickenHealth.gameObject.name.Contains("PsychicHen"))
        {
            carTeleporting = true;
            chickenHealth.gameObject.GetComponent<PsychicHen>().SpawnPortal(this.gameObject);
        }

        if (chickenHealth.gameObject.TryGetComponent<IceChicken>(out IceChicken iceChick))
        {
            freezeCoroutine = StartCoroutine(FreezeCar(iceChick.freezeLength, iceChick.freezeColour));
        }

        // Hit Stop, TurboChicken has different movement script;
        // if(chickenHealth.gameObject.name.Contains("Turbo")){
        if (chickenHealth.gameObject.GetComponent<AlternativeChickenMovement>())
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(CarHitStop(chickenHealth.gameObject.GetComponent<AlternativeChickenMovement>().GetChickenHitstop()));
        }
        else
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(CarHitStop(chickenHealth.gameObject.GetComponent<ChickenMovement>().GetChickenHitstop()));
        }

        // Damage Poultry
        chickenHealth.TakeDamage(damage);

        // Destroy Self if Bomb Chicken
        BombChickenHealth bombChickenHealth = chickenHealth as BombChickenHealth;
        if (bombChickenHealth != null && canIBeBombed)
        {
            // Destroy the car as well
            // Destroy(gameObject);
            LaunchCar();
        }

        // Canera Shake
        CameraShaker.instance.Shake(camShakeDuration, camShakeMagnitude);

        // Impact Sound
        soundManager.PlayChickenHit();

        // Slice Sound
        if (isSlicingCar)
            soundManager.PlayRandomSlice();
    }

    protected virtual void HandleSheepCollision(Sheep sheep)
    {
        sheep.HandleDeath();
    }

    public void KillChicken(ChickenHealth chickenHealth)
    {
        // Increase Kill Count
        if (gameManager != null)
            gameManager.killCount++;
        if (tutorialManager != null)
            tutorialManager.killCount++;

        // Increase Car-Specific Kill Count
        carKillCount++;

        // Increase Score
        totalPoints += chickenHealth.pointsReward * carKillCount;
        // gameManager.AddPlayerScore(chickenHealth.pointsReward * carKillCount);

        // Change Combo Multiplier
        // float currentComboMultiplier = defaultComboMultiplier + (comboMultiplier * (carKillCount - 1)); **THE BIGGEST CODE FUCK UP IVE EVER SEEN HOW DID WE NOT SEE THIS???!?!?!?!?!**

        //// +100 Points Pop-Up
        //ShowPopup(
        //    chickenHealth.transform.position,
        //    $"{chickenHealth.pointsReward * currentComboMultiplier} {scorePopUpMsg}"
        //);

        // +100 Points Pop-Up
        ShowPopup(
            chickenHealth.transform.position,
            $"{chickenHealth.pointsReward * carKillCount} {scorePopUpMsg}"
        );
    }

    private void ShowPopup(Vector3 position, string msg)
    {
        // Point Indicator
        ScorePopup newPopUp = Instantiate(
            scorePopUp,
            position,
            Quaternion.identity
        ).GetComponent<ScorePopup>();
        newPopUp.SetText(msg);
        Destroy(newPopUp.gameObject, popupDestroyDelay);
    }


    public IEnumerator FreezeCar(float freezeLength, Color freezeColour)
    {
        if (rb != null && carInAction == true)
        {

            float freezeTimer = 0.75f;

            //Do everything that needs to be done to make the car "frozen"

            //If rb.velocity is 0, set current velocity to starting velocity
            Vector3 currentVelocity = rb.velocity;

            if (currentVelocity.magnitude == 0)
            {
                currentVelocity = new Vector3(0, carSpeed, 0);
                yield break;
            }

            SetCarSpeed(0);

            //Give car blue tint

            carSpriteObject.GetComponent<SpriteRenderer>().color = freezeColour;


            yield return new WaitForSecondsRealtime(freezeLength - (freezeLength - freezeTimer));


            Vector2 currentSpritePos = new Vector2(carSpriteObject.transform.position.x, carSpriteObject.transform.position.y);

            while (freezeTimer > 0)
            {

                carSpriteObject.transform.position = currentSpritePos;

                Vector2 freezeDirection = new Vector2(Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f));

                freezeDirection = (freezeDirection.normalized / 10);

                carSpriteObject.transform.position = currentSpritePos + freezeDirection;

                freezeTimer -= Time.deltaTime;

                yield return new WaitForSecondsRealtime(Time.deltaTime);

            }

            carSpriteObject.transform.position = currentSpritePos;

            carSpriteObject.GetComponent<SpriteRenderer>().color = Color.white;

            if (carInAction == false)
            {
                yield break;
            }

            SetCarSpeed(currentVelocity.y);


        }
    }


    public void SpinOutCar(bool launching)
    {
        if (canSpinOut == true && isSpinning == false)
        {
            if (!launching)
            {
                BoxCollider2D carCollider = GetComponent<BoxCollider2D>();
                carCollider.size = new Vector2(1.8f, carCollider.size.y);
            }

            isSpinning = true;
        }
    }

    private IEnumerator CarHitStop(float hitStopLength)
    {
        if (rb != null && carInAction == true && carInHitStop == false)
        {
            carInHitStop = true;

            // Store the original speed before entering the hit stop
            float originalSpeed = rb.velocity.y;

            rb.velocity = Vector3.zero;

            yield return new WaitForSecondsRealtime(hitStopLength * carHitStopEffectMultiplier);

            if (carInAction == false)
            {
                yield break;
            }

            // Restore the original speed after the hit stop
            rb.velocity = transform.up * currentSpeed;

            carInHitStop = false;
        }
    }

    public void LaunchCar()
    {
        carInAction = false;

        // Generate random x and y components between -10 and 10
        float randomX = UnityEngine.Random.Range(-1f, 1f);
        float randomY = UnityEngine.Random.Range(-1f, 1f);

        // Round to -0.5 or 0.5 if the number is between -0.5 and 0.5
        if (randomX > -0.5f && randomX < 0.5f)
        {
            randomX = (randomX > 0) ? 0.5f : -0.5f;
        }

        if (randomY > -0.5f && randomY < 0.5f)
        {
            randomY = (randomY > 0) ? 0.5f : -0.5f;
        }

        // Create a Vector2 with the random components
        Vector2 randomVector = new Vector2(randomX, randomY);

        // Normalize the Vector2
        Vector2 normalizedVector = randomVector.normalized;

        Collider2D collider = gameObject.GetComponent<Collider2D>();

        if (collider != null)
        {
            GetComponent<Collider2D>().enabled = false;
        }
        else
        {
            GetComponentInChildren<Collider2D>().enabled = false;
        }

        if (comboText != null)
            comboText.enabled = false;

        rb.velocity = normalizedVector * 20;

        if (carType == CarType.Heavy || carType == CarType.Light)
        {
            if (DeathExplosion == null) return;

            GameObject currentBomb = Instantiate(DeathExplosion, transform.position, Quaternion.identity);

            Destroy(currentBomb, 0.85f);
        }

        if (freezeCoroutine != null)
        {
            StopCoroutine(freezeCoroutine);
        }

        float carLaunchScale;

        if (carType == CarType.Light)
        {
            carLaunchScale = 12;
        }
        else
        {
            carLaunchScale = 6;
        }

        // Check if this car has a special launch
        if (this is ISpecialLaunch specialLaunchCar)
        {
            specialLaunchCar.PerformSpecialLaunch();
        }

        StartCoroutine(LaunchCarCoroutine(new Vector3(carLaunchScale, carLaunchScale, 1), new Vector3(normalizedVector.x, normalizedVector.y, gameObject.transform.position.z)));

    }

    private IEnumerator LaunchCarCoroutine(Vector3 targetScale, Vector3 targetPos)
    {
        Vector3 initialScale = gameObject.transform.localScale;
        Vector3 initialPos = gameObject.transform.position;
        float t = 0;

        float launchSpeed = 0.5f;

        canSpinOut = true;
        SpinOutCar(true);

        while (t < 1.5)
        {
            t += Time.deltaTime * launchSpeed;

            // Linearly interpolate vehicle scale
            gameObject.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);

            yield return null;
        }
    }

    public virtual void CarGoesOffscreen()
    {   
        carInAction = false;
        DestroySelf();
    }

    public void DestroySelf()
    {
        carSpriteObject.SetActive(false);
        if (totalPoints > 0)
        {
            if (gameManager != null) gameManager.AddPlayerScore(totalPoints);
            if (gameManager != null) gameManager.SetHighestCombo(carKillCount);
            if (tutorialManager != null) tutorialManager.AddPlayerScore(totalPoints);
        }

        if (exhaustParticleParent != null)
        {
            bool parentDestroyed = false;

            exhaustParticleParent.transform.SetParent(null);

            if (exhaustParticleParent.transform.childCount == 0)
            {
                var main = exhaustParticleParent.GetComponent<ParticleSystem>().main;
                main.stopAction = ParticleSystemStopAction.None;
                main.loop = false;
                Destroy(exhaustParticleParent.gameObject, main.duration + main.startLifetime.constantMax);
                return;
            }

            for (int i = 0; i < exhaustParticleParent.transform.childCount; i++)
            {
                var main = exhaustParticleParent.transform.GetChild(i).GetComponent<ParticleSystem>().main;
                main.stopAction = ParticleSystemStopAction.None;
                main.loop = false;
                if (!parentDestroyed)
                {
                    Destroy(exhaustParticleParent.gameObject, main.duration + main.startLifetime.constantMax);
                    parentDestroyed = true;
                }

            }

        }

        Destroy(gameObject);
    }
}
