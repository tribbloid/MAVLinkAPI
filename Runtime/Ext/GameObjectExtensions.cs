using System.Text;
using UnityEngine;

namespace MAVLinkAPI.Ext
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Generates a unique ID for a GameObject based on its path in the scene hierarchy.
        /// This path is unique within the scene.
        /// </summary>
        /// <param name="gameObject">The GameObject to generate the ID for.</param>
        /// <returns>A string representing the unique path of the GameObject.</returns>
        public static string GetScenePath(this GameObject gameObject)
        {
            if (gameObject == null)
            {
                return "";
            }

            StringBuilder path = new StringBuilder("/" + gameObject.name);
            Transform parent = gameObject.transform.parent;

            while (parent != null)
            {
                path.Insert(0, "/" + parent.name);
                parent = parent.parent;
            }

            return path.ToString();
        }
    }
}