using RapidPro.MVC;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows the hearts left as icons: full ones in colour, lost ones faded.
/// </summary>
public class HeartsView : View
{
    [Tooltip("One icon per heart, left to right.")]
    [SerializeField] private Image[] _icons;

    [SerializeField] private Color _fullColor = new Color(0.93f, 0.23f, 0.33f);
    [SerializeField] private Color _lostColor = new Color(1f, 1f, 1f, 0.25f);

    public void ShowHearts(int p_current, int p_max)
    {
        for (int i = 0; i < _icons.Length; i++)
        {
            _icons[i].gameObject.SetActive(i < p_max);
            _icons[i].color = i < p_current ? _fullColor : _lostColor;
        }
    }
}
