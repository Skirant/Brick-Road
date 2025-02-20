using DG.Tweening;
using UnityEngine;

public class stickManManager : MonoBehaviour
{
    #region FIELDS

    [Header("FX (Effects)")]
    [SerializeField]
    //private ParticleSystem blood;

    // ����, ��������������� � ���, �������� �� ������� ������
    private bool hasJumped;

    // ����������� ���������� ��� ����������� ������� ��������������� �����
    private static float killSoundTimer;
    private static int killSoundCounter;

    #endregion

    #region UNITY_METHODS

    private void Start()
    {
        // ��������� TweensCapacity, ����� �������� ������������
        DOTween.SetTweensCapacity(4000, 150);
    }

    private void OnTriggerEnter(Collider other)
    {
        // ��������� ������ �� ����
        if (other.CompareTag("Red") && other.transform.parent.childCount > 0)
        {
            // ���������� �������
            Destroy(other.gameObject);
            Destroy(gameObject);

            // �������� ����� ���������� ���������� ���������
            PlayerManager.PlayerManagerInstance.UpdateStickManCount();

            // ���������� EnemyManager, ��� ���������� �������� �����
            EnemyManager component = other.transform.parent.GetComponent<EnemyManager>();
            if (component != null)
            {
                component.UpdateCounterText();
            }

            // ������������ ��������������� ����� "Kill" ����� ������ � �������
            PlayKillSoundLimited();
        }

        if (other.CompareTag("Spike"))
        {
            // ������������� ��������� ����� ��������
            transform.DOKill();
            Destroy(gameObject);

            // ���������� � ������ ��������
            PlayerManager.PlayerManagerInstance.UpdateStickManCount();
            PlayKillSoundLimited();
        }

        if (other.CompareTag("Barricade"))
        {
            // ������������� �����, ������� �� �����������
            transform.DOKill();
            // ����� ����� ��������� ������ �����, ���� �����������
        }

        // ��������� �� ��������� �������� ��� ������
        if (other.CompareTag("Jump") && !hasJumped)
        {
            hasJumped = true;
            PlayerManager.PlayerManagerInstance.OnStickmanHitJump(this);
        }
    }

    #endregion

    #region PUBLIC_METHODS

    /// <summary>
    /// ��������� ������ � ������� DoLocalJump � ���������� PlayerManager � �����������.
    /// </summary>
    public void DoJump()
    {
        // ����������� ������ ����� DoLocalJump
        transform.DOLocalJump(
            transform.localPosition,
            3f,      // ������ ������
            1,       // ���������� ��������� ������
            1.5f     // ����� ������
        )
        .SetEase(Ease.Flash)
        .OnComplete(() =>
        {
            // �� ���������� ������ ���������� PlayerManager
            if (PlayerManager.PlayerManagerInstance != null)
            {
                PlayerManager.PlayerManagerInstance.OnStickmanLanded(this);
            }
        });
    }

    #endregion

    #region PRIVATE_METHODS

    /// <summary>
    /// ������� �����, �������������� ��������������� ����� "Kill" ����� �������� � �������.
    /// </summary>
    private void PlayKillSoundLimited()
    {
        // ���� ������ ������ ������� � ������� ������ ������
        if (Time.time - killSoundTimer > 0.2f)
        {
            // ���������� ������� � ��������� ������
            killSoundTimer = Time.time;
            killSoundCounter = 0;
        }

        // ���� ����� ��� �� ���������, ����������� ����
        if (killSoundCounter < 10)
        {
            killSoundCounter++;
            SoundManager.Instance.PlaySound("Kill");
        }
    }

    #endregion
}
