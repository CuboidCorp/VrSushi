using UnityEngine;

namespace XRMultiplayer
{
    public class GameObjectToggle : MonoBehaviour
    {
        [SerializeField] GameObject[] objectsToToggle;

        public void AddGameObject(GameObject obj)
        {
            if (obj != null)
            {
                var list = new System.Collections.Generic.List<GameObject>(objectsToToggle)
                {
                    obj
                };
                objectsToToggle = list.ToArray();
            }
        }

        public void ToggleObjects()
        {
            foreach (var obj in objectsToToggle)
            {
                obj.SetActive(!obj.activeSelf);
            }
        }
    }
}
