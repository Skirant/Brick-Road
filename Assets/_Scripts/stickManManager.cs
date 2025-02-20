using DG.Tweening;
using UnityEngine;

public class stickManManager : MonoBehaviour
{
    #region FIELDS

    [Header("FX (Effects)")]
    [SerializeField]
    //private ParticleSystem blood;

    // Флаг, сигнализирующий о том, совершил ли стикмен прыжок
    private bool hasJumped;

    // Статические переменные для ограничения частоты воспроизведения звука
    private static float killSoundTimer;
    private static int killSoundCounter;

    #endregion

    #region UNITY_METHODS

    private void Start()
    {
        // Установка TweensCapacity, чтобы избежать переполнения
        DOTween.SetTweensCapacity(4000, 150);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем объект по тэгу
        if (other.CompareTag("Red") && other.transform.parent.childCount > 0)
        {
            // Уничтожаем объекты
            Destroy(other.gameObject);
            Destroy(gameObject);

            // Вызываем метод уменьшения количества стикменов
            PlayerManager.PlayerManagerInstance.UpdateStickManCount();

            // Уведомляем EnemyManager, что необходимо обновить текст
            EnemyManager component = other.transform.parent.GetComponent<EnemyManager>();
            if (component != null)
            {
                component.UpdateCounterText();
            }

            // Ограничиваем воспроизведение звука "Kill" пятью разами в секунду
            PlayKillSoundLimited();
        }

        if (other.CompareTag("Spike"))
        {
            // Останавливаем возможные твины движения
            transform.DOKill();
            Destroy(gameObject);

            // Уведомляем о смерти стикмена
            PlayerManager.PlayerManagerInstance.UpdateStickManCount();
            PlayKillSoundLimited();
        }

        if (other.CompareTag("Barricade"))
        {
            // Останавливаем твины, стикмен не разрушается
            transform.DOKill();
            // Здесь можно проиграть эффект крови, если понадобится
        }

        // Проверяем на спусковой механизм для прыжка
        if (other.CompareTag("Jump") && !hasJumped)
        {
            hasJumped = true;
            PlayerManager.PlayerManagerInstance.OnStickmanHitJump(this);
        }
    }

    #endregion

    #region PUBLIC_METHODS

    /// <summary>
    /// Выполняет прыжок с помощью DoLocalJump и уведомляет PlayerManager о приземлении.
    /// </summary>
    public void DoJump()
    {
        // Настраиваем прыжок через DoLocalJump
        transform.DOLocalJump(
            transform.localPosition,
            3f,      // высота прыжка
            1,       // количество прыжковых вершин
            1.5f     // время прыжка
        )
        .SetEase(Ease.Flash)
        .OnComplete(() =>
        {
            // По завершению прыжка уведомляем PlayerManager
            if (PlayerManager.PlayerManagerInstance != null)
            {
                PlayerManager.PlayerManagerInstance.OnStickmanLanded(this);
            }
        });
    }

    #endregion

    #region PRIVATE_METHODS

    /// <summary>
    /// Игровой метод, ограничивающий воспроизведение звука "Kill" пятью вызовами в секунду.
    /// </summary>
    private void PlayKillSoundLimited()
    {
        // Если прошла больше секунды с момента первой записи
        if (Time.time - killSoundTimer > 0.2f)
        {
            // Сбрасываем счётчик и обновляем таймер
            killSoundTimer = Time.time;
            killSoundCounter = 0;
        }

        // Если лимит ещё не достигнут, проигрываем звук
        if (killSoundCounter < 10)
        {
            killSoundCounter++;
            SoundManager.Instance.PlaySound("Kill");
        }
    }

    #endregion
}
