#nullable enable
using Autofill;
using MAVLinkAPI.UI.Tables;
using MAVLinkAPI.Util.NullSafety;

namespace MAVLinkAPI.Util.Resource.UI
{
    public class CleanableTableController : LifetimeBinding
    {
        [Autofill] public TableLayout table = null!;
        [Required] public CleanableRowBinding template = null!;
        public TableRow TemplateRow => template.row;

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
                var binding = Instantiate(_controller.template);
                binding.gameObject.SetActive(true);
                binding.Bind(cleanable);

                _controller.table.AddRow(binding.row);
            }
        }

        public override Lifetime Lifetime => lifetimeExisting.Lazy(() => new LifetimeWithSync(this))!;

        public void AddDummy()
        {
            var dummy = new Cleanable.Dummy(Lifetime);
        }
    }
}