using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerManager : MonoBehaviour
{
    [Header("Основные ссылки и переменные")]
    public Transform player;
    [SerializeField] private TextMeshPro CounterText;
    [SerializeField] private GameObject CounterLabel;
    [SerializeField] private GameObject stickMan;
    [Range(0f, 1f)][SerializeField] private float DistanceFactor = 0.2f;
    [Range(0f, 1f)][SerializeField] private float Radius = 0.5f;

    [Header("Скорости движения")]
    public float playerSpeed = 5f;
    public float idelSpeed = 2f;
    public float fightSpeed = 1f;
    [HideInInspector] public float roadSpeed;

    [Header("Ссылки на Road, Enemy и т.д.")]
    [SerializeField] private Transform road;
    [SerializeField] private Transform enemy;

    private int numberOfStickmans;
    private int numberOfEnemyStickmans;

    private Camera mainCamera;
    private Vector3 mouseStartPos, playerStartPos;
    private bool moveByTouch;
    private bool attack;
    public bool gameState;

    [HideInInspector] public Vector3 cachedEndPosition;
    public bool isEndPositionDirty = false;

    private bool isGameOver = false;

    public static PlayerManager PlayerManagerInstance { get; private set; }

    [Header("Скорость ускорения и проверка на одноразовость")]
    [SerializeField] private float speedMultiplier = 2f;
    private bool speedBust = true;

    private HashSet<stickManManager> jumpers = new HashSet<stickManManager>();

    [Header("Границы от человечков")]
    [SerializeField] private float distance50 = 0f;
    [SerializeField] private float distance300 = 0f;

    [SerializeField] private float centerMoveDuration = 1f; // Время, за которое игрок сдвинется к X=0
    [SerializeField] private float slowDownRate = 1f;       // Коэффициент экспоненциального замедления

    // Для плавного смещения к X=0 после TriggerEndGame
    private bool isMovingToCenterEndGame = false;
    private float endGameCenterTimer = 0f;

    private float centerMoveTimer = 0f;
    private bool isMovingToCenter = false;
    private bool trigerEndGame = false;

    [Header("Тест")]
    public int testMankeStikmants = 0;

    private void Awake()
    {
        if (PlayerManagerInstance == null)
            PlayerManagerInstance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        player = transform;
        numberOfStickmans = player.childCount - 1;
        CounterText.text = numberOfStickmans.ToString();
        mainCamera = Camera.main;
        gameState = true;

        // Добавляем стартовых юнитов с учётом прокачки из Progress
        MakeStickMan(testMankeStikmants + Progress.Instance.PlayerInfo.StartUnitsLevel);
    }

    private void Update()
    {
        Attack();

        if (gameState)
        {
            road.Translate(-road.forward * Time.deltaTime * roadSpeed);
        }

        if (numberOfStickmans <= 5)
        {
            GameOver();
        }

        // Плавное смещение к центру в конце уровня
        if (isMovingToCenterEndGame)
        {
            endGameCenterTimer += Time.deltaTime;
            float t = Mathf.Clamp01(endGameCenterTimer / centerMoveDuration);

            float newX = Mathf.Lerp(transform.position.x, 0f, t);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);

            // Останавливаем движение по дороге
            playerSpeed = 0f;
            // Если нужно, можно проверять, когда t >= 1f, чтобы остановить окончательно
        }
    }

    private void LateUpdate()
    {
        if (!gameState) return;

        // Определяем текущие границы по X в зависимости от количества stickman
        float minX = numberOfStickmans < 250 ? -distance50 : -distance300;
        float maxX = numberOfStickmans < 250 ? distance50 : distance300;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        transform.position = pos;
    }

    private void Attack()
    {
        if (attack)
        {
            // Плавное замедление, пока идёт атака
            roadSpeed = Mathf.Lerp(10, 1, Time.deltaTime * slowDownRate);

            // Притягиваем игрока к центру (x=0) перед боем
            if (isMovingToCenter)
            {
                centerMoveTimer += Time.deltaTime;
                float t = Mathf.Clamp01(centerMoveTimer / centerMoveDuration);
                float newX = Mathf.Lerp(transform.position.x, 0f, t);
                transform.position = new Vector3(newX, transform.position.y, transform.position.z);

                if (t >= 1f)
                    isMovingToCenter = false;
            }

            // Поворачиваем каждого stickman к врагу
            Vector3 enemyDirection = new Vector3(enemy.position.x, transform.position.y, enemy.position.z) - transform.position;
            for (int i = 1; i < transform.childCount; i++)
            {
                transform.GetChild(i).rotation = Quaternion.Slerp(
                    transform.GetChild(i).rotation,
                    Quaternion.LookRotation(enemyDirection, Vector3.up),
                    Time.deltaTime * 3f
                );
            }

            // Если у врага ещё есть Stickman
            if (enemy.GetChild(1).childCount > 1f)
            {
                // Сводим столкновение
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (i < enemy.GetChild(1).childCount)
                    {
                        Vector3 Distance = enemy.GetChild(1).GetChild(i).position - transform.GetChild(i).position;
                        if (Distance.magnitude < 1f)
                        {
                            transform.GetChild(i).position = Vector3.Lerp(
                                transform.GetChild(i).position,
                                new Vector3(
                                    enemy.GetChild(1).GetChild(i).position.x,
                                    transform.GetChild(i).position.y,
                                    enemy.GetChild(1).GetChild(i).position.z
                                ),
                                Time.deltaTime * 5f
                            );
                        }
                    }
                }
            }
            else
            {
                // Врагов больше нет
                roadSpeed = idelSpeed;
                attack = false;
                FormatStickMan();

                // Сбрасываем поворот человечков
                for (int i = 1; i < transform.childCount; i++)
                {
                    transform.GetChild(i).rotation = Quaternion.identity;
                }

                enemy.gameObject.SetActive(false);
                isEndPositionDirty = true;
            }

            // Если у нас остался только "Player" (childCount == 1)
            if (transform.childCount == 1)
            {
                enemy.transform.GetChild(1).GetComponent<EnemyManager>().StopAttacking();
                gameObject.SetActive(false);
            }
        }
        else
        {
            MoveThePlayer();
        }
    }

    private void MoveThePlayer()
    {
        if (Input.GetMouseButtonDown(0) && gameState)
        {
            moveByTouch = true;
            Plane plane = new Plane(Vector3.up, 0f);
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out float distance))
            {
                mouseStartPos = ray.GetPoint(distance + 1f);
                playerStartPos = transform.position;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            moveByTouch = false;
        }

        if (moveByTouch)
        {
            Plane plane = new Plane(Vector3.up, 0f);
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 mousePos = ray.GetPoint(distance + 1f);
                Vector3 move = mousePos - mouseStartPos;
                Vector3 control = playerStartPos + move;

                if (numberOfStickmans < 250)
                    control.x = Mathf.Clamp(control.x, -distance50, distance50);
                else if (numberOfStickmans > 250)
                    control.x = Mathf.Clamp(control.x, -distance300, distance300);

                transform.position = new Vector3(
                    Mathf.Lerp(transform.position.x, control.x, Time.deltaTime * playerSpeed),
                    transform.position.y,
                    transform.position.z
                );
            }
        }
    }

    /// <summary>
    /// Сформировать расположение stickman'ов вокруг Player.
    /// </summary>
    public void FormatStickMan()
    {
        int stickmanCount = player.childCount - 1;
        if (stickmanCount <= 0) return;

        float baseRadius = Radius;
        float angleOffset = 10f;
        int stickmansPerLayer = 8;

        int currentLayer = 0;
        int stickmansInCurrentLayer = 0;

        for (int i = 1; i < player.childCount; i++)
        {
            if (stickmansInCurrentLayer >= stickmansPerLayer * (currentLayer + 1))
            {
                currentLayer++;
                stickmansPerLayer += 4; // Увеличиваем число stickman в каждом новом слое
            }

            float layerRadius = baseRadius + currentLayer * DistanceFactor;

            float angle = (360f / stickmansPerLayer) * stickmansInCurrentLayer;
            angle += Random.Range(-angleOffset, angleOffset);

            float rad = angle * Mathf.Deg2Rad;
            float x = Mathf.Cos(rad) * layerRadius;
            float z = Mathf.Sin(rad) * layerRadius;

            player.transform.GetChild(i).DOLocalMove(new Vector3(x, 0, z), 0.5f)
                .SetEase(Ease.OutBack);

            stickmansInCurrentLayer++;
        }
    }

    /// <summary>
    /// Создаёт нужное количество Stickman в группе.
    /// </summary>
    public void MakeStickMan(int number)
    {
        for (int i = numberOfStickmans; i < number; i++)
        {
            Instantiate(stickMan, transform.position, quaternion.identity, transform);
        }

        numberOfStickmans = transform.childCount - 1;
        CounterText.text = numberOfStickmans.ToString();
        FormatStickMan();
        isEndPositionDirty = true;
    }

    /// <summary>
    /// Возвращает самую «заднюю» (по Z) позицию в группе.
    /// </summary>
    /// <returns>Vector3, где z — минимален у всей группы.</returns>
    public Vector3 GetEndOfCrowdPosition()
    {
        if (isEndPositionDirty)
        {
            if (transform.childCount <= 1)
            {
                cachedEndPosition = transform.position;
            }
            else
            {
                cachedEndPosition = transform.GetChild(1).position;

                for (int i = 1; i < transform.childCount; i++)
                {
                    if (transform.GetChild(i).position.z < cachedEndPosition.z)
                    {
                        cachedEndPosition.z = transform.GetChild(i).position.z;
                    }
                }
            }
            isEndPositionDirty = false;
        }
        return cachedEndPosition;
    }

    /// <summary>
    /// Удалить указанное количество Stickman из группы.
    /// </summary>
    public void RemoveStickMen(int count)
    {
        int actualCount = Mathf.Min(count, transform.childCount - 1);

        List<Transform> childrenToRemove = new List<Transform>(actualCount);
        for (int i = 0; i < actualCount; i++)
        {
            if (i + 1 < transform.childCount)
            {
                childrenToRemove.Add(transform.GetChild(i + 1));
            }
        }

        foreach (Transform child in childrenToRemove)
        {
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }

        // С небольшой задержкой обновим счётчик, когда объекты действительно будут удалены
        StartCoroutine(UpdateStickManCountDelayed());
    }

    private IEnumerator UpdateStickManCountDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        UpdateStickManCount();
    }

    /// <summary>
    /// Пересчитать кол-во Stickman и обновить текстовый счётчик.
    /// </summary>
    public void UpdateStickManCount()
    {
        numberOfStickmans = transform.childCount - 1;
        CounterText.text = numberOfStickmans.ToString();
    }

    public void OnStickmanLanded(stickManManager stickman)
    {
        jumpers.Remove(stickman);

        if (jumpers.Count == 0)
        {
            FormatStickMan();
        }
    }

    public void OnStickmanHitJump(stickManManager stickman)
    {
        jumpers.Add(stickman);
        stickman.DoJump();
    }

    private void GameOver()
    {
        if (isGameOver) return;

        UpdateStickManCount();

        if (numberOfStickmans <= 0)
        {
            isGameOver = true;
            roadSpeed = 0;
            CounterLabel.SetActive(false);

            if (trigerEndGame)
            {
                WinGame();
            }
            else
            {
                GameManager.Instance.GameOver();
            }
        }
    }

    public int GetStickmanCount()
    {
        return transform.childCount - 1;
    }

    private float lastGateBonusTime = -1f;
    private float gateBonusCooldown = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Gate"))
        {
            // Проверяем задержку между бонусами ворот
            if (Time.time - lastGateBonusTime < gateBonusCooldown) return;
            lastGateBonusTime = Time.time;

            GateManager gateManager = other.GetComponent<GateManager>();
            int currentStickmanCount = numberOfStickmans;

            // Логика расчёта
            int newCount = gateManager.multiply
                ? currentStickmanCount * gateManager.randomNumber
                : currentStickmanCount + gateManager.randomNumber;

            int toAdd = newCount;
            if (gateManager.isFirstGate)
            {
                MakeStickMan(toAdd);
            }
            else
            {
                MakeStickMan(toAdd + 1);
            }

            SoundManager.Instance.PlaySound("passageGate");
        }

        if (other.CompareTag("Enemy"))
        {
            enemy = other.transform;
            attack = true;
            isMovingToCenter = true;
            centerMoveTimer = 0f;
            other.transform.GetChild(1).GetComponent<EnemyManager>().AttackThem(transform);
        }

        if (other.CompareTag("TrigerEndGame"))
        {
            if (speedBust)
            {
                roadSpeed *= speedMultiplier;
                speedBust = false;
                isMovingToCenterEndGame = true;
                endGameCenterTimer = 0f;
                trigerEndGame = true;
            }
        }

        if (other.CompareTag("TrigerOpenChest"))
        {
            roadSpeed = 0;
            CounterLabel.SetActive(false);
            WinGame();
        }

        if (other.CompareTag("Jump"))
        {
            SoundManager.Instance.PlaySound("Jump");
        }
    }

    public void BuyStickMan(int number)
    {
        MakeStickMan(GetStickmanCount() + number);
    }

    public void StartGame()
    {
        roadSpeed = idelSpeed;
        playerSpeed = idelSpeed;
    }

    public void WinGame()
    {
        GameManager.Instance.WinGame();

        int winCoin = 100 + (int)(100 * ((float)numberOfStickmans / 100));
        GameManager.Instance.CoinManager(winCoin);

        SoundManager.Instance.PlaySound("Win");
    }
}
