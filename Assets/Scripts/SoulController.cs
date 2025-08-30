using System.Collections.Generic;
using UnityEngine;

public class SoulController : MonoBehaviour
{
    private static SoulController _instance;
    public static SoulController Instance
    {
        get
        {
            if (_instance == null) _instance = FindFirstObjectByType<SoulController>();
            return _instance;
        }
        private set => _instance = value;
    }

    private void Awake()
    {
        Instance = this;
    }

    public List<SoulItem> Souls;
}
