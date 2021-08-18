using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CalendarDateItem : MonoBehaviour {
    Image buttonColor;
    Button buttonState;


    public void OnDateItemClick()
    {
        CalendarController._calendarInstance.OnDateItemClick(gameObject.GetComponentInChildren<Text>().text);
        buttonColor = gameObject.GetComponent<Image>();
        buttonState = gameObject.GetComponent<Button>();
        if (buttonState.IsActive() == true && CalendarController.checkClickDate == false)
            buttonColor.color = new Color(0 / 255f, 240 / 255f, 255 / 255f, 255 / 255f);
        else if (buttonState.IsActive() == true && CalendarController.checkClickDate == true)
            buttonColor.color = new Color(255 / 255f, 230 / 255f, 0, 255 / 255f);


    }
}
