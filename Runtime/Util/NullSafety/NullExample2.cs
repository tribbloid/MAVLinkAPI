using UnityEngine;

namespace MAVLinkAPI.Util.NullSafety
{
    public class NullExample2 : MonoBehaviour
    {
        [SerializeField] [Required] private GameObject field1;

        [Required] public GameObject field2;
    }
}