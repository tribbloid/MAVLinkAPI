using UnityEngine;

namespace MAVLinkAPI.Util.NullSafety
{
    [Required]
    public class NullExample1 : MonoBehaviour
    {
        [SerializeField] private GameObject field1;

        public GameObject field2;
    }
}