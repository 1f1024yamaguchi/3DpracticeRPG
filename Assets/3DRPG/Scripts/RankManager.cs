using UnityEngine;

public static class RankManager 
{
    public static string GetRank(float time)
    {
        if (time < 120f)
        {
            return "SS";
        }
        else if (time < 150f)
        {
            return "S";
        }
        else if (time < 300f)
        {
            return "A+";
        }
        else if (time < 600f)
        {
            return "A";
        }
        else if (time < 900f)
        {
            return "B";
        }
        {
            return "C";
        }
    }

    public static Color GetRankColor(string rank)
    {
        switch (rank)
        {
            case "SS": return Color.yellow;
            case "S":  return new Color(1f, 0.5f, 0f);
            case "A+": return Color.green;
            case "A":  return new Color(0.3f, 0.8f, 1f);
            case "B":  return Color.cyan;
            default:   return Color.white;
        }
    }
    
}
