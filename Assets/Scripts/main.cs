using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System;
/// <summary>
/* 
1. 遊戲一開始玩家分數為 0， 並隨機產生一個 7*7 的盤面
2. 三個或三個以上相同顏色的糖果在十字方向相鄰會自動消除 消除後畫面上方會掉下新的糖果
3. 算分方法為(個數＊分數)
   舉例來說 如果 三個相鄰的綠色糖果在一次移動中消除 分數 = 10 * 3
4. 每一局玩家可以以滑動方式移動一顆糖果 不限局數的玩下去
5. 每當玩家沒有任何地方可以滑動繼續遊戲時，盤面會重新洗牌 直到玩家有移動可能為止
6. 洗牌的定義為：盤面目前各色糖果數量不變 僅改變在畫面上的排列
遊戲規則與玩法可以參考知名遊戲 candy crush
紅色        1
黃色        3
藍色        5
綠色        10					
*/
/// </summary>

public enum ENUM_CANDY_SCORE : int
{
    CANDY_RED = 1,
    CANDY_YELLOW = 3,
    CANDY_BLUE = 5,
    CANDY_GREEN = 10,
    MAX
}


public class main : MonoBehaviour
{
    protected static main Self;
    protected int iScore = 0;
    protected const int MAX_CANDY_COLOR = 4;//糖果顏色,4色
    protected const int CANDY_POOL_WIDTH = 7;// 7*7 的盤面
    protected const int CANDY_POOL_HEIGH = 7;// 7*7 的盤面
    protected const int MAX_CANDY = CANDY_POOL_WIDTH * CANDY_POOL_HEIGH;
    protected const int CANDY_SIZE_WIDTH = 150;//糖果SIZE
    protected const int CANDY_SIZE_HEIGH = 150;//糖果SIZE
    protected GameObject[] currentObjects;//糖果,display    
    public GameObject[] prefabCandy;//糖果,prefab
    public Sprite[] spritesCandy;//糖果,sprite
    public GameObject canvasObject;//畫布,display
    public Text textDisplayScore;//分數,display
    //data
    protected int[,] currentColorArrays;//糖果顏色,
    protected int iImageClicked = -1;//按下的糖果座標,    
    protected int[] iDestoryedCandy = new int[MAX_CANDY];//十字方向相鄰的糖果->被消除,
    protected int iDestoryedCandyCount = 0;//被消除的糖果
    //
    void awake()
    {
        //currentObjects = new GameObject[MAX_CANDY];//bug?
    }
    // Use this for initialization
    void Start ()
    {
        Self = this;
        Self.Init();
        for (int i = 0; i < MAX_CANDY; i++) iDestoryedCandy[i] = -1;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Home))
        {
            Application.Quit();
        }
    }
    //Instance
    public static main Instance()
    {
        return Self;
    }
    //洗牌
    public void Reset()
    {
        iScore = 0;
        textDisplayScore.text = "" + iScore;
        iDestoryedCandyCount = 0;
        for (int i = 0; i < MAX_CANDY; i++) iDestoryedCandy[i] = -1;

        int[] indexArray = new int [MAX_CANDY] ;
        bool[] flagArray = new bool[MAX_CANDY];
        int[,] tempColorArrays = new int[CANDY_POOL_WIDTH, CANDY_POOL_HEIGH];
        //default
        for (int iIndex = 0; iIndex < MAX_CANDY; iIndex++)
        {
            indexArray[iIndex] = iIndex;
            flagArray[iIndex] = false;
            int i = 0, j = 0;
            convertToDimensional(iIndex, ref i, ref j);
            tempColorArrays[i,j] = currentColorArrays[i, j];
        }
        //random
        for (int iIndex = 0; iIndex < MAX_CANDY; /* skip */)
        {
            int newIndex = UnityEngine.Random.Range(0, MAX_CANDY);
            if (flagArray[newIndex] == true) continue;
            //
            flagArray[newIndex] = true;
            indexArray[iIndex] = newIndex;
            int i = 0, j = 0, newi = 0,newj = 0;
            convertToDimensional(iIndex, ref i, ref j);
            convertToDimensional(newIndex, ref newi, ref newj);
            currentColorArrays[newi, newj] = tempColorArrays[i, j];
            currentObjects[newIndex].GetComponent<Image>().sprite = spritesCandy[tempColorArrays[i, j] ];
            iIndex++;
        }
    }
    public void Init()
    {
        currentObjects = new GameObject[MAX_CANDY];
        currentColorArrays = new int[CANDY_POOL_WIDTH, CANDY_POOL_HEIGH];

        iScore = 0;
        textDisplayScore.text = "" + iScore;
        int x = CANDY_SIZE_WIDTH * (-3);
        int y = CANDY_SIZE_WIDTH * (-3);
        int iCount = 0;
        for (int j = 0; j < CANDY_POOL_HEIGH; j++)
        {
            for (int i = 0; i < CANDY_POOL_WIDTH; i++) 
            {
                Vector3 pos = new Vector3(x+ (CANDY_SIZE_WIDTH*i), y+(CANDY_SIZE_HEIGH * j), 0);
                int iColor = UnityEngine.Random.Range(0, MAX_CANDY_COLOR);//0,1,2,3
                //Debug.Log("candy color="+ iColor);
                currentColorArrays[i, j] = iColor;
                currentObjects[iCount] = Instantiate(prefabCandy[iColor], pos, Quaternion.identity);
                currentObjects[iCount].transform.SetParent(canvasObject.transform, false);
                currentObjects[iCount].name = ""+iCount;
                iCount++;
            }
        }
    }
    //
    public void Click(string s)
    {
        int i = int.Parse(s);
        iImageClicked = i;
    }
    public void Check(string s)
    {
        int iDest = int.Parse(s);
        //
        bool isNear = IsNeighbor(iImageClicked, iDest);
        if (isNear)
        {
            //Debug.Log("source iImageClicked=" + iImageClicked + " swap to Dest=" + iDest);
            Swap(iImageClicked, iDest);
            int iSource = iDest;
            int iTarget = iImageClicked;
            //check candy iImageClicked , at pos iDest
            bool isCandy1 = IsSameColor(iSource, iDest);
            //check candy iDest , at pos iImageClicked 
            bool isCandy2 = IsSameColor(iTarget, iImageClicked);            
            if (isCandy1 == true || isCandy2 == true)
            {
                SpawnCandy();
                onMouseDownScript.Instance().Reset();                
                iImageClicked = -1;
            }
            else
            {
                Debug.Log("candy back..");
                Swap(iImageClicked, iDest);
                onMouseDownScript.Instance().Reset();
                iImageClicked = -1;
            }
        }
        else
        {            
            onMouseDownScript.Instance().Reset();
            iImageClicked = -1;
        }
    }
    //for compare
    public bool IsNeighbor(int iSource,int iTarget)
    {
        int i = Math.Abs(iSource - iTarget);
        if (CANDY_POOL_WIDTH == i) return true;//上or下
        //
        if (i == 1)//左or右
        {
            int j = iSource % CANDY_POOL_WIDTH;
            int k = iSource - iTarget;
            if (j == 0 && k == 1) return false;//最左
            if (j == (CANDY_POOL_WIDTH-1) && k == -1) return false;//最右
            return true;
        }        
        return false;
    }
    //check 三個或三個以上相同顏色的糖果在十字方向相鄰會自動消除
    public bool IsSameColor(int iSource,int iTargetPosition)
    {
        int i = 0, j = 0;
        convertToDimensional(iSource, ref i, ref j);        
        int iColor = currentColorArrays[i, j];
        convertToDimensional(iTargetPosition, ref i, ref j);
        //算分方法為(個數＊分數)
        int iCount = 0;
        int iBonus = 0;
        int iTemp1 = 0;
        int[] iDestoryCandy = new int[MAX_CANDY];
        switch (iColor)
        {
            case (0): iBonus = (int)ENUM_CANDY_SCORE.CANDY_RED; break;
            case (1): iBonus = (int)ENUM_CANDY_SCORE.CANDY_YELLOW; break;
            case (2): iBonus = (int)ENUM_CANDY_SCORE.CANDY_BLUE; break;
            case (3): iBonus = (int)ENUM_CANDY_SCORE.CANDY_GREEN; break;
            default:break;
        }
        //4方向         
        for (int k = 1; k <CANDY_POOL_WIDTH;k++)
        {
            if ((i - k) < 0) break;
            bool b = checkDirection(i-k, j, iColor);
            if (b)
            {
                iDestoryCandy[iTemp1] = convertToIndex(i - k, j);
                iTemp1++;
            }
            else
                break;
        }
        
        for (int k = 1; k< CANDY_POOL_WIDTH; k++)
        {
            if ((i + k) >= CANDY_POOL_WIDTH) break;
            bool b = checkDirection(i + k, j, iColor);
            if (b)
            {
                iDestoryCandy[iTemp1] = convertToIndex(i + k, j);
                iTemp1++;
            }
            else break;
        }
        
        if (iTemp1 >= 2)
            iCount += iTemp1;
        else
        {   //reset
            iTemp1 = 0;
            iDestoryCandy[0] = -1;
            iDestoryCandy[1] = -1;
        }
        //
        int iTemp2 = 0;
        for (int k = 1; k < CANDY_POOL_HEIGH; k++)
        {
            if ((j - k) < 0) break;
            bool b = checkDirection(i, j-k, iColor);
            if (b)
            {
                iDestoryCandy[iTemp1+ iTemp2] = convertToIndex(i, j-k);
                iTemp2++;
            }
            else break;
        }
        
        for (int k = 1; k < CANDY_POOL_HEIGH; k++)
        {
            if ((j + k) >= CANDY_POOL_HEIGH) break;
            bool b = checkDirection(i, j+k, iColor);
            if (b)
            {
                iDestoryCandy[iTemp1 + iTemp2] = convertToIndex(i, j + k);
                iTemp2++;
            }
            else break;
        }
        
        if (iTemp2 >= 2) iCount += iTemp2;
        //add self
        if (iCount >= 2)
        {
            iDestoryCandy[iCount] = iTargetPosition;
            iCount += 1;//source
        }
        iScore += (iCount * iBonus);
        textDisplayScore.text = "" + iScore;

        if (iCount > 0)
        {        
            for (int iAmount = 0; iAmount < iCount; iAmount++)
            {
                iDestoryedCandy[iDestoryedCandyCount + iAmount] = iDestoryCandy[iAmount];                
            }
            iDestoryedCandyCount += iCount;
            return true;
        }
        else return false;
    }
    public bool checkDirection(int x,int y,int iTarget)
    {
        if (currentColorArrays[x, y] == iTarget) return true;
        return false;
    }
    //swap clicked image 
    public void Swap(int iSource, int iTarget)
    {
        int iImageClickedColor = -1;
        int i = 0, j = 0;
        convertToDimensional(iSource, ref i, ref j);
        iImageClickedColor = currentColorArrays[i, j] ;
        int k = 0,l = 0;
        convertToDimensional(iTarget, ref k, ref l);

        if (currentColorArrays[k, l] != -1)
            currentObjects[iSource].GetComponent<Image>().sprite = spritesCandy[currentColorArrays[k, l]];
        if (currentColorArrays[i, j] != -1)
            currentObjects[iTarget].GetComponent<Image>().sprite = spritesCandy[currentColorArrays[i, j]];
        currentColorArrays[i, j] = currentColorArrays[k, l];
        currentColorArrays[k, l] = iImageClickedColor;
        //TODO:animation?
    }
    //
    public void SpawnCandy()
    {        
        if (iDestoryedCandyCount <= 0) return;
        //sort
        int[] iTemp = new int[iDestoryedCandyCount];
        for (int i = 0; i < iDestoryedCandyCount; i++)
        {
            iTemp[i] = iDestoryedCandy[i];
        }
        Array.Sort(iTemp);
        //debug
        string s = "";
        for (int i = 0; i < iDestoryedCandyCount; i++)
            s = s + iTemp[i] + ",";
        Debug.Log("Destory Candy conuts =" + s);
        //destory candy
        for (int i = 0; i < iDestoryedCandyCount; i++)
        {
            int x = 0, y = 0;
            convertToDimensional(iTemp[i], ref x, ref y);
            currentColorArrays[x, y]=-1;
            //TODO:animation?
        }
        //move down by column
        for (int iColumn = 0; iColumn  < CANDY_POOL_WIDTH; iColumn++)
        {
            for (int j = 0; j < CANDY_POOL_HEIGH; j++)
            {
                int iSource = 0+j;
                if (iSource >= CANDY_POOL_HEIGH) break;
                if (currentColorArrays[iColumn, iSource] != -1) continue;
                //
                for (int k = 1; k < CANDY_POOL_HEIGH; k++)
                {
                    int iTarget = iSource + k;
                    if (iTarget >= CANDY_POOL_HEIGH) break;
                    if (currentColorArrays[iColumn, iTarget] == -1) continue;
                    MoveDown(iColumn, iSource, iTarget);
                    break;
                }
            }            
        }
        //spawn new candy
        for (int j = 0; j < CANDY_POOL_HEIGH; j++)
        {
            for (int i = 0; i < CANDY_POOL_WIDTH; i++)
            {
                if (currentColorArrays[i, j] == -1)
                {
                    int iColor = UnityEngine.Random.Range(0, MAX_CANDY_COLOR);                                                                              
                    currentColorArrays[i, j] = iColor;
                    int iSource = convertToIndex(i, j);
                    currentObjects[iSource].GetComponent<Image>().sprite = spritesCandy[iColor];
                }
            }
        }
        //reset
        iDestoryedCandyCount = 0;
        for (int i = 0; i < MAX_CANDY; i++) iDestoryedCandy[i] = -1;
    }
    public void MoveDown(int iColumn,int iSource, int iTarge)
    {
        int iFrom = convertToIndex(iColumn, iSource);
        int iTo = convertToIndex(iColumn, iTarge);
        Swap(iFrom, iTo);
    }
    //
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    //for debug
    public void debugMsg()
    {
        for (int j = 0; j < CANDY_POOL_HEIGH; j++)
        {
            string s = "";
            for (int i = 0; i < CANDY_POOL_WIDTH; i++)
            {
                s = s + currentColorArrays[i, j] + ",";
            }
            Debug.Log("candy color=" + s);
        }
    }
    public void debugMoveDown()
    {
        iDestoryedCandyCount = 2;
        iDestoryedCandy[0] = 0;
        iDestoryedCandy[1] = 1;
        SpawnCandy();
    }
    //tool
    public int convertToIndex(int i,int j)
    {
        return (j * CANDY_POOL_WIDTH + i);
    }
    public void convertToDimensional(int iIndex,ref int i,ref int j)
    {
        i = iIndex % CANDY_POOL_WIDTH;
        j = iIndex / CANDY_POOL_WIDTH;
    }
}
