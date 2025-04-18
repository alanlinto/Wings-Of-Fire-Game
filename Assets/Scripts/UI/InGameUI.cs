using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum InGameUIState {
    Normal,
    BossBattle,
    PlayerDied,
    BossDied
}

public class InGameUI : MonoBehaviour {

    public HealthBar playerHealthBarScript;
    public BossHealthBar bossHealthBarScript;
    public MinionMeter minionMeterScript;
    public EventTextFading youWinTextScript;
    public EventTextFading gameOverTextScript;
    public GameObject playAgainButton;
    public float playAgainButtonDelay;

    InGameUIState state;

    void Start() {
        state = InGameUIState.Normal;
        playAgainButton.SetActive(false);
    }

    public void OnEnterBossArena() {
        // If this already happened, or the boss/player is dead, who cares!
        if (state != InGameUIState.Normal) { return; }

        state = InGameUIState.BossBattle;

        bossHealthBarScript.Show();
    }

    public void OnLeaveBossArena() {
        // If this already happened, or the boss/player is dead, who cares!
        if (state != InGameUIState.BossBattle) { return; }

        state = InGameUIState.Normal;

        bossHealthBarScript.Hide();
    }

    public void OnPlayerDeath() {
        // If the boss died first, who cares!
        if (state == InGameUIState.BossDied) { return; }

        state = InGameUIState.PlayerDied;
        playerHealthBarScript.Hide();
        bossHealthBarScript.Hide();
        minionMeterScript.Hide();
        gameOverTextScript.Show();
        StartCoroutine(ShowPlayAgainButtonAfterDelayCoroutine());
    }

    public void OnBossDeath() {
        // If the player died first, too late!
        if (state == InGameUIState.PlayerDied) { return; }

        state = InGameUIState.BossDied;
        playerHealthBarScript.Hide();
        bossHealthBarScript.Hide();
        minionMeterScript.Hide();
        youWinTextScript.Show();
        StartCoroutine(ShowPlayAgainButtonAfterDelayCoroutine());
    }

    public void SetupMinionMeter(int value, int max) {
        minionMeterScript.Setup(value, max);
    }

    public void SetMinionMeterValue(int value, bool animate) {
        minionMeterScript.SetValue(value, animate);
    }

    public void SetBossHealthPercentage(float percentage) {
        bossHealthBarScript.SetPercentage(percentage);
    }

    public void SetPlayerHealthPercentage(float percentage) {
        playerHealthBarScript.SetPercentage(percentage);
    }

    public void OnPlayAgainButtonClicked() {
        SceneManager.LoadScene("MainScene");
    }

    IEnumerator ShowPlayAgainButtonAfterDelayCoroutine() {
        yield return new WaitForSeconds(playAgainButtonDelay);
        playAgainButton.SetActive(true);
    }
}
