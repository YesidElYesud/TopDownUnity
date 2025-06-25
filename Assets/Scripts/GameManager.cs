using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    private HashSet<string> keys = new HashSet<string>();

    public int monedas = 0;
    public TextMeshProUGUI monedasText;
    
    // Start is called before the first frame update
    void Start()
    {
        UpdateMonedasUI();
    }
    void Awake()
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

    // Update is called once per frame
    public void AddMoneda(int cantidad)
    {
        monedas += cantidad;
        UpdateMonedasUI();
    }

    public void addkey(string keyID)
    {
        keys.Add(keyID);
        Debug.Log("Llave Obtenida" + keyID);
    }
    void UpdateMonedasUI()
    {
        Debug.Log("Monedas:" + monedas);
        if (monedasText != null)
            monedasText.text = monedas.ToString();
    }

    public bool haskey(string keyID)
    {
        return keys.Contains(keyID);
    }
}
