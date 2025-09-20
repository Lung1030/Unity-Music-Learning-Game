using UnityEngine;
using UnityEngine.UI;

public class PianGameLog : MonoBehaviour
{
    public static int clearcount = 0;
    public Text clearcountText;

    [Header("簡單關卡一小星星")]
    public static bool gamesmallstart_isclear = false;
    public Text gamesmallstart;

    [Header("簡單關卡二小蜜蜂")]
    public static bool gamesmallbee_isclear = false;
    public Text gamesmallbee;

    [Header("簡單關卡三兩隻老虎")]
    public static bool gametwotiger_isclear = false;
    public Text gametwotiger;
    [Header("簡單關卡四火車快飛")]
    public static bool gametrain_isclear = false;
    public Text gametrain;
    [Header("簡單關卡五倫敦鐵橋")]
    public static bool gamebridge_isclear = false;
    public Text gamebridge;
    void Start()
    {
        // 載入紀錄
        gamesmallstart_isclear = PlayerPrefs.GetInt("gamesmallstart_isclear", 0) == 1;
        gamesmallbee_isclear = PlayerPrefs.GetInt("gamesmallbee_isclear", 0) == 1;
        gametwotiger_isclear = PlayerPrefs.GetInt("gametwotiger_isclear", 0) == 1;
        gametrain_isclear = PlayerPrefs.GetInt("gametrain_isclear", 0) == 1;
        gamebridge_isclear = PlayerPrefs.GetInt("gamebridge_isclear", 0) == 1;
        clearcount = PlayerPrefs.GetInt("clearcount");
    }

    void Update()
    {
        // 顯示通關狀態
        if (gamesmallstart_isclear) gamesmallstart.color = Color.green;
        if (gamesmallbee_isclear) gamesmallbee.color = Color.green;
        if (gametwotiger_isclear) gametwotiger.color = Color.green;
        if (gametrain_isclear) gametrain.color = Color.green;
        if (gamebridge_isclear) gamebridge.color = Color.green;
        clearcountText.text = $"{clearcount}/5";
    }

    public static void SaveClearStatus()
    {
        PlayerPrefs.SetInt("gamesmallstart_isclear", gamesmallstart_isclear ? 1 : 0);
        PlayerPrefs.SetInt("gamesmallbee_isclear", gamesmallbee_isclear ? 1 : 0);
        PlayerPrefs.SetInt("gametwotiger_isclear", gametwotiger_isclear ? 1 : 0);
        PlayerPrefs.SetInt("gametrain_isclear", gametrain_isclear ? 1 : 0);
        PlayerPrefs.SetInt("gamebridge_isclear", gamebridge_isclear ? 1 : 0);
        PlayerPrefs.SetInt("clearcount", clearcount);
        PlayerPrefs.Save();
    }
}
