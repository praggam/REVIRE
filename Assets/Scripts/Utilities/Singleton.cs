using UnityEngine;

// all singleton classes should inherit from this class
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T _instance;
    public static T Instance
    {
        get { 
            return _instance; 
        }
    }

    // this method must be called in Awake() of inheriting singleton class 
    protected void InitializeSingleton(T obj)
    {

        if (_instance == null)
        {
            _instance = obj;
        }
        else if (_instance != obj)
        {
            // make sure that there is only one instance at a time  by destroying this one if one already exists
            Destroy(obj.gameObject);
            return;
        }
    }
}
