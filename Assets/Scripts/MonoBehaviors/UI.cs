using UnityEngine;

public class UI : MonoBehaviour
{

    private PopUpPanel popUpPanel;

    // Start is called before the first frame update
    void Awake()
    {
        popUpPanel = GetComponentInChildren<PopUpPanel>();
    }

    public void ShowGreeting(Player p)
    {
        popUpPanel.GreetingWindow(p);
    }
}
