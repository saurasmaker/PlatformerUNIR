using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance {  get { return _instance; } }

    private PlayerInput _playerInput;
    public PlayerInput PlayerInput { get { return _playerInput; } }

    // Start is called before the first frame update
    void Awake()
    {
        SetSingleton();
        GetRequiredComponents();
    }

    // Update is called once per frame
    void Update()
    {
        
    }






    private void SetSingleton()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }

    private void GetRequiredComponents()
    {
        if (_playerInput == null)
            _playerInput = GetComponent<PlayerInput>();
    }
}
