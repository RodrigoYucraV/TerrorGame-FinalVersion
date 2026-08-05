using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PlayerEventSystem : MonoBehaviour
{
    public static PlayerEventSystem Instance { get; private set; }

    public event Action<IPlayerPositionProvider> OnPlayerReady;
    public event Action OnPlayerUnready;

    private IPlayerPositionProvider cachedProvider;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
         
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void NotifyPlayerReady(IPlayerPositionProvider playerPositionProvider)
    {
        cachedProvider = playerPositionProvider;
        Debug.Log($"PlayerEventSystem: Jugador listo - {playerPositionProvider.GetType().Name}");
        OnPlayerReady?.Invoke(playerPositionProvider);
    }

    public void NotifyPlayerUnready()
    {
        cachedProvider = null;
        Debug.Log("PlayerEventSystem: Jugador no disponible");
        OnPlayerUnready?.Invoke();
    }

    public IPlayerPositionProvider GetCachedProvider()
    {
        return cachedProvider;
    }
}
