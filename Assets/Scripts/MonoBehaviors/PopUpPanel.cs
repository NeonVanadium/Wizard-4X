using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopUpPanel : MonoBehaviour
{

    private Text title;
    private Text body;

    // Start is called before the first frame update
    private void Start()
    {
        Show(false);
        Text[] labels = GetComponentsInChildren<Text>();
        title = labels[0];
        body = labels[1];
    }

    public void Show(bool val)
    {
        this.gameObject.SetActive(val);
    }

    public void GreetingWindow(Player p)
    {
        Show(true);
        title.text = "You encounter a stranger.";
        body.text = $"Blithering imbecile. I am {p.name}. Hit the close button or stare at this sentence for all eternity.";
    }


}
