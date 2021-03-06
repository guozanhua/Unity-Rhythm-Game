﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class NoteObj{
	public GameObject leftNote, downNote, upNote, rightNote;
}

public class GameController : MonoBehaviour {

	// List<int> whichNote = new List<int>() {1, 2, 2, 8, 2, 4, 4, 1, 2, 1, 4, 2, 4, 4, 8, 4, 2, 4, 8, 8, 4, 2, 4, 4};
	List<int> xPosList = new List<int>() {-3, -1, 1, 3};

	public float offset;
	public float distance;
	public NoteObj notes;
	public Text scoreText;
	public Text comboText;
	public Image black;
	public Animator anim;

	private MapController mapController;
	private NoteData thisStage;
	private int waveMark;
	private int noteCnt;
	private int score;
	private int hitted;
	private int[] combo;

	private bool isInit;
	private bool isBarEnd;
	private int barCount;
	private float arrowSpeed;
	private float barExecutedTime;
	private float barTime;
	private float songTimer;
	private float timeOffset;

	void Start () {
		GameObject mapControllerObject = GameObject.FindWithTag("MapController");
		if(mapControllerObject != null)
			mapController = mapControllerObject.GetComponent<MapController>();
		if(mapController == null)
			Debug.Log("Cannot find 'MapController' script");
		thisStage = mapController.getCurrentStage();
		waveMark = 0;
		timeOffset = distance / 10.0f;
		barExecutedTime = 0;
		barTime = (60.0f / mapController.getCurrentBPM(barExecutedTime)) * 4.0f;

		score = 0;
		hitted = 0;
		combo = new int[2] {0, 0};
		noteCnt = 0;
		updateScoreAndCombo();
		isInit = false;
		isBarEnd = true;
		StartCoroutine(gameStart());
	}

	void Update() {
		if (isInit && (waveMark < thisStage.bars.Count)) {
            songTimer = mapController.getSongTimer();

            if ((songTimer + timeOffset) >= (barExecutedTime - barTime) && isBarEnd) {
            	Debug.Log("songTime: " + songTimer + ", barExecutedTime: " + barExecutedTime);
            	StartCoroutine(spawnBar ());
            	barExecutedTime += barTime;
            }
        }
        if((waveMark >= thisStage.bars.Count) && !mapController._nowPlaying.isPlaying)
        	StartCoroutine(gameEnd ());
	}

	IEnumerator spawnBar () {
		isBarEnd = false;
		List<Notes> thisWave = thisStage.bars[waveMark++];
		barTime = (60.0f / mapController.getCurrentBPM(barExecutedTime)) * 4.0f;
		for(int i = 0; i < thisWave.Count; i++) {
			yield return spawnNote(thisWave[i], thisWave.Count);
		}
		isBarEnd = true;
	}

	IEnumerator spawnNote (Notes currentBar, int count) {
		Vector3 notePos = new Vector3 (0, transform.position.y, transform.position.z - 1);
		if(currentBar.left) {
			notePos.x = xPosList[0];
			Instantiate (notes.leftNote, notePos, notes.leftNote.transform.rotation);
			noteCnt++;
		}
		if(currentBar.down) {
			notePos.x = xPosList[1];
			Instantiate (notes.downNote, notePos, notes.downNote.transform.rotation);
			noteCnt++;
		}
		if(currentBar.up) {
			notePos.x = xPosList[2];
			Instantiate (notes.upNote, notePos, notes.upNote.transform.rotation);
			noteCnt++;
		}
		if(currentBar.right) {
			notePos.x = xPosList[3];
			Instantiate (notes.rightNote, notePos, notes.rightNote.transform.rotation);
			noteCnt++;
		}
		yield return new WaitForSeconds((barTime / count) - Time.deltaTime);
	}

	public float getSongTime() {
		return songTimer;
	}

	public void comboSuccess() {
		hitted += 1;
		combo[0] += 1;
		if(combo[0] > combo[1]) combo[1] = combo[0];
		score += 10 * combo[0];
		updateScoreAndCombo();
	}

	public void comboFail(int name) {
		combo[0] = 0;
		// Debug.Log(name + "'s end time: " + songTimer);
		updateScoreAndCombo();
	}

	void updateScoreAndCombo() {
		string scoreStr = score.ToString();
		scoreText.text = "Score: \n" + scoreStr.PadLeft(8, '0');
		if(combo[0] > 1)
			comboText.text = "x" + combo[0];
		else
			comboText.text = "";
	}

	IEnumerator gameStart(){
		yield return new WaitForSeconds (offset);
		Debug.Log("Game Start");
		isInit = true;
		yield return mapController.playAudio (timeOffset);
	}

	IEnumerator gameEnd(){
		yield return new WaitForSeconds (offset);
		PlayerPrefs.SetInt("score", score);
		PlayerPrefs.SetInt("maxCombo", combo[1]);
		PlayerPrefs.SetInt("playerHitted", hitted);
		PlayerPrefs.SetInt("totalBeats", noteCnt);
		anim.SetBool("Fade", true);
		yield return new WaitUntil(() => black.color.a == 1);
		SceneManager.LoadScene("ScoreBoard", LoadSceneMode.Single);
	}
}
