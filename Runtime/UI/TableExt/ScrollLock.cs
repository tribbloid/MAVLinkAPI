using System.Collections;
using TNRD.Autohook;
using MAVLinkAPI.UI.Tables;
using MAVLinkAPI.Util.NullSafety;
using UnityEngine;
using UnityEngine.UI;

namespace MAVLinkAPI.UI.TableExt
{
    public class ScrollLock : MonoBehaviour
    {
        [Required] public TableLayout table = null!;

        [AutoHook(SearchArea = AutoHookSearchArea.Parent)] [Required]
        public ScrollRect scroll;


        public IEnumerable RefreshTable()
        {
            var p = scroll.verticalNormalizedPosition;
            table.UpdateLayout();

            // LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.GetComponent<RectTransform>());
            yield return new WaitForEndOfFrame(); // TODO: still cause flashing

            scroll.verticalNormalizedPosition = p;
        }

        // public void Lock()
        // {
        //     scrollPosition = scroll.verticalNormalizedPosition;
        // }
        //
        // public void Unlock()
        // {
        //     scroll.verticalNormalizedPosition = scrollPosition;
        // }
    }
}