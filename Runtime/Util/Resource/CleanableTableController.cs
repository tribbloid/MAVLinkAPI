#nullable enable
using System.Collections.Generic;
using UI.Tables;
using Autofill;
using MAVLinkAPI.Util.NullSafety;
using UnityEngine;

namespace MAVLinkAPI.Util.Resource
{
    public class CleanableTableController : LifetimeBinding
    {
        [Autofill] public TableLayout table;
        [Required] public CleanableRowBinding template;
        public TableRow templateRow => template.row;

        // Helper class to manage lifetime and row creation
        private class LifetimeWithSync : Lifetime
        {
            private readonly CleanableTableController _controller;

            public LifetimeWithSync(CleanableTableController controller) // Calls Lifetime(IntPtr.Zero, true)
            {
                _controller = controller;
            }

            public override void Register(Cleanable cleanable)
            {
                base.Register(cleanable);
                // Instantiate the new row using the controller's context
                var row = Instantiate(_controller.templateRow);
                // row._controller.table.AddRow(row);
            }
        }

        public override Lifetime? Lifetime => _lifetime.Lazy(() => new LifetimeWithSync(this));
    }
}