using UnityEngine;

namespace MAVLinkAPI.Util.Maybe
{
    public class NullExample2 : MonoBehaviour
    {
        [SerializeField] [Required] private GameObject field1;

        [Required] public GameObject field2;
    }
}