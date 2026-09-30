using UnityEngine;

public class SaveTest : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            //开始存档
            SaveManager.Instance.Save(0);
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            //开始读档
            SaveManager.Instance.Load(0);
        }
    }
}