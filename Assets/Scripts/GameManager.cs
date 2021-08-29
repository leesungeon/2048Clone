using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    bool wait, move, stop;

    public GameObject[] n;
    public GameObject quit;
    public Text Score, BestScore, GetScore;

    int x, y, i, j, k, l;
    int score;

    Vector3 firstPos, calPos;
    GameObject[,] Square = new GameObject[4, 4];
    void Start()
    {
        Spawn();
        Spawn();
        BestScore.text = PlayerPrefs.GetInt("BestScore").ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        if (stop) return;

        if (Input.GetMouseButtonDown(0) || (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            wait = true;
            firstPos = Input.GetMouseButtonDown(0) ? Input.mousePosition : (Vector3)Input.GetTouch(0).position;
        }

        if (Input.GetMouseButton(0) || (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved))
        {
            calPos = (Input.GetMouseButton(0) ? Input.mousePosition : (Vector3)Input.GetTouch(0).position) - firstPos;

            if (calPos.magnitude < 100) return;
            calPos.Normalize();

            if (wait)
            {
                wait = false;

                // 위
                if (calPos.y > 0 && calPos.x > -0.5f && calPos.x < 0.5f) 
                { 
                    for (x = 0; x <= 3; x++) for (y = 0; y <= 2; y++) for (i = 3; i >= y + 1; i--)
                                Judgement(x, i - 1, x, i); 
                }
                // 아래
                else if (calPos.y < 0 && calPos.x > -0.5f && calPos.x < 0.5f) 
                { 
                    for (x = 0; x <= 3; x++) for (y = 3; y >= 1; y--) for (i = 0; i <= y - 1; i++)
                                Judgement(x, i + 1, x, i); 
                }
                // 오른쪽
                else if (calPos.x > 0 && calPos.y > -0.5f && calPos.y < 0.5f) 
                { 
                    for (y = 0; y <= 3; y++) for (x = 0; x <= 2; x++) for (i = 3; i >= x + 1; i--) 
                                Judgement(i - 1, y, i, y); 
                }
                // 왼쪽
                else if (calPos.x < 0 && calPos.y > -0.5f && calPos.y < 0.5f) 
                { 
                    for (y = 0; y <= 3; y++) for (x = 3; x >= 1; x--) for (i = 0; i <= x - 1; i++) 
                                Judgement(i + 1, y, i, y); 
                }
                else return;

                
                if (move)
                {
                    move = false;
                    Spawn();
                    k = l = 0;

                    if (score > 0)
                    {
                        GetScore.text = "+" + score.ToString() + "      ";
                        GetScore.GetComponent<Animator>().SetTrigger("GetScoreB");
                        GetScore.GetComponent<Animator>().SetTrigger("GetScore");
                        Score.text = (int.Parse(Score.text) + score).ToString();
                        
                        if (PlayerPrefs.GetInt("BestScore", 0) < int.Parse(Score.text))
                        {
                            PlayerPrefs.SetInt("BestScore", int.Parse(Score.text));
                        }
                        
                        BestScore.text = PlayerPrefs.GetInt("BestScore").ToString();
                        score = 0;
                    }

                    for (x = 0; x <= 3; x++)
                    {
                        for (y = 0; y <= 3; y++)
                        {
                            if (Square[x, y] == null) { k++; continue; }
                            if (Square[x, y].tag == "Combine") Square[x, y].tag = "Untagged";
                        }
                    }
                    if(k == 0)
                    {
                        for (y = 0; y <= 3; y++) for (x = 0; x <= 2; x++) if (Square[x, y].name == Square[x + 1, y].name) l++;
                        for (x = 0; x <= 3; x++) for (y = 0; y <= 2; y++) if (Square[x, y].name == Square[x, y + 1].name) l++;
                        if (l == 0) { stop = true; quit.SetActive(true); return; }
                    }

                }
            }
        }
    }

        void Spawn()
        {
            while (true) { x = Random.Range(0, 4); y = Random.Range(0, 4); if (Square[x, y] == null) break; }
            Square[x, y] = Instantiate(Random.Range(0, int.Parse(Score.text) > 800 ? 4 : 8) > 0 ? n[0] : n[1], new Vector3(1.2f * x - 1.8f, 1.2f * y - 1.8f, 0), Quaternion.identity);
            Square[x, y].GetComponent<Animator>().SetTrigger("Spawn");
        }

        void Judgement(int x1, int y1, int x2, int y2)
        {
            if (Square[x2, y2] == null && Square[x1, y1] != null)
            {
                move = true;
                Square[x1, y1].GetComponent<Moving>().Move(x2, y2, false);
                Square[x2, y2] = Square[x1, y1];
                Square[x1, y1] = null;
            }

            if (Square[x1, y1] != null && Square[x2, y2] != null && Square[x1, y1].name == Square[x2, y2].name && Square[x1, y1].tag != "Combine" && Square[x2, y2].tag != "Combine")
            {
                move = true;
                
                for (j = 0; j <= 16; j++) if (Square[x2, y2].name == n[j].name + "(Clone)") break;
                    
                Square[x1, y1].GetComponent<Moving>().Move(x2, y2, true);
                Destroy(Square[x2, y2]);
                Square[x1, y1] = null;
                Square[x2, y2] = Instantiate(n[j + 1], new Vector3(1.2f * x2 - 1.8f, 1.2f * y2 - 1.8f, 0), Quaternion.identity);
                Square[x2, y2].tag = "Combine";
                Square[x2, y2].GetComponent<Animator>().SetTrigger("Combine");
                score += (int)Mathf.Pow(2, j + 2);
                
            }
        }
        public void Restart() { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
    }
