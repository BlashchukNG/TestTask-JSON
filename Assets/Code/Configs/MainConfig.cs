using UnityEngine;

namespace Code.Configs
{
    [CreateAssetMenu(fileName = "config main", menuName = "GAME/Main Config", order = 0)]
    public class MainConfig : 
        ScriptableObject
    {
        public string url;
    }
}