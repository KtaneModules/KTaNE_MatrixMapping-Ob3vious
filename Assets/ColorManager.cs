using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class ColorManager {
    public enum Style
    {
        Binary,
        StatusLight
    }


    private List<Color> _primaryColors = new List<Color>();
    private List<Color> _secondaryColors = new List<Color>();

    public ColorManager(Style style)
    {
        switch (style)
        {
            case Style.Binary:
                _primaryColors.Add(new Color(0.0625f, 0.0625f, 0.0625f));
                _secondaryColors.Add(new Color(0.9375f, 0.9375f, 0.9375f));
                break;
            case Style.StatusLight:
                _primaryColors.Add(new Color(0.0625f, 0.0625f, 0.0625f));
                _secondaryColors.Add(new Color(1f, 0f, 0f));
                _secondaryColors.Add(new Color(1f, 0.75f, 0f));
                _secondaryColors.Add(new Color(0f, 1f, 0f));
                break;
        }
    }

    public ColorManager(int s1, int s2)
    {
        SetColors(_primaryColors, s1);
        SetColors(_secondaryColors, s2);
    }

    public void Empty()
    {
        _primaryColors = new List<Color>();
        _secondaryColors = new List<Color>();
        _primaryColors.Add(new Color(0.0625f, 0.0625f, 0.0625f));
        _secondaryColors.Add(new Color(0.0625f, 0.0625f, 0.0625f));
    }

    private void SetColors(List<Color> colors, int s)
    {
        float hueOffset = Rnd.Range(0f, 1f);

        float minDistance = 1f / (2f * s - 1f);
        List<float> bonusDistances = new List<float>();
        for (int i = 0; i < s - 1; i++)
            bonusDistances.Add(Rnd.Range(0f, 1f - minDistance * s));
        bonusDistances.Sort();

        List<float> finalDistances = new List<float>();
        finalDistances.Add(hueOffset);
        for (int i = 0; i < bonusDistances.Count; i++)
            finalDistances.Add(((i + 1) * minDistance + bonusDistances[i] + hueOffset) % 1);

        foreach (float hue in finalDistances)
        {
            Color color = Color.HSVToRGB(hue, 1, 1);
            colors.Add(color);
        }
    }

    public Color Get(int state, int value)
    {
        List<Color> colorSet = state == 0 ? _primaryColors : _secondaryColors;
        return colorSet[value];
    }

    public override string ToString()
    {
        return "{" + _primaryColors.Join(", ") + "}, {" + _secondaryColors.Join(", ") + "}";
    }
}
